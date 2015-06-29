using System;
using System.Linq;
using OpenTK;

namespace QTM2Unity
{
    class FABRIK : IKSolver
    {
        override public bool SolveBoneChain(Bone[] bones, Bone target, Bone parent)
        {
            // Calculate distances 
            float[] distances;
            GetDistances(out distances, ref bones);

            double dist = Math.Abs((bones[0].Pos - target.Pos).Length);
            if (dist > distances.Sum()) // the target is unreachable
            {
                TargetUnreachable(bones, target.Pos, parent);
                return true;
            }

            // The target is reachable
            int numberOfBones = bones.Length;
            bones[numberOfBones - 1].Orientation = target.Orientation;
            Vector3 root = bones[0].Pos;
            int iterations = 0;
            float lastDistToTarget = float.MaxValue;

            float distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            while (distToTarget > threshold && iterations++ < maxIterations && distToTarget < lastDistToTarget)
            {
                // Forward reaching
                ForwardReaching(ref bones, ref distances, target);
                // Backward reaching
                BackwardReaching(ref bones, ref distances, root, parent);

                lastDistToTarget = distToTarget;
                distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            }
            bones[bones.Length - 1].Orientation = target.Orientation;

            return (distToTarget <= threshold);
        }



        private void ForwardReaching(ref Bone[] bones, ref float[] distances, Bone target)
        {
            
            bones[bones.Length - 1].Pos = target.Pos;
            bones[bones.Length - 1].Orientation = target.Orientation; //TODO if bone is endeffector, we should not look at rot constraints
            for (int i = bones.Length - 2; i >= 0; i--)
            {
                SamePosCheck(ref bones, i);

                // Position
                Vector3 newPos;
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;
                // bones[i].Pos = (1 - l) * bones[i + 1].Pos + l * bones[i].Pos;
                newPos = (1 - l) * bones[i + 1].Pos + l * bones[i].Pos;

                bones[i].Pos = newPos;

                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
            }
        }

        private void BackwardReaching(ref Bone[] bones, ref float[] distances, Vector3 root, Bone parent)
        {

            bones[0].Pos = root;

            for (int i = 0; i < bones.Length - 1; i++)
            {
                SamePosCheck(ref bones, i);
                Vector3 newPos;
                // Position
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;

                newPos = (1 - l) * bones[i].Pos + l * bones[i + 1].Pos;

                Bone prevBone = (i > 0) ? bones[i - 1] : parent;
                if (bones[i].HasConstraints)
                {
                    Vector3 res;
                    Quaternion rot;
                    newPos =
                        Constraint.CheckRotationalConstraints(bones[i], prevBone.Orientation, newPos, out res, out rot) ?
                        res : newPos;
                }
                bones[i + 1].Pos = newPos;
                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos,bones[i].Stiffness);
                if (bones[i].HasConstraints)
                {
                    Quaternion rot;
                    if (Constraint.CheckOrientationalConstraint(bones[i], prevBone, out rot))
                    {
                        bones[i].Rotate(rot);
                    }

                }
            }
        }
        private void SamePosCheck(ref Bone[] bones, int i) {
            if (bones[i+1].Pos == bones[i].Pos)
            {
                float small = 0.001f;
                // move one of them a small distance along the chain
                if (i+2 < bones.Length)
                {
                    Vector3 pushed = Vector3.Normalize(bones[i + 2].Pos - bones[i + 1].Pos) * small;
                        bones[i + 1].Pos += 
                            !pushed.IsNaN() ? 
                            pushed : 
                            new Vector3(small, small, small); ;
                }
                else if (i - 1 >= 0)
                {
                    Vector3 pushed = bones[i - 1].Pos +
                        Vector3.Normalize(bones[i - 1].Pos - bones[i].Pos) * small;
                    bones[i].Pos += !pushed.IsNaN() ? pushed : new Vector3(small, small, small); ;
                }
            }
        }
    }
}
