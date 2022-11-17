using System;
using Debug = System.Diagnostics.Debug;
using Rangeman.Common;
using Microsoft.Extensions.Logging;

namespace Rangeman
{
    internal class CharChangedObserver : IObserver<Tuple<Guid, byte[]>>
    {
        private ILogger<CharChangedObserver> logger;

        public CharChangedObserver(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<CharChangedObserver>();
        }

        public void OnCompleted()
        {
            logger.LogDebug("COmpleted");
        }

        public void OnError(Exception error)
        {
            logger.LogDebug("OnError");
        }

        public void OnNext(Tuple<Guid, byte[]> value)
        {
            logger.LogDebug($"OnNext Guid = { value.Item1}  value = { Utils.GetPrintableBytesArray(value.Item2 )}");
        }
    }
}