using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Timers;
using System.Xml.Linq;
using CsvHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProjectilePlayground.Content.controls;
using static System.Formats.Asn1.AsnWriter;

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
        private ScaledSprite [] _titleSprites;
        private Button [] _titleButtons;
        private ScaledSprite[] _tutorialSprites;
        private Button [] _tutorialButtons;
        
        
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

        
        // parameters for the projectile (temporary)

       
        readonly Vector2 startPos = new(50,605);
        DateTime timerStartTime;
        DateTime PausedStartTime;
        float PausedTime;
        bool isFrictionless;
        float initial_speed;
        float initial_angle;
        float angularVelocity;
        float time;

        readonly string presetsPath = @"C:\Users\olive\source\repos\ProjectilePlayground\ProjectilePlayground\presets.csv";
        string presetName;
        ProjectileProperties projectileProperties;

        bool IsCustom; // to toggle the drag coefficient sliders when custom projectile is selected
        


        /// <summary>
        /// this is to mainatain trail node size for all different presets for their different image sizes
        /// </summary>
        readonly float desiredSize = 15f;
        readonly float desiredSizeBig = 25f;
        float trailNodeScale;
        float trailBigNodeScale;

        /// <summary>
        /// this is allows the target buttons to be accessible anywhere in the code
        /// </summary>
        TargetButton targetVertButton;
        TargetButton targetHoriButton;

        // base values

        float baseSpeed;
        float baseAngle;
        float baseGravity;
        float baselinearDragCoefficient;
        float baseAngularVelocity;
        float baseAngularDragCoefficient;
        float baseMass;
        float baseRadius;
        float baseScale;

        // for collision settings
        bool isRotationalCollisions;

        // for choosing the right scene
        bool isTitle;
        bool isTutorial;

        // for the tutorial slides
        int slideNumber;

       

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
            
            isFrictionless = true;
            initial_speed = 10f;
            initial_angle = 45f;

            presetName = "basic";
            projectileProperties = new ProjectileProperties();
            IsCustom = true;

            // tutorial/title screens
            isTitle = true;
            isTutorial = false;
            slideNumber = 0;


            Get_CsvData();


            // slider base properties
            baseSpeed = 10f;
            baseAngle = 45f;
            baseGravity = 9.81f;
            baselinearDragCoefficient = 0f;
            baseAngularVelocity = 0f;
            baseAngularDragCoefficient = 0f;
            baseMass = 2f;
            baseRadius = 0.25f;
            baseScale = 0.3f;

            // making UI visible
            isVisibleSliders = true;

            // pause screen
            isPaused = false;

            // closing the program
            isExit = false;

            // for collision settings
            isRotationalCollisions = false;

            // for handling paused errors
            PausedTime = 0f;



            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatchUI = new SpriteBatch(GraphicsDevice);

            _spriteBatchCamera = new SpriteBatch(GraphicsDevice);

            // load title content in here

            var titleSprite = new ScaledSprite(Content.Load<Texture2D>("sprites/title"), new Vector2(230, 90), 1f);

            var simulationButton = new Button(Content.Load<Texture2D>("sprites/simulationButton"), new Vector2(300, 400), 1f, Content.Load<SpriteFont>("fonts/font"), 13);

            simulationButton.Click += Button_Click;

            var tutorialButton = new Button(Content.Load<Texture2D>("sprites/tutorialButton"), new Vector2(800, 400), 1f, Content.Load<SpriteFont>("fonts/font"), 14);

            tutorialButton.Click += Button_Click;

            _titleSprites = new ScaledSprite[1] { 
                titleSprite,
            };

            _titleButtons = new Button[2] { simulationButton,
                tutorialButton};


            // load tutorial content in here

            _tutorialSprites = new ScaledSprite[6];
        

            for (int i = 0; i < 6; i++) // adding all the slides for the tutorial
            {
                var sprite = new ScaledSprite(Content.Load<Texture2D>("sprites/tutorial" + i.ToString()), Vector2.Zero, 1f);
                _tutorialSprites[i] = sprite;
            }

            var tutorialButtonLeft = new Button(Content.Load<Texture2D>("sprites/ButtonLeft"), new Vector2(40, 650), 1f, Content.Load<SpriteFont>("fonts/font"), 15);

            tutorialButtonLeft.Click += Button_Click;

            var tutorialButtonRight = new Button(Content.Load<Texture2D>("sprites/ButtonRight"), new Vector2(1100, 650), 1f, Content.Load<SpriteFont>("fonts/font"), 16);

            tutorialButtonRight.Click += Button_Click;

            _tutorialButtons = new Button[2] { tutorialButtonLeft, 
                tutorialButtonRight };


            // TODO: use this.Content to load your simulation content here

            projectile = new Projectile(Content.Load<Texture2D>("sprites/"+projectileProperties.path), startPos, projectileProperties.scale, baseSpeed * ConversionToSI(), projectileProperties.mass, baseAngle, projectileProperties.radius, projectileProperties.linearDragCoefficient, angularVelocity, projectileProperties.angularDragCoefficient, projectileProperties.coeffiecentOfResitution, isFrictionless);
            
            pixelsPerM = ConversionToSI();

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

            var tickProjectilesButton = new TickBox(Content.Load<Texture2D>("sprites/tickBox"), new Vector2(750, 50), 0.1f, Content.Load<SpriteFont>("fonts/font"), 2, Content.Load<Texture2D>("sprites/tick"))
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

            menuButton = new Button(Content.Load<Texture2D>("sprites/Button"), new Vector2(50, 100), 8f, Content.Load<SpriteFont>("fonts/font"), 5)
            {
                text = "PRESS 'esc' TO RETURN TO SIM \n \n PRESS 'm' TO RETURN TO MAIN MENU",
            };

            var tennisBallButton = new Button(Content.Load<Texture2D>("sprites/Button"), new Vector2(680, 680), 0.6f, Content.Load<SpriteFont>("fonts/font"), 6)
            {
                text = "Tennis ball",
            };

            tennisBallButton.Click += Button_Click;

            var basicBallButton = new Button(Content.Load<Texture2D>("sprites/Button"), new Vector2(580, 680), 0.6f, Content.Load<SpriteFont>("fonts/font"), 7)
            {
                text = "Custom",
            };

            basicBallButton.Click += Button_Click;

            var beachBallButton = new Button(Content.Load<Texture2D>("sprites/Button"), new Vector2(780, 680), 0.6f, Content.Load<SpriteFont>("fonts/font"), 8)
            {
                text = "Beach ball",
            };

            beachBallButton.Click += Button_Click;

            var cannonBallButton = new Button(Content.Load<Texture2D>("sprites/Button"), new Vector2(880, 680), 0.6f, Content.Load<SpriteFont>("fonts/font"), 9)
            {
                text = "Cannon ball",
            };

            cannonBallButton.Click += Button_Click;

            targetVertButton = new TargetButton(Content.Load<Texture2D>("sprites/target"), new Vector2(200, 40), 1f, Content.Load<SpriteFont>("fonts/font"), 10, "target")
            { 
                text = "Place Vertical Body"
            };


            targetVertButton.Click += Button_Click;

            targetHoriButton = new TargetButton(Content.Load<Texture2D>("sprites/targetHori"), new Vector2(300, 70), 1f, Content.Load<SpriteFont>("fonts/font"), 11, "targetHori")
            {
                text = "Place Horizontal Body"
            };

            targetHoriButton.Click += Button_Click;

            var tickRotationalCollisionsButton = new TickBox(Content.Load<Texture2D>("sprites/tickBox"), new Vector2(550, 50), 0.1f, Content.Load<SpriteFont>("fonts/font"), 12, Content.Load<Texture2D>("sprites/tick"))
            {
                text = "UNLOCK ROTATION \n (WIP)"
            };


            tickRotationalCollisionsButton.Click += Button_Click;


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
                text_max = "30 m/s",
                text_desc = "initial speed",
                index = 0,
                maxValue = 30f,
                minValue = 0,
            };

  
            speedSlider.Click += ScrollerClick;

            var gravitySlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(950, 200),
                1.25f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            {
                text_scroller = "na",
                text_min = "0 m/s^2",
                text_max = "50 m/s^2",
                text_desc = "\n gravity",
                index = 2,
                maxValue = 50f,
                minValue = 0,
            };

            gravitySlider.Click += ScrollerClick;

            var dragSlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(900, 400),
                0.6f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            {
                text_scroller = "na",
                text_min = "0",
                text_max = "0.1",
                text_desc = "drag \n coefficient",
                index = 3,
                maxValue = 0.1f,
                minValue = 0,
            };

            dragSlider.Click += ScrollerClick;

            var angularDragSlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(900, 300),
                0.6f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            {
                text_scroller = "na",
                text_min = "0",
                text_max = "1",
                text_desc = "angular \n drag \n coefficient",
                index = 5,
                maxValue = 1f,
                minValue = 0,
            };

            angularDragSlider.Click += ScrollerClick;

            var angularSlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(950, 100),
                1.25f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            {
                text_scroller = "na",
                text_min = "-100 rad/s",
                text_max = "100 rad/s",
                text_desc = "angular \n velocity",
                index = 4,
                maxValue = 100f,
                minValue = -100f,
            };

            angularSlider.Click += ScrollerClick;

            var massSlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(1100, 300),
                0.75f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            {
                text_scroller = "na",
                text_min = "0.1 kg",
                text_max = "100 kg",
                text_desc = "\n mass",
                index = 6,
                maxValue = 99.9f,
                minValue = 0.1f,
            };

            massSlider.Click += ScrollerClick;

            var radiusSlider = new Slider(
                Content.Load<Texture2D>("sprites/scroller"),
                new Vector2(1100, 400),
                0.75f,
                Content.Load<SpriteFont>("fonts/font"),
                Content.Load<Texture2D>("sprites/sliderbar"),
                Content.Load<Texture2D>("sprites/flashingCursor"),
                false)
            {
                text_scroller = "na",
                text_min = "0.1 m",
                text_max = "1 m",
                text_desc = "\n radius",
                index = 7,
                maxValue = 0.9f,
                minValue = 0.1f,
            };

            radiusSlider.Click += ScrollerClick;



            var cannon = new Cannon(
                Content.Load<Texture2D>("sprites/cannonHead"),
                new Vector2(50, 610),
                1f,
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
                Content.Load<Texture2D>("sprites/floorLong"), 
                new Vector2(640,730), 
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
                tennisBallButton,
                basicBallButton,
                cannonBallButton,
                beachBallButton,
                targetVertButton,
                targetHoriButton,
                tickRotationalCollisionsButton
            };

            _sliders = new Slider[]
            {
                speedSlider,
                dragSlider,
                gravitySlider,
                angularSlider,
                angularDragSlider,
                cannon,
                massSlider,
                radiusSlider
            };

            

            ResetAllSliders();

            _projectiles = new List<Projectile>
            {
                
            };

            _bodies = new List<RigidBody>
            {
                floorBody,
                
            };

           


            _contacts = new List<Vector2> { };
            _projectileQueue = new Queue<Projectile> { };
            _nodeQueue = new Queue<TrailNode> { };
            queuing = false;
            tickqueuing = false;

            _timers = new List<Timer> { };



            
        }

        private void Get_CsvData()
        {
            using var reader = new StreamReader(presetsPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<ProjectileProperties>();

            foreach (ProjectileProperties record in records)
            {
                if (record.name == presetName)
                {
                    projectileProperties = record;
                    trailNodeScale = desiredSize / Content.Load<Texture2D>("sprites/" + projectileProperties.path).Width;
                    trailBigNodeScale = desiredSizeBig / Content.Load<Texture2D>("sprites/" + projectileProperties.path).Width;

                }
            }
            
        }
        
        public float ConversionToSI()

        // 1. find how many pixels in radius
        // 2. find how many radius' make a meter
        // 3. use that scale to find pixels to meter

        {
            var texture = Content.Load<Texture2D>("sprites/" + projectileProperties.path);
            float radiusP = (texture.Width * projectileProperties.scale) / 2; // finds the radius of the projectile in pixels
            float radiusPerMeter = 1 / projectileProperties.radius; // eg if radius = 0.5 therefore there would be 2 radius' per meter
            float pixelsToMeter = radiusPerMeter * radiusP;
            return pixelsToMeter;
        }

        // called everytime shootbutton is clicked, fires new projectile & sets up runtime timers
        private void Button_Click(object sender, System.EventArgs e)
        {
            var button = sender as Button;

            switch (button.index) // for handling different buttons
            { 
                case 0: // shoot button
                    _bodies.Remove(projectile.body);

                    PausedTime = 0f;
                    
                    projectile = new Projectile(Content.Load<Texture2D>("sprites/" + projectileProperties.path), startPos, projectileProperties.scale, initial_speed, projectileProperties.mass, initial_angle, projectileProperties.radius, projectileProperties.linearDragCoefficient, angularVelocity, projectileProperties.angularDragCoefficient, projectileProperties.coeffiecentOfResitution, isFrictionless);

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

                    if (IsCustom) // only reset these if the custom preset is seleced
                    {
                        projectileProperties.linearDragCoefficient = baselinearDragCoefficient;
                        projectileProperties.angularDragCoefficient = baseAngularDragCoefficient;
                        projectileProperties.mass = baseMass;
                        projectileProperties.radius = baseRadius;
                        projectileProperties.scale = baseScale;
                    }

                    ResetAllSliders(); // sets position of sliders to correct place
                    // reset all rigid bodies
                    _bodies.Clear();
                    var floorBody = RigidBody.CreateRectangleBody(
                    Content.Load<Texture2D>("sprites/floorLong"),
                    new Vector2(640, 730),
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

                    

                    _bodies.Add( floorBody );
                    

                    

                    break;
                case 2: // UI button
                    if (isVisibleSliders)
                        isVisibleSliders = false;
                    else
                        isVisibleSliders = true;
                    break;

                case 3: // friction tick box
                   
                    if (isFrictionless)
                    {
                        isFrictionless = false;
                    }
                    else
                    {                         
                        isFrictionless = true;
                    }

                        
                    
                    break;

                case 4: // zooming in
                    camera.Zoom(0.1f);
                    break;

                case 5: // zooming out
                    camera.Zoom(-0.1f);
                    break;

                case 6: // tennis ball preset
                    /// 
                    /// summary (true for all preset buttons)
                    /// 
                    /// 1. all projectiles need to be cleared from projectile and rigid bodies lists
                    /// 2. need to change projectilePreset via calling get_CsvData
                    /// 3. reset all sliders so that new pixelsPerM are taken into effect
                    /// 



                    _projectiles.Clear();
                    _bodies.Remove(projectile.body); 
                    presetName = "tennis ball";
                    Get_CsvData();
                    IsCustom = false;
                    pixelsPerM = ConversionToSI();

                    ResetAllSliders();
                    initial_angle = baseAngle;
                    initial_speed = baseSpeed * pixelsPerM;
                    environment.gravity = new Vector2(0, baseGravity * pixelsPerM);
                    angularVelocity = baseAngularVelocity;
                    break;

                case 7: // custom ball preset
                    _projectiles.Clear();
                    _bodies.Remove(projectile.body);
                    presetName = "basic";
                    Get_CsvData();
                    IsCustom = true;
                    pixelsPerM = ConversionToSI();

                    ResetAllSliders();
                    initial_angle = baseAngle;
                    initial_speed = baseSpeed * pixelsPerM;
                    environment.gravity = new Vector2(0, baseGravity * pixelsPerM);
                    angularVelocity = baseAngularVelocity;
                    projectileProperties.linearDragCoefficient = baselinearDragCoefficient;
                    projectileProperties.angularDragCoefficient = baseAngularDragCoefficient;
                    projectileProperties.mass = baseMass;
                    break;

                case 8: // beach ball preset
                    _projectiles.Clear();
                    _bodies.Remove(projectile.body);
                    presetName = "beach ball";
                    Get_CsvData();
                    IsCustom = false;
                    pixelsPerM = ConversionToSI();

                    ResetAllSliders();
                    initial_angle = baseAngle;
                    initial_speed = baseSpeed * pixelsPerM;
                    environment.gravity = new Vector2(0, baseGravity * pixelsPerM);
                    angularVelocity = baseAngularVelocity;
                    break;

                case 9: // cannon ball preset
                    _projectiles.Clear();
                    _bodies.Remove(projectile.body);
                    presetName = "cannon ball";
                    Get_CsvData();
                    IsCustom = false;
                    pixelsPerM = ConversionToSI();

                    ResetAllSliders();
                    initial_angle = baseAngle;
                    initial_speed = baseSpeed * pixelsPerM;
                    environment.gravity = new Vector2(0, baseGravity * pixelsPerM);
                    angularVelocity = baseAngularVelocity;
                    break;

                case 10: // vertical target button
                    if (targetVertButton.isClicked)
                    {
                        PlaceTargetBody(targetVertButton);
                    }
                    else
                    {
                        targetVertButton.isClicked = true;
                    }
                        
                    
                    break;

                case 11: // horizontal target button
                    if (targetHoriButton.isClicked)
                    {
                        PlaceTargetBody(targetHoriButton);
                    }
                    else
                    {
                        targetHoriButton.isClicked = true;
                    }


                    break;

                case 12: // unlock rotations tick box
                    if (isRotationalCollisions)
                    {
                        isRotationalCollisions = false;
                    }
                    else isRotationalCollisions = true;
                    break;

                case 13: // go to simulation button
                    isTitle = false;
                    break;

                case 14: // go to tutorial button
                    isTutorial = true;
                    isTitle = false;
                    break;
                case 15: // left button in tutorial
                    if (slideNumber == 0)
                    {
                        isTutorial = false;
                        isTitle = true;
                       
                    }
                    else
                    {
                        slideNumber -= 1;
                    }
                    break;

                case 16: // right button in tutorial
                    if (slideNumber == 5)
                    {
                        isTutorial = false;
                        isTitle = true;
                        slideNumber = 0;
                    }
                    else
                    {
                        slideNumber += 1;
                    }
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
                    if (IsCustom) // only reset these if the custom projectile being used
                        projectileProperties.linearDragCoefficient = e.property;
                    break;
                case 4:
                    angularVelocity = e.property;
                    break;
                case 5:
                    if (IsCustom) // only reset these if the custom projectile being used
                        projectileProperties.angularDragCoefficient = e.property;
                    break;
                case 6:
                    if (IsCustom) // only reset these if the custom projectile being used
                        projectileProperties.mass = e.property;
                    break;
                case 7:
                    if (IsCustom)   // only reset these if the custom projectile being used
                    { // for ajusting the scale of the projectile when radius is changed
                        projectileProperties.scale = (projectileProperties.radius * e.property) / projectileProperties.scale;

                        projectileProperties.radius = e.property;

                    }
                        
                        
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
            else if (!isPaused)
            {
                
                time = time + 0.1f;
                try
                {
                    if (projectile.body.isTrail)
                    {
                        TrailNode node = new TrailNode(
                        Content.Load<SpriteFont>("fonts/font"),
                        (startPos - projectile.position).Y / pixelsPerM,
                        time,
                        VectorMaths.Length(projectile.position - startPos) / pixelsPerM,
                        Content.Load<Texture2D>("sprites/" + projectileProperties.path),
                        new Vector2(projectile.position.X, projectile.position.Y - 4), // provide offset
                        trailNodeScale,
                        false,
                        Content.Load<Texture2D>("sprites/textBox")
                        );
                        _nodeQueue.Enqueue(node);
                        tickqueuing = true;
                    }
                    
                }
                catch (Exception exception)
                {
                    Console.WriteLine("yo");
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
                    case 6:
                        slider.PropertyPlacement(baseMass);
                        break;
                    case 7:
                        slider.PropertyPlacement(baseRadius);                        
                        break;
                    default :
                        break;
                }

            }
        }

        void PlaceTargetBody(TargetButton button)
        {
            /// summary
            /// body is instantiated at mouse position, if colliding with anything, body is destroyed
            /// 
            
            var tempBody = RigidBody.CreateRectangleBody(
                        Content.Load<Texture2D>("sprites/"+button.texturePath), // texture that needs to fit with the button
                        button.scaledOffset, // pos
                        1f, // scale
                        new Vector2(0, 0), // linear velocity
                        0.5f, // restitution
                        10f, // mass for handling collisions
                        false, // static?
                        0f, // angular velocity
                        0f, // linear drag co
                        0f, // angular drag co
                        isFrictionless,
                        pixelsPerM);


            var count = 0;
            for (int i = 0; i < _bodies.Count; i++) // handling collisions *temp
            {
                
                if (Collisions.IntersectingPolygons(tempBody.CollisionRect.Center, tempBody.CollisionRect.vertices, _bodies[i].CollisionRect.Center ,_bodies[i].CollisionRect.vertices, out Vector2 normal, out float depth))
                {
                    // means tempBody is colliding with something and cant be added
                    
                    return;
                }
                else
                {
                    count ++;
                }
            }
            

            if (count == _bodies.Count) // if no bodies are colliding with temp then place the body
            {
                
                _bodies.Add(tempBody);
                button.isClicked = false;
            }
        }

        void CollisionPhase()
        {
            for (int i = 0; i < _bodies.Count - 1; i++)
            {
                //int count = 0;

                for (int j = 1 + i; j < _bodies.Count; j++) // only resolve collisions for bodies that haven't been fully resolved yet
                {
                    if (Collisions.IntersectingPolygons(_bodies[i].CollisionRect.Center + _bodies[i].position, _bodies[i].CollisionRect.vertices, _bodies[j].CollisionRect.Center + _bodies[j].position, _bodies[j].CollisionRect.vertices, out Vector2 normal, out float depth))
                    {

                        Collisions.GetContactPoints(
                            _bodies[i].CollisionRect.vertices,
                            _bodies[j].CollisionRect.vertices,
                            out Vector2 contact1,
                            out Vector2 contact2,
                            out int contactCount);


                        // make sure bodies do not overlap
                        _bodies[i].Move(-normal * depth / 2);

                        _bodies[j].Move(normal * depth / 2);

                        if (isRotationalCollisions)
                        {
                            if (!isFrictionless)
                            {
                                Collisions.ResolveCollisionsWithRotationAndFriction(_bodies[i], _bodies[j], normal, environment, contact1, contact2, contactCount, pixelsPerM);
                            }
                            else
                            {
                                Collisions.ResolveCollisionsWithRotation(_bodies[i], _bodies[j], normal, environment, contact1, contact2, contactCount, pixelsPerM);
                            }
                        }

                        else
                        {
                            if (!isFrictionless)
                            {
                                Collisions.ResolveCollisionsBasicAndFriction(_bodies[i], _bodies[j], normal, environment, contact1, contact2, contactCount, depth);
                            }
                            else
                            {
                                Collisions.ResolveCollisionsBasic(_bodies[i], _bodies[j], normal, environment, contact1, contact2, contactCount, depth);
                            }
                        }

                        if (_bodies[i].isStatic || _bodies[j].isStatic) // for applying gravity only when not in contact of ground
                        {
                            _bodies[i].isOnGround = true;
                            _bodies[j].isOnGround = true;
                        }



                        _contacts.Add(contact1);
                        if (contactCount == 2)
                            _contacts.Add(contact2);



                    }

                }


            }
        }
        void HandleCollisions()
        {
            if (_bodies.Count > 1) // only try to detect collisions if more than one rigid body present
            {
                int numberOfEpochs = 10;
                for (int i = 0; i < numberOfEpochs; i++) // repeating the collision dectection to increase resolution
                {
                    CollisionPhase();
                }
                    
            }
        }

        protected void UpdateSimulation(GameTime gameTime)
        {
            previousKeys = currentKeys;
            currentKeys = Keyboard.GetState();



            if (previousKeys.IsKeyDown(Keys.Escape) && currentKeys.IsKeyUp(Keys.Escape))
            {
                if (isPaused)
                {
                    isPaused = false;
                    PausedTime += (float)(DateTime.Now - PausedStartTime).TotalSeconds;
                }
                else
                {
                    isPaused = true;
                    PausedStartTime = DateTime.Now;
                    
                }
                    
            }


            // TODO: Add your update logic here


            if (!isPaused)
            {



                foreach (var button in _buttons)
                {
                    button.Update(gameTime, environment, camera);

                }
                if (isVisibleSliders)
                    foreach (var slider in _sliders)
                    {
                        if (!IsCustom) // only update custom sliders if custom preset projectile is selected
                        {
                            if (!(slider.index == 5 || slider.index == 3 || slider.index == 6 || slider.index == 7))
                            {
                                slider.Update(gameTime, environment, camera);
                            }
                        }
                        else
                        {
                            slider.Update(gameTime, environment, camera);
                        }

                    }


                HandleCollisions();


                foreach (var body in _bodies)
                {
                    body.Update(gameTime, environment, camera);
                }



                foreach (var projectile in _projectiles)
                {
                    projectile.Update(gameTime, environment, camera);
                }

                if (projectile.body.previousLinearVelocity.Y < 0 && projectile.body.linearVelocity.Y > 0 && projectile.body.isTrail) // adding trail node at highest point
                {
                    var time = (float)(DateTime.Now - timerStartTime).TotalSeconds;
                    time -= PausedTime;
                    

                    TrailNode node = new TrailNode(
                        Content.Load<SpriteFont>("fonts/font"),
                        (startPos - projectile.position).Y / pixelsPerM,
                        time,
                        VectorMaths.Length(projectile.position - startPos) / pixelsPerM,
                        Content.Load<Texture2D>("sprites/" + projectileProperties.path),
                        new Vector2(projectile.position.X, projectile.position.Y - 6), // provides an offset to place in middle of path
                        trailBigNodeScale,
                        true,
                        Content.Load<Texture2D>("sprites/textBox")
                        );
                    try
                    {
                        _nodeQueue.Enqueue(node);
                        tickqueuing = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

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
                    try
                    {
                        foreach (var node in _nodeQueue)
                        {
                            projectile._nodes.Add(node);
                        }
                        tickqueuing = false;
                        _nodeQueue.Clear();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                }

            }
            else // if paused
            {


                if (previousKeys.IsKeyDown(Keys.M) && currentKeys.IsKeyUp(Keys.M)) // title button pressed
                {
                    isTitle = true;
                    isPaused = false;
                    _projectiles.Clear();
                    ResetAllSliders();
                }
            }
            if (isExit) Exit();
        }

        protected void UpdateTitleScreen(GameTime gameTime)
        {
            foreach (var sprite in _titleSprites)
            {
                sprite.Update(gameTime, environment, camera);
            }
            foreach (var button in _titleButtons)
            {
                button.Update(gameTime, environment, camera);
            }
        }

        protected void UpdateTutorialScreen(GameTime gameTime)
        {
            var currentScreen = _tutorialSprites[slideNumber];
            currentScreen.Update(gameTime, environment, camera);

            foreach (var button in _tutorialButtons)
            {
                button.Update(gameTime, environment, camera);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            
            if (isTitle)
            {
                UpdateTitleScreen(gameTime);
            }
            else if (isTutorial)
            {
                UpdateTutorialScreen(gameTime);
            }
            else
                UpdateSimulation(gameTime);

            base.Update(gameTime);
        }


        protected void DrawSimulation(GameTime gameTime)
        {
            if (!isPaused)
                GraphicsDevice.Clear(Color.CornflowerBlue);
            else
                GraphicsDevice.Clear(Color.LightSlateGray);
            // TODO: Add your drawing code here


            /// summary 
            /// the sprites drawn are broken down into two different sprite batches
            /// 1. for UI elements not effected by the camera
            /// 2. scalable elements that will be effected by the camera.
            /// they need to be broken up to allow for the appropriate transformation matrix
            /// to be applied to the correct sprites

            _spriteBatchCamera.Begin(samplerState: SamplerState.LinearWrap, transformMatrix: camera.GetCameraScaleMatrix());
            _spriteBatchUI.Begin(samplerState: SamplerState.LinearWrap);

            
            foreach (var body in _bodies)
            {
                if (body.isStatic)
                {
                    body.Draw(gameTime, _spriteBatchCamera); // dont scale floor
                }
                else if (!(body.shapeType == ShapeType.Circle)) // dont draw projectiles
                    body.Draw(gameTime, _spriteBatchCamera);

            }
            foreach (var projectile in _projectiles) // projectiles drawn here to keep the trail nodes
            {
                projectile.Draw(gameTime, _spriteBatchCamera);
            }
            foreach (var button in _buttons)
            {
                button.Draw(gameTime, _spriteBatchUI, _spriteBatchCamera);
            }
            if (isVisibleSliders)
                foreach (var slider in _sliders)
                {
                    if (slider.isCannon)
                    {
                        slider.Draw(gameTime, _spriteBatchCamera);
                    }
                    else
                    {
                        if (!IsCustom) // only draw custom sliders if custom preset projectile is selected
                        {
                            if (!(slider.index == 5 || slider.index == 3 || slider.index == 6 || slider.index == 7))
                            {
                                slider.Draw(gameTime, _spriteBatchUI);
                            }
                        }
                        else
                        {
                            slider.Draw(gameTime, _spriteBatchUI);
                        }
                    }

                }


            // clear contacts to not keep contacts that dont exist anymore
            //foreach (var contact in _contacts)
            //{
            //    Primitives2D.DrawRectangle(_spriteBatchUI, contact, new (5,5), Color.AliceBlue);
            //}
            _contacts.Clear();

            if (isPaused)
                menuButton.Draw(gameTime, _spriteBatchUI, _spriteBatchCamera);


            _spriteBatchCamera.End();
            _spriteBatchUI.End();

        }

        protected void DrawTitleScreen(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatchUI.Begin(samplerState: SamplerState.LinearWrap);
            _spriteBatchCamera.Begin(samplerState: SamplerState.LinearWrap, transformMatrix: camera.GetCameraScaleMatrix());
            foreach (var sprite in _titleSprites)
            {
                sprite.Draw(gameTime, _spriteBatchUI);
            }
            foreach (var button in _titleButtons)
            {
                button.Draw(gameTime, _spriteBatchUI, _spriteBatchCamera);
            }
            _spriteBatchUI.End();
            _spriteBatchCamera.End();
        }

        protected void DrawTutorialScreen(GameTime gameTime)
        {
            _spriteBatchUI.Begin(samplerState: SamplerState.LinearWrap);
            _spriteBatchCamera.Begin(samplerState: SamplerState.LinearWrap, transformMatrix: camera.GetCameraScaleMatrix());

            var currentScreen = _tutorialSprites[slideNumber];
            currentScreen.Draw(gameTime, _spriteBatchUI);

            foreach (var button in _tutorialButtons)
            {
                button.Draw(gameTime, _spriteBatchUI, _spriteBatchCamera);
            }

            _spriteBatchUI.End();
            _spriteBatchCamera.End();

        }
        protected override void Draw(GameTime gameTime)
        {

            if (isTitle)
            {
                DrawTitleScreen(gameTime);
            }
            else if (isTutorial)
            {
                DrawTutorialScreen(gameTime);
            }
            else
                DrawSimulation(gameTime);

            base.Draw(gameTime);
        }
    }
}
