using Microsoft.Extensions.Logging;
using nexus.core;
using nexus.protocols.ble;
using nexus.protocols.ble.scan;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Rangeman.Services.BluetoothConnector
{
    public class BluetoothConnectorService
    {
        private const string WatchDeviceName = "CASIO GPR-B1000";
        private readonly IBluetoothLowEnergyAdapter ble;
        private readonly ILogger<BluetoothConnectorService> logger;

        private CancellationTokenSource scanCancellationTokenSource = null;
        private BlePeripheralConnectionRequest currentConnection;

        public BluetoothConnectorService(IBluetoothLowEnergyAdapter ble, ILogger<BluetoothConnectorService> logger)
        {
            this.ble = ble;
            this.logger = logger;
        }

        public async Task FindAndConnectToWatch(Action<string> progressMessageMethod, 
                Func<BlePeripheralConnectionRequest,Task<bool>> successfullyConnectedMethod,
                Func<Task<bool>> watchCommandExecutionFailed = null,
                Action beforeStartScanningMethod = null)
        {
            if (progressMessageMethod is null)
            {
                throw new ArgumentNullException(nameof(progressMessageMethod));
            }

            if (successfullyConnectedMethod is null)
            {
                throw new ArgumentNullException(nameof(successfullyConnectedMethod));
            }

            scanCancellationTokenSource = new CancellationTokenSource();

            if(beforeStartScanningMethod!= null)
            {
                beforeStartScanningMethod();
            }

            IBlePeripheral device = null;

            await ble.ScanForBroadcasts((a) =>
            {
                if (a.Advertisement != null)
                {
                    var advertisedName = a.Advertisement.DeviceName;

                    Debug.WriteLine($"--- BluetoothConnectorService Looking for connected watch, advertised device name: {advertisedName}");

                    if (advertisedName != null &&
                        advertisedName.Contains(WatchDeviceName))
                    {
                        Debug.WriteLine("--- BluetoothConnectorService - advertised name contains CASIO");

                        device = a;

                        scanCancellationTokenSource.Cancel();
                    }
                }
            }, scanCancellationTokenSource.Token);


            if (device != null)
            {
                progressMessageMethod("Found Casio device. Trying to connect ...");
                currentConnection = await ble.ConnectToDevice(device);

                if (currentConnection.IsSuccessful())
                {
                    try
                    {
                        progressMessageMethod("Successfully connected to the watch.");

                        await successfullyConnectedMethod(currentConnection);
                    }
                    catch(Exception ex)
                    {
                        logger.LogError(ex, "An unexpected error occured during running watch commands");

                        if(watchCommandExecutionFailed != null)
                        {
                            await watchCommandExecutionFailed();
                        }
                    }
                    finally
                    {
                        await currentConnection.GattServer.Disconnect();
                    }
                }
            }
        }

        public async Task DisconnectFromWatch(Action<string> progressMessageMethod)
        {
            try
            {
                if (progressMessageMethod is null)
                {
                    throw new ArgumentNullException(nameof(progressMessageMethod));
                }

                logger.LogInformation("Started DisconnectFromWatch");

                if(scanCancellationTokenSource != null && !scanCancellationTokenSource.IsCancellationRequested)
                {
                    scanCancellationTokenSource.Cancel();
                    progressMessageMethod("Bluetooth: GPR-B1000 device scanning successfully aborted.");
                }

                if (currentConnection.ConnectionResult == ConnectionResult.Success)
                {
                    if (currentConnection.GattServer != null)
                    {
                        await currentConnection.GattServer.Disconnect();
                        currentConnection.GattServer.TryDispose();
                        progressMessageMethod("Watch successfully disconnected from the phone.");
                    }
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "An unexpected error occured during disconnecting the watch.");
                progressMessageMethod("An unexpected error occured during disconnecting the watch.");
            }
        }
    }
}