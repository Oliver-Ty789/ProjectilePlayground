using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ProjectilePlayground
{
    internal class Environment
    {
        public Vector2 gravity;
        //public string playstate;
        //public Vector2 Wind;
        public Environment(Vector2 gravity) 
        { 
            this.gravity = gravity;
        }
    }
}
