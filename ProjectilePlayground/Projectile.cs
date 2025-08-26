using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectilePlayground
{
    internal class Projectile : ScaledSprite
    {
        private Vector2 initialPos;

        public int mass;
        public Vector2 velocity;
        public float initialAngle;
        //private Vector2 Force;
        public float radius;
        public Vector2 previousVelocity;
        // public float C_of_D; // unimportant at this time
        //public float C_of_E;
        //public float angluar_velocity;



        public List<TrailNode> _nodes;

        public Projectile(Texture2D texture, Vector2 position, float scale, float initial_s, int mass, float initial_a, float radius) : base (texture, position, scale)
        {
            this.mass = mass;
            
            this.initialAngle = initial_a;
            this.radius = radius;
            this.initialPos = position;

            _nodes = new List<TrailNode> { };

            velocity = new Vector2((float)(initial_s * Math.Cos(initial_a * Math.PI / 180)), -(float)(initial_s * Math.Sin(initial_a * Math.PI / 180)));
        }
   
        public void ApplyVelocity(float delta)
        {
            this.position += this.velocity*delta;

        }

        public void ApplyForces(Environment environment, float delta)
        {
            // gravity
            if (position.Y < initialPos.Y + 1 && !(velocity == new Vector2(0,0)))
            {
                previousVelocity = velocity;
                velocity += environment.gravity * delta;
            }
            else
            {
                velocity = new Vector2(0, 0);
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
            _spriteBatch.Draw(texture, Rect, Color.White);

            foreach (var node in _nodes)
            {
                node.Draw(gameTime, _spriteBatch);
            }
            //Console.WriteLine(_nodes.Count());

            base.Draw(gameTime, _spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment environment)
        {

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds; // difference in time between frames, keeps velocity/acceleration consitent 
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
