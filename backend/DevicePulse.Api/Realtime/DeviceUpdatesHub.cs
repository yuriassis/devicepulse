using Microsoft.AspNetCore.SignalR;

namespace DevicePulse.Api.Realtime;

public sealed class DeviceUpdatesHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var organizationId = Context.User?.FindFirst("organization_id")?.Value;
        if (!string.IsNullOrWhiteSpace(organizationId)) await Groups.AddToGroupAsync(Context.ConnectionId, $"organization:{organizationId}");
        await base.OnConnectedAsync();
    }
}
