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
        public float dragCoefficient;
        public float radius;
        public Vector2 previousVelocity;
        private Vector2 resistiveForce;
        // public float C_of_D; // unimportant at this time
        //public float C_of_E;
        //public float angluar_velocity;



        public List<TrailNode> _nodes;

        public Projectile(Texture2D texture, Vector2 position, float scale, float initial_s, int mass, float initial_a, float radius, float dragCoefficient) : base (texture, position, scale)
        {
            this.mass = mass;
            
            this.initialAngle = initial_a;
            this.radius = radius;
            this.initialPos = position;
            this.dragCoefficient = dragCoefficient;

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
            resistiveForce = new Vector2(Convert.ToSingle(dragCoefficient * 0.5 * environment.airPressure * area * MathF.Pow(velocity.X, 2)),
                Convert.ToSingle(-dragCoefficient * 0.5 * environment.airPressure * area * MathF.Pow(velocity.Y, 2)));
        }

        private void ApplyForces(Environment environment, float delta)
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
                resistiveForce = new Vector2(0,0);
                position.Y = initialPos.Y;
                return;
            }

            // drag
           // Console.WriteLine(resistiveForce);
            velocity -= resistiveForce * delta;
            
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
            _spriteBatch.Draw(texture, DrawingRect, Color.White);

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
            ApplyDrag(environment);
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
