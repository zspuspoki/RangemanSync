using nexus.core;
using nexus.protocols.ble;
using System;

namespace Rangeman.Services.Common
{
    public static class BleGattServerConnectionExtensions
    {
        public static IDisposable NotifyCharacteristicValue(this IBleGattServerConnection server, Guid service, 
            Guid characteristic, 
            Action<byte[]> onNotify, 
            Action onComplete,
            Action<Exception> onError = null)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }

            if (onNotify == null)
            {
                throw new ArgumentNullException("onNotify");
            }

            return server.NotifyCharacteristicValue(service, characteristic, Observer.Create(delegate (Tuple<Guid, byte[]> tuple)
            {
                onNotify(tuple.Item2);
            }, onComplete, onError));
        }
    }
}