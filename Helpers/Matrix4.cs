using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
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
        /// Create a transformation matrix for a coordinate system based off of three points
        /// </summary>
        /// <param name="forwardPoint">point in forward direction</param>
        /// <param name="leftPoint">point in left direction</param>
        /// <param name="rightPoint">point in right direction</param>
        /// <returns>matrix coordinate system based on points with the mid points as transformation</returns>
        public static Matrix4 GetOrientation(Vector3 forwardPoint, Vector3 leftPoint, Vector3 rightPoint)
        {
            Vector3 backMid = (leftPoint - rightPoint) * 0.5f + rightPoint;
            Vector3 mid = forwardPoint + (backMid - forwardPoint) * 2 / 3;

            Vector3 front = mid - backMid;
            Vector3 right = rightPoint - backMid;

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
            //Vector4 pos = new Vector4(root);
            mat.Row0 = right4v;
            mat.Row1 = up4v;
            mat.Row2 = front4v;
            
            return mat;
        }
        /// <summary>
        /// creates a transformation matrix from a two position vectors and a direction vector defined as up (Z)
        /// </summary>
        /// <param name="root">position, wher to start from</param>
        /// <param name="target">target vector</param>
        /// <param name="up">up vector, the direction defined the Z axis should alinge as good as possible</param>
        /// <returns>transformation matrix with Y axis towards target and Z as close to up vector as possible</returns>
        public static Matrix4 LookAtUp(Vector3 root, Vector3 target, Vector3 up)
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
            return matrix;
        }
        /// <summary>
        /// creates a transformation matrix from a two position vectors and a direction vector defined as right (X)
        /// </summary>
        /// <param name="root">position, wher to start from</param>
        /// <param name="target">target vector</param>
        /// <param name="right">right vector, the direction defined the X axis should alinge as good as possible</param>
        /// <returns>transformation matrix with Y axis towards target and X as close to right vector as possible</returns>
        public static Matrix4 LookAtRight(Vector3 root, Vector3 target, Vector3 right)
        {
            Matrix4 matrix = new Matrix4();
            Vector3 X, Y, Z;
            Y = target - root;
            Y.Normalize();
            X = right;
            Z = Vector3.Cross(Y, X);
            X = Vector3.Cross(Z, Y);
            X.Normalize();
            Z.Normalize();
            matrix = GetMatrix(root, Z, Y, X);

            return matrix;
        }
    }
}
