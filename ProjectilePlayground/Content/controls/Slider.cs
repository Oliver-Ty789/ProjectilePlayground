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
        private SpriteFont font;
        private bool isHovering;
        private bool isDragging;
        private MouseState previousMouse;
        private Texture2D barTexture;
        private Vector2 barPosition;

        // public
        public event EventHandler<SliderClickEventArgs> Click;
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

        public Slider(Texture2D texture, Vector2 position, float scale, SpriteFont font, Texture2D barTexture ) : base(texture, position, scale)
        {
            this.barTexture = barTexture;
            this.font = font;
            penColour = Color.Black;
            barPosition = new Vector2(position.X - 30, position.Y);
        }

        private Vector2 ScrollerMovement(Vector2 position, MouseState currentMouse, MouseState previousMouse)
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

        public float FindProperty(float maxValue)
        {

            // find how far along the scroller is on the bar then calcuate its selected value
            float wholeBar = BarRect.Right - (BarRect.Left + 40);
            float scrollerBar = position.X - BarRect.Left;

            float pencentageBar = scrollerBar / wholeBar;

            text_scroller = $"{Math.Round(pencentageBar * maxValue, 1)}";
            return pencentageBar * maxValue;
        }


        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime, SpriteBatch spriteBatch)
        {

            var colour = Color.White;
             
            if (isHovering)
                colour = Color.Gray;

            spriteBatch.Draw(barTexture, BarRect, Color.White);
            spriteBatch.Draw(texture, Rect, colour);

            // all text stuff

            if (!string.IsNullOrEmpty(text_scroller))
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
        }

        public override void Update(GameTime gameTime, Environment E)
        {

            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();

            var mouseRect = new Rectangle(currentMouse.X, currentMouse.Y, 1, 1);

            isHovering = false;

            // checking if mouse is hovering and or clicking the button

            if (mouseRect.Intersects(Rect) || isDragging)
            {
                isHovering = true;

                if (currentMouse.LeftButton == ButtonState.Pressed) 
                {
                    
                    // used the change in postion between the current and previous mousestates to determine how far to move the scroller

                    position = ScrollerMovement(position, currentMouse, previousMouse);
                    isDragging = true;
                }

                // for setting values

                else if ((currentMouse.LeftButton == ButtonState.Released) && (previousMouse.LeftButton == ButtonState.Pressed))
                {
                    Click?.Invoke(this, new SliderClickEventArgs(property, index));
                    isDragging = false;
                }
            }

            property = FindProperty(maxValue);
            
            base.Update(gameTime, E);
        } 
    }
}
