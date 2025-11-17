using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectilePlayground
{
    sealed class Projectile : ScaledSprite
    {
       
        public Vector2 initialVelocity;
        public float radius;
        public Vector2 previousVelocity;
        public List<TrailNode> _nodes;
        public RigidBody body;

        // private
        private VerticesRectangle _collisionRect;
        

        public Projectile(Texture2D texture, Vector2 position, float scale, float initial_s, int mass, float initial_a, float radius, float linearDragCoefficient, float angularVelocity, float angularDragCoefficient, float restitution, bool isFrictionless) : base (texture, position, scale)
        {
            this.radius = radius;
            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
            CollisionRect = _collisionRect;
            initialVelocity = VectorMaths.ToVector2(initial_s, initial_a);

            body = RigidBody.CreateCircleBody(texture, position, scale, initialVelocity, restitution, radius, mass, false, angularVelocity, linearDragCoefficient, angularDragCoefficient, isFrictionless, ConversionToSI());

            _nodes = new List<TrailNode> { };
               
        }
        
        public float ConversionToSI()

            // 1. find how many pixels in radius
            // 2. find how many radius' make a meter
            // 3. use that scale to find pixels to meter

        {
            float radiusP = (texture.Width * scale) /2; // finds the radius of the projectile in pixels
            float radiusPerMeter = 1/radius; // eg if radius = 0.5 therefore there would be 2 radius' per meter
            float pixelsToMeter = radiusPerMeter * radiusP;
            return pixelsToMeter;
        }

        public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            body.Draw(gameTime, _spriteBatch);

            foreach (var node in _nodes)
            {
                node.Draw(gameTime, _spriteBatch);
            }
            

            base.Draw(gameTime, _spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment environment)
        {
            previousVelocity = body.linearVelocity;
            
            position = body.position;
          
            foreach (var node in _nodes)
            {
                node.Update(gameTime, environment);
            }

            base.Update(gameTime, environment);
        }
    }
}
