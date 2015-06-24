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
                return TargetUnreachable(bones, target.Pos, parent);
            }

            // The target is reachable
            int numberOfBones = bones.Length;
            bones[numberOfBones - 1].Orientation = target.Orientation;
            Vector3 root = bones[0].Pos;
            int iterations = 0;
            //float test = 0.1f;
            //bool toggle = false;
            //int samePosIterations = 0;
            //int pushes = 0;
            //float lastDistToTarget = float.MaxValue;
            float distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            while (distToTarget > threshold && iterations++ < maxIterations)
            {
                //bool move = false;
                //if (distToTarget >= lastDistToTarget)
                //{
                //    if (samePosIterations > 3)
                //    {
                //        if (pushes++ > 5) break;
                //        move = true;
                //    }
                //    else
                //    {
                //        samePosIterations++;
                //    }
                //}
                //else samePosIterations = 0;


                //// Check if target is on the chain
                //if (IsTargetOnChain(ref bones, ref target))
                //{
                //    // Bend chain a small degree
                //    Quaternion rot = Quaternion.FromAxisAngle(bones[0].GetXAxis(), MathHelper.DegreesToRadians(1));
                //    //bones[0].Rotate(rot);
                //    ForwardKinematics(ref bones, rot, 0);
                //}

                // Forward reaching
                ForwardReaching(ref bones, ref distances, target);
                //if (move)
                //{
                //    Quaternion q;
                //    float rad = MathHelper.DegreesToRadians(test);//* (toggle ? -1 : 1));//MathHelper.PiOver6 * test * (toggle ? -1 : 1);
                //    if (rad > 360) //MathHelper.TwoPi)
                //    {
                //        break;
                //    }
                //    q = QuaternionHelper.RotationX(rad); // bra/ok på ben, kass på armar
                //    //q = QuaternionHelper.RotationY(rad); // kass överallt
                //    //q = QuaternionHelper.RotationZ(rad); // ok på ben, kass på armar
                //    //q = Quaternion.FromAxisAngle(bones[0].GetXAxis(), rad); // ok på ben, kass på armar
                //    //q = Quaternion.FromAxisAngle(bones[0].GetYAxis(), rad); // ok på ben, kass på armar
                //    //q = Quaternion.FromAxisAngle(bones[0].GetZAxis(), rad); // ok/kass på ben, kass på armar
                //    //q = QuaternionHelper.RotationBetween(bones[0].GetYAxis(), target.Pos - bones[0].Pos); // ok på ben, kass på armar
                //    //Vector3 tre = Vector3.Cross(bones[0].GetYAxis(), target.Pos - bones[0].Pos);
                //    //q = Quaternion.FromAxisAngle(tre, rad);
                //    //q = Quaternion.Invert( bones[0].Orientation);
                //    ForwardKinematics(ref bones, q, i: (toggle ? 1 : 0));

                //    //bones[0].Rotate(q);
                //    //bones[1].Rotate(q);
                //    //UnityEngine.Debug.Log(rad);
                //    if (toggle) test += 10f;
                //    toggle = !toggle;
                //}
                // Backward reaching
                BackwardReaching(ref bones, ref distances, root, parent);

                //lastDistToTarget = distToTarget;
                distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            }
            bones[bones.Length - 1].Orientation = target.Orientation;

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
                //EnsureOrientationalConstraints(ref bones[i + 1], ref bones[i], true);
                //if (bones[i].Constraints != Vector4.Zero)
                //{
                //    Vector3 res;
                //    Quaternion rot;

                //    if (Constraint.CheckRotationalConstraints(bones[i + 1], bones[i], bones[i + 2].Pos, out res, out rot))
                //    {
                //        bones[i].Pos = bones[i + 1].Pos + Vector3.Transform(bones[i].Pos - bones[i + 1].Pos, Quaternion.Invert(rot));
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
                
                //Bone prevBone = (i > 0) ? bones[i - 1] : parent;
                //if (bones[i].Constraints != Vector4.Zero)
                //{
                //    Vector3 res;
                //    Quaternion rot;
                //    newPos =
                //        Constraint.CheckRotationalConstraints(bones[i], prevBone, newPos, out res, out rot) ?
                //        res : newPos;
                //}
                bones[i + 1].Pos = newPos;
                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos,bones[i].Stiffness);
                // Constraints
                //EnsureOrientationalConstraints(ref bones[i], ref prevBone, false);
              
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
