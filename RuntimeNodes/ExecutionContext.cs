using RuntimeNodes.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes
{
    public class ExecutionContext
    {
        [ThreadStatic]
        private static ExecutionContext _current;
        public static ExecutionContext Current => _current;

        public void Select() { _current = this; }
        public void Unselect() { _current = null; }

               
        /// <summary>
        /// Execution
        /// </summary>
        int swapIndex = 0; 
        Queue<ActivableNode> PendingNodes = new Queue<ActivableNode>();
        List<MultiStepNode>[] MultiStepNodes = new List<MultiStepNode>[2];
        List<MultiStepNode> _currentMultiStepNodes = null;
        List<MultiStepNode> _nextMultiStepNodes = null;
        Queue<ActivableNode> _runOnce = new Queue<ActivableNode>();
        HashSet<ulong> _runOnceIds = new HashSet<ulong>();

        public NodeActivator RootActivator { get; private set; }

        public void Initialize(NodeActivator rootActivator)
        {
            MultiStepNodes[0] = new List<MultiStepNode>();
            MultiStepNodes[1] = new List<MultiStepNode>();
            RootActivator = rootActivator;
        }

        public void Step()
        {
            Select();

            _currentMultiStepNodes = MultiStepNodes[swapIndex % 2];
            _nextMultiStepNodes = MultiStepNodes[(swapIndex + 1) % 2];
            _nextMultiStepNodes.Clear();

            if (_currentMultiStepNodes.Count == 0)
            {
                while (_runOnce.Count > 0)
                {
                    ActivableNode runOnceNode = _runOnce.Dequeue();
                    _runOnceIds.Remove(runOnceNode.Id);
                    runOnceNode.Step();
                }
            }

            foreach (var node in _currentMultiStepNodes)
            {
                node.Step();

                if(node.StillActivated)
                {
                    _nextMultiStepNodes.Add(node);
                }

                while(_runOnce.Count > 0)
                {
                    ActivableNode runOnceNode = _runOnce.Dequeue();
                    _runOnceIds.Remove(runOnceNode.Id);
                    runOnceNode.Step();
                }
            }

            Unselect();
            ++swapIndex;
        }

        internal void AddRunOnce(ActivableNode node)
        {
            _runOnceIds.Add(node.Id);
        }

        internal void ActivateNode(ActivableNode node)
        {
            if(_runOnceIds.Contains(node.Id))
                _runOnce.Enqueue(node);

            if (node is MultiStepNode)
            {
                _nextMultiStepNodes.Add((MultiStepNode)node);
            }
        }

        
        /// <summary>
        /// Initialization
        /// </summary>
        Dictionary<ulong, IValueAccessor>   _contextValues      = new Dictionary<ulong, IValueAccessor>();
        Dictionary<ulong, ulong[]>          _nodeConnections    = new Dictionary<ulong, ulong[]>();
        Dictionary<ulong, ActivableNode>    _nodes              = new Dictionary<ulong, ActivableNode>();

        /// <summary>
        /// Initialization
        /// </summary>
        /// 
        internal IValueAccessor[] GetValueAccessors(ulong id)
        {
            if (_nodeConnections.TryGetValue(id, out ulong[] nodeConnections))
            {
                var values = new IValueAccessor[nodeConnections.Length];
                for (int i = 0; i < nodeConnections.Length; ++i)
                {
                    ulong valueId = nodeConnections[i];
                    if (valueId == 0)
                        continue;
                    if (_contextValues.TryGetValue(valueId, out IValueAccessor valAccessor))
                        values[i] = valAccessor;
                    else
                        throw new ArgumentException($"Value Accessor with ID '{valueId}' not registered");
                }
                return values;
            }
            throw new ArgumentException($"Node with ID '{id}' not registered");
        }

        public ResolveTypeResult ConnectNode(ulong nodeId, ulong valueId, uint nodeIoIndex)
        {
            ActivableNode node = _nodes[nodeId];
            if (!_nodeConnections.TryGetValue(nodeId, out ulong[] nodeConnections))
            {
                var nodeInfo = node.NodeInfo;
                nodeConnections = new ulong[nodeInfo.InputCount + nodeInfo.OutputCount];
                nodeConnections[nodeIoIndex] = valueId;
                _nodeConnections.Add(nodeId, nodeConnections);
            }
            else
                nodeConnections[nodeIoIndex] = valueId;
            node.Init();
            return (node as ITypesToResolveNode)?.ResolveTypes();
        }

        internal void AddContextValue(ContextValue contextValue)
        {
            _contextValues.Add(contextValue.Id, contextValue);
        }

        public void AddNode(ActivableNode node)
        {
            _nodes.Add(node.Id, node);
            if (node is MultiStepNode multiStepNode && multiStepNode.StillActivated)
                _nextMultiStepNodes.Add(multiStepNode);
        }

        public void RemoveNode(ActivableNode node)
        {
            _nodes.Remove(node.Id);
            if (node is MultiStepNode multiStepNode)
                multiStepNode.StillActivated = false;
        }
    }
}
