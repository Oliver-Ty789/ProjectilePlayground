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


        public float scale;

        public Microsoft.Xna.Framework.Rectangle Rect // dependent on position at time of call, better than a variable
        {
            get
            {
                return new Microsoft.Xna.Framework.Rectangle((int)position.X, 
                    (int)position.Y, 
                    (int)(texture.Width * scale), 
                    (int)(texture.Height * scale));

            }
        }
        public ScaledSprite(Texture2D texture, Vector2 position, float scale): base(texture, position)
        {
            this.scale = scale;
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // used to be overrided in subclasses if needed
        }
        public virtual void Update(GameTime gameTime)
        {
            // used to be overrided if needed in subclasses
        }


}
}
