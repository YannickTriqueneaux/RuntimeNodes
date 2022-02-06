using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes
{
    public class NodeActivator
    {
        private List<ActivableNode> _listeners = new List<ActivableNode>();
        private HashSet<ulong> _listenerIds = new HashSet<ulong>();
        
        internal void Activate()
        {
            foreach(var listener in _listeners)
            {
                listener.TryActivate();
            }
        }

        public void AddListener(ActivableNode listener)
        {
            if (_listenerIds.Contains(listener.Id))
                throw new InvalidOperationException($"Listener ID {listener.Id} already registered to NodeActivator");
            _listeners.Add(listener);
            _listenerIds.Add(listener.Id);
        }

        public void RemoveListerner(ActivableNode listener)
        {
            if(!_listenerIds.Contains(listener.Id))
                throw new InvalidOperationException($"Listener ID {listener.Id} wasn't registered to NodeActivator");
            _listeners.Remove(listener);
            _listenerIds.Remove(listener.Id);
        }
    }
}
