using System.Collections.Generic;

namespace employeeID.Droid
{
    public class AppPermissions : Xamarin.Essentials.Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
        {
            ("android.permission.BLUETOOTH_SCAN", true),
            ("android.permission.BLUETOOTH_CONNECT", true),
            ("android.permission.ACCESS_FINE_LOCATION", true),
            ("android.permission.WRITE_EXTERNAL_STORAGE", true),
            ("android.permission.READ_EXTERNAL_STORAGE", true)
        }.ToArray();
    }
}