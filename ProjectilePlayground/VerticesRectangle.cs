using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static System.Formats.Asn1.AsnWriter;


namespace ProjectilePlayground
{
    internal class VerticesRectangle
    {
        public Vector2[] vertices
        {
            get;
            set;
        }
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
        public float Width;
        public float Height;
        public float X;
        public float Y;
        public Vector2 Center;

        public VerticesRectangle(Vector2 position, float width, float height, float scale)
        {
            vertices = new Vector2[4]
            {
                position,
                position + new Vector2(width * scale, 0),
                position + new Vector2(width, height)*scale,
                position + new Vector2(0, height * scale)
            };

            Left = position.X;
            Right = position.X + (width * scale);
            Top = position.Y;
            Bottom = position.Y + (height * scale);
            Width = width * scale;
            Height = height * scale;
            X = position.X;
            Y = position.Y;
            Center = new Vector2(width * scale / 2, height * scale / 2);
        }

        
        public static VerticesRectangle GetTransformedRectangle(VerticesRectangle rectangle, float angle, Vector2 translation, Vector2 pivot, float scale)

        {

            // move vertices around a pivot & translate them


            // transformed vertices to return
            VerticesRectangle transformedRectangle = new(new Vector2(rectangle.Left, rectangle.Top), rectangle.Width, rectangle.Height, 1f);
            Vector2[] transformedVerts = new Vector2[rectangle.vertices.Length];

            for (int i = 0; i < rectangle.vertices.Length; i++)

            {

                // Get base Mesh vertex in Local space
                //Console.Write($"num {i} vert from {rectangle.vertices[i]} at pivot {pivot}");

                // make pivot the origin
                Matrix translateToOrigin = Matrix.CreateTranslation(-pivot.X, -pivot.Y, 0);
                Matrix translateToPivot = Matrix.CreateTranslation(pivot.X, pivot.Y, 0);

                var vert = Vector2.Transform(rectangle.vertices[i], translateToOrigin);
               // Console.Write($"num {i} vert translated to local space: {vert}");

                /* matrix model,

                   Would require Vector3 with z set to 1

                   would require 3x3 matrix

                   [  MathF.Cos(angle) -MathF.Sin(angle)  Position.x ]

                   [  MathF.Sin(angle) MathF.Cos(angle)   Position.y ]

                   [  0 		    0               1            ]

                   rotatedVert=vertTorate*Matrix;

           */


                // matrix declerations

                Matrix scaleMatrix = Matrix.CreateScale(scale, scale, 1f);
                Matrix rotationMatrix = Matrix.CreateRotationZ(angle);
                Matrix translationMatrix = Matrix.CreateTranslation(translation.X, translation.Y, 0f);
                

                // combining all transformations into one matrix
                Matrix transformationMatrix =  rotationMatrix * translationMatrix * scaleMatrix;
                    

                // applying transformation to transformable vert placeholder
                vert = Vector2.Transform(vert, transformationMatrix);

               // Console.WriteLine($"num {i} vert transformed in local space: {vert}");

                // get vert base mesh back in global space
                vert = Vector2.Transform(vert, translateToPivot);

                // Write to output mesh

                transformedVerts[i] = vert;
                transformedRectangle.vertices[i] = vert;
                
            }

            //VerticesRectangle transformedRectangle = new(transformedVerts[0], VectorMaths.Length(transformedVerts[0] - transformedVerts[1]), VectorMaths.Length(transformedVerts[1] - transformedVerts[2]), 1f);

            return transformedRectangle;

        }

        public static Vector2 GetTransformedCircle(Vector2 center, Vector2 pivot, float angle, Vector2 translation, float scale)
        { 
            // move vertices around a pivot & translate them


            
            // make pivot the origin
            Matrix translateToOrigin = Matrix.CreateTranslation(-pivot.X, -pivot.Y, 0);
            Matrix translateToPivot = Matrix.CreateTranslation(pivot.X, pivot.Y, 0);

            var translatedCenter = Vector2.Transform(center, translateToOrigin);
           
            // matrix declerations

            Matrix scaleMatrix = Matrix.CreateScale(scale, scale, 1f);
            Matrix rotationMatrix = Matrix.CreateRotationZ(angle);
            Matrix translationMatrix = Matrix.CreateTranslation(translation.X, translation.Y, 0f);


            // combining all transformations into one matrix
            Matrix transformationMatrix = rotationMatrix * translationMatrix;
            

            // applying transformation to transformable vert placeholder
            translatedCenter = Vector2.Transform(translatedCenter, transformationMatrix);
        

                   
            // get translatedCenter base mesh back in global space
            translatedCenter = Vector2.Transform(translatedCenter, translateToPivot);

            // Write to output mesh
            return translatedCenter;

        }
    }
}


//public static VerticesRectangle GetCameraRectangle(VerticesRectangle rectangle, float angle, Vector2 position, VEctor2 pivot, Vector2 CameraPosition, Vector2 CameraScale)

//        {

//            // move vertices around a pivot

//            VerticesRectangle rotatedTriangle = new VerticesRectangle();

//            for (int i = 0; i < rectangle.vertices.Length; i++)

//            {

//                // add temp rectangle to return

//                // Get base Mesh vertex

//                var vertToRotate = rectangle.vertices[i] + pivot;

//                // Create temporary holder

//                var RotatedVert = new Vector2();

//                // Apply rotation

//                RotatedVert.x = vertToRotate.X * MathF.Cos(angle) - vertToRotate.Y * MathF.Sin(angle);

//                RotatedVert.y = vertToRotate.X * MathF.Sin(angle) + vertToRotate.Y * MathF.Cos(angle);


//                rotatedVert = rotatedVert - Pivot.


//                // Apply translation

//                RotatedVert += position;


//                /* Could be replaced by matrix,

//                    Would require Vector3 with z set to 1

//                    would require 3x3 matrix

//                    [  MathF.Cos(angle) -MathF.Sin(angle)  Position.x ]

//                    [  MathF.Sin(angle) MathF.Cos(angle)   Position.y ]

//                    [  0 		    0               1            ]

//                    rotatedVert=vertTorate*Matrix;

//            */

//                var CameraVert = new Vector2();


//                // Get position Relative to Camera

//                CameraVert = (RotatedVert - CameraPosition);

//                // Apply Scaling

//                CameraVert *= CameraScale;

//                /*

//                    [ CamerScale.x  0  -CameraPosition.x]

//                    [ 0  CamerScale.y  -CameraPosition.x]

//                    [ 0  	0  	1	]

//                */

//                // Write to output mesh

//                rotatedTriangle.vertices[i] = CameraVert;

//            }

//            return rectangle;

//        }
//    }
