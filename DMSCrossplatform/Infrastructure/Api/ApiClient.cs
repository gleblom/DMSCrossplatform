using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DMSCrossplatform.Infrastructure.Api;

public class ApiClient: IApiClient
{
        private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        },
        NullValueHandling = NullValueHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc
    };

    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _log;

    public ApiClient(HttpClient http, ILogger<ApiClient> log)
    {
        _http = http;
        _log = log;
    }

    public async Task<TResponse> GetAsync<TResponse>(string path, CancellationToken ct = default)
    {
        using var resp = await _http.GetAsync(path, ct);
        
        var content = await resp.Content.ReadAsStringAsync(ct);
        return await ReadAsync<TResponse>(resp, ct);
    }

    public async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default)
    {
        using var resp = await _http.PostAsync(path, JsonContent(body), ct);
        return await ReadAsync<TResponse>(resp, ct);
    }

    public async Task PostJsonAsync<TRequest>(string path, TRequest body, CancellationToken ct = default)
    {
        using var resp = await _http.PostAsync(path, JsonContent(body), ct);
        await EnsureSuccessAsync(resp, ct);
    }

    public async Task<TResponse> PutJsonAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default)
    {
        using var resp = await _http.PutAsync(path, JsonContent(body), ct);
        return await ReadAsync<TResponse>(resp, ct);
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        using var resp = await _http.DeleteAsync(path, ct);
        await EnsureSuccessAsync(resp, ct);
    }

    public async Task<TResponse> PostFormAsync<TResponse>(string path, MultipartFormDataContent content, CancellationToken ct = default)
    {
        using var resp = await _http.PostAsync(path, content, ct);
        return await ReadAsync<TResponse>(resp, ct);
    }

    public async Task<TResponse> PostUrlEncodedAsync<TResponse>(string path, FormUrlEncodedContent content, CancellationToken ct = default)
    {
        using var resp = await _http.PostAsync(path, content, ct);
        return await ReadAsync<TResponse>(resp, ct);
    }

    private static StringContent JsonContent<T>(T body)
    {
        var json = JsonConvert.SerializeObject(body, JsonSettings);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private async Task<T> ReadAsync<T>(HttpResponseMessage resp, CancellationToken ct)
    {
        await EnsureSuccessAsync(resp, ct);
        var raw = await resp.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(raw))
            return default!;
        return JsonConvert.DeserializeObject<T>(raw, JsonSettings)!;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode) return;
        var raw = await resp.Content.ReadAsStringAsync(ct);
        var msg = ExtractMessage(raw, resp.StatusCode);
        _log.LogWarning("API error {Status} {Path}: {Body}", resp.StatusCode, resp.RequestMessage?.RequestUri, raw);
        throw new ApiException(resp.StatusCode, msg, resp.StatusCode.ToString());
    }

    private static string ExtractMessage(string raw, HttpStatusCode status)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return $"Сервер вернул {(int)status}.";
        try
        {
            dynamic? obj = JsonConvert.DeserializeObject<dynamic>(raw);
            if (obj?.detail is not null)
            {
                return obj.detail.ToString();
            }
        }
        catch
        {
            // fallthrough
        }
        return raw;
    }
}