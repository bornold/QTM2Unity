using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    abstract class IKSolver
    {
        abstract public Bone[] solveBoneChain(Bone[] bones, Bone target, Quaternion pRot);

        // TODO probably better if we just keep length in bones... oor is it...
        protected void getDistances(out float[] distances, ref Bone[] bones)
        {
            distances = new float[bones.Length - 1];
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = (bones[i].Pos - bones[i + 1].Pos).Length;
            }
        }
        protected void ForwardKinematics(ref Bone[] bones, Quaternion rotation, int i)
        {
            for (int j = bones.Length - 1; j >= i; j--)
            {
                if (j > i)
                {
                    bones[j].Pos = bones[i].Pos +
                        OpenTK.Vector3.Transform((bones[j].Pos - bones[i].Pos), rotation);
                }

                // rotate orientation
                bones[j].Rotate(rotation);
            }
        }
    }
}
