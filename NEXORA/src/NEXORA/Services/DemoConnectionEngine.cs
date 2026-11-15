using System;
using System.Threading.Tasks;
using NEXORA.Models;

namespace NEXORA.Services
{
    // Safe placeholder: this class does NOT create a VPN tunnel.
    // It deliberately reports NotConfigured until a compatible backend is integrated.
    public sealed class DemoConnectionEngine : IConnectionEngine
    {
        public ConnectionStatus Status { get; private set; } = ConnectionStatus.NotConfigured;
        public event EventHandler<ConnectionStatus> StatusChanged;

        public Task ConnectAsync(Profile profile)
        {
            SetStatus(ConnectionStatus.NotConfigured);
            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            SetStatus(ConnectionStatus.Disconnected);
            return Task.CompletedTask;
        }

        private void SetStatus(ConnectionStatus status)
        {
            Status = status;
            var handler = StatusChanged;
            if (handler != null) handler(this, status);
        }
    }
}
