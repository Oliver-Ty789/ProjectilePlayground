using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectilePlayground.Content.controls
{
    internal class Slider : Button
    {
        // slider is a special type of button that we are going to manipulate to choose specific values for our projectile

        public Slider(Texture2D texture, Vector2 position, float scale, SpriteFont font) : base (texture, position, scale, font)
        {

        }
    }
}
