using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectilePlayground
{
    internal class TrailNode : ScaledSprite
    {
        // store properties, be put into a list, be interactable, be lightweight to reduce latency and not overload the program

        // private
        private SpriteFont font;
        private MouseState currentMouse;
        private Color penColour;
        private bool isHovering;
        private Color colour;
        private VerticesRectangle _collisionRect;
        private Texture2D boxTexture;

        // public
        public float height;
        public float time;
        public float range;
        public bool isYellow;

        public TrailNode(SpriteFont font, float height, float time, float range, Texture2D texture, Vector2 position, float scale, bool isYellow, Texture2D boxTexture) : base(texture, position, scale)
        {
            this.font = font;
            this.height = height;
            this.time = time;
            this.range = range;
            this.isYellow = isYellow;
            this.penColour = Color.Black;
            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
            CollisionRect = _collisionRect;
            this.boxTexture = boxTexture;
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (isYellow)
            {
                colour = Color.Yellow;
            }
            else
            {
                colour = Color.White;
            }

            if (isHovering) // display properties as well
            {
                colour = Color.Gray;


                // for the background box
                var xbox = position.X - 120;
                var ybox = position.Y - 70;
                spriteBatch.Draw(boxTexture, new Vector2(xbox, ybox), Color.White);


                spriteBatch.Draw(texture, DrawingRect, colour);
                var x = position.X - 90;
                // height
                var yheight = position.Y - 30;
                spriteBatch.DrawString(font, $"height: {Math.Round(height, 2)}m", new Vector2(x, yheight), penColour);
                // time
                var ytime = position.Y;
                spriteBatch.DrawString(font, $"time: {Math.Round(time, 2)}s", new Vector2(x, ytime), penColour);
                // range
                var yrange = position.Y + 30;
                spriteBatch.DrawString(font, $"range: {Math.Round(range, 2)}m", new Vector2(x, yrange), penColour);

                

            }

            else
            {
                spriteBatch.Draw(texture, DrawingRect, colour);
            }

            

               
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, Environment environment, Camera2D camera)
        {

            currentMouse = Mouse.GetState();

            Vector2 scaledOffset = new(currentMouse.X, currentMouse.Y);
            scaledOffset = Vector2.Transform(scaledOffset, camera.camInverseMatrix);

            var mouseRect = new VerticesRectangle(scaledOffset, 1, 1, 1);

            if (Collisions.IntersectingPolygons(mouseRect.Center, mouseRect.vertices, CollisionRect.Center, CollisionRect.vertices, out Vector2 normal, out float depth))
            {
                isHovering = true;
            }
            else
            {
                isHovering = false;
            }

                base.Update(gameTime, environment, camera);
        }
    }
}
