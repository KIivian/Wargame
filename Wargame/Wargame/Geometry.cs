using System.Drawing;
using System;

namespace Wargame
{
    public static class Geometry
    {
        public static PointF[] GetHexCorners(PointF center, float hexRadius)
        {
            var corners = new PointF[6];
            for (var i = 0; i < 6; i++)
            {
                var angle_deg = 60 * i + 30;
                var angle_rad = Math.PI / 180 * angle_deg;
                corners[i].X = (float)(center.X + hexRadius * Math.Cos(angle_rad));
                corners[i].Y = (float)(center.Y + hexRadius * Math.Sin(angle_rad));
            }
            return corners;
        }
        public static double GetDistance(PointF p1, PointF p2) => Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
        public static PointF[] GetSquareCorners(PointF center, float hexRadius)
        {
            var halfHexWidth = (float)Math.Sqrt(0.75 * hexRadius * hexRadius);
            var size = (float)(1.0 / 2 * halfHexWidth);
            var result = new PointF[4];
            result[0] = new PointF(center.X - size, center.Y - size);
            result[1] = new PointF(center.X - size, center.Y + size);
            result[2] = new PointF(center.X + size, center.Y + size);
            result[3] = new PointF(center.X + size, center.Y - size);
            return result;
        }
        public static PointF[] GetRhombCorners(PointF center, float hexRadius)
        {
            var halfHexWidth = (float)Math.Sqrt(0.75 * hexRadius * hexRadius);
            var size = (float)(2.0 / 3 * halfHexWidth);
            var result = new PointF[4];
            result[0] = new PointF(center.X, center.Y - size);
            result[1] = new PointF(center.X - size, center.Y);
            result[2] = new PointF(center.X, center.Y + size);
            result[3] = new PointF(center.X + size, center.Y);
            return result;
        }
        public static PointF[] GetTriangleCorners(PointF center, bool isUpper, float hexRadius)
        {
            var halfHexWidth = (float)Math.Sqrt(0.75 * hexRadius * hexRadius);
            var result = new PointF[3];
            var size = (float)(1.3 / 2 * halfHexWidth);
            if (isUpper)
            {
                result[0] = new PointF(center.X, center.Y - size);
                result[1] = new PointF((float)(center.X - Math.Cos(60) * size), (float)(center.Y + size / 2));
                result[2] = new PointF((float)(center.X + Math.Cos(60) * size), (float)(center.Y + size / 2));
            }
            else
            {
                result[0] = new PointF(center.X, center.Y + size);
                result[1] = new PointF((float)(center.X - Math.Cos(60) * size), (float)(center.Y - size / 2));
                result[2] = new PointF((float)(center.X + Math.Cos(60) * size), (float)(center.Y - size / 2));
            }
            return result;
        }
        public static PointF[,] GetMapPoints(float hexRadius, Map map)
        {
            var halfHexWidth = (float)Math.Sqrt(0.75 * hexRadius * hexRadius);
            var mapPoints = new PointF[map.Width, map.Height];

            for (var i = 0; i < map.Width; i++)
            {
                for (var j = 0; j < map.Height; j++)
                {
                    mapPoints[i, j].X = hexRadius + halfHexWidth * i;
                    mapPoints[i, j].Y = hexRadius + hexRadius * 3 * j + 1.5f * hexRadius * (i % 2);
                }
            }
            return mapPoints;
        }
    }
}
