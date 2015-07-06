using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{  
    public static class QuaternionHelper
    {
        public static float precision = 0.9999f;
        /// <summary>
        /// Defines the zerp quaternion.
        /// </summary>
        public static Quaternion Zero = new Quaternion(0, 0, 0, 0);

        /// <summary>
        /// Check if any element in quaternion is NaN
        /// </summary>
        /// <param name="quaternion"> Quaternion to be checked </param>
        /// <returns>True if any of x, y, z, w is NaN</returns>
        public static bool IsNaN(this Quaternion q)
        {
            return q.Xyz.IsNaN() || float.IsNaN(q.W);
        }

        /// <summary>
        /// Rotation around X axis
        /// </summary>
        /// <param name="radians">rotation amount</param>   
        /// <returns>Quaternion with rotation</returns>
        public static Quaternion RotationX(float radians)
        {
            float s, c, angle;
            angle = (radians * 0.5f);
            s = Mathf.Sin(angle);
            c = Mathf.Cos(angle);
            return new Quaternion(s, 0.0f, 0.0f, c);
        }

        /// <summary>
        /// Rotation around Y axis
        /// </summary>
        /// <param name="radians">rotation amount</param>
        /// <returns>Quaternion with rotation</returns>
        public static Quaternion RotationY(float radians)
        {
            float s, c, angle;
            angle = (radians * 0.5f);
            s = Mathf.Sin(angle);
            c = Mathf.Cos(angle);
            return new Quaternion(0.0f, s, 0.0f, c);
        }

        /// <summary>
        /// Rotation around Z axis
        /// </summary>
        /// <param name="radians">rotation amount</param>
        /// <returns>Quaternion with rotation</returns>
        public static Quaternion RotationZ(float radians)
        {
            float s, c, angle;
            angle = (radians * 0.5f);
            s = Mathf.Sin(angle);
            c = Mathf.Cos(angle);
            return new Quaternion(0.0f, 0.0f, s, c);
        }

        /// <summary>
        /// Quaternion from Matrix4 matrix
        /// </summary>
        /// <param name="matrix">Matrix4 matrix</param>
        /// <returns>Quaternion</returns>
        public static Quaternion FromMatrix(Matrix4 matrix)
        {
            float[] matrixArray = new float[9];
            matrixArray[0] = matrix.M11;
            matrixArray[1] = matrix.M21;
            matrixArray[2] = matrix.M31;
            matrixArray[3] = matrix.M12;
            matrixArray[4] = matrix.M22;
            matrixArray[5] = matrix.M32;
            matrixArray[6] = matrix.M13;
            matrixArray[7] = matrix.M23;
            matrixArray[8] = matrix.M33;
            return FromMatrix(matrixArray);
        }
        /// <summary>
        /// Quaternion from matrix array
        /// </summary>
        /// <param name="array float">size nine array rep of a rotation matrix</param>
        /// <returns>Quaternion</returns>
        public static Quaternion FromMatrix(float[] matrix)
        {
            float trace, radicand, scale, xx, yx, zx, xy, yy, zy, xz, yz, zz, tmpx, tmpy, tmpz, tmpw, qx, qy, qz, qw;
            bool negTrace, ZgtX, ZgtY, YgtX;
            bool largestXorY, largestYorZ, largestZorX;

            xx = matrix[0];
            yx = matrix[1];
            zx = matrix[2];
            xy = matrix[3];
            yy = matrix[4];
            zy = matrix[5];
            xz = matrix[6];
            yz = matrix[7];
            zz = matrix[8];

            trace = ((xx + yy) + zz);

            negTrace = (trace < 0.0);
            ZgtX = zz > xx;
            ZgtY = zz > yy;
            YgtX = yy > xx;
            largestXorY = (!ZgtX || !ZgtY) && negTrace;
            largestYorZ = (YgtX || ZgtX) && negTrace;
            largestZorX = (ZgtY || !YgtX) && negTrace;

            if (largestXorY)
            {
                zz = -zz;
                xy = -xy;
            }
            if (largestYorZ)
            {
                xx = -xx;
                yz = -yz;
            }
            if (largestZorX)
            {
                yy = -yy;
                zx = -zx;
            }

            radicand = (((xx + yy) + zz) + 1.0f);
            scale = (0.5f * (1.0f / Mathf.Sqrt(radicand)));

            tmpx = ((zy - yz) * scale);
            tmpy = ((xz - zx) * scale);
            tmpz = ((yx - xy) * scale);
            tmpw = (radicand * scale);
            qx = tmpx;
            qy = tmpy;
            qz = tmpz;
            qw = tmpw;

            if (largestXorY)
            {
                qx = tmpw;
                qy = tmpz;
                qz = tmpy;
                qw = tmpx;
            }
            if (largestYorZ)
            {
                tmpx = qx;
                tmpz = qz;
                qx = qy;
                qy = tmpx;
                qz = qw;
                qw = tmpz;
            }

            return new Quaternion(qx, qy, qz, qw);
        }


        /// <summary>
        /// Calculates the difference in rotation between two Quaternions
        /// if result is 0, there is no diffrence between the Quaternions
        /// if the results is 1, the diffrence is 180 degrees diffrence 
        /// </summary>
        /// <param name="a">The first quaternion</param>
        /// <param name="b">The secound quaternion</param>
        /// <returns>float between 0 and 1 where 0 the Quaternions are the same, and 1 they are at a 180 degrees diffrences</returns>
        public static float DiffrenceBetween(Quaternion right, Quaternion left)
        {
            float dot = left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
            
            return 1f - Mathf.Sqrt(dot);
        }

        ///// <summary>
        ///// Rotates a vector according to a quaternion
        ///// </summary>
        ///// <param name="Vector">the quaternion to rotate from</param>
        ///// <param name="Vector">the vector to be rotated</param>
        ///// <returns>The rotated vector</returns>
        //public static Vector3 Rotate(Quaternion quaternion, Vector3 vec)
        //{
        //    float tmpX, tmpY, tmpZ, tmpW;
        //    tmpX = (((quaternion.W * vec.X) + (quaternion.Y * vec.Z)) - (quaternion.Z * vec.Y));
        //    tmpY = (((quaternion.W * vec.Y) + (quaternion.Z * vec.X)) - (quaternion.X * vec.Z));
        //    tmpZ = (((quaternion.W * vec.Z) + (quaternion.X * vec.Y)) - (quaternion.Y * vec.X));
        //    tmpW = (((quaternion.X * vec.X) + (quaternion.Y * vec.Y)) + (quaternion.Z * vec.Z));
        //    return new Vector3(
        //        ((((tmpW * quaternion.X) + (tmpX * quaternion.W)) - (tmpY * quaternion.Z)) + (tmpZ * quaternion.Y)),
        //        ((((tmpW * quaternion.Y) + (tmpY * quaternion.W)) - (tmpZ * quaternion.X)) + (tmpX * quaternion.Z)),
        //        ((((tmpW * quaternion.Z) + (tmpZ * quaternion.W)) - (tmpX * quaternion.Y)) + (tmpY * quaternion.X))
        //   );
        //}

        //private static float Norm(this Quaternion quaternion )
        //{
        //    return quaternion.X * quaternion.X + 
        //           quaternion.Y * quaternion.Y + 
        //           quaternion.Z * quaternion.Z +
        //           quaternion.W * quaternion.W;
        //}

        //public static void Normalize(this Quaternion quaternion)
        //{        
        //    Quaternion.Normalize()
        //    //float lenSqr = quaternion.norm();
        //    float lenInv = (1.0f / Mathf.Sqrt(quaternion.Norm()));
        //    quaternion.X = (quaternion.X * lenInv);
        //    quaternion.Y = (quaternion.Y * lenInv);
        //    quaternion.Z = (quaternion.Z * lenInv);
        //    quaternion.W = (quaternion.W * lenInv);

        //}


        ///// <summary>
        ///// Calculates the angle in radians for a rotation around a specific axis
        ///// </summary>
        ///// <param name="quaternion">the rotation quaternion</param>
        ///// <param name="front">the specific axis</param>
        ///// <returns>the rotation angle around the given axis</returns>
        //public static float GetAngleAroundRad(this Quaternion quaternion, Vector3 axis)
        //{
        //    float d = Vector3.Dot(quaternion.Xyz, axis);
        //    float l = length(axis.X * d, axis.Y * d, axis.Z * d, quaternion.W);

        //    return (l == 0) ? 0f : (float)(2.0 * Math.Acos(Mathf.Clamp((float) (quaternion.W / Math.Sqrt(l)), -1f, 1f)));
        //}

        ///// <summary>
        ///// Calculates the angle in degrees for a rotation around a specific axis
        ///// </summary>
        ///// <param name="quaternion">the rotation quaternion</param>
        ///// <param name="front">the specific axis</param>
        ///// <returns>the rotation angle around the given axis</returns>
        //public static float GetAngleAround(this Quaternion quaternion, Vector3 axis)
        //{
        //    return OpenTK.MathHelper.RadiansToDegrees(GetAngleAroundRad(quaternion, axis));
        //}

    

        // Returns a quaternion representing the rotation from vector a to b
        public static Quaternion GetRotationBetween(Vector3 a, Vector3 b, float stiffness = 1f)
        {
            if ((a == Vector3.Zero || b == Vector3.Zero) || (a == b) || Vector3Helper.Parallel(a,b,1-precision))
                return Quaternion.Identity; 

            a.NormalizeFast();
            b.NormalizeFast();

            if (Vector3.Dot(a, b) < -precision) // a and b are opposite
            {
                return Quaternion.Normalize(Quaternion.FromAxisAngle(Vector3.UnitZ, Mathf.PI));
            }

            return Quaternion.FromAxisAngle(Vector3.Cross(a, b), Vector3.CalculateAngle(a, b) * stiffness);
        }

        public static Quaternion GetRotation2(Vector3 a, Vector3 b)
        {
            a.NormalizeFast();
            b.NormalizeFast();
            if (Vector3.Dot(a, b) > precision) // a and b are parallel
            {
                return Quaternion.Identity;
            }
            return Quaternion.FromAxisAngle(Vector3.NormalizeFast(Vector3.Cross(a, b)), Vector3.CalculateAngle(a, b));
        }
        

        /// <summary>
        /// Get orientation of three points
        /// </summary>
        /// <param name="back">position vector of back marker</param>
        /// <param name="left">position vector of left marker</param>
        /// <param name="right">position vector of right marker</param>
        /// <returns>Quaternion rotation</returns>
        public static Quaternion GetOrientation(Vector3 forwardPoint, Vector3 leftPoint, Vector3 rightPoint)
        {
            Vector3 backMid = (leftPoint - rightPoint) * 0.5f + rightPoint;

            Vector3 front = forwardPoint - backMid;
            Vector3 right = rightPoint - leftPoint;
            front.NormalizeFast();
            right.NormalizeFast();

            Vector3 up = Vector3.Cross(front, right);
            Quaternion frontRot = GetRotation2(Vector3.UnitZ, front);
            Vector3 possibleUp = Vector3.Transform(Vector3.UnitY, frontRot);
            Quaternion upRot = GetRotation2(possibleUp, up);

            Quaternion orientation = upRot * frontRot;
            return orientation;
        }

        ///// <summary>
        ///// Get orientation from three points
        ///// </summary>
        ///// <param name="sacrum">position vector of sacrum marker</param>
        ///// <param name="leftHip">position vector of left hip marker</param>
        ///// <param name="rightHip">position vector of right hip marker</param>
        ///// <returns>Quaternion with rotation of hip</returns>
        //public static Quaternion GetHipOrientation(Vector3 sacrum, Vector3 leftHip, Vector3 rightHip)
        //{
        //    Vector3 hipMarkerMid = (leftHip - rightHip) * 0.5f + rightHip;
        //    Vector3 right = leftHip - rightHip;
        //    Vector3 front = hipMarkerMid - sacrum;
        //    front.Normalize();
        //    right.Normalize();
        //    Vector3 up = Vector3.Cross(right, front);
        //    Quaternion frontRot = GetRotation2(Vector3.UnitZ, front);
        //    Vector3 possibleUp = Vector3.Transform(Vector3.UnitY, frontRot);
        //    Quaternion upRot = GetRotation2(possibleUp, up);

        //    Quaternion orientation = upRot * frontRot;
        //    return orientation;
        //}
        /// <summary>
        /// Get quaternion with rotation as Y axis towards target as close as z parameter as possible
        /// </summary>
        /// <param name="source">position vector to look from</param>
        /// <param name="leftHip">position vector to look at</param>
        /// <param name="rightHip">direction Z axis</param>
        /// <returns>Quaternion with rotation to target</returns>
        public static Quaternion LookAtUp(Vector3 source, Vector3 target, Vector3 z)
        {
            Vector3 y = target - source;
            Vector3[] normal = { y, z };
            Vector3Helper.OrthoNormalize(ref normal);
            y = normal[0];
            z = normal[1];
            //Vector3 x = Vector3.Cross(z, y);

            Quaternion zRot = GetRotation2(Vector3.UnitZ, z);
            //Vector3 possibleY = Vector3.Transform(Vector3.UnitY, zRot);

            //Quaternion yRot = GetRotation2(possibleY, y);
            //Quaternion orientation = yRot * zRot;
            return GetRotation2(Vector3.Transform(Vector3.UnitY, zRot), y) * zRot;
        }
        /// <summary>
        /// Get quaternion with rotation as Y axis towards target and X towards right parameter
        /// </summary>
        /// <param name="source">position vector to look from</param>
        /// <param name="target">position vector to look at</param>
        /// <param name="X axis">direction vector of defenition of x axis</param>
        /// <returns>Quaternion with rotation to target</returns>
        public static Quaternion LookAtRight(Vector3 source, Vector3 target, Vector3 x)
        {
            Vector3 y = target - source;
            Vector3[] normal = { y, x };
            Vector3Helper.OrthoNormalize(ref normal);
            y = normal[0];
            x = normal[1];
            //Vector3 z = Vector3.Cross(y, x);
            Quaternion zRot = GetRotation2(Vector3.UnitZ, Vector3.Cross(y, x));
            //Vector3 possibleY = Vector3.Transform(Vector3.UnitY, zRot);

            //Quaternion yRot = GetRotation2(possibleY, y);
            //Quaternion orientation = yRot * zRot;
            return GetRotation2(Vector3.Transform(Vector3.UnitY, zRot), y) * zRot;
        }
        /// <summary>
        /// Get quaternion with front and right vector
        /// </summary>
        /// <param name="source">Front vector</param>
        /// <param name="target">right vector</param>
        /// <returns>Quaternion with rotation</returns>
        public static Quaternion GetOrientationFromYX(Vector3 y, Vector3 x)
        {
            Vector3[] normal = { y, x };
            Vector3Helper.OrthoNormalize(ref normal);
            y = normal[0];
            x = normal[1];
            //Vector3 z = Vector3.Cross(y, x);
            Quaternion zRot = GetRotation2(Vector3.UnitZ, Vector3.Cross(y, x));
            //Vector3 possibleY = Vector3.Transform(Vector3.UnitY, zRot);

            //Quaternion yRot = GetRotation2(possibleY, y);
            //Quaternion orientation = yRot * zRot;
            return GetRotation2(Vector3.Transform(Vector3.UnitY, zRot), y) * zRot;
        }
        public static Quaternion RotationBetween(Vector3 from, Vector3 to)
        {
            //float angle = Vector3.CalculateAngle(from, to);
            //Vector3 axis = Vector3.Cross(from, to);
            return Quaternion.FromAxisAngle(Vector3.Cross(from, to), Vector3.CalculateAngle(from, to));
        }
    }
}
