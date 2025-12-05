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

        private VerticesRectangle _collisionRect;
        public float scale
        {  
            get;
            set;
        }

        public Microsoft.Xna.Framework.Rectangle DrawingRect // dependent on position at time of call, better than a variable
        {
            get
            {
                return new Microsoft.Xna.Framework.Rectangle((int)position.X,
                    (int)position.Y,
                    (int)(texture.Width * scale),
                    (int)(texture.Height * scale));

            }
        }
        public Microsoft.Xna.Framework.Rectangle SourceRect
        {
            get
            {
                return new Microsoft.Xna.Framework.Rectangle(
                    0,
                    0,
                    (int)(texture.Width),
                    (int)(texture.Height));
            }
        }
        //public VerticesRectangle CollisionRect
        //{
        //    get
        //    {
        //        return new VerticesRectangle(
        //            position,
        //            texture.Width,
        //            texture.Height,
        //            scale);
        //    }
        //    set;
        //}

        public VerticesRectangle CollisionRect
        {
            get => _collisionRect;

            set => _collisionRect = value;
        }
        public ScaledSprite(Texture2D texture, Vector2 position, float scale) : base(texture, position)
        {
            this.scale = scale;
        }

        public VerticesRectangle Set_collisionRect(VerticesRectangle tempRect)
        {
            return _collisionRect = tempRect;
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // used to be overrided in subclasses if needed
        }
        public virtual void Update(GameTime gameTime, Environment environment, Camera2D camera)
        {
            // used to be overrided if needed in subclasses
           
        }


    }
}
