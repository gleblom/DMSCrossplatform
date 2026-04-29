using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DMSCrossplatform.Infrastructure.Api;

public interface IApiClient
{
    Task<TResponse> GetAsync<TResponse>(string path, CancellationToken ct = default);
    Task<TResponse> PostJsonAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default);
    Task PostJsonAsync<TRequest>(string path, TRequest body, CancellationToken ct = default);
    Task<TResponse> PutJsonAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task<TResponse> PostFormAsync<TResponse>(string path, MultipartFormDataContent content, CancellationToken ct = default);
    Task<TResponse> PostUrlEncodedAsync<TResponse>(string path, FormUrlEncodedContent content, CancellationToken ct = default);
    
}