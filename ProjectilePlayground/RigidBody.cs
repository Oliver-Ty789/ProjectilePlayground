using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectilePlayground
{
    public enum ShapeType
    {
        Circle = 0,
        Rectangle = 1
    }
    internal class RigidBody : ScaledSprite
    {
        /// <summary>
        ///  keeping mostly everything public as attributes will be accessible in projectile class
        /// </summary>
        // public 
        public Vector2 linearVelocity
        {
            get; set;
        }

        public Vector2 previousLinearVelocity;
        public float angularVelocity;
        public readonly float linearDragCoefficient;
        public readonly float angularDragCoefficient;
        public readonly float staticFrictionCoefficient;
        public readonly float dynamicFrictionCoefficient;
        public Vector2 staticFriction;
        public Vector2 dynamicFriction;
        public float rotation;
        public float projection;
       
        public VerticesRectangle _collisionRect;

        public List<Vector2> _contactPoints;
        public List<RigidBody> _collidingWith;

       
        public readonly float mass;
        public readonly float restitution;
        public readonly float area;
        public readonly float rotationalInertia;
        public readonly float invInertia;
        public readonly float invMass;

        public bool isStatic;
        public bool isCollisionResolved;
        public bool isFrictionless;

        public readonly float radius;
       

        public readonly ShapeType shapeType;

        // for getting rid of the trail after collisionCount >= 2
        public bool isTrail { get; set; }
        public int collisionCount { get; set; }

        // private
        private Vector2 resistiveLinearForce;
        private float resistiveAngularForce;
        private Vector2 magnusForce;
        private Vector2 resultantForce;
        private Vector2 initialPos;

        // private instatiation as different shapes need different instantiations
        private RigidBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity,  float mass, float restitution, float area, bool isStatic, float radius, ShapeType shapeType, float angularVelocity, float linearDragCoeffficient, float angularDragCoefficient, bool isFrictionless, float pixelsPerM) : base(texture, position, scale)
        {
            this.linearVelocity = linearVelocity;
            this.angularVelocity = angularVelocity;
            this.rotation = 0f;
            this.linearDragCoefficient = linearDragCoeffficient;
            this.angularDragCoefficient = angularDragCoefficient;
            staticFrictionCoefficient = 0.8f;
            dynamicFrictionCoefficient = 0.5f;
          
            this.mass = mass;
            this.restitution = restitution;
            this.area = area;
            this.isStatic = isStatic;

            this.position = position;
            this.scale = scale;
            this.initialPos = position;
            this.radius = radius;
            
            this.shapeType = shapeType;
            isCollisionResolved = false;
            this.isFrictionless = isFrictionless;

            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
            _contactPoints = new List<Vector2>()
            {

            };
            _collidingWith = new List<RigidBody>() { };

            // adding an offset to compenstate the change in position for the source rect

            var offset = new Vector2(-(texture.Width/2) * scale,-(texture.Height/2) * scale);

            _collisionRect = VerticesRectangle.GetTransformedRectangle(_collisionRect, 0f, offset, new Vector2(0, 0), 1f);

            CollisionRect = _collisionRect;

            this.rotationalInertia = GetRotationalIntertia(pixelsPerM);

            if (!isStatic)
            {
                this.invInertia = 1f / rotationalInertia;
                this.invMass = 1f / mass;
            }
            else
            {
                this.invInertia = 0f;
                this.invMass = 0f;
            }

            isTrail = true;
            collisionCount = 0;
            
        }

        public static RigidBody CreateCircleBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity, float restitution, float radius, float mass, bool isStatic, float angularVelocity, float linearDragCoeffficient, float angularDragCoefficient, bool isFrictionless, float pixelsPerM)
        {
            float area = radius * radius * MathF.PI;

            return new RigidBody(texture, position, scale, linearVelocity, mass, restitution, area, isStatic, radius, ShapeType.Circle, angularVelocity, linearDragCoeffficient, angularDragCoefficient, isFrictionless, pixelsPerM);
        }

        public static RigidBody CreateRectangleBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity, float restitution, float mass, bool isStatic, float angularVelocity, float linearDragCoeffficient, float angularDragCoefficient, bool isFrictionless, float pixelsPerM)
        {
            float area = (texture.Width * texture.Height)*scale;

            return new RigidBody(texture, position, scale, linearVelocity, mass, restitution, area, isStatic, 0f, ShapeType.Rectangle, angularVelocity, linearDragCoeffficient, angularDragCoefficient, isFrictionless, pixelsPerM);
        }

        private float GetRotationalIntertia(float pixelsPerM)
        {
            if (shapeType == ShapeType.Circle)
            {
                float radiusPix = CollisionRect.Width;
                return (1f/2f) * mass * radiusPix * radiusPix;
            }
            else
            {
                float widthM = CollisionRect.Width;
                float heightM = CollisionRect.Height ;
                return (1f/12f) * mass * (widthM * widthM  + heightM * heightM);
            }
        }


        public void Move(Vector2 amount) // making sure boides are not inside of eachother
        {
            if (isStatic) 
            {
                return;
            }
            position += amount;
            _collisionRect = VerticesRectangle.GetTransformedRectangle(CollisionRect, 0f, amount, CollisionRect.Center + position, 1f);
            CollisionRect = _collisionRect;
        }

        private void ApplyVelocity(float delta)
        {
            position += linearVelocity * delta;
            _collisionRect = VerticesRectangle.GetTransformedRectangle(CollisionRect, 0f, linearVelocity * delta, CollisionRect.Center + position, 1f);
            CollisionRect = _collisionRect;
            rotation += angularVelocity * delta;
            var changeInRotation = angularVelocity * delta;
          
           

            // apply to collision rect

        
            if (shapeType == ShapeType.Rectangle)
            {
                _collisionRect = VerticesRectangle.GetTransformedRectangle(CollisionRect, changeInRotation, Vector2.Zero, position, 1f);
                CollisionRect = _collisionRect;
            }
        }

        private void ApplyDrag(Environment environment)
        {
            // applying a formula to determine drag forces

           
           
            resistiveLinearForce = new Vector2(Convert.ToSingle(-linearDragCoefficient * 0.5 * environment.airPressure * area * MathF.Pow(linearVelocity.X, 2)),
                Convert.ToSingle(linearDragCoefficient * 0.5 * environment.airPressure * area * MathF.Pow(linearVelocity.Y, 2)));

            resistiveAngularForce = Convert.ToSingle(0.5 * angularDragCoefficient * MathF.Pow(angularVelocity, 2) * environment.airPressure * area);

            
        }
        private void ApplyMagnus()
        {
            if (shapeType == ShapeType.Circle)
            {
                var unitVelocity = VectorMaths.UnitVector(linearVelocity);
                var perpendicularVector = new Vector2(-unitVelocity.Y, unitVelocity.X);
                var magnusForceMag = linearDragCoefficient * angularVelocity * VectorMaths.Length(linearVelocity);
                magnusForce = perpendicularVector * magnusForceMag;
            }
        }

        private void ApplyFriction(Environment environment, float delta)
        {
            // calc resultant velocity (calc is short for calculator for anyone who just joined the stream) 
            // gravity
            resultantForce = environment.gravity * mass;

            // drag
            resultantForce += resistiveLinearForce;


            // magnus
            resultantForce += magnusForce;


            // these magnitudes need to be parallel, therefore they need to be projected onto one another (edit: try to apply this in collisions)
        
           
            var acceleration = new Vector2(resultantForce.X / mass, resultantForce.Y / mass);
            projection = VectorMaths.DotProduct(acceleration, staticFriction);
            var accMag = MathF.Abs(VectorMaths.Length(acceleration));
            var staticMag = MathF.Abs(VectorMaths.Length(staticFriction));

            

            if (staticMag < accMag) // apply dynamic friction
            {
                linearVelocity += dynamicFriction;
                var dynamicFrictionMag = VectorMaths.Length(dynamicFriction);
                angularVelocity -= dynamicFrictionMag * (0.005f * angularVelocity);
                
            }
            // appling static friction is merely not moving the object as there is no further external forces
            else
            {
                linearVelocity = Vector2.Zero;
                angularVelocity = 0f;
            }
        }

        private void ApplyForces(Environment environment, float delta)
        {
            resultantForce = Vector2.Zero;


            
            // gravity

            resultantForce += environment.gravity * mass;


            // drag
            resultantForce += resistiveLinearForce;


            // magnus
            resultantForce += magnusForce;



           
            previousLinearVelocity = linearVelocity;
            // F = ma
            Vector2 acceleration = new (resultantForce.X / mass, resultantForce.Y / mass);

            linearVelocity += acceleration * delta; // as 60 ticks per second
            if (angularVelocity > 0)
                angularVelocity -= resistiveAngularForce * delta;
            else
                angularVelocity += resistiveAngularForce * delta;
                
            

           
        }
        
        //public static void HandleCollisions(RigidBody body1, RigidBody body2, out RigidBody bodya, out RigidBody bodyb) // called from main
        //{
        //   var bodya = 
        //}


        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            var pivot = new Vector2(SourceRect.Width / 2f, SourceRect.Height / 2f);
            spriteBatch.Draw(texture, DrawingRect, SourceRect, Color.White, rotation, pivot, SpriteEffects.None, 0f);

            for (int i = 0; i < 4; i++)
            {
                Primitives2D.DrawLine(spriteBatch, CollisionRect.vertices[i], CollisionRect.vertices[(i + 1) % 4], Color.White);
            }

            //Primitives2D.FillRectangle(spriteBatch, CollisionRect.Center.X + position.X, CollisionRect.Center.X + position.Y , 10, 10, Color.White);
            Primitives2D.FillRectangle(spriteBatch, position.X, position.Y, 10, 10, Color.Orange);
            base.Draw(gameTime, spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment environment, Camera2D camera)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds; // difference in time between frames, keeps velocity/acceleration consitent

            
            if (!isStatic)
            {
                ApplyDrag(environment);
                ApplyMagnus();
                ApplyForces(environment, delta);

                if (!isFrictionless)
                {
                    ApplyFriction(environment, delta);
                }

                ApplyVelocity(delta);
            }
            
            

            base.Update(gameTime, environment, camera);
        }
    }
}

    