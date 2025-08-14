using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectilePlayground
{
    internal class ScaledSprite : Sprite
    {


        public int width;
        public int height;

        public Microsoft.Xna.Framework.Rectangle Rect
        {
            get
            {
                return new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y, width, height);
            }
        }
        public ScaledSprite(Texture2D texture, Vector2 position, int width, int height): base(texture, position)
        { 
            this.width = width;
            this.height = height;
        }


}
}
