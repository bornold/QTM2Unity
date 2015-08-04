using OpenTK;
namespace QTM2Unity
{
    class CCD : IKSolver
    {
        private int degreeStep = 10;
        override public bool SolveBoneChain(Bone[] bones, Bone target, Bone grandparent)
        {

            if (!IsReachable(bones,target))
            {
                TargetUnreachable(bones, target.Pos, grandparent);
                bones[bones.Length - 1].Orientation = new Quaternion(target.Orientation.Xyz, target.Orientation.W);
                return true;
            }

            int numberOfBones = bones.Length;
            int iter = 0;
            int degrees = degreeStep;
            bool toggle = false;
            bool doneOneLapAroundYAxis = false;
            int maxdegrees = 120;
            float lastDistanceToTarget = float.MaxValue;
            float distanceToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;

            // main loop
            while (threshold < distanceToTarget
                && MaxIterations < ++iter  
                && (!doneOneLapAroundYAxis || degrees < maxdegrees))
            {
                // if CCD is stuck becouse of constraints, we twist the chain
                if ((distanceToTarget >= lastDistanceToTarget))
                {
                    if (!doneOneLapAroundYAxis && degrees > maxdegrees)
                    {
                        doneOneLapAroundYAxis = true;
                        degrees = degreeStep;
                    }
                    else if (degrees > maxdegrees)
                    {
                        break;
                    }
                    Quaternion q = doneOneLapAroundYAxis ?
                        QuaternionHelper2.RotationX(MathHelper.DegreesToRadians(toggle ? degrees : -degrees))
                      :
                       QuaternionHelper2.RotationY(MathHelper.DegreesToRadians(toggle ? degrees : -degrees));
                    ForwardKinematics(ref bones, q);
                    if (toggle)
                    {
                        degrees += degreeStep;
                    }
                    toggle = !toggle;
                }

                // for each bone, starting with the one closest to the end effector 
                // (but not the end effector itself)
                Vector3 a, b;
                Quaternion rotation;
                for (int i = numberOfBones - 2; i >= 0; i--)
                {
                    // Get the vectors between the points
                    a = bones[numberOfBones - 1].Pos - bones[i].Pos;
                    b = target.Pos - bones[i].Pos;
                    // Make a rotation quaternion and rotate 
                    // - first the endEffector
                    // - then the rest of the affected joints
                    rotation = (a.LengthFast == 0 || b.LengthFast == 0) ? Quaternion.Identity
                        : QuaternionHelper2.GetRotationBetween(a, b, bones[i].Stiffness);

                    if (bones[i].HasConstraints)
                    {
                        //Vector3 trg = bones[i].Pos + Vector3.Transform(bones[i + 1].Pos - bones[i].Pos, rotation);
                        Vector3 res;
                        Quaternion rot;
                        if (constraints.CheckRotationalConstraints(
                            bones[i],
                            ((i > 0) ? bones[i - 1] : grandparent).Orientation, //Reference
                            bones[i].Pos + Vector3.Transform(bones[i + 1].Pos - bones[i].Pos, rotation), // Target
                            out res, out rot))
                        {
                            rotation = rot * rotation;
                        }
                    }

                    ForwardKinematics(ref bones, rotation, i);

                    if (bones[i].HasTwistConstraints)
                    {
                        Quaternion rotation2;
                        if (constraints.CheckOrientationalConstraint(bones[i], (i > 0) ? bones[i - 1] : grandparent, out rotation2))
                        {
                            //ForwardRotation(ref bones, rotation2, i);
                            ForwardKinematics(ref bones, rotation2, i);
                        }
                    }
                }
                lastDistanceToTarget = distanceToTarget;
                distanceToTarget = (bones[bones.Length - 1].Pos - target.Pos).LengthFast;
            }
            bones[bones.Length - 1].Orientation = new Quaternion (target.Orientation.Xyz,target.Orientation.W);
            return (distanceToTarget <= threshold);
        }
    }
}
