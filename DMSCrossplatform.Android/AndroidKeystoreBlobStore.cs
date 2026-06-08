using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using Android.Security.Keystore;
using DMSCrossplatform.Infrastructure.Storage;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;
using CipherMode = Javax.Crypto.CipherMode;

namespace DMSCrossplatform.Android;

[SupportedOSPlatform("android23.0")]
public sealed class AndroidKeystoreBlobStore : ISessionBlobStore
{
    private const string KeyStoreAlias = "dmscrossplatform.sessions.masterkey";
    private const string Transformation = "AES/GCM/NoPadding";
    private const int GcmTagSizeBits = 128;
    private const byte BlobVersion = 1;

    private readonly string _baseDir;

    public AndroidKeystoreBlobStore() : this(global::Android.App.Application.Context)
    {
    }

    public AndroidKeystoreBlobStore(Context context)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(23))
            throw new PlatformNotSupportedException("Android Keystore AES requires API 23+.");

        var baseDir = context.NoBackupFilesDir?.AbsolutePath
                      ?? context.FilesDir?.AbsolutePath
                      ?? throw new InvalidOperationException("Cannot resolve Android app storage directory.");

        Directory.CreateDirectory(baseDir);
        _baseDir = baseDir;
    }

    public async ValueTask<string?> ReadAsync(string key)
    {
        var filePath = GetPath(key);
        if (!File.Exists(filePath))
            return null;

        try
        {
            Console.WriteLine($"Session path {filePath}");
            var blob = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
            var clearBytes = Decrypt(blob);
            return System.Text.Encoding.UTF8.GetString(clearBytes);
        }
        catch (Exception ex) 
        {
            // Ловим System.Exception, так как .NET-обертки для Android могут пробрасывать
            // Java.Lang.Exception, Android.Security.KeyStoreException и другие классы.
            Console.WriteLine($"Stored session could not be decrypted and will be cleared: {ex}");
            
            TryDelete(filePath);

            // Если проблема вызвана инвалидацией ключа, его нужно удалить,
            // чтобы при следующей попытке записи сгенерировался новый.
            if (ex is KeyPermanentlyInvalidatedException || 
                ex.Message.Contains("KeyPermanentlyInvalidatedException") ||
                ex.Message.Contains("MAC verification failed"))
            {
                TryDeleteKeyEntry();
            }

            return null;
        }
    }

    public async ValueTask WriteAsync(string key, string value)
    {
        var clearBytes = System.Text.Encoding.UTF8.GetBytes(value);
        
        byte[] blob;
        try
        {
            blob = Encrypt(clearBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Write session exception {ex}");
            // Если ключ сломался на этапе шифрования, чистим KeyStore
            if (ex is KeyPermanentlyInvalidatedException || ex.Message.Contains("KeyPermanentlyInvalidatedException"))
            {
                TryDeleteKeyEntry();
            }
            throw;
        }

        var filePath = GetPath(key);
        var tempPath = $"{filePath}.{Guid.NewGuid():N}.tmp";

        try
        {
            await File.WriteAllBytesAsync(tempPath, blob).ConfigureAwait(false);
            File.Move(tempPath, filePath, true);
        }
        catch
        {
            TryDelete(tempPath);
            throw;
        }
    }

    public ValueTask DeleteAsync(string key)
    {
        TryDelete(GetPath(key));
        return ValueTask.CompletedTask;
    }

    private string GetPath(string key) => Path.Combine(_baseDir, $"{key}.bin");

    private static void TryDelete(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch { /* Игнорируем ошибки при удалении */ }
    }

    private static byte[] Encrypt(byte[] clearBytes)
    {
        var secretKey = GetSecretKey(createIfMissing: true) 
                    ?? throw new CryptographicException("Failed to get or create secret key.");
        using var cipher = Cipher.GetInstance(Transformation)
                           ?? throw new CryptographicException("AES-GCM cipher is unavailable.");
        
        cipher.Init(CipherMode.EncryptMode, secretKey);

        var iv = cipher.GetIV() ?? throw new CryptographicException("Cipher did not provide an IV.");
        var cipherBytes = cipher.DoFinal(clearBytes)
                          ?? throw new CryptographicException("Encryption produced no output.");
        Console.WriteLine($"Secret key: {secretKey}");
        var result = new byte[1 + 4 + iv.Length + cipherBytes.Length];
        result[0] = BlobVersion;
        Console.WriteLine($"Result: {result.Length}");
        Console.WriteLine($"IV: {iv.Length}");
        BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(1, 4), iv.Length);
        iv.CopyTo(result.AsSpan(5));
        cipherBytes.CopyTo(result.AsSpan(5 + iv.Length));
        return result;
    }

    private static byte[] Decrypt(byte[] blob)
    {
        Console.WriteLine("Starting decryption:");
        if (blob.Length < 5)
        {
            Console.WriteLine("Invalid blob length.");
            throw new CryptographicException("Invalid blob length.");
        }

        var version = blob[0];
        Console.WriteLine("Version processed:");
        if (version != BlobVersion)
        {
            Console.WriteLine($"Unsupported blob version: {version}.");
            throw new CryptographicException($"Unsupported blob version: {version}.");
        }

        var ivLength = BinaryPrimitives.ReadInt32LittleEndian(blob.AsSpan(1, 4));
        Console.WriteLine($"ivLength: {ivLength}");
        // Надежная проверка длины IV (для AES-GCM это обычно 12 байт)
        if (ivLength <= 0 || ivLength > blob.Length - 5)
        {
            Console.WriteLine("Invalid IV length in blob.");
            throw new CryptographicException("Invalid IV length in blob.");
        }


        var iv = blob.AsSpan(5, ivLength).ToArray();
        var cipherBytes = blob.AsSpan(5 + ivLength).ToArray();
        
        var secretKey = GetSecretKey(createIfMissing: false);
        if (secretKey == null)
        {
            Console.WriteLine($"SecretKey is null.");
            throw new CryptographicException("Master key is missing from AndroidKeyStore. Data is unrecoverable.");
        }
        
        using var cipher = Cipher.GetInstance(Transformation)
                           ?? throw new CryptographicException("AES-GCM cipher is unavailable.");

        // Именно здесь может вылететь KeyPermanentlyInvalidatedException
        cipher.Init(CipherMode.DecryptMode, secretKey, new GCMParameterSpec(GcmTagSizeBits, iv));
        
        // А здесь вылетает Android.Security.KeyStoreException (MAC verification failed)
        return cipher.DoFinal(cipherBytes)
               ?? throw new CryptographicException("Decryption produced no output.");
    }

    private static ISecretKey? GetSecretKey(bool createIfMissing)
    {
        var keyStore = KeyStore.GetInstance("AndroidKeyStore")
                       ?? throw new CryptographicException("AndroidKeyStore is unavailable.");

        keyStore.Load(null);

        try
        {
            // Сначала проверяем, есть ли вообще такой Alias
            if (keyStore.ContainsAlias(KeyStoreAlias))
            {
                var key = keyStore.GetKey(KeyStoreAlias, null);
                if (key != null)
                {
                    // ВАЖНО: Используем JavaCast вместо 'as ISecretKey'
                    return key.JavaCast<ISecretKey>();
                }
            }
        }
        catch (Exception ex) when (ex is UnrecoverableKeyException || ex is KeyPermanentlyInvalidatedException || ex is Java.Lang.Exception)
        {
            TryDeleteKeyEntry();
        }

        // Если мы только читаем, и ключа нет — возвращаем null, не генерируя новый
        if (!createIfMissing)
            return null;

        using var keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, "AndroidKeyStore")
                                 ?? throw new CryptographicException("AndroidKeyStore AES key generator is unavailable.");

        var spec = new KeyGenParameterSpec.Builder(
                KeyStoreAlias,
                KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
            .SetBlockModes(KeyProperties.BlockModeGcm)
            .SetEncryptionPaddings(KeyProperties.EncryptionPaddingNone)
            .SetKeySize(256)
            .Build();

        keyGenerator.Init(spec);

        var generatedKey = keyGenerator.GenerateKey();
        return generatedKey?.JavaCast<ISecretKey>() 
               ?? throw new CryptographicException("AndroidKeyStore did not generate an AES secret key.");
    }

    // Вынесенный метод для удаления ключа, чтобы его можно было вызывать из ReadAsync/WriteAsync
    private static void TryDeleteKeyEntry()
    {
        try 
        { 
            var keyStore = KeyStore.GetInstance("AndroidKeyStore");
            keyStore?.Load(null);
            keyStore?.DeleteEntry(KeyStoreAlias); 
        } 
        catch { /* Игнорируем ошибки при удалении ключа */ }
    }
}
