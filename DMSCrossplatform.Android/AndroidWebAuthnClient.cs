using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Credentials;
using Android.OS;
using AndroidX.Credentials;
using DMSCrossplatform.Infrastructure;
using Java.Util.Concurrent;
using CredentialManager = AndroidX.Credentials.CredentialManager;
using CredentialOption = AndroidX.Credentials.CredentialOption;
using GetCredentialRequest = AndroidX.Credentials.GetCredentialRequest;

namespace DMSCrossplatform.Android;



public sealed class AndroidWebAuthnClient : Java.Lang.Object, IWebAuthnClient
{
    private readonly Activity _activity;
    private readonly ICredentialManager _credentialManager;
    private readonly IExecutor _executor = Executors.NewSingleThreadExecutor();

    public AndroidWebAuthnClient(Activity activity)
    {
        _activity = activity ?? throw new ArgumentNullException(nameof(activity));
        _credentialManager = CredentialManager.Companion.Create(activity);
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

        _credentialManager.CreateCredentialAsync(
            _activity,
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

        _credentialManager.GetCredentialAsync(
            _activity,
            request,
            cancellationSignal,
            _executor,
            new GetCallback(
                onResult: result =>
                {
                    var credential = result;
                    if (credential is PublicKeyCredential pk)
                    {
                        tcs.TrySetResult(pk.AuthenticationResponseJson);
                        return;
                    }

                    tcs.TrySetException(new InvalidOperationException(
                        $"Unexpected credential type: {credential?.Class?.Name ?? "<null>"}"));
                },
                onError: error =>
                {
                    tcs.TrySetException(new InvalidOperationException(
                        error?.ToString() ?? "GetCredentialAsync failed"));
                }));

        return tcs.Task;
    }

    private sealed class CreateCallback : Java.Lang.Object, ICredentialManagerCallback
    {
        private readonly Action<Java.Lang.Object?> _onResult;
        private readonly Action<Java.Lang.Object?> _onError;

        public CreateCallback(Action<Java.Lang.Object?> onResult, Action<Java.Lang.Object?> onError)
        {
            _onResult = onResult;
            _onError = onError;
        }

        public void OnResult(Java.Lang.Object result) => _onResult(result);
        public void OnError(Java.Lang.Object e) => _onError(e);
    }

    private sealed class GetCallback : Java.Lang.Object, ICredentialManagerCallback
    {
        private readonly Action<Java.Lang.Object?> _onResult;
        private readonly Action<Java.Lang.Object?> _onError;

        public GetCallback(Action<Java.Lang.Object?> onResult, Action<Java.Lang.Object?> onError)
        {
            _onResult = onResult;
            _onError = onError;
        }

        public void OnResult(Java.Lang.Object result) => _onResult(result);
        public void OnError(Java.Lang.Object e) => _onError(e);
    }
}


