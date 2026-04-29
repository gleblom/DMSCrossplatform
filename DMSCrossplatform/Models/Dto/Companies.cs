namespace DMSCrossplatform.Models.Dto;

using System;
using Newtonsoft.Json;


public class CompanyCreateDto
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("director_id")] public Guid DirectorId { get; set; }
}

public class CompanyReadDto
{
    [JsonProperty("id")] public Guid Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("director_id")] public Guid DirectorId { get; set; }
}

public class CompanyUpdateDto
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("company_id")] public Guid CompanyId { get; set; }
}