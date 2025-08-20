using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectilePlayground.Content.controls;

namespace ProjectilePlayground
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private List<Button> _buttons;

        Projectile projectile;

        Environment environment;



        float pixelsPerM;
        //float pixelsPerM_s;
        // float pixelsPerM_s_s;
       



        // parameters for the projectile

        float scale;
        Vector2 startPos;
        float initial_speed;
        int mass;
        float initial_angle;
        double radius; 



        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1280; // Set your desired width
            _graphics.PreferredBackBufferHeight = 720; // Set your desired height
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            // make mouse visible during the game
            IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var shootButton = new Button(Content.Load<Texture2D>("sprites/Button"), new Vector2(950, 600), 2f, Content.Load<SpriteFont>("fonts/font"))
            {
                text = "SHOOT",
            };

            shootButton.Click += ShootButton_Click;

            _buttons = new List<Button>()
            {
                shootButton,
            };

            // TODO: use this.Content to load your game content here

            // parameters for first projectile (test)
            Texture2D texture = Content.Load<Texture2D>("sprites/Final_face_circle");
            startPos = new Vector2(30, 630);
            scale = 0.25f;
            initial_speed = 500f;
            mass = 10;
            initial_angle = 80f;
            radius = 0.5d;

            projectile = new Projectile(texture, startPos, scale, initial_speed, mass, initial_angle, radius);

            pixelsPerM = projectile.ConversionToSI();

            environment = new Environment(new Vector2(0, pixelsPerM* 9.81f));
        }

        private void ShootButton_Click(object sender, System.EventArgs e)
        {
            var random = new Random();
            initial_angle = (float)random.Next(10,80);
            projectile = new Projectile(Content.Load<Texture2D>("sprites/Final_face_circle"), startPos, scale, initial_speed, mass, initial_angle, radius);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            foreach (var button in _buttons)
            {
                button.Update(gameTime);
            }

            projectile.Update(gameTime, environment);
        
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin(samplerState: SamplerState.LinearWrap);

            foreach (var button in _buttons)
            {
                button.Draw(gameTime, _spriteBatch);
            }

            //_spriteBatch.Draw(sprite.texture, sprite.Rect, Color.White);
            _spriteBatch.Draw(projectile.texture, projectile.Rect, Color.Red);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
