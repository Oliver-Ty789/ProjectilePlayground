using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace ProjectilePlayground.Content.controls
{
    internal class TextBox : ScaledSprite
    {
        // private
        private int length;
        private SpriteFont font;
        private KeyboardState keyboardState;
        private Vector2 currentCursorPosition;
       
        // public
        public string currentText { get; set; }
        public int animationTime { get; set; }

        public TextBox(Texture2D texture, Vector2 position, float scale, string currentText, SpriteFont font) : base(texture, position, scale)
        {
            length = 5;
            animationTime = 0;
            this.currentText = currentText;
            currentCursorPosition = new Vector2(position.X + (font.MeasureString(currentText)).X, position.Y);
            this.font = font;
        }
        
        // for checking if flasingCursor should be drawn 
        public bool IsFlashingCursorDrawn()
        {
            int time = animationTime % 60;
            if (time >= 0 && time <= 30)
            {
                return true;
            }
            else
                return false;
        }

        public string AddMoreText(char text)
        {
            Vector2 spacing = new Vector2();
            keyboardState = Keyboard.GetState();

            if (text == '\b') // if entered key is a backspace
            {
                if (currentText.Length > 0)
                {
                    spacing = font.MeasureString(currentText.Substring(currentText.Length - 1));

                    currentText = currentText.Remove(currentText.Length - 1, 1);
                    currentCursorPosition.X -= spacing.X; // moves cursor back
                }
            }

            else
            {
                if (currentText.Length < length)
                {
                    currentText += text;
                    spacing = font.MeasureString(text.ToString());
                    currentCursorPosition.X += spacing.X; // moves cursor forward
                }
            }
            return currentText;


        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            spriteBatch.DrawString(font, currentText, position, Color.Black);

            if (IsFlashingCursorDrawn())
            {
                Rectangle cursorRect = new Rectangle((int)currentCursorPosition.X, (int)currentCursorPosition.Y, (int)(texture.Width * scale), (int)(texture.Height * scale));
                spriteBatch.Draw(texture, cursorRect, Color.White);
            }

            base.Draw(gameTime, spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment environment)
        {
            // increments by 60 every second
            animationTime++;
            base.Update(gameTime, environment);
        }
    }
}
