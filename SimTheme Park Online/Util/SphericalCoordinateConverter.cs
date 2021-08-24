using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SimTheme_Park_Online.Util
{
    /// <summary>
    /// Converts Spherical Coordinates in C# by Jeremy Glazebrook
    /// </summary>
    public class SphericalCoordinateConverter
    {
        public static Vector3 ToCartesian(float radius, float theta, float omega)
        {
            float X = (float)(radius * Math.Cos(theta) * Math.Sin(omega));
            float Y = (float)(radius * Math.Sin(theta) * Math.Sin(omega));
            float Z = (float)(radius * Math.Cos(omega));
            return new Vector3(X, Y, Z);
        }
        public static Vector3 ToSpherical(float X, float Y, float Z)
        {
            float radius = (float)Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
            float theta = (float)Math.Atan2(Y, X);
            float omega = (float)Math.Acos(Z / radius);
            return new Vector3(radius, theta, omega);
        }
    }
}
