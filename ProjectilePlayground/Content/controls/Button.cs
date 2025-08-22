using System;
using System.Collections.Generic;

using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectilePlayground.Content.controls
{
    internal class Button : ScaledSprite
    {
        // get position, rectangle, is clicked state/bool, mouse states, text properties

        // private
        private MouseState currentMouse;
        private SpriteFont font;
        private bool isHovering;
        private MouseState previousMouse;

        // public
        public event EventHandler Click;
        public bool isClicked { get; private set; }
        public Color penColour { get; set; }
        public string text { get; set; }
       
        public Button (Texture2D texture, Vector2 position, float scale, SpriteFont font) : base (texture, position, scale)
        {
            this.font = font;
            penColour = Color.Black;
        }
        public override void Draw(GameTime gameTime ,SpriteBatch spriteBatch)
        {
            var colour = Color.White;

            if (isHovering)
                colour = Color.Gray;

            spriteBatch.Draw(texture, Rect, colour);

            if (!string.IsNullOrEmpty(text))
            {
                var x = (Rect.X + (Rect.Width / 2)) - (font.MeasureString(text).X / 2);
                var y = (Rect.Y + (Rect.Height / 2)) - (font.MeasureString(text).Y / 2);

                spriteBatch.DrawString(font, text, new Vector2 (x, y), penColour);
            }
        }

        public override void Update(GameTime gameTime, Environment E)
        {
            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();

            var mouseRect = new Rectangle(currentMouse.X, currentMouse.Y, 1, 1);

            isHovering = false;

            // checking if mouse is hovering and or clicking the button

            if (mouseRect.Intersects(Rect))
            {
                isHovering = true;

                if ((currentMouse.LeftButton == ButtonState.Released) && (previousMouse.LeftButton == ButtonState.Pressed))
                {
                    Click?.Invoke(this, new EventArgs());
                }
            }
            base.Update(gameTime, E);
        }



    }
}
