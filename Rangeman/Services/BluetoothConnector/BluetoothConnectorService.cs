using Microsoft.Extensions.Logging;
using nexus.core;
using nexus.protocols.ble;
using nexus.protocols.ble.scan;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rangeman.Services.BluetoothConnector
{
    public class BluetoothConnectorService
    {
        private const string WatchDeviceName = "CASIO GPR-B1000";
        private const string WellKnownServiceGuidOfCasio = "00001804-0000-1000-8000-00805f9b34fb";

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
                Func<BlePeripheralConnectionRequest, Task<bool>> successfullyConnectedMethod,
                Func<Task<bool>> watchCommandExecutionFailed = null,
                Action beforeStartScanningMethod = null,
                double? timeout = null)
        {
            if (progressMessageMethod is null)
            {
                throw new ArgumentNullException(nameof(progressMessageMethod));
            }

            if (successfullyConnectedMethod is null)
            {
                throw new ArgumentNullException(nameof(successfullyConnectedMethod));
            }

            if(scanCancellationTokenSource != null)
            {
                if(!scanCancellationTokenSource.IsCancellationRequested)
                {
                    logger.LogDebug("Bluetooth connector: Existing cancellation token detected. Requesting cancellation ...");
                    scanCancellationTokenSource.Cancel();
                }
            }

            scanCancellationTokenSource = new CancellationTokenSource();

            if (beforeStartScanningMethod != null)
            {
                beforeStartScanningMethod();
            }

            logger.LogDebug($"--- BluetoothConnectorService - before ScanForBroadcasts, (scanCancellationTokenSource == null) = {scanCancellationTokenSource == null}");

            progressMessageMethod("Trying to connect to Casio device ...");

            if (timeout != null)
            {
                currentConnection = await ble.FindAndConnectToDevice(
                    new ScanFilter().AddAdvertisedService(new Guid(WellKnownServiceGuidOfCasio)),
                    TimeSpan.FromSeconds(timeout.Value));
            }
            else
            {
                currentConnection = await ble.FindAndConnectToDevice(
                    new ScanFilter().AddAdvertisedService(new Guid(WellKnownServiceGuidOfCasio)),
                    scanCancellationTokenSource.Token);
            }

            if (currentConnection.IsSuccessful())
            {
                logger.LogDebug("--- BluetoothConnectorService - successfully connected device");

                try
                {
                    using var state = ble.CurrentState.Subscribe((state) =>
                        logger.LogDebug($"Bluetooth state changed! State: {state}"),
                        () => logger.LogDebug("Bluetooth state checker ended!"),
                        (exception) => logger.LogError(exception, "An unexpected error occured during checking the state."));

                    progressMessageMethod("Successfully connected to the watch.");

                    await successfullyConnectedMethod(currentConnection);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An unexpected error occured during running watch commands");

                    if (watchCommandExecutionFailed != null)
                    {
                        await watchCommandExecutionFailed();
                    }
                }
                finally
                {
                    await currentConnection.GattServer.Disconnect();
                }
            }
            else
            {
                logger.LogDebug("--- BluetoothConnectorService - failed to connect");
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