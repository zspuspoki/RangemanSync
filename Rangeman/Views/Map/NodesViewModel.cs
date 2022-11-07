﻿using System;
using System.Collections.Generic;

namespace Rangeman.Views.Map
{
    internal class NodesViewModel : ViewModelBase
    {
        private CircularLinkedList<NodeViewModel> nodes;

        private LinkedListNode<NodeViewModel> currentSelectedLinkedListNode = null;
        private NodeViewModel currentSelectedNode;

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
        }

        public void DeleteNode(NodeViewModel node)
        {
            var linkedListNodeToDelete = nodes.Find(node);
            linkedListNodeToDelete.Value.Longitude = 0;
            linkedListNodeToDelete.Value.Latitude = 0;
            linkedListNodeToDelete.Value.Visible = true;

            currentSelectedLinkedListNode = linkedListNodeToDelete;  // let's select it again, so it can be placed again on the map
        }

        /// <summary>
        /// This should be exectuted when node selection button is clicked in the bottom right corener
        /// </summary>
        public void ClickOnSelectNode()
        {
            var nodeTitle = currentSelectedLinkedListNode.Value.Title;
            SetCurrentNodeThatCanBePlacedOnMap(nodeTitle);
        }

        public void AddNodeToMap(double longitude, double latitude)
        {
            if (currentSelectedLinkedListNode != null)
            {
                var nodeTitle = currentSelectedLinkedListNode.Value.Title;
                currentSelectedLinkedListNode.Value.Longitude = longitude;
                currentSelectedLinkedListNode.Value.Latitude = latitude;
                currentSelectedLinkedListNode.Value.Visible = false;

                SetCurrentNodeThatCanBePlacedOnMap(nodeTitle, () => 
                {
                    var missingNodeToAddViewModel = new MissingNodeToAddViewModel();
                    currentSelectedLinkedListNode = new LinkedListNode<NodeViewModel>(missingNodeToAddViewModel);
                });
            }
        }

        public IEnumerable<GpsCoordinatesViewModel> GetStartEndCoordinates()
        {
            return GetCoordinatesByCategory(new NodeViewModel { Title = "S" }, NodeCategory.StartEnd);
        }

        public List<GpsCoordinatesViewModel> GetTransitPointCoordinates()
        {
            return GetCoordinatesByCategory(new NodeViewModel { Title = "1" }, NodeCategory.StartEnd);
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
                currentLinkedListNode = currentLinkedListNode.Next;
            }

            return result;
        }

        private void SetCurrentNodeThatCanBePlacedOnMap(string nodeTitle, Action actionWhenOneCircleEndedWithoutResult = null)
        {
            while (currentSelectedLinkedListNode.Value.Longitude > 0 &&
                    currentSelectedLinkedListNode.Value.Longitude > 0)
            {
                currentSelectedLinkedListNode = currentSelectedLinkedListNode.Next;
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