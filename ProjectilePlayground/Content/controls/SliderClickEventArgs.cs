using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectilePlayground.Content.controls
{
    internal class SliderClickEventArgs : EventArgs
    {
        public float property;
        public int index;
        public SliderClickEventArgs(float property, int index)
            { 
            this.property = property; 
            this.index = index;
            }
    }
}
