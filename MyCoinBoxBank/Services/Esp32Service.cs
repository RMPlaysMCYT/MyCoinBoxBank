using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyCoinBoxBank.Services;

public class Esp32Service : IESP32Service
{
    private readonly HttpClient _http;
    public string Esp32IpAddress { get; set; } = "192.168.4.1";

    public Esp32Service()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    public async Task<bool> StartCoinAcceptorAsync() => await SendCommand("/start");
    public async Task<bool> StopCoinAcceptorAsync()  => await SendCommand("/stop");
    public async Task<bool> PingAsync()               => await SendCommand("/ping");

    public async Task<decimal> GetLastCoinAsync()
    {
        if (string.IsNullOrWhiteSpace(Esp32IpAddress)) return 0;
        try
        {
            var raw = await _http.GetStringAsync($"http://{Esp32IpAddress}/lastcoin");
            if (int.TryParse(raw.Trim(), out var centavos) && centavos > 0)
                return centavos / 100m; // convert centavos to pesos
            return 0;
        }
        catch { return 0; }
    }

    private async Task<bool> SendCommand(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(Esp32IpAddress)) return false;
        try
        {
            var response = await _http.GetAsync($"http://{Esp32IpAddress}{endpoint}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}