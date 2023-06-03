using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace Rangeman.Views.Time
{
    public class NTPTimeInfo
    {
        [Display(ShortName = "NTP server's URL")]
        public string NTPServer { get; set; }
    }
}