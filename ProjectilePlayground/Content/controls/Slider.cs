using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectilePlayground.Content.controls
{
    internal class Slider : ScaledSprite
    {
        // slider is a special type of button that we are going to manipulate to choose specific values for our projectile
        // need scrollertexture, its own rect, 

        // private
        private MouseState currentMouse;
        private MouseState previousMouse;
        private KeyboardState currentKey;
        private KeyboardState previousKey;
        private SpriteFont font;
        private bool isHovering;
        private bool isDragging;
        private bool isTexting;
        private Texture2D barTexture;
        private Vector2 barPosition;
        private TextBox textBox;
        private Texture2D cursorTexture;
        private int timeBeforeNextDelete;

        // public
        public event EventHandler<SliderClickEventArgs> Click;
        public List<char> _chars;
        public bool isClicked { get; private set; }
        public Color penColour { get; set; }
        public float property {  get; set; }
        public int index { get; set; }
        public float maxValue { get; set; }
        public string text_scroller { get; set; }
        public string text_min {  get; set; }
        public string text_max { get; set; }
        public string text_desc { get; set; }

        public Rectangle BarRect
        {
            get
            {
                return new Rectangle((int)barPosition.X,
                    (int)barPosition.Y,
                    (int)(barTexture.Width * scale),
                    (int)(barTexture.Height * scale));
            }
        }

        public Rectangle PropertyRect
        {
            get
            {
                return new Rectangle(
                    (int)(Rect.X + (Rect.Width / 2) - (font.MeasureString(text_scroller).X / 2)),
                    (int)(Rect.Y + (Rect.Height / 2) - (font.MeasureString(text_scroller).Y / 2) + 30),
                    (int)(font.MeasureString(text_scroller).X * scale),
                    (int)(font.MeasureString(text_scroller).Y * scale));
            }
        }

        public Slider(Texture2D texture, Vector2 position, float scale, SpriteFont font, Texture2D barTexture , Texture2D cursorTexture) : base(texture, position, scale)
        {
            this.barTexture = barTexture;
            this.font = font;
            this.cursorTexture = cursorTexture;
            penColour = Color.Black;
            barPosition = new Vector2(position.X - 30, position.Y);
            _chars = new List<char>();
        }

        private bool IsDecimalPoint()
        {
            foreach (var letter in text_scroller)
            {
                if (letter == '.')
                    return true;
            }
            return false;
        }

        private Vector2 ScrollerMovement(Vector2 position)
        {
            // moves scoller to cusors X pos
          
            float offset = currentMouse.X - previousMouse.X;

            // try stop gittering

            if (position.X == BarRect.Left && offset < 0f)
                return position;
            else if (position.X == (BarRect.Right-40) && offset > 0f)
                return position;

            else
            {
                // stays within bar
                if (Rect.Left < BarRect.Left)
                    return position = new Vector2(BarRect.Left, position.Y);
                else if (Rect.Right > BarRect.Right)
                    return position = new Vector2(BarRect.Right - 40, position.Y);

                else
                    return position = new Vector2(position.X + offset, position.Y);
            }
            
        }

        public void PropertyPlacement(float value) // move scroller to parameter
        {
            // find how far along the scroller should be due to value & maxValue

            float percentageBar = value / maxValue;

            // find available space on bar
            float space = (BarRect.Right - (BarRect.Left+ 40));

            position = new Vector2((space * percentageBar)  + BarRect.Left, position.Y);
        }

        private float FindProperty()
        {

            // find how far along the scroller is on the bar then calcuate its selected value
            float wholeBar = BarRect.Right - (BarRect.Left + 40);
            float scrollerBar = position.X - BarRect.Left;

            float pencentageBar = scrollerBar / wholeBar;

            text_scroller = $"{Math.Round(pencentageBar * maxValue, 1)}";
            return pencentageBar * maxValue;
        }

        private void HandleInput()
        {
            Keys[] keys = currentKey.GetPressedKeys();
            String value = String.Empty;

            if (currentKey.IsKeyUp(Keys.Back) && currentKey.IsKeyUp(Keys.Delete)) // adds delay to not delete text too quickly
            {
                timeBeforeNextDelete = 10;
            }

            if (keys.Count() > 0)
            {
                if (currentKey.IsKeyDown(Keys.Back) || currentKey.IsKeyDown(Keys.Delete)) // deletes text
                {
                    if (timeBeforeNextDelete == 0) // ready for next delete
                    {
                        timeBeforeNextDelete = 10;
                    }

                    if (timeBeforeNextDelete == 10)
                    {
                        text_scroller = textBox.AddMoreText('\b'); 
                    }

                    timeBeforeNextDelete--; // allows 1 delete every 1/6 of a second
                    return;
                }
                

                if (((int)keys[0] >= 48 && (int)keys[0] <= 57) || ((int)keys[0] >= 96 && (int)keys[0] <= 105)) // only allows top row/ num pad keys
                {
                    value = keys[0].ToString().Substring(keys[0].ToString().Length - 1); // strips inputted char of formatting so that only number is passed through

                    if (previousKey.IsKeyUp(keys[0]) && currentKey.IsKeyDown(keys[0])) // check if just pressed
                    {
                        text_scroller = textBox.AddMoreText(value.ToCharArray()[0]);
                    }

                }
                if (keys[0] == Keys.OemPeriod && !IsDecimalPoint()) // check if decimal point and no other decimal points already there
                {
                    if (previousKey.IsKeyUp(keys[0]) && currentKey.IsKeyDown(keys[0])) // check if just pressed
                        textBox.AddMoreText('.');
                }
            }   
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            var colour = Color.White;
             
            if (isHovering)
                colour = Color.Gray;

            spriteBatch.Draw(barTexture, BarRect, Color.White);
            spriteBatch.Draw(texture, Rect, colour);

            // all text stuff

            if (!string.IsNullOrEmpty(text_scroller) && !isTexting) // make sure writable text doesn't overlap
            {
                var x = (Rect.X + (Rect.Width / 2)) - (font.MeasureString(text_scroller).X / 2);
                var y = (Rect.Y + (Rect.Height / 2)) - (font.MeasureString(text_scroller).Y / 2) + 30;

                spriteBatch.DrawString(font, text_scroller, new Vector2(x, y), penColour);
            }
            if (!string.IsNullOrEmpty(text_min))
            {
                var x = BarRect.Left - (font.MeasureString(text_scroller).X / 2) - 40;
                var y = BarRect.Y + (font.MeasureString(text_scroller).Y / 2);

                spriteBatch.DrawString(font, text_min, new Vector2(x, y), penColour);
            }
            if (!string.IsNullOrEmpty(text_max))
            {
                var x = BarRect.Right + (font.MeasureString(text_scroller).X / 2) + 20;
                var y = BarRect.Y + ((font.MeasureString(text_scroller).Y / 2) );

                spriteBatch.DrawString(font, text_max, new Vector2(x, y), penColour);
            }
            if (!string.IsNullOrEmpty(text_desc))
            {
                var x = (BarRect.Left - (font.MeasureString(text_scroller).X / 2)) - 100;
                var y = BarRect.Y + (font.MeasureString(text_scroller).Y / 2);

                spriteBatch.DrawString(font, text_desc, new Vector2(x, y), penColour);
            }

            if (isTexting)
                textBox.Draw(gameTime, spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment E)
        {

            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();
            previousKey = currentKey;
            currentKey = Keyboard.GetState();

            var mouseRect = new Rectangle(currentMouse.X, currentMouse.Y, 1, 1);

            isHovering = false;

            // checking if mouse is hovering and or clicking the button

            if (mouseRect.Intersects(Rect) || isDragging)
            {
                isHovering = true;

                if (currentMouse.LeftButton == ButtonState.Pressed) 
                {
                    
                    // used the change in postion between the current and previous mousestates to determine how far to move the scroller

                    position = ScrollerMovement(position);
                    isDragging = true;
                }

                // for setting values

                else if ((currentMouse.LeftButton == ButtonState.Released) && (previousMouse.LeftButton == ButtonState.Pressed))
                {
                    Click?.Invoke(this, new SliderClickEventArgs(property, index));
                    isDragging = false;
                }
            }

            // for text inputs
            if (mouseRect.Intersects(PropertyRect) && (currentMouse.LeftButton == ButtonState.Released) && (previousMouse.LeftButton == ButtonState.Pressed))
            {
                isTexting = true;
                var x = (Rect.X + (Rect.Width / 2)) - (font.MeasureString(text_scroller).X / 2);
                var y = (Rect.Y + (Rect.Height / 2)) - (font.MeasureString(text_scroller).Y / 2) + 30;

                textBox = new TextBox(cursorTexture, new Vector2(x,y), 1f, text_scroller, font);
            }
            // check if any keys have been first pressed
            if (previousKey.GetPressedKeyCount() == 0 && currentKey.GetPressedKeyCount() == 1 && isTexting)
            { 
                if (currentKey.GetPressedKeys()[0] == Keys.Enter) // if enter has been clicked, set property 
                {
                    isTexting = false;
                    
                    // need to check if value is appropriate
                    float value = Convert.ToSingle(text_scroller);
                    Console.WriteLine(value);
                    if (value <= maxValue)
                    {
                        PropertyPlacement(value);
                        property = value;
                        Click?.Invoke(this, new SliderClickEventArgs(property, index));
                    }
                }
            }


            if (isTexting)
            {
                textBox.Update(gameTime, E);
                HandleInput();
            }



            else
                property = FindProperty();

            base.Update(gameTime, E);
        } 
    }
}
