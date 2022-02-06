using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes.Utils
{
    public static class NodeInspector
    {
        public class NodeInfo
        {
            public uint InputCount = 0;
            public uint OutputCount = 0;
            public Type NodeType;
        }

        static Dictionary<Type, NodeInfo> _nodeInfos = new Dictionary<Type, NodeInfo>();

        public static NodeInfo GetNodeInfo(Type nodeType)
        {
            if(_nodeInfos.TryGetValue(nodeType, out NodeInfo nodeInfo))
                return nodeInfo;

            nodeInfo = InspectNode(nodeType);
            _nodeInfos.Add(nodeType, nodeInfo);
            return nodeInfo;
        }

        private static NodeInfo InspectNode(Type nodeType)
        {
            NodeInfo nodeInfo = new NodeInfo { NodeType = nodeType };
            foreach (var prop in nodeType.GetProperties())
            {
                OutputAttribute outAtt = prop.GetCustomAttributes(false).OfType<OutputAttribute>().FirstOrDefault();
                InputAttribute inAtt = prop.GetCustomAttributes(false).OfType<InputAttribute>().FirstOrDefault();
                if (outAtt != null)
                    ++nodeInfo.OutputCount;
                if (inAtt != null)
                    ++nodeInfo.InputCount;
            }
            return nodeInfo;
        }
    }
}
