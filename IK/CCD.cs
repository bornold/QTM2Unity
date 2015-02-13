using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using QTM2Unity.Unity;
using Debug = UnityEngine.Debug;

namespace QTM2Unity
{
    class CCD
    {
       
        private static float threshold = 0.0001f; // TODO define a good default threshold value 
                                // This depends on where the position is defined on the end effector
        private static int numberOfIterations = 50; // TODO what's a good value?

        // And what happens when we don't have a position for some joint?
        // Probably don't want to handle here

        // Note: The end effector is assumed to be the last element in bones
        public static Bone[] solveBoneChain(Bone[] bones, Vector3 target)
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
                    }
                    
                    // check constraints starting with first affected joint and moving towards end effector
                    for (int j = i; j < bones.Length; j++)
                    {
                        if (j > 0) // TODO not checking root right now, should we?
                            checkOrientationalConstraint(ref bones[j], bones[j - 1]);
                    }
                    
                    // Check if we are close enough to target
                    if ((target - bones[numberOfBones-1].Pos).Length <= threshold)
                    {
                        return bones;
                    }
                }
                iter++;
            }
                return bones;
        }

        // TODO should be private, public for test purposes
        // TODO maybe this is better in the OrientationalContraint class
        public static void checkOrientationalConstraint(ref Bone b, Bone parent)
        {
            if (b.OrientationalConstraint != null) // if there exist a constraint
            {
                Vector3 direction = b.getDirection();
                float twistAngle = getTwistAngle(b, parent);

                float from = b.OrientationalConstraint.From;
                float to = b.OrientationalConstraint.To;

                if (!(twistAngle >= from && twistAngle <= to)) // not inside constraints
                {
                    // rotate the bone around its direction vector to be inside
                    // the constraints (rotate it from its current angle to from or to)
                    // TODO rotating the right directio? (-/+)
                    if (twistAngle < from) // TODO add some precision (so it doesn't need to rotate eg 0,000324)
                    {
                        // rotate clockwise
                        Debug.Log("Twistangle is " + twistAngle + ". Rotating " + b.Name + 
                            " " + (twistAngle - from) + " clockwise around itself.");
                        b.rotate(Math.Abs(MathHelper.DegreesToRadians(twistAngle - from)), direction);
                    }
                    else if (twistAngle > to)
                    {
                        // rotate anticlockwise
                        Debug.Log("Twistangle is " + twistAngle + ". Rotating " + b.Name +
                            " " + (twistAngle - to) + " anticlockwise around itself.");
                        b.rotate(-Math.Abs(MathHelper.DegreesToRadians(twistAngle - to)), direction);
                    }
                }
            }
        }

        // Calculates the angle b is twisted around its direction vector in radians
        // TODO make private. Only public for testing purposes.
        public static float getTwistAngle(Bone b, Bone parent)
        {
            Vector3 direction = b.getDirection();
            Vector3 up = b.getUp();
            Vector3 right = b.getRight();

            // construct a reference vector which the twist/orientation will depend on
            // The reference is the parents up vector projected on the same plane as the 
            // current bone's up vector
            Vector3 reference = Vector3Helper.ProjectOnPlane(parent.getUp(), direction);

            float twistAngle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(reference, up));

            if (Vector3.CalculateAngle(reference, right) > Mathf.PI/2) // b is twisted left with respect to parent
                return -twistAngle;

            return twistAngle;
        }

        private static void checkRotationalConstraint(Bone b, Bone parent, Vector3 target)
        {
            if (b.RotationalConstraint != null) // there exist a constraint
            {
                // find the line passing through the joint under consideration and its parent
                Vector3 L1 = b.Pos - parent.Pos;
                L1.Normalize();
                L1 = L1 * (target - b.Pos).Length; // makes sure L1 has sufficient length
                // find the projection O of the target on line L1
                Vector3 O = Vector3Helper.ProjectAndCreate(target, L1);
                // find the distance S between O and the joint position
                float S = (O - b.Pos).Length;
                // Map the target in such a way that O is located at the origin
                // and the axes defining the constraints are aligned with x,y
                target = mapToOrigin(target, L1, parent.getRight(), O);

                // Angles defining ellipse radii
                float ax, ay;

                // Locate target in a particular quadrant
                if (target.X >= 0 && target.Y >= 0)
                {
                    // x, y quadrant
                    // Ellipse defined by q1, q2
                    ax = b.RotationalConstraint.getAngle(1);
                    ay = b.RotationalConstraint.getAngle(2);
                }
                else if (target.X < 0 && target.Y >= 0)
                {
                    // -x, y quadrant
                    // Ellipse defined by q2, q3
                    ay = b.RotationalConstraint.getAngle(2);
                    ax = b.RotationalConstraint.getAngle(3);
                }
                else if (target.X <= 0 && target.Y < 0)
                {
                    // -x,-y quadrant
                    // Ellipse defined by q3, q0
                    ax = b.RotationalConstraint.getAngle(3);
                    ay = b.RotationalConstraint.getAngle(0);
                }
                else /*if (target.X > 0 && target.Y < 0)*/
                {
                    // x, -y quadrant
                    // Ellipse defined by q0, q1
                    ay = b.RotationalConstraint.getAngle(0);
                    ax = b.RotationalConstraint.getAngle(1);
                }

                // Calculate ellipse radius for x and y
                float radiusX = S * Mathf.Tan(ax);
                float radiusY = S * Mathf.Tan(ay);

                if (!(Math.Abs(target.X) <= radiusX && Math.Abs(target.Y) <= radiusY)) // target not inside
                {
                    // Find nearest point from target on ellipse defined by ex and ey
                }

                // Undo the origin mapping
            }
        }

        private static Vector3 mapToOrigin(Vector3 target, Vector3 L1, Vector3 right, Vector3 origin)
        {
            // Translation:
            target = target.translate(origin);
            L1 = L1.translate(origin);
            right = right.translate(origin);
            // Rotation:
            Quaternion rot1 = QuaternionHelper.getRotation(L1, Vector3.UnitZ);
            right = Vector3.Transform(right, rot1);
            Quaternion rot2 = QuaternionHelper.getRotation(right, Vector3.UnitX);
            
            return Vector3.Transform(target, rot2 * rot1);
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
