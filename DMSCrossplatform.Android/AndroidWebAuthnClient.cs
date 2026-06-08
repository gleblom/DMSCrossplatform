using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Credentials;
using Android.OS;
using AndroidX.Credentials;
using DMSCrossplatform.Infrastructure;
using Java.Util.Concurrent;
using CredentialManager = AndroidX.Credentials.CredentialManager;
using CredentialOption = AndroidX.Credentials.CredentialOption;
using GetCredentialRequest = AndroidX.Credentials.GetCredentialRequest;
using GetCredentialResponse = AndroidX.Credentials.GetCredentialResponse;
using Object = Java.Lang.Object;

namespace DMSCrossplatform.Android;



public sealed class AndroidWebAuthnClient : IWebAuthnClient
{
    private Context Activity => global::Android.App.Application.Context;
    private ICredentialManager? _credentialManager;
    private readonly IExecutor _executor = Executors.NewSingleThreadExecutor();
    private readonly object _lock = new object();

    public AndroidWebAuthnClient()
    {
        // Ленивая инициализация CredentialManager для ускорения старта приложения
    }

    private ICredentialManager GetCredentialManager()
    {
        if (_credentialManager == null)
        {
            lock (_lock)
            {
                if (_credentialManager == null)
                {
                    _credentialManager = CredentialManager.Companion.Create(Activity);
                }
            }
        }
        return _credentialManager;
    }

    public Task<string> RegisterAsync(string optionsJson, CancellationToken ct = default)
    {
        var request = new CreatePublicKeyCredentialRequest(optionsJson);
        return ExecuteCreateAsync(request, ct);
    }

    public Task<string> AuthenticateAsync(string optionsJson, CancellationToken ct = default)
    {
        var option = new GetPublicKeyCredentialOption(optionsJson);
        var request = new GetCredentialRequest(new List<CredentialOption> { option });
        return ExecuteGetAsync(request, ct);
    }

    private Task<string> ExecuteCreateAsync(CreatePublicKeyCredentialRequest request, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancellationSignal = new CancellationSignal();

        using var reg = ct.Register(() =>
        {
            try { cancellationSignal.Cancel(); } catch { /* ignore */ }
            tcs.TrySetCanceled(ct);
        });

        GetCredentialManager().CreateCredentialAsync(
            Activity,
            request,
            cancellationSignal,
            _executor,
            new CreateCallback(
                onResult: result =>
                {
                    if (result is CreatePublicKeyCredentialResponse pk)
                    {
                        tcs.TrySetResult(pk.RegistrationResponseJson);
                        return;
                    }

                    tcs.TrySetException(new InvalidOperationException(
                        $"Unexpected create result type: {result?.Class?.Name ?? "<null>"}"));
                },
                onError: error =>
                {
                    tcs.TrySetException(new InvalidOperationException(
                        error?.ToString() ?? "CreateCredentialAsync failed"));
                }));

        return tcs.Task;
    }

    private Task<string> ExecuteGetAsync(GetCredentialRequest request, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancellationSignal = new CancellationSignal();

        using var reg = ct.Register(() =>
        {
            try { cancellationSignal.Cancel(); } catch { /* ignore */ }
            tcs.TrySetCanceled(ct);
        });
        
        GetCredentialManager().GetCredentialAsync(
            Activity,
            request,
            cancellationSignal,
            _executor,
            new GetCallback(
                onResult: result =>
                {
                    var cred = (GetCredentialResponse)result;
                    if (cred.Credential is PublicKeyCredential pk)
                    {
                        tcs.TrySetResult(pk.AuthenticationResponseJson);
                        return;
                    }

                    tcs.TrySetException(new InvalidOperationException(
                        $"Unexpected credential type: {result?.Class?.Name ?? "<null>"}"));
                },
                onError: error =>
                {
                    tcs.TrySetException(new InvalidOperationException(
                        error?.ToString() ?? "GetCredentialAsync failed"));
                }));

        return tcs.Task;
    }

    private sealed class CreateCallback : Object, ICredentialManagerCallback
    {
        private readonly Action<Object?> _onResult;
        private readonly Action<Object?> _onError;

        public CreateCallback(Action<Object?> onResult, Action<Object?> onError)
        {
            _onResult = onResult;
            _onError = onError;
        }

        public void OnResult(Object? result) => _onResult(result);
        public void OnError(Object e) => _onError(e);
    }

    private sealed class GetCallback : Object, ICredentialManagerCallback
    {
        private readonly Action<Object?> _onResult;
        private readonly Action<Object?> _onError;

        public GetCallback(Action<Object?> onResult, Action<Object?> onError)
        {
            _onResult = onResult;
            _onError = onError;
        }

        public void OnResult(Object result) => _onResult(result);
        public void OnError(Object e) => _onError(e);
    }
}


