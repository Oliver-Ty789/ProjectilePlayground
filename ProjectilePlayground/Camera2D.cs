using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Xna.Framework;
using static System.Formats.Asn1.AsnWriter;


namespace ProjectilePlayground
{
    internal class Camera2D
    {
        /// <summary>
        ///  the camera object is only ever in one spot, therefore its position is readonly,
        ///  and the only other attributes determine it's zoom qualities due to its abstracted use case
        /// </summary>

        private readonly Vector2 position = new Vector2(0, 650);
        private float zoom = 1f;
        private Matrix camScaleMatrix = Matrix.Identity;
        private Vector3 scaleVector3 = Vector3.Zero;
        private bool IsScaling = false;

        public Camera2D()
        {

        }

        public float Zoom()
        {
            return zoom;
        }
        public void Zoom(float value)
        {
            zoom += value;
            if (zoom < 0.5f)
            {
                zoom = 0.5f;
            }
            if (zoom > 2f)
            {
                zoom = 2f;
            }
            IsScaling = true;
            Console.WriteLine($"zoom  {zoom}");
        }

       
        public Matrix GetCameraScaleMatrix()
        {
            /// only need to return a Matrix if acutally tried to scale this tick
            /// 

            Matrix transform = Matrix.Identity;
            if (IsScaling)
            {
                scaleVector3.X = zoom;
                scaleVector3.Y = zoom;
                scaleVector3.Z = 1f;

               
                Matrix TranslateToCamera = Matrix.CreateTranslation(-position.X, -position.Y, 0f);
                Matrix TranslateToWorld = Matrix.CreateTranslation(position.X, position.Y, 0f);
                Matrix.CreateScale(ref scaleVector3, out camScaleMatrix);
                
                transform = TranslateToCamera * camScaleMatrix * TranslateToWorld;
                
            }

            return transform;

        }
    }
}
