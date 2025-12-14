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
        private VerticesRectangle _collisionRect;

        public MouseState currentMouse;
        public MouseState previousMouse;
        public KeyboardState currentKey;
        public KeyboardState previousKey;
        public SpriteFont font;
        public bool isHovering;
        public bool isDragging;
        public bool isTexting;
        public bool isCannon;
        public Texture2D barTexture;
        public Vector2 barPosition;
        public TextBox textBox;
        public Texture2D cursorTexture;
        public int timeBeforeNextDelete;
        public float overallChangeInRotation;
        public bool isClockwise;
        public Vector2 rectOffset; // for matching placement of sprite

   
       // public VerticesRectangle verticesRectangleProperty;
        public event EventHandler<SliderClickEventArgs> Click;
        public List<char> _chars;
        public bool isClicked { get; private set; }
        public Color penColour { get; set; }
        public float property {  get; set; }
        public int index { get; set; }
        public float maxValue { get; set; }
        public float minValue { get; set; }
        public string text_scroller { get; set; }
        public string text_min {  get; set; }
        public string text_max { get; set; }
        public string text_desc { get; set; }

        public Rectangle BarRect
        {
            get
            {
                return new Rectangle(
                    (int) barPosition.X,
                    (int) barPosition.Y,
                    (int) (barTexture.Width * scale),
                    (int) (barTexture.Height * scale)
                    );
            }
        }

        public VerticesRectangle PropertyRect
        {
            get
            {
                return new VerticesRectangle(
                    new Vector2((CollisionRect.X + (CollisionRect.Width / 2) - (font.MeasureString(text_scroller).X / 2)), (CollisionRect.Y + (CollisionRect.Height / 2) - (font.MeasureString(text_scroller).Y / 2) + 30)),
                    font.MeasureString(text_scroller).X,
                    font.MeasureString(text_scroller).Y,
                    scale);
            }
        }

        public Slider(Texture2D texture, Vector2 position, float scale, SpriteFont font, Texture2D barTexture , Texture2D cursorTexture, bool isCannon) : base(texture, position, scale)
        {
            this.barTexture = barTexture;
            this.font = font;
            this.cursorTexture = cursorTexture;
            this.isCannon = isCannon;
            penColour = Color.Black;
            barPosition = new Vector2(position.X - 30, position.Y);
            _chars = new List<char>();
            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
            CollisionRect = _collisionRect;
            overallChangeInRotation = 0;
            rectOffset = new Vector2(0,0);

        }
        private bool IsNegative()
        {
            foreach (var letter in text_scroller)
            {
                if (letter == '-')
                    return true;
            }
            return false;
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

        public Vector2 ScrollerMovement(Vector2 position)
        {
            // moves scoller to cusors X pos
          
            float offset = currentMouse.X - previousMouse.X;

            // try stop gittering

            if (position.X == BarRect.Left && offset < 0f)
                return position;
            else if (position.X == (BarRect.Right- (20 * scale)) && offset > 0f)
                return position;

            else
            {
                // stays within bar
                if (CollisionRect.Left < BarRect.Left)
                    return position = new Vector2(BarRect.Left, position.Y);
                else if (CollisionRect.Right > BarRect.Right)
                    return position = new Vector2(BarRect.Right - (20 * scale), position.Y);

                else
                    return position = new Vector2(position.X + offset, position.Y);
            }
            
        }

        public virtual void PropertyPlacement(float value) // move scroller to parameter
        {
            // find how far along the scroller should be due to value & maxValue
            // find available space on bar
            

            float percentageBar = (value - minValue)/(maxValue-minValue);
            float space = (BarRect.Right - (BarRect.Left + (20 * scale)));

            position = new Vector2((space * percentageBar) + BarRect.Left, position.Y);
    
            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
        
               

        }

        public virtual float FindProperty()
        {

            // find how far along the scroller is on the bar then calcuate its selected value

            if (index == 4) // handles negative numbers
            {
                float halfBar = BarRect.Right - (BarRect.Left + (10 * scale) + BarRect.Width/2);
                float scrollerBar = position.X - BarRect.Left;

                float percentageBar = scrollerBar / halfBar;

                if (percentageBar >= 1)
                {
                    text_scroller = $"{Math.Round((percentageBar - 1) * maxValue, 1)}";
                    return (percentageBar - 1) * maxValue;
                }

                else
                {
                    text_scroller = $"{Math.Round((1 - percentageBar) * minValue, 1)}";
                    return (1 - percentageBar) * minValue;
                }

            }
            else
            {
                float wholeBar = BarRect.Right - (BarRect.Left + (20 * scale));
                float scrollerBar = position.X - BarRect.Left;

                float percentageBar = scrollerBar / wholeBar;

                if (index == 3 || index == 5)
                    text_scroller = $"{Math.Round(percentageBar * maxValue, 3)}";
                else
                    text_scroller = $"{Math.Round(percentageBar * maxValue, 1)}";
                return (percentageBar * maxValue) + minValue;
            }
                
        }

        public void HandleInput()
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
                if (keys[0] == Keys.OemMinus && !IsNegative())
                {
                    if (previousKey.IsKeyUp(keys[0]) && currentKey.IsKeyDown(keys[0])) // check if just pressed
                        textBox.AddMoreText('-');
                }
            }   
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            var colour = Color.White;
             
            if (isHovering)
                colour = Color.Gray;

            spriteBatch.Draw(barTexture, BarRect, Color.White);
            spriteBatch.Draw(texture, DrawingRect, colour);

            // all text stuff

            if (!string.IsNullOrEmpty(text_scroller) && !isTexting) // make sure writable text doesn't overlap
            {
                var x = (CollisionRect.X + (CollisionRect.Width / 2)) - (font.MeasureString(text_scroller).X / 2);
                var y = (CollisionRect.Y + (CollisionRect.Height / 2)) - (font.MeasureString(text_scroller).Y / 2) + 30;

                spriteBatch.DrawString(font, text_scroller, new Vector2(x, y), penColour);
            }
            if (!string.IsNullOrEmpty(text_min))
            {
                var x = BarRect.Left  - 40;
                var y = BarRect.Y + (font.MeasureString(text_scroller).Y / 2);

                spriteBatch.DrawString(font, text_min, new Vector2(x, y), penColour);
            }
            if (!string.IsNullOrEmpty(text_max))
            {
                var x = BarRect.Right + 10;
                var y = BarRect.Y + ((font.MeasureString(text_scroller).Y / 2) );

                spriteBatch.DrawString(font, text_max, new Vector2(x, y), penColour);
            }
            if (!string.IsNullOrEmpty(text_desc))
            {
                var x = BarRect.Left - 50;
                var y = BarRect.Y  - 50;

                spriteBatch.DrawString(font, text_desc, new Vector2(x, y), penColour);
            }

            if (isTexting)
                textBox.Draw(gameTime, spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment E, Camera2D camera)
        {

            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();
            previousKey = currentKey;
            currentKey = Keyboard.GetState();

            Vector2 scaledOffset = new(currentMouse.X, currentMouse.Y);

            if (isCannon)
            {
                
                scaledOffset = Vector2.Transform(scaledOffset, camera.camInverseMatrix);
            }

            var mouseRect = new VerticesRectangle(scaledOffset, 1, 1, 1);

            //var mouseRect = new Rectangle(currentMouse.Position.X, currentMouse.Position.Y, 1, 1);

            isHovering = false;

            // checking if mouse is hovering and or clicking the button

            if (Collisions.IntersectingPolygons(mouseRect.Center ,mouseRect.vertices, CollisionRect.Center,CollisionRect.vertices, out Vector2 normal, out float depth) || isDragging)
            {
                isHovering = true;

                if (currentMouse.LeftButton == ButtonState.Pressed) 
                {
                    
                    // used the change in postion between the current and previous mousestates to determine how far to move the scroller
                    if (!isCannon)
                    {
                        position = ScrollerMovement(position);
                        _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
                        
                    }
                    isDragging = true;
                }

                // for setting values

                else if ((currentMouse.LeftButton == ButtonState.Released) && (previousMouse.LeftButton == ButtonState.Pressed))
                {
                    Click?.Invoke(this, new SliderClickEventArgs(property, index));
                    isDragging = false;
                    if (isCannon)
                    { 
                          
                        _collisionRect = VerticesRectangle.GetTransformedRectangle(CollisionRect, overallChangeInRotation, new Vector2(0,0), new Vector2(0, 10) + position, 1f);
                        CollisionRect = _collisionRect;
                        overallChangeInRotation = 0f;
                    }
                }
            }

            // for text inputs
            if (Collisions.IntersectingPolygons(mouseRect.Center ,mouseRect.vertices, PropertyRect.Center,PropertyRect.vertices, out Vector2 normal2, out float depth2) && (currentMouse.LeftButton == ButtonState.Released) && (previousMouse.LeftButton == ButtonState.Pressed) && !isHovering)
            {
                isTexting = true;
                var x = (CollisionRect.X + (CollisionRect.Width / 2)) - (font.MeasureString(text_scroller).X / 2);
                var y = (CollisionRect.Y + (CollisionRect.Height / 2)) - (font.MeasureString(text_scroller).Y / 2) + 30;

                textBox = new TextBox(cursorTexture, new Vector2(x,y), 1f, text_scroller, font);
            }
            // check if any keys have been first pressed
            if (previousKey.GetPressedKeyCount() == 0 && currentKey.GetPressedKeyCount() == 1 && isTexting)
            { 
                if (currentKey.GetPressedKeys()[0] == Keys.Enter) // if enter has been clicked, set property 
                {
                    isTexting = false;
                    // need to check if value is appropriate

                    try
                    {
                        float value = Convert.ToSingle(text_scroller);

                        if (value <= maxValue && value >= minValue)
                        {

                            PropertyPlacement(value);
                            property = value;
                            Click?.Invoke(this, new SliderClickEventArgs(property, index));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }

            if (isTexting)
            {
                
                textBox.Update(gameTime, E, camera);
                HandleInput();
            }
            else
            {
                property = FindProperty();
            }

            if (isTexting && isDragging) // fixes bug to stop changing values via text when using slider
            {
                isTexting = false;
            }

            if (!isCannon)
                CollisionRect = _collisionRect;

            base.Update(gameTime, E, camera);
        } 
    }
}
