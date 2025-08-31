using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Xml.Linq;
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

        private List<ScaledSprite> _sprites;
        private List<Projectile> _projectiles;
        private List<Timer> _timers;
        
        
        // for waiting projectiles
        private Queue<Projectile> _projectileQueue;
        private Queue<TrailNode> _nodeQueue;
        bool queuing;
        bool tickqueuing;

        Projectile projectile;

        Environment environment;

        float pixelsPerM;
        
        // parameters for the projectile

        Texture2D texture;
        float scale;
        Vector2 startPos;
        float initial_speed;
        int mass;
        float initial_angle;
        float radius;
        float time;
        DateTime timerStartTime;

        // base values

        float baseSpeed;
        float baseAngle;



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

            // parameters for first projectile (test)
            texture = Content.Load<Texture2D>("sprites/Final_face_circle");
            startPos = new Vector2(30, 630);
            scale = 0.25f;
            initial_speed = 15f;
            mass = 10;
            initial_angle = 40f;
            radius = 0.5f;
            time = 0f;

            // slider base properties
            baseSpeed = 15f;
            baseAngle = 45f;


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);


            // TODO: use this.Content to load your game content here

            var shootButton = new Button(Content.Load<Texture2D>("sprites/Button"), new Vector2(950, 600), 2f, Content.Load<SpriteFont>("fonts/font"))
            {
                text = "SHOOT",
            };
             
            shootButton.Click += ShootButton_Click;

            var speedSlider = new Slider(Content.Load<Texture2D>("sprites/scroller"), 
                new Vector2(800, 400), 
                2f, 
                Content.Load<SpriteFont>("fonts/font"), 
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"))
            { 
                text_scroller = "na" ,
                text_min = "0 m/s",
                text_max = "30 m/s",
                text_desc = "speed",
                index = 0,
                maxValue = 30f
            };

  
            speedSlider.Click += ScrollerClick;

            var angleSlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(800, 300),
                2f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"))
            {
                text_scroller = "na",
                text_min = "0 degrees",
                text_max = "90 degrees",
                text_desc = "angle",
                index = 1,
                maxValue = 90f,
            };

            angleSlider.Click += ScrollerClick;


            var gravitySlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(800, 200),
                2f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"))
            {
                text_scroller = "na",
                text_min = "0 m/s^2",
                text_max = "50 m/s^2",
                text_desc = "gravity",
                index = 2,
                maxValue = 50f,
            };

            gravitySlider.Click += ScrollerClick;

            projectile = new Projectile(texture, startPos, scale, initial_speed, mass, initial_angle, radius);

            pixelsPerM = projectile.ConversionToSI();

            environment = new Environment(new Vector2( pixelsPerM* 9.81f));

            // set all sliders beforehand
            speedSlider.PropertyPlacement(baseSpeed);
            angleSlider.PropertyPlacement(baseAngle);
            gravitySlider.PropertyPlacement(environment.gravity.Y/pixelsPerM);



            _sprites = new List<ScaledSprite> { 
                shootButton,
                speedSlider,
                angleSlider,
                gravitySlider,
            };

            _projectiles = new List<Projectile>
            {
                projectile,
            };

            _projectileQueue = new Queue<Projectile> { };
            _nodeQueue = new Queue<TrailNode> { };
            queuing = false;
            tickqueuing = false;

            _timers = new List<Timer> { };

        }
        // called everytime shootbutton is clicked, fires new projectile & sets up runtime timers
        private void ShootButton_Click(object sender, System.EventArgs e)
        {
            projectile = new Projectile(texture, startPos, scale, initial_speed, mass, initial_angle, radius);
            queuing = true;

            foreach (var clock in _timers)
            {
                Console.WriteLine(_timers.Count());
                clock.Dispose();
            }
            _timers.Clear();
            Timer timer = new Timer(100);
            timer.Elapsed += Tick;
            timer.Enabled = true;
            time = 0;
            timerStartTime = DateTime.Now; // to record how long timer has been running, for turning points
            timer.Start();
            _timers.Add(timer);
            _projectileQueue.Enqueue(projectile);
        }

        // called everytime scroller is let go, sets properties for projectile
        private void ScrollerClick(object sender, SliderClickEventArgs e)
        {
            switch (e.index) // sets all attributes of projectile
            {
                case 0:
                    initial_speed = e.property * pixelsPerM;
                    break;

                case 1:
                    initial_angle = e.property;
                    break;
                case 2:
                    environment.gravity = new Vector2(0 ,e.property * pixelsPerM);
                    break;
                default:
                    break;

            }
        }

        // timer timeout:
        private void Tick(object sender, ElapsedEventArgs e)
        {

            if (projectile.velocity == new Vector2(0, 0))
            {
            }
            else
            {
                time = time + 0.1f;
                TrailNode node = new TrailNode(
                    Content.Load<SpriteFont>("fonts/font"),
                    (startPos - projectile.position).Y / pixelsPerM,
                    time,
                    VectorMaths.Length(projectile.position - startPos) / pixelsPerM ,
                    texture,
                    new Vector2(projectile.position.X, projectile.position.Y + 5), // provide offset
                    .1f,
                    false
                    );
                _nodeQueue.Enqueue(node);
                tickqueuing = true;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            foreach (var sprite in _sprites)
            {
                sprite.Update(gameTime, environment);
            }

            foreach (var projectile in _projectiles)
            {
                projectile.Update(gameTime, environment);
            }


            if (projectile.previousVelocity.Y < 0 &&  projectile.velocity.Y > 0) // adding trail node at highest point
            {
                TrailNode node = new TrailNode(
                    Content.Load<SpriteFont>("fonts/font"),
                    (startPos - projectile.position).Y / pixelsPerM,
                    (float)(DateTime.Now - timerStartTime).TotalSeconds,
                    VectorMaths.Length(projectile.position - startPos) / pixelsPerM,
                    texture,
                    new Vector2 (projectile.position.X, projectile.position.Y + 2), // provides an offset to place in middle of path
                    .15f,
                    true
                    );
                _nodeQueue.Enqueue(node);
                tickqueuing = true;
            }

            if (queuing) // add projectile to projectile list
            {
                foreach (var projectile in _projectileQueue)
                {
                    _projectiles.Clear();
                    _projectiles.Add(projectile);
                }
                queuing = false;
                _projectileQueue.Clear();
            }
            if (tickqueuing) // add node to projectile
            {
                foreach (var node in _nodeQueue)
                {
                    projectile._nodes.Add(node);
                }
                tickqueuing = false;
                _nodeQueue.Clear();
            }



                base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin(samplerState: SamplerState.LinearWrap);
            foreach (var projectile in _projectiles)
            {
                projectile.Draw(gameTime, _spriteBatch);
            }
            foreach (var sprite in _sprites)
            {
                sprite.Draw(gameTime, _spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
// use projectile positions
// place small dots at every other frame or so, contains stuff like velocity, height and tiem