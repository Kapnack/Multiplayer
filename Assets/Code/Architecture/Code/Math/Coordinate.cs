using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Code.Architecture.Code.Math
{
    public struct Coordinate
    {
        public float x;
        public float y;
        public float z;

        public Coordinate(float x, float y, float z)
        {
            this.x = x; 
            this.y = y; 
            this.z = z;
        }

        static public Coordinate Zero => new Coordinate(0, 0, 0);
    }
}
