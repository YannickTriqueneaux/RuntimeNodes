using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ImNodes.NET.ImNodes_Internal;

namespace ImNodes.NET
{
    public partial class ImNodes
    {
        // the structure of this file:
        //
        // [SECTION] bezier curve helpers
        // [SECTION] draw list helper
        // [SECTION] ui state logic
        // [SECTION] render helpers
        // [SECTION] API implementation


        public struct CubicBezier
        {
            public Vector2 P0, P1, P2, P3;
            public int NumSegments;
        };

        public static Vector2 EvalCubicBezier(
                float t,
                Vector2 P0,
                Vector2 P1,
                Vector2 P2,
                Vector2 P3)
        {
            // B(t) = (1-t)**3 p0 + 3(1 - t)**2 t P1 + 3(1-t)t**2 P2 + t**3 P3

            float u = 1.0f - t;
            float b0 = u * u * u;
            float b1 = 3 * u * u * t;
            float b2 = 3 * u * t * t;
            float b3 = t * t * t;
            return new Vector2(
                b0 * P0.X + b1 * P1.X + b2 * P2.X + b3 * P3.X,
                b0 * P0.Y + b1 * P1.Y + b2 * P2.Y + b3 * P3.Y);
        }

        public static Vector2 ImLineClosestPoint(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 ap = p - a;
            Vector2 ab_dir = b - a;
            float ab_len = ImSqrt(ab_dir.X * ab_dir.X + ab_dir.Y * ab_dir.Y);
            ab_dir *= 1.0f / ab_len;
            float dot = ap.X * ab_dir.X + ap.Y * ab_dir.Y;
            if (dot< 0.0f)
                return a;
            if (dot > ab_len)
                return b;
            return a + ab_dir* dot;
        }
        
        public static float  ImLengthSqr(Vector2 lhs) { return lhs.X*lhs.X + lhs.Y*lhs.Y; }
        public static float ImSqrt(float lhs) { return (float)Math.Sqrt(lhs); }
        
        public static int ImMin(int lhs, int rhs) { return lhs < rhs ? lhs : rhs; }
        public static int ImMax(int lhs, int rhs) { return lhs >= rhs ? lhs : rhs; }
        public static float ImMin(float lhs, float rhs) { return lhs < rhs ? lhs : rhs; }
        public static float ImMax(float lhs, float rhs) { return lhs >= rhs ? lhs : rhs; }
        public static Vector2 ImMin(Vector2 lhs, Vector2 rhs) { return new Vector2(ImMin(lhs.X, rhs.X), ImMin(lhs.Y, rhs.Y)); }
        public static Vector2 ImMax(Vector2 lhs, Vector2 rhs) { return new Vector2(ImMax(lhs.X, rhs.X), ImMax(lhs.Y, rhs.Y)); }
        public static void ImSwap(ref Vector2 lhs, ref Vector2 rhs) { Vector2 tmp = rhs; rhs.X = lhs.X; rhs.Y = lhs.Y; lhs.X = tmp.X; lhs.Y = tmp.Y; }
        public static void ImSwap(ref float lhs, ref float rhs) { float tmp = rhs; rhs = lhs; lhs = tmp; }

        // Calculates the closest point along each Bezier curve segment.
        public static Vector2 GetClosestPointOnCubicBezier(int num_segments, Vector2 p, CubicBezier cb)
        {
            Debug.Assert(num_segments > 0);
            Vector2 p_last = cb.P0;
            Vector2 p_closest = new Vector2();
            float p_closest_dist = float.MaxValue;
            float t_step = 1.0f / (float)num_segments;
            for (int i = 1; i <= num_segments; ++i)
            {
                Vector2 p_current = EvalCubicBezier(t_step * i, cb.P0, cb.P1, cb.P2, cb.P3);
                Vector2 p_line = ImLineClosestPoint(p_last, p_current, p);
                float dist = ImLengthSqr(p - p_line);
                if (dist < p_closest_dist)
                {
                    p_closest = p_line;
                    p_closest_dist = dist;
                }
                p_last = p_current;
            }
            return p_closest;
        }

        public static float GetDistanceToCubicBezier(
                Vector2      pos,
                CubicBezier  cubic_bezier,
                int num_segments)
        {
            Vector2 point_on_curve = GetClosestPointOnCubicBezier(num_segments, pos, cubic_bezier);

            Vector2 to_curve = point_on_curve - pos;
            return ImSqrt(ImLengthSqr(to_curve));
        }

            public static ImRect GetContainingRectForCubicBezier(CubicBezier cb)
            {
                Vector2 min = new Vector2(ImMin(cb.P0.X, cb.P3.X), ImMin(cb.P0.Y, cb.P3.Y));
                Vector2 max = new Vector2(ImMax(cb.P0.X, cb.P3.X), ImMax(cb.P0.Y, cb.P3.Y));

                float hover_distance = GImNodes.Style.LinkHoverDistance;

                ImRect rect = new ImRect(min, max);
                rect.Add(cb.P1);
                rect.Add(cb.P2);
                rect.Expand(new Vector2(hover_distance, hover_distance));

                return rect;
            }

            internal static CubicBezier GetCubicBezier(
                Vector2 start,
                Vector2 end,
                ImNodesAttributeType start_type,
                float line_segments_per_length)
            {
                Debug.Assert(
                    (start_type == ImNodesAttributeType.Input) || (start_type == ImNodesAttributeType.Output));
                if (start_type == ImNodesAttributeType.Input)
                {
                    ImSwap(ref start, ref end);
                }

                float link_length = ImSqrt(ImLengthSqr(end - start));
                Vector2 offset = new Vector2(0.25f * link_length, 0.0f);
                CubicBezier cubic_bezier = new CubicBezier();
                cubic_bezier.P0 = start;
                cubic_bezier.P1 = start + offset;
                cubic_bezier.P2 = end - offset;
                cubic_bezier.P3 = end;
                cubic_bezier.NumSegments = ImMax((int)(link_length * line_segments_per_length), 1);
                return cubic_bezier;
            }

            public static float EvalImplicitLineEq(Vector2 p1, Vector2 p2, Vector2 p)
            {
                return (p2.Y - p1.Y) * p.X + (p1.X - p2.X) * p.Y + (p2.X * p1.Y - p1.X * p2.Y);
            }

            public static int Sign(float val) => (val > 0.0f ? 1 : 0) - (val < 0.0f ? 1 : 0);

            unsafe public static bool RectangleOverlapsLineSegment( ImRect rect, Vector2 p1, Vector2 p2)
            {
                // Trivial case: rectangle contains an endpoint
                if (rect.Contains(p1) || rect.Contains(p2))
                {
                    return true;
                }

                // Flip rectangle if necessary
                ImRect flip_rect = rect;

                if (flip_rect.Min.X > flip_rect.Max.X)
                {
                    ImSwap(ref flip_rect.Min.X, ref flip_rect.Max.X);
                }

                if (flip_rect.Min.Y > flip_rect.Max.Y)
                {
                    ImSwap(ref flip_rect.Min.Y, ref flip_rect.Max.Y);
                }

                // Trivial case: line segment lies to one particular side of rectangle
                if ((p1.X < flip_rect.Min.X && p2.X < flip_rect.Min.X) ||
                    (p1.X > flip_rect.Max.X && p2.X > flip_rect.Max.X) ||
                    (p1.Y < flip_rect.Min.Y && p2.Y < flip_rect.Min.Y) ||
                    (p1.Y > flip_rect.Max.Y && p2.Y > flip_rect.Max.Y))
                {
                    return false;
                }

                var corner_signs = stackalloc int[4]
                {
                    Sign(EvalImplicitLineEq(p1, p2, flip_rect.Min)),
                    Sign(EvalImplicitLineEq(p1, p2, new Vector2(flip_rect.Max.X, flip_rect.Min.Y))),
                    Sign(EvalImplicitLineEq(p1, p2, new Vector2(flip_rect.Min.X, flip_rect.Max.Y))),
                    Sign(EvalImplicitLineEq(p1, p2, flip_rect.Max))
                };

                int sum = 0;
                int sum_abs = 0;

                for (int i = 0; i < 4; ++i)
                {
                    sum += corner_signs[i];
                    sum_abs += Math.Abs(corner_signs[i]);
                }

                // At least one corner of rectangle lies on a different side of line segment
                return Math.Abs(sum) != sum_abs;
            }

            public static bool RectangleOverlapsBezier(ImRect rectangle, CubicBezier cubic_bezier)
            {
                Vector2 current =
                    EvalCubicBezier(0.0f, cubic_bezier.P0, cubic_bezier.P1, cubic_bezier.P2, cubic_bezier.P3);
                float dt = 1.0f / cubic_bezier.NumSegments;
                for (int s = 0; s < cubic_bezier.NumSegments; ++s)
                {
                    Vector2 next = EvalCubicBezier(
                        (float)((s + 1) * dt),
                        cubic_bezier.P0,
                        cubic_bezier.P1,
                        cubic_bezier.P2,
                        cubic_bezier.P3);
                    if (RectangleOverlapsLineSegment(rectangle, current, next))
                    {
                        return true;
                    }
                    current = next;
                }
                return false;
            }

            internal static bool RectangleOverlapsLink(
                ImRect              rectangle,
                Vector2              start,
                Vector2              end,
                ImNodesAttributeType start_type)
            {
                // First level: simple rejection test via rectangle overlap:

                ImRect lrect = new ImRect(start, end);
                if (lrect.Min.X > lrect.Max.X)
                {
                    ImSwap(ref lrect.Min.X, ref lrect.Max.X);
                }

                if (lrect.Min.Y > lrect.Max.Y)
                {
                    ImSwap(ref lrect.Min.Y, ref lrect.Max.Y);
                }

                if (rectangle.Overlaps(lrect))
                {
                    // First, check if either one or both endpoinds are trivially contained
                    // in the rectangle

                    if (rectangle.Contains(start) || rectangle.Contains(end))
                    {
                        return true;
                    }

                    // Second level of refinement: do a more expensive test against the
                    // link

                    CubicBezier cubic_bezier =
                        GetCubicBezier(start, end, start_type, GImNodes.Style.LinkLineSegmentsPerLength);
                    return RectangleOverlapsBezier(rectangle, cubic_bezier);
                }

                return false;
            }

            // [SECTION] coordinate space conversion helpers

            public static Vector2 ScreenSpaceToGridSpace(ImNodesEditorContext editor, Vector2 v)
            {
                return v - GImNodes.CanvasOriginScreenSpace - editor.Panning;
            }

            public static ImRect ScreenSpaceToGridSpace(ImNodesEditorContext editor, ImRect r)
            {
                return new ImRect(ScreenSpaceToGridSpace(editor, r.Min), ScreenSpaceToGridSpace(editor, r.Max));
            }

            public static Vector2 GridSpaceToScreenSpace(ImNodesEditorContext editor, Vector2 v)
            {
                return v + GImNodes.CanvasOriginScreenSpace + editor.Panning;
            }

            public static Vector2 GridSpaceToEditorSpace(ImNodesEditorContext editor, Vector2 v)
            {
                return v + editor.Panning;
            }

            public static Vector2 EditorSpaceToGridSpace(ImNodesEditorContext editor, Vector2 v)
            {
                return v - editor.Panning;
            }

            public static Vector2 EditorSpaceToScreenSpace(Vector2 v)
            {
                return GImNodes.CanvasOriginScreenSpace + v;
            }

            public static Vector2 MiniMapSpaceToGridSpace( ImNodesEditorContext editor, Vector2 v)
            {
                return (v - editor.MiniMapContentScreenSpace.Min) / editor.MiniMapScaling +
                       editor.GridContentBounds.Min;
            }

            public static Vector2 ScreenSpaceToMiniMapSpace( ImNodesEditorContext editor, Vector2 v)
            {
                return (ScreenSpaceToGridSpace(editor, v) - editor.GridContentBounds.Min) *
                           editor.MiniMapScaling +
                       editor.MiniMapContentScreenSpace.Min;
            }

            public static ImRect ScreenSpaceToMiniMapSpace(ImNodesEditorContext editor, ImRect r)
            {
                return new ImRect(
                    ScreenSpaceToMiniMapSpace(editor, r.Min), ScreenSpaceToMiniMapSpace(editor, r.Max));
            }

            // [SECTION] draw list helper

            unsafe void ImDrawListGrowChannels(ImDrawListPtr draw_list, int num_channels)
            {
                ImDrawListSplitter* splitter = &draw_list.NativePtr->_Splitter;
                ImDrawListSplitterPtr splitterPtr = new ImDrawListSplitterPtr(splitter);

                if (splitter->_Count == 1)
                {
                    splitterPtr.Split(draw_list, num_channels + 1);
                    return;
                }

                // NOTE: this logic has been lifted from ImDrawListSplitter.Split with slight modifications
                // to allow nested splits. The main modification is that we only create new ImDrawChannel
                // instances after splitter._Count, instead of over the whole splitter._Channels array like
                // the regular ImDrawListSplitter.Split method does.

                int old_channel_capacity = splitter->_Channels.Size;
                // NOTE: _Channels is not resized down, and therefore _Count <= _Channels.size()!
                const int old_channel_count = splitter._Count;
                const int requested_channel_count = old_channel_count + num_channels;
                if (old_channel_capacity < old_channel_count + num_channels)
                {
                    splitter._Channels.resize(requested_channel_count);
                }

                splitter._Count = requested_channel_count;

                for (int i = old_channel_count; i < requested_channel_count; ++i)
                {
                    ImDrawChannel & channel = splitter._Channels[i];

                    // If we're inside the old capacity region of the array, we need to reuse the existing
                    // memory of the command and index buffers.
                    if (i < old_channel_capacity)
                    {
                        channel._CmdBuffer.resize(0);
                        channel._IdxBuffer.resize(0);
                    }
                    // Else, we need to construct new draw channels.
                    else
                    {
                        IM_PLACEMENT_NEW(&channel) ImDrawChannel();
                    }

                    {
                        ImDrawCmd draw_cmd;
                        draw_cmd.ClipRect = draw_list->_ClipRectStack.back();
                        draw_cmd.TextureId = draw_list->_TextureIdStack.back();
                        channel._CmdBuffer.push_back(draw_cmd);
                    }
                }
            }

            void ImDrawListSplitterSwapChannels(
                ImDrawListSplitter& splitter,
                const int lhs_idx,
                const int rhs_idx)
            {
                if (lhs_idx == rhs_idx)
                {
                    return;
                }

                assert(lhs_idx >= 0 && lhs_idx < splitter._Count);
                assert(rhs_idx >= 0 && rhs_idx < splitter._Count);

                ImDrawChannel & lhs_channel = splitter._Channels[lhs_idx];
                ImDrawChannel & rhs_channel = splitter._Channels[rhs_idx];
                lhs_channel._CmdBuffer.swap(rhs_channel._CmdBuffer);
                lhs_channel._IdxBuffer.swap(rhs_channel._IdxBuffer);

                const int current_channel = splitter._Current;

                if (current_channel == lhs_idx)
                {
                    splitter._Current = rhs_idx;
                }
                else if (current_channel == rhs_idx)
                {
                    splitter._Current = lhs_idx;
                }
            }

            void DrawListSet(ImDrawList* window_draw_list)
            {
                GImNodes.CanvasDrawList = window_draw_list;
                GImNodes.NodeIdxToSubmissionIdx.Clear();
                GImNodes.NodeIdxSubmissionOrder.clear();
            }

            // The draw list channels are structured as follows. First we have our base channel, the canvas grid
            // on which we render the grid lines in BeginNodeEditor(). The base channel is the reason
            // draw_list_submission_idx_to_background_channel_idx offsets the index by one. Each BeginNode()
            // call appends two new draw channels, for the node background and foreground. The node foreground
            // is the channel into which the node's ImGui content is rendered. Finally, in EndNodeEditor() we
            // append one last draw channel for rendering the selection box and the incomplete link on top of
            // everything else.
            //
            // +----------+----------+----------+----------+----------+----------+
            // |          |          |          |          |          |          |
            // |canvas    |node      |node      |...       |...       |click     |
            // |grid      |background|foreground|          |          |interaction
            // |          |          |          |          |          |          |
            // +----------+----------+----------+----------+----------+----------+
            //            |                     |
            //            |   submission idx    |
            //            |                     |
            //            -----------------------

            void DrawListAddNode(const int node_idx)
            {
                GImNodes.NodeIdxToSubmissionIdx.SetInt(
                    static_cast<ImGuiID>(node_idx), GImNodes.NodeIdxSubmissionOrder.Size);
                GImNodes.NodeIdxSubmissionOrder.push_back(node_idx);
                ImDrawListGrowChannels(GImNodes.CanvasDrawList, 2);
            }

            void DrawListAppendClickInteractionChannel()
            {
                // NOTE: don't use this function outside of EndNodeEditor. Using this before all nodes have been
                // added will screw up the node draw order.
                ImDrawListGrowChannels(GImNodes.CanvasDrawList, 1);
            }

            int DrawListSubmissionIdxToBackgroundChannelIdx(const int submission_idx)
            {
                // NOTE: the first channel is the canvas background, i.e. the grid
                return 1 + 2 * submission_idx;
            }

            int DrawListSubmissionIdxToForegroundChannelIdx(const int submission_idx)
            {
                return DrawListSubmissionIdxToBackgroundChannelIdx(submission_idx) + 1;
            }

            void DrawListActivateClickInteractionChannel()
            {
                GImNodes.CanvasDrawList->_Splitter.SetCurrentChannel(
                    GImNodes.CanvasDrawList, GImNodes.CanvasDrawList->_Splitter._Count - 1);
            }

            void DrawListActivateCurrentNodeForeground()
            {
                const int foreground_channel_idx =
                    DrawListSubmissionIdxToForegroundChannelIdx(GImNodes.NodeIdxSubmissionOrder.Size - 1);
                GImNodes.CanvasDrawList->_Splitter.SetCurrentChannel(
                    GImNodes.CanvasDrawList, foreground_channel_idx);
            }

            void DrawListActivateNodeBackground(const int node_idx)
            {
                const int submission_idx =
                    GImNodes.NodeIdxToSubmissionIdx.GetInt(static_cast<ImGuiID>(node_idx), -1);
                // There is a discrepancy in the submitted node count and the rendered node count! Did you call
                // one of the following functions
                // * EditorContextMoveToNode
                // * SetNodeScreenSpacePos
                // * SetNodeGridSpacePos
                // * SetNodeDraggable
                // after the BeginNode/EndNode function calls?
                assert(submission_idx != -1);
                const int background_channel_idx = DrawListSubmissionIdxToBackgroundChannelIdx(submission_idx);
                GImNodes.CanvasDrawList->_Splitter.SetCurrentChannel(
                    GImNodes.CanvasDrawList, background_channel_idx);
            }

            void DrawListSwapSubmissionIndices(const int lhs_idx, const int rhs_idx)
            {
                assert(lhs_idx != rhs_idx);

                const int lhs_foreground_channel_idx = DrawListSubmissionIdxToForegroundChannelIdx(lhs_idx);
                const int lhs_background_channel_idx = DrawListSubmissionIdxToBackgroundChannelIdx(lhs_idx);
                const int rhs_foreground_channel_idx = DrawListSubmissionIdxToForegroundChannelIdx(rhs_idx);
                const int rhs_background_channel_idx = DrawListSubmissionIdxToBackgroundChannelIdx(rhs_idx);

                ImDrawListSplitterSwapChannels(
                    GImNodes.CanvasDrawList->_Splitter,
                    lhs_background_channel_idx,
                    rhs_background_channel_idx);
                ImDrawListSplitterSwapChannels(
                    GImNodes.CanvasDrawList->_Splitter,
                    lhs_foreground_channel_idx,
                    rhs_foreground_channel_idx);
            }

            void DrawListSortChannelsByDepth(const ImVector<int>& node_idx_depth_order)
            {
                if (GImNodes.NodeIdxToSubmissionIdx.Data.Size < 2)
                {
                    return;
                }

                assert(node_idx_depth_order.Size == GImNodes.NodeIdxSubmissionOrder.Size);

                int start_idx = node_idx_depth_order.Size - 1;

                while (node_idx_depth_order[start_idx] == GImNodes.NodeIdxSubmissionOrder[start_idx])
                {
                    if (--start_idx == 0)
                    {
                        // early out if submission order and depth order are the same
                        return;
                    }
                }

                // TODO: this is an O(N^2) algorithm. It might be worthwhile revisiting this to see if the time
                // complexity can be reduced.

                for (int depth_idx = start_idx; depth_idx > 0; --depth_idx)
                {
                    const int node_idx = node_idx_depth_order[depth_idx];

                    // Find the current index of the node_idx in the submission order array
                    int submission_idx = -1;
                    for (int i = 0; i < GImNodes.NodeIdxSubmissionOrder.Size; ++i)
                    {
                        if (GImNodes.NodeIdxSubmissionOrder[i] == node_idx)
                        {
                            submission_idx = i;
                            break;
                        }
                    }
                    assert(submission_idx >= 0);

                    if (submission_idx == depth_idx)
                    {
                        continue;
                    }

                    for (int j = submission_idx; j < depth_idx; ++j)
                    {
                        DrawListSwapSubmissionIndices(j, j + 1);
                        ImSwap(GImNodes.NodeIdxSubmissionOrder[j], GImNodes.NodeIdxSubmissionOrder[j + 1]);
                    }
                }
            }

            // [SECTION] ui state logic

            Vector2 GetScreenSpacePinCoordinates(
    const ImRect&              node_rect,
                const ImRect&              attribute_rect,
                const ImNodesAttributeType type)
            {
                assert(type == ImNodesAttributeType_Input || type == ImNodesAttributeType_Output);
                const float x = type == ImNodesAttributeType_Input
                                    ? (node_rect.Min.X - GImNodes.Style.PinOffset)
                                    : (node_rect.Max.X + GImNodes.Style.PinOffset);
                return Vector2(x, 0.5f * (attribute_rect.Min.Y + attribute_rect.Max.Y));
            }

            Vector2 GetScreenSpacePinCoordinates(const ImNodesEditorContext editor, const ImPinData& pin)
            {
                const ImRect&parent_node_rect = editor.Nodes.Pool[pin.ParentNodeIdx].Rect;
                return GetScreenSpacePinCoordinates(parent_node_rect, pin.AttributeRect, pin.Type);
            }

            bool MouseInCanvas()
            {
                // This flag should be true either when hovering or clicking something in the canvas.
                const bool is_window_hovered_or_focused = ImGui.IsWindowHovered() || ImGui.IsWindowFocused();

                return is_window_hovered_or_focused &&
                       GImNodes.CanvasRectScreenSpace.Contains(ImGui.GetMousePos());
            }

            void BeginNodeSelection(ImNodesEditorContext editor, const int node_idx)
            {
                // Don't start selecting a node if we are e.g. already creating and dragging
                // a new link! New link creation can happen when the mouse is clicked over
                // a node, but within the hover radius of a pin.
                if (editor.ClickInteraction.Type != ImNodesClickInteractionType.None)
                {
                    return;
                }

                editor.ClickInteraction.Type = ImNodesClickInteractionType.Node;
                // If the node is not already contained in the selection, then we want only
                // the interaction node to be selected, effective immediately.
                //
                // Otherwise, we want to allow for the possibility of multiple nodes to be
                // moved at once.
                if (!editor.SelectedNodeIndices.contains(node_idx))
                {
                    editor.SelectedNodeIndices.clear();
                    editor.SelectedLinkIndices.clear();
                    editor.SelectedNodeIndices.push_back(node_idx);

                    // Ensure that individually selected nodes get rendered on top
                    ImVector<int> & depth_stack = editor.NodeDepthOrder;
                    const int* const elem = depth_stack.find(node_idx);
                    assert(elem != depth_stack.end());
                    depth_stack.erase(elem);
                    depth_stack.push_back(node_idx);
                }
            }

            void BeginLinkSelection(ImNodesEditorContext editor, const int link_idx)
            {
                editor.ClickInteraction.Type = ImNodesClickInteractionType.Link;
                // When a link is selected, clear all other selections, and insert the link
                // as the sole selection.
                editor.SelectedNodeIndices.clear();
                editor.SelectedLinkIndices.clear();
                editor.SelectedLinkIndices.push_back(link_idx);
            }

            void BeginLinkDetach(ImNodesEditorContext editor, const int link_idx, const int detach_pin_idx)
            {
                const ImLinkData&link = editor.Links.Pool[link_idx];
                ImClickInteractionState & state = editor.ClickInteraction;
                state.Type = ImNodesClickInteractionType.LinkCreation;
                state.LinkCreation.EndPinIdx.Reset();
                state.LinkCreation.StartPinIdx =
                    detach_pin_idx == link.StartPinIdx ? link.EndPinIdx : link.StartPinIdx;
                GImNodes.DeletedLinkIdx = link_idx;
            }

            void BeginLinkCreation(ImNodesEditorContext editor, const int hovered_pin_idx)
            {
                editor.ClickInteraction.Type = ImNodesClickInteractionType.LinkCreation;
                editor.ClickInteraction.LinkCreation.StartPinIdx = hovered_pin_idx;
                editor.ClickInteraction.LinkCreation.EndPinIdx.Reset();
                editor.ClickInteraction.LinkCreation.Type = ImNodesLinkCreationType_Standard;
                GImNodes.ImNodesUIState |= ImNodesUIState_LinkStarted;
            }

            void BeginLinkInteraction(
                ImNodesEditorContext editor,
                const int link_idx,
                const ImOptionalIndex pin_idx = ImOptionalIndex())
            {
                // Check if we are clicking the link with the modifier pressed.
                // This will in a link detach via clicking.

                const bool modifier_pressed = GImNodes.Io.LinkDetachWithModifierClick.Modifier == NULL
                                                  ? false
                                                  : *GImNodes.Io.LinkDetachWithModifierClick.Modifier;

                if (modifier_pressed)
                {
                    const ImLinkData&link = editor.Links.Pool[link_idx];
                    const ImPinData&start_pin = editor.Pins.Pool[link.StartPinIdx];
                    const ImPinData&end_pin = editor.Pins.Pool[link.EndPinIdx];
                    Vector2mouse_pos = GImNodes.MousePos;
                    const float dist_to_start = ImLengthSqr(start_pin.Pos - mouse_pos);
                    const float dist_to_end = ImLengthSqr(end_pin.Pos - mouse_pos);
                    const int closest_pin_idx = dist_to_start < dist_to_end ? link.StartPinIdx : link.EndPinIdx;

                    editor.ClickInteraction.Type = ImNodesClickInteractionType.LinkCreation;
                    BeginLinkDetach(editor, link_idx, closest_pin_idx);
                    editor.ClickInteraction.LinkCreation.Type = ImNodesLinkCreationType_FromDetach;
                }
                else
                {
                    if (pin_idx.HasValue())
                    {
                        const int hovered_pin_flags = editor.Pins.Pool[pin_idx.Value()].Flags;

                        // Check the 'click and drag to detach' case.
                        if (hovered_pin_flags & ImNodesAttributeFlags_EnableLinkDetachWithDragClick)
                        {
                            BeginLinkDetach(editor, link_idx, pin_idx.Value());
                            editor.ClickInteraction.LinkCreation.Type = ImNodesLinkCreationType_FromDetach;
                        }
                        else
                        {
                            BeginLinkCreation(editor, pin_idx.Value());
                        }
                    }
                    else
                    {
                        BeginLinkSelection(editor, link_idx);
                    }
                }
            }

            void BeginCanvasInteraction(ImNodesEditorContext editor)
            {
                const bool any_ui_element_hovered =
                    GImNodes.HoveredNodeIdx.HasValue() || GImNodes.HoveredLinkIdx.HasValue() ||
                    GImNodes.HoveredPinIdx.HasValue() || ImGui.IsAnyItemHovered();

                const bool mouse_not_in_canvas = !MouseInCanvas();

                if (editor.ClickInteraction.Type != ImNodesClickInteractionType.None ||
                    any_ui_element_hovered || mouse_not_in_canvas)
                {
                    return;
                }

                const bool started_panning = GImNodes.AltMouseClicked;

                if (started_panning)
                {
                    editor.ClickInteraction.Type = ImNodesClickInteractionType.Panning;
                }
                else if (GImNodes.LeftMouseClicked)
                {
                    editor.ClickInteraction.Type = ImNodesClickInteractionType.BoxSelection;
                    editor.ClickInteraction.BoxSelector.Rect.Min =
                        ScreenSpaceToGridSpace(editor, GImNodes.MousePos);
                }
            }

            void BoxSelectorUpdateSelection(ImNodesEditorContext editor, ImRect box_rect)
            {
                // Invert box selector coordinates as needed

                if (box_rect.Min.X > box_rect.Max.X)
                {
                    ImSwap(ref box_rect.Min.X, ref box_rect.Max.X);
                }

                if (box_rect.Min.Y > box_rect.Max.Y)
                {
                    ImSwap(ref box_rect.Min.Y, ref box_rect.Max.Y);
                }

                // Update node selection

                editor.SelectedNodeIndices.clear();

                // Test for overlap against node rectangles

                for (int node_idx = 0; node_idx < editor.Nodes.Pool.size(); ++node_idx)
                {
                    if (editor.Nodes.InUse[node_idx])
                    {
                        ImNodeData & node = editor.Nodes.Pool[node_idx];
                        if (box_rect.Overlaps(node.Rect))
                        {
                            editor.SelectedNodeIndices.push_back(node_idx);
                        }
                    }
                }

                // Update link selection

                editor.SelectedLinkIndices.clear();

                // Test for overlap against links

                for (int link_idx = 0; link_idx < editor.Links.Pool.size(); ++link_idx)
                {
                    if (editor.Links.InUse[link_idx])
                    {
                        const ImLinkData&link = editor.Links.Pool[link_idx];

                        const ImPinData&pin_start = editor.Pins.Pool[link.StartPinIdx];
                        const ImPinData&pin_end = editor.Pins.Pool[link.EndPinIdx];
                        const ImRect&node_start_rect = editor.Nodes.Pool[pin_start.ParentNodeIdx].Rect;
                        const ImRect&node_end_rect = editor.Nodes.Pool[pin_end.ParentNodeIdx].Rect;

                        const Vector2 start = GetScreenSpacePinCoordinates(
                            node_start_rect, pin_start.AttributeRect, pin_start.Type);
                        const Vector2 end =
                            GetScreenSpacePinCoordinates(node_end_rect, pin_end.AttributeRect, pin_end.Type);

                        // Test
                        if (RectangleOverlapsLink(box_rect, start, end, pin_start.Type))
                        {
                            editor.SelectedLinkIndices.push_back(link_idx);
                        }
                    }
                }
            }

            void TranslateSelectedNodes(ImNodesEditorContext editor)
            {
                if (GImNodes.LeftMouseDragging)
                {
                    const ImGuiIO&io = ImGui.GetIO();
                    for (int i = 0; i < editor.SelectedNodeIndices.size(); ++i)
                    {
                        const int node_idx = editor.SelectedNodeIndices[i];
                        ImNodeData & node = editor.Nodes.Pool[node_idx];
                        if (node.Draggable)
                        {
                            node.Origin += io.MouseDelta - editor.AutoPanningDelta;
                        }
                    }
                }
            }

            struct LinkPredicate
            {
                bool operator()(const ImLinkData& lhs, const ImLinkData& rhs) const
                {
        // Do a unique compare by sorting the pins' addresses.
        // This catches duplicate links, whether they are in the
        // same direction or not.
        // Sorting by pin index should have the uniqueness guarantees as sorting
        // by id -- each unique id will get one slot in the link pool array.

                int lhs_start = lhs.StartPinIdx;
                int lhs_end = lhs.EndPinIdx;
                int rhs_start = rhs.StartPinIdx;
                int rhs_end = rhs.EndPinIdx;

        if (lhs_start > lhs_end)
        {
            ImSwap(ref lhs_start, ref lhs_end);
            }

        if (rhs_start > rhs_end)
        {
            ImSwap(ref rhs_start, ref rhs_end);
        }

        return lhs_start == rhs_start && lhs_end == rhs_end;
    }
};

ImOptionalIndex FindDuplicateLink(
    const ImNodesEditorContext editor,
    const int start_pin_idx,
    const int end_pin_idx)
{
    ImLinkData test_link(0);
test_link.StartPinIdx = start_pin_idx;
test_link.EndPinIdx = end_pin_idx;
for (int link_idx = 0; link_idx < editor.Links.Pool.size(); ++link_idx)
{
    const ImLinkData&link = editor.Links.Pool[link_idx];
    if (LinkPredicate()(test_link, link) && editor.Links.InUse[link_idx])
    {
        return ImOptionalIndex(link_idx);
    }
}

return ImOptionalIndex();
}

bool ShouldLinkSnapToPin(
    const ImNodesEditorContext editor,
    const ImPinData&            start_pin,
    const int hovered_pin_idx,
    const ImOptionalIndex duplicate_link)
{
    const ImPinData&end_pin = editor.Pins.Pool[hovered_pin_idx];

    // The end pin must be in a different node
    if (start_pin.ParentNodeIdx == end_pin.ParentNodeIdx)
    {
        return false;
    }

    // The end pin must be of a different type
    if (start_pin.Type == end_pin.Type)
    {
        return false;
    }

    // The link to be created must not be a duplicate, unless it is the link which was created on
    // snap. In that case we want to snap, since we want it to appear visually as if the created
    // link remains snapped to the pin.
    if (duplicate_link.HasValue() && !(duplicate_link == GImNodes.SnapLinkIdx))
    {
        return false;
    }

    return true;
}

void ClickInteractionUpdate(ImNodesEditorContext editor)
{
    switch (editor.ClickInteraction.Type)
    {
        case ImNodesClickInteractionType.BoxSelection:
            {
                editor.ClickInteraction.BoxSelector.Rect.Max =
                    ScreenSpaceToGridSpace(editor, GImNodes.MousePos);

                ImRect box_rect = editor.ClickInteraction.BoxSelector.Rect;
                box_rect.Min = GridSpaceToScreenSpace(editor, box_rect.Min);
                box_rect.Max = GridSpaceToScreenSpace(editor, box_rect.Max);

                BoxSelectorUpdateSelection(editor, box_rect);

                const ImU32 box_selector_color = GImNodes.Style.Colors[ImNodesCol_BoxSelector];
                const ImU32 box_selector_outline = GImNodes.Style.Colors[ImNodesCol_BoxSelectorOutline];
                GImNodes.CanvasDrawList->AddRectFilled(box_rect.Min, box_rect.Max, box_selector_color);
                GImNodes.CanvasDrawList->AddRect(box_rect.Min, box_rect.Max, box_selector_outline);

                if (GImNodes.LeftMouseReleased)
                {
                    ImVector<int> & depth_stack = editor.NodeDepthOrder;
                    const ImVector<int>&selected_idxs = editor.SelectedNodeIndices;

                    // Bump the selected node indices, in order, to the top of the depth stack.
                    // NOTE: this algorithm has worst case time complexity of O(N^2), if the node selection
                    // is ~ N (due to selected_idxs.contains()).

                    if ((selected_idxs.Size > 0) && (selected_idxs.Size < depth_stack.Size))
                    {
                        int num_moved = 0; // The number of indices moved. Stop after selected_idxs.Size
                        for (int i = 0; i < depth_stack.Size - selected_idxs.Size; ++i)
                        {
                            for (int node_idx = depth_stack[i]; selected_idxs.contains(node_idx);
                                 node_idx = depth_stack[i])
                            {
                                depth_stack.erase(depth_stack.begin() + static_cast<size_t>(i));
                                depth_stack.push_back(node_idx);
                                ++num_moved;
                            }

                            if (num_moved == selected_idxs.Size)
                            {
                                break;
                            }
                        }
                    }

                    editor.ClickInteraction.Type = ImNodesClickInteractionType.None;
                }
            }
            break;
        case ImNodesClickInteractionType.Node:
            {
                TranslateSelectedNodes(editor);

                if (GImNodes.LeftMouseReleased)
                {
                    editor.ClickInteraction.Type = ImNodesClickInteractionType.None;
                }
            }
            break;
        case ImNodesClickInteractionType.Link:
            {
                if (GImNodes.LeftMouseReleased)
                {
                    editor.ClickInteraction.Type = ImNodesClickInteractionType.None;
                }
            }
            break;
        case ImNodesClickInteractionType.LinkCreation:
            {
                const ImPinData&start_pin =
                    editor.Pins.Pool[editor.ClickInteraction.LinkCreation.StartPinIdx];

                const ImOptionalIndex maybe_duplicate_link_idx =
                    GImNodes.HoveredPinIdx.HasValue()
                        ? FindDuplicateLink(
                              editor,
                              editor.ClickInteraction.LinkCreation.StartPinIdx,
                              GImNodes.HoveredPinIdx.Value())
                        : ImOptionalIndex();

                const bool should_snap =
                    GImNodes.HoveredPinIdx.HasValue() &&
                    ShouldLinkSnapToPin(
                        editor, start_pin, GImNodes.HoveredPinIdx.Value(), maybe_duplicate_link_idx);

                // If we created on snap and the hovered pin is empty or changed, then we need signal that
                // the link's state has changed.
                const bool snapping_pin_changed =
                    editor.ClickInteraction.LinkCreation.EndPinIdx.HasValue() &&
                    !(GImNodes.HoveredPinIdx == editor.ClickInteraction.LinkCreation.EndPinIdx);

                // Detach the link that was created by this link event if it's no longer in snap range
                if (snapping_pin_changed && GImNodes.SnapLinkIdx.HasValue())
                {
                    BeginLinkDetach(
                        editor,
                        GImNodes.SnapLinkIdx.Value(),
                        editor.ClickInteraction.LinkCreation.EndPinIdx.Value());
                }

                const Vector2 start_pos = GetScreenSpacePinCoordinates(editor, start_pin);
                // If we are within the hover radius of a receiving pin, snap the link
                // endpoint to it
                const Vector2 end_pos = should_snap
                                           ? GetScreenSpacePinCoordinates(
                                                 editor, editor.Pins.Pool[GImNodes.HoveredPinIdx.Value()])
                                           : GImNodes.MousePos;

                const CubicBezier cubic_bezier = GetCubicBezier(
                    start_pos, end_pos, start_pin.Type, GImNodes.Style.LinkLineSegmentsPerLength);
#if IMGUI_VERSION_NUM < 18000
        GImNodes.CanvasDrawList->AddBezierCurve(
#else
                GImNodes.CanvasDrawList->AddBezierCubic(
#endif
            cubic_bezier.P0,
                    cubic_bezier.P1,
                    cubic_bezier.P2,
                    cubic_bezier.P3,
                    GImNodes.Style.Colors[ImNodesCol_Link],
                    GImNodes.Style.LinkThickness,
                    cubic_bezier.NumSegments);

                const bool link_creation_on_snap =
                    GImNodes.HoveredPinIdx.HasValue() &&
                    (editor.Pins.Pool[GImNodes.HoveredPinIdx.Value()].Flags &
                     ImNodesAttributeFlags_EnableLinkCreationOnSnap);

                if (!should_snap)
                {
                    editor.ClickInteraction.LinkCreation.EndPinIdx.Reset();
                }

                const bool create_link =
                    should_snap && (GImNodes.LeftMouseReleased || link_creation_on_snap);

                if (create_link && !maybe_duplicate_link_idx.HasValue())
                {
                    // Avoid send OnLinkCreated() events every frame if the snap link is not saved
                    // (only applies for EnableLinkCreationOnSnap)
                    if (!GImNodes.LeftMouseReleased &&
                        editor.ClickInteraction.LinkCreation.EndPinIdx == GImNodes.HoveredPinIdx)
                    {
                        break;
                    }

                    GImNodes.ImNodesUIState |= ImNodesUIState_LinkCreated;
                    editor.ClickInteraction.LinkCreation.EndPinIdx = GImNodes.HoveredPinIdx.Value();
                }

                if (GImNodes.LeftMouseReleased)
                {
                    editor.ClickInteraction.Type = ImNodesClickInteractionType.None;
                    if (!create_link)
                    {
                        GImNodes.ImNodesUIState |= ImNodesUIState_LinkDropped;
                    }
                }
            }
            break;
        case ImNodesClickInteractionType.Panning:
            {
                const bool dragging = GImNodes.AltMouseDragging;

                if (dragging)
                {
                    editor.Panning += ImGui.GetIO().MouseDelta;
                }
                else
                {
                    editor.ClickInteraction.Type = ImNodesClickInteractionType.None;
                }
            }
            break;
        case ImNodesClickInteractionType.ImGuiItem:
            {
                if (GImNodes.LeftMouseReleased)
                {
                    editor.ClickInteraction.Type = ImNodesClickInteractionType.None;
                }
            }
        case ImNodesClickInteractionType.None:
            break;
        default:
            assert(!"Unreachable code!");
            break;
    }
}

void ResolveOccludedPins(const ImNodesEditorContext editor, ImVector<int>& occluded_pin_indices)
{
    const ImVector<int>&depth_stack = editor.NodeDepthOrder;

    occluded_pin_indices.resize(0);

    if (depth_stack.Size < 2)
    {
        return;
    }

    // For each node in the depth stack
    for (int depth_idx = 0; depth_idx < (depth_stack.Size - 1); ++depth_idx)
    {
        const ImNodeData&node_below = editor.Nodes.Pool[depth_stack[depth_idx]];

        // Iterate over the rest of the depth stack to find nodes overlapping the pins
        for (int next_depth_idx = depth_idx + 1; next_depth_idx < depth_stack.Size;
             ++next_depth_idx)
        {
            const ImRect&rect_above = editor.Nodes.Pool[depth_stack[next_depth_idx]].Rect;

            // Iterate over each pin
            for (int idx = 0; idx < node_below.PinIndices.Size; ++idx)
            {
                const int pin_idx = node_below.PinIndices[idx];
                Vector2pin_pos = editor.Pins.Pool[pin_idx].Pos;

                if (rect_above.Contains(pin_pos))
                {
                    occluded_pin_indices.push_back(pin_idx);
                }
            }
        }
    }
}

ImOptionalIndex ResolveHoveredPin(
    const ImObjectPool<ImPinData>& pins,
    const ImVector<int>&           occluded_pin_indices)
{
    float smallest_distance = FLT_MAX;
    ImOptionalIndex pin_idx_with_smallest_distance;

    const float hover_radius_sqr = GImNodes.Style.PinHoverRadius * GImNodes.Style.PinHoverRadius;

    for (int idx = 0; idx < pins.Pool.Size; ++idx)
    {
        if (!pins.InUse[idx])
        {
            continue;
        }

        if (occluded_pin_indices.contains(idx))
        {
            continue;
        }

        Vector2pin_pos = pins.Pool[idx].Pos;
        const float distance_sqr = ImLengthSqr(pin_pos - GImNodes.MousePos);

        // TODO: GImNodes.Style.PinHoverRadius needs to be copied into pin data and the pin-local
        // value used here. This is no longer called in BeginAttribute/EndAttribute scope and the
        // detected pin might have a different hover radius than what the user had when calling
        // BeginAttribute/EndAttribute.
        if (distance_sqr < hover_radius_sqr && distance_sqr < smallest_distance)
        {
            smallest_distance = distance_sqr;
            pin_idx_with_smallest_distance = idx;
        }
    }

    return pin_idx_with_smallest_distance;
}

ImOptionalIndex ResolveHoveredNode(const ImVector<int>& depth_stack)
{
    if (GImNodes.NodeIndicesOverlappingWithMouse.size() == 0)
    {
        return ImOptionalIndex();
    }

    if (GImNodes.NodeIndicesOverlappingWithMouse.size() == 1)
    {
        return ImOptionalIndex(GImNodes.NodeIndicesOverlappingWithMouse[0]);
    }

    int largest_depth_idx = -1;
    int node_idx_on_top = -1;

    for (int i = 0; i < GImNodes.NodeIndicesOverlappingWithMouse.size(); ++i)
    {
        const int node_idx = GImNodes.NodeIndicesOverlappingWithMouse[i];
        for (int depth_idx = 0; depth_idx < depth_stack.size(); ++depth_idx)
        {
            if (depth_stack[depth_idx] == node_idx && (depth_idx > largest_depth_idx))
            {
                largest_depth_idx = depth_idx;
                node_idx_on_top = node_idx;
            }
        }
    }

    assert(node_idx_on_top != -1);
    return ImOptionalIndex(node_idx_on_top);
}

ImOptionalIndex ResolveHoveredLink(
    const ImObjectPool<ImLinkData>& links,
    const ImObjectPool<ImPinData>&  pins)
{
    float smallest_distance = FLT_MAX;
    ImOptionalIndex link_idx_with_smallest_distance;

    // There are two ways a link can be detected as "hovered".
    // 1. The link is within hover distance to the mouse. The closest such link is selected as being
    // hovered over.
    // 2. If the link is connected to the currently hovered pin.
    //
    // The latter is a requirement for link detaching with drag click to work, as both a link and
    // pin are required to be hovered over for the feature to work.

    for (int idx = 0; idx < links.Pool.Size; ++idx)
    {
        if (!links.InUse[idx])
        {
            continue;
        }

        const ImLinkData&link = links.Pool[idx];
        const ImPinData&start_pin = pins.Pool[link.StartPinIdx];
        const ImPinData&end_pin = pins.Pool[link.EndPinIdx];

        // If there is a hovered pin links can only be considered hovered if they use that pin
        if (GImNodes.HoveredPinIdx.HasValue())
        {
            if (GImNodes.HoveredPinIdx == link.StartPinIdx ||
                GImNodes.HoveredPinIdx == link.EndPinIdx)
            {
                return idx;
            }
            continue;
        }

        // TODO: the calculated CubicBeziers could be cached since we generate them again when
        // rendering the links

        const CubicBezier cubic_bezier = GetCubicBezier(
            start_pin.Pos, end_pin.Pos, start_pin.Type, GImNodes.Style.LinkLineSegmentsPerLength);

        // The distance test
        {
            const ImRect link_rect = GetContainingRectForCubicBezier(cubic_bezier);

            // First, do a simple bounding box test against the box containing the link
            // to see whether calculating the distance to the link is worth doing.
            if (link_rect.Contains(GImNodes.MousePos))
            {
                const float distance = GetDistanceToCubicBezier(
                    GImNodes.MousePos, cubic_bezier, cubic_bezier.NumSegments);

                // TODO: GImNodes.Style.LinkHoverDistance could be also copied into ImLinkData,
                // since we're not calling this function in the same scope as ImNodes.Link(). The
                // rendered/detected link might have a different hover distance than what the user
                // had specified when calling Link()
                if (distance < GImNodes.Style.LinkHoverDistance && distance < smallest_distance)
                {
                    smallest_distance = distance;
                    link_idx_with_smallest_distance = idx;
                }
            }
        }
    }

    return link_idx_with_smallest_distance;
}

// [SECTION] render helpers

inline ImRect GetItemRect() { return ImRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax()); }

inline Vector2 GetNodeTitleBarOrigin(const ImNodeData& node)
{
    return node.Origin + node.LayoutStyle.Padding;
}

inline Vector2 GetNodeContentOrigin(const ImNodeData& node)
{
    const Vector2 title_bar_height =
        Vector2(0.f, node.TitleBarContentRect.GetHeight() + 2.0f * node.LayoutStyle.Padding.Y);
    return node.Origin + title_bar_height + node.LayoutStyle.Padding;
}

inline ImRect GetNodeTitleRect(const ImNodeData& node)
{
    ImRect expanded_title_rect = node.TitleBarContentRect;
    expanded_title_rect.Expand(node.LayoutStyle.Padding);

    return ImRect(
        expanded_title_rect.Min,
        expanded_title_rect.Min + Vector2(node.Rect.GetWidth(), 0.f) +
            Vector2(0.f, expanded_title_rect.GetHeight()));
}

void DrawGrid(ImNodesEditorContext editor, Vector2 canvas_size)
{
    const Vector2 offset = editor.Panning;

    for (float x = fmodf(offset.X, GImNodes.Style.GridSpacing); x < canvas_size.x;
         x += GImNodes.Style.GridSpacing)
    {
        GImNodes.CanvasDrawList->AddLine(
            EditorSpaceToScreenSpace(Vector2(x, 0.0f)),
            EditorSpaceToScreenSpace(Vector2(x, canvas_size.Y)),
            GImNodes.Style.Colors[ImNodesCol_GridLine]);
    }

    for (float y = fmodf(offset.Y, GImNodes.Style.GridSpacing); y < canvas_size.y;
         y += GImNodes.Style.GridSpacing)
    {
        GImNodes.CanvasDrawList->AddLine(
            EditorSpaceToScreenSpace(Vector2(0.0f, y)),
            EditorSpaceToScreenSpace(Vector2(canvas_size.X, y)),
            GImNodes.Style.Colors[ImNodesCol_GridLine]);
    }
}

struct QuadOffsets
{
    Vector2 TopLeft, BottomLeft, BottomRight, TopRight;
};

QuadOffsets CalculateQuadOffsets(const float side_length)
{
    const float half_side = 0.5f * side_length;

    QuadOffsets offset;

    offset.TopLeft = Vector2(-half_side, half_side);
    offset.BottomLeft = Vector2(-half_side, -half_side);
    offset.BottomRight = Vector2(half_side, -half_side);
    offset.TopRight = Vector2(half_side, half_side);

    return offset;
}

struct TriangleOffsets
{
    Vector2 TopLeft, BottomLeft, Right;
};

TriangleOffsets CalculateTriangleOffsets(const float side_length)
{
    // Calculates the Vec2 offsets from an equilateral triangle's midpoint to
    // its vertices. Here is how the left_offset and right_offset are
    // calculated.
    //
    // For an equilateral triangle of side length s, the
    // triangle's height, h, is h = s * sqrt(3) / 2.
    //
    // The length from the base to the midpoint is (1 / 3) * h. The length from
    // the midpoint to the triangle vertex is (2 / 3) * h.
    const float sqrt_3 = sqrtf(3.0f);
    const float left_offset = -0.1666666666667f * sqrt_3 * side_length;
    const float right_offset = 0.333333333333f * sqrt_3 * side_length;
    const float vertical_offset = 0.5f * side_length;

    TriangleOffsets offset;
    offset.TopLeft = Vector2(left_offset, vertical_offset);
    offset.BottomLeft = Vector2(left_offset, -vertical_offset);
    offset.Right = Vector2(right_offset, 0.f);

    return offset;
}

void DrawPinShape(Vector2 pin_pos, const ImPinData& pin, const ImU32 pin_color)
{
    static const int CIRCLE_NUM_SEGMENTS = 8;

    switch (pin.Shape)
    {
        case ImNodesPinShape_Circle:
            {
                GImNodes.CanvasDrawList->AddCircle(
                    pin_pos,
                    GImNodes.Style.PinCircleRadius,
                    pin_color,
                    CIRCLE_NUM_SEGMENTS,
                    GImNodes.Style.PinLineThickness);
            }
            break;
        case ImNodesPinShape_CircleFilled:
            {
                GImNodes.CanvasDrawList->AddCircleFilled(
                    pin_pos, GImNodes.Style.PinCircleRadius, pin_color, CIRCLE_NUM_SEGMENTS);
            }
            break;
        case ImNodesPinShape_Quad:
            {
                const QuadOffsets offset = CalculateQuadOffsets(GImNodes.Style.PinQuadSideLength);
                GImNodes.CanvasDrawList->AddQuad(
                    pin_pos + offset.TopLeft,
                    pin_pos + offset.BottomLeft,
                    pin_pos + offset.BottomRight,
                    pin_pos + offset.TopRight,
                    pin_color,
                    GImNodes.Style.PinLineThickness);
            }
            break;
        case ImNodesPinShape_QuadFilled:
            {
                const QuadOffsets offset = CalculateQuadOffsets(GImNodes.Style.PinQuadSideLength);
                GImNodes.CanvasDrawList->AddQuadFilled(
                    pin_pos + offset.TopLeft,
                    pin_pos + offset.BottomLeft,
                    pin_pos + offset.BottomRight,
                    pin_pos + offset.TopRight,
                    pin_color);
            }
            break;
        case ImNodesPinShape_Triangle:
            {
                const TriangleOffsets offset =
                    CalculateTriangleOffsets(GImNodes.Style.PinTriangleSideLength);
                GImNodes.CanvasDrawList->AddTriangle(
                    pin_pos + offset.TopLeft,
                    pin_pos + offset.BottomLeft,
                    pin_pos + offset.Right,
                    pin_color,
                    // NOTE: for some weird reason, the line drawn by AddTriangle is
                    // much thinner than the lines drawn by AddCircle or AddQuad.
                    // Multiplying the line thickness by two seemed to solve the
                    // problem at a few different thickness values.
                    2.f * GImNodes.Style.PinLineThickness);
            }
            break;
        case ImNodesPinShape_TriangleFilled:
            {
                const TriangleOffsets offset =
                    CalculateTriangleOffsets(GImNodes.Style.PinTriangleSideLength);
                GImNodes.CanvasDrawList->AddTriangleFilled(
                    pin_pos + offset.TopLeft,
                    pin_pos + offset.BottomLeft,
                    pin_pos + offset.Right,
                    pin_color);
            }
            break;
        default:
            assert(!"Invalid PinShape value!");
            break;
    }
}

void DrawPin(ImNodesEditorContext editor, const int pin_idx)
{
    ImPinData & pin = editor.Pins.Pool[pin_idx];
    const ImRect&parent_node_rect = editor.Nodes.Pool[pin.ParentNodeIdx].Rect;

    pin.Pos = GetScreenSpacePinCoordinates(parent_node_rect, pin.AttributeRect, pin.Type);

    ImU32 pin_color = pin.ColorStyle.Background;

    if (GImNodes.HoveredPinIdx == pin_idx)
    {
        pin_color = pin.ColorStyle.Hovered;
    }

    DrawPinShape(pin.Pos, pin, pin_color);
}

void DrawNode(ImNodesEditorContext editor, const int node_idx)
{
    const ImNodeData&node = editor.Nodes.Pool[node_idx];
    ImGui.SetCursorPos(node.Origin + editor.Panning);

    const bool node_hovered =
        GImNodes.HoveredNodeIdx == node_idx &&
        editor.ClickInteraction.Type != ImNodesClickInteractionType.BoxSelection;

    ImU32 node_background = node.ColorStyle.Background;
    ImU32 titlebar_background = node.ColorStyle.Titlebar;

    if (editor.SelectedNodeIndices.contains(node_idx))
    {
        node_background = node.ColorStyle.BackgroundSelected;
        titlebar_background = node.ColorStyle.TitlebarSelected;
    }
    else if (node_hovered)
    {
        node_background = node.ColorStyle.BackgroundHovered;
        titlebar_background = node.ColorStyle.TitlebarHovered;
    }

    {
        // node base
        GImNodes.CanvasDrawList->AddRectFilled(
            node.Rect.Min, node.Rect.Max, node_background, node.LayoutStyle.CornerRounding);

        // title bar:
        if (node.TitleBarContentRect.GetHeight() > 0.f)
        {
            ImRect title_bar_rect = GetNodeTitleRect(node);

#if IMGUI_VERSION_NUM < 18200
            GImNodes.CanvasDrawList->AddRectFilled(
                title_bar_rect.Min,
                title_bar_rect.Max,
                titlebar_background,
                node.LayoutStyle.CornerRounding,
                ImDrawCornerFlags_Top);
#else
            GImNodes.CanvasDrawList->AddRectFilled(
                title_bar_rect.Min,
                title_bar_rect.Max,
                titlebar_background,
                node.LayoutStyle.CornerRounding,
                ImDrawFlags_RoundCornersTop);

#endif
        }

        if ((GImNodes.Style.Flags & ImNodesStyleFlags_NodeOutline) != 0)
        {
#if IMGUI_VERSION_NUM < 18200
            GImNodes.CanvasDrawList->AddRect(
                node.Rect.Min,
                node.Rect.Max,
                node.ColorStyle.Outline,
                node.LayoutStyle.CornerRounding,
                ImDrawCornerFlags_All,
                node.LayoutStyle.BorderThickness);
#else
            GImNodes.CanvasDrawList->AddRect(
                node.Rect.Min,
                node.Rect.Max,
                node.ColorStyle.Outline,
                node.LayoutStyle.CornerRounding,
                ImDrawFlags_RoundCornersAll,
                node.LayoutStyle.BorderThickness);
#endif
        }
    }

    for (int i = 0; i < node.PinIndices.size(); ++i)
    {
        DrawPin(editor, node.PinIndices[i]);
    }

    if (node_hovered)
    {
        GImNodes.HoveredNodeIdx = node_idx;
    }
}

void DrawLink(ImNodesEditorContext editor, const int link_idx)
{
    const ImLinkData&link = editor.Links.Pool[link_idx];
    const ImPinData&start_pin = editor.Pins.Pool[link.StartPinIdx];
    const ImPinData&end_pin = editor.Pins.Pool[link.EndPinIdx];

    const CubicBezier cubic_bezier = GetCubicBezier(
        start_pin.Pos, end_pin.Pos, start_pin.Type, GImNodes.Style.LinkLineSegmentsPerLength);

    const bool link_hovered =
        GImNodes.HoveredLinkIdx == link_idx &&
        editor.ClickInteraction.Type != ImNodesClickInteractionType.BoxSelection;

    if (link_hovered)
    {
        GImNodes.HoveredLinkIdx = link_idx;
    }

    // It's possible for a link to be deleted in begin_link_interaction. A user
    // may detach a link, resulting in the link wire snapping to the mouse
    // position.
    //
    // In other words, skip rendering the link if it was deleted.
    if (GImNodes.DeletedLinkIdx == link_idx)
    {
        return;
    }

    ImU32 link_color = link.ColorStyle.Base;
    if (editor.SelectedLinkIndices.contains(link_idx))
    {
        link_color = link.ColorStyle.Selected;
    }
    else if (link_hovered)
    {
        link_color = link.ColorStyle.Hovered;
    }

#if IMGUI_VERSION_NUM < 18000
    GImNodes.CanvasDrawList->AddBezierCurve(
#else
    GImNodes.CanvasDrawList->AddBezierCubic(
#endif
        cubic_bezier.P0,
        cubic_bezier.P1,
        cubic_bezier.P2,
        cubic_bezier.P3,
        link_color,
        GImNodes.Style.LinkThickness,
        cubic_bezier.NumSegments);
}

void BeginPinAttribute(
    const int id,
    const ImNodesAttributeType type,
    const ImNodesPinShape shape,
    const int node_idx)
{
    // Make sure to call BeginNode() before calling
    // BeginAttribute()
    assert(GImNodes.CurrentScope == ImNodesScope_Node);
    GImNodes.CurrentScope = ImNodesScope_Attribute;

    ImGui.BeginGroup();
    ImGui.PushID(id);

    ImNodesEditorContext & editor = EditorContextGet();

    GImNodes.CurrentAttributeId = id;

    const int pin_idx = ObjectPoolFindOrCreateIndex(editor.Pins, id);
    GImNodes.CurrentPinIdx = pin_idx;
    ImPinData & pin = editor.Pins.Pool[pin_idx];
    pin.Id = id;
    pin.ParentNodeIdx = node_idx;
    pin.Type = type;
    pin.Shape = shape;
    pin.Flags = GImNodes.CurrentAttributeFlags;
    pin.ColorStyle.Background = GImNodes.Style.Colors[ImNodesCol_Pin];
    pin.ColorStyle.Hovered = GImNodes.Style.Colors[ImNodesCol_PinHovered];
}

void EndPinAttribute()
{
    assert(GImNodes.CurrentScope == ImNodesScope_Attribute);
    GImNodes.CurrentScope = ImNodesScope_Node;

    ImGui.PopID();
    ImGui.EndGroup();

    if (ImGui.IsItemActive())
    {
        GImNodes.ActiveAttribute = true;
        GImNodes.ActiveAttributeId = GImNodes.CurrentAttributeId;
    }

    ImNodesEditorContext & editor = EditorContextGet();
    ImPinData & pin = editor.Pins.Pool[GImNodes.CurrentPinIdx];
    ImNodeData & node = editor.Nodes.Pool[GImNodes.CurrentNodeIdx];
    pin.AttributeRect = GetItemRect();
    node.PinIndices.push_back(GImNodes.CurrentPinIdx);
}

void Initialize(ImNodesContext* context)
{
    context->CanvasOriginScreenSpace = Vector2(0.0f, 0.0f);
    context->CanvasRectScreenSpace = ImRect(Vector2(0.f, 0.f), Vector2(0.f, 0.f));
    context->CurrentScope = ImNodesScope_None;

    context->CurrentPinIdx = INT_MAX;
    context->CurrentNodeIdx = INT_MAX;

    context->DefaultEditorCtx = EditorContextCreate();
    EditorContextSet(GImNodes.DefaultEditorCtx);

    context->CurrentAttributeFlags = ImNodesAttributeFlags_None;
    context->AttributeFlagStack.push_back(GImNodes.CurrentAttributeFlags);

    StyleColorsDark();
}

void Shutdown(ImNodesContext* ctx) { EditorContextFree(ctx->DefaultEditorCtx); }

// [SECTION] minimap

static inline bool IsMiniMapActive()
{
    ImNodesEditorContext & editor = EditorContextGet();
    return editor.MiniMapEnabled && editor.MiniMapSizeFraction > 0.0f;
}

static inline bool IsMiniMapHovered()
{
    ImNodesEditorContext & editor = EditorContextGet();
    return IsMiniMapActive() &&
           ImGui.IsMouseHoveringRect(
               editor.MiniMapRectScreenSpace.Min, editor.MiniMapRectScreenSpace.Max);
}

static inline void CalcMiniMapLayout()
{
    ImNodesEditorContext & editor = EditorContextGet();
    const Vector2 offset = GImNodes.Style.MiniMapOffset;
    const Vector2 border = GImNodes.Style.MiniMapPadding;
    const ImRect editor_rect = GImNodes.CanvasRectScreenSpace;

    // Compute the size of the mini-map area
    Vector2 mini_map_size;
    float mini_map_scaling;
    {
        const Vector2 max_size =
            ImFloor(editor_rect.GetSize() * editor.MiniMapSizeFraction - border * 2.0f);
        const float max_size_aspect_ratio = max_size.X / max_size.y;
        const Vector2 grid_content_size = editor.GridContentBounds.IsInverted()
                                             ? max_size
                                             : ImFloor(editor.GridContentBounds.GetSize());
        const float grid_content_aspect_ratio = grid_content_size.X / grid_content_size.y;
        mini_map_size = ImFloor(
            grid_content_aspect_ratio > max_size_aspect_ratio
                ? Vector2(max_size.X, max_size.X / grid_content_aspect_ratio)
                : Vector2(max_size.Y * grid_content_aspect_ratio, max_size.Y));
        mini_map_scaling = mini_map_size.X / grid_content_size.x;
    }

    // Compute location of the mini-map
    Vector2 mini_map_pos;
    {
        Vector2 align;

        switch (editor.MiniMapLocation)
        {
            case ImNodesMiniMapLocation_BottomRight:
                align.X = 1.0f;
                align.Y = 1.0f;
                break;
            case ImNodesMiniMapLocation_BottomLeft:
                align.X = 0.0f;
                align.Y = 1.0f;
                break;
            case ImNodesMiniMapLocation_TopRight:
                align.X = 1.0f;
                align.Y = 0.0f;
                break;
            case ImNodesMiniMapLocation_TopLeft: // [[fallthrough]]
            default:
                align.X = 0.0f;
                align.Y = 0.0f;
                break;
        }

        const Vector2 top_left_pos = editor_rect.Min + offset + border;
        const Vector2 bottom_right_pos = editor_rect.Max - offset - border - mini_map_size;
        mini_map_pos = ImFloor(ImLerp(top_left_pos, bottom_right_pos, align));
    }

    editor.MiniMapRectScreenSpace =
        ImRect(mini_map_pos - border, mini_map_pos + mini_map_size + border);
    editor.MiniMapContentScreenSpace = ImRect(mini_map_pos, mini_map_pos + mini_map_size);
    editor.MiniMapScaling = mini_map_scaling;
}

static void MiniMapDrawNode(ImNodesEditorContext editor, const int node_idx)
{
    const ImNodeData&node = editor.Nodes.Pool[node_idx];

    const ImRect node_rect = ScreenSpaceToMiniMapSpace(editor, node.Rect);

    // Round to near whole pixel value for corner-rounding to prevent visual glitches
    const float mini_map_node_rounding =
        floorf(node.LayoutStyle.CornerRounding * editor.MiniMapScaling);

    ImU32 mini_map_node_background;

    if (editor.ClickInteraction.Type == ImNodesClickInteractionType.None &&
        ImGui.IsMouseHoveringRect(node_rect.Min, node_rect.Max))
    {
        mini_map_node_background = GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundHovered];

        // Run user callback when hovering a mini-map node
        if (editor.MiniMapNodeHoveringCallback)
        {
            editor.MiniMapNodeHoveringCallback(node.Id, editor.MiniMapNodeHoveringCallbackUserData);
        }
    }
    else if (editor.SelectedNodeIndices.contains(node_idx))
    {
        mini_map_node_background = GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundSelected];
    }
    else
    {
        mini_map_node_background = GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackground];
    }

    const ImU32 mini_map_node_outline = GImNodes.Style.Colors[ImNodesCol_MiniMapNodeOutline];

    GImNodes.CanvasDrawList->AddRectFilled(
        node_rect.Min, node_rect.Max, mini_map_node_background, mini_map_node_rounding);

    GImNodes.CanvasDrawList->AddRect(
        node_rect.Min, node_rect.Max, mini_map_node_outline, mini_map_node_rounding);
}

static void MiniMapDrawLink(ImNodesEditorContext editor, const int link_idx)
{
    const ImLinkData&link = editor.Links.Pool[link_idx];
    const ImPinData&start_pin = editor.Pins.Pool[link.StartPinIdx];
    const ImPinData&end_pin = editor.Pins.Pool[link.EndPinIdx];

    const CubicBezier cubic_bezier = GetCubicBezier(
        ScreenSpaceToMiniMapSpace(editor, start_pin.Pos),
        ScreenSpaceToMiniMapSpace(editor, end_pin.Pos),
        start_pin.Type,
        GImNodes.Style.LinkLineSegmentsPerLength / editor.MiniMapScaling);

    // It's possible for a link to be deleted in begin_link_interaction. A user
    // may detach a link, resulting in the link wire snapping to the mouse
    // position.
    //
    // In other words, skip rendering the link if it was deleted.
    if (GImNodes.DeletedLinkIdx == link_idx)
    {
        return;
    }

    const ImU32 link_color =
        GImNodes.Style.Colors
            [editor.SelectedLinkIndices.contains(link_idx) ? ImNodesCol_MiniMapLinkSelected
                                                           : ImNodesCol_MiniMapLink];

#if IMGUI_VERSION_NUM < 18000
    GImNodes.CanvasDrawList->AddBezierCurve(
#else
    GImNodes.CanvasDrawList->AddBezierCubic(
#endif
        cubic_bezier.P0,
        cubic_bezier.P1,
        cubic_bezier.P2,
        cubic_bezier.P3,
        link_color,
        GImNodes.Style.LinkThickness * editor.MiniMapScaling,
        cubic_bezier.NumSegments);
}

static void MiniMapUpdate()
{
    ImNodesEditorContext & editor = EditorContextGet();

    ImU32 mini_map_background;

    if (IsMiniMapHovered())
    {
        mini_map_background = GImNodes.Style.Colors[ImNodesCol_MiniMapBackgroundHovered];
    }
    else
    {
        mini_map_background = GImNodes.Style.Colors[ImNodesCol_MiniMapBackground];
    }

    // Create a child window bellow mini-map, so it blocks all mouse interaction on canvas.
    int flags = ImGuiWindowFlags_NoBackground;
    ImGui.SetCursorScreenPos(editor.MiniMapRectScreenSpace.Min);
    ImGui.BeginChild("minimap", editor.MiniMapRectScreenSpace.GetSize(), false, flags);

    const ImRect&mini_map_rect = editor.MiniMapRectScreenSpace;

    // Draw minimap background and border
    GImNodes.CanvasDrawList->AddRectFilled(
        mini_map_rect.Min, mini_map_rect.Max, mini_map_background);

    GImNodes.CanvasDrawList->AddRect(
        mini_map_rect.Min, mini_map_rect.Max, GImNodes.Style.Colors[ImNodesCol_MiniMapOutline]);

    // Clip draw list items to mini-map rect (after drawing background/outline)
    GImNodes.CanvasDrawList->PushClipRect(
        mini_map_rect.Min, mini_map_rect.Max, true /* intersect with editor clip-rect */);

    // Draw links first so they appear under nodes, and we can use the same draw channel
    for (int link_idx = 0; link_idx < editor.Links.Pool.size(); ++link_idx)
    {
        if (editor.Links.InUse[link_idx])
        {
            MiniMapDrawLink(editor, link_idx);
        }
    }

    for (int node_idx = 0; node_idx < editor.Nodes.Pool.size(); ++node_idx)
    {
        if (editor.Nodes.InUse[node_idx])
        {
            MiniMapDrawNode(editor, node_idx);
        }
    }

    // Draw editor canvas rect inside mini-map
    {
        const ImU32 canvas_color = GImNodes.Style.Colors[ImNodesCol_MiniMapCanvas];
        const ImU32 outline_color = GImNodes.Style.Colors[ImNodesCol_MiniMapCanvasOutline];
        const ImRect rect = ScreenSpaceToMiniMapSpace(editor, GImNodes.CanvasRectScreenSpace);

        GImNodes.CanvasDrawList->AddRectFilled(rect.Min, rect.Max, canvas_color);
        GImNodes.CanvasDrawList->AddRect(rect.Min, rect.Max, outline_color);
    }

    // Have to pop mini-map clip rect
    GImNodes.CanvasDrawList->PopClipRect();

    bool mini_map_is_hovered = ImGui.IsWindowHovered();

    ImGui.EndChild();

    bool center_on_click = mini_map_is_hovered && ImGui.IsMouseDown(ImGuiMouseButton_Left) &&
                           editor.ClickInteraction.Type == ImNodesClickInteractionType.None &&
                           !GImNodes.NodeIdxSubmissionOrder.empty();
    if (center_on_click)
    {
        Vector2 target = MiniMapSpaceToGridSpace(editor, ImGui.GetMousePos());
        Vector2 center = GImNodes.CanvasRectScreenSpace.GetSize() * 0.5f;
        editor.Panning = ImFloor(center - target);
    }

    // Reset callback info after use
    editor.MiniMapNodeHoveringCallback = NULL;
    editor.MiniMapNodeHoveringCallbackUserData = NULL;
}

// [SECTION] selection helpers

template < typename T >
  void SelectObject(const ImObjectPool<T>& objects, ImVector<int>& selected_indices, const int id)
{
    const int idx = ObjectPoolFind(objects, id);
    assert(idx >= 0);
    assert(selected_indices.find(idx) == selected_indices.end());
    selected_indices.push_back(idx);
}

template < typename T >
  void ClearObjectSelection(
    const ImObjectPool<T>& objects,
    ImVector<int>&         selected_indices,
    const int              id)
{
    const int idx = ObjectPoolFind(objects, id);
    assert(idx >= 0);
    assert(selected_indices.find(idx) != selected_indices.end());
    selected_indices.find_erase_unsorted(idx);
}

template < typename T >
  bool IsObjectSelected(const ImObjectPool<T>& objects, ImVector<int>& selected_indices, const int id)
{
    const int idx = ObjectPoolFind(objects, id);
    return selected_indices.find(idx) != selected_indices.end();
}

} // namespace
} // namespace IMNODES_NAMESPACE

// [SECTION] API implementation


ImNodesIO.ImNodesIO()
    : EmulateThreeButtonMouse(), LinkDetachWithModifierClick(),
      AltMouseButton(ImGuiMouseButton_Middle), AutoPanningSpeed(1000.0f)
{
}

ImNodesStyle.ImNodesStyle()
    : GridSpacing(32.f), NodeCornerRounding(4.f), NodePadding(8.f, 8.f), NodeBorderThickness(1.f),
      LinkThickness(3.f), LinkLineSegmentsPerLength(0.1f), LinkHoverDistance(10.f),
      PinCircleRadius(4.f), PinQuadSideLength(7.f), PinTriangleSideLength(9.5),
      PinLineThickness(1.f), PinHoverRadius(10.f), PinOffset(0.f), MiniMapPadding(8.0f, 8.0f),
      MiniMapOffset(4.0f, 4.0f), Flags(ImNodesStyleFlags_NodeOutline | ImNodesStyleFlags_GridLines),
      Colors()
{
}

namespace IMNODES_NAMESPACE
{
    ImNodesContext* CreateContext()
    {
        ImNodesContext* ctx = IM_NEW(ImNodesContext)();
        if (GImNodes == NULL)
            SetCurrentContext(ctx);
        Initialize(ctx);
        return ctx;
    }

    void DestroyContext(ImNodesContext* ctx)
    {
        if (ctx == NULL)
            ctx = GImNodes;
        Shutdown(ctx);
        if (GImNodes == ctx)
            SetCurrentContext(NULL);
        IM_DELETE(ctx);
    }

    ImNodesContext* GetCurrentContext() { return GImNodes; }

    void SetCurrentContext(ImNodesContext* ctx) { GImNodes = ctx; }

    ImNodesEditorContext* EditorContextCreate()
    {
        void* mem = ImGui.MemAlloc(sizeof(ImNodesEditorContext));
        new(mem) ImNodesEditorContext();
        return (ImNodesEditorContext*)mem;
    }

    void EditorContextFree(ImNodesEditorContext* ctx)
    {
        ctx->~ImNodesEditorContext();
        ImGui.MemFree(ctx);
    }

    void EditorContextSet(ImNodesEditorContext* ctx) { GImNodes.EditorCtx = ctx; }

    Vector2 EditorContextGetPanning()
    {
        const ImNodesEditorContexteditor = EditorContextGet();
        return editor.Panning;
    }

    void EditorContextResetPanning(Vector2 pos)
    {
        ImNodesEditorContext & editor = EditorContextGet();
        editor.Panning = pos;
    }

    void EditorContextMoveToNode(const int node_id)
    {
        ImNodesEditorContext & editor = EditorContextGet();
        ImNodeData & node = ObjectPoolFindOrCreateObject(editor.Nodes, node_id);

        editor.Panning.X = -node.Origin.x;
        editor.Panning.Y = -node.Origin.y;
    }

    void SetImGuiContext(ImGuiContext* ctx) { ImGui.SetCurrentContext(ctx); }

    ImNodesIO& GetIO() { return GImNodes.Io; }

    ImNodesStyle& GetStyle() { return GImNodes.Style; }

    void StyleColorsDark()
    {
        GImNodes.Style.Colors[ImNodesCol_NodeBackground] = IM_COL32(50, 50, 50, 255);
        GImNodes.Style.Colors[ImNodesCol_NodeBackgroundHovered] = IM_COL32(75, 75, 75, 255);
        GImNodes.Style.Colors[ImNodesCol_NodeBackgroundSelected] = IM_COL32(75, 75, 75, 255);
        GImNodes.Style.Colors[ImNodesCol_NodeOutline] = IM_COL32(100, 100, 100, 255);
        // title bar colors match ImGui's titlebg colors
        GImNodes.Style.Colors[ImNodesCol_TitleBar] = IM_COL32(41, 74, 122, 255);
        GImNodes.Style.Colors[ImNodesCol_TitleBarHovered] = IM_COL32(66, 150, 250, 255);
        GImNodes.Style.Colors[ImNodesCol_TitleBarSelected] = IM_COL32(66, 150, 250, 255);
        // link colors match ImGui's slider grab colors
        GImNodes.Style.Colors[ImNodesCol_Link] = IM_COL32(61, 133, 224, 200);
        GImNodes.Style.Colors[ImNodesCol_LinkHovered] = IM_COL32(66, 150, 250, 255);
        GImNodes.Style.Colors[ImNodesCol_LinkSelected] = IM_COL32(66, 150, 250, 255);
        // pin colors match ImGui's button colors
        GImNodes.Style.Colors[ImNodesCol_Pin] = IM_COL32(53, 150, 250, 180);
        GImNodes.Style.Colors[ImNodesCol_PinHovered] = IM_COL32(53, 150, 250, 255);

        GImNodes.Style.Colors[ImNodesCol_BoxSelector] = IM_COL32(61, 133, 224, 30);
        GImNodes.Style.Colors[ImNodesCol_BoxSelectorOutline] = IM_COL32(61, 133, 224, 150);

        GImNodes.Style.Colors[ImNodesCol_GridBackground] = IM_COL32(40, 40, 50, 200);
        GImNodes.Style.Colors[ImNodesCol_GridLine] = IM_COL32(200, 200, 200, 40);

        // minimap colors
        GImNodes.Style.Colors[ImNodesCol_MiniMapBackground] = IM_COL32(25, 25, 25, 150);
        GImNodes.Style.Colors[ImNodesCol_MiniMapBackgroundHovered] = IM_COL32(25, 25, 25, 200);
        GImNodes.Style.Colors[ImNodesCol_MiniMapOutline] = IM_COL32(150, 150, 150, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapOutlineHovered] = IM_COL32(150, 150, 150, 200);
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackground] = IM_COL32(200, 200, 200, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundHovered] = IM_COL32(200, 200, 200, 255);
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundSelected] =
            GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundHovered];
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeOutline] = IM_COL32(200, 200, 200, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapLink] = GImNodes.Style.Colors[ImNodesCol_Link];
        GImNodes.Style.Colors[ImNodesCol_MiniMapLinkSelected] =
            GImNodes.Style.Colors[ImNodesCol_LinkSelected];
        GImNodes.Style.Colors[ImNodesCol_MiniMapCanvas] = IM_COL32(200, 200, 200, 25);
        GImNodes.Style.Colors[ImNodesCol_MiniMapCanvasOutline] = IM_COL32(200, 200, 200, 200);
    }

    void StyleColorsClassic()
    {
        GImNodes.Style.Colors[ImNodesCol_NodeBackground] = IM_COL32(50, 50, 50, 255);
        GImNodes.Style.Colors[ImNodesCol_NodeBackgroundHovered] = IM_COL32(75, 75, 75, 255);
        GImNodes.Style.Colors[ImNodesCol_NodeBackgroundSelected] = IM_COL32(75, 75, 75, 255);
        GImNodes.Style.Colors[ImNodesCol_NodeOutline] = IM_COL32(100, 100, 100, 255);
        GImNodes.Style.Colors[ImNodesCol_TitleBar] = IM_COL32(69, 69, 138, 255);
        GImNodes.Style.Colors[ImNodesCol_TitleBarHovered] = IM_COL32(82, 82, 161, 255);
        GImNodes.Style.Colors[ImNodesCol_TitleBarSelected] = IM_COL32(82, 82, 161, 255);
        GImNodes.Style.Colors[ImNodesCol_Link] = IM_COL32(255, 255, 255, 100);
        GImNodes.Style.Colors[ImNodesCol_LinkHovered] = IM_COL32(105, 99, 204, 153);
        GImNodes.Style.Colors[ImNodesCol_LinkSelected] = IM_COL32(105, 99, 204, 153);
        GImNodes.Style.Colors[ImNodesCol_Pin] = IM_COL32(89, 102, 156, 170);
        GImNodes.Style.Colors[ImNodesCol_PinHovered] = IM_COL32(102, 122, 179, 200);
        GImNodes.Style.Colors[ImNodesCol_BoxSelector] = IM_COL32(82, 82, 161, 100);
        GImNodes.Style.Colors[ImNodesCol_BoxSelectorOutline] = IM_COL32(82, 82, 161, 255);
        GImNodes.Style.Colors[ImNodesCol_GridBackground] = IM_COL32(40, 40, 50, 200);
        GImNodes.Style.Colors[ImNodesCol_GridLine] = IM_COL32(200, 200, 200, 40);

        // minimap colors
        GImNodes.Style.Colors[ImNodesCol_MiniMapBackground] = IM_COL32(25, 25, 25, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapBackgroundHovered] = IM_COL32(25, 25, 25, 200);
        GImNodes.Style.Colors[ImNodesCol_MiniMapOutline] = IM_COL32(150, 150, 150, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapOutlineHovered] = IM_COL32(150, 150, 150, 200);
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackground] = IM_COL32(200, 200, 200, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundSelected] =
            GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundHovered];
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundSelected] = IM_COL32(200, 200, 240, 255);
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeOutline] = IM_COL32(200, 200, 200, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapLink] = GImNodes.Style.Colors[ImNodesCol_Link];
        GImNodes.Style.Colors[ImNodesCol_MiniMapLinkSelected] =
            GImNodes.Style.Colors[ImNodesCol_LinkSelected];
        GImNodes.Style.Colors[ImNodesCol_MiniMapCanvas] = IM_COL32(200, 200, 200, 25);
        GImNodes.Style.Colors[ImNodesCol_MiniMapCanvasOutline] = IM_COL32(200, 200, 200, 200);
    }

    void StyleColorsLight()
    {
        GImNodes.Style.Colors[ImNodesCol_NodeBackground] = IM_COL32(240, 240, 240, 255);
        GImNodes.Style.Colors[ImNodesCol_NodeBackgroundHovered] = IM_COL32(240, 240, 240, 255);
        GImNodes.Style.Colors[ImNodesCol_NodeBackgroundSelected] = IM_COL32(240, 240, 240, 255);
        GImNodes.Style.Colors[ImNodesCol_NodeOutline] = IM_COL32(100, 100, 100, 255);
        GImNodes.Style.Colors[ImNodesCol_TitleBar] = IM_COL32(248, 248, 248, 255);
        GImNodes.Style.Colors[ImNodesCol_TitleBarHovered] = IM_COL32(209, 209, 209, 255);
        GImNodes.Style.Colors[ImNodesCol_TitleBarSelected] = IM_COL32(209, 209, 209, 255);
        // original imgui values: 66, 150, 250
        GImNodes.Style.Colors[ImNodesCol_Link] = IM_COL32(66, 150, 250, 100);
        // original imgui values: 117, 138, 204
        GImNodes.Style.Colors[ImNodesCol_LinkHovered] = IM_COL32(66, 150, 250, 242);
        GImNodes.Style.Colors[ImNodesCol_LinkSelected] = IM_COL32(66, 150, 250, 242);
        // original imgui values: 66, 150, 250
        GImNodes.Style.Colors[ImNodesCol_Pin] = IM_COL32(66, 150, 250, 160);
        GImNodes.Style.Colors[ImNodesCol_PinHovered] = IM_COL32(66, 150, 250, 255);
        GImNodes.Style.Colors[ImNodesCol_BoxSelector] = IM_COL32(90, 170, 250, 30);
        GImNodes.Style.Colors[ImNodesCol_BoxSelectorOutline] = IM_COL32(90, 170, 250, 150);
        GImNodes.Style.Colors[ImNodesCol_GridBackground] = IM_COL32(225, 225, 225, 255);
        GImNodes.Style.Colors[ImNodesCol_GridLine] = IM_COL32(180, 180, 180, 100);

        // minimap colors
        GImNodes.Style.Colors[ImNodesCol_MiniMapBackground] = IM_COL32(25, 25, 25, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapBackgroundHovered] = IM_COL32(25, 25, 25, 200);
        GImNodes.Style.Colors[ImNodesCol_MiniMapOutline] = IM_COL32(150, 150, 150, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapOutlineHovered] = IM_COL32(150, 150, 150, 200);
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackground] = IM_COL32(200, 200, 200, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundSelected] =
            GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundHovered];
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeBackgroundSelected] = IM_COL32(200, 200, 240, 255);
        GImNodes.Style.Colors[ImNodesCol_MiniMapNodeOutline] = IM_COL32(200, 200, 200, 100);
        GImNodes.Style.Colors[ImNodesCol_MiniMapLink] = GImNodes.Style.Colors[ImNodesCol_Link];
        GImNodes.Style.Colors[ImNodesCol_MiniMapLinkSelected] =
            GImNodes.Style.Colors[ImNodesCol_LinkSelected];
        GImNodes.Style.Colors[ImNodesCol_MiniMapCanvas] = IM_COL32(200, 200, 200, 25);
        GImNodes.Style.Colors[ImNodesCol_MiniMapCanvasOutline] = IM_COL32(200, 200, 200, 200);
    }

    void BeginNodeEditor()
    {
        assert(GImNodes.CurrentScope == ImNodesScope_None);
        GImNodes.CurrentScope = ImNodesScope_Editor;

        // Reset state from previous pass

        ImNodesEditorContext & editor = EditorContextGet();
        editor.AutoPanningDelta = Vector2(0, 0);
        editor.GridContentBounds = ImRect(FLT_MAX, FLT_MAX, FLT_MIN, FLT_MIN);
        editor.MiniMapEnabled = false;
        ObjectPoolReset(editor.Nodes);
        ObjectPoolReset(editor.Pins);
        ObjectPoolReset(editor.Links);

        GImNodes.HoveredNodeIdx.Reset();
        GImNodes.HoveredLinkIdx.Reset();
        GImNodes.HoveredPinIdx.Reset();
        GImNodes.DeletedLinkIdx.Reset();
        GImNodes.SnapLinkIdx.Reset();

        GImNodes.NodeIndicesOverlappingWithMouse.clear();

        GImNodes.ImNodesUIState = ImNodesUIState_None;

        GImNodes.MousePos = ImGui.GetIO().MousePos;
        GImNodes.LeftMouseClicked = ImGui.IsMouseClicked(0);
        GImNodes.LeftMouseReleased = ImGui.IsMouseReleased(0);
        GImNodes.AltMouseClicked =
            (GImNodes.Io.EmulateThreeButtonMouse.Modifier != NULL &&
             *GImNodes.Io.EmulateThreeButtonMouse.Modifier && GImNodes.LeftMouseClicked) ||
            ImGui.IsMouseClicked(GImNodes.Io.AltMouseButton);
        GImNodes.LeftMouseDragging = ImGui.IsMouseDragging(0, 0.0f);
        GImNodes.AltMouseDragging =
            (GImNodes.Io.EmulateThreeButtonMouse.Modifier != NULL && GImNodes.LeftMouseDragging &&
             (*GImNodes.Io.EmulateThreeButtonMouse.Modifier)) ||
            ImGui.IsMouseDragging(GImNodes.Io.AltMouseButton, 0.0f);
        GImNodes.AltMouseScrollDelta = ImGui.GetIO().MouseWheel;

        GImNodes.ActiveAttribute = false;

        ImGui.BeginGroup();
        {
            ImGui.PushStyleVar(ImGuiStyleVar_FramePadding, Vector2(1.f, 1.f));
            ImGui.PushStyleVar(ImGuiStyleVar_WindowPadding, Vector2(0.f, 0.f));
            ImGui.PushStyleColor(ImGuiCol_ChildBg, GImNodes.Style.Colors[ImNodesCol_GridBackground]);
            ImGui.BeginChild(
                "scrolling_region",
                Vector2(0.f, 0.f),
                true,
                ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoMove |
                    ImGuiWindowFlags_NoScrollWithMouse);
            GImNodes.CanvasOriginScreenSpace = ImGui.GetCursorScreenPos();

            // NOTE: we have to fetch the canvas draw list *after* we call
            // BeginChild(), otherwise the ImGui UI elements are going to be
            // rendered into the parent window draw list.
            DrawListSet(ImGui.GetWindowDrawList());

            {
                const Vector2 canvas_size = ImGui.GetWindowSize();
                GImNodes.CanvasRectScreenSpace = ImRect(
                    EditorSpaceToScreenSpace(Vector2(0.f, 0.f)), EditorSpaceToScreenSpace(canvas_size));

                if (GImNodes.Style.Flags & ImNodesStyleFlags_GridLines)
                {
                    DrawGrid(editor, canvas_size);
                }
            }
        }
    }

    void EndNodeEditor()
    {
        assert(GImNodes.CurrentScope == ImNodesScope_Editor);
        GImNodes.CurrentScope = ImNodesScope_None;

        ImNodesEditorContext & editor = EditorContextGet();

        bool no_grid_content = editor.GridContentBounds.IsInverted();
        if (no_grid_content)
        {
            editor.GridContentBounds = ScreenSpaceToGridSpace(editor, GImNodes.CanvasRectScreenSpace);
        }

        // Detect ImGui interaction first, because it blocks interaction with the rest of the UI

        if (GImNodes.LeftMouseClicked && ImGui.IsAnyItemActive())
        {
            editor.ClickInteraction.Type = ImNodesClickInteractionType.ImGuiItem;
        }

        // Detect which UI element is being hovered over. Detection is done in a hierarchical fashion,
        // because a UI element being hovered excludes any other as being hovered over.

        // Don't do hovering detection for nodes/links/pins when interacting with the mini-map, since
        // its an *overlay* with its own interaction behavior and must have precedence during mouse
        // interaction.

        if ((editor.ClickInteraction.Type == ImNodesClickInteractionType.None ||
             editor.ClickInteraction.Type == ImNodesClickInteractionType.LinkCreation) &&
            MouseInCanvas() && !IsMiniMapHovered())
        {
            // Pins needs some special care. We need to check the depth stack to see which pins are
            // being occluded by other nodes.
            ResolveOccludedPins(editor, GImNodes.OccludedPinIndices);

            GImNodes.HoveredPinIdx = ResolveHoveredPin(editor.Pins, GImNodes.OccludedPinIndices);

            if (!GImNodes.HoveredPinIdx.HasValue())
            {
                // Resolve which node is actually on top and being hovered using the depth stack.
                GImNodes.HoveredNodeIdx = ResolveHoveredNode(editor.NodeDepthOrder);
            }

            // We don't check for hovered pins here, because if we want to detach a link by clicking and
            // dragging, we need to have both a link and pin hovered.
            if (!GImNodes.HoveredNodeIdx.HasValue())
            {
                GImNodes.HoveredLinkIdx = ResolveHoveredLink(editor.Links, editor.Pins);
            }
        }

        for (int node_idx = 0; node_idx < editor.Nodes.Pool.size(); ++node_idx)
        {
            if (editor.Nodes.InUse[node_idx])
            {
                DrawListActivateNodeBackground(node_idx);
                DrawNode(editor, node_idx);
            }
        }

        // In order to render the links underneath the nodes, we want to first select the bottom draw
        // channel.
        GImNodes.CanvasDrawList->ChannelsSetCurrent(0);

        for (int link_idx = 0; link_idx < editor.Links.Pool.size(); ++link_idx)
        {
            if (editor.Links.InUse[link_idx])
            {
                DrawLink(editor, link_idx);
            }
        }

        // Render the click interaction UI elements (partial links, box selector) on top of everything
        // else.

        DrawListAppendClickInteractionChannel();
        DrawListActivateClickInteractionChannel();

        if (IsMiniMapActive())
        {
            CalcMiniMapLayout();
            MiniMapUpdate();
        }

        // Handle node graph interaction

        if (!IsMiniMapHovered())
        {
            if (GImNodes.LeftMouseClicked && GImNodes.HoveredLinkIdx.HasValue())
            {
                BeginLinkInteraction(editor, GImNodes.HoveredLinkIdx.Value(), GImNodes.HoveredPinIdx);
            }

            else if (GImNodes.LeftMouseClicked && GImNodes.HoveredPinIdx.HasValue())
            {
                BeginLinkCreation(editor, GImNodes.HoveredPinIdx.Value());
            }

            else if (GImNodes.LeftMouseClicked && GImNodes.HoveredNodeIdx.HasValue())
            {
                BeginNodeSelection(editor, GImNodes.HoveredNodeIdx.Value());
            }

            else if (
                GImNodes.LeftMouseClicked || GImNodes.LeftMouseReleased ||
                GImNodes.AltMouseClicked || GImNodes.AltMouseScrollDelta != 0.f)
            {
                BeginCanvasInteraction(editor);
            }

            bool should_auto_pan =
                editor.ClickInteraction.Type == ImNodesClickInteractionType.BoxSelection ||
                editor.ClickInteraction.Type == ImNodesClickInteractionType.LinkCreation ||
                editor.ClickInteraction.Type == ImNodesClickInteractionType.Node;
            if (should_auto_pan && !MouseInCanvas())
            {
                Vector2 mouse = ImGui.GetMousePos();
                Vector2 center = GImNodes.CanvasRectScreenSpace.GetCenter();
                Vector2 direction = (center - mouse);
                direction = direction * ImInvLength(direction, 0.0);

                editor.AutoPanningDelta =
                    direction * ImGui.GetIO().DeltaTime * GImNodes.Io.AutoPanningSpeed;
                editor.Panning += editor.AutoPanningDelta;
            }
        }
        ClickInteractionUpdate(editor);

        // At this point, draw commands have been issued for all nodes (and pins). Update the node pool
        // to detect unused node slots and remove those indices from the depth stack before sorting the
        // node draw commands by depth.
        ObjectPoolUpdate(editor.Nodes);
        ObjectPoolUpdate(editor.Pins);

        DrawListSortChannelsByDepth(editor.NodeDepthOrder);

        // After the links have been rendered, the link pool can be updated as well.
        ObjectPoolUpdate(editor.Links);

        // Finally, merge the draw channels
        GImNodes.CanvasDrawList->ChannelsMerge();

        // pop style
        ImGui.EndChild();      // end scrolling region
        ImGui.PopStyleColor(); // pop child window background color
        ImGui.PopStyleVar();   // pop window padding
        ImGui.PopStyleVar();   // pop frame padding
        ImGui.EndGroup();
    }

    void MiniMap(
    const float minimap_size_fraction,
        const ImNodesMiniMapLocation location,
        const ImNodesMiniMapNodeHoveringCallback node_hovering_callback,
        const ImNodesMiniMapNodeHoveringCallbackUserData node_hovering_callback_data)
    {
        // Check that editor size fraction is sane; must be in the range (0, 1]
        assert(minimap_size_fraction > 0.f && minimap_size_fraction <= 1.f);

        // Remember to call before EndNodeEditor
        assert(GImNodes.CurrentScope == ImNodesScope_Editor);

        ImNodesEditorContext & editor = EditorContextGet();

        editor.MiniMapEnabled = true;
        editor.MiniMapSizeFraction = minimap_size_fraction;
        editor.MiniMapLocation = location;

        // Set node hovering callback information
        editor.MiniMapNodeHoveringCallback = node_hovering_callback;
        editor.MiniMapNodeHoveringCallbackUserData = node_hovering_callback_data;

        // Actual drawing/updating of the MiniMap is done in EndNodeEditor so that
        // mini map is draw over everything and all pin/link positions are updated
        // correctly relative to their respective nodes. Hence, we must store some of
        // of the state for the mini map in GImNodes for the actual drawing/updating
    }

    void BeginNode(const int node_id)
    {
        // Remember to call BeginNodeEditor before calling BeginNode
        assert(GImNodes.CurrentScope == ImNodesScope_Editor);
        GImNodes.CurrentScope = ImNodesScope_Node;

        ImNodesEditorContext & editor = EditorContextGet();

        const int node_idx = ObjectPoolFindOrCreateIndex(editor.Nodes, node_id);
        GImNodes.CurrentNodeIdx = node_idx;

        ImNodeData & node = editor.Nodes.Pool[node_idx];
        node.ColorStyle.Background = GImNodes.Style.Colors[ImNodesCol_NodeBackground];
        node.ColorStyle.BackgroundHovered = GImNodes.Style.Colors[ImNodesCol_NodeBackgroundHovered];
        node.ColorStyle.BackgroundSelected = GImNodes.Style.Colors[ImNodesCol_NodeBackgroundSelected];
        node.ColorStyle.Outline = GImNodes.Style.Colors[ImNodesCol_NodeOutline];
        node.ColorStyle.Titlebar = GImNodes.Style.Colors[ImNodesCol_TitleBar];
        node.ColorStyle.TitlebarHovered = GImNodes.Style.Colors[ImNodesCol_TitleBarHovered];
        node.ColorStyle.TitlebarSelected = GImNodes.Style.Colors[ImNodesCol_TitleBarSelected];
        node.LayoutStyle.CornerRounding = GImNodes.Style.NodeCornerRounding;
        node.LayoutStyle.Padding = GImNodes.Style.NodePadding;
        node.LayoutStyle.BorderThickness = GImNodes.Style.NodeBorderThickness;

        // ImGui.SetCursorPos sets the cursor position, local to the current widget
        // (in this case, the child object started in BeginNodeEditor). Use
        // ImGui.SetCursorScreenPos to set the screen space coordinates directly.
        ImGui.SetCursorPos(GridSpaceToEditorSpace(editor, GetNodeTitleBarOrigin(node)));

        DrawListAddNode(node_idx);
        DrawListActivateCurrentNodeForeground();

        ImGui.PushID(node.Id);
        ImGui.BeginGroup();
    }

    void EndNode()
    {
        assert(GImNodes.CurrentScope == ImNodesScope_Node);
        GImNodes.CurrentScope = ImNodesScope_Editor;

        ImNodesEditorContext & editor = EditorContextGet();

        // The node's rectangle depends on the ImGui UI group size.
        ImGui.EndGroup();
        ImGui.PopID();

        ImNodeData & node = editor.Nodes.Pool[GImNodes.CurrentNodeIdx];
        node.Rect = GetItemRect();
        node.Rect.Expand(node.LayoutStyle.Padding);

        editor.GridContentBounds.Add(node.Origin);
        editor.GridContentBounds.Add(node.Origin + node.Rect.GetSize());

        if (node.Rect.Contains(GImNodes.MousePos))
        {
            GImNodes.NodeIndicesOverlappingWithMouse.push_back(GImNodes.CurrentNodeIdx);
        }
    }

    Vector2 GetNodeDimensions(int node_id)
    {
        ImNodesEditorContext & editor = EditorContextGet();
        const int node_idx = ObjectPoolFind(editor.Nodes, node_id);
        assert(node_idx != -1); // invalid node_id
        const ImNodeData&node = editor.Nodes.Pool[node_idx];
        return node.Rect.GetSize();
    }

    void BeginNodeTitleBar()
    {
        assert(GImNodes.CurrentScope == ImNodesScope_Node);
        ImGui.BeginGroup();
    }

    void EndNodeTitleBar()
    {
        assert(GImNodes.CurrentScope == ImNodesScope_Node);
        ImGui.EndGroup();

        ImNodesEditorContext & editor = EditorContextGet();
        ImNodeData & node = editor.Nodes.Pool[GImNodes.CurrentNodeIdx];
        node.TitleBarContentRect = GetItemRect();

        ImGui.ItemAdd(GetNodeTitleRect(node), ImGui.GetID("title_bar"));

        ImGui.SetCursorPos(GridSpaceToEditorSpace(editor, GetNodeContentOrigin(node)));
    }

    void BeginInputAttribute(const int id, const ImNodesPinShape shape)
    {
        BeginPinAttribute(id, ImNodesAttributeType_Input, shape, GImNodes.CurrentNodeIdx);
    }

    void EndInputAttribute() { EndPinAttribute(); }

    void BeginOutputAttribute(const int id, const ImNodesPinShape shape)
    {
        BeginPinAttribute(id, ImNodesAttributeType_Output, shape, GImNodes.CurrentNodeIdx);
    }

    void EndOutputAttribute() { EndPinAttribute(); }

    void BeginStaticAttribute(const int id)
    {
        // Make sure to call BeginNode() before calling BeginAttribute()
        assert(GImNodes.CurrentScope == ImNodesScope_Node);
        GImNodes.CurrentScope = ImNodesScope_Attribute;

        GImNodes.CurrentAttributeId = id;

        ImGui.BeginGroup();
        ImGui.PushID(id);
    }

    void EndStaticAttribute()
    {
        // Make sure to call BeginNode() before calling BeginAttribute()
        assert(GImNodes.CurrentScope == ImNodesScope_Attribute);
        GImNodes.CurrentScope = ImNodesScope_Node;

        ImGui.PopID();
        ImGui.EndGroup();

        if (ImGui.IsItemActive())
        {
            GImNodes.ActiveAttribute = true;
            GImNodes.ActiveAttributeId = GImNodes.CurrentAttributeId;
        }
    }

    void PushAttributeFlag(const ImNodesAttributeFlags flag)
    {
        GImNodes.CurrentAttributeFlags |= flag;
        GImNodes.AttributeFlagStack.push_back(GImNodes.CurrentAttributeFlags);
    }

    void PopAttributeFlag()
    {
        // PopAttributeFlag called without a matching PushAttributeFlag!
        // The bottom value is always the default value, pushed in Initialize().
        assert(GImNodes.AttributeFlagStack.size() > 1);

        GImNodes.AttributeFlagStack.pop_back();
        GImNodes.CurrentAttributeFlags = GImNodes.AttributeFlagStack.back();
    }

    void Link(const int id, const int start_attr_id, const int end_attr_id)
    {
        assert(GImNodes.CurrentScope == ImNodesScope_Editor);

        ImNodesEditorContext & editor = EditorContextGet();
        ImLinkData & link = ObjectPoolFindOrCreateObject(editor.Links, id);
        link.Id = id;
        link.StartPinIdx = ObjectPoolFindOrCreateIndex(editor.Pins, start_attr_id);
        link.EndPinIdx = ObjectPoolFindOrCreateIndex(editor.Pins, end_attr_id);
        link.ColorStyle.Base = GImNodes.Style.Colors[ImNodesCol_Link];
        link.ColorStyle.Hovered = GImNodes.Style.Colors[ImNodesCol_LinkHovered];
        link.ColorStyle.Selected = GImNodes.Style.Colors[ImNodesCol_LinkSelected];

        // Check if this link was created by the current link event
        if ((editor.ClickInteraction.Type == ImNodesClickInteractionType.LinkCreation &&
             editor.Pins.Pool[link.EndPinIdx].Flags & ImNodesAttributeFlags_EnableLinkCreationOnSnap &&
             editor.ClickInteraction.LinkCreation.StartPinIdx == link.StartPinIdx &&
             editor.ClickInteraction.LinkCreation.EndPinIdx == link.EndPinIdx) ||
            (editor.ClickInteraction.LinkCreation.StartPinIdx == link.EndPinIdx &&
             editor.ClickInteraction.LinkCreation.EndPinIdx == link.StartPinIdx))
        {
            GImNodes.SnapLinkIdx = ObjectPoolFindOrCreateIndex(editor.Links, id);
        }
    }

    void PushColorStyle(const ImNodesCol item, unsigned int color)
    {
        GImNodes.ColorModifierStack.push_back(ImNodesColElement(GImNodes.Style.Colors[item], item));
        GImNodes.Style.Colors[item] = color;
    }

    void PopColorStyle()
    {
        assert(GImNodes.ColorModifierStack.size() > 0);
        const ImNodesColElement elem = GImNodes.ColorModifierStack.back();
        GImNodes.Style.Colors[elem.Item] = elem.Color;
        GImNodes.ColorModifierStack.pop_back();
    }

    struct ImNodesStyleVarInfo
    {
        ImGuiDataType Type;
        ImU32 Count;
        ImU32 Offset;
        void* GetVarPtr(ImNodesStyle* style) const { return (void*) ((unsigned char*)style + Offset); }
};

static const ImNodesStyleVarInfo GStyleVarInfo[] = {
    // ImNodesStyleVar_GridSpacing
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, GridSpacing)},
    // ImNodesStyleVar_NodeCornerRounding
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, NodeCornerRounding)},
    // ImNodesStyleVar_NodePadding
    {ImGuiDataType_Float, 2, (ImU32)IM_OFFSETOF(ImNodesStyle, NodePadding)},
    // ImNodesStyleVar_NodeBorderThickness
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, NodeBorderThickness)},
    // ImNodesStyleVar_LinkThickness
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, LinkThickness)},
    // ImNodesStyleVar_LinkLineSegmentsPerLength
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, LinkLineSegmentsPerLength)},
    // ImNodesStyleVar_LinkHoverDistance
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, LinkHoverDistance)},
    // ImNodesStyleVar_PinCircleRadius
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, PinCircleRadius)},
    // ImNodesStyleVar_PinQuadSideLength
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, PinQuadSideLength)},
    // ImNodesStyleVar_PinTriangleSideLength
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, PinTriangleSideLength)},
    // ImNodesStyleVar_PinLineThickness
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, PinLineThickness)},
    // ImNodesStyleVar_PinHoverRadius
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, PinHoverRadius)},
    // ImNodesStyleVar_PinOffset
    {ImGuiDataType_Float, 1, (ImU32)IM_OFFSETOF(ImNodesStyle, PinOffset)},
    // ImNodesStyleVar_MiniMapPadding
    {ImGuiDataType_Float, 2, (ImU32)IM_OFFSETOF(ImNodesStyle, MiniMapPadding)},
    // ImNodesStyleVar_MiniMapOffset
    {ImGuiDataType_Float, 2, (ImU32)IM_OFFSETOF(ImNodesStyle, MiniMapOffset)},
};

static const ImNodesStyleVarInfo* GetStyleVarInfo(ImNodesStyleVar idx)
{
    IM_ASSERT(idx >= 0 && idx < ImNodesStyleVar_COUNT);
    IM_ASSERT(IM_ARRAYSIZE(GStyleVarInfo) == ImNodesStyleVar_COUNT);
    return &GStyleVarInfo[idx];
}

void PushStyleVar(const ImNodesStyleVar item, const float value)
{
    const ImNodesStyleVarInfo* var_info = GetStyleVarInfo(item);
    if (var_info->Type == ImGuiDataType_Float && var_info->Count == 1)
    {
        float&style_var = *(float*)var_info->GetVarPtr(&GImNodes.Style);
        GImNodes.StyleModifierStack.push_back(ImNodesStyleVarElement(item, style_var));
        style_var = value;
        return;
    }
    IM_ASSERT(0 && "Called PushStyleVar() float variant but variable is not a float!");
}

void PushStyleVar(const ImNodesStyleVar item, Vector2 value)
{
    const ImNodesStyleVarInfo* var_info = GetStyleVarInfo(item);
    if (var_info->Type == ImGuiDataType_Float && var_info->Count == 2)
    {
        Vector2 & style_var = *(Vector2*)var_info->GetVarPtr(&GImNodes.Style);
        GImNodes.StyleModifierStack.push_back(ImNodesStyleVarElement(item, style_var));
        style_var = value;
        return;
    }
    IM_ASSERT(0 && "Called PushStyleVar() Vector2 variant but variable is not a Vector2!");
}

void PopStyleVar(int count)
{
    while (count > 0)
    {
        assert(GImNodes.StyleModifierStack.size() > 0);
        const ImNodesStyleVarElement style_backup = GImNodes.StyleModifierStack.back();
        GImNodes.StyleModifierStack.pop_back();
        const ImNodesStyleVarInfo* var_info = GetStyleVarInfo(style_backup.Item);
        void* style_var = var_info->GetVarPtr(&GImNodes.Style);
        if (var_info->Type == ImGuiDataType_Float && var_info->Count == 1)
        {
            ((float*)style_var)[0] = style_backup.FloatValue[0];
        }
        else if (var_info->Type == ImGuiDataType_Float && var_info->Count == 2)
        {
            ((float*)style_var)[0] = style_backup.FloatValue[0];
            ((float*)style_var)[1] = style_backup.FloatValue[1];
        }
        count--;
    }
}

void SetNodeScreenSpacePos(const int node_id, Vector2 screen_space_pos)
{
    ImNodesEditorContext & editor = EditorContextGet();
    ImNodeData & node = ObjectPoolFindOrCreateObject(editor.Nodes, node_id);
    node.Origin = ScreenSpaceToGridSpace(editor, screen_space_pos);
}

void SetNodeEditorSpacePos(const int node_id, Vector2 editor_space_pos)
{
    ImNodesEditorContext & editor = EditorContextGet();
    ImNodeData & node = ObjectPoolFindOrCreateObject(editor.Nodes, node_id);
    node.Origin = EditorSpaceToGridSpace(editor, editor_space_pos);
}

void SetNodeGridSpacePos(const int node_id, Vector2 grid_pos)
{
    ImNodesEditorContext & editor = EditorContextGet();
    ImNodeData & node = ObjectPoolFindOrCreateObject(editor.Nodes, node_id);
    node.Origin = grid_pos;
}

void SetNodeDraggable(const int node_id, const bool draggable)
{
    ImNodesEditorContext & editor = EditorContextGet();
    ImNodeData & node = ObjectPoolFindOrCreateObject(editor.Nodes, node_id);
    node.Draggable = draggable;
}

Vector2 GetNodeScreenSpacePos(const int node_id)
{
    ImNodesEditorContext & editor = EditorContextGet();
    const int node_idx = ObjectPoolFind(editor.Nodes, node_id);
    assert(node_idx != -1);
    ImNodeData & node = editor.Nodes.Pool[node_idx];
    return GridSpaceToScreenSpace(editor, node.Origin);
}

Vector2 GetNodeEditorSpacePos(const int node_id)
{
    ImNodesEditorContext & editor = EditorContextGet();
    const int node_idx = ObjectPoolFind(editor.Nodes, node_id);
    assert(node_idx != -1);
    ImNodeData & node = editor.Nodes.Pool[node_idx];
    return GridSpaceToEditorSpace(editor, node.Origin);
}

Vector2 GetNodeGridSpacePos(const int node_id)
{
    ImNodesEditorContext & editor = EditorContextGet();
    const int node_idx = ObjectPoolFind(editor.Nodes, node_id);
    assert(node_idx != -1);
    ImNodeData & node = editor.Nodes.Pool[node_idx];
    return node.Origin;
}

bool IsEditorHovered() { return MouseInCanvas(); }

bool IsNodeHovered(int* const node_id)
{
    assert(GImNodes.CurrentScope == ImNodesScope_None);
    assert(node_id != NULL);

    const bool is_hovered = GImNodes.HoveredNodeIdx.HasValue();
    if (is_hovered)
    {
        const ImNodesEditorContexteditor = EditorContextGet();
        *node_id = editor.Nodes.Pool[GImNodes.HoveredNodeIdx.Value()].Id;
    }
    return is_hovered;
}

bool IsLinkHovered(int* const link_id)
{
    assert(GImNodes.CurrentScope == ImNodesScope_None);
    assert(link_id != NULL);

    const bool is_hovered = GImNodes.HoveredLinkIdx.HasValue();
    if (is_hovered)
    {
        const ImNodesEditorContexteditor = EditorContextGet();
        *link_id = editor.Links.Pool[GImNodes.HoveredLinkIdx.Value()].Id;
    }
    return is_hovered;
}

bool IsPinHovered(int* const attr)
{
    assert(GImNodes.CurrentScope == ImNodesScope_None);
    assert(attr != NULL);

    const bool is_hovered = GImNodes.HoveredPinIdx.HasValue();
    if (is_hovered)
    {
        const ImNodesEditorContexteditor = EditorContextGet();
        *attr = editor.Pins.Pool[GImNodes.HoveredPinIdx.Value()].Id;
    }
    return is_hovered;
}

int NumSelectedNodes()
{
    assert(GImNodes.CurrentScope == ImNodesScope_None);
    const ImNodesEditorContexteditor = EditorContextGet();
    return editor.SelectedNodeIndices.size();
}

int NumSelectedLinks()
{
    assert(GImNodes.CurrentScope == ImNodesScope_None);
    const ImNodesEditorContexteditor = EditorContextGet();
    return editor.SelectedLinkIndices.size();
}

void GetSelectedNodes(int* node_ids)
{
    assert(node_ids != NULL);

    const ImNodesEditorContexteditor = EditorContextGet();
    for (int i = 0; i < editor.SelectedNodeIndices.size(); ++i)
    {
        const int node_idx = editor.SelectedNodeIndices[i];
        node_ids[i] = editor.Nodes.Pool[node_idx].Id;
    }
}

void GetSelectedLinks(int* link_ids)
{
    assert(link_ids != NULL);

    const ImNodesEditorContexteditor = EditorContextGet();
    for (int i = 0; i < editor.SelectedLinkIndices.size(); ++i)
    {
        const int link_idx = editor.SelectedLinkIndices[i];
        link_ids[i] = editor.Links.Pool[link_idx].Id;
    }
}

void ClearNodeSelection()
{
    ImNodesEditorContext & editor = EditorContextGet();
    editor.SelectedNodeIndices.clear();
}

void ClearNodeSelection(int node_id)
{
    ImNodesEditorContext & editor = EditorContextGet();
    ClearObjectSelection(editor.Nodes, editor.SelectedNodeIndices, node_id);
}

void ClearLinkSelection()
{
    ImNodesEditorContext & editor = EditorContextGet();
    editor.SelectedLinkIndices.clear();
}

void ClearLinkSelection(int link_id)
{
    ImNodesEditorContext & editor = EditorContextGet();
    ClearObjectSelection(editor.Links, editor.SelectedLinkIndices, link_id);
}

void SelectNode(int node_id)
{
    ImNodesEditorContext & editor = EditorContextGet();
    SelectObject(editor.Nodes, editor.SelectedNodeIndices, node_id);
}

void SelectLink(int link_id)
{
    ImNodesEditorContext & editor = EditorContextGet();
    SelectObject(editor.Links, editor.SelectedLinkIndices, link_id);
}

bool IsNodeSelected(int node_id)
{
    ImNodesEditorContext & editor = EditorContextGet();
    return IsObjectSelected(editor.Nodes, editor.SelectedNodeIndices, node_id);
}

bool IsLinkSelected(int link_id)
{
    ImNodesEditorContext & editor = EditorContextGet();
    return IsObjectSelected(editor.Links, editor.SelectedLinkIndices, link_id);
}

bool IsAttributeActive()
{
    assert((GImNodes.CurrentScope & ImNodesScope_Node) != 0);

    if (!GImNodes.ActiveAttribute)
    {
        return false;
    }

    return GImNodes.ActiveAttributeId == GImNodes.CurrentAttributeId;
}

bool IsAnyAttributeActive(int* const attribute_id)
{
    assert((GImNodes.CurrentScope & (ImNodesScope_Node | ImNodesScope_Attribute)) == 0);

    if (!GImNodes.ActiveAttribute)
    {
        return false;
    }

    if (attribute_id != NULL)
    {
        *attribute_id = GImNodes.ActiveAttributeId;
    }

    return true;
}

bool IsLinkStarted(int* const started_at_id)
{
    // Call this function after EndNodeEditor()!
    assert(GImNodes.CurrentScope == ImNodesScope_None);
    assert(started_at_id != NULL);

    const bool is_started = (GImNodes.ImNodesUIState & ImNodesUIState_LinkStarted) != 0;
    if (is_started)
    {
        const ImNodesEditorContexteditor = EditorContextGet();
        const int pin_idx = editor.ClickInteraction.LinkCreation.StartPinIdx;
        *started_at_id = editor.Pins.Pool[pin_idx].Id;
    }

    return is_started;
}

bool IsLinkDropped(int* const started_at_id, const bool including_detached_links)
{
    // Call this function after EndNodeEditor()!
    assert(GImNodes.CurrentScope == ImNodesScope_None);

    const ImNodesEditorContexteditor = EditorContextGet();

    const bool link_dropped =
        (GImNodes.ImNodesUIState & ImNodesUIState_LinkDropped) != 0 &&
        (including_detached_links ||
         editor.ClickInteraction.LinkCreation.Type != ImNodesLinkCreationType_FromDetach);

    if (link_dropped && started_at_id)
    {
        const int pin_idx = editor.ClickInteraction.LinkCreation.StartPinIdx;
        *started_at_id = editor.Pins.Pool[pin_idx].Id;
    }

    return link_dropped;
}

bool IsLinkCreated(
    int* const  started_at_pin_id,
    int* const  ended_at_pin_id,
    bool* const created_from_snap)
{
    assert(GImNodes.CurrentScope == ImNodesScope_None);
    assert(started_at_pin_id != NULL);
    assert(ended_at_pin_id != NULL);

    const bool is_created = (GImNodes.ImNodesUIState & ImNodesUIState_LinkCreated) != 0;

    if (is_created)
    {
        const ImNodesEditorContexteditor = EditorContextGet();
        const int start_idx = editor.ClickInteraction.LinkCreation.StartPinIdx;
        const int end_idx = editor.ClickInteraction.LinkCreation.EndPinIdx.Value();
        const ImPinData&start_pin = editor.Pins.Pool[start_idx];
        const ImPinData&end_pin = editor.Pins.Pool[end_idx];

        if (start_pin.Type == ImNodesAttributeType_Output)
        {
            *started_at_pin_id = start_pin.Id;
            *ended_at_pin_id = end_pin.Id;
        }
        else
        {
            *started_at_pin_id = end_pin.Id;
            *ended_at_pin_id = start_pin.Id;
        }

        if (created_from_snap)
        {
            *created_from_snap =
                editor.ClickInteraction.Type == ImNodesClickInteractionType.LinkCreation;
        }
    }

    return is_created;
}

bool IsLinkCreated(
    int* started_at_node_id,
    int* started_at_pin_id,
    int* ended_at_node_id,
    int* ended_at_pin_id,
    bool* created_from_snap)
{
    assert(GImNodes.CurrentScope == ImNodesScope_None);
    assert(started_at_node_id != NULL);
    assert(started_at_pin_id != NULL);
    assert(ended_at_node_id != NULL);
    assert(ended_at_pin_id != NULL);

    const bool is_created = (GImNodes.ImNodesUIState & ImNodesUIState_LinkCreated) != 0;

    if (is_created)
    {
        const ImNodesEditorContexteditor = EditorContextGet();
        const int start_idx = editor.ClickInteraction.LinkCreation.StartPinIdx;
        const int end_idx = editor.ClickInteraction.LinkCreation.EndPinIdx.Value();
        const ImPinData&start_pin = editor.Pins.Pool[start_idx];
        const ImNodeData&start_node = editor.Nodes.Pool[start_pin.ParentNodeIdx];
        const ImPinData&end_pin = editor.Pins.Pool[end_idx];
        const ImNodeData&end_node = editor.Nodes.Pool[end_pin.ParentNodeIdx];

        if (start_pin.Type == ImNodesAttributeType_Output)
        {
            *started_at_pin_id = start_pin.Id;
            *started_at_node_id = start_node.Id;
            *ended_at_pin_id = end_pin.Id;
            *ended_at_node_id = end_node.Id;
        }
        else
        {
            *started_at_pin_id = end_pin.Id;
            *started_at_node_id = end_node.Id;
            *ended_at_pin_id = start_pin.Id;
            *ended_at_node_id = start_node.Id;
        }

        if (created_from_snap)
        {
            *created_from_snap =
                editor.ClickInteraction.Type == ImNodesClickInteractionType.LinkCreation;
        }
    }

    return is_created;
}

bool IsLinkDestroyed(int* const link_id)
{
    assert(GImNodes.CurrentScope == ImNodesScope_None);

    const bool link_destroyed = GImNodes.DeletedLinkIdx.HasValue();
    if (link_destroyed)
    {
        const ImNodesEditorContexteditor = EditorContextGet();
        const int link_idx = GImNodes.DeletedLinkIdx.Value();
        *link_id = editor.Links.Pool[link_idx].Id;
    }

    return link_destroyed;
}

namespace
{
    void NodeLineHandler(ImNodesEditorContext editor, const char* const line)
    {
        int id;
        int x, y;
        if (sscanf(line, "[node.%i", &id) == 1)
        {
            const int node_idx = ObjectPoolFindOrCreateIndex(editor.Nodes, id);
            GImNodes.CurrentNodeIdx = node_idx;
            ImNodeData & node = editor.Nodes.Pool[node_idx];
            node.Id = id;
        }
        else if (sscanf(line, "origin=%i,%i", &x, &y) == 2)
        {
            ImNodeData & node = editor.Nodes.Pool[GImNodes.CurrentNodeIdx];
            node.Origin = Vector2((float)x, (float)y);
        }
    }

    void EditorLineHandler(ImNodesEditorContext editor, const char* const line)
    {
        (void)sscanf(line, "panning=%f,%f", &editor.Panning.X, &editor.Panning.Y);
    }
} // namespace

const char* SaveCurrentEditorStateToIniString(size_t * const data_size)
{
    return SaveEditorStateToIniString(&EditorContextGet(), data_size);
}

const char* SaveEditorStateToIniString(
    const ImNodesEditorContext * const editor_ptr,
    size_t * const                     data_size)
{
    assert(editor_ptr != NULL);
    const ImNodesEditorContexteditor = *editor_ptr;

    GImNodes.TextBuffer.clear();
    // TODO: check to make sure that the estimate is the upper bound of element
    GImNodes.TextBuffer.reserve(64 * editor.Nodes.Pool.size());

    GImNodes.TextBuffer.appendf(
        "[editor]\npanning=%i,%i\n", (int)editor.Panning.X, (int)editor.Panning.Y);

    for (int i = 0; i < editor.Nodes.Pool.size(); i++)
    {
        if (editor.Nodes.InUse[i])
        {
            const ImNodeData&node = editor.Nodes.Pool[i];
            GImNodes.TextBuffer.appendf("\n[node.%d]\n", node.Id);
            GImNodes.TextBuffer.appendf("origin=%i,%i\n", (int)node.Origin.X, (int)node.Origin.Y);
        }
    }

    if (data_size != NULL)
    {
        *data_size = GImNodes.TextBuffer.size();
    }

    return GImNodes.TextBuffer.c_str();
}

void LoadCurrentEditorStateFromIniString(const char* const data, const size_t data_size)
{
    LoadEditorStateFromIniString(&EditorContextGet(), data, data_size);
}

void LoadEditorStateFromIniString(
    ImNodesEditorContext* const editor_ptr,
    const char* const           data,
    const size_t data_size)
{
    if (data_size == 0u)
    {
        return;
    }

    ImNodesEditorContext & editor = editor_ptr == NULL ? EditorContextGet() : *editor_ptr;

    char* buf = (char*)ImGui.MemAlloc(data_size + 1);
    const char* buf_end = buf + data_size;
    memcpy(buf, data, data_size);
    buf[data_size] = 0;

    void(*line_handler)(ImNodesEditorContext &, const char*);
line_handler = NULL;
char* line_end = NULL;
for (char* line = buf; line < buf_end; line = line_end + 1)
{
    while (*line == '\n' || *line == '\r')
    {
        line++;
    }
    line_end = line;
    while (line_end < buf_end && *line_end != '\n' && *line_end != '\r')
    {
        line_end++;
    }
    line_end[0] = 0;

    if (*line == ';' || *line == '\0')
    {
        continue;
    }

    if (line[0] == '[' && line_end[-1] == ']')
    {
        line_end[-1] = 0;
        if (strncmp(line + 1, "node", 4) == 0)
        {
            line_handler = NodeLineHandler;
        }
        else if (strcmp(line + 1, "editor") == 0)
        {
            line_handler = EditorLineHandler;
        }
    }

    if (line_handler != NULL)
    {
        line_handler(editor, line);
    }
}
ImGui.MemFree(buf);
}

void SaveCurrentEditorStateToIniFile(const char* const file_name)
{
    SaveEditorStateToIniFile(&EditorContextGet(), file_name);
}

void SaveEditorStateToIniFile(const ImNodesEditorContext* const editor, const char* const file_name)
{
    size_t data_size = 0u;
    const char* data = SaveEditorStateToIniString(editor, &data_size);
    FILE* file = ImFileOpen(file_name, "wt");
    if (!file)
    {
        return;
    }

    fwrite(data, sizeof(char), data_size, file);
    fclose(file);
}

void LoadCurrentEditorStateFromIniFile(const char* const file_name)
{
    LoadEditorStateFromIniFile(&EditorContextGet(), file_name);
}

void LoadEditorStateFromIniFile(ImNodesEditorContext* const editor, const char* const file_name)
{
    size_t data_size = 0u;
    char* file_data = (char*)ImFileLoadToMemory(file_name, "rb", &data_size);

    if (!file_data)
    {
        return;
    }

    LoadEditorStateFromIniString(editor, file_data, data_size);
    ImGui.MemFree(file_data);
}
} // namespace IMNODES_NAMESPACE
    }
}
