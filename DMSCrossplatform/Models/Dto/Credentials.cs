using System.Collections.Generic;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DMSCrossplatform.Models.Dto;


public class WebAuthnOptionsResponseDto

{
    [JsonProperty("challenge_id")]
    public string ChallengeId { get; set; }


    [JsonProperty("options")]
    public PublicKeyCredentialCreationOptionsDto Options { get; set; } 
}
public class WebAuthnLoginOptionsResponseDto

{
    [JsonProperty("challenge_id")]
    public string ChallengeId { get; set; }


    [JsonProperty("options")]
    public PublicKeyCredentialRequestOptionsDto Options { get; set; } 
}


public class WebAuthnFinishRequestDto
{
    [JsonProperty("challenge_id")]
    public string ChallengeId { get; set; }


    [JsonProperty("credential")]
    public JToken Credential { get; set; } 
}


public class PublicKeyCredentialCreationOptionsDto
{
    [JsonProperty("rp")]
    public RpDto Rp { get; set; }

    [JsonProperty("user")]
    public UserDto User { get; set; }

    [JsonProperty("challenge")]
    public string Challenge { get; set; }

    [JsonProperty("pubKeyCredParams")]
    public List<PubKeyCredParamDto> PubKeyCredParams { get; set; }

    [JsonProperty("timeout")]
    public int? Timeout { get; set; }

    [JsonProperty("excludeCredentials")]
    public List<CredentialDescriptorDto> ExcludeCredentials { get; set; }

    [JsonProperty("authenticatorSelection")]
    public AuthenticatorSelectionDto AuthenticatorSelection { get; set; }
}

public class PublicKeyCredentialRequestOptionsDto
{
    [JsonProperty("challenge")]
    public string Challenge { get; set; }

    [JsonProperty("rpId")]
    public string RpId { get; set; }

    [JsonProperty("timeout")]
    public int? Timeout { get; set; }

    [JsonProperty("allowCredentials")]
    public List<CredentialDescriptorDto> AllowCredentials { get; set; }

    [JsonProperty("userVerification")]
    public string UserVerification { get; set; }
}


public class RpDto
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}

public class UserDto
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("displayName")]
    public string DisplayName { get; set; }
}

public class PubKeyCredParamDto
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("alg")]
    public int Alg { get; set; }
}

public class CredentialDescriptorDto
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
}

public class AuthenticatorSelectionDto
{
    [JsonProperty("residentKey")]
    public string ResidentKey { get; set; }

    [JsonProperty("userVerification")]
    public string UserVerification { get; set; }
}