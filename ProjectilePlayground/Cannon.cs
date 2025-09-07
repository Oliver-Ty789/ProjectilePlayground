using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectilePlayground.Content.controls;

namespace ProjectilePlayground
{
    internal class Cannon : Slider // create a vector 
    {

        // private
        private Vector2 wheelPosition;
        private float rotationR;
        private Vector2 textureOrigin;
        //private float rotationD;

        public Cannon(Texture2D texture, Vector2 position, float scale, Texture2D barTexture, SpriteFont font, Texture2D cursorTexture, bool isCannon) : base (texture, position, scale, font, barTexture, cursorTexture, isCannon)
        {
            wheelPosition = new Vector2(position.X - 10, position.Y + 10);
            rotationR = 0;
            textureOrigin = new Vector2(20,25);
        }

        private void CannonOrientation() 
        {
            // use change in rotation around sprite to change orientation

            var offsetCurrentMousePos = new Vector2(currentMouse.X - position.X, currentMouse.Y - position.Y);
            var offsetPreviousMousePos = new Vector2(previousMouse.X - position.X, previousMouse.Y - position.Y);
            var differenceMousePos = new Vector2(currentMouse.X - previousMouse.X, currentMouse.Y -  previousMouse.Y);

            // find magnitude of vectors
            var offsetCurrentMouseMag = MathF.Sqrt(MathF.Pow(offsetCurrentMousePos.X, 2) + MathF.Pow(offsetCurrentMousePos.Y, 2));
            var offsetPreviousMouseMag = MathF.Sqrt(MathF.Pow(offsetPreviousMousePos.X, 2) + MathF.Pow(offsetPreviousMousePos.Y, 2));
            var differenceMouseMag = MathF.Sqrt(MathF.Pow(differenceMousePos.X, 2) + MathF.Pow(differenceMousePos.Y, 2));
            
            if (offsetCurrentMousePos.Y > offsetPreviousMousePos.Y) // make sure cannon turns with mouse
                rotationR += MathF.Acos((MathF.Pow(offsetCurrentMouseMag, 2) + MathF.Pow(offsetPreviousMouseMag, 2) - MathF.Pow(differenceMouseMag, 2)) / (2 * offsetCurrentMouseMag * offsetPreviousMouseMag));
            else
                rotationR -= MathF.Acos((MathF.Pow(offsetCurrentMouseMag, 2) + MathF.Pow(offsetPreviousMouseMag, 2) - MathF.Pow(differenceMouseMag, 2)) / (2 * offsetCurrentMouseMag * offsetPreviousMouseMag));
            if (rotationR < -MathF.PI/2) rotationR = -MathF.PI/2;
            if (rotationR > 0) rotationR = 0;
        }

        public override void PropertyPlacement(float value)
        {
            // just change rotation
            rotationR = -(value * (MathF.PI/180));
        }

        public override float FindProperty()
        {
            // just convert to degrees
            
            text_scroller = $"{Math.Round(-(rotationR / (MathF.PI / 180)),1)}";
            return -(rotationR / (MathF.PI / 180));
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            var colour = Color.White;

            if (isHovering)
                colour = Color.Gray;

            spriteBatch.Draw(texture, DrawingRect, SourceRect, colour, rotationR, textureOrigin, SpriteEffects.None, 0f);
            spriteBatch.Draw(barTexture, BarRect, Color.White);
            // all text stuff

            if (!string.IsNullOrEmpty(text_scroller) && !isTexting) // make sure writable text doesn't overlap
            {
                var x = (CollisionRect.X + (CollisionRect.Width / 2)) - (font.MeasureString(text_scroller).X / 2);
                var y = (CollisionRect.Y + (CollisionRect.Height / 2)) - (font.MeasureString(text_scroller).Y / 2) + 30;

                spriteBatch.DrawString(font, text_scroller, new Vector2(x, y), penColour);
            }

            if (isTexting)
                textBox.Draw(gameTime, spriteBatch);
        }




        public override void Update(GameTime gameTime, Environment environment)
        {
            if (isDragging)
            {
                CannonOrientation();
            }
            base.Update(gameTime, environment);
        }
    }
}
