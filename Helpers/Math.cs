using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    public static class Mathf
    {
        public static Func<float, float> Cos = angleR => (float)Math.Cos(angleR);
        public static Func<float, float> Sin = angleR => (float)Math.Sin(angleR);
        public static Func<float, float> Acos = angleR => (float)Math.Acos(angleR);
        public static Func<float, float> Tan = angleR => (float)Math.Tan(angleR);
        public static Func<float, float> Sqrt = power => (float)Math.Sqrt(power);
        public static float PI = (float)Math.PI;


        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        
        /// <summary>
        /// Finding the nearest point on an ellipse from the given point
        /// According to http://www.geometrictools.com/Documentation/DistancePointEllipseEllipsoid.pdf
        /// eMax >= eMin > 0, point.X > 0, point.Y > 0
        /// values must be in first quadrant? (+,+)
        /// </summary>
        /// <param name="eMax">The larger radius of the ellipse</param>
        /// <param name="eMin">The smaller radius of the ellipse</param>
        /// <param name="point">The point of which the closest point the ellipse exists</param>
        /// <returns>A Point on the ellipse defined by eMax and eMin with a minimum distance to the given point</returns>
        public static Vector2 FindNearestPointOnEllipse(float eMax, float eMin, Vector2 point)
        {
            if (point.Y > 0)
            {
                if (point.X > 0)
                {
                    float z0 = point.X / eMax;
                    float z1 = point.Y / eMin;
                    float g = (float)(Math.Pow(z0, 2) + Math.Pow(z1, 2) - 1);

                    if (g != 0)
                    {
                        float r0 = (float)Math.Pow(eMax / eMin, 2);
                        float sbar = GetRoot(r0, z0, z1, g);
                        
                        float x0 = r0 * point.X / (sbar + r0);
                        float x1 = point.Y / (sbar + 1);
                        return new Vector2(x0, x1);
                    }
                    else
                    {
                        return point;
                    }
                }
                else // point.X == 0
                {
                    return new Vector2(0, eMin);
                }
            }
            else // point.Y == 0
            {
                float numer0 = eMax* point.X;
                float denom0 = (float)(Math.Pow(eMax, 2) - Math.Pow(eMin, 2));
                if (numer0 < denom0)
                {
                    float xde0 = numer0 / denom0;
                    float x0 = eMax* xde0;
                    float x1 = (float)(eMin * Math.Sqrt(1 - xde0*xde0));
                    return new Vector2(x0, x1);
                }
                else
                {
                    return new Vector2(eMax, 0);
                }
            }
        }
        

        /// <summary>
        /// According to the document http://www.geometrictools.com/Documentation/DistancePointEllipseEllipsoid.pdf
        /// finds the root of the 
        /// </summary>
        /// <param name="r0"></param>
        /// <param name="z0"></param>
        /// <param name="z1"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static float GetRoot(float r0, float z0, float z1, float g)
        {
            int maxIterations = 149; 

            float n0 = r0 *z0;
            float s0 = z1 - 1;
            float s1 = (g < 0 ? 0 : RobustLength(n0, z1) -1);
            float s = 0;

            for (int i = 0; i < maxIterations; i++ )
            {
                s = (s0 + s1) / 2;
                if ( s == s0 || s == s1)
                    break;
                
                float ratio0 = n0 / (s + r0);
                float ratio1 = z1 / (s + 1);
                g = (float)(Math.Pow(ratio0, 2) + Math.Pow(ratio1, 2) -1);
                
                if (g > 0)
                {
                    s0 = s;
                }
                else if (g < 0)
                {
                    s1 = s;
                }
                else
                {
                    break;
                }
            }
            return s; 
        }

        /// <summary>
        /// computes the length of the vector (v0, v1) by avoiding floating-point overflow that could occur 
        /// normally when computing v0^2 + v1^0 
        /// (ref http://www.geometrictools.com/Documentation/DistancePointEllipseEllipsoid.pdf)
        /// 
        /// </summary>
        /// <param name="v0">First component of the vector</param>
        /// <param name="v1">Secound component of the vector</param>
        /// <returns>The length of the vector</returns>
        private static float RobustLength(float v0, float v1)
        {
             if (Math.Max(Math.Abs(v0), Math.Abs(v1)) == Math.Abs(v0))
             {
                 return (float)(Math.Abs(v0) * Math.Sqrt(1 + Math.Pow(v1/v0, 2)));
             }
             else
             {
                 return (float)(Math.Abs(v1) * Math.Sqrt(1 + Math.Pow(v0 / v1, 2)));
             }
        }
    }
}
