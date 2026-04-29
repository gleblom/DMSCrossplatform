using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DMSCrossplatform.Models.Dto;

public class ApprovalRouteCreateDto
{
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
}

public class ApprovalRouteUpdateDto
{
    [JsonProperty("name")] public string? Name { get; set; }
}

public class ApprovalRouteReadDto
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = string.Empty;
    [JsonProperty("created_by")] public Guid CreatedBy { get; set; }
    [JsonProperty("company_id")] public Guid CompanyId { get; set; }
}

public class RouteNodeCreateDto
{
    [JsonProperty("approver_id")] public Guid ApproverId { get; set; }
    [JsonProperty("step_index")] public int StepIndex { get; set; }
}

public class RouteNodeUpdateDto
{
    [JsonProperty("approver_id")] public Guid? ApproverId { get; set; }
    [JsonProperty("step_index")] public int? StepIndex { get; set; }
}

public class RouteNodeReadDto
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("route_id")] public int RouteId { get; set; }
    [JsonProperty("approver_id")] public Guid ApproverId { get; set; }
    [JsonProperty("step_index")] public int StepIndex { get; set; }
}

public class RouteEdgeCreateDto
{
    [JsonProperty("from_node_id")] public int FromNodeId { get; set; }
    [JsonProperty("to_node_id")] public int ToNodeId { get; set; }
}

public class RouteEdgeReadDto
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("route_id")] public int RouteId { get; set; }
    [JsonProperty("from_node_id")] public int FromNodeId { get; set; }
    [JsonProperty("to_node_id")] public int ToNodeId { get; set; }
}

public class RouteGraphNodeDto
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("route_id")] public int RouteId { get; set; }
    [JsonProperty("approver_id")] public Guid ApproverId { get; set; }
    [JsonProperty("approver_email")] public string? ApproverEmail { get; set; }
    [JsonProperty("approver_full_name")] public string? ApproverFullName { get; set; }
    [JsonProperty("step_index")] public int StepIndex { get; set; }
    [JsonProperty("incoming_count")] public int IncomingCount { get; set; }
    [JsonProperty("outgoing_count")] public int OutgoingCount { get; set; }
    [JsonProperty("is_start")] public bool IsStart { get; set; }
    [JsonProperty("is_terminal")] public bool IsTerminal { get; set; }
    [JsonProperty("level")] public int Level { get; set; }
}

public class RouteGraphDto
{
    [JsonProperty("route")] public ApprovalRouteReadDto Route { get; set; } = new();
    [JsonProperty("nodes")] public List<RouteGraphNodeDto> Nodes { get; set; } = new();
    [JsonProperty("edges")] public List<RouteEdgeReadDto> Edges { get; set; } = new();
    [JsonProperty("levels")] public List<List<int>> Levels { get; set; } = new();
}


