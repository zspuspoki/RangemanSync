using System;
using System.Collections.Generic;

namespace Rangeman.Views.Map
{
    internal class NodesViewModel : ViewModelBase
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
            var linkedListNodeToDelete = nodes.Find(userSelectedPinNodeForDeletion);
            linkedListNodeToDelete.Value.Longitude = 0;
            linkedListNodeToDelete.Value.Latitude = 0;
            linkedListNodeToDelete.Value.Visible = true;

            currentSelectedLinkedListNode = linkedListNodeToDelete;  // let's select it again, so it can be placed again on the map
            CurrentSelectedNode = currentSelectedLinkedListNode.Value;
        }

        /// <summary>
        /// This should be exectuted when node selection button is clicked in the bottom right corener
        /// </summary>
        public void ClickOnSelectNode()
        {
            var nodeTitle = currentSelectedLinkedListNode.Value.Title;

            do
            {
                var oldNode = currentSelectedLinkedListNode;
                currentSelectedLinkedListNode = currentSelectedLinkedListNode.Next();
                if(currentSelectedLinkedListNode == null)
                {
                    currentSelectedLinkedListNode = oldNode;
                    break;
                }
            } while ((currentSelectedLinkedListNode.Value.Longitude > 0 &&
                    currentSelectedLinkedListNode.Value.Longitude > 0) || 
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

        public IEnumerable<GpsCoordinatesViewModel> GetStartEndCoordinates()
        {
            var result = new List<GpsCoordinatesViewModel>();
            result.AddRange(GetCoordinatesByCategory(new NodeViewModel { Title = "S" }, NodeCategory.StartEnd));
            result.AddRange(GetCoordinatesByCategory(new NodeViewModel { Title = "G" }, NodeCategory.StartEnd));

            return result;
        }

        public List<GpsCoordinatesViewModel> GetTransitPointCoordinates()
        {
            return GetCoordinatesByCategory(new NodeViewModel { Title = "1" }, NodeCategory.Transit);
        }

        public List<GpsCoordinatesViewModel> GetOrderedCoordinatesFromStartToGoal()
        {
            var result = new List<GpsCoordinatesViewModel>();
            result.AddRange(GetCoordinatesByCategory(new NodeViewModel { Title = "S" }, NodeCategory.StartEnd));
            result.AddRange(GetCoordinatesByCategory(new NodeViewModel { Title = "1" }, NodeCategory.Transit));
            result.AddRange(GetCoordinatesByCategory(new NodeViewModel { Title = "G" }, NodeCategory.StartEnd));
            return result;
        }

        public bool HasRoute()
        {
            var startNode = nodes.Find(new NodeViewModel { Title = "S" });
            var goalNode = nodes.Find(new NodeViewModel { Title = "G" });
            var startNodeHasCoordinates = startNode.Value.Longitude > 0 && startNode.Value.Latitude > 0;
            var goalNodeHasCoordinates = goalNode.Value.Longitude > 0 && goalNode.Value.Latitude > 0;

            return startNodeHasCoordinates && goalNodeHasCoordinates;
        }

        private List<GpsCoordinatesViewModel> GetCoordinatesByCategory(NodeViewModel firstNodeToLookFor, NodeCategory category)
        {
            var result = new List<GpsCoordinatesViewModel>();
            var currentLinkedListNode = nodes.Find(firstNodeToLookFor);
            while (currentLinkedListNode.Value.Category == category)
            {
                var gpsCoordinatesViewModel = new GpsCoordinatesViewModel
                {
                    Latitude = currentLinkedListNode.Value.Latitude,
                    Longitude = currentLinkedListNode.Value.Longitude
                };

                result.Add(gpsCoordinatesViewModel);
                currentLinkedListNode = currentLinkedListNode.Next();
            }

            return result;
        }

        private void SetCurrentNodeThatCanBePlacedOnMap(string nodeTitle, Action actionWhenOneCircleEndedWithoutResult = null)
        {
            while (currentSelectedLinkedListNode.Value.Longitude > 0 &&
                    currentSelectedLinkedListNode.Value.Longitude > 0)
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