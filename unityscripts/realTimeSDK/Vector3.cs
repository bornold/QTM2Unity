using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity.Unity
{
    static class Vector3Helper
    {
        /// <summary>
        /// The point in between two points 
        /// </summary>
        /// <param name="left vector">the first vector</param>
        /// <param name="right vector">the secound vector</param>
        /// <returns>Vector3 in between the points</returns>
        public static Vector3 MidPoint(this Vector3 leftVect, Vector3 rightVect)
        {
            return (leftVect - rightVect) * 0.5f + rightVect;
        }
        /// <summary>
        /// The Center point of three points
        /// </summary>
        /// <param name="right vector">the vector pointing forward</param>
        /// <param name="left vector">the left vector</param>
        /// <param name="right vector">the right vector</param>
        /// <returns>Vector3 in the middle of three vectors</returns>
        public static Vector3 MidPoint(this Vector3 leftVect, Vector3 rightVect, Vector3 forwardVect)
        {
            Vector3 backMid = leftVect.MidPoint(rightVect);
            return forwardVect + (backMid - forwardVect) * 2 / 3;
        }

        public static Vector3 GetHipForward(Vector3 sacrum, Vector3 leftHip, Vector3 rightHip)
        {
            Vector3 backMid = (leftHip - rightHip) * 0.5f + rightHip;
            Vector3 mid = sacrum + (backMid - sacrum) * 2 / 3;

            Vector3 front = backMid - mid;

            front.Normalize();

            return front;
        }

    }
}
