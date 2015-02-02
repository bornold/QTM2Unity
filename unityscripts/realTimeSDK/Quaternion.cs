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
        public static Func<float, float> Sqrt = power => (float)Math.Sqrt(power);
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

        /// <summary>
        /// Quaternion from matrix
        /// </summary>
        /// <param name="matrix">matrix</param>
        /// <returns></returns>
        public static Quaternion FromMatrix(Matrix4 matrix)
        {
            float[] matrixArray = new float[9];
            matrixArray[0] = matrix.M11; //m00;
            matrixArray[1] = matrix.M21; //m10;
            matrixArray[2] = matrix.M31; //m20;
            matrixArray[3] = matrix.M12; //m01;
            matrixArray[4] = matrix.M22; //m11;
            matrixArray[5] = matrix.M32; //m21;
            matrixArray[6] = matrix.M13; //m02;
            matrixArray[7] = matrix.M23; //m12;
            matrixArray[8] = matrix.M33; //m22;

            return FromMatrix(matrixArray);
        }

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
        public static Quaternion LookAt(Vector3 root, Vector3 target, Vector3 up)
        {
            return FromMatrix(Matrix4Helper.LookAt(root, target, up));
        }

        /// <summary>
        /// Calculates the angle in radians for a rotation around a specific axis
        /// </summary>
        /// <param name="quaternion">the rotation quaternion</param>
        /// <param name="front">the specific axis</param>
        /// <returns>the rotation angle around the given axis</returns>
        public static float getAngleAroundRad(this Quaternion quaternion, Vector3 axis)
        {
            float d = Vector3.Dot(quaternion.Xyz, axis);
            float l = length(axis.X * d, axis.Y * d, axis.Z * d, quaternion.W);

            return (l == 0) ? 0f : (float)(2.0 * Math.Acos(clamp((float) (quaternion.W / Math.Sqrt(l)), -1f, 1f)));
        }

        /// <summary>
        /// Calculates the angle in degrees for a rotation around a specific axis
        /// </summary>
        /// <param name="quaternion">the rotation quaternion</param>
        /// <param name="front">the specific axis</param>
        /// <returns>the rotation angle around the given axis</returns>
        public static float getAngleAround(this Quaternion quaternion, Vector3 axis)
        {
            return radiansToDegrees(getAngleAroundRad(quaternion, axis));
        }

        public static float length (float x, float y, float z, float w) 
        {
		    return x * x + y * y + z * z + w * w;
	    }

        // TODO move to some math util (not quaternion specific)
        public static float clamp (float value, float min, float max) 
        {
		    if (value < min) return min;
		    if (value > max) return max;
		    return value;
	    }

        public static float degreesToRadians(float degrees)
        {
            return degrees * (Mathf.PI / 180);
        }
    
        public static float radiansToDegrees(float radians)
        {
            return radians * (180 / Mathf.PI);
        }
    }
}
