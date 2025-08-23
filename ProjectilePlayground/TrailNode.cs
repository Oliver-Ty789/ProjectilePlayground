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

        // public
        public float height;
        public float time;
        public float range;

        public TrailNode(SpriteFont font, float height, float time, float range, Texture2D texture, Vector2 position, float scale) : base(texture, position, scale)
        {
            this.font = font;
            this.height = height;
            this.time = time;
            this.range = range;
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, SpriteBatch spriteBatch)
        {
            var colour = Color.White;

            if (isHovering) // display properties as well
            {
                colour = Color.Gray;
                spriteBatch.Draw(texture, Rect, colour);
                var x = position.X + 30;
                // height
                var yheight = position.Y - 30;
                spriteBatch.DrawString(font, $"height: {height}", new Vector2(x, yheight), penColour);
                // time
                var ytime = position.Y;
                spriteBatch.DrawString(font, $"time: {time}", new Vector2(x, ytime), penColour);
                // range
                var yrange = position.Y + 30;
                spriteBatch.DrawString(font, $"range: {range}", new Vector2(x, yrange), penColour);
            }

            else
            {
                spriteBatch.Draw(texture, Rect, colour);
            }

                base.Draw(gameTime, spriteBatch);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, Environment environment)
        {

            currentMouse = Mouse.GetState();
            var mouseRect = new Rectangle(currentMouse.X, currentMouse.Y, 1, 1);

            if (mouseRect.Intersects(Rect))
            {
                isHovering = true;
            }
            else
            {
                isHovering = false;
            }

                base.Update(gameTime, environment);
        }
    }
}
