using System.Collections.Generic;

namespace Rangeman.Views.Map
{
    internal class NodesViewModel
    {
        private CircularLinkedList<NodeViewModel> nodes;

        private LinkedListNode<NodeViewModel> currentSelectedLinkedListNode = null;

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

            for(var i=1;i<=9;i++)
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

        public void AddNodeToMap(double longitude, double latitude)
        {
            if(currentSelectedLinkedListNode != null)
            {
                currentSelectedLinkedListNode.Value.Longitude = longitude;
                currentSelectedLinkedListNode.Value.Latitude = latitude;
                currentSelectedLinkedListNode.Value.Visible = false;
            }

            //TODO: Get the first element that doesn't have long, lat info
            currentSelectedLinkedListNode = currentSelectedLinkedListNode.Next;
            currentSelectedLinkedListNode.Value.Visible = true;
        }
    }
}