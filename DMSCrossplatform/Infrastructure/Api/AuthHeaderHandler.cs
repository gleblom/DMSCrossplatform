using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DMSCrossplatform.Services;

namespace DMSCrossplatform.Infrastructure.Api;

public class AuthHeaderHandler: DelegatingHandler
{
    private readonly ISessionService _session;

    public AuthHeaderHandler(ISessionService session)
    {
        _session = session;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await ApplyTokenAsync(request);
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized) return response;

        // Пробуем рефрешнуть и повторить
        var refreshed = await _session.TryRefreshAsync(cancellationToken);
        if (!refreshed) return response;

        response.Dispose();
        await ApplyTokenAsync(request);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task ApplyTokenAsync(HttpRequestMessage request)
    {
        var token = await _session.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}