using ImGuiNET;
using imnodesNET;
using RuntimeNodes.ImGUI.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using static RuntimeNodes.ImGUI.Utils.UnsafeUtils.Arrays;
using ImNodes = imnodesNET.imnodes;

namespace RuntimeNodes.ImGUI.Sample.example
{
    class Color_Node_Editor : INode_Editor
    {
        enum NodeType
        {
            add,
            multiply,
            output,
            sine,
            time,
            value
        };

        class Node
        {
            public NodeType type;
            public float value;

            public Node(NodeType t)
            {
                type = t;
                value = 0.0f;
            }

            public Node(NodeType t, float v)
            {
                type = t;
                value = v;
            }
        };

        static bool emulate_three_button_mouse = false;

        static float clamp(float a, float min, float max)
        {
            return Math.Min(Math.Max(a, min), max);
        }

        static uint evaluate(Graph<Node> graph, int root_node)
        {
            Stack<int> postorder = new Stack<int>();
            Graph<Node>.dfs_traverse(
                graph, root_node, (int node_id) => { postorder.Push(node_id); });

            Stack<float> value_stack = new Stack<float>();
            while (postorder.Count > 0)
            {
                int id = postorder.Pop();
                Node node = graph.node(id);

                switch (node.type)
                {
                    case NodeType.add:
                    {
                        float rhs = value_stack.Pop();
                        float lhs = value_stack.Pop();
                        value_stack.Push(lhs + rhs);
                    }
                    break;
                    case NodeType.multiply:
                    {
                        float rhs = value_stack.Pop();
                        float lhs = value_stack.Pop();
                        value_stack.Push(rhs * lhs);
                    }
                    break;
                    case NodeType.sine:
                    {
                        float x = value_stack.Pop();
                        float res = (float)Math.Abs(Math.Sin((double)x));
                        value_stack.Push(res);
                    }
                    break;
                    case NodeType.time:
                    {
                        value_stack.Push(DateTime.Now.Second);
                    }
                    break;
                    case NodeType.value:
                    {
                        // If the edge does not have an edge connecting to another node, then just use the value
                        // at this node. It means the node's input pin has not been connected to anything and
                        // the value comes from the node's UI.
                        if (graph.num_edges_from_node(id) == 0)
                        {
                            value_stack.Push(node.value);
                        }
                    }
                    break;
                    default:
                        break;
                }
            }

            // The final output node isn't evaluated in the loop -- instead we just pop
            // the three values which should be in the stack.
            Debug.Assert(value_stack.Count == 3);
            byte b = (byte)(255.0f * clamp(value_stack.Pop(), 0.0f, 1.0f) + 0.5f);
            byte g = (byte)(255.0f * clamp(value_stack.Pop(), 0.0f, 1.0f) + 0.5f);
            byte r = (byte)(255.0f * clamp(value_stack.Pop(), 0.0f, 1.0f) + 0.5f);

            return ColorUtils.GetColorU32(r, g, b, 255);
        }

        class ColorNodeEditor
        {

            Graph<Node> graph_ = new Graph<Node>();
            List<UiNode> nodes_ = new List<UiNode>();
            int root_node_id_;
            AlignedArray<int> selected_links = new AlignedArray<int>();
            AlignedArray<int> selected_nodes = new AlignedArray<int>();

            public ColorNodeEditor()
            {
                root_node_id_ = -1;
            }

            DateTime m_startedTime = DateTime.Now;
            public unsafe void show()
            {
                var flags = ImGuiWindowFlags.MenuBar;

                bool opened = false;
                // The node editor window
                ImGui.Begin("color node editor", ref opened, flags);

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("Style"))
                    {
                        if (ImGui.MenuItem("Classic"))
                        {
                            ImGui.StyleColorsClassic();
                            ImNodes.StyleColorsClassic();
                        }
                        if (ImGui.MenuItem("Dark"))
                        {
                            ImGui.StyleColorsDark();
                            ImNodes.StyleColorsDark();
                        }
                        if (ImGui.MenuItem("Light"))
                        {
                            ImGui.StyleColorsLight();
                            ImNodes.StyleColorsLight();
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }

                ImGui.TextUnformatted("Edit the color of the output color window using nodes.");
                ImGui.Columns(2);
                ImGui.TextUnformatted("A -- add node");
                ImGui.TextUnformatted("X -- delete selected node or link");
                ImGui.NextColumn();

                if (ImGui.Checkbox("emulate_three_button_mouse", ref emulate_three_button_mouse))
                {
                    ImGuiIOPtr io = ImGui.GetIO();
                    ImGuiIO* ioPtr = io.NativePtr;
                    byte* keyAlt = &ioPtr->KeyAlt;
                    ImNodes.GetIO()->emulate_three_button_mouse.modifier =
                        emulate_three_button_mouse ? keyAlt : null;
                }
                ImGui.Columns(1);

                ImNodes.BeginNodeEditor();

                // Handle new nodes
                // These are driven by the user, so we place this code before rendering the nodes
                {
                    bool open_popup = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) &&
                                            ImNodes.IsEditorHovered() &&
                                            ImGui.IsKeyReleased((int)Veldrid.Key.A);

                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8.0f, 8.0f));
                    if (!ImGui.IsAnyItemHovered() && open_popup)
                    {
                        ImGui.OpenPopup("add node");
                    }

                    if (ImGui.BeginPopup("add node"))
                    {
                        Vector2 click_pos = ImGui.GetMousePosOnOpeningCurrentPopup();

                        if (ImGui.MenuItem("add"))
                        {
                            Node op = new Node(NodeType.add);

                            UiNode ui_node = new UiNode();
                            ui_node.type = UiNodeType.add;
                            ui_node.add.lhs = graph_.insert_node(new Node(NodeType.value, 0.0f));
                            ui_node.add.rhs = graph_.insert_node(new Node(NodeType.value, 0.0f));
                            ui_node.id = graph_.insert_node(op);

                            graph_.insert_edge(ui_node.id, ui_node.add.lhs);
                            graph_.insert_edge(ui_node.id, ui_node.add.rhs);

                            nodes_.Add(ui_node);
                            ImNodes.SetNodeScreenSpacePos(ui_node.id, click_pos);
                        }

                        if (ImGui.MenuItem("multiply"))
                        {
                            Node op = new Node(NodeType.multiply);

                            UiNode ui_node = new UiNode();
                            ui_node.type = UiNodeType.multiply;
                            ui_node.multiply.lhs = graph_.insert_node(new Node(NodeType.value, 0.0f));
                            ui_node.multiply.rhs = graph_.insert_node(new Node(NodeType.value, 0.0f));
                            ui_node.id = graph_.insert_node(op);

                            graph_.insert_edge(ui_node.id, ui_node.multiply.lhs);
                            graph_.insert_edge(ui_node.id, ui_node.multiply.rhs);

                            nodes_.Add(ui_node);
                            ImNodes.SetNodeScreenSpacePos(ui_node.id, click_pos);
                        }

                        if (ImGui.MenuItem("output") && root_node_id_ == -1)
                        {
                            Node output = new Node(NodeType.output);

                            UiNode ui_node = new UiNode();
                            ui_node.type = UiNodeType.output;
                            ui_node.output.r = graph_.insert_node(new Node(NodeType.value, 0.0f));
                            ui_node.output.g = graph_.insert_node(new Node(NodeType.value, 0.0f));
                            ui_node.output.b = graph_.insert_node(new Node(NodeType.value, 0.0f));
                            ui_node.id = graph_.insert_node(output);

                            graph_.insert_edge(ui_node.id, ui_node.output.r);
                            graph_.insert_edge(ui_node.id, ui_node.output.g);
                            graph_.insert_edge(ui_node.id, ui_node.output.b);

                            nodes_.Add(ui_node);
                            ImNodes.SetNodeScreenSpacePos(ui_node.id, click_pos);
                            root_node_id_ = ui_node.id;
                        }

                        if (ImGui.MenuItem("sine"))
                        {
                            Node value = new Node(NodeType.value, 0.0f);
                            Node op = new Node(NodeType.sine);

                            UiNode ui_node = new UiNode();
                            ui_node.type = UiNodeType.sine;
                            ui_node.sine.input = graph_.insert_node(value);
                            ui_node.id = graph_.insert_node(op);

                            graph_.insert_edge(ui_node.id, ui_node.sine.input);

                            nodes_.Add(ui_node);
                            ImNodes.SetNodeScreenSpacePos(ui_node.id, click_pos);
                        }

                        if (ImGui.MenuItem("time"))
                        {
                            UiNode ui_node = new UiNode();
                            ui_node.type = UiNodeType.time;
                            ui_node.id = graph_.insert_node(new Node(NodeType.time));

                            nodes_.Add(ui_node);
                            ImNodes.SetNodeScreenSpacePos(ui_node.id, click_pos);
                        }

                        ImGui.EndPopup();
                    }
                    ImGui.PopStyleVar();
                }

                foreach (UiNode node in nodes_)
                {
                    switch (node.type)
                    {
                        case UiNodeType.add:
                        {
                            float node_width = 100.0f;
                            ImNodes.BeginNode(node.id);

                            ImNodes.BeginNodeTitleBar();
                            ImGui.TextUnformatted("add");
                            ImNodes.EndNodeTitleBar();
                            {
                                ImNodes.BeginInputAttribute(node.add.lhs);
                                float label_width = ImGui.CalcTextSize("left").X;
                                ImGui.TextUnformatted("left");
                                if (graph_.num_edges_from_node(node.add.lhs) == 0)
                                {
                                    ImGui.SameLine();
                                    ImGui.PushItemWidth(node_width - label_width);
                                    ImGui.DragFloat("##hidelabel", ref graph_.node(node.add.lhs).value, 0.01f);
                                    ImGui.PopItemWidth();
                                }
                                ImNodes.EndInputAttribute();
                            }

                            {
                                ImNodes.BeginInputAttribute(node.add.rhs);
                                float label_width = ImGui.CalcTextSize("right").X;
                                ImGui.TextUnformatted("right");
                                if (graph_.num_edges_from_node(node.add.rhs) == 0)
                                {
                                    ImGui.SameLine();
                                    ImGui.PushItemWidth(node_width - label_width);
                                    ImGui.DragFloat("##hidelabel", ref graph_.node(node.add.rhs).value, 0.01f);
                                    ImGui.PopItemWidth();
                                }
                                ImNodes.EndInputAttribute();
                            }

                            ImGui.Spacing();

                            {
                                ImNodes.BeginOutputAttribute(node.id);
                                float label_width = ImGui.CalcTextSize("result").X;
                                ImGui.Indent(node_width - label_width);
                                ImGui.TextUnformatted("result");
                                ImNodes.EndOutputAttribute();
                            }

                            ImNodes.EndNode();
                        }
                        break;
                        case UiNodeType.multiply:
                        {
                            float node_width = 100.0f;
                            ImNodes.BeginNode(node.id);

                            ImNodes.BeginNodeTitleBar();
                            ImGui.TextUnformatted("multiply");
                            ImNodes.EndNodeTitleBar();

                            {
                                ImNodes.BeginInputAttribute(node.multiply.lhs);
                                float label_width = ImGui.CalcTextSize("left").X;
                                ImGui.TextUnformatted("left");
                                if (graph_.num_edges_from_node(node.multiply.lhs) == 0)
                                {
                                    ImGui.SameLine();
                                    ImGui.PushItemWidth(node_width - label_width);
                                    ImGui.DragFloat(
                                        "##hidelabel", ref graph_.node(node.multiply.lhs).value, 0.01f);
                                    ImGui.PopItemWidth();
                                }
                                ImNodes.EndInputAttribute();
                            }

                            {
                                ImNodes.BeginInputAttribute(node.multiply.rhs);
                                float label_width = ImGui.CalcTextSize("right").X;
                                ImGui.TextUnformatted("right");
                                if (graph_.num_edges_from_node(node.multiply.rhs) == 0)
                                {
                                    ImGui.SameLine();
                                    ImGui.PushItemWidth(node_width - label_width);
                                    ImGui.DragFloat(
                                        "##hidelabel", ref graph_.node(node.multiply.rhs).value, 0.01f);
                                    ImGui.PopItemWidth();
                                }
                                ImNodes.EndInputAttribute();
                            }

                            ImGui.Spacing();

                            {
                                ImNodes.BeginOutputAttribute(node.id);
                                float label_width = ImGui.CalcTextSize("result").X;
                                ImGui.Indent(node_width - label_width);
                                ImGui.TextUnformatted("result");
                                ImNodes.EndOutputAttribute();
                            }

                            ImNodes.EndNode();
                        }
                        break;
                        case UiNodeType.output:
                        {
                            float node_width = 100.0f;
                            ImNodes.PushColorStyle(ColorStyle.TitleBar, ColorUtils.GetColorU32(11, 109, 191, 255));
                            ImNodes.PushColorStyle(ColorStyle.TitleBarHovered, ColorUtils.GetColorU32(45, 126, 194, 255));
                            ImNodes.PushColorStyle(ColorStyle.TitleBarSelected, ColorUtils.GetColorU32(81, 148, 204, 255));
                            ImNodes.BeginNode(node.id);

                            ImNodes.BeginNodeTitleBar();
                            ImGui.TextUnformatted("output");
                            ImNodes.EndNodeTitleBar();

                            ImGui.Dummy(new Vector2(node_width, 0.0f));
                            {
                                ImNodes.BeginInputAttribute(node.output.r);
                                float label_width = ImGui.CalcTextSize("r").X;
                                ImGui.TextUnformatted("r");
                                if (graph_.num_edges_from_node(node.output.r) == 0)
                                {
                                    ImGui.SameLine();
                                    ImGui.PushItemWidth(node_width - label_width);
                                    ImGui.DragFloat(
                                        "##hidelabel", ref graph_.node(node.output.r).value, 0.01f, 0.0f, 1.0f);
                                    ImGui.PopItemWidth();
                                }
                                ImNodes.EndInputAttribute();
                            }

                            ImGui.Spacing();

                            {
                                ImNodes.BeginInputAttribute(node.output.g);
                                float label_width = ImGui.CalcTextSize("g").X;
                                ImGui.TextUnformatted("g");
                                if (graph_.num_edges_from_node(node.output.g) == 0)
                                {
                                    ImGui.SameLine();
                                    ImGui.PushItemWidth(node_width - label_width);
                                    ImGui.DragFloat(
                                        "##hidelabel", ref graph_.node(node.output.g).value, 0.01f, 0.0f, 1.0f);
                                    ImGui.PopItemWidth();
                                }
                                ImNodes.EndInputAttribute();
                            }

                            ImGui.Spacing();

                            {
                                ImNodes.BeginInputAttribute(node.output.b);
                                float label_width = ImGui.CalcTextSize("b").X;
                                ImGui.TextUnformatted("b");
                                if (graph_.num_edges_from_node(node.output.b) == 0)
                                {
                                    ImGui.SameLine();
                                    ImGui.PushItemWidth(node_width - label_width);
                                    ImGui.DragFloat(
                                        "##hidelabel", ref graph_.node(node.output.b).value, 0.01f, 0.0f, 1.0f);
                                    ImGui.PopItemWidth();
                                }
                                ImNodes.EndInputAttribute();
                            }
                            ImNodes.EndNode();
                            ImNodes.PopColorStyle();
                            ImNodes.PopColorStyle();
                            ImNodes.PopColorStyle();
                        }
                        break;
                        case UiNodeType.sine:
                        {
                            float node_width = 100.0f;
                            ImNodes.BeginNode(node.id);

                            ImNodes.BeginNodeTitleBar();
                            ImGui.TextUnformatted("sine");
                            ImNodes.EndNodeTitleBar();

                            {
                                ImNodes.BeginInputAttribute(node.sine.input);
                                float label_width = ImGui.CalcTextSize("number").X;
                                ImGui.TextUnformatted("number");
                                if (graph_.num_edges_from_node(node.sine.input) == 0)
                                {
                                    ImGui.SameLine();
                                    ImGui.PushItemWidth(node_width - label_width);
                                    ImGui.DragFloat(
                                        "##hidelabel", ref graph_.node(node.sine.input).value, 0.01f, 0.0f, 1.0f);
                                    ImGui.PopItemWidth();
                                }
                                ImNodes.EndInputAttribute();
                            }

                            ImGui.Spacing();

                            {
                                ImNodes.BeginOutputAttribute(node.id);
                                float label_width = ImGui.CalcTextSize("output").X;
                                ImGui.Indent(node_width - label_width);
                                ImGui.TextUnformatted("output");
                                ImNodes.EndInputAttribute();
                            }

                            ImNodes.EndNode();
                        }
                        break;
                        case UiNodeType.time:
                        {
                            ImNodes.BeginNode(node.id);

                            ImNodes.BeginNodeTitleBar();
                            ImGui.TextUnformatted("time");
                            ImNodes.EndNodeTitleBar();

                            ImNodes.BeginOutputAttribute(node.id);
                            ImGui.Text("output");
                            ImNodes.EndOutputAttribute();

                            ImNodes.EndNode();
                        }
                        break;
                    }
                }

                foreach (var edge in graph_.edges())
                {
                    // If edge doesn't start at value, then it's an internal edge, i.e.
                    // an edge which links a node's operation to its input. We don't
                    // want to render node internals with visible links.
                    if (graph_.node(edge.from).type != NodeType.value)
                        continue;

                    ImNodes.Link(edge.id, edge.from, edge.to);
                }

                //minimap ImNodes.MiniMap(0.2f, minimap_location_);
                ImNodes.EndNodeEditor();

                // Handle new links
                // These are driven by Imnodes, so we place the code after EndNodeEditor().

                {
                    int start_attr = 0, end_attr = 0;
                    if (ImNodes.IsLinkCreated(ref start_attr, ref end_attr))
                    {
                        NodeType start_type = graph_.node(start_attr).type;
                        NodeType end_type = graph_.node(end_attr).type;

                        bool valid_link = start_type != end_type;
                        if (valid_link)
                        {
                            // Ensure the edge is always directed from the value to
                            // whatever produces the value
                            if (start_type != NodeType.value)
                            {
                                int tmp = start_attr;
                                start_attr = end_attr;
                                end_attr = tmp;
                            }
                            graph_.insert_edge(start_attr, end_attr);
                        }
                    }
                }

                // Handle deleted links

                {
                    int link_id = 0;
                    if (ImNodes.IsLinkDestroyed(ref link_id))
                    {
                        graph_.erase_edge(link_id);
                    }
                }

                {
                    int num_selected = ImNodes.NumSelectedLinks();
                    if (num_selected > 0 && ImGui.IsKeyReleased((int)Veldrid.Key.X))
                    {
                        selected_links.Resize(num_selected);
                        fixed (int* data = &selected_links.Elements[0])
                        {
                            ImNodes.GetSelectedLinks(ref Unsafe.AsRef<int>(data));
                        }
                        selected_links.SetLength(num_selected);
                        foreach (int edge_id in selected_links)
                        {
                            graph_.erase_edge(edge_id);
                        }
                    }
                }

                {
                    int num_selected = ImNodes.NumSelectedNodes();
                    if (num_selected > 0 && ImGui.IsKeyReleased((int)Veldrid.Key.X))
                    {
                        selected_nodes.Resize(num_selected);

                        fixed (int* data = &selected_nodes.Elements[0])
                        {
                            ImNodes.GetSelectedNodes(ref Unsafe.AsRef<int>(data));
                        }
                        selected_nodes.SetLength(num_selected);
                        foreach (int node_id in selected_nodes)
                        {
                            graph_.erase_node(node_id);
                            UiNode uiNode = nodes_.Find(node => node.id == node_id);
                            // Erase any additional internal nodes
                            switch (uiNode.type)
                            {
                                case UiNodeType.add:
                                    graph_.erase_node(uiNode.add.lhs);
                                    graph_.erase_node(uiNode.add.rhs);
                                    break;
                                case UiNodeType.multiply:
                                    graph_.erase_node(uiNode.multiply.lhs);
                                    graph_.erase_node(uiNode.multiply.rhs);
                                    break;
                                case UiNodeType.output:
                                    graph_.erase_node(uiNode.output.r);
                                    graph_.erase_node(uiNode.output.g);
                                    graph_.erase_node(uiNode.output.b);
                                    root_node_id_ = -1;
                                    break;
                                case UiNodeType.sine:
                                    graph_.erase_node(uiNode.sine.input);
                                    break;
                                default:
                                    break;
                            }
                            nodes_.Remove(uiNode);
                        }
                    }
                }

                ImGui.End();

                // The color output window

                uint color = root_node_id_ != -1 ? evaluate(graph_, root_node_id_) : ColorUtils.GetColorU32(255, 20, 147, 255);
                ImGui.PushStyleColor(ImGuiCol.WindowBg, color);
                ImGui.Begin("output color");
                ImGui.End();
                ImGui.PopStyleColor();
            }

            enum UiNodeType
            {
                add,
                multiply,
                output,
                sine,
                time,
            };

            class UiNode
            {
                public UiNodeType type;
                // The identifying id of the ui node. For add, multiply, sine, and time
                // this is the "operation" node id. The additional input nodes are
                // stored in the structs.
                public int id;

                public struct Add
                {
                    public int lhs, rhs;
                };
                public Add add = new Add();

                public struct Multiply
                {
                    public int lhs, rhs;
                };
                public Multiply multiply = new Multiply();

                public struct Output
                {
                    public int r, g, b;
                };
                public Output output = new Output();

                public struct Input
                {
                    public int input;
                };
                public Input sine = new Input();
            };
        };

        private static ColorNodeEditor color_editor;
        public unsafe void NodeEditorInitialize()
        {
            color_editor = new ColorNodeEditor();
            IO* io = ImNodes.GetIO();
            ImGuiIO* imIo = ImGui.GetIO().NativePtr;
            io->link_detach_with_modifier_click.modifier = &imIo->KeyCtrl;
        }

        public void NodeEditorShow() { color_editor.show(); }

        public void NodeEditorShutdown() { color_editor = null; }
    }
} // namespace
