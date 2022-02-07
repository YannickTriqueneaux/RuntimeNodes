using ImGuiNET;
using imnodesNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Veldrid.Sdl2;
using ImNodes = imnodesNET.imnodes;

namespace RuntimeNodes.ImGUI.Sample.example
{
    class multi_editor
    {
        class Node
        {
            public int   id;
            public float value;

            public Node(int i, float v)
            {
                id = i;
                value = v;
            }
        };

        class Link
        {
            public int id;
            public int start_attr;
            public int end_attr;
        };

        class Editor
        {
            public IntPtr context = IntPtr.Zero;
            public List<Node> nodes = new List<Node>();
            public List<Link> links = new List<Link>();
            public int current_id = 0;
            
            public void show_editor(string editorName)
            {
                ImNodes.EditorContextSet(context);

                ImGui.Begin(editorName);
                ImGui.TextUnformatted("A -- add node");

                ImNodes.BeginNodeEditor();
                
                if (ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) &&
                    ImNodes.IsEditorHovered() && ImGui.IsKeyReleased((int)SDL_Scancode.SDL_SCANCODE_A))
                {
                    int node_id = ++current_id;
                    ImNodes.SetNodeScreenSpacePos(node_id, ImGui.GetMousePos());
                    nodes.Add(new Node(node_id, 0.0f));
                }

                foreach (Node node in nodes)
                {
                    ImNodes.BeginNode(node.id);

                    ImNodes.BeginNodeTitleBar();
                    ImGui.TextUnformatted("node");
                    ImNodes.EndNodeTitleBar();

                    ImNodes.BeginInputAttribute(node.id << 8);
                    ImGui.TextUnformatted("input");
                    ImNodes.EndInputAttribute();

                    ImNodes.BeginStaticAttribute(node.id << 16);
                    ImGui.PushItemWidth(120.0f);
                    ImGui.DragFloat("value", ref node.value, 0.01f);
                    ImGui.PopItemWidth();
                    ImNodes.EndStaticAttribute();

                    ImNodes.BeginOutputAttribute(node.id << 24);
                    float text_width = ImGui.CalcTextSize("output").X;
                    ImGui.Indent(120.0f + ImGui.CalcTextSize("value").X - text_width);
                    ImGui.TextUnformatted("output");
                    ImNodes.EndOutputAttribute();

                    ImNodes.EndNode();
                }

                foreach (Link link in links)
                {
                    ImNodes.Link(link.id, link.start_attr, link.end_attr);
                }

                ImNodes.EndNodeEditor();

                {
                    Link link = new Link();
                    if (ImNodes.IsLinkCreated(ref link.start_attr, ref link.end_attr))
                    {
                        link.id = ++current_id;
                        links.Add(link);
                    }
                }

                {
                    int link_id = 0;
                    if (ImNodes.IsLinkDestroyed(ref link_id))
                    {
                        Link lk = links.Find(link => link.id == link_id);
                        Debug.Assert(lk != null);
                        links.Remove(lk);
                    }
                }

                ImGui.End();
            }
        };
        
        Editor editor1 = new Editor();
        Editor editor2 = new Editor();

        unsafe void NodeEditorInitialize()
        {
            editor1.context = ImNodes.EditorContextCreate();
            editor2.context = ImNodes.EditorContextCreate();
            ImNodes.PushAttributeFlag(AttributeFlags.EnableLinkDetachWithDragClick);

            IO* io = ImNodes.GetIO();
            io->link_detach_with_modifier_click.modifier = &ImGui.GetIO().NativePtr->KeyCtrl;
        }

        void NodeEditorShow()
        {
            editor1.show_editor("editor1");
            editor2.show_editor("editor2");
        }

        void NodeEditorShutdown()
        {
            ImNodes.PopAttributeFlag();
            ImNodes.EditorContextFree(editor1.context);
            ImNodes.EditorContextFree(editor2.context);
        }
    }
} // namespace
