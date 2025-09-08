using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectilePlayground
{
    internal class Projectile : ScaledSprite
    {
        public int mass;
        public Vector2 velocity;
        public float initialAngle;
        public float dragCoefficient;
        public float radius;
        public Vector2 previousVelocity;
        public float angluarVelocity;
        public Vector2 resultantForce;
        // public float C_of_D; // unimportant at this time
        //public float C_of_E;
        public List<TrailNode> _nodes;

        // private
        private Vector2 resistiveForce;
        private Vector2 magnusForce;
        private float spriteRotation;
        private Vector2 initialPos;
        private VerticesRectangle _collisionRect;

        public Projectile(Texture2D texture, Vector2 position, float scale, float initial_s, int mass, float initial_a, float radius, float dragCoefficient, float angularVelocity) : base (texture, position, scale)
        {
            this.mass = mass;
            
            this.initialAngle = initial_a;
            this.radius = radius;
            this.initialPos = position;
            this.dragCoefficient = dragCoefficient;
            this.angluarVelocity = angularVelocity;
            this.magnusForce = new Vector2(0, 0);
            this.resultantForce = new Vector2(0, 0);
            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
            CollisionRect = _collisionRect;

            _nodes = new List<TrailNode> { };
            
            velocity = VectorMaths.ToVector2(initial_s, initial_a);
        }
   
        private void ApplyVelocity(float delta)
        {
            this.position += this.velocity*delta;

        }

        private void ApplyDrag(Environment environment)
        {
            // applying a formula to determine drag forces
            var area = radius * MathF.PI;
            resistiveForce = new Vector2(Convert.ToSingle(-dragCoefficient * 0.5 * environment.airPressure * area * MathF.Pow(velocity.X, 2)),
                Convert.ToSingle(dragCoefficient * 0.5 * environment.airPressure * area * MathF.Pow(velocity.Y, 2)));
        }

        private void ApplyMagnus()
        {
            var unitVelocity = VectorMaths.UnitVector(velocity);
            var perpendicularVector = new Vector2(-unitVelocity.Y, unitVelocity.X);
            var magnusForceMag = dragCoefficient * angluarVelocity * VectorMaths.Length(velocity);
            magnusForce = perpendicularVector * magnusForceMag;
        }

        private void ApplyForces(Environment environment, float delta)
        {
            // gravity
            resultantForce = environment.gravity * mass;
            

            // drag
            resultantForce += resistiveForce;
            

            // magnus
            resultantForce += magnusForce;
            


            if (position.Y < initialPos.Y + 1 && !(velocity == new Vector2(0,0)))
            {
                previousVelocity = velocity;
                // F = ma
                var acceleration = new Vector2(resultantForce.X / mass, resultantForce.Y / mass);
                Console.WriteLine(acceleration.ToString());
                velocity += acceleration * delta;
                spriteRotation += angluarVelocity * delta;
                
            }
            else
            {
                velocity = new Vector2(0, 0);
                resistiveForce = new Vector2(0,0);
                magnusForce = new Vector2(0, 0);
                resultantForce = new Vector2(0, 0);
                position.Y = initialPos.Y;
            }
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
            var pivot = new Vector2(SourceRect.Width / 2f, SourceRect.Height / 2f);
            _spriteBatch.Draw(texture, DrawingRect, SourceRect, Color.White, spriteRotation, pivot, SpriteEffects.None, 0f);

            foreach (var node in _nodes)
            {
                node.Draw(gameTime, _spriteBatch);
            }
            

            base.Draw(gameTime, _spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment environment)
        {

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds; // difference in time between frames, keeps velocity/acceleration consitent 
            ApplyDrag(environment);
            ApplyMagnus();
            ApplyForces(environment, delta);
            ApplyVelocity(delta);


            // for trail nodes
            foreach (var node in _nodes)
            {
                node.Update(gameTime, environment);
            }

            base.Update(gameTime, environment);
        }
    }
}
