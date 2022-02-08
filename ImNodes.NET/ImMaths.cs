using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImNodes.NET
{
    public struct ImRect
    {
       public Vector2 Min;    // Upper-left
       public Vector2 Max;    // Lower-right

       public static readonly ImRect Default = new ImRect(new Vector2(float.MaxValue, float.MaxValue), new Vector2(-float.MaxValue, -float.MaxValue));
       public ImRect(Vector2 min, Vector2 max) { Min = min; Max = max; }
       public ImRect(Vector4 v) : this( new Vector2(v.X, v.Y), new Vector2(v.Z, v.W)) { }
       public ImRect(float x1, float y1, float x2, float y2) : this(new Vector2(x1, y1), new Vector2(x2, y2)) { }

       public Vector2 GetCenter() => new Vector2((Min.X+Max.X)*0.5f, (Min.Y+Max.Y)*0.5f);
       public Vector2 GetSize() => new Vector2(Max.X-Min.X, Max.Y-Min.Y);
       public float GetWidth() => Max.X-Min.X;
       float GetHeight() => Max.Y-Min.Y;
       public Vector2 GetTL() => Min;                   // Top-left
       public Vector2 GetTR() => new Vector2(Max.X, Min.Y);  // Top-right
       public Vector2 GetBL() => new Vector2(Min.X, Max.Y);  // Bottom-left
       public Vector2 GetBR() => Max;                   // Bottom-right
       public bool Contains( Vector2 p) => p.X >= Min.X && p.Y >= Min.Y  && p.X < Max.X && p.Y < Max.Y;
       public bool Contains( ImRect r) => r.Min.X >= Min.X && r.Min.Y >= Min.Y && r.Max.X < Max.X && r.Max.Y < Max.Y;
       public bool Overlaps( ImRect r) => r.Min.Y < Max.Y  && r.Max.Y > Min.Y  && r.Min.X < Max.X && r.Max.X > Min.X;
       public void Add(Vector2 rhs) { if (Min.X > rhs.X) Min.X = rhs.X; if (Min.Y > rhs.Y) Min.Y = rhs.Y; if (Max.X < rhs.X) Max.X = rhs.X; if (Max.Y < rhs.Y) Max.Y = rhs.Y; }
       public void Add(ImRect rhs) { if (Min.X > rhs.Min.X) Min.X = rhs.Min.X; if (Min.Y > rhs.Min.Y) Min.Y = rhs.Min.Y; if (Max.X < rhs.Max.X) Max.X = rhs.Max.X; if (Max.Y < rhs.Max.Y) Max.Y = rhs.Max.Y; }
       public void Expand(float amount) { Min.X -= amount; Min.Y -= amount; Max.X += amount; Max.Y += amount; }
       public void Expand( Vector2 amount) { Min.X -= amount.X; Min.Y -= amount.Y; Max.X += amount.X; Max.Y += amount.Y; }
       public void Reduce( Vector2 amount) { Min.X += amount.X; Min.Y += amount.Y; Max.X -= amount.X; Max.Y -= amount.Y; }
       public void Clip( ImRect clip) { if (Min.X < clip.Min.X) Min.X = clip.Min.X; if (Min.Y < clip.Min.Y) Min.Y = clip.Min.Y; if (Max.X > clip.Max.X) Max.X = clip.Max.X; if (Max.Y > clip.Max.Y) Max.Y = clip.Max.Y; }
       public void Floor() { Min.X = (float)(int)Min.X; Min.Y = (float)(int)Min.Y; Max.X = (float)(int)Max.X; Max.Y = (float)(int)Max.Y; }
       public Vector2 GetClosestPoint(Vector2 p, bool on_edge)
       {
            if (!on_edge && Contains(p))
               return p;
            if (p.X > Max.X) p.X = Max.X;
            else if (p.X < Min.X) p.X = Min.X;
            if (p.Y > Max.Y) p.Y = Max.Y;
            else if (p.Y < Min.Y) p.Y = Min.Y;
            return p;
        }
   };
}
