using Mss.WorkForce.Code.Models.ModelSimulation;

namespace Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.DataOrganization
{
    public class Dijkstra
    {
        #region Variables
        private DataSimulatorTablaRequest data;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the shortest paths between all the warehouse routes
        /// </summary>
        /// <param name="data">Database data</param>
        public Dijkstra(DataSimulatorTablaRequest data) 
        {
            this.data = data;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates the shortest paths between all the warehouse routes
        /// </summary>
        /// <param name="warehouseId">Id of the warehouse to calculate the routes for</param>
        /// <returns>List of the created routes</returns>
        public List<Core.Layout.Route> CreateRoutes(Guid warehouseId) 
        {
            List<Core.Layout.Route> list = new List<Core.Layout.Route>();

            var graph = new Graph(this.data.Route, this.data.Area);
            var allShortestPaths = graph.CalculateAllPairsShortestPaths();

            foreach (var fromArea in allShortestPaths.Keys)
            {
                foreach (var toArea in allShortestPaths[fromArea].Keys)
                {
                    list.Add(new Core.Layout.Route(fromArea, toArea, allShortestPaths[fromArea][toArea]));
                }
            }

            return list;
        }
        #endregion
    }

    internal class Graph
    {
        #region Variables
        private Dictionary<Guid, List<(Guid, int)>> adjList = new();
        #endregion

        #region Constructor
        internal Graph(IEnumerable<Models.Models.Route> routes, IEnumerable<Models.Models.Area> areas)
        {
            foreach (var a in areas)
            {
                adjList[a.Id] = new List<(Guid, int)>();

                var route = routes.Where(x => x.DepartureAreaId == a.Id);
                foreach (var r in route)
                    adjList[a.Id].Add((r.ArrivalAreaId, r.TimeMin.GetValueOrDefault(0)));

                var bidirectionalRoute = routes.Where(x => x.ArrivalAreaId == a.Id && x.Bidirectional);
                foreach (var r in bidirectionalRoute)
                    adjList[a.Id].Add((r.DepartureAreaId, r.TimeMin.GetValueOrDefault(0)));
            }
        }
        #endregion

        #region Methods
        #region Private
        /// <summary>
        /// Calculates the shortest path from a starting node to all other nodes in a weighted, directed graph represented by an adjacency list (adjList), using Dijkstra's algorithm.
        /// </summary>
        /// <param name="start">The starting node</param>
        /// <returns>A dictionary mapping each node to its shortest distance from the start node.</returns>
        private Dictionary<Guid, int> Dijkstra(Guid start)
        {
            var distances = adjList.Keys.ToDictionary(node => node, node => int.MaxValue);
            distances[start] = 0;

            var priorityQueue = new SortedSet<(int distance, Guid node)>(Comparer<(int, Guid)>.Create((x, y) =>
                x.Item1 == y.Item1 ? x.Item2.CompareTo(y.Item2) : x.Item1.CompareTo(y.Item1)));
            priorityQueue.Add((0, start));

            while (priorityQueue.Any())
            {
                var (currentDistance, currentNode) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                if (currentDistance > distances[currentNode]) continue;

                foreach (var (neighbor, time) in adjList[currentNode])
                {
                    int newDist = currentDistance + time;
                    if (newDist < distances[neighbor])
                    {
                        priorityQueue.Remove((distances[neighbor], neighbor));
                        distances[neighbor] = newDist;
                        priorityQueue.Add((newDist, neighbor));
                    }
                }
            }

            return distances;
        }
        #endregion

        #region Internal
        /// <summary>
        /// Calculates the shortest path between all pairs of nodes in a weighted, directed graph using Dijkstra's algorithm.
        /// </summary>
        /// <returns>A dictionary where each key is a node (Guid) and each value is a dictionary that maps every other node to its shortest distance from the key node.</returns>
        internal Dictionary<Guid, Dictionary<Guid, int>> CalculateAllPairsShortestPaths()
        {
            var allPairs = new Dictionary<Guid, Dictionary<Guid, int>>();

            foreach (var node in adjList.Keys)
            {
                allPairs[node] = Dijkstra(node);
            }

            return allPairs;
        }
        #endregion
        #endregion
    }
}
