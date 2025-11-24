using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        private SpriteBatch _spriteBatchUI;
        private SpriteBatch _spriteBatchCamera;

        private Slider [] _sliders;
        private Button [] _buttons;
        private List<Projectile> _projectiles;
        private List<Timer> _timers;
        private List<RigidBody> _bodies;
        private List<Vector2> _contacts;
        private List<ScaledSprite> _scalableSprites;
        
        // for queues
        private Queue<Projectile> _projectileQueue;
        private Queue<TrailNode> _nodeQueue;
        private bool isExit;
        bool queuing;
        bool tickqueuing;

        Projectile projectile;

        Camera2D camera;

        Environment environment;

        float pixelsPerM;

        // for hiding UI elements
        private bool isVisibleSliders;

        // for pause screen
        private bool isPaused;
        private KeyboardState currentKeys;
        private KeyboardState previousKeys;
        private Button menuButton;

        
        // parameters for the projectile

        Texture2D texture;
        float projectileScale;
        Vector2 startPos;
        float initial_speed;
        float mass;
        float initial_angle;
        float radius;
        float time;
        float linearDragCoefficient;
        float angularVelocity;
        float angularDragCoefficient;
        float restitution;
        DateTime timerStartTime;
        bool isFrictionless;

        // base values

        float baseSpeed;
        float baseAngle;
        float baseGravity;
        float baselinearDragCoefficient;
        float baseAngularVelocity;
        float baseAngularDragCoefficient;

        // global values

        float globalScale;
       

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
            startPos = new Vector2(40, 600);
            projectileScale = 0.25f;
            initial_speed = 15f;
            mass = 2f;
            initial_angle = 40f;
            radius = 0.5f;
            time = 0f;
            linearDragCoefficient = 0f;
            angularVelocity = 0f;
            angularDragCoefficient = 0f;
            restitution = 0.5f;
            isFrictionless = true;


            // slider base properties
            baseSpeed = 15f;
            baseAngle = 45f;
            baseGravity = 9.81f;
            baselinearDragCoefficient = 0f;
            baseAngularVelocity = 0f;
            baseAngularDragCoefficient = 0f;

            // making UI visible
            isVisibleSliders = true;

            // pause screen
            isPaused = false;

            // closing the program
            isExit = false;

            // global scale 
            globalScale = 1f;


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatchUI = new SpriteBatch(GraphicsDevice);

            _spriteBatchCamera = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            projectile = new Projectile(texture, startPos, projectileScale, baseSpeed, mass, baseAngle, radius, linearDragCoefficient, angularVelocity, angularDragCoefficient, restitution, isFrictionless);

            pixelsPerM = projectile.ConversionToSI();

            camera = new Camera2D();


            var shootButton = new Button(Content.Load<Texture2D>("sprites/Button"), new Vector2(980, 660), 1.25f, Content.Load<SpriteFont>("fonts/font"), 0)
            {
                text = "SHOOT",
            };
             
            shootButton.Click += Button_Click;

            var resetButton = new Button(Content.Load<Texture2D>("sprites/Button"), new Vector2(1170, 20), 0.5f, Content.Load<SpriteFont>("fonts/font"), 1)
            {
                text = "RESET",
            };

            resetButton.Click += Button_Click;

            var tickProjectilesButton = new TickBox(Content.Load<Texture2D>("sprites/tickBox"), new Vector2(800, 50), 0.1f, Content.Load<SpriteFont>("fonts/font"), 2, Content.Load<Texture2D>("sprites/tick"))
            {
                text = "HIDE UI"
            };

            tickProjectilesButton.Click += Button_Click;

            var tickFrictionButton = new TickBox(Content.Load<Texture2D>("sprites/tickBox"), new Vector2(1000, 50), 0.1f, Content.Load<SpriteFont>("fonts/font"), 3, Content.Load<Texture2D>("sprites/tick"))
            {
                text = "FRICTION"
            };

            tickFrictionButton.Click += Button_Click;

            var zoomInButton = new Button(Content.Load<Texture2D>("sprites/zoomIn"), new Vector2(70, 20), 0.3f, Content.Load<SpriteFont>("fonts/font"), 4);
           
            zoomInButton.Click += Button_Click;

            var zoomOutButton = new Button(Content.Load<Texture2D>("sprites/zoomOut"), new Vector2(20, 20), 0.3f, Content.Load<SpriteFont>("fonts/font"), 5);

            zoomOutButton.Click += Button_Click;

            menuButton = new Button(Content.Load<Texture2D>("sprites/Button"), new Vector2(50, 100), 8f, Content.Load<SpriteFont>("fonts/font"), 1)
            {
                text = "PRESS 'm' TO RETURN TO SIM \n \n PRESS 'esc' TO EXIT PROGRAM",
            };


            var speedSlider = new Slider(Content.Load<Texture2D>("sprites/scroller"), 
                new Vector2(300, 670), 
                1f, 
                Content.Load<SpriteFont>("fonts/font"), 
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            { 
                text_scroller = "na" ,
                text_min = "0 m/s",
                text_max = "50 m/s",
                text_desc = "initial speed",
                index = 0,
                maxValue = 50f,
                minValue = 0,
            };

  
            speedSlider.Click += ScrollerClick;

            var gravitySlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(800, 200),
                2f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            {
                text_scroller = "na",
                text_min = "0 m/s^2",
                text_max = "50 m/s^2",
                text_desc = "gravity",
                index = 2,
                maxValue = 50f,
                minValue = 0,
            };

            gravitySlider.Click += ScrollerClick;

            var dragSlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(800, 100),
                2f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            {
                text_scroller = "na",
                text_min = "0",
                text_max = "0.01",
                text_desc = "drag coefficient",
                index = 3,
                maxValue = 0.01f,
                minValue = 0,
            };

            dragSlider.Click += ScrollerClick;

            var angularDragSlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(800, 300),
                2f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            {
                text_scroller = "na",
                text_min = "0",
                text_max = "0.01",
                text_desc = "angular drag coefficient",
                index = 5,
                maxValue = 0.01f,
                minValue = 0,
            };

            angularDragSlider.Click += ScrollerClick;

            var angularSlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(800, 400),
                2f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            {
                text_scroller = "na",
                text_min = "-100 rad/s",
                text_max = "100 rad/s",
                text_desc = "angular velocity",
                index = 4,
                maxValue = 100f,
                minValue = -100f,
            };

            angularSlider.Click += ScrollerClick;

            var cannon = new Cannon(
                Content.Load<Texture2D>("sprites/cannonHead"),
                new Vector2(50, 610),
                1f * globalScale,
                Content.Load<Texture2D>("sprites/cannonWheel"),
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                true)
            { 
                text_scroller = "na",
                index = 1,
                maxValue = 90f,
                minValue = 0,
            };

            cannon.Click += ScrollerClick;

            var floorBody = RigidBody.CreateRectangleBody(
                Content.Load<Texture2D>("sprites/floor"), 
                new Vector2(640,750), 
                1f, 
                new Vector2(0, 0), 
                1f, 
                int.MaxValue, // for handling collisions
                true, 
                0f, 
                0f, 
                0f,
                isFrictionless,
                pixelsPerM);

            var testBody = RigidBody.CreateRectangleBody(
                Content.Load<Texture2D>("sprites/target"),
                new Vector2(600, 500),
                1.5f,
                new Vector2(0, 0),
                0.5f,
                10f, // for handling collisions
                false,
                0f,
                0f,
                0f,
                isFrictionless,
                pixelsPerM);

            var testBody2 = RigidBody.CreateRectangleBody(
                Content.Load<Texture2D>("sprites/target"), // texture
                new Vector2(800, 500), // pos
                1f, // scale
                new Vector2(0, 0), // linear velocity
                0.5f, // restitution
                5f, // mass for handling collisions
                false, // static?
                0f, // angular velocity
                0f, // linear drag co
                0f, // angular drag co
                isFrictionless,
                pixelsPerM);

            initial_speed *= pixelsPerM; // convert pixels/s to m/s

            environment = new Environment(new Vector2( 0,pixelsPerM* 9.81f), 1.225f);

            _buttons = new Button[] 
            { 
                shootButton,
                resetButton,
                tickProjectilesButton,
                tickFrictionButton,
                zoomInButton,
                zoomOutButton,
            };

            _sliders = new Slider[]
            {
                speedSlider,
                dragSlider,
                gravitySlider,
                angularSlider,
                angularDragSlider,
                cannon
            };

            

            ResetAllSliders();

            _projectiles = new List<Projectile>
            {
                
            };

            _bodies = new List<RigidBody>
            {
                floorBody,
                testBody,
                testBody2
            };

            _scalableSprites = new List<ScaledSprite>
            {
                testBody,
                testBody2,
                cannon,
                projectile,

            };


            _contacts = new List<Vector2> { };
            _projectileQueue = new Queue<Projectile> { };
            _nodeQueue = new Queue<TrailNode> { };
            queuing = false;
            tickqueuing = false;

            _timers = new List<Timer> { };

            
        }
        // called everytime shootbutton is clicked, fires new projectile & sets up runtime timers
        private void Button_Click(object sender, System.EventArgs e)
        {
            var button = sender as Button;

            switch (button.index) // for handling different buttons
            { 
                case 0: // shoot button
                    _bodies.Remove(projectile.body);
                    projectile = new Projectile(texture, startPos, projectileScale, initial_speed, mass, initial_angle, radius, linearDragCoefficient, angularVelocity, angularDragCoefficient, restitution, isFrictionless);
                    
                    queuing = true;

                    foreach (var clock in _timers)
                    {
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
                    break;

                case 1: // reset button
                    _projectiles.Clear();
                    initial_angle = baseAngle;
                    initial_speed = baseSpeed * pixelsPerM;
                    environment.gravity = new Vector2(0, baseGravity * pixelsPerM);
                    angularVelocity = baseAngularVelocity;
                    linearDragCoefficient = baselinearDragCoefficient;
                    angularDragCoefficient = baseAngularDragCoefficient;
                    ResetAllSliders(); // sets position of sliders to correct place
                    // reset all rigid bodies
                    _bodies.Clear();
                    var floorBody = RigidBody.CreateRectangleBody(
                    Content.Load<Texture2D>("sprites/floor"),
                    new Vector2(640, 750),
                    1f,
                    new Vector2(0, 0),
                    1f,
                    int.MaxValue, // for handling collisions
                    true,
                    0f,
                    0f,
                    0f,
                    isFrictionless,
                    pixelsPerM);

                    var testBody = RigidBody.CreateRectangleBody(
                        Content.Load<Texture2D>("sprites/target"),
                        new Vector2(600, 500),
                        1.5f,
                        new Vector2(0, 0),
                        0.5f,
                        10f, // for handling collisions
                        false,
                        0f,
                        0f,
                        0f,
                        isFrictionless,
                        pixelsPerM);

                    var testBody2 = RigidBody.CreateRectangleBody(
                        Content.Load<Texture2D>("sprites/target"), // texture
                        new Vector2(800, 500), // pos
                        1f, // scale
                        new Vector2(0, 0), // linear velocity
                        0.5f, // restitution
                        5f, // mass for handling collisions
                        false, // static?
                        0f, // angular velocity
                        0f, // linear drag co
                        0f, // angular drag co
                        isFrictionless,
                        pixelsPerM);

                    _bodies.Add( floorBody );
                    _bodies.Add(testBody );
                    _bodies.Add(testBody2);

                    camera.ResetZoom();

                    break;
                case 2:
                    if (isVisibleSliders)
                        isVisibleSliders = false;
                    else
                        isVisibleSliders = true;
                    break;

                case 3:
                    for (int i = 0;  i < _bodies.Count; i++)
                    {
                        if (_bodies[i].isFrictionless)
                        {
                            _bodies[i].isFrictionless = false;
                            isFrictionless = false;
                        }
                        else
                        {
                            _bodies[i].isFrictionless = true;
                            isFrictionless = true;
                        }

                        
                    }
                    break;

                case 4: // zooming in
                    camera.Zoom(0.1f);
                    break;

                case 5: // zooming out
                    camera.Zoom(-0.1f);
                    break;

                default:
                    break;

            }
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
                case 3:
                    linearDragCoefficient = e.property;
                    break;
                case 4:
                    angularVelocity = e.property;
                    break;
                case 5:
                    angularDragCoefficient = e.property;
                    break;
                default:
                    break;

            }
        }

        // timer timeout:
        private void Tick(object sender, ElapsedEventArgs e)
        {

            if (projectile.body.linearVelocity == new Vector2(0, 0) && !isExit)
            {
            }
            else
            {
                time = time + 0.1f;
                try
                {
                    TrailNode node = new TrailNode(
                    Content.Load<SpriteFont>("fonts/font"),
                    (startPos - projectile.position).Y / pixelsPerM,
                    time,
                    VectorMaths.Length(projectile.position - startPos) / pixelsPerM,
                    texture,
                    new Vector2(projectile.position.X, projectile.position.Y - 4), // provide offset
                    .1f,
                    false
                    );
                    _nodeQueue.Enqueue(node);
                    tickqueuing = true;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            }
        }

        protected void ResetAllSliders()
        {
            foreach (var slider in _sliders)
            {
                switch (slider.index) 
                {
                    case 0:
                        slider.PropertyPlacement(baseSpeed);
                        break;
                    case 1:
                        slider.PropertyPlacement(baseAngle);
                        break;
                    case 2: 
                        slider.PropertyPlacement(baseGravity);
                        break;
                    case 3:
                        slider.PropertyPlacement(baselinearDragCoefficient);
                        break;
                    case 4:
                        slider.PropertyPlacement(baseAngularVelocity);
                        break;
                    case 5:
                        slider.PropertyPlacement(baseAngularDragCoefficient);
                        break;
                    default :
                        break;
                }

            }
        }

      

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                isExit = true;
            previousKeys = currentKeys;
            currentKeys = Keyboard.GetState();

            if (previousKeys.IsKeyDown(Keys.M) && currentKeys.IsKeyUp(Keys.M))
            {
                if (isPaused)
                {
                    isPaused = false;

                }
                else
                    isPaused = true;
            }


            // zooming in and out (temp)
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
               
                camera.Zoom(-0.1f);
                //camera.UpdateScalableElements(_scalableSprites);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                
                camera.Zoom(0.1f);
                //camera.UpdateScalableElements(_scalableSprites);
            }

            
           

            // TODO: Add your update logic here
            if (!isPaused)
            {
                foreach (var button in _buttons)
                {
                    button.Update(gameTime, environment);
                }
                if (isVisibleSliders)
                    foreach (var slider in _sliders)
                    {
                        slider.Update(gameTime, environment);
                    }


                foreach (var body in _bodies)
                {
                    body.Update(gameTime, environment);
                }

                if (_bodies.Count > 1) // only try to detect collisions if more than one rigid body present
                {
                    for (int i = 0; i < _bodies.Count; i++) // handling collisions *temp*
                    {
                        int count = 0;

                        for (int j = 1; j < _bodies.Count; j++)
                        {
                            if (Collisions.IntersectingPolygons(_bodies[i].CollisionRect.vertices, _bodies[(i + j) % (_bodies.Count)].CollisionRect.vertices, out Vector2 normal, out float depth))
                            {

                                Collisions.GetContactPoints(
                                    _bodies[i].CollisionRect.vertices,
                                    _bodies[(i + j) % (_bodies.Count)].CollisionRect.vertices,
                                    out Vector2 contact1,
                                    out Vector2 contact2,
                                    out int contactCount);
                                //Collisions.ResolveCollisionsWithRotation(_bodies[i], _bodies[(i + j) % (_bodies.Count)], normal, environment, contact1, contact2, contactCount, pixelsPerM);
                                Collisions.ResolveCollisionsBasic(_bodies[i], _bodies[(i + j) % (_bodies.Count)], normal, environment, contact1, contact2, contactCount);
                                _contacts.Add(contact1);
                                if (contactCount == 2)
                                    _contacts.Add(contact2);
                                
                            }
                            else
                            {
                                count++;
                                _bodies[i]._collidingWith.Remove(_bodies[(i + j) % (_bodies.Count)]);
                            }
                            
                            if (count == _bodies.Count - 1)
                            {
                                _bodies[i].isCollisionResolved = false;
                                
                            }
                            
                        }
                        //Console.WriteLine(_bodies[i]._collidingWith.Count);
                    }

                    foreach (var projectile in _projectiles)
                    {
                        projectile.Update(gameTime, environment);
                    }


                    if (projectile.body.previousLinearVelocity.Y < 0 && projectile.body.linearVelocity.Y > 0) // adding trail node at highest point
                    {
                        TrailNode node = new TrailNode(
                            Content.Load<SpriteFont>("fonts/font"),
                            (startPos - projectile.position).Y / pixelsPerM,
                            (float)(DateTime.Now - timerStartTime).TotalSeconds,
                            VectorMaths.Length(projectile.position - startPos) / pixelsPerM,
                            texture,
                            new Vector2(projectile.position.X, projectile.position.Y - 6), // provides an offset to place in middle of path
                            .15f,
                            true
                            );
                        _nodeQueue.Enqueue(node);
                        tickqueuing = true;
                    }

                    if (queuing) // add projectile to projectile list
                    {
                        foreach (var newprojectile in _projectileQueue)
                        {
                            _projectiles.Clear();
                            _projectiles.Add(newprojectile);
                            _bodies.Add(newprojectile.body);
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

                }
                if (isExit) Exit();

                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!isPaused)
                GraphicsDevice.Clear(Color.CornflowerBlue);
            else
                GraphicsDevice.Clear(Color.LightSlateGray);
            // TODO: Add your drawing code here

            _spriteBatchCamera.Begin(samplerState: SamplerState.LinearWrap, transformMatrix: camera.GetCameraScaleMatrix());
            _spriteBatchUI.Begin(samplerState: SamplerState.LinearWrap);
            
            foreach (var projectile in _projectiles)
            {
                projectile.Draw(gameTime, _spriteBatchCamera);
            }
            foreach (var body in _bodies)
            {
                if (body.isStatic)
                {
                    body.Draw(gameTime, _spriteBatchUI); // dont scale floor
                }
                else if (!(body.shapeType == ShapeType.Circle)) // dont draw projectiles
                    body.Draw(gameTime, _spriteBatchCamera);
                
            }
            foreach (var button in _buttons)
            {
                button.Draw(gameTime, _spriteBatchUI);
            }
            if (isVisibleSliders)
                foreach (var slider in _sliders)
                {
                    slider.Draw(gameTime, _spriteBatchUI);
                }

            //foreach (var contact in _contacts)
            //{
            //    Primitives2D.FillRectangle(_spriteBatch, contact.X - 10, contact.Y -10 , 20, 20, Color.Red, 0f);
            //}
            // clear contacts to not keep contacts that dont exit anymore
            _contacts.Clear();

            if (isPaused)
                menuButton.Draw(gameTime, _spriteBatchUI);

            _spriteBatchUI.End();
            _spriteBatchCamera.End();

            



            base.Draw(gameTime);
        }
    }
}
// use projectile positions
// place small dots at every other frame or so, contains stuff like velocity, height and tiem