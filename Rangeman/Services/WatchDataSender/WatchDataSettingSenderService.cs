﻿using Microsoft.Extensions.Logging;
using nexus.protocols.ble;
using Rangeman.WatchDataSender;
using System.Threading.Tasks;

namespace Rangeman.Services.WatchDataSender
{
    public class WatchDataSettingSenderService
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

        public async Task SendTime(ushort year, byte month, byte day, byte hour, byte minute, byte seconds, byte dayOfWeek, byte miliseconds,
            double? latitude, double? longitude)
        {
            logger.LogInformation("--- Starting SendTime()");

            var remoteWatchController = new RemoteWatchController(this.connection.GattServer, loggerFactory);

            await remoteWatchController.SetCurrentTime(year, month, day, hour, minute, seconds, dayOfWeek, miliseconds, latitude, longitude);
        }

        public async Task SendTide(string cityName, double latitude, double longitude, ushort year, byte month, byte day, byte hour, byte minute)
        {
            logger.LogInformation("--- Starting SendTide()");

            var remoteWatchController = new RemoteWatchController(this.connection.GattServer, loggerFactory);

            await remoteWatchController.SetTide(cityName, latitude, longitude, year, month, day, hour, minute);
        }
    }
}