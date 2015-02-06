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
        public static Func<float, float> Acos = angleR => (float)Math.Acos(angleR);
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
        /// Rotates a vector according to a quaternion
        /// </summary>
        /// <param name="Vector">the quaternion to rotate from</param>
        /// <param name="Vector">the vector to be rotated</param>
        /// <returns>The rotated vector</returns>
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
            return FromMatrix(Matrix4Helper.LookAtUp(root, target, up));
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
            return MathHelper.RadiansToDegrees(getAngleAroundRad(quaternion, axis));
        }

        public static float length (float x, float y, float z, float w) 
        {
		    return x * x + y * y + z * z + w * w;
	    }

        // TODO move to quaternion helper or something
        // Returns a quaternion representing the rotation from vector a to b
        public static Quaternion getRotation(Vector3 a, Vector3 b)
        {
            a.Normalize();
            b.Normalize();

            float precision = 0.99f; // TODO not sure if good value
            if (Vector3.Dot(a, b) > precision) // a and b are parallel
            {
                return Quaternion.Identity;
            }
            if (Vector3.Dot(a, b) < -precision) // a and b are opposite
            {
                return Quaternion.Normalize(Quaternion.FromAxisAngle(new Vector3(1, 1, 1), Mathf.PI));
            }

            float angle = Vector3.CalculateAngle(a, b);
            Vector3 axis = Vector3.Cross(a, b);
            axis.Normalize();

            return Quaternion.Normalize(Quaternion.FromAxisAngle(axis, angle));
        }

        // TODO move to some math util (not quaternion specific)
        public static float clamp (float value, float min, float max) 
        {
		    if (value < min) return min;
		    if (value > max) return max;
		    return value;
	    }
        /// <summary>
        /// Get hip orientation 
        /// </summary>
        /// <param name="sacrum">position vector of sacrum marker</param>
        /// <param name="leftHip">position vector of left hip marker</param>
        /// <param name="rightHip">position vector of right hip marker</param>
        /// <returns>Quaternion with rotation of hip</returns>
        public static Quaternion GetHipOrientation(Vector3 sacrum, Vector3 leftHip, Vector3 rightHip)
        {
            Vector3 hipMarkerMid = (leftHip - rightHip) * 0.5f + rightHip;


            Vector3 hipMid = sacrum + (hipMarkerMid - sacrum) * 2 / 3;
            Vector3 right = hipMarkerMid - rightHip;
            Vector3 front = hipMid - sacrum;

            front.Normalize();
            right.Normalize();
            Vector3 up = Vector3.Cross(right, front);
            Quaternion ret = fromAxes(right, up, front);

            ret.Normalize();


            return ret;
        }


        /// </summary>
        /// <param name="sourcePoint">Coordinates of source point</param>
        /// <param name="destPoint">Coordinates of destionation point</param>
        /// <returns></returns>
        public static Quaternion LookAt(Vector3 c, Vector3 p)
        {

            Vector3 forwardVector = Vector3.Normalize(c - p);

            float dot = Vector3.Dot(Vector3.UnitZ, forwardVector);

            if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                return new Quaternion(Vector3.UnitZ, Mathf.PI);
            }
            if (Math.Abs(dot - (1.0f)) < 0.000001f)
            {
                return Quaternion.Identity;
            }

            float rotAngle = (float)Math.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(Vector3.UnitZ, forwardVector);
            rotAxis = Vector3.Normalize(rotAxis);
            return Quaternion.FromAxisAngle(rotAxis, rotAngle);

        }


        public static Quaternion lookAt(Vector3 lookAt, Vector3 upDirection, bool isCamera)
        {
            Vector3 forward = lookAt;
            Vector3 up = upDirection;
            Vector3[] vecs = new Vector3[2];
            vecs[0] = forward; vecs[1] = up;

            Vector3Helper.OrthoNormalize(ref vecs);

            Vector3 right = Vector3.Cross(forward, up);// forward.clone().cross(up);

            Quaternion camera = new Quaternion(), ret = new Quaternion();
            camera = fromAxes(right, up, forward);

            if (isCamera)
            {
                return camera;
            }
            else
            {
                ret = Quaternion.Identity;
                ret = Quaternion.Multiply(ret, camera); //ret.multiply(camera);
                ret = Quaternion.Invert(ret);//ret.inverseSelf();

                return ret;
            }
        }

        public static Quaternion fromAxes(Vector3 xAxis, Vector3 yAxis, Vector3 zAxis)
        {
            return fromAxes(xAxis.X, xAxis.Y, xAxis.Z, yAxis.X, yAxis.Y, yAxis.Z, zAxis.X, zAxis.Y, zAxis.Z);
        }

        public static Quaternion fromAxes(double xx, double xy, double xz, double yx, double yy, double yz,
                double zx, double zy, double zz)
        {
            // The trace is the sum of the diagonal elements; see
            // http://mathworld.wolfram.com/MatrixTrace.html
            double m00 = xx, m01 = xy, m02 = xz;
            double m10 = yx, m11 = yy, m12 = yz;
            double m20 = zx, m21 = zy, m22 = zz;
            double t = m00 + m11 + m22;

            //Protect the division by s by ensuring that s >= 1
            double x, y, z, w;
            if (t >= 0)
            {
                double s = Math.Sqrt(t + 1); // |s| >= 1
                w = 0.5 * s; // |w| >= 0.5
                s = 0.5 / s; //<- This division cannot be bad
                x = (m21 - m12) * s;
                y = (m02 - m20) * s;
                z = (m10 - m01) * s;
            }
            else if ((m00 > m11) && (m00 > m22))
            {
                double s = Math.Sqrt(1.0 + m00 - m11 - m22); // |s| >= 1
                x = s * 0.5; // |x| >= 0.5
                s = 0.5 / s;
                y = (m10 + m01) * s;
                z = (m02 + m20) * s;
                w = (m21 - m12) * s;
            }
            else if (m11 > m22)
            {
                double s = Math.Sqrt(1.0 + m11 - m00 - m22); // |s| >= 1
                y = s * 0.5; // |y| >= 0.5
                s = 0.5 / s;
                x = (m10 + m01) * s;
                z = (m21 + m12) * s;
                w = (m02 - m20) * s;
            }
            else
            {
                double s = Math.Sqrt(1.0 + m22 - m00 - m11); // |s| >= 1
                z = s * 0.5; // |z| >= 0.5
                s = 0.5 / s;
                x = (m02 + m20) * s;
                y = (m21 + m12) * s;
                w = (m10 - m01) * s;
            }
            return new Quaternion((float)x, (float)y, (float)z, (float)w);
        }

    }
}
