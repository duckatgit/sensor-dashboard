using Microsoft.AspNetCore.SignalR;

namespace Sensor_Api.Hubs
{
    public class SensorHub :Hub
    {
        private readonly ILogger<SensorHub> _logger;

        public SensorHub(ILogger<SensorHub> logger)
        {
            _logger = logger;
        }

        // Called when a new client connects
        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await Clients.Caller.SendAsync("Connected", $"Connected to SensorHub with ID: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        // Called when a client disconnects
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendCommand(string command)
        {
            _logger.LogInformation($"Command from client: {command}");
            await Clients.All.SendAsync("ReceiveCommand", command);
        }
    }
}
