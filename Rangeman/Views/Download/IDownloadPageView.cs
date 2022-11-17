
using System.Threading.Tasks;

namespace Rangeman.Views.Download
{
    public interface IDownloadPageView
    {
        Task<bool> DisplayAlert(string title, string message, string yes, string no);
        Task DisplayAlert(string title, string message, string yes);
    }
}