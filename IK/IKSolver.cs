using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    abstract class IKSolver
    {
        abstract public Bone[] solveBoneChain(Bone[] bones, Bone target, Vector3 L1);

        // TODO probably better if we just keep length in bones... oor is it...
        protected void getDistances(out float[] distances, ref Bone[] bones)
        {
            distances = new float[bones.Length - 1];
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = (bones[i].Pos - bones[i + 1].Pos).Length;
            }
        }
    }
}
