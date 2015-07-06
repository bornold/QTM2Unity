﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    static class Vector3Helper
    {
        /// <summary>
        /// Defines a NaN Vector3.
        /// </summary>
        public static Vector3 NaN = new Vector3(float.NaN, float.NaN, float.NaN);

        /// <summary>
        /// The point in between two points 
        /// </summary>
        /// <param name="left vector">the first vector</param>
        /// <param name="right vector">the secound vector</param>
        /// <returns>Vector3 in between the points</returns>
        public static Vector3 MidPoint(this Vector3 leftVect, Vector3 rightVect)
        {
            return PointBetween(leftVect, rightVect, 0.5f);
        }
        /// <summary>
        /// The point d between two points 
        /// </summary>
        /// <param name="left vector">the first vector</param>
        /// <param name="right vector">the secound vector</param>
        /// <param name="distance">distans closeer to right</param>
        /// <returns>Vector3 in between the points</returns>
        public static Vector3 PointBetween(this Vector3 leftVect, Vector3 rightVect, float dist)
        {
            return (leftVect - rightVect) * dist + rightVect;
        }
        ///// <summary>
        ///// The Center point of three points
        ///// </summary>
        ///// <param name="right vector">the vector pointing forward</param>
        ///// <param name="left vector">the left vector</param>
        ///// <param name="right vector">the right vector</param>
        ///// <returns>Vector3 in the middle of three vectors</returns>
        //public static Vector3 MidPoint(Vector3 leftVect, Vector3 rightVect, Vector3 forwardVect)
        //{
        //    Vector3 backMid = leftVect.MidPoint(rightVect);
        //    return forwardVect + (backMid - forwardVect) * 2 / 3;
        //}

        //public static Vector3 GetHipForward(Vector3 sacrum, Vector3 leftHip, Vector3 rightHip)
        //{
        //    Vector3 backMid = (leftHip - rightHip) * 0.5f + rightHip;

        //    Vector3 front = backMid - (sacrum + (backMid - sacrum) * 2 / 3);

        //    front.Normalize();

        //    return front;
        //}
        /// <summary>
        /// Applies Gram-Schmitt Ortho-normalization to the given set of input Vectro3 objects.
        /// </summary>
        /// <param name="vector array">Array of Vector3 objects to be ortho-normalized</param>
        public static void OrthoNormalize(ref Vector3[] vecs)
        {
            for (int i = 0; i < vecs.Length; ++i)
            {
                Vector3 accum = Vector3.Zero;

                for (int j = 0; j < i; ++j)
                {
                    accum += ProjectAndCreate(vecs[i], vecs[j]);
                }

                vecs[i] = Vector3.Subtract(vecs[i], accum);
                vecs[i].Normalize();
            }
        }
        /// <summary>
        /// Projects Vector3 v1 onto Vector3 v2 and creates a new Vector3 for the result.
        /// </summary>
        /// <param name="vector 1"> Vector3 to be projected.</param>
        /// <param name="vector2">v2 Vector3 the Vector3 to be projected on.</param>
        /// <returns>The result of the projection.</returns>
        public static Vector3 ProjectAndCreate(Vector3 v1, Vector3 v2)
        {
            double d = Vector3.Dot(v1,v2);
            double d_div = d / v2.Length;
            return new Vector3 (v2 * (float)d_div);
        }

        public static Vector3 Project(Vector3 a, Vector3 b)
        {
            return (Vector3.Dot(a, b) / Vector3.Dot(b, b)) * b;

            //return new Vector3((Vector3.Dot(a, b) / Vector3.Dot(b, b)) * b);
        }

        //// sorry for another project function
        //// TODO need to decide which function to keep

        ///// <summary>
        ///// Projects Vector3 a onto Vector3 b and creates a new Vector3 for the result.
        ///// </summary>
        ///// <param name="a"> Vector to be projected.</param>
        ///// <param name="b"> Vector3 to project on.</param>
        ///// <returns>The result of the projection.</returns>
        //public static Vector3 ProjectOnVector(Vector3 a, Vector3 b)
        //{
        //    // The projection is defined as |a|cos(t) * norm(b) 
        //    // where t is the angle between the vectors
        //    float t = Vector3.CalculateAngle(a, b);
        //    return a.Length * Mathf.Cos(t) * Vector3.Normalize(b);
        //}

        ///// <summary>
        ///// Projects Vector3 v onto the plane with the given normal and creates 
        ///// a new Vector3 for the result
        ///// </summary>
        ///// <param name="v"> Vector to be projected</param>
        ///// <param name="normal">The normal of the plane the vector should be projected on</param>
        ///// <returns>The result of the projection</returns>
        //public static Vector3 ProjectOnPlane(Vector3 v, Vector3 normal)
        //{
        //    float dot = Vector3.Dot(v, normal);
        //    float magnitude = normal.Length;
        //    return new Vector3(v - ((dot / (magnitude * magnitude)) * normal));
        //}

        ///// <summary>
        ///// Translates a vector such that 'origin' is the new origin
        ///// </summary>
        ///// <param name="v"> Vector to be translated </param>
        ///// <param name="origin">The new origin</param>
        ///// <returns>The result of the translation</returns>
        //public static Vector3 translate(this Vector3 v, Vector3 origin)
        //{
        //    return v - origin;
        //}

        /// <summary>
        /// Check if any element in vector is NaN
        /// </summary>
        /// <param name="vector"> Vector to be checked </param>
        /// <returns>True if any of x, y, z is NaN</returns>
        public static bool IsNaN(this Vector3 v)
        {
            return float.IsNaN(v.X) || float.IsNaN(v.Y) ||  float.IsNaN(v.Z);
        }

        public static bool Parallel(Vector3 a, Vector3 b, float precision)
        {
            if (a.IsNaN() || b.IsNaN()) return true; // what?
            return Math.Abs((a.X / b.X) - (a.Y / b.Y)) < precision
                && Math.Abs((a.X / b.X) - (a.Z / b.Z)) < precision;
        }
    }
}
