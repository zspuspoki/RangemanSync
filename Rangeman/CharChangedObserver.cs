﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Debug = System.Diagnostics.Debug;

namespace Rangeman
{
    internal class CharChangedObserver : IObserver<Tuple<Guid, byte[]>>
    {
        public void OnCompleted()
        {
            Debug.WriteLine("COmpleted");
        }

        public void OnError(Exception error)
        {
            Debug.WriteLine("OnError");
        }

        public void OnNext(Tuple<Guid, byte[]> value)
        {
            Debug.WriteLine($"OnNext Guid = { value.Item1}  value = { Util.GetPrintableBytesArray(value.Item2 )}");
        }
    }
}