using System;
using System.Collections.Generic;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Clipper;
using SharpDX;
using Color = System.Drawing.Color;
using static EnsoulSharp.SDK.Clipper.Clipper;
using Path = System.Collections.Generic.List<EnsoulSharp.SDK.Clipper.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<EnsoulSharp.SDK.Clipper.IntPoint>>;
using GamePath = System.Collections.Generic.List<SharpDX.Vector2>;
namespace Challenger_Series.Utils
{
    /// <summary>
    /// Class that contains the geometry related methods.
    /// </summary>
    public static class Geometry
    {
        private const int CircleLineSegmentN = 22;

        public static bool IsOutside(this Vector3 point, Geometry.Polygon poly)
        {
            var p = new IntPoint(point.X, point.Y);
            return Clipper.PointInPolygon(p, poly.ToClipperPath()) != 1;
        }

        public static bool IsOutside(this Vector2 point, Polygon poly)
        {
            var p = new IntPoint(point.X, point.Y);
            return Clipper.PointInPolygon(p, poly.ToClipperPath()) != 1;
        }

        public static Vector3 SwitchYZ(this Vector3 v)
        {
            return new Vector3(v.X, v.Z, v.Y);
        }

        //Clipper
        public static List<Polygon> ToPolygons(this Paths v)
        {
            var result = new List<Polygon>();
            foreach (var path in v)
            {
                result.Add(path.ToPolygon());
            }
            return result;
        }

        /// <summary>
        /// Returns the position on the path after t milliseconds at speed speed.
        /// </summary>
        public static Vector2 PositionAfter(this GamePath self, int t, int speed, int delay = 0)
        {
            var distance = Math.Max(0, t - delay) * speed / 1000;
            for (var i = 0; i <= self.Count - 2; i++)
            {
                var from = self[i];
                var to = self[i + 1];
                var d = (int)to.Distance(from);
                if (d > distance)
                {
                    return from + distance * (to - from).Normalized();
                }
                distance -= d;
            }
            return self[self.Count - 1];
        }


        public static class Util
        {
            public static void DrawLineInWorld(Vector3 start, Vector3 end, int width, Color color)
            {
                var from = Drawing.WorldToScreen(start);
                var to = Drawing.WorldToScreen(end);
                Drawing.DrawLine(from[0], from[1], to[0], to[1], width, color);
            }
        }

        public static Polygon ToPolygon(this Path v)
        {
            var polygon = new Polygon();
            foreach (var point in v)
            {
                polygon.Add(new Vector2(point.X, point.Y));
            }
            return polygon;
        }

        public static Paths ClipPolygons(List<Polygon> polygons)
        {
            var subj = new Paths(polygons.Count);
            var clip = new Paths(polygons.Count);
            foreach (var polygon in polygons)
            {
                subj.Add(polygon.ToClipperPath());
                clip.Add(polygon.ToClipperPath());
            }
            var solution = new Paths();
            var c = new Clipper();
            c.AddPaths(subj, PolyType.ptSubject, true);
            c.AddPaths(clip, PolyType.ptClip, true);
            c.Execute(ClipType.ctUnion, solution, PolyFillType.pftPositive, PolyFillType.pftEvenOdd);
            return solution;
        }

        public class Circle
        {
            public Vector2 Center;
            public float Radius;

            public Circle(Vector2 center, float radius)
            {
                Center = center;
                Radius = radius;
            }

            public Polygon ToPolygon(int offset = 0, float overrideWidth = -1)
            {
                var result = new Polygon();
                var outRadius = (overrideWidth > 0
                    ? overrideWidth
                    : (offset + Radius) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentN));
                for (var i = 1; i <= CircleLineSegmentN; i++)
                {
                    var angle = i * 2 * Math.PI / CircleLineSegmentN;
                    var point = new Vector2(
                        Center.X + outRadius * (float)Math.Cos(angle), Center.Y + outRadius * (float)Math.Sin(angle));
                    result.Add(point);
                }
                return result;
            }
        }

        public class Polygon
        {
            public List<Vector2> Points = new List<Vector2>();

            public void Add(Vector2 point)
            {
                Points.Add(point);
            }

            public Path ToClipperPath()
            {
                var result = new Path(Points.Count);
                foreach (var point in Points)
                {
                    result.Add(new IntPoint(point.X, point.Y));
                }
                return result;
            }

            public bool IsOutside(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, ToClipperPath()) != 1;
            }

            public void Draw(Color color, int width = 1)
            {
                for (var i = 0; i <= Points.Count - 1; i++)
                {
                    var nextIndex = (Points.Count - 1 == i) ? 0 : (i + 1);
                    Util.DrawLineInWorld(Points[i].ToVector3(), Points[nextIndex].ToVector3(), width, color);
                }
            }
        }


        /// <summary>
        /// Represents a rectangle polygon.
        /// </summary>
        public class Rectangle : Polygon
        {
            /// <summary>
            /// Gets the direction.
            /// </summary>
            /// <value>
            /// The direction.
            /// </value>
            public Vector2 Direction { get { return (End - Start).Normalized(); } }

            /// <summary>
            /// Gets the perpendicular.
            /// </summary>
            /// <value>
            /// The perpendicular.
            /// </value>
            public Vector2 Perpendicular { get { return Direction.Perpendicular(); } }

            /// <summary>
            /// The end
            /// </summary>
            public Vector2 End;

            /// <summary>
            /// The start
            /// </summary>
            public Vector2 Start;

            /// <summary>
            /// The width
            /// </summary>
            public float Width;

            /// <summary>
            /// Initializes a new instance of the <see cref="Rectangle"/> class.
            /// </summary>
            /// <param name="start">The start.</param>
            /// <param name="end">The end.</param>
            /// <param name="width">The width.</param>
            public Rectangle(Vector3 start, Vector3 end, float width) : this(start.ToVector2(), end.ToVector2(), width) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="Rectangle"/> class.
            /// </summary>
            /// <param name="start">The start.</param>
            /// <param name="end">The end.</param>
            /// <param name="width">The width.</param>
            public Rectangle(Vector2 start, Vector2 end, float width)
            {
                Start = start;
                End = end;
                Width = width;
                UpdatePolygon();
            }

            /// <summary>
            /// Updates the polygon.
            /// </summary>
            /// <param name="offset">The offset.</param>
            /// <param name="overrideWidth">Width of the override.</param>
            public void UpdatePolygon(int offset = 0, float overrideWidth = -1)
            {
                Points.Clear();
                Points.Add(
                    Start + (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular - offset * Direction);
                Points.Add(
                    Start - (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular - offset * Direction);
                Points.Add(
                    End - (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular + offset * Direction);
                Points.Add(
                    End + (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular + offset * Direction);
            }
        }

        public class Ring
        {
            public Vector2 Center;
            public float Radius;
            public float RingRadius; //actually radius width.

            public Ring(Vector2 center, float radius, float ringRadius)
            {
                Center = center;
                Radius = radius;
                RingRadius = ringRadius;
            }

            public Polygon ToPolygon(int offset = 0)
            {
                var result = new Polygon();
                var outRadius = (offset + Radius + RingRadius) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentN);
                var innerRadius = Radius - RingRadius - offset;
                for (var i = 0; i <= CircleLineSegmentN; i++)
                {
                    var angle = i * 2 * Math.PI / CircleLineSegmentN;
                    var point = new Vector2(
                        Center.X - outRadius * (float)Math.Cos(angle), Center.Y - outRadius * (float)Math.Sin(angle));
                    result.Add(point);
                }
                for (var i = 0; i <= CircleLineSegmentN; i++)
                {
                    var angle = i * 2 * Math.PI / CircleLineSegmentN;
                    var point = new Vector2(
                        Center.X + innerRadius * (float)Math.Cos(angle),
                        Center.Y - innerRadius * (float)Math.Sin(angle));
                    result.Add(point);
                }
                return result;
            }
        }

        public class Sector
        {
            public float Angle;
            public Vector2 Center;
            public Vector2 Direction;
            public float Radius;

            public Sector(Vector2 center, Vector2 direction, float angle, float radius)
            {
                Center = center;
                Direction = direction;
                Angle = angle;
                Radius = radius;
            }

            public Polygon ToPolygon(int offset = 0)
            {
                var result = new Polygon();
                var outRadius = (Radius + offset) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentN);
                result.Add(Center);
                var Side1 = Direction.Rotated(-Angle * 0.5f);
                for (var i = 0; i <= CircleLineSegmentN; i++)
                {
                    var cDirection = Side1.Rotated(i * Angle / CircleLineSegmentN).Normalized();
                    result.Add(new Vector2(Center.X + outRadius * cDirection.X, Center.Y + outRadius * cDirection.Y));
                }
                return result;
            }
        }
    }
}