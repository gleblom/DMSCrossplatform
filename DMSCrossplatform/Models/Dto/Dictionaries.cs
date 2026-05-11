using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace DMSCrossplatform.Models.Dto;

public class RoleCreateDto
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}

public class RoleUpdateDto
{
    [JsonProperty("name")] public string? Name { get; set; }
}

public class RoleReadDto
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("company_id")] public Guid? CompanyId { get; set; }
}

public class UnitCreateDto
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("company_ids")] public List<Guid?> CompanyIds { get; set; } = new();
}

public class UnitUpdateDto
{
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("company_ids")] public List<Guid>? CompanyIds { get; set; }
}

public class UnitReadDto: ObservableObject
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("company_ids")] public List<Guid> CompanyIds { get; set; } = new();


}

public class RoleCategoryDto
{
    [JsonProperty("role_id")] public int RoleId { get; set; }
    [JsonProperty("category_ids")] public List<int> CategoryIds { get; set; } = new();
}

public class RoleCategoryReadDto
{
    [JsonProperty("role_id")] public int RoleId { get; set; }
    [JsonProperty("category_id")] public int CategoryId { get; set; } 
}

public class SimpleDto
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}