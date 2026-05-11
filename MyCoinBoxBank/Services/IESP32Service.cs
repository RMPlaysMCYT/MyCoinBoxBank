using System.Threading.Tasks;

namespace MyCoinBoxBank.Services;

public interface IESP32Service
{
    Task<bool> TriggerInsertAsync();
    Task<bool> TriggerWithdrawAsync();
    Task<bool> PingAsync();
    string Esp32IpAddress { get; set; }
}