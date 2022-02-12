using ImGuiNET;
using ImNodes.NET.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ImNodes.NET.ImNodes;

namespace ImNodes.NET
{
    public class ImNodes_Internal
    {
        public static ImNodesContext GImNodes;

        public enum ImNodesScope : int
        {
            None = 1,
            Editor = 1 << 1,
            Node = 1 << 2,
            Attribute = 1 << 3
        };

        public enum ImNodesAttributeType : int
        {
            None,
            Input,
            Output
        };

        public enum ImNodesUIState : int
        {
            None = 0,
            LinkStarted = 1 << 0,
            LinkDropped = 1 << 1,
            LinkCreated = 1 << 2
        };

        public enum ImNodesClickInteractionType : int
        {
            Node,
            Link,
            LinkCreation,
            Panning,
            BoxSelection,
            ImGuiItem,
            None
        };

        public enum ImNodesLinkCreationType
        {
            Standard,
            FromDetach
        };

        public interface ImObjectPoolItem
        {
            uint Id { get; }
            void Destroy();
        }
        public class ImObjectPool<T> where T : ImObjectPoolItem
        {
            public List<T> Pool = new List<T>();
            public List<bool> InUse = new List<bool>();
            public List<int> FreeList = new List<int>();
            public ImGuiStoragePtr IdMap;
        };


        public class ImNodeData : ImObjectPoolItem
        {
            public uint Id { get; private set; }
            public Vector2 Origin; // The node origin is in editor space
            public ImRect TitleBarContentRect;
            public ImRect Rect;
            public List<int> PinIndices;
            public bool Draggable;
            public ColorStyle_ ColorStyle;
            public LayoutStyle_ LayoutStyle;

            public class ColorStyle_
            {
                public int Background, BackgroundHovered, BackgroundSelected, Outline, Titlebar, TitlebarHovered,
                    TitlebarSelected;
            }

            public class LayoutStyle_
            {
                public float CornerRounding;
                public Vector2 Padding;
                public float BorderThickness;
            }


            public ImNodeData(uint node_id)
            {
                Id = node_id; Origin = new Vector2(100.0f, 100.0f); Rect = new ImRect(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f));
                ColorStyle = new ColorStyle_(); LayoutStyle = new LayoutStyle_(); PinIndices = new List<int>(); Draggable = true;
            }

            public virtual void Destroy() 
            { 
                Id = uint.MaxValue; 
            }
        };

        public class ImPinData : ImObjectPoolItem
        {
            public uint Id { get; private set; }
            public int ParentNodeIdx = 0;
            public ImRect AttributeRect;
            public ImNodesAttributeType Type = ImNodesAttributeType.None;
            public ImNodesPinShape Shape = ImNodesPinShape.ImNodesPinShape_CircleFilled;
            public Vector2 Pos; // screen-space coordinates
            public ImNodesAttributeFlags Flags = ImNodesAttributeFlags.ImNodesAttributeFlags_None;
            public ColorStyle_ ColorStyle;

            public struct ColorStyle_
            {
                public int Background, Hovered;
            }

            public ImPinData(uint pin_id)
            {
                Id = pin_id;
            }

            public void Destroy()
            {
                Id = uint.MaxValue;
            }
        }

        public class ImLinkData : ImObjectPoolItem
        {
            public uint Id { get; private set; }
            public int StartPinIdx = 0, EndPinIdx = 0;
            public ColorStyle_ ColorStyle;
            public struct ColorStyle_
            {
                int Base, Hovered, Selected;
            }

            public ImLinkData(uint link_id) { Id = link_id; }
            public void Destroy()
            {
                Id = uint.MaxValue;
            }
        };

        public class ImClickInteractionState
        {
            public ImNodesClickInteractionType Type = ImNodesClickInteractionType.Node;
            public LinkCreation_ LinkCreation;
            public struct LinkCreation_
            {
                int StartPinIdx;
                int? EndPinIdx;
                ImNodesLinkCreationType Type;
            }
            public BoxSelector_ BoxSelector;
            public struct BoxSelector_
            {
                ImRect Rect; // Coordinates in grid space
            }
        };

        public class ImNodesColElement
        {
            int Color;
            public ImNodesCol Item;

            ImNodesColElement(int c, ImNodesCol s) { Color = c; Item = s; }
        };

        public class ImNodesStyleVarElement
        {
            public ImNodesStyleVar Item;
            public float[] FloatValue = new float[2];

            public ImNodesStyleVarElement(ImNodesStyleVar variable, float value)
            {
                Item = variable;
                FloatValue[0] = value;
            }

            public ImNodesStyleVarElement(ImNodesStyleVar variable, Vector2 value)
            {
                Item = variable;
                FloatValue[0] = value.X;
                FloatValue[1] = value.Y;
            }
        };


        // [SECTION] global and editor context structs

        public class ImNodesEditorContext
        {
            public ImObjectPool<ImNodeData> Nodes = new ImObjectPool<ImNodeData>();
            public ImObjectPool<ImPinData> Pins = new ImObjectPool<ImPinData>();
            public ImObjectPool<ImLinkData> Links = new ImObjectPool<ImLinkData>();

            public List<int> NodeDepthOrder = new List<int>();

            // ui related fields
            public Vector2 Panning = new Vector2(0.0f, 0.0f);
            public Vector2 AutoPanningDelta;
            // Minimum and maximum extents of all content in grid space. Valid after final
            // ImNodes::EndNode() call.
            public ImRect GridContentBounds;

            public List<int> SelectedNodeIndices;
            public List<int> SelectedLinkIndices;

            public ImClickInteractionState ClickInteraction;

            // Mini-map state set by MiniMap()

            public bool MiniMapEnabled = false;
            public ImNodesMiniMapLocation MiniMapLocation;
            public float MiniMapSizeFraction = 0.0f;
            public ImNodesMiniMapNodeHoveringCallback MiniMapNodeHoveringCallback;
            public object MiniMapNodeHoveringCallbackUserData = null;

            // Mini-map state set during EndNodeEditor() call

            public ImRect MiniMapRectScreenSpace;
            public ImRect MiniMapContentScreenSpace;
            public float MiniMapScaling = 0.0f;
        };

        public unsafe class ImNodesContext
        {
            public ImNodesEditorContext DefaultEditorCtx = null;
            public ImNodesEditorContext EditorCtx = null;

            // Canvas draw list and helper state
            public ImDrawListPtr CanvasDrawList = null;
            public ImGuiStorage NodeIdxToSubmissionIdx;
            public List<int> NodeIdxSubmissionOrder;
            public List<int> NodeIndicesOverlappingWithMouse;
            public List<int> OccludedPinIndices;

            // Canvas extents
            public Vector2 CanvasOriginScreenSpace;
            public ImRect CanvasRectScreenSpace;

            // Debug helpers
            public ImNodesScope CurrentScope;

            // Configuration state
            public ImNodesIO Io;
            public ImNodesStyle Style;
            public List<ImNodesColElement> ColorModifierStack;
            public List<ImNodesStyleVarElement> StyleModifierStack;
            public ImGuiTextBuffer TextBuffer;

            public int CurrentAttributeFlags;
            public List<int> AttributeFlagStack;

            // UI element state
            public int CurrentNodeIdx;
            public int CurrentPinIdx;
            public int CurrentAttributeId;

            public int? HoveredNodeIdx;
            public int? HoveredLinkIdx;
            public int? HoveredPinIdx;

            public int? DeletedLinkIdx;
            public int? SnapLinkIdx;

            // Event helper state
            // TODO: this should be a part of a state machine, and not a member of the global struct.
            // Unclear what parts of the code this relates to.
            public int ImNodesUIState;

            public int ActiveAttributeId;
            public bool ActiveAttribute;

            // ImGui::IO cache

            public Vector2 MousePos;

            public bool LeftMouseClicked;
            public bool LeftMouseReleased;
            public bool AltMouseClicked;
            public bool LeftMouseDragging;
            public bool AltMouseDragging;
            public float AltMouseScrollDelta;
        };

        public static ImNodesEditorContext EditorContextGet()
        {
            // No editor context was set! Did you forget to call ImNodes::CreateContext()?
            Debug.Assert(GImNodes?.EditorCtx != null);
            return GImNodes.EditorCtx;
        }

        // [SECTION] ObjectPool implementation

        public static int ObjectPoolFind<T>(ImObjectPool<T> objects, uint id) where T : ImObjectPoolItem
        {
            int index = objects.IdMap.GetInt(id, -1);
            return index;
        }

        public static void ObjectPoolUpdate<T>(ImObjectPool<T> objects) where T : ImObjectPoolItem
        {
            for (int i = 0; i < objects.InUse.Count; ++i)
            {
                uint id = objects.Pool[i].Id;

                if (!objects.InUse[i] && objects.IdMap.GetInt(id, -1) == i)
                {
                    objects.IdMap.SetInt(id, -1);
                    objects.FreeList.Add(i);
                    (objects.Pool[i]).Destroy();
                }
            }
        }

        public static void ObjectPoolUpdate(ImObjectPool<ImNodeData> nodes)
        {
            for (int i = 0; i < nodes.InUse.Count; ++i)
            {
                if (nodes.InUse[i])
                {
                    nodes.Pool[i].PinIndices.Clear();
                }
                else
                {
                    uint id = nodes.Pool[i].Id;

                    if (nodes.IdMap.GetInt(id, -1) == i)
                    {
                        // Remove node idx form depth stack the first time we detect that this idx slot is
                        // unused
                        List<int> depth_stack = EditorContextGet().NodeDepthOrder;
                        int elem = depth_stack.IndexOf(i);
                        depth_stack.RemoveAt(elem);

                        nodes.IdMap.SetInt(id, -1);
                        nodes.FreeList.Add(i);
                        (nodes.Pool[i]).Destroy();
                    }
                }
            }
        }

        public static void ObjectPoolReset<T>(ImObjectPool<T> objects) where T : ImObjectPoolItem
        {
            if (objects.InUse.Any())
            {
                objects.InUse.Clear();
            }
        }

        public static int ObjectPoolFindOrCreateIndex<T>(ImObjectPool<T> objects, uint id, Func<uint, T> constructor) where T : ImObjectPoolItem
        {
            int index = objects.IdMap.GetInt(id, -1);

            // Construct new object
            if (index == -1)
            {
                if (objects.FreeList.Count == 0)
                {
                    index = objects.Pool.Count;
                    Debug.Assert(objects.Pool.Count == objects.InUse.Count);
                    int new_size = objects.Pool.Count + 1;
                    objects.Pool.Add(constructor(id));
                    objects.InUse.Resize(new_size);
                }
                else
                {
                    index = objects.FreeList.Last();
                    objects.FreeList.RemoveAt(objects.FreeList.Count - 1);
                    objects.Pool[index] = constructor(id);
                }
                objects.IdMap.SetInt(id, index);
            }

            // Flag it as used
            objects.InUse[index] = true;

            return index;
        }

        public static int ObjectPoolFindOrCreateIndex(ImObjectPool<ImNodeData> nodes, uint node_id)
        {
            int node_idx = nodes.IdMap.GetInt(node_id, -1);

            // Construct new node
            if (node_idx == -1)
            {
                if (nodes.FreeList.Count == 0)
                {
                    node_idx = nodes.Pool.Count;
                    Debug.Assert(nodes.Pool.Count == nodes.InUse.Count);
                    int new_size = nodes.Pool.Count + 1;
                    nodes.Pool.Add(new ImNodeData(node_id));
                    nodes.InUse.Resize(new_size);
                }
                else
                {
                    node_idx = nodes.FreeList.Last();
                    nodes.FreeList.RemoveAt(nodes.FreeList.Count - 1);
                    nodes.Pool[node_idx] = new ImNodeData(node_id);
                }
                nodes.IdMap.SetInt(node_id, node_idx);

                ImNodesEditorContext editor = EditorContextGet();
                editor.NodeDepthOrder.Add(node_idx);
            }

            // Flag node as used
            nodes.InUse[node_idx] = true;

            return node_idx;
        }

        public static T ObjectPoolFindOrCreateObject<T>(ImObjectPool<T> objects, uint id, Func<uint, T> constructor) where T : ImObjectPoolItem
        {
            int index = ObjectPoolFindOrCreateIndex(objects, id, constructor);
            return objects.Pool[index];
        }
    }
}
