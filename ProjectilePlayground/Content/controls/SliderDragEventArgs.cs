using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectilePlayground.Content.controls
{
    internal class SliderDragEventArgs : EventArgs
    {
        public Vector2 scrollerPosition { get; }
        public SliderDragEventArgs(Vector2 scrollerPosition)
        {
            this.scrollerPosition = scrollerPosition;
        }

    }
}
