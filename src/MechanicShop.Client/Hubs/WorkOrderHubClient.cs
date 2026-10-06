using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace MechanicShop.Client.Hubs;

public sealed class WorkOrderHubClient : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private bool _isStarted;
    private bool _isDisposed;

    public WorkOrderHubClient(IWebAssemblyHostEnvironment env)
    {
        var baseUrl = env.BaseAddress;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}hubs/workorders")
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task StartAsync(Func<Task> onWorkOrdersChanged)
    {
        if (_isDisposed || _isStarted)
        {
            return;
        }

        _hubConnection.On("WorkOrdersChanged", async () =>
        {
            if (!_isDisposed)
            {
                await onWorkOrdersChanged.Invoke();
            }
        });

        await _hubConnection.StartAsync();
        _isStarted = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_hubConnection.State is HubConnectionState.Connected or HubConnectionState.Connecting)
        {
            await _hubConnection.StopAsync();
        }

        await _hubConnection.DisposeAsync();
    }
}