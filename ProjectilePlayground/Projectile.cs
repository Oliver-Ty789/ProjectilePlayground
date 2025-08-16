using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectilePlayground
{
    internal class Projectile : ScaledSprite
    {
        public Vector2 initial_velocity;
        public int mass;
        public Vector2 velocity;
        public double initial_angle;
        // public float C_of_D; // unimportant at this time
        //public float C_of_E;
        //public float angluar_velocity;

        public Projectile(Texture2D texture, Vector2 position, float scale, Vector2 intial_v, int mass, double intial_a) : base (texture, position, scale)
        {
            this.initial_velocity = intial_v;
            this.mass = mass;
            this.velocity = intial_v;
            this.initial_angle = intial_a;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
