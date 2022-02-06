using System;
using System.Collections.Generic;
using RuntimeNodes.Utils;
using static RuntimeNodes.Utils.NodeInspector;

namespace RuntimeNodes
{
    public interface INode
    {
        ulong Id { get; }
        NodeInfo NodeInfo { get; }
    }

    public abstract class ActivableNode : INode
    {
        public ulong Id { get; internal set; }

        private NodeInfo _nodeInfo = null;
        public NodeInfo NodeInfo { get{ if (_nodeInfo != null)return _nodeInfo; _nodeInfo = NodeInspector.GetNodeInfo(this.GetType()); return _nodeInfo; } }

        private List<IValueAccessor> _listeningContextValues = new List<IValueAccessor>();

        public abstract void Step();

        internal void TryActivate()
        {
            foreach(var contextValue in _listeningContextValues)
            {
                if (!contextValue.HasValue)
                    return;
            }
            Activate();
        }

        public virtual void Init() { }

        protected virtual void Activate()
        {
            ExecutionContext.Current.ActivateNode(this);
        }

        protected IValueAccessor[] UpdateAndGetValueAccessors()
        {
            foreach(ContextValue val in _listeningContextValues)
                val?.Activator.RemoveListerner(this);

            _listeningContextValues.Clear();

            var valueAccessors = ExecutionContext.Current.GetValueAccessors(Id);
            for(int i = 0; i < NodeInfo.InputCount; ++i)
            {
                (valueAccessors[i] as ContextValue)?.Activator.AddListener(this);
                _listeningContextValues.Add(valueAccessors[i]);
            }
            return valueAccessors;
        }
    }

    public abstract class MultiStepNode : ActivableNode
    {
        public bool StillActivated { get; internal set; }

        protected void Deactivate()
        {
            StillActivated = false;
        }

        protected override void Activate()
        {
            if(!StillActivated)
            {
                StillActivated = true;
                base.Activate();
            }
        }
    }
}