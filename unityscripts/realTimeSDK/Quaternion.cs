using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity.Unity
{
    public static class Mathf
    {
        public static Func<float, float> Cos = angleR => (float)Math.Cos(angleR);
        public static Func<float, float> Sin = angleR => (float)Math.Sin(angleR);
        public static Func<float, float> Sqrt = angleR => (float)Math.Sqrt(angleR);
        public static float PI = (float)Math.PI;
    }   
    public static class QuaternionHelper
    {
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

        //TODO find a Matrix4 that is not Unity
        /// <summary>
        /// Quaternion from matrix
        /// </summary>
        /// <param name="matrix">matrix</param>
        /// <returns></returns>
       /* public static Quaternion FromMatrix(Matrix4x4 matrix)
        {
            float[] matrixArray = new float[9];
            matrixArray[0] = matrix.m00;
            matrixArray[1] = matrix.m10;
            matrixArray[2] = matrix.m20;
            matrixArray[3] = matrix.m01;
            matrixArray[4] = matrix.m11;
            matrixArray[5] = matrix.m21;
            matrixArray[6] = matrix.m02;
            matrixArray[7] = matrix.m12;
            matrixArray[8] = matrix.m22;

            return FromMatrix(matrixArray);
        }
        */
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

        public static Vector3 Rotate(this Quaternion quaternion, Vector3 vec)
        {
            float tmpX, tmpY, tmpZ, tmpW;
            tmpX = (((quaternion.W * vec.X) + (quaternion.Y * vec.Z)) - (quaternion.Z * vec.Y));
            tmpY = (((quaternion.W * vec.Y) + (quaternion.Z * vec.X)) - (quaternion.X * vec.Z));
            tmpZ = (((quaternion.W * vec.Z) + (quaternion.X * vec.Y)) - (quaternion.Y * vec.X));
            tmpW = (((quaternion.X * vec.X) + (quaternion.Y * vec.Y)) + (quaternion.Z * vec.Z));
            return new Vector3(
                ((((tmpW * quaternion.X) + (tmpX * quaternion.W)) - (tmpY * quaternion.Z)) + (tmpZ * quaternion.Y)),
                ((((tmpW * quaternion.Y) + (tmpY * quaternion.W)) - (tmpZ * quaternion.X)) + (tmpX * quaternion.Z)),
                ((((tmpW * quaternion.Z) + (tmpZ * quaternion.W)) - (tmpX * quaternion.Y)) + (tmpY * quaternion.X))
           );
        }

        private static float norm(this Quaternion quaternion )
        {
            double result;
            result = (quaternion.X * quaternion.X);
            result = (result + (quaternion.Y * quaternion.Y));
            result = (result + (quaternion.Z * quaternion.Z));
            result = (result + (quaternion.W * quaternion.W));
            return (float)result;
        }

        public static Quaternion Normalize(this Quaternion quaternion)
        {        
            float lenSqr = quaternion.norm();
            float lenInv = (1.0f / Mathf.Sqrt(lenSqr));
            return new Quaternion(
                        (quaternion.X * lenInv),
                        (quaternion.Y * lenInv),
                        (quaternion.Z * lenInv),
                        (quaternion.W * lenInv)
                        );
        }
    
    }
}
