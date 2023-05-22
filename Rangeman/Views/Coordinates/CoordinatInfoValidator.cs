using System;
using System.Collections.ObjectModel;

namespace Rangeman.Views.Coordinates
{
    public class CoordinateInfoValidator
    {
        private readonly ObservableCollection<CoordinateInfo> coordinates;

        public CoordinateInfoValidator(ObservableCollection<CoordinateInfo> coordinates)
        {
            this.coordinates = coordinates;
        }

        public bool IsValid(out string erorMessages)
        {
            erorMessages = "";
            if(!CollectionHasValidStart())
            {
                erorMessages = "The start point has invalid coordinates.";
                return false;
            }

            if (!CollectionHasValidGoal())
            {
                erorMessages = "The goal point has invalid coordinates.";
                return false;
            }

            for(var i=1;i<=9;i++)
            {
                if(!TransitPointIsCorrect((c)=> c.NodeName == i.ToString()))
                {
                    erorMessages = $"The {i}. transit point has invalid coordinates.";
                    return false;
                }
            }

            return true;
        }

        private bool CollectionHasValidStart()
        {
            return CheckIfCollectionIsCorrect((c)=> c.NodeName == "S");
        }

        private bool CollectionHasValidGoal()
        {
            return CheckIfCollectionIsCorrect((c) => c.NodeName == "G");
        }

        private bool TransitPointIsCorrect(Func<CoordinateInfo, bool> coordinateTypeIsAcceptableFunc)
        {
            foreach (var enteredCoordinate in coordinates)
            {
                if (coordinateTypeIsAcceptableFunc(enteredCoordinate))
                {
                    if (string.IsNullOrWhiteSpace(enteredCoordinate.Coordinates))
                    {
                        return true;
                    }

                    var splittedValue = enteredCoordinate.Coordinates.Split(',');
                    if (splittedValue.Length != 2)
                    {
                        return false;
                    }

                    if (!double.TryParse(splittedValue[0], out var _))
                    {
                        return false;
                    }

                    if (!double.TryParse(splittedValue[0], out var _))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CheckIfCollectionIsCorrect(Func<CoordinateInfo, bool> coordinateTypeIsAcceptableFunc)
        {
            foreach (var enteredCoordinate in coordinates)
            {
                if(coordinateTypeIsAcceptableFunc(enteredCoordinate))
                {
                    if (string.IsNullOrWhiteSpace(enteredCoordinate.Coordinates))
                    {
                        return false;
                    }

                    var splittedValue = enteredCoordinate.Coordinates.Split(',');
                    if (splittedValue.Length != 2)
                    {
                        return false;
                    }

                    if (!double.TryParse(splittedValue[0], out var _))
                    {
                        return false;
                    }

                    if (!double.TryParse(splittedValue[0], out var _))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}