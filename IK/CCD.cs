using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using QTM2Unity.Unity;
using QTM2Unity.SkeletonModel;

namespace QTM2Unity.IK
{
    class CCD
    {
       
        private static float threshold = 0.1f; // TODO define a good default threshold value 
                                // This depends on where the position is defined on the end effector
        private static int numberOfIterations = 50; // TODO what's a good value?

        // And what happens when we don't have a position for some joint?
        // Probably don't want to handle here

        // Note: The end effector is assumed to be the last element in bones
        // TODO maximum number of iterations?
        // TODO now it just changes the bones' pos, need to change orientation??
        public static SkeletonModel.Bone[] solveBoneChain(SkeletonModel.Bone[] bones, Vector3 target)
        {
            int numberOfBones = bones.Length;
            int iter = 0;
            while ((target - (bones[numberOfBones-1].Pos)).Length > threshold
                && iter < numberOfIterations)
            {
                // for each bone, starting with the one closest to the end effector 
                // (but not the end effector itself)
                for (int i = numberOfBones - 2; i >= 0; i--)
                {
                    // Get the vectors between the points
                    Vector3 a = bones[numberOfBones-1].Pos - bones[i].Pos;
                    Vector3 b = target - bones[i].Pos;

                    // Make a rotation quaternion and rotate 
                    // - first the endEffector
                    // - then the rest of the affected joints
                    Quaternion rotation = QuaternionHelper.getRotation(a, b);
                    for (int j = numberOfBones - 1; j >= i; j--)
                    {
                        if (j > i)
                        {
                            bones[j].Pos = bones[i].Pos + 
                                OpenTK.Vector3.Transform((bones[j].Pos - bones[i].Pos), rotation);
                        }
                        
                        // rotate orientation
                        bones[j].rotate(rotation);

                        // Check constraints
                       // if (j > 0) // TODO not checking root right now, should we?
                         //   checkOrientationalConstraint(ref bones[j], bones[j-1]);
                    }
                    // I think we need to do this check here <-- TODO check that I'm right
                    if ((target - bones[numberOfBones-1].Pos).Length <= threshold)
                    {
                        return bones;
                    }
                }
                iter++;
            }
                return bones;
        }

        private static void checkOrientationalConstraint(ref SkeletonModel.Bone b, SkeletonModel.Bone parent)
        {
            if (b.OrientationalConstraint != null) // if there exist a constraint
            {
                Vector3 direction = b.getDirection();
                float twistAngle = getTwistAngle(b, parent);
                
                float angle1 = QuaternionHelper.degreesToRadians(b.OrientationalConstraint.Angle1);
                float angle2 = QuaternionHelper.degreesToRadians(b.OrientationalConstraint.Angle2);

                if (twistAngle < angle1)
                {
                    // rotate the bone around its direction vector to be inside
                    // the constraints (rotate it from its current angle to Angle1
                    // rotate b's orientation angleAroundDirection - Angle1 around direction

                    b.rotate(twistAngle - angle1, direction);
                }
                else if (twistAngle > angle2)
                {
                    b.rotate(twistAngle - angle2, direction);
                }
            }
        }

        // Calculates the angle b is twisted around its direction vector in radians
        // TODO make private. Only public for testing purposes.
        public static float getTwistAngle(SkeletonModel.Bone b, SkeletonModel.Bone parent)
        {
            Vector3 direction = b.getDirection();
            Vector3 up = b.getUp();
            Vector3 right = b.getRight();

            // construct a reference vector which the twist/orientation will depend on
            // The reference is the parents up vector rotated to match b's rotation
            float angle = Mathf.PI - Vector3.CalculateAngle(parent.getDirection(), direction);
            Vector3 reference;
            if (angle > 0)
            {
                Vector3 axis = Vector3.Cross(parent.getDirection(), direction);
                axis.Normalize();

                reference = Vector3.Transform(parent.getUp(), Quaternion.FromAxisAngle(axis, angle));
            }
            else // parent and b have the same rotation
            {
                reference = parent.getUp();
            }

            float twistAngle = Vector3.CalculateAngle(reference, up);

            if (Vector3.CalculateAngle(reference, right) < 90) // b is twisted left with respect to parent
                return -twistAngle;

            return twistAngle;
        }

        // TODO: remove this when it is not useful anymore, but keep it for now
        private Vector3[] solveChain(Vector3[] jointPositions, Vector3 target)
        {
            int numberOfJoints = jointPositions.Length - 1;
            while ((target - jointPositions[jointPositions.Length - 1]).Length > threshold)
            {
                for (int i = numberOfJoints - 1; i >= 0; i--)
                // for each joint, starting with the one closest to the end effector
                {
                    // Get the vectors between the points
                    Vector3 a = jointPositions[jointPositions.Length - 1] - jointPositions[i];
                    Vector3 b = target - jointPositions[i];
                    
                    // Make a rotation quarternion and rotate 
                    // - first the endEffector
                    // - then the rest of the affected joints
                    Quaternion rotation = QuaternionHelper.getRotation(a, b); 
                    for (int j = numberOfJoints; j >= i; j--)
                    {
                        jointPositions[j] = Vector3.Transform(jointPositions[j], rotation);
                    }
                    // I think we need to do this check here <-- TODO check som I'm right
                    if ((target - jointPositions[jointPositions.Length - 1]).Length > threshold)
                    {
                        return jointPositions;
                    }
                }
            }
                return jointPositions; 
        }

    }
}
