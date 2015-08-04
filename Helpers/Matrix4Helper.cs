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
        /// Create matrix from position and three vectors
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="front">forward vector</param>
        /// <param name="up">up vector</param>
        /// <param name="right">right vector</param>
        /// <returns>matrix with cooridnate system based on vectors</returns>
        public static Matrix4 GetMatrix(Vector3 position, Vector3 front, Vector3 up, Vector3 right)
        {
            return new Matrix4(
                    new Vector4(right),
                    new Vector4(up),
                    new Vector4(front),
                    new Vector4(position.X, position.Y, position.Z, 1)
                );
        }

        /// <summary>
        /// Returns a rotation matrix with position <0,0,0> according to vector x and z
        /// </summary>
        /// <param name="x"> the x vector</param>
        /// <param name="z">the z vector</param>
        /// <returns>Rotation matrix</returns>
        public static Matrix4 GetOrientationMatrix(Vector3 x, Vector3 z)
        {
            x.NormalizeFast();
            z.NormalizeFast();
            Vector3 y = Vector3.Cross(z, x);
            return GetMatrix(Vector3.Zero, z, y, Vector3.Cross(y, z));
        }
    }

}
