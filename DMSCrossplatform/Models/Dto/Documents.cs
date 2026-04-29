namespace DMSCrossplatform.Models.Dto;

using System;
using Newtonsoft.Json;


public class DocumentCreateDto
{
    [JsonProperty("title")] public string Title { get; set; } = string.Empty;
    [JsonProperty("category_id")] public int CategoryId { get; set; }
    [JsonProperty("unit_id")] public int UnitId { get; set; }
    [JsonProperty("expires_at")] public DateTime? ExpiresAt { get; set; }
}

public class DocumentReadDto
{
    [JsonProperty("id")] public Guid Id { get; set; }
    [JsonProperty("title")] public string Title { get; set; } = string.Empty;
    [JsonProperty("current_step_index")] public int CurrentStepIndex { get; set; }
    [JsonProperty("status_id")] public int StatusId { get; set; }
    [JsonProperty("category_id")] public int CategoryId { get; set; }
    [JsonProperty("route_id")] public int? RouteId { get; set; }
    [JsonProperty("author_id")] public Guid AuthorId { get; set; }
    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
    [JsonProperty("updated_at")] public DateTime UpdatedAt { get; set; }
}
public class DocumentFullReadDto
{
    [JsonProperty("id")] public Guid Id { get; set; }
    [JsonProperty("title")] public string Title { get; set; } = string.Empty;
    [JsonProperty("current_step_index")] public int CurrentStepIndex { get; set; }
    [JsonProperty("status_id")] public int StatusId { get; set; }
    [JsonProperty("category_id")] public int CategoryId { get; set; }
    [JsonProperty("route_id")] public int? RouteId { get; set; }
    [JsonProperty("author_id")] public Guid AuthorId { get; set; }
    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
    [JsonProperty("updated_at")] public DateTime UpdatedAt { get; set; }
    [JsonProperty("unit_id")] public int UnitId { get; set; }
    [JsonProperty("latest_version_id")] public Guid? LatestVersionId { get; set; }
    [JsonProperty("latest_version_number")] public int? LatestVersionNumber { get; set; }
    [JsonProperty("author_email")] public string AuthorEmail { get; set; } = string.Empty;
    [JsonProperty("unit_name")] public string UnitName { get; set; } = string.Empty;
    [JsonProperty("status_name")] public string StatusName { get; set; } = string.Empty;
    [JsonProperty("category_name")] public string CategoryName { get; set; } = string.Empty;
    [JsonProperty("route_name")] public string? RouteName { get; set; }
    [JsonProperty("expires_at")] public DateTime? ExpiresAt { get; set; }
    [JsonProperty("first_name")] public string FirstName { get; set; } = string.Empty;
    [JsonProperty("second_name")] public string SecondName { get; set; } = string.Empty;
    [JsonProperty("third_name")] public string ThirdName { get; set; } = string.Empty;
}
public class DocumentSubmitDto
{
    [JsonProperty("route_id")] public int RouteId { get; set; }
}

public class DocumentVersionCreateDto
{
    [JsonProperty("storage_object_name")] public string StorageObjectName { get; set; } = string.Empty;
    [JsonProperty("original_file_name")] public string OriginalFileName { get; set; } = string.Empty;
    [JsonProperty("mime_type")] public string MimeType { get; set; } = string.Empty;
    [JsonProperty("file_size")] public long FileSize { get; set; }
}

public class DocumentVersionReadDto
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("document_id")] public Guid DocumentId { get; set; }
    [JsonProperty("version_number")] public int VersionNumber { get; set; }
    [JsonProperty("url")] public string Url { get; set; } = string.Empty;
    [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
}