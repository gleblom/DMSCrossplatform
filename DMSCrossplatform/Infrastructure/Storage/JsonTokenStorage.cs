using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DMSCrossplatform.Infrastructure.Storage;


public sealed class JsonTokenStorage : ITokenStorage
{
    private const string StorageKey = "dms.sessions.v1";
    private readonly ISessionBlobStore _store;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonTokenStorage(ISessionBlobStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<StoredSession>> GetAllAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var data = await ReadCoreAsync().ConfigureAwait(false);
            return data.Sessions;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<StoredSession?> GetActiveAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var data = await ReadCoreAsync().ConfigureAwait(false);
            return data.Sessions.FirstOrDefault(s =>
                string.Equals(s.Email, data.ActiveEmail, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SetActiveAsync(string email)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var data = await ReadCoreAsync().ConfigureAwait(false);
            data.ActiveEmail = email;
            await WriteCoreAsync(data).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(StoredSession session)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var data = await ReadCoreAsync().ConfigureAwait(false);
            data.Sessions.RemoveAll(s =>
                string.Equals(s.Email, session.Email, StringComparison.OrdinalIgnoreCase));
            data.Sessions.Add(session);
            data.ActiveEmail = session.Email;
            await WriteCoreAsync(data).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RemoveAsync(string email)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var data = await ReadCoreAsync().ConfigureAwait(false);
            data.Sessions.RemoveAll(s =>
                string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));

            if (string.Equals(data.ActiveEmail, email, StringComparison.OrdinalIgnoreCase))
                data.ActiveEmail = null;

            await WriteCoreAsync(data).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task ClearActiveAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            var data = await ReadCoreAsync().ConfigureAwait(false);
            data.ActiveEmail = null;
            await WriteCoreAsync(data).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<SessionsData> ReadCoreAsync()
    {
        try
        {
            var json = await _store.ReadAsync(StorageKey).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
                return new SessionsData();

            return JsonSerializer.Deserialize<SessionsData>(json, JsonOptions) ?? new SessionsData();
        }
        catch
        {
            return new SessionsData();
        }
    }

    private Task WriteCoreAsync(SessionsData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        return _store.WriteAsync(StorageKey, json).AsTask();
    }

    private sealed class SessionsData
    {
        public string? ActiveEmail { get; set; }
        public List<StoredSession> Sessions { get; set; } = new();
    }
}