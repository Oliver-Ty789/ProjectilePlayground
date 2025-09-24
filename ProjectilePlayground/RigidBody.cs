using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectilePlayground
{
    public enum ShapeType
    {
        Circle = 0,
        rectangle = 1
    }
    internal class RigidBody : ScaledSprite
    {
        /// <summary>
        ///  keeping mostly everything public as attributes will be accessible in projectile class
        /// </summary>
        // public 
        public Vector2 linearVelocity;
        public Vector2 previousLinearVelocity;
        public float angularVelocity;
        public readonly float linearDragCoefficient;
        public readonly float angularDragCoefficient;
        public float rotation;
        //public float rotationalVelocity;
        public VerticesRectangle _collisionRect;

        public readonly float density;
        public readonly float mass;
        public readonly float restitution;
        public readonly float area;

        public readonly bool isStatic;

        public readonly float radius;
        public readonly float width;
        public readonly float height;

        public readonly ShapeType shapeType;

        // private
        private Vector2 resistiveLinearForce;
        private float resistiveAngularForce;
        private Vector2 magnusForce;
        private Vector2 resultantForce;
        private Vector2 initialPos;

        // private instatiation as different shapes need different instantiations
        private RigidBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity,  float mass, float restitution, float area, bool isStatic, float radius, float width, float height, ShapeType shapeType, float angularVelocity, float linearDragCoeffficient, float angularDragCoefficient) : base(texture, position, scale)
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
            this.width = width;
            this.height = height;
            this.shapeType = shapeType;
            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
            CollisionRect = _collisionRect;
        }

        public static RigidBody CreateCircleBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity, float restitution, float radius, float mass, bool isStatic, float angularVelocity, float linearDragCoeffficient, float angularDragCoefficient)
        {
            float area = radius * radius * MathF.PI;

            return new RigidBody(texture, position, scale, linearVelocity, mass, restitution, area, isStatic, radius, 0f, 0f, ShapeType.Circle, angularVelocity, linearDragCoeffficient, angularDragCoefficient);
        }

        public static RigidBody CreateRectangleBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity, float restitution, float width, float height, float mass, bool isStatic, float angularVelocity, float linearDragCoeffficient, float angularDragCoefficient)
        {
            float area = width * height;

            return new RigidBody(texture, position, scale, linearVelocity, mass, restitution, area, isStatic, 0f, width, height, ShapeType.Circle, angularVelocity, linearDragCoeffficient, angularDragCoefficient);
        }


        private void ApplyVelocity(float delta)
        {
            position += linearVelocity * delta;

        }

        private void ApplyDrag(Environment environment)
        {
            // applying a formula to determine drag forces
           
            resistiveLinearForce = new Vector2(Convert.ToSingle(-linearDragCoefficient * 0.5 * environment.airPressure * area * MathF.Pow(linearVelocity.X, 2)),
                Convert.ToSingle(linearDragCoefficient * 0.5 * environment.airPressure * area * MathF.Pow(linearVelocity.Y, 2)));

            resistiveAngularForce = Convert.ToSingle(0.5 * angularDragCoefficient * MathF.Pow(angularVelocity, 2) * environment.airPressure * area);
            Console.WriteLine(resistiveLinearForce);
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



            if (position.Y < initialPos.Y + 1 && !(linearVelocity == new Vector2(0, 0)))
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




        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            var pivot = new Vector2(SourceRect.Width / 2f, SourceRect.Height / 2f);
            spriteBatch.Draw(texture, DrawingRect, SourceRect, Color.White, rotation, pivot, SpriteEffects.None, 0f);
            base.Draw(gameTime, spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment environment)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds; // difference in time between frames, keeps velocity/acceleration consitent

            ApplyDrag(environment);
            ApplyMagnus();
            ApplyForces(environment, delta);
            ApplyVelocity(delta);
            

            base.Update(gameTime, environment);
        }
    }
}

    