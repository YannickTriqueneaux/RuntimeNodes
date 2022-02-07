using ImGuiNET;
using System.Numerics;
using ImNodes = imnodesNET.imnodes;

namespace RuntimeNodes.ImGUI.Sample.example
{
    class hello : INode_Editor
    {
        class HelloWorldNodeEditor
        {
            public void show()
            {
                ImGui.Begin("simple node editor");
        
                ImNodes.BeginNodeEditor();
                ImNodes.BeginNode(1);
        
                ImNodes.BeginNodeTitleBar();
                ImGui.TextUnformatted("simple node :)");
                ImNodes.EndNodeTitleBar();
        
                ImNodes.BeginInputAttribute(2);
                ImGui.Text("input");
                ImNodes.EndInputAttribute();
        
                ImNodes.BeginOutputAttribute(3);
                ImGui.Indent(40);
                ImGui.Text("output");
                ImNodes.EndOutputAttribute();
        
                ImNodes.EndNode();
                ImNodes.EndNodeEditor();
        
                ImGui.End();
            }
        };

        HelloWorldNodeEditor editor;

        public void NodeEditorInitialize()
        {
            editor = new HelloWorldNodeEditor();
            ImNodes.SetNodeGridSpacePos(1, new Vector2(200.0f, 200.0f));
        }
        
        public void NodeEditorShow()
        {
            editor.show();
        }
        
        public void NodeEditorShutdown()
        {
            editor = null;
        }
    }
} // namespace
