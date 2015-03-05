using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    class FABRIK : IKSolver
    {
        private static float threshold = 0.0001f; // TODO define a good default threshold value
                                                  // Place in IKSolver instead?

        override public Bone[] solveBoneChain(Bone[] bones, Vector3 target)
        {
            // Calculate distances 
            float[] distances;
            getDistances(out distances, bones);

            double dist = Math.Abs((bones[0].Pos - target).Length);
            if (dist > distances.Sum()) // the target is unreachable
            {
                return targetUnreachable(ref distances, bones, target);
            }

            // The target is reachable
            Vector3 root = bones[0].Pos;
            while ((bones[bones.Length-1].Pos - target).Length > threshold)
            {
                // Forward reaching
                forwardReaching(ref bones, ref distances, target);
                
                // Backward reaching
                backwardReaching(ref bones, ref distances, root);
            }

            return bones;
        }

        // TODO probably better if we just keep length in bones... oor is it...
        private void getDistances(out float[] distances, Bone[] bones)
        {
            distances = new float[bones.Length-1];
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = (bones[i].Pos - bones[i + 1].Pos).Length;
            }
        }

        private Bone[] targetUnreachable(ref float[] distances, Bone[] bones, Vector3 target)
        {
            for (int i = 0; i < distances.Length; i++)
            {
                // position
                float r = Math.Abs((target - bones[i].Pos).Length);
                float l = distances[i] / r;

                bones[i + 1].Pos = ((1 - l) * bones[i].Pos) + (l * target);

                // orientation
                bones[i].rotateTowards(bones[i + 1].Pos - bones[i].Pos);
            }
            return bones;
        }

        private void forwardReaching(ref Bone[] bones, ref float[] distances, Vector3 target)
        {
            bones[bones.Length-1].Pos = target;
            for (int i = bones.Length - 2; i >= 0; i--)
            {
                // Position
                float r = Math.Abs((bones[i + 1].Pos - bones[i].Pos).Length);
                float l = distances[i] / r;

                bones[i].Pos = (1 - l) * bones[i + 1].Pos + l * bones[i].Pos;

                // Orientation
                bones[i].rotateTowards(bones[i + 1].Pos - bones[i].Pos);
            }
        }

        private void backwardReaching(ref Bone[] bones, ref float[] distances, Vector3 root)
        {
            bones[0].Pos = root;
            for (int i = 0; i < bones.Length-1; i++)
            {
                // Position
                float r = Math.Abs((bones[i + 1].Pos - bones[i].Pos).Length);
                float l = distances[i] / r;

                bones[i+1].Pos = (1 - l) * bones[i].Pos + l * bones[i+1].Pos;

                // Orientation
                bones[i].rotateTowards(bones[i+1].Pos - bones[i].Pos);
            }
        }
    }
}
