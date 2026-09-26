using System.Numerics;

namespace AquaModelLibrary.Data.BluePoint.CTAL
{
    /// <summary>
    /// Texture Atlas entry. Each point is essentially a uv coordinate for a rectangle's vertices. They don't use a consistent order
    /// </summary>
    public struct CTALEntry
    {
        public CTALEntryHash hash;
        public Vector2 point0;
        public Vector2 point1;
        public Vector2 point2;
        public Vector2 point3;
        public int sliceNumber;

        /// <summary>
        /// Returns the top left point of the image as well as its width and height
        /// </summary>
        public void GetDimensionsAndLocation(int fullSourceWidth, int fullSourceHeight, out Vector2 topLeftPoint, out int width, out int height)
        {
            List<Vector2> points = new List<Vector2>() { point0, point1, point2, point3 };
            Vector2 minPoint = new Vector2(99999, 99999);
            Vector2 maxPoint = new Vector2();

            foreach(var point in points)
            {
                minPoint = Vector2.Min(minPoint, point);
                maxPoint = Vector2.Max(maxPoint, point);
            }
            topLeftPoint = new Vector2(fullSourceWidth * minPoint.X, fullSourceHeight * minPoint.Y);
            width = (int)(fullSourceWidth * (maxPoint.X - minPoint.X));
            height = (int)(fullSourceHeight * (maxPoint.Y - minPoint.Y));
        }
    }
}
