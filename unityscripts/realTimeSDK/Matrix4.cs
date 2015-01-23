using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity.Unity
{
    static class Matrix4Helper
    {
        public static Matrix4 GetOrientation2(Vector3 forwardVect, Vector3 leftVect, Vector3 rightVect)
        {
            Vector3 backMid = (leftVect - rightVect) * 0.5f + rightVect;
            Vector3 mid = forwardVect + (backMid - forwardVect) * 2 / 3;

            Vector3 front = mid - backMid;
            Vector3 right = rightVect - backMid;

            front.Normalize();
            right.Normalize();

            Vector3 up = Vector3.Cross(front, right);

            Matrix4 mat = new Matrix4(new Vector4(mid), new Vector4(front), new Vector4(up), new Vector4(right));

            return mat;
        }
        /// <summary>
        /// Create a matrix for a coordinate system based off of three game objects
        /// </summary>
        /// <param name="forwardVect">gameobject in forward direction</param>
        /// <param name="leftVect">gameobject in left direction</param>
        /// <param name="rightVect">gameobject in right direction</param>
        /// <returns>matrix coordinate system based on gameobjects</returns>
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

            //mat.SetTRS(position, Quaternion.Identity, Vector3.One);
            Matrix4 translation, rotation, scale;
            translation = Matrix4.CreateTranslation(position);
            rotation = Matrix4.CreateFromAxisAngle(Vector3.One,0f);
            scale = Matrix4.Scale(1);

            mat = scale * rotation * translation;

            Vector4 up4v = new Vector4(up.X, up.Y, up.Z, 0);
            Vector4 front4v = new Vector4(front.X, front.Y, front.Z, 0);
            Vector4 right4v = new Vector4(right.X, right.Y, right.Z, 0);
            
            //mat.SetColumn(0, right4v);
            mat.M11 = right4v.X;
            mat.M21 = right4v.Y;
            mat.M31 = right4v.Z;
            mat.M41 = right4v.W;

            //mat.SetColumn(1, up4v);
            mat.M12 = up4v.X;
            mat.M22 = up4v.Y;
            mat.M32 = up4v.Z;
            mat.M42 = up4v.W;
            
            //mat.SetColumn(2, front4v);
            mat.M13 = front4v.X;
            mat.M23 = front4v.Y;
            mat.M33 = front4v.Z;
            mat.M43 = front4v.W;

            return mat;
        }
        public static Matrix4 LookAt(Vector3 root, Vector3 target, Vector3 up)
        {
            Matrix4 matrix = new Matrix4();
            Vector3 x, Y, Z;
            Z = root - target;
            Z.Normalize();
            Y = up;
            x = Vector3.Cross(Y, Z);
            Y = Vector3.Cross(Z, x);
            x.Normalize();
            Y.Normalize();

            //Matrix[0][0] = X.x;
            matrix.M11 = x.X;
            //Matrix[1][0] = X.y;
            matrix.M21 = x.Y;
            //Matrix[2][0] = X.z;
            matrix.M31 = x.Z;
            //Matrix[3][0] = -X.Dot( root );
            matrix.M41 = -(Vector3.Dot(x, root));
            //Matrix[0][1] = Y.x;
            matrix.M12 = Y.X;
            //Matrix[1][1] = Y.y;
            matrix.M22 = Y.Y;
            //Matrix[2][1] = Y.z;
            matrix.M32 = Y.Z;
            //Matrix[3][1] = -Y.Dot( root );
            matrix.M42 = -(Vector3.Dot(Y, root));
            //Matrix[0][2] = Z.x;
            matrix.M13 = Z.X;
            //Matrix[1][2] = Z.y;
            matrix.M23 = Z.Y;
            //Matrix[2][2] = Z.z;
            matrix.M33 = Z.Z;
            //Matrix[3][2] = -Z.Dot( root );
            matrix.M43 = -(Vector3.Dot(Z, root));
            //Matrix[0][3] = 0;
            matrix.M14 = 0;
            //Matrix[1][3] = 0;
            matrix.M24 = 0;
            //Matrix[2][3] = 0;
            matrix.M34 = 0;
            //Matrix[3][3] = 1.0f;
            matrix.M44 = 1.0f;
            return matrix;
        }
    }
}
