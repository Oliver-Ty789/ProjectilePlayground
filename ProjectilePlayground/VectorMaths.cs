using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;

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

        public static float DotProduct(Vector2 a, Vector2 b)
        {
            // I need a . b = |a||b|*cos(angle between them)
            // also the dot product is equal to i(a * b) + j(a * b), further maths coming in handy here

            return a.X * b.X + a.Y * b.Y;
        }
        
        //public static Vector2 CrossProduct(Vector2 a, Vector2 b)
        //{
        //    // a x b = Area * 
        //}
        
    }
}
