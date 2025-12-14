using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace ProjectilePlayground.Content.controls
{
    internal class TargetButton : Button
    {
        /// <summary>
        /// same functionality as a button class however with extra functionality once clicked
        /// 
        /// </summary>

        // private
        private MouseState currentMouse;
        
        
        // public 
        public Vector2 scaledOffset;
        public string texturePath;



        public TargetButton(Texture2D texture, Vector2 position, float scale, SpriteFont font, int index, string path) : base(texture, position, scale, font, index)
        {
            texturePath = path;
            
        }

        
        public override void Update(GameTime gameTime, Environment E, Camera2D camera)
        {

            if (isClicked)
            {
                currentMouse = Mouse.GetState();

                scaledOffset = new(currentMouse.X, currentMouse.Y);

                scaledOffset = Vector2.Transform(scaledOffset, camera.camInverseMatrix);

               
            }
            base.Update(gameTime, E, camera);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, SpriteBatch spriteBatchCamera)
        {
            // draw extra sprite if clicked

            if (isClicked)
            {
                // use camera sprite batch to make sure it is in right location/scale when placed
                var pivot = new Vector2(SourceRect.Width / 2f, SourceRect.Height / 2f);

                var offsetRect = new Rectangle(
                    (int)scaledOffset.X,
                    (int)scaledOffset.Y,
                    DrawingRect.Width,
                    DrawingRect.Height);

                spriteBatchCamera.Draw(texture, offsetRect, SourceRect, Color.White, 0f, pivot, SpriteEffects.None, 0f);
            }

            base.Draw(gameTime, spriteBatch, spriteBatchCamera);
        }
    }
}
