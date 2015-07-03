using OpenTK;
namespace QTM2Unity
{
    class CCD : IKSolver
    {
        // Note: The end effector is assumed to be the last element in bones
        override public bool SolveBoneChain(Bone[] bones, Bone target, Bone grandparent)
        {
            
            if (!IsReachable(bones,target))
            {
                TargetUnreachable(bones, target.Pos);
                return true;
            }

            int numberOfBones = bones.Length;
            int iter = 0;
            int degrees = 5;
            int loopsWithSameDist = 0;
            bool toggle = false;
            bool doneOneLapAroundYAxis = false;
            int maxdegrees = 120;
            float lastDistToTarget = float.MaxValue;
            float distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            while (distToTarget > threshold && ++iter < maxIterations && (!doneOneLapAroundYAxis || degrees < maxdegrees))
            {
                //loopsWithSameDist = (distToTarget >= lastDistToTarget) ? loopsWithSameDist + 1 : 0;
                //if (loopsWithSameDist > 2)
                    if ((distToTarget >= lastDistToTarget))
                {
                    if (!doneOneLapAroundYAxis && degrees > maxdegrees)
                    {
                        doneOneLapAroundYAxis = true;
                        degrees = 2;
                    }   
                    Quaternion q = doneOneLapAroundYAxis ?
                        QuaternionHelper.RotationX(MathHelper.DegreesToRadians(toggle ? degrees : -degrees))
                      :
                       QuaternionHelper.RotationY(MathHelper.DegreesToRadians(toggle ? degrees : -degrees));
                    ForwardKinematics(ref bones, q);
                    if (toggle)
                    {
                        degrees = degrees + 2;
                    }
                    toggle = !toggle;
                }

                //// Check if target is on the chain
                //if (IsTargetOnChain(ref bones, ref target))
                //{
                //    // Bend chain a small degree
                //    Quaternion rot = Quaternion.FromAxisAngle(bones[0].GetXAxis(), MathHelper.DegreesToRadians(1));
                //    ForwardKinematics(ref bones, rot, 0);
                //}

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
                        : QuaternionHelper.GetRotationBetween(a, b, bones[i].Stiffness);

                    if (bones[i].HasConstraints)
                    {

                        //Vector3 trg = bones[i].Pos + Vector3.Transform(bones[i + 1].Pos - bones[i].Pos, rotation);
                        Vector3 res;
                        Quaternion rot;
                        if (Constraint.CheckRotationalConstraints(
                            bones[i],
                            ((i > 0) ? bones[i - 1] : grandparent).Orientation, //Reference
                            bones[i].Pos + Vector3.Transform(bones[i + 1].Pos - bones[i].Pos, rotation), // Target
                            out res, out rot))
                        {
                            //a = bones[i + 1].Pos - bones[i].Pos;
                            //b = res - bones[i].Pos;
                            //rotation = QuaternionHelper.GetRotationBetween(a, b);
                            rotation = rot * rotation;
                        }
                    }

                    ForwardKinematics(ref bones, rotation, i);

                    if ( bones[i].HasTwistConstraints)
                    {
                        Quaternion rotation2;
                        if (Constraint.CheckOrientationalConstraint(bones[i], (i > 0) ? bones[i - 1] : grandparent, out rotation2))
                        {
                            ForwardKinematics(ref bones, rotation2, i);
                        }
                    }
                }
                lastDistToTarget = distToTarget;
                distToTarget = (bones[bones.Length - 1].Pos - target.Pos).LengthFast;
            }
            bones[bones.Length - 1].Orientation = new Quaternion (target.Orientation.Xyz,target.Orientation.W);
            return (distToTarget <= threshold);
        }
    }
}
