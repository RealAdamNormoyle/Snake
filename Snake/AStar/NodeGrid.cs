using System.Numerics;

namespace Pathfinding.AStar {
    internal class NodeGrid {

        private readonly Vector2[] _adjacentPositions = new Vector2[] {
            new Vector2(1,1),
            new Vector2(1,0),
            new Vector2(1,-1),
            new Vector2(0,-1),
            new Vector2(-1,-1),
            new Vector2(-1,0),
            new Vector2(-1,1),
            new Vector2(0,1)
        };

        public readonly int Width;

        public readonly int Height;

        public readonly Node[] Nodes;

        public NodeGrid(int width, int height) {
            Width = width;
            Height = height;
            Nodes = new Node[Width * Height];

            for (int i = 0; i < Nodes.Length; i++) {
                Nodes[i].Position = new Vector2(i % Width, i / Width);
            }
        }

        public Vector2? GetClosestUnblockedPosition(Vector2 position) {
            if (!GetNode(position).Blocked) {
                return position;
            }

            int[] nodes = GetAdjacentNodes(position);
            for (int i = 0; i < nodes.Length; i++) {
                if (!Nodes[nodes[i]].Blocked) {
                    return Nodes[nodes[i]].Position;
                }
            }

            return null;
        }

        public int[] GetAdjacentNodes(Vector2 position) {
            List<int> nodes = new List<int>();

            for (int i = 0; i < _adjacentPositions.Length; i++) {
                int index = GetIndex(position + _adjacentPositions[i]);
                if (index < 0 || index >= Nodes.Length) {
                    continue;
                }

                nodes.Add(index);
            }

            return nodes.ToArray();
        }

        public void ToggleBlocked(Vector2 position) {
            int index = ((int)position.Y * Width) + (int)position.X;
            if (index < 0 || index >= Nodes.Length) {
                return;
            }

            Nodes[index].Blocked = !Nodes[index].Blocked;
        }

        public void ClearBlocked() {
            for (int i = 0; i < Nodes.Length; i++) {
                Nodes[i].Blocked = false;
            }
        }

        public void SetBlocked(Vector2 position, bool blocked) {
            int index = ((int)position.Y * Width) + (int)position.X;
            if (index < 0 || index >= Nodes.Length) {
                return;
            }

            Nodes[index].Blocked = blocked;
        }

        public Node GetNode(Vector2 position) {
            int index = GetIndex(position);
            if (index < 0 || index >= Nodes.Length) {
                return default;
            }

            return Nodes[index];
        }

        private int GetIndex(Vector2 position) {
            return ((int)position.Y * Width) + (int)position.X;
        }
    }

    internal struct Node {

        public bool Blocked;

        public Vector2 Position;

        public int Index;

        public float GScore;

        public float FScore;

        public int PreviousIndex;

    }
}
