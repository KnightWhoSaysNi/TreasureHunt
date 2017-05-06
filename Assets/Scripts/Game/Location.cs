using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable]
    public struct Location
    {
        public Location(float latitude, float longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
