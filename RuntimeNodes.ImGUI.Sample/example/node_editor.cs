using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeNodes.ImGUI.Sample.example
{
    interface INode_Editor
    {
        void NodeEditorInitialize();
        void NodeEditorShow();
        void NodeEditorShutdown();
    }
}
