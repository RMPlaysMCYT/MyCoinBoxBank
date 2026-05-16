using System.Threading.Tasks;

namespace MyCoinBoxBank.Services;

public interface IESP32Service
{
    Task<bool> StartCoinAcceptorAsync();
    Task<bool> StopCoinAcceptorAsync();
    Task<decimal> GetLastCoinAsync(); // returns 0 if no coin yet
    Task<bool> PingAsync();
    string Esp32IpAddress { get; set; }
}