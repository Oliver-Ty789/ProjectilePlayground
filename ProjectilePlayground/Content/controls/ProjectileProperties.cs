using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace ProjectilePlayground.Content.controls
{
    public class ProjectileProperties 
    {
        /// <summary>
        ///  this is to store all the properties of a projectile selected within the simulation
        ///  from the csv file to be used in Game1
        /// </summary>

        [Name("Name")]
        public string name { get; set; }

        [Name("path")]
        public string path { get; set; }

        [Name("radius (m)")]
        public float radius { get; set; }

        [Name("mass (kg)")]
        public float mass { get; set; }

        [Name("linear resistance coefficient")]
        public float linearDragCoefficient { get; set; }

        [Name("angular resistance coefficient")]
        public float angularDragCoefficient { get; set; }

        // Note: header includes misspelling and trailing space so attribute matches exactly
        [Name("coeffiecent of resitution ")]
        public float coeffiecentOfResitution { get; set; }

        [Name("scale")]
        public float scale { get; set; }
    }


}
 
