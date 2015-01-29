using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity.Unity
{
    static class Matrix4Helper
    {
        /// <summary>
        /// Get hip orientation 
        /// </summary>
        /// <param name="sacrum">position vector of sacrum marker</param>
        /// <param name="leftHip">position vector of left hip marker</param>
        /// <param name="rightHip">position vector of right hip marker</param>
        /// <returns>Transformation matrix of middle of hip and a its rotation, scale 1</returns>
        public static Matrix4 GetHipOrientation(Vector3 sacrum, Vector3 leftHip, Vector3 rightHip)
        {
            
            Vector3 hipMarkerMid = (leftHip - rightHip ) * 0.5f + rightHip;
            Vector3 hipMid = sacrum + (hipMarkerMid - sacrum) * 2 / 3;
            Vector3 right = hipMarkerMid - rightHip;
            Vector3 front = hipMid - sacrum;

            front.Normalize();
            right.Normalize();

            Vector3 up = Vector3.Cross(right, front);

            Matrix4 hip = GetMatrix(hipMid, front, up, right);

            return hip;
        
        }
        /// <summary>
        /// Create a transformation matrix for a coordinate system based off of three vectors
        /// </summary>
        /// <param name="forwardVect">vector in forward direction</param>
        /// <param name="leftVect">vector in left direction</param>
        /// <param name="rightVect">vector in right direction</param>
        /// <returns>matrix coordinate system based on vectors with the mid points as transformation</returns>
        public static Matrix4 GetOrientation(Vector3 forwardVect, Vector3 leftVect, Vector3 rightVect)
        {
            Vector3 backMid = (leftVect - rightVect) * 0.5f + rightVect;
            Vector3 mid = forwardVect + (backMid - forwardVect) * 2 / 3;

            Vector3 front = mid - backMid;
            Vector3 right = rightVect - backMid;

            front.Normalize();
            right.Normalize();

            Vector3 up = Vector3.Cross(front, right);

            Matrix4 mat = GetMatrix(mid, front, up, right);

            return mat;
        }
        /// <summary>
        /// Create matrix from position and three vectors
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="front">forward vector</param>
        /// <param name="up">up vector</param>
        /// <param name="right">right vector</param>
        /// <returns>matrix with cooridnate system based on vectors</returns>
        public static Matrix4 GetMatrix(Vector3 position, Vector3 front, Vector3 up, Vector3 right)
        {
            Matrix4 mat = Matrix4.Identity;
            
            Matrix4 translation, rotation, scale;
            translation = Matrix4.CreateTranslation(position);
            rotation = Matrix4.CreateFromAxisAngle(Vector3.One,0f);
            scale = Matrix4.Scale(1);

            mat = scale * rotation * translation;
            
            Vector4 right4v = new Vector4(right);
            Vector4 up4v = new Vector4(up);
            Vector4 front4v = new Vector4(front);
            
            mat.Row0 = right4v;
            mat.Row1 = up4v;
            mat.Row2 = front4v;
            return mat;
        }
        /// <summary>
        /// creates a transformation matrix from a two position vectors and a direction vector defined as up
        /// </summary>
        /// <param name="position">position, wher to start from</param>
        /// <param name="front">target vector</param>
        /// <param name="right">right vector</param>
        /// <param name="up">up vector, the direction defined the Z axis should alinge as good as possible</param>
        /// <returns>transformation matrix with Y axis towards target and Z as close to up vector as possible</returns>
        public static Matrix4 LookAt(Vector3 root, Vector3 target, Vector3 up)
        {
            Matrix4 matrix = new Matrix4();
            Vector3 X, Y, Z;
            Y = target - root; 
            Y.Normalize();
            Z = up;
            X = Vector3.Cross(Z, Y);
            Z = Vector3.Cross(Y,X);
            X.Normalize();
            Z.Normalize();
            matrix = GetMatrix(root, Z, Y, X);
            /*
            matrix.M11 = X.X;
            matrix.M12 = X.Y;
            matrix.M13 = X.Z;
            matrix.M14 = -(Vector3.Dot(X, root));
            matrix.M21 = Y.X;
            matrix.M22 = Y.Y;
            matrix.M23 = Y.Z;
            matrix.M24 = -(Vector3.Dot(Y, root));
            matrix.M31 = Z.X;
            matrix.M32 = Z.Y;
            matrix.M33 = Z.Z;
            matrix.M34 = -(Vector3.Dot(Z, root));
            matrix.M41 = 0;
            matrix.M42 = 0;
            matrix.M43 = 0;
            matrix.M44 = 1.0f;
            */
            return matrix;
        }
    }
}
