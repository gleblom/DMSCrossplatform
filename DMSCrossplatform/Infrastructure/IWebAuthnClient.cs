using System.Threading;
using System.Threading.Tasks;

namespace DMSCrossplatform.Infrastructure;

public interface IWebAuthnClient
{
    Task<string> RegisterAsync(string optionsJson, CancellationToken ct = default);
    Task<string> AuthenticateAsync(string optionsJson, CancellationToken ct = default);
}