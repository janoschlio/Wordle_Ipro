using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BlazorServerApp.Services.Player;

public class PlayerService
{
    private const string StorageKey = "wordle-player-id";

    private readonly ProtectedLocalStorage _storage;

    private string? _playerId;

    public PlayerService(ProtectedLocalStorage storage)
    {
        _storage = storage;
    }
    
    public async Task<string> EnsurePlayerIdAsync()
    {
        if (_playerId is not null)
            return _playerId;

        _playerId = await ReadStoredIdAsync() ?? await CreateIdAsync();

        return _playerId;
    }

    private async Task<string?> ReadStoredIdAsync()
    {
        try
        {
            var stored = await _storage.GetAsync<string>(StorageKey);

            return stored.Success && !string.IsNullOrWhiteSpace(stored.Value)
                ? stored.Value
                : null;
        }
        catch (CryptographicException)
        {
            // Der Wert liegt verschluesselt im Browser. Nach einem Neustart des
            // Servers sind die Data-Protection-Schluessel weg und der alte Wert
            // laesst sich nicht mehr lesen -- dann gibt es eine neue Kennung.
            return null;
        }
    }

    private async Task<string> CreateIdAsync()
    {
        var id = Guid.NewGuid().ToString();
        await _storage.SetAsync(StorageKey, id);

        return id;
    }
}
