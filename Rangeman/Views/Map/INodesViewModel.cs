using System.Collections.Generic;

namespace Rangeman.Views.Map
{
    public interface INodesViewModel
    {
        IEnumerable<GpsCoordinatesViewModel> GetStartEndCoordinates(bool removeEmptyEntries = true);
        List<GpsCoordinatesViewModel> GetTransitPointCoordinates(bool removeEmptyEntries = true);
    }
}