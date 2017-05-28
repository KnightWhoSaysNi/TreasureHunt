using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable]
    public class Location : IEquatable<Location>
    {
        public Location(float latitude, float longitude, float radius = 100)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Radius = radius;
        }

        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Radius { get; set; }        

        public bool Equals(Location other)
        {
            return (this.Latitude == other.Latitude) && (this.Longitude == other.Longitude);
        }
    }
}
