using RuntimeNodes.ImGUI.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes.ImGUI.Sample.example
{
    // a very simple directional graph
    class Graph<NodeType>
        where NodeType: class
    {
        int current_id_ = 0;
        // These contains map to the node id
        Dictionary<int, NodeType> nodes_ = new Dictionary<int, NodeType>();
        Dictionary<int, int> edges_from_node_ = new Dictionary<int, int>();
        Dictionary<int, List<int>> node_neighbors_ = new Dictionary<int, List<int>>();

        // This container maps to the edge id
        Dictionary<int, Edge> edges_ = new Dictionary<int, Edge>();

        public class Edge
        {
            public int id;
            public int from;
            public int to;

            public Edge() { }
            public Edge(int id, int f, int t)
            {
                this.id = id;
                from = f;
                to = t;
            }

            public int  opposite(int n) { return n == from ? to : from; }
            public bool contains(int n) { return n == from || n == to; }
        };

        // Element access
        public NodeType node(int node_id)
        {
            return nodes_.TryGetValue(node_id, out NodeType node) ? node : null;
        }

        public int[] neighbors(int node_id)
        {
            if(!node_neighbors_.TryGetValue(node_id, out List<int> nodes))
            {
                throw new KeyNotFoundException();
            }
            return nodes.ToArray();
        }

        public Edge[] edges()
        {
            return edges_.Values.ToArray();
        }

        // Capacity

        public int num_edges_from_node(int node_id)
        {
            if (!edges_from_node_.TryGetValue(node_id, out int num))
            {
                throw new KeyNotFoundException();
            }
            return num;
        }

        // Modifiers

        public int insert_node(NodeType node)
        {
            int id = current_id_++;
            Debug.Assert(!nodes_.ContainsKey(id));
            nodes_.Add(id, node);
            edges_from_node_.Add(id, 0);
            node_neighbors_.Add(id, new List<int>());
            return id;
        }

        public void erase_node(int id)
        {
            // first, remove any potential dangling edges
            {
                List<int> edges_to_erase = ListPool<int>.Take();

                foreach (Edge edge in edges_.Values)
                {
                    if (edge.contains(id))
                    {
                        edges_to_erase.Add(edge.id);
                    }
                }

                foreach (int edge_id in edges_to_erase)
                {
                    erase_edge(edge_id);
                }

                edges_to_erase.Clear();
                ListPool<int>.Release(ref edges_to_erase);
            }

            nodes_.Remove(id);
            edges_from_node_.Remove(id);
            node_neighbors_.Remove(id);
        }

        public int insert_edge(int from, int to)
        {
            int id = current_id_++;
            Debug.Assert(!edges_.ContainsKey(id));
            Debug.Assert(nodes_.ContainsKey(from));
            Debug.Assert(nodes_.ContainsKey(to));
            edges_.Add(id, new Edge(id, from, to));

            // update neighbor count
            Debug.Assert(edges_from_node_.ContainsKey(from));
            int edgesNum = edges_from_node_[from];
            edges_from_node_[from] = ++edgesNum;
            
            // update neighbor list
            Debug.Assert(node_neighbors_.ContainsKey(from));
            node_neighbors_[from].Add(to);

            return id;
        }

        public void erase_edge(int edge_id)
        {

            // This is a bit lazy, we find the pointer here, but we refind it when we erase the edge based
            // on id key.
            Debug.Assert(edges_.ContainsKey(edge_id));
            Edge edge = edges_[edge_id];

            // update neighbor count
            Debug.Assert(edges_from_node_.ContainsKey(edge.from));
            int edge_count = edges_from_node_[edge.from];
            Debug.Assert(edge_count > 0);
            edge_count -= 1;

            // update neighbor list
            {
                Debug.Assert(node_neighbors_.ContainsKey(edge.from));
                List<int> neighbors = node_neighbors_[edge.from];
                bool removed = neighbors.Remove(edge.to);
                Debug.Assert(removed);
            }

            edges_.Remove(edge_id);
        }
        
        public static void dfs_traverse(Graph<NodeType> graph, int start_node, Action<int> visitor)
        {
            Stack<int> stack = new Stack<int>();

            stack.Push(start_node);

            while (stack.Count > 0)
            {
                int current_node = stack.Pop();

                visitor(current_node);

                foreach (int neighbor in graph.neighbors(current_node))
                {
                    stack.Push(neighbor);
                }
            }
        }
    };
}
