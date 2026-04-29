using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace DMSCrossplatform.Infrastructure.Storage;

public class FileTokenStorage: ITokenStorage
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public FileTokenStorage()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DMSCrossplatform");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "sessions.json");
    }

    public Task<IReadOnlyList<StoredSession>> GetAllAsync()
        => Task.FromResult<IReadOnlyList<StoredSession>>(Read().Sessions);

    public Task<StoredSession?> GetActiveAsync()
    {
        var data = Read();
        return Task.FromResult(data.Sessions.FirstOrDefault(s => s.Email == data.ActiveEmail));
    }

    public Task SetActiveAsync(string email)
    {
        var data = Read();
        data.ActiveEmail = email;
        Write(data);
        return Task.CompletedTask;
    }

    public Task SaveAsync(StoredSession session)
    {
        var data = Read();
        data.Sessions.RemoveAll(s => s.Email == session.Email);
        data.Sessions.Add(session);
        data.ActiveEmail = session.Email;
        Write(data);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string email)
    {
        var data = Read();
        data.Sessions.RemoveAll(s => s.Email == email);
        if (data.ActiveEmail == email) data.ActiveEmail = null;
        Write(data);
        return Task.CompletedTask;
    }

    public Task ClearActiveAsync()
    {
        var data = Read();
        data.ActiveEmail = null;
        Write(data);
        return Task.CompletedTask;
    }

    private SessionsData Read()
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath)) return new SessionsData();
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<SessionsData>(json) ?? new SessionsData();
            }
            catch
            {
                return new SessionsData();
            }
        }
    }

    private void Write(SessionsData data)
    {
        lock (_lock)
        {
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }

    private class SessionsData
    {
        public string? ActiveEmail { get; set; }
        public List<StoredSession> Sessions { get; set; } = new();
    }
}