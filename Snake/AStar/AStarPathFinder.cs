using System.Numerics;

namespace Pathfinding.AStar {
    internal class AStarPathFinder {

        private static readonly Vector2[] _neighbourOffsets = {
            new Vector2(0,1),
            //new Vector2(1,1),
            new Vector2(1,0),
            //new Vector2(1,-1),
            new Vector2(0,-1),
            //new Vector2(-1,-1),
            new Vector2(-1,0),
            //new Vector2(-1,1)
        };

        public static void FindPath(Vector2 from, Vector2 to, NodeGrid grid, Action<Vector2[]> onComplete) {
            List<int> nodes = new List<int>();
            List<int> visited = new List<int>();
            List<int> neighbours = new List<int>();

            int startIndex = 0;
            for (int i = 0; i < grid.Nodes.Length; i++) {
                grid.Nodes[i].FScore = float.MaxValue;
                grid.Nodes[i].GScore = float.MaxValue;
                grid.Nodes[i].PreviousIndex = -1;
                grid.Nodes[i].Index = i;
                if (grid.Nodes[i].Position == from) {
                    startIndex = i;
                }
                nodes.Add(i);
            }

            float fscore = GetHeuristic(grid.Nodes[nodes[startIndex]], to);
            grid.Nodes[nodes[startIndex]].FScore = fscore;
            grid.Nodes[nodes[startIndex]].GScore = 0;

            bool done = false;
            while (!done) {
                if (nodes.Count == 0) {
                    done = true;
                    continue;
                }

                int currentNodeIndex = GetNextBestNode(nodes, grid.Nodes);
                ref Node currentNode = ref grid.Nodes[currentNodeIndex];
                if (currentNode.Position == to) {
                    done = true;
                    visited.Add(currentNodeIndex);
                    continue;
                }

                GetNeighbours(currentNode, grid.Nodes, neighbours);
                for (int i = 0; i < neighbours.Count; i++) {
                    if (visited.Contains(neighbours[i]) ||
                        grid.Nodes[neighbours[i]].Blocked) {
                        continue;
                    }

                    ref Node neighbourNode = ref grid.Nodes[neighbours[i]];
                    float newGScore = currentNode.GScore + 1;
                    neighbourNode.GScore = newGScore;
                    neighbourNode.FScore = newGScore + GetHeuristic(neighbourNode, to);
                    neighbourNode.PreviousIndex = currentNode.Index;
                }

                visited.Add(currentNodeIndex);
                nodes.Remove(currentNodeIndex);
            }

            List<Vector2> pathOutput = new List<Vector2>();
            int index = visited[visited.Count - 1];
            while (grid.Nodes[index].PreviousIndex >= 0) {
                pathOutput.Insert(0, grid.Nodes[index].Position);
                index = grid.Nodes[index].PreviousIndex;
            }

            pathOutput.Insert(0, from);
            onComplete.Invoke(pathOutput.ToArray());
        }

        private static void GetNeighbours(Node node, Node[] nodes, List<int> list) {
            list.Clear();
            for (int i = 0; i < _neighbourOffsets.Length; i++) {
                Vector2 pos = node.Position + _neighbourOffsets[i];
                for (int j = 0; j < nodes.Length; j++) {
                    if (nodes[j].Position == pos) {
                        list.Add(nodes[j].Index);
                    }
                }
            }
        }

        private static int GetNextBestNode(List<int> indexes, Node[] nodes) {
            float bestF = float.MaxValue;
            int bestN = 0;
            for (int i = 0; i < indexes.Count; i++) {
                if (nodes[indexes[i]].FScore < bestF) {
                    bestF = nodes[indexes[i]].FScore;
                    bestN = i;
                }
            }

            return indexes[bestN];
        }

        private static float GetHeuristic(Node node, Vector2 end) {
            return Vector2.DistanceSquared(node.Position, end);
        }
    }
}
