using System;
using System.Collections.Generic;

namespace Rangeman.Views.Map
{
    public class NodesViewModel : ViewModelBase
    {
        private CircularLinkedList<NodeViewModel> nodes;

        private LinkedListNode<NodeViewModel> currentSelectedLinkedListNode = null;
        private NodeViewModel currentSelectedNode;
        private NodeViewModel userSelectedPinNodeForDeletion;

        public NodeViewModel CurrentSelectedNode 
        { 
            get
            {
                return currentSelectedNode;
            }
            set
            {
                currentSelectedNode = value;
                OnPropertyChanged(nameof(CurrentSelectedNode));
            }
        }

        public NodesViewModel()
        {
            var availableNodes = new List<NodeViewModel>()
            {
                new NodeViewModel
                {
                    Title = "S",
                    Category = NodeCategory.StartEnd,
                    Visible = true
                },
                new NodeViewModel
                {
                    Title = "G",
                    Category = NodeCategory.StartEnd,
                    Visible = false
                }
            };

            for (var i = 1; i <= 9; i++)
            {
                availableNodes.Add(new NodeViewModel
                {
                    Title = i.ToString(),
                    Category = NodeCategory.Transit,
                    Visible = false
                });
            }

            nodes = new CircularLinkedList<NodeViewModel>(availableNodes);
            currentSelectedLinkedListNode = nodes.First;
            CurrentSelectedNode = currentSelectedLinkedListNode.Value;
        }

        public void SelectNodeForDeletion(string title, double longitude, double latitude)
        {
            var node = new NodeViewModel
            {
                Title = title,
                Category = title == "S" &&
                           title == "G" ? NodeCategory.StartEnd : NodeCategory.Transit,
                Longitude = longitude,
                Latitude = latitude
            };

            userSelectedPinNodeForDeletion = node;
        }

        public void DeleteSelectedNode()
        {
            if (userSelectedPinNodeForDeletion != null)
            {
                var linkedListNodeToDelete = nodes.Find(userSelectedPinNodeForDeletion);
                linkedListNodeToDelete.Value.InvalidateLongLatValues();
                linkedListNodeToDelete.Value.Visible = true;

                currentSelectedLinkedListNode = linkedListNodeToDelete;  // let's select it again, so it can be placed again on the map
                CurrentSelectedNode = currentSelectedLinkedListNode.Value;
                userSelectedPinNodeForDeletion = null;
            }
            else
            {
                throw new InvalidOperationException("Please select a node (1-9 or S/G) to delete before pressing the delete node button.");
            }
        }

        /// <summary>
        /// This should be exectuted when node selection button is clicked in the bottom right corener
        /// </summary>
        public void ClickOnSelectNode()
        {
            do
            {
                var oldNode = currentSelectedLinkedListNode;
                currentSelectedLinkedListNode = currentSelectedLinkedListNode.Next();
                if(currentSelectedLinkedListNode == null)
                {
                    currentSelectedLinkedListNode = oldNode;
                    break;
                }
            } while (currentSelectedLinkedListNode.Value.HasValidCoordinates || 
                    currentSelectedLinkedListNode.Value.Visible == false);
            CurrentSelectedNode = currentSelectedLinkedListNode.Value;
        }

        public string AddNodeToMap(double longitude, double latitude)
        {
            if (currentSelectedLinkedListNode != null)
            {
                if(currentSelectedLinkedListNode.Value is MissingNodeToAddViewModel)
                {
                    return null;
                }

                var nodeTitle = currentSelectedLinkedListNode.Value.Title;
                currentSelectedLinkedListNode.Value.Longitude = longitude;
                currentSelectedLinkedListNode.Value.Latitude = latitude;
                currentSelectedLinkedListNode.Value.Visible = false;

                SetCurrentNodeThatCanBePlacedOnMap(nodeTitle, () => 
                {
                    var missingNodeToAddViewModel = new MissingNodeToAddViewModel();
                    currentSelectedLinkedListNode = new LinkedListNode<NodeViewModel>(missingNodeToAddViewModel);
                });

                return nodeTitle;
            }

            return null;
        }

        public bool HasTheSameLatitudeLongitudeNode(double latitude, double longitude)
        {
            LinkedListNode<NodeViewModel> node = nodes.First;
            if (node != null)
            {
                do
                {
                    if(node.Value.Latitude == latitude && node.Value.Longitude == longitude)
                    {
                        return true;
                    }

                    node = node.Next;
                } while (node != nodes.First && node!= null);
            }

            return false;
        }

        public IEnumerable<GpsCoordinatesViewModel> GetStartEndCoordinates(bool removeEmptyEntries = true)
        {
            return GetCoordinatesByCategory(new NodeViewModel { Title = "S" }, NodeCategory.StartEnd, removeEmptyEntries);
        }

        public List<GpsCoordinatesViewModel> GetTransitPointCoordinates(bool removeEmptyEntries = true)
        {
            var result = GetCoordinatesByCategory(new NodeViewModel { Title = "1" }, NodeCategory.Transit, removeEmptyEntries);
            return result;
        }

        public List<GpsCoordinatesViewModel> GetLineConnectableCoordinatesFromStartToGoal()
        {
            var result = new List<GpsCoordinatesViewModel>();
            result.AddRange(GetStartEndCoordinates(false));
            
            var transitpointCoordinates = GetTransitPointCoordinates(false);
            var trimIndex = -1;
            bool enableTrimming = false;

            for(int i=1;i<transitpointCoordinates.Count;i++)
            {
                if (!transitpointCoordinates[i].HasValidCoordinates)
                {
                    trimIndex = i;
                }
                else
                {
                    if(trimIndex != -1)
                    {
                        enableTrimming = true;
                        break;
                    }
                }
            }

            result.InsertRange(1, transitpointCoordinates);

            if (enableTrimming)
            {
                trimIndex++;
                result.RemoveRange(trimIndex, result.Count - trimIndex);
            }

            return result;
        }

        public bool HasRoute()
        {
            var startNode = nodes.Find(new NodeViewModel { Title = "S" });
            var goalNode = nodes.Find(new NodeViewModel { Title = "G" });
            var startNodeHasCoordinates = startNode.Value.HasValidCoordinates;
            var goalNodeHasCoordinates = goalNode.Value.HasValidCoordinates;

            if(!(startNodeHasCoordinates && goalNodeHasCoordinates))
            {
                return false;
            }
            else
            {
                return CanEveryTransitNodeBeConnected();
            }
        }

        private bool CanEveryTransitNodeBeConnected()
        {
            var transitpointCoordinates = GetTransitPointCoordinates(false);
            var trimIndex = -1;
            bool enableTrimming = false;

            for (int i = 1; i < transitpointCoordinates.Count; i++)
            {
                if (!transitpointCoordinates[i].HasValidCoordinates)
                {
                    trimIndex = i;
                }
                else
                {
                    if (trimIndex != -1)
                    {
                        enableTrimming = true;
                        break;
                    }
                }
            }

            return !enableTrimming;
        }

        private List<GpsCoordinatesViewModel> GetCoordinatesByCategory(NodeViewModel firstNodeToLookFor, NodeCategory category, bool removeEmptyEntries)
        {
            var result = new List<GpsCoordinatesViewModel>();
            var currentLinkedListNode = nodes.Find(firstNodeToLookFor);
            while (currentLinkedListNode.Value.Category == category)
            {
                if(removeEmptyEntries && !currentLinkedListNode.Value.HasValidCoordinates)
                {
                    currentLinkedListNode = currentLinkedListNode.Next();
                    continue;
                }

                var latitude = currentLinkedListNode.Value.Latitude;
                var longitude = currentLinkedListNode.Value.Longitude;

                var gpsCoordinatesViewModel = new GpsCoordinatesViewModel
                {
                    Latitude = latitude,
                    Longitude = longitude
                };

                result.Add(gpsCoordinatesViewModel);
                currentLinkedListNode = currentLinkedListNode.Next();
            }

            return result;
        }

        private void SetCurrentNodeThatCanBePlacedOnMap(string nodeTitle, Action actionWhenOneCircleEndedWithoutResult = null)
        {
            while (currentSelectedLinkedListNode.Value.HasValidCoordinates)
            {
                currentSelectedLinkedListNode = currentSelectedLinkedListNode.Next();
                if (currentSelectedLinkedListNode.Value.Title == nodeTitle)
                {
                    if(actionWhenOneCircleEndedWithoutResult != null)
                    {
                        actionWhenOneCircleEndedWithoutResult();
                    }

                    break;
                }
            }

            currentSelectedLinkedListNode.Value.Visible = true;
            CurrentSelectedNode = currentSelectedLinkedListNode.Value;
        }
    }
}