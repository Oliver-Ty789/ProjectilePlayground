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
    sealed class Projectile : ScaledSprite
    {
        //public int mass;
        public Vector2 initialVelocity;
        //public float initialAngle;
        //public float dragCoefficient;
        //public float angularDragCoefficient;
        public float radius;
        public Vector2 previousVelocity;
        //public float angluarVelocity;
        //public Vector2 resultantForce;
        //// public float C_of_D; // unimportant at this time
        ////public float C_of_E;
        public List<TrailNode> _nodes;
        public RigidBody body;

        // private
        //private Vector2 resistiveLinearForce;
        //private float resistiveAngularForce;
        //private Vector2 magnusForce;
        //private float spriteRotation;
        //private Vector2 initialPos;
        private VerticesRectangle _collisionRect;
        

        public Projectile(Texture2D texture, Vector2 position, float scale, float initial_s, int mass, float initial_a, float radius, float linearDragCoefficient, float angularVelocity, float angularDragCoefficient, float restitution) : base (texture, position, scale)
        {
            //this.mass = mass;
            
            //this.initialAngle = initial_a;
            this.radius = radius;
            //this.initialPos = position;
            //this.dragCoefficient = linearDragCoefficient;
            //this.angluarVelocity = angularVelocity;
            //this.angularDragCoefficient = angularDragCoefficient;
            //this.magnusForce = new Vector2(0, 0);
            //this.resultantForce = new Vector2(0, 0);
            _collisionRect = new VerticesRectangle(position, texture.Width, texture.Height, scale);
            CollisionRect = _collisionRect;
            initialVelocity = VectorMaths.ToVector2(initial_s, initial_a);

            body = RigidBody.CreateCircleBody(texture, position, scale, initialVelocity, restitution, radius, mass, false, angularVelocity, linearDragCoefficient, angularDragCoefficient);

            _nodes = new List<TrailNode> { };
            
            
        }
        //private void ApplyMagnus()
        //{
        //    var unitVelocity = VectorMaths.UnitVector(body.linearVelocity);
        //    var perpendicularVector = new Vector2(-unitVelocity.Y, unitVelocity.X);
        //    var magnusForceMag = dragCoefficient * angluarVelocity * VectorMaths.Length(body.linearVelocity);
        //    magnusForce = perpendicularVector * magnusForceMag;
        //    //Console.WriteLine(dragCoefficient);
        //}
        //private void ApplyDrag(Environment environment)
        //{
        //    // applying a formula to determine drag forces
        //    var area = body.area;
        //    resistiveLinearForce = new Vector2(Convert.ToSingle(-dragCoefficient * 0.5 * environment.airPressure * area * MathF.Pow(body.linearVelocity.X, 2)),
        //        Convert.ToSingle(dragCoefficient * 0.5 * environment.airPressure * area * MathF.Pow(body.linearVelocity.Y, 2)));

        //    resistiveAngularForce = Convert.ToSingle(0.5 * angularDragCoefficient * MathF.Pow(angluarVelocity, 2) * environment.airPressure * area);
        //}
        //private void ApplyForces(Environment environment, float delta)
        //{
        //    // gravity
        //    resultantForce = environment.gravity * mass;
            

        //    // drag
        //    resultantForce += resistiveLinearForce;
            

        //    // magnus
        //    resultantForce += magnusForce;
            


        //    if (position.Y < initialPos.Y + 1 && !(body.linearVelocity == new Vector2(0,0)))
        //    {
        //        previousVelocity = body.linearVelocity;
        //        // F = ma
        //        var acceleration = new Vector2(resultantForce.X / mass, resultantForce.Y / mass);
                
        //        body.linearVelocity += acceleration * delta;
        //        if (angluarVelocity > 0)
        //            angluarVelocity -= resistiveAngularForce;
        //        else
        //            angluarVelocity += resistiveAngularForce;
        //        spriteRotation += angluarVelocity * delta;
        //    }
        //    else
        //    {
        //        body.linearVelocity = new Vector2(0, 0);
        //        resistiveLinearForce = new Vector2(0,0);
        //        magnusForce = new Vector2(0, 0);
        //        resultantForce = new Vector2(0, 0);
        //        position.Y = initialPos.Y;
        //    }
        //}

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

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds; // difference in time between frames, keeps velocity/acceleration consitent 
            //ApplyDrag(environment);
            //ApplyMagnus();
            //ApplyForces(environment, delta);
            ////ApplyVelocity(delta);
            previousVelocity = body.linearVelocity;
            body.Update(gameTime, environment);
            position = body.position;
            // for trail nodes
            foreach (var node in _nodes)
            {
                node.Update(gameTime, environment);
            }

            base.Update(gameTime, environment);
        }
    }
}
