using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Android.Content;
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
    private const string KeyStoreName = "AndroidKeyStore";
    private const string Transformation = "AES/GCM/NoPadding";
    private const int GcmTagSizeBits = 128;
    private const byte BlobVersion = 1;

    private readonly string _filePath;

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
        _filePath = Path.Combine(baseDir, "dms.sessions.v1.bin");
    }

    public async ValueTask<string?> ReadAsync(string key)
    {
        if (!File.Exists(_filePath))
            return null;

        try
        {
            var blob = await File.ReadAllBytesAsync(_filePath).ConfigureAwait(false);
            var clearBytes = Decrypt(blob);
            return System.Text.Encoding.UTF8.GetString(clearBytes);
        }
        catch
        {
            return null;
        }
    }

    public async ValueTask WriteAsync(string key, string value)
    {
        var clearBytes = System.Text.Encoding.UTF8.GetBytes(value);
        var blob = Encrypt(clearBytes);
        await File.WriteAllBytesAsync(_filePath, blob).ConfigureAwait(false);
    }

    public ValueTask DeleteAsync(string key)
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);

        return ValueTask.CompletedTask;
    }

    private static byte[] Encrypt(byte[] clearBytes)
    {
        var secretKey = GetOrCreateSecretKey();
        using var cipher = Cipher.GetInstance(Transformation);
        cipher.Init(CipherMode.EncryptMode, secretKey);

        var iv = cipher.GetIV() ?? throw new CryptographicException("Cipher did not provide an IV.");
        var cipherBytes = cipher.DoFinal(clearBytes);

        var result = new byte[1 + 4 + iv.Length + cipherBytes.Length];
        result[0] = BlobVersion;
        BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(1, 4), iv.Length);
        iv.CopyTo(result.AsSpan(5));
        cipherBytes.CopyTo(result.AsSpan(5 + iv.Length));
        return result;
    }

    private static byte[] Decrypt(byte[] blob)
    {
        if (blob.Length < 5)
            throw new CryptographicException("Invalid blob.");

        var version = blob[0];
        if (version != BlobVersion)
            throw new CryptographicException($"Unsupported blob version: {version}.");

        var ivLength = BinaryPrimitives.ReadInt32LittleEndian(blob.AsSpan(1, 4));
        if (ivLength <= 0 || blob.Length < 5 + ivLength)
            throw new CryptographicException("Invalid blob.");

        var iv = blob.AsSpan(5, ivLength).ToArray();
        var cipherBytes = blob.AsSpan(5 + ivLength).ToArray();

        var secretKey = GetOrCreateSecretKey();
        using var cipher = Cipher.GetInstance(Transformation);
        cipher.Init(CipherMode.DecryptMode, secretKey, new GCMParameterSpec(GcmTagSizeBits, iv));
        return cipher.DoFinal(cipherBytes);
    }

    private static ISecretKey GetOrCreateSecretKey()
    {
        var keyStore = KeyStore.GetInstance(KeyStoreName);
        keyStore.Load(null);

        try
        {
            var existing = keyStore.GetKey(KeyStoreAlias, null) as ISecretKey;
            if (existing is not null)
                return existing;
        }
        catch
        {
            try { keyStore.DeleteEntry(KeyStoreAlias); } catch { }
        }

        using var keyGenerator = KeyGenerator.GetInstance(KeyProperties.KeyAlgorithmAes, KeyStoreName);
        var spec = new KeyGenParameterSpec.Builder(
                KeyStoreAlias,
                KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
            .SetBlockModes(KeyProperties.BlockModeGcm)
            .SetEncryptionPaddings(KeyProperties.EncryptionPaddingNone)
            .SetKeySize(256)
            .Build();

        keyGenerator.Init(spec);
        return (ISecretKey)keyGenerator.GenerateKey();
    }
}
