using System;
using System.Net.Http;

namespace MyCoinBoxBank.Services;

public class Esp32Service : IESP32Service
{
    private readonly HttpClient _http;
    public string Esp32IpAddress { get; set; } = "";

    public Esp32Service()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

}

