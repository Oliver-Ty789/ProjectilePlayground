using System;
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
        public float rotation;
        //public float rotationalVelocity;
        public VerticesRectangle _collisionRect;

        //public readonly float density;
        public readonly float mass;
        public readonly float restitution;
        public readonly float area;

        public bool isStatic;
        public bool isCollisionResolved;

        public readonly float radius;
        //public readonly float width;
        //public readonly float height;

        public readonly ShapeType shapeType;

        // private
        private Vector2 resistiveLinearForce;
        private float resistiveAngularForce;
        private Vector2 magnusForce;
        private Vector2 resultantForce;
        private Vector2 initialPos;

        // private instatiation as different shapes need different instantiations
        private RigidBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity,  float mass, float restitution, float area, bool isStatic, float radius, ShapeType shapeType, float angularVelocity, float linearDragCoeffficient, float angularDragCoefficient) : base(texture, position, scale)
        {
            this.linearVelocity = linearVelocity;
            this.angularVelocity = angularVelocity;
            this.rotation = 0f;
            this.linearDragCoefficient = linearDragCoeffficient;
            this.angularDragCoefficient = angularDragCoefficient;
            //this.rotationalVelocity = 0f;

            //this.density = density;
            this.mass = mass;
            this.restitution = restitution;
            this.area = area;
            this.isStatic = isStatic;

            this.position = position;
            this.scale = scale;
            this.initialPos = position;
            this.radius = radius;
            //this.width = width;
            //this.height = height;
            this.shapeType = shapeType;
            isCollisionResolved = false;
            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);

            // adding an offset to compenstate the change in position for the source rect

            var offset = new Vector2(-(texture.Width/2) * scale,-(texture.Height/2) * scale);

            _collisionRect = VerticesRectangle.GetTransformedRectangle(_collisionRect, 0f, offset, new Vector2(0, 0), 1f);

            CollisionRect = _collisionRect;
        }

        public static RigidBody CreateCircleBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity, float restitution, float radius, float mass, bool isStatic, float angularVelocity, float linearDragCoeffficient, float angularDragCoefficient)
        {
            float area = radius * radius * MathF.PI;

            return new RigidBody(texture, position, scale, linearVelocity, mass, restitution, area, isStatic, radius, ShapeType.Circle, angularVelocity, linearDragCoeffficient, angularDragCoefficient);
        }

        public static RigidBody CreateRectangleBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity, float restitution, float mass, bool isStatic, float angularVelocity, float linearDragCoeffficient, float angularDragCoefficient)
        {
            float area = (texture.Width * texture.Height)*scale;

            return new RigidBody(texture, position, scale, linearVelocity, mass, restitution, area, isStatic, 0f, ShapeType.Rectangle, angularVelocity, linearDragCoeffficient, angularDragCoefficient);
        }


        private void ApplyVelocity(float delta)
        {
            position += linearVelocity * delta;
            _collisionRect = VerticesRectangle.GetTransformedRectangle(CollisionRect, 0f, linearVelocity * delta, CollisionRect.Center + position, 1f);
            CollisionRect = _collisionRect;
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

        private void ApplyForces(Environment environment, float delta)
        {
            // gravity

            resultantForce = environment.gravity * mass;


            // drag
            resultantForce += resistiveLinearForce;


            // magnus
            resultantForce += magnusForce;



            if (!(linearVelocity == new Vector2(0, 0)))
            {
                previousLinearVelocity = linearVelocity;
                // F = ma
                var acceleration = new Vector2(resultantForce.X / mass, resultantForce.Y / mass);

                linearVelocity += acceleration * delta;
                if (angularVelocity > 0)
                    angularVelocity -= resistiveAngularForce;
                else
                    angularVelocity += resistiveAngularForce;
                rotation += angularVelocity * delta;
            }
            else
            {
                linearVelocity = new Vector2(0, 0);
                resistiveLinearForce = new Vector2(0, 0);
                magnusForce = new Vector2(0, 0);
                resultantForce = new Vector2(0, 0);
                position.Y = initialPos.Y;
            }
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
                Primitives2D.DrawLine(spriteBatch, CollisionRect.vertices[i], CollisionRect.vertices[(i+1)%4], Color.White);
            }
            base.Draw(gameTime, spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment environment)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds; // difference in time between frames, keeps velocity/acceleration consitent

            if (!isStatic) // only apply physics to non-static bodies and not while impulse is being applied
            {
                if (!isCollisionResolved)
                {
                    ApplyDrag(environment);
                    ApplyMagnus();
                    ApplyForces(environment, delta);
                }

                ApplyVelocity(delta);
            }

            if (shapeType == ShapeType.Rectangle)
            {
                for (int i = 0;i < 4;i++)
                {
                   // Console.WriteLine($"{i} : {CollisionRect.vertices[i]}");
                }
            }

            base.Update(gameTime, environment);
        }
    }
}

    