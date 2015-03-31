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

        override public Bone[] SolveBoneChain(Bone[] bones, Bone target, Bone parent)
        {
            // Calculate distances 
            float[] distances;
            GetDistances(out distances, ref bones);

            double dist = Math.Abs((bones[0].Pos - target.Pos).Length);
            if (dist > distances.Sum()) // the target is unreachable
            {
                return TargetUnreachable(ref distances, bones, target.Pos, parent);
            }

            // The target is reachable
            Vector3 root = bones[0].Pos;
            int iterations = 0;
            bones[bones.Length - 1].Orientation = target.Orientation;
            while ((bones[bones.Length - 1].Pos - target.Pos).Length > threshold && iterations++ < 500)
            {
                // Forward reaching
                ForwardReaching(ref bones, ref distances, target);

                // Backward reaching
                BackwardReaching(ref bones, ref distances, root, parent);
            }

            return bones;
        }

        private Bone[] TargetUnreachable(ref float[] distances, Bone[] bones, Vector3 target, Bone parent)
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
                Bone prevBone = (i > 0) ? bones[i - 1] : parent;
                EnsureRotationalConstraints(ref bones[i], ref prevBone);
                Vector3 dir = (i > 0) ? bones[i].Pos - bones[i - 1].Pos : parent.GetDirection();
                EnsureOrientationalConstraints(ref bones[i + 1], ref bones[i], dir);
            }
            return bones;
        }

        private void ForwardReaching(ref Bone[] bones, ref float[] distances, Bone target)
        {
            bones[bones.Length - 1].Pos = target.Pos;
            bones[bones.Length - 1].Orientation = target.Orientation; //TODO if bone is endeffector, we should not look at orient constraints 
            for (int i = bones.Length - 2; i >= 0; i--)
            {
                // Position
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;
                bones[i].Pos = (1 - l) * bones[i + 1].Pos + l * bones[i].Pos;

                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);

                // Constraints
                EnsureRotationalConstraints(ref bones[i], ref bones[i + 1]);
                EnsureOrientationalConstraints(ref bones[i], ref bones[i + 1], -bones[i + 1].GetDirection());

            }
        }

        private void BackwardReaching(ref Bone[] bones, ref float[] distances, Vector3 root, Bone parent)
        {
            bones[0].Pos = root;
            for (int i = 0; i < bones.Length - 1; i++)
            {
                // Position
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;
                bones[i + 1].Pos = (1 - l) * bones[i].Pos + l * bones[i + 1].Pos;

                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);

                // Constraints
                Bone prevBone = (i > 0) ? bones[i - 1] : parent;
                EnsureRotationalConstraints(ref bones[i], ref prevBone);
                Vector3 dir = (i > 0) ? bones[i].Pos - bones[i - 1].Pos : parent.GetDirection();
                EnsureOrientationalConstraints(ref bones[i + 1], ref bones[i], dir);
            }
        }
        private bool EnsureOrientationalConstraints(ref Bone target, ref Bone reference, Vector3 L1)
        {
            if (reference.Constraints != Vector4.Zero)
            {
                Vector3 res;
                if (Constraint.CheckRotationalConstraints(reference, target.Pos, L1, out res))
                {
                    target.Pos = res;
                    reference.RotateTowards(target.Pos - reference.Pos);
                    return true;
                }
            }
            return false;
        }
        private bool EnsureRotationalConstraints(ref Bone target, ref Bone reference)
        {
            if (target.LeftTwist > 0 && target.RightTwist > 0)
            {
                Quaternion rotation = Quaternion.Identity;
                if (Constraint.CheckOrientationalConstraint(target, reference, out rotation))
                {
                    target.Rotate(rotation);
                    return true;
                }
            }
            return false;
        }
    }
}
