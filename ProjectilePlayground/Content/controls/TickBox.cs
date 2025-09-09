using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectilePlayground.Content.controls
{
    internal class TickBox : Button
    {
        // private
        private Texture2D tickTexture;
        private VerticesRectangle _collisionRect;


        public TickBox (Texture2D texture, Vector2 position, float scale, SpriteFont font, int index, Texture2D tickTexture) : base(texture, position, scale, font, index)
        {
            this.tickTexture = tickTexture;
            this.isTicked = false;
            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
            CollisionRect = _collisionRect;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            if (isTicked)
            {
                spriteBatch.Draw(tickTexture, DrawingRect, colour);
            }
            if (!string.IsNullOrEmpty(text))
            {
                var x = (CollisionRect.X + (CollisionRect.Width)) ;
                var y = (CollisionRect.Y + (CollisionRect.Height/2) - font.MeasureString(text).Y/2);

                spriteBatch.DrawString(font, text, new Vector2(x, y), penColour);
            }
        }

        public override void Update(GameTime gameTime, Environment E)
        {
            base.Update(gameTime, E);
        }
    }
}
