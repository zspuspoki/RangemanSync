using nexus.protocols.ble;
using nexus.protocols.ble.scan;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Rangeman.Services.BluetoothConnector
{
    internal class BluetoothConnectorService
    {
        private const string WatchDeviceName = "CASIO GPR-B1000";
        private readonly IBluetoothLowEnergyAdapter ble;

        public BluetoothConnectorService(IBluetoothLowEnergyAdapter ble)
        {
            this.ble = ble;
        }

        public async Task FindAndConnectToWatch(Action<string> progressMessageMethod, Func<BlePeripheralConnectionRequest, Task<bool>> successfullyConnectedMethod)
        {
            if (progressMessageMethod is null)
            {
                throw new ArgumentNullException(nameof(progressMessageMethod));
            }

            if (successfullyConnectedMethod is null)
            {
                throw new ArgumentNullException(nameof(successfullyConnectedMethod));
            }

            CancellationTokenSource scanCancellationTokenSource = new CancellationTokenSource();
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
                var connection = await ble.ConnectToDevice(device);

                if (connection.IsSuccessful())
                {
                    try
                    {
                        progressMessageMethod("Successfully connected to the watch.");

                        await successfullyConnectedMethod(connection);
                    }
                    catch(Exception ex)
                    {
                        //TODO: LOg error
                    }
                    finally
                    {
                        await connection.GattServer.Disconnect();
                    }
                }
            }


        }
    }
}