using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    class DampedLeastSquares : Jacobian
    {

        override protected void calculateDTheta(out float[,] dTheta, ref Vector3[,] J,
            ref Bone[] bones, ref Bone target)
        {
            // dTheta = J.transpose * Inverse(J*J.transpose + lambda^2*I) * e
            float lambda = 1.1f; 
            Vector3 e = target.Pos - bones[bones.Length - 1].Pos;
            Vector3[,] JT = transpose(J);
            float[,] JJT = mult(J, JT);
            // Obs: JTT[0,0] only works because we have one end effector only
            dTheta = mult(mult(JT, (1 / JJT[0,0] * lambda * lambda)), e);
        }

        override protected void calculateDTheta(out float[] dTheta, ref Vector3[,] J,
            ref Bone[] endEffectors, ref Bone[] targets)
        {
            throw new NotImplementedException();
        }
    }
}
