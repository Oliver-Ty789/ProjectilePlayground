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
        ///  keeping everything public as attributes will be inherited by projectile class
        /// </summary>
        // public 
        public Vector2 linearVelocity;
        public Vector2 angularVelocity;
        public float rotation;
        public float rotationalVelocity;
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

        // private instatiation as different shapes need different instantiations
        private RigidBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity,  float mass, float restitution, float area, bool isStatic, float radius, float width, float height, ShapeType shapeType) : base(texture, position, scale)
        {
            this.linearVelocity = linearVelocity;
            this.angularVelocity = new Vector2(0,0);
            this.rotation = 0f;
            this.rotationalVelocity = 0f;

            //this.density = density;
            this.mass = mass;
            this.restitution = restitution;
            this.area = area;
            this.isStatic = isStatic;

            this.radius = radius;
            this.width = width;
            this.height = height;
            this.shapeType = shapeType;
            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
            CollisionRect = _collisionRect;
        }

        public static RigidBody CreateCircleBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity, float restitution, float radius, float mass, bool isStatic)
        {
            float area = radius * radius * MathF.PI;

            return new RigidBody(texture, position, scale, linearVelocity, mass, restitution, area, isStatic, radius, 0f, 0f, ShapeType.Circle);
        }

        public static RigidBody CreateRectangleBody(Texture2D texture, Vector2 position, float scale, Vector2 linearVelocity, float restitution, float width, float height, float mass, bool isStatic)
        {
            float area = width * height;

            return new RigidBody(texture, position, scale, linearVelocity, mass, restitution, area, isStatic, 0f, width, height, ShapeType.Circle);
        }


        private void ApplyVelocity(float delta)
        {
            position += linearVelocity * delta;

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, DrawingRect, SourceRect, Color.White);

            base.Draw(gameTime, spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment environment)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds; // difference in time between frames, keeps velocity/acceleration consitent
            ApplyVelocity(delta);


            base.Update(gameTime, environment);
        }
    }
}

    