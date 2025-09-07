using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading.Tasks;


namespace ProjectilePlayground
{
    internal struct VerticesRectangle
    {
        public Vector2[] vertices;
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
        public float Width;
        public float Height;
        public float X;
        public float Y;
        
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
            Right = position.X + (width* scale);
            Top = position.Y;
            Bottom = position.Y + (height*scale);
            Width = width * scale;
            Height = height * scale; 
            X = position.X;
            Y = position.Y;
        }
    }
}
