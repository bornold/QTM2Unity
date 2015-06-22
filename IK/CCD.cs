using OpenTK;

namespace QTM2Unity
{
    class CCD : IKSolver
    {
        // Note: The end effector is assumed to be the last element in bones
        override public Bone[] SolveBoneChain(Bone[] bones, Bone target, Bone parent)
        {

            int numberOfBones = bones.Length;
            int iter = 0;
            int degrees = 1;
            int toggle = 0;
            float lastDistToTarget = float.MaxValue;
            float distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            while (distToTarget > threshold && iter++ < maxIterations && degrees < 362)
            {
                if (distToTarget >= lastDistToTarget)
                {
                    Quaternion q = (toggle > 1) ?
                         QuaternionHelper.RotationX((toggle > 2 ? -1 : 1) * MathHelper.DegreesToRadians(degrees))
                        :
                         QuaternionHelper.RotationY((toggle > 0 ? -1 : 1) * MathHelper.DegreesToRadians(degrees));
                    ForwardKinematics(ref bones, q);
                    if (++toggle > 3)
                    {
                        toggle = 0;
                        degrees++;
//                        UnityEngine.Debug.Log(degrees);
                    }
                }

                // Check if target is on the chain
                if (IsTargetOnChain(ref bones, ref target))
                {
                    // Bend chain a small degree
                    Quaternion rot = Quaternion.FromAxisAngle(bones[0].GetXAxis(), MathHelper.DegreesToRadians(1));
                    ForwardKinematics(ref bones, rot, 0);
                }

                // for each bone, starting with the one closest to the end effector 
                // (but not the end effector itself)
                for (int i = numberOfBones - 2; i >= 0; i--)
                {
                    // Get the vectors between the points
                    Vector3 a = bones[numberOfBones - 1].Pos - bones[i].Pos;
                    Vector3 b = target.Pos - bones[i].Pos;
                    Quaternion rotation;
                    // Make a rotation quaternion and rotate 
                    // - first the endEffector
                    // - then the rest of the affected joints
                    rotation = (a.LengthFast == 0 || b.LengthFast == 0) ? Quaternion.Identity
                        : QuaternionHelper.GetRotationBetween(a, b);

                    if (bones[i].HasConstraints)
                    {

                        Vector3 trg = bones[i].Pos + Vector3.Transform(bones[i + 1].Pos - bones[i].Pos, rotation);
                        Bone reference = (i > 0) ? bones[i - 1] : parent;
                        Vector3 res;
                        Quaternion rot;

                        if (Constraint.CheckRotationalConstraints(bones[i], reference, trg, out res, out rot))
                        {
                            a = bones[i + 1].Pos - bones[i].Pos;
                            b = res - bones[i].Pos;
                            rotation = QuaternionHelper.GetRotationBetween(a, b);
                        }
                    }

                    ForwardKinematics(ref bones, rotation, i);

                    if (bones[i].HasTwistConstraints)
                    {
                        Quaternion rotation2;
                        if (Constraint.CheckOrientationalConstraint(bones[i], (i > 0) ? bones[i - 1] : parent, out rotation2))
                        {
                            ForwardKinematics(ref bones, rotation2, i);
                        }
                    }
                }
                lastDistToTarget = distToTarget;
                distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            }
//            UnityEngine.Debug.Log(degrees);
            bones[bones.Length - 1].Orientation = target.Orientation;
            return bones;
        }
    }
}
