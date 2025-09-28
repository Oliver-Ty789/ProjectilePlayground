using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Design;

namespace ProjectilePlayground
{
    internal static class Collisions 
    {
        public static bool IntersectingPolygons(Vector2[] verticesA, Vector2[] verticesB, out Vector2 normal) // finds egde and normal to see if polygons overlap in an axis
        {
            normal = Vector2.Zero;
            float depth = float.MaxValue;
            for (int i = 0; i< verticesA.Length; i++)
            {
                Vector2 va = verticesA[i];
                Vector2 vb = verticesA[(i + 1) % verticesA.Length];

                Vector2 edge = va - vb;
                Vector2 axis = new Vector2(-edge.Y, edge.X); // finds the perpendicular vector to chosen edge

                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);
                

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                float axisDepth = MathF.Min(maxB - minA, maxA - minB); // to return perpendicular normal to both polygons
                if (axisDepth < depth)
                {
                    normal = axis;
                    depth = axisDepth;
                }

            }
            for (int i = 0; i < verticesB.Length; i++)
            {
                Vector2 va = verticesB[i];
                Vector2 vb = verticesB[(i + 1) % verticesB.Length];

                Vector2 edge = va - vb;
                Vector2 axis = new Vector2(-edge.Y, edge.X); // finds the perpendicular vector to chosen edge

                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    normal = axis;
                    depth = axisDepth;
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

        public static void ResolveCollisions(RigidBody bodyA, RigidBody bodyB, Vector2 normal)
        {
            
            var relativeVelocity = bodyA.linearVelocity - bodyB.linearVelocity;
            //Console.WriteLine(relativeVelocity);
            if (!bodyA.isCollisionResolved || !bodyB.isCollisionResolved) // if velocity is too low then too stop fazing through just keep bodies still
            {

                var j = 0f; // scalar quantity for impulse

                var e = MathF.Min(bodyA.restitution, bodyB.restitution);

                j = -(1 + e) * VectorMaths.DotProduct(normal, relativeVelocity) / VectorMaths.DotProduct(normal, normal * ((1 / bodyA.mass) + (1 / bodyB.mass)));

                bodyA.linearVelocity = bodyA.linearVelocity + (j / bodyA.mass) * normal;
                bodyB.linearVelocity = bodyB.linearVelocity - (j / bodyB.mass) * normal;
                return;
            }
            
        }




        /// maybe implement later
        //public static bool IntersectingCirclePolygon(Vector2 center, float radius, VerticesRectangle rectangle, Rectangle originalRect,  float angle)
        //{

        //    var rotatedCenter = VerticesRectangle.GetTransformedCircle(center, rectangle.Center, -angle, new Vector2(0, 0), 1f);


        //    var rectCenter = rectangle.Center;
        //    // checking left side

        //    // only ever need to check horizontal
        //    if (MathF.Abs(rectCenter.X - rotatedCenter.X) < MathF.Abs(rectCenter.X - (rotatedCenter.X + radius)))
        //    {

        //    }


        //    return true;
        //}
    }
}
