using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ImNodes.NET
{
    public partial class ImNodes
    {
        public enum ImNodesCol : int
        {
            NodeBackground = 0,
            NodeBackgroundHovered,
            NodeBackgroundSelected,
            NodeOutline,
            TitleBar,
            TitleBarHovered,
            TitleBarSelected,
            Link,
            LinkHovered,
            LinkSelected,
            Pin,
            PinHovered,
            BoxSelector,
            BoxSelectorOutline,
            GridBackground,
            GridLine,
            MiniMapBackground,
            MiniMapBackgroundHovered,
            MiniMapOutline,
            MiniMapOutlineHovered,
            MiniMapNodeBackground,
            MiniMapNodeBackgroundHovered,
            MiniMapNodeBackgroundSelected,
            MiniMapNodeOutline,
            MiniMapLink,
            MiniMapLinkSelected,
            MiniMapCanvas,
            MiniMapCanvasOutline,
            COUNT
        };

        public enum ImNodesStyleVar : int
        {
            GridSpacing = 0,
            NodeCornerRounding,
            NodePadding,
            NodeBorderThickness,
            LinkThickness,
            LinkLineSegmentsPerLength,
            LinkHoverDistance,
            PinCircleRadius,
            PinQuadSideLength,
            PinTriangleSideLength,
            PinLineThickness,
            PinHoverRadius,
            PinOffset,
            MiniMapPadding,
            MiniMapOffset,
            COUNT
        };

        public enum ImNodesStyleFlags : int
        {
            None = 0,
            NodeOutline = 1 << 0,
            GridLines = 1 << 2
        };

        public enum ImNodesPinShape : int
        {
            Circle,
            CircleFilled,
            Triangle,
            TriangleFilled,
            Quad,
            QuadFilled
        };

        // This enum controls the way the attribute pins behave.
        public enum ImNodesAttributeFlags : int
        {
            ImNodesAttributeFlags_None = 0,
            // Allow detaching a link by left-clicking and dragging the link at a pin it is connected to.
            // NOTE: the user has to actually delete the link for this to work. A deleted link can be
            // detected by calling IsLinkDestroyed() after EndNodeEditor().
            ImNodesAttributeFlags_EnableLinkDetachWithDragClick = 1 << 0,
            // Visual snapping of an in progress link will trigger IsLink Created/Destroyed events. Allows
            // for previewing the creation of a link while dragging it across attributes. See here for demo:
            // https://github.com/Nelarius/imnodes/issues/41#issuecomment-647132113 NOTE: the user has to
            // actually delete the link for this to work. A deleted link can be detected by calling
            // IsLinkDestroyed() after EndNodeEditor().
            ImNodesAttributeFlags_EnableLinkCreationOnSnap = 1 << 1
        };

        public class ImNodesIO
        {
            public unsafe class EmulateThreeButtonMouse_
            {
                // The keyboard modifier to use in combination with mouse left click to pan the editor view.
                // Set to NULL by default. To enable this feature, set the modifier to point to a boolean
                // indicating the state of a modifier. For example,
                //
                // ImNodes::GetIO().EmulateThreeButtonMouse.Modifier = &ImGui::GetIO().KeyAlt;
                public bool* Modifier = null;
            }
            public EmulateThreeButtonMouse_ EmulateThreeButtonMouse = new EmulateThreeButtonMouse_();

            public unsafe class LinkDetachWithModifierClick_
            {
                // Pointer to a boolean value indicating when the desired modifier is pressed. Set to NULL
                // by default. To enable the feature, set the modifier to point to a boolean indicating the
                // state of a modifier. For example,
                //
                // ImNodes::GetIO().LinkDetachWithModifierClick.Modifier = &ImGui::GetIO().KeyCtrl;
                //
                // Left-clicking a link with this modifier pressed will detach that link. NOTE: the user has
                // to actually delete the link for this to work. A deleted link can be detected by calling
                // IsLinkDestroyed() after EndNodeEditor().
                public bool* Modifier = null;
            }
            public LinkDetachWithModifierClick_ LinkDetachWithModifierClick = new LinkDetachWithModifierClick_();

            // Holding alt mouse button pans the node area, by default middle mouse button will be used
            // Set based on ImGuiMouseButton values
            public int AltMouseButton;

            // Panning speed when dragging an element and mouse is outside the main editor view.
            public float AutoPanningSpeed;
        };

        public class ImNodesStyle
        {
            public float GridSpacing;

            public float NodeCornerRounding;
            public Vector2 NodePadding;
            public float NodeBorderThickness;

            public float LinkThickness;
            public float LinkLineSegmentsPerLength;
            public float LinkHoverDistance;

            // The following variables control the look and behavior of the pins. The default size of each
            // pin shape is balanced to occupy approximately the same surface area on the screen.

            // The circle radius used when the pin shape is either ImNodesPinShape_Circle or
            // ImNodesPinShape_CircleFilled.
            public float PinCircleRadius;
            // The quad side length used when the shape is either ImNodesPinShape_Quad or
            // ImNodesPinShape_QuadFilled.
            public float PinQuadSideLength;
            // The equilateral triangle side length used when the pin shape is either
            // ImNodesPinShape_Triangle or ImNodesPinShape_TriangleFilled.
            public float PinTriangleSideLength;
            // The thickness of the line used when the pin shape is not filled.
            public float PinLineThickness;
            // The radius from the pin's center position inside of which it is detected as being hovered
            // over.
            public float PinHoverRadius;
            // Offsets the pins' positions from the edge of the node to the outside of the node.
            public float PinOffset;

            // Mini-map padding size between mini-map edge and mini-map content.
            public Vector2 MiniMapPadding;
            // Mini-map offset from the screen side.
            public Vector2 MiniMapOffset;

            // By default, ImNodesStyleFlags_NodeOutline and ImNodesStyleFlags_Gridlines are enabled.
            public ImNodesStyleFlags Flags;
            // Set these mid-frame using Push/PopColorStyle. You can index this color array with with a
            // ImNodesCol value.
            public uint[] Colors = new uint[(int)ImNodesCol.COUNT];
        };

        public enum ImNodesMiniMapLocation
        {
            ImNodesMiniMapLocation_BottomLeft,
            ImNodesMiniMapLocation_BottomRight,
            ImNodesMiniMapLocation_TopLeft,
            ImNodesMiniMapLocation_TopRight,
        };

        // Callback type used to specify special behavior when hovering a node in the minimap
        public delegate void ImNodesMiniMapNodeHoveringCallback(int node_id, object data);
    }
}
