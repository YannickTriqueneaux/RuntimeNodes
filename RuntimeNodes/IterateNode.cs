using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes
{
    public class IterateNode : MultiStepNode
    {
        [Input]
        public IRuntimeNodeEnumerator<object> Enumerator { get; set; }

        [Input]
        public IValueAccessor Stop { get; set; }
        
        [Output]
        public NodeActivator Activator { get; internal set; }

        [Output]
        public object CurrentItem => Enumerator.Current;

        protected override void Activate()
        {
            Enumerator.Reset();
            base.Activate();
        }

        public override void Step()
        {
            if ((Stop.HasValue && (bool)Stop.Value) || !Enumerator.MoveNext())
            {
                this.Deactivate();
            }

            Activator.Activate();
        }
    }


    public class ConsumeNode : MultiStepNode
    {
        [Input]
        public IRuntimeNodeEnumerator<object> Enumerator { get; set; }

        [Input]
        public IValueAccessor Stop { get; set; }

        [Output]
        public NodeActivator Activator { get; internal set; }

        [Output]
        public object CurrentItem => Enumerator.Current;

        protected override void Activate()
        {
            Enumerator.Reset();
            base.Activate();
        }

        public override void Step()
        {
            if ((Stop.HasValue && (bool)Stop.Value) || !Enumerator.MoveNext())
            {
                this.Deactivate();
            }

            Activator.Activate();
        }
    }
}
