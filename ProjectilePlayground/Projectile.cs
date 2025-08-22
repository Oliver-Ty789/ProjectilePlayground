using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectilePlayground
{
    internal class Projectile : ScaledSprite
    {
        public int mass;
        public Vector2 velocity;
        public float initial_angle;
        //private Vector2 Force;
        public double radius;
        public float initial_speed;
        // public float C_of_D; // unimportant at this time
        //public float C_of_E;
        //public float angluar_velocity;

        public Projectile(Texture2D texture, Vector2 position, float scale, float initial_s, int mass, float initial_a, double radius) : base (texture, position, scale)
        {
            this.mass = mass;
            this.initial_speed = initial_s;
            this.initial_angle = initial_a;
            this.radius = radius;

            velocity = new Vector2((float)(initial_s * Math.Cos(initial_a * Math.PI / 180)), -(float)(initial_s * Math.Sin(initial_a * Math.PI / 180)));
        }
   
        public void ApplyVelocity(float delta)
        {
            this.position += this.velocity*delta;

        }

        public void ApplyForces(Environment environment, float delta)
        {
            // gravity
            if (position.Y < 640)
            {
                velocity += environment.gravity * delta;
            }
            else
            {
                velocity = new Vector2(0, 0);
            }
        }

        public float ConversionToSI()

            // 1. find how many pixels in radius
            // 2. find how many radius' make a meter
            // 3. use that scale to find pixels to meter

        {
            double radiusP = (texture.Width * scale) /2; // finds the radius of the projectile in pixels
            double radiusPerMeter = 1/radius; // eg if radius = 0.5 therefore there would be 2 radius' per meter
            float pixelsToMeter = Convert.ToSingle(radiusPerMeter * radiusP);
            return pixelsToMeter;
        }

        public override void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(texture, Rect, Color.White);
            base.Draw(gameTime, _spriteBatch);
        }

        public override void Update(GameTime gameTime, Environment environment)
        {

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds; // difference in time between frames, keeps velocity/acceleration consitent 
            ApplyForces(environment, delta);
            ApplyVelocity(delta);
            base.Update(gameTime, environment);
        }
    }
}
