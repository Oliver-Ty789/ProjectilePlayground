using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectilePlayground
{
    internal static class Collisions
    {
        public static bool IntersectingPolygons(Vector2[] verticesA, Vector2[] verticesB) // finds egde and normal to see if polygons overlap in an axis
        {
            for (int i = 0; i< verticesA.Length; i++)
            {
                Vector2 va = verticesA[i];
                Vector2 vb = verticesA[(i + 1) % verticesA.Length];

                Vector2 edge = va - vb;
                Vector2 normal = new Vector2(-edge.Y, edge.X); // finds the perpendicular vector to chosen edge

                ProjectVertices(verticesA, normal, out float minA, out float maxA);
                ProjectVertices(verticesA, normal, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }
            }
            for (int i = 0; i < verticesB.Length; i++)
            {
                Vector2 va = verticesB[i];
                Vector2 vb = verticesB[(i + 1) % verticesB.Length];

                Vector2 edge = va - vb;
                Vector2 normal = new Vector2(-edge.Y, edge.X); // finds the perpendicular vector to chosen edge

                ProjectVertices(verticesA, normal, out float minA, out float maxA);
                ProjectVertices(verticesA, normal, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }
            }
            return true;
        }

        private static void ProjectVertices(Vector2[] vertices, Vector2 normal, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0;i < vertices.Length;i++)
            {
                Vector2 v = vertices[i];
                float projection = VectorMaths.DotProduct(v, normal);

                if (projection < min)
                {
                    min = projection;
                }
                if (projection > max)
                {
                    max = projection;
                }
            }


        }
    }
}
