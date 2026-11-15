using System;
using System.Threading.Tasks;
using NEXORA.Models;

namespace NEXORA.Services
{
    // Contract for a real VPN backend. Implementations must be added and tested
    // separately for each supported Windows version and CPU architecture.
    public interface IConnectionEngine
    {
        ConnectionStatus Status { get; }
        event EventHandler<ConnectionStatus> StatusChanged;
        Task ConnectAsync(Profile profile);
        Task DisconnectAsync();
    }
}
