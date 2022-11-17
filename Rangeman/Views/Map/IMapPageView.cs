using Mapsui.UI.Forms;
using System.Threading.Tasks;

namespace Rangeman.Views.Map
{
    public interface IMapPageView
    {
        void PlaceOnMapClicked(Position p);
        void UpdateMapToUseMbTilesFile();
        void AddLinesBetweenPinsAsLayer();
        void RemoveSelectedPin();
        Task DisplayAlert(string title, string message, string button);
        void ShowOnMap(Position p);
    }
}