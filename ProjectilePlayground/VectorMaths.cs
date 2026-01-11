using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;

namespace ProjectilePlayground
{
    internal class VectorMaths
    {

        public static float Length(Vector2 vector)
        {
            return MathF.Sqrt((float)vector.X * (float)vector.X + (float)vector.Y * (float)vector.Y); // uses pythagoras to find length of vector from origin
        }

        public static Vector2 UnitVector(Vector2 vector)
        {
            float length = Length(vector);
            if (length == 0)
            {
                return Vector2.Zero; // Avoid division by zero
            }
            return new Vector2(vector.X / length, vector.Y / length); // returns a unit vector
        }

        public static Vector2 ToVector2(float length, float angle)
        {
            return new Vector2((float)(length * Math.Cos(angle * Math.PI / 180)), -(float)(length * Math.Sin(angle * Math.PI / 180)));
        }

        public static float DotProduct(Vector2 a, Vector2 b)
        {
            // I need a . b = |a||b|*cos(angle between them)
            // also the dot product is equal to i(a * b) + j(a * b), further maths coming in handy here

            return a.X * b.X + a.Y * b.Y;
        }

        public static float CrossProductArea(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - b.X * a.Y;
        }

        private static Vector2 CrossProduct(Vector2 a, Vector2 b)
        {
            
            return new (a.X * b.Y, -(a.Y * b.X));

        }

        public static bool NearlyEqual(float a, float b) // due to floating point accuracies, this is better than using != for boolean expressions
        {
            bool result = MathF.Abs(a - b) < 0.5f;
            return result;
        }

        public static bool NearlyEqual(Vector2 a, Vector2 b)
        {
            return NearlyEqual(a.X, b.X) && NearlyEqual(a.Y, b.Y);
        }
        
    }
}
