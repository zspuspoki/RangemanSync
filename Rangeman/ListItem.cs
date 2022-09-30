using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using nexus.protocols.ble.scan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rangeman
{
    internal class ListItem
    {
        public string Name { get; set; }
        public IBlePeripheral Device { get; set; }
    }
}