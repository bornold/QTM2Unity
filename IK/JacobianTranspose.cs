using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Debug = UnityEngine.Debug;

namespace QTM2Unity
{
    class JacobianTranspose : Jacobian
    {
        override protected void calculateDTheta(out float[,] dTheta, ref Vector3[,] J, 
            ref Bone[] bones, ref Bone target)
        {
            Vector3[,] JT = transpose(J);
            Vector3 e = target.Pos - bones[bones.Length - 1].Pos;
            float JJT = mult(J, JT)[0, 0]; // Obs: only works because we only have one end effector

            // alpha = dot(e, J * JT * e) / dot(J * JT * e, J * JT * e);
            float alpha = Vector3.Dot(e, JJT * e) / Vector3.Dot(JJT * e, JJT * e);

            dTheta = mult(alpha, mult(JT, e));
        }

        override protected void calculateDTheta(out float[] dTheta, ref Vector3[,] J,
            ref Bone[] endEffectors, ref Bone[] targets)
        {
            Vector3[,] JT = transpose(J);
            Vector3[] e = new Vector3[targets.Length];
            for (int i = 0; i < e.Length; i++)
            {
                e[i] = targets[i].Pos - endEffectors[i].Pos;
            }
            float[,] JJT = mult(J, JT);

            // alpha = dot(e, J * JT * e) / dot(J * JT * e, J * JT * e);
            float alpha = mult(e, mult(JJT, e)) / mult(mult(JJT, e), mult(JJT, e));

            dTheta = mult(alpha, mult(JT, e));
        }
    }
}
