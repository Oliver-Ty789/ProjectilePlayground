using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectilePlayground
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        ScaledSprite sprite;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            Texture2D texture = Content.Load<Texture2D>("Final_face_circle");
            sprite = new ScaledSprite(texture, new Vector2(100, 100), 0.2f);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            // getting the mouse state

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                var mousePosition = Mouse.GetState().Position.ToVector2();
                sprite.position = mousePosition;
                Console.WriteLine(VectorMaths.DotProduct(new Vector2(4, 3), new Vector2(8,6)));
            }
        
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin(samplerState: SamplerState.LinearWrap);

            _spriteBatch.Draw(sprite.texture, sprite.Rect, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
