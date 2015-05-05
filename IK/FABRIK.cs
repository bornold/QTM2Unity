using System;
using System.Linq;
using OpenTK;

namespace QTM2Unity
{
    class FABRIK : IKSolver
    {
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
            int numberOfBones = bones.Length;
            bones[numberOfBones - 1].Orientation = target.Orientation;
            Vector3 root = bones[0].Pos;
            int iterations = 0;
            float lastDistToTarget = float.MaxValue;
            float distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            while (distToTarget > threshold && iterations++ < maxIterations)
            {
                if (distToTarget >= lastDistToTarget)
                {
                    ForwardKinematics(ref bones, QuaternionHelper.RotationZ(MathHelper.PiOver6));
                }
                // Check if target is on the chain
                if (IsTargetOnChain(ref bones, ref target))
                {
                    // Bend chain a small degree
                    Quaternion rot = Quaternion.FromAxisAngle(bones[0].GetXAxis(), MathHelper.DegreesToRadians(1));
                    //bones[0].Rotate(rot);
                    ForwardKinematics(ref bones, rot, 0);
                }

                // Forward reaching
                ForwardReaching(ref bones, ref distances, target);
                // Backward reaching
                BackwardReaching(ref bones, ref distances, root, parent);
                
                lastDistToTarget = distToTarget;
                distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
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
                Vector3 newPos = ((1 - l) * bones[i].Pos) + (l * target);
                Bone prevBone = (i > 0) ? bones[i - 1] : parent;

                if (bones[i].Constraints != Vector4.Zero)
                {
                    Vector3 res;
                    Quaternion rot;
                    newPos = 
                        Constraint.CheckRotationalConstraints(bones[i], prevBone, newPos, out res, out rot) ? 
                        res : newPos;
                }
                bones[i + 1].Pos = newPos;
                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
                // Constraints
                EnsureOrientationalConstraints(ref bones[i], ref prevBone, false);

            }
            return bones;
        }

        private void ForwardReaching(ref Bone[] bones, ref float[] distances, Bone target)
        {
            
            bones[bones.Length - 1].Pos = target.Pos;
            bones[bones.Length - 1].Orientation = target.Orientation; //TODO if bone is endeffector, we should not look at rot constraints
            for (int i = bones.Length - 2; i >= 0; i--)
            {
                if (bones[i+1].Pos == bones[i].Pos)
                {
                    // move one of them a small distance along the chain
                    if (i+2 < bones.Length)
                    {
                        bones[i + 1].Pos = bones[i + 1].Pos +
                            Vector3.Normalize(bones[i + 2].Pos - bones[i + 1].Pos) * 0.001f;
                    }
                    else if (i - 1 >= 0)
                    {
                        bones[i].Pos = bones[i - 1].Pos +
                            Vector3.Normalize(bones[i - 1].Pos - bones[i].Pos) * 0.001f;
                    }
                    // else terminate
                }
                // Position
                Vector3 newPos;
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;
                // bones[i].Pos = (1 - l) * bones[i + 1].Pos + l * bones[i].Pos;

                newPos = (1 - l) * bones[i + 1].Pos + l * bones[i].Pos;
                bones[i].Pos = newPos;

                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
                  
                // Constraints
                EnsureOrientationalConstraints(ref bones[i+1], ref bones[i], true);
                //if ( (i+2 < bones.Length) && (bones[i].Constraints != Vector4.Zero) )
                //{
                //    Vector3 res;
                //    Quaternion rot;

                //    if (Constraint.CheckRotationalConstraints(bones[i + 1], bones[i], bones[i + 2].Pos, out res, out rot))
                //    {
                //        //bones[i].Pos = bones[i + 1].Pos + Vector3.Transform(bones[i].Pos - bones[i + 1].Pos, Quaternion.Invert(rot));
                //    }
                //}
            }
        }

        private void BackwardReaching(ref Bone[] bones, ref float[] distances, Vector3 root, Bone parent)
        {
            bones[0].Pos = root;
            for (int i = 0; i < bones.Length - 1; i++)
            {
                if (bones[i + 1].Pos == bones[i].Pos)
                {
                    // move one of them a small distance along the chain
                    if (i + 2 < bones.Length)
                    {
                        bones[i + 1].Pos = bones[i + 1].Pos +
                            Vector3.Normalize(bones[i + 2].Pos - bones[i + 1].Pos) * 0.001f;
                    }
                    else if (i - 1 >= 0)
                    {
                        bones[i].Pos = bones[i - 1].Pos +
                            Vector3.Normalize(bones[i - 1].Pos - bones[i].Pos) * 0.001f;
                    }
                    // else terminate? TODO
                }

                Vector3 newPos;
                // Position
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;

                newPos = (1 - l) * bones[i].Pos + l * bones[i + 1].Pos;
                
                Bone prevBone = (i > 0) ? bones[i - 1] : parent;
                if (bones[i].Constraints != Vector4.Zero)
                {
                    Vector3 res;
                    Quaternion rot;
                    newPos =
                        Constraint.CheckRotationalConstraints(bones[i], prevBone, newPos, out res, out rot) ?
                        res : newPos;
                }
                bones[i + 1].Pos = newPos;
                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
                // Constraints
                EnsureOrientationalConstraints(ref bones[i], ref prevBone, false);
              
            }
        }

        private bool EnsureOrientationalConstraints(ref Bone target, ref Bone reference, bool forward)
        {
            if (target.StartTwistLimit > -1 && target.EndTwistLimit > -1 && !target.Orientation.Xyz.IsNaN() && !reference.Orientation.Xyz.IsNaN())
            {
                Quaternion rotation = Quaternion.Identity;
                if (Constraint.CheckOrientationalConstraint(target, reference, out rotation))
                {
                    if (forward)
                    {
                        reference.Rotate(rotation);
                    }
                    else
                    {
                        target.Rotate(rotation);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
