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

        override public Bone[] solveBoneChain(Bone[] bones, Bone target, Vector3 L1)
        {
            // Calculate distances 
            float[] distances;
            getDistances(out distances, ref bones);

            double dist = Math.Abs((bones[0].Pos - target.Pos).Length);
            if (dist > distances.Sum()) // the target is unreachable
            {
                return targetUnreachable(ref distances, bones, target.Pos);
            }

            // The target is reachable
            Vector3 root = bones[0].Pos;
            while ((bones[bones.Length - 1].Pos - target.Pos).Length > threshold)
            {
                // Forward reaching
                forwardReaching(ref bones, ref distances, target);
                
                // Backward reaching
                backwardReaching(ref bones, ref distances, root, L1);
            }

            return bones;
        }

        private Bone[] targetUnreachable(ref float[] distances, Bone[] bones, Vector3 target)
        {
            for (int i = 0; i < distances.Length; i++)
            {
                // position
                float r = (target - bones[i].Pos).Length;
                float l = distances[i] / r;

                bones[i + 1].Pos = ((1 - l) * bones[i].Pos) + (l * target);

                // orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
            }
            return bones;
        }

        private void forwardReaching(ref Bone[] bones, ref float[] distances, Bone target)
        {
            bones[bones.Length-1] = target;
            for (int i = bones.Length - 2; i >= 0; i--)
            {
                // Position
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;

                bones[i].Pos = (1 - l) * bones[i + 1].Pos + l * bones[i].Pos;

                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
                
                if (bones[i + 1].RotationalConstraint != null && bones[i].RotationalConstraint != null)
                {
                    Vector4 constra = bones[i + 1].RotationalConstraint.Constraints;
                    Vector3 joint = bones[i + 1].Pos;
                    Vector3 targ = bones[i].Pos;
                    Vector3 dir = -bones[i + 1].GetDirection();
                    Vector3 res;
                    if (bones[i].RotationalConstraint.RotationalConstraints(targ, joint, dir, constra, out res))
                    {
                        bones[i].Pos = res;
                        bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
                    }
                }
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
                if (bones[i].RotationalConstraint != null)
                {
                    Vector4 constra = bones[i].RotationalConstraint.Constraints;
                    Vector3 joint = bones[i].Pos;
                    Vector3 targ = bones[i+1].Pos;
                    Vector3 dir = (i > 0) ? bones[i].Pos - bones[i - 1].Pos : L1;
                    Vector3 res;
                    if (bones[i].RotationalConstraint.RotationalConstraints(targ, joint, dir, constra, out res))
                    {
                        bones[i+1].Pos = res;
                        bones[i+1].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
                    }
                }
            }
        }
    }
}
