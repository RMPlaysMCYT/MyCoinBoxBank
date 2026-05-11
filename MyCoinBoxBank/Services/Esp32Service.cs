using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyCoinBoxBank.Services;

public class Esp32Service : IESP32Service
{
    private readonly HttpClient _http;
    public string Esp32IpAddress { get; set; } = "";

    public Esp32Service()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<bool> TriggerInsertAsync()
    {
        return await SendCommand("/insert");
    }
    public async Task<bool> TriggerWithdrawAsync()
    {
        return await SendCommand("/withdraw");
    }

    public async Task<bool> PingAsync()
    {
        return await SendCommand("/ping");
    }

    private async Task<bool> SendCommand(string endpoint)
    {
        try
        {
            var response = await _http.GetAsync($"http://{Esp32IpAddress}{endpoint}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

