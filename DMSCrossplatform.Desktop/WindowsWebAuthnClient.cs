using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure;
using DMSCrossplatform.Models.Dto;
using DSInternals.Win32.WebAuthn;
using DSInternals.Win32.WebAuthn.COSE;

namespace DMSCrossplatform.Desktop;

public class WindowsWebAuthnClient: IWebAuthnClient
{
    private readonly WebAuthnApi _api = new ();
    
    
    public async Task<string> RegisterAsync(string optionsJson, CancellationToken ct = default)
    {
        var options = JsonSerializer.Deserialize<PublicKeyCredentialCreationOptionsDto>(optionsJson);

        var challenge = Base64UrlConverter.FromBase64UrlString(options.Challenge);
        
        var timeout = Convert.ToUInt32(options.Timeout);

        var publicCredParams = 
            options
                .PubKeyCredParams
                .Select(param => new PublicKeyCredentialParameter((Algorithm)param.Alg, param.Type))
                .ToList();
        var exclude =
            options
                .ExcludeCredentials
                .Select(cred =>
                    new PublicKeyCredentialDescriptor(
                        Base64UrlConverter.FromBase64UrlString(cred.Id), type: cred.Type))
            .ToList();
        
        
        var credential = _api.AuthenticatorMakeCredential(
            new PublicKeyCredentialCreationOptions
            {   
                RelyingParty  = new RelyingPartyInformation()
                {
                    Id = options.Rp.Id,
                    Name = options.Rp.Name
                },
                User = new UserInformation()
                {
                    Id = Base64UrlConverter.FromBase64UrlString(options.User.Id),
                    Name = options.User.Name,
                    DisplayName = options.User.Name,
                },
                Challenge = challenge,
                TimeoutMilliseconds =  timeout,
                Attestation = AttestationConveyancePreference.None,
                AuthenticatorSelection = new AuthenticatorSelectionCriteria()
                {
                    AuthenticatorAttachment = AuthenticatorAttachment.CrossPlatform,
                    UserVerificationRequirement = UserVerificationRequirement.Required,
                    ResidentKey = ResidentKeyRequirement.Required,
                    RequireResidentKey =  true
                },
                PublicKeyCredentialParameters = publicCredParams,
                ExcludeCredentials = exclude
                
            }
        );
        

        return JsonSerializer.Serialize(credential);
    }

    public async Task<string> AuthenticateAsync(string optionsJson, CancellationToken ct = default)
    {
        var options = JsonSerializer.Deserialize<PublicKeyCredentialRequestOptionsDto>(optionsJson);

        var challenge = Base64UrlConverter.FromBase64UrlString(options.Challenge);
   
        var assertion = await _api.AuthenticatorGetAssertionAsync(
            options.RpId, 
            challenge, 
            UserVerificationRequirement.Required, 
            AuthenticatorAttachment.CrossPlatform, 
            cancellationToken: ct);

        return JsonSerializer.Serialize(assertion);
    }
}