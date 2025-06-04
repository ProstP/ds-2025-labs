using System.Text;
using System.Text.Json;
using ProtoCli.Client.Contract.Request;
using ProtoCli.Client.Contract.Response;

namespace ProtoCli.Client;

public class ProtoKeyClient
{
    private readonly string _storageHost;
    private readonly HttpClient _httpClient;

    public ProtoKeyClient(
        string storageHost)
    {
        _storageHost = storageHost;
        _httpClient = new();
    }

    public async Task Set(string key, int value)
    {
        SetRequest request = new()
        {
            Key = key,
            Value = value,
        };
        string json = JsonSerializer.Serialize(request);
        using StringContent stringContent = new(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClient.PostAsync($"{_storageHost}/set", stringContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new ArgumentException
                ($"Server error: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        }

        return;
    }
    public async Task<int> GetAsync(string key)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"{_storageHost}/get/{key}");

        if (!response.IsSuccessStatusCode)
        {
            throw new ArgumentException
                ($"Server error: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        }

        string jsonData = await response.Content.ReadAsStringAsync();
        GetResponse data = JsonSerializer.Deserialize<GetResponse>(jsonData, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
        return data.Value;
    }
    public async Task<string[]> KeysAsync(string prefix)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"{_storageHost}/keys?prefix={prefix}");

        if (!response.IsSuccessStatusCode)
        {
            throw new ArgumentException
                ($"Server error: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        }

        string jsonData = await response.Content.ReadAsStringAsync();
        KeysResponse data = JsonSerializer.Deserialize<KeysResponse>(jsonData, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
        return data.Keys;
    }
}