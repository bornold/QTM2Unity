using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    class FABRIK : IKSolver
    {
        private static float threshold = 0.001f; // TODO define a good default threshold value
                                                  // Place in IKSolver instead?

        override public Bone[] solveBoneChain(Bone[] bones, Bone target, Vector3 L1)
        {
            UnityEngine.Debug.Log("solving chain");
            foreach (Bone b in bones) UnityEngine.Debug.Log(b.Name + b.Pos);
            // Calculate distances 
            float[] distances;
            getDistances(out distances, ref bones);

            double dist = Math.Abs((bones[0].Pos - target.Pos).Length);
            if (dist > distances.Sum()) // the target is unreachable
            {
                //UnityEngine.Debug.Log("target unreachable");
                return targetUnreachable(ref distances, bones, target.Pos, L1);
            }

            // The target is reachable
            Vector3 root = bones[0].Pos;
            int iterations = 0;
            bones[bones.Length - 1].Orientation = target.Orientation;
            while ((bones[bones.Length - 1].Pos - target.Pos).Length > threshold && iterations++ < 25)
            {
                // Forward reaching
                forwardReaching(ref bones, ref distances, target);
                
                // Backward reaching
                backwardReaching(ref bones, ref distances, root, L1);
            }

            return bones;
        }

        private Bone[] targetUnreachable(ref float[] distances, Bone[] bones, Vector3 target, Vector3 L1)
        {
            for (int i = 0; i < distances.Length; i++)
            {
                // Position
                float r = (target - bones[i].Pos).Length;
                float l = distances[i] / r;
                bones[i + 1].Pos = ((1 - l) * bones[i].Pos) + (l * target);
                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
                
                // Constraints
                Vector3 dir = (i > 0) ? bones[i].Pos - bones[i - 1].Pos : L1;
                bones[i].EnsureConstraints(ref bones[i + 1], dir, true);
                
                

            }
            return bones;
        }

        private void forwardReaching(ref Bone[] bones, ref float[] distances, Bone target)
        {
            bones[bones.Length - 1].Pos = target.Pos;
            bones[bones.Length - 1].Orientation = target.Orientation;
            for (int i = bones.Length - 2; i >= 0; i--)
            {
                // Position
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;
                bones[i].Pos = (1 - l) * bones[i + 1].Pos + l * bones[i].Pos;

                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);

                // Constraints
                bones[i+1].EnsureConstraints(ref bones[i], -bones[i+1].GetDirection(), (i>0));

            }
        }

        private void backwardReaching(ref Bone[] bones, ref float[] distances, Vector3 root, Vector3 L1)
        {
            bones[0].Pos = root;
            for (int i = 0; i < bones.Length-1; i++)
            {
                // Position
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;
                bones[i+1].Pos = (1 - l) * bones[i].Pos + l * bones[i+1].Pos;

                // Orientation
                bones[i].RotateTowards(bones[i+1].Pos - bones[i].Pos);

                // Constraints
                Vector3 dir = (i > 0) ? bones[i].Pos - bones[i - 1].Pos : L1;
                bones[i].EnsureConstraints(ref bones[i + 1], dir, ( i+1 < bones.Length - 1));
                
            }
        }
    }
}
