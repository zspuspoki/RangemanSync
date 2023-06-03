using Microsoft.Extensions.Logging;
using nexus.protocols.ble;
using Rangeman.WatchDataSender;
using System.Threading.Tasks;

namespace Rangeman.Services.WatchDataSender
{
    internal class WatchDataSettingSenderService
    {
        private readonly BlePeripheralConnectionRequest connection;
        private readonly ILoggerFactory loggerFactory;
        private ILogger<WatchDataSettingSenderService> logger;

        public WatchDataSettingSenderService(BlePeripheralConnectionRequest connection, ILoggerFactory loggerFactory)
        {
            this.connection = connection;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<WatchDataSettingSenderService>();
        }

        public async Task SendTime(ushort year, byte month, byte day, byte hour, byte minute, byte seconds, byte dayOfWeek, byte miliseconds)
        {
            logger.LogInformation("--- Starting SendTime()");

            var remoteWatchController = new RemoteWatchController(this.connection.GattServer, loggerFactory);

            await remoteWatchController.SetCurrentTime(year, month, day, hour, minute, seconds, dayOfWeek, miliseconds);
        }
    }
}