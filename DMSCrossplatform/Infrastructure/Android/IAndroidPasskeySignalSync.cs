using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DMSCrossplatform.Infrastructure.Android;

public interface IAndroidPasskeySignalSync
{
    Task SignalAcceptedIdsAsync(
        string rpId,
        Guid? userId,
        IReadOnlyList<string> activeCredentialIds,
        CancellationToken ct = default);
}