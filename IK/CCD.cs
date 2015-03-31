using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using QTM2Unity.Unity;
using Debug = UnityEngine.Debug;

namespace QTM2Unity
{
    class CCD : IKSolver
    {

        private static float threshold = 0.0001f; // TODO define a good default threshold value 
        // This depends on where the position is defined on the end effector
        private static int numberOfIterations = 500; // TODO what's a good value?

        // And what happens when we don't have a position for some joint?
        // Probably don't want to handle here

        // Note: The end effector is assumed to be the last element in bones
        override public Bone[] SolveBoneChain(Bone[] bones, Bone target, Bone parent)
        {
            int numberOfBones = bones.Length;
            int iter = 0;
            while ((target.Pos - (bones[numberOfBones - 1].Pos)).Length > threshold
                && iter < numberOfIterations)
            {
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

                    rotation = (a.Length == 0 || b.Length == 0) ? rotation = Quaternion.Identity : rotation = QuaternionHelper.getRotation(a, b);
                    if (bones[i].Constraints != Vector4.Zero)
                    {

                        Vector3 trg = bones[i].Pos + Vector3.Transform(bones[i + 1].Pos - bones[i].Pos, rotation);
                        Vector3 dir = (i > 0) ? bones[i].Pos - bones[i - 1].Pos : parent.GetDirection();
                        Vector3 res;
                        Vector3 cpo = new Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z);
                        if (Constraint.CheckRotationalConstraints(bones[i], trg, dir, out res))
                        {
                            Vector3 cpo2 = new Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z);
                            a = bones[i + 1].Pos - bones[i].Pos;
                            b = res - bones[i].Pos;
                            rotation = QuaternionHelper.getRotation(a, b);
                        }
                    }
                    ForwardKinematics(ref bones, rotation, i);
                    if (bones[i].LeftTwist > 0 && bones[i].RightTwist > 0)
                    {
                        Quaternion rotation2 = Quaternion.Identity;
                        if (Constraint.CheckOrientationalConstraint(bones[i], (i > 0) ? bones[i - 1] : parent, out rotation2))
                        {
                            //bones[i].Rotate(rotation2);
                            ForwardKinematics(ref bones, rotation2, i);
                        }
                    }
                }
                iter++;
            }
            return bones;
        }


        public float getTwistAngle(Bone b, Bone parent)
        {
            Vector3 direction = b.GetDirection();
            Vector3 up = b.GetUp();
            Vector3 right = b.GetRight();

            // construct a reference vector which the twist/orientation will depend on
            // The reference is the parents up vector projected on the same plane as the 
            // current bone's up vector
            Vector3 reference = Vector3Helper.ProjectOnPlane(parent.GetUp(), direction);

            float twistAngle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(reference, up));

            if (Vector3.CalculateAngle(reference, right) > Mathf.PI / 2) // b is twisted left with respect to parent
                return -twistAngle;

            return twistAngle;
        }

        // TODO should not be public. Or should probably not be in this class even..
#if false
        public void checkRotationalConstraint(ref Bone[] bs, int current)
        {
            if (current == 0)
                return; // Don't check constraint for root
            RotationalConstraint rc = bs[current - 1].RotationalConstraint;
            if (rc != null) // there exist a constraint on parent
            {
                Vector3 pos;
                calculatePosition(out pos, ref bs[current], ref bs[current - 1]);

                // Rotate b to the nearest point 
                // Rotate b's children with the same rotation
                if (pos != bs[current].Pos) // the position has moved
                {
                    Debug.Log("Position before: " +
                        bs[current].Pos.X + ", " + bs[current].Pos.Y + ", " + bs[current].Pos.Z);
                    // Rotate b towards pos and follow with all its children
                    Vector3 parent = bs[current].Pos - bs[current - 1].Pos;
                    Vector3 newParent = pos - bs[current - 1].Pos;
                    Vector3 axis = 
                        Vector3.Cross(parent, newParent);
                    axis.Normalize();
                    float angle = Vector3.CalculateAngle(parent, newParent);
                    Quaternion rot = Quaternion.FromAxisAngle(axis, angle);

                    Debug.Log("Rotate " + bs[current].Name + " " + angle + " around "
                        + axis.X + ", " + axis.Y + ", " + axis.Z);

                    for (int i = current-1; i < bs.Length; i++)
                    {
                        bs[i].rotate(rot);
                    }
                    Debug.Log("Position after: " +
                        bs[current].Pos.X + ", " + bs[current].Pos.Y + ", " + bs[current].Pos.Z);
                }
            }
        }

        // denna kan kanske bli finare
        private void calculatePosition(out Vector3 pos, ref Bone b, ref Bone parent)
        {
            RotationalConstraint rc = parent.RotationalConstraint;
            pos = b.Pos;
            // Find parents constraint cone
            Vector3 L1 = rc.getDirection();
            L1.Normalize();
            L1 = L1 * (pos - parent.Pos).Length; // makes sure L1 has sufficient length
            // find the projection O of the target on line L1
            Vector3 O = Vector3Helper.ProjectAndCreate(pos-parent.Pos, L1); // must project the right vector!!
            // find the distance S between O and the joint position
            float S = (O - pos).Length;
            // Map the target in such a way that O is located at the origin
            // and the axes defining the constraints are aligned with x,y
            Quaternion mappingQuat = mapToOrigin(ref pos, L1, rc.getRight(), O);

            // Angles defining ellipse radii
            float ax, ay;
            findAngles(ref rc, pos, out ax, out ay);

            // Calculate ellipse radius for x and y
            float radiusX = S * Mathf.Tan(ax);
            float radiusY = S * Mathf.Tan(ay);

            // If b's pos is outside the parents constraint
            if (!(Math.Abs(pos.X) <= radiusX && Math.Abs(pos.Y) <= radiusY)) // target not inside
            {
                Debug.Log(b.Name + " not inside constraint");
                // Find nearest point from b on ellipse defined by radiusX and radiusY
                Vector2 newPoint = Mathf.FindNearestPointOnEllipse
                    (Math.Max(radiusX, radiusY), Math.Min(radiusX, radiusY),
                    new Vector2(Math.Abs(pos.X), Math.Abs(pos.Y)));

                // Move target to nearest point on the ellipse
                moveTarget(ref pos, newPoint);

            } 
            else
                Debug.Log(b.Name + " is inside constraint");

            // Undo the origin mapping
            // Rotation
            pos = Vector3.Transform(pos, Quaternion.Invert(mappingQuat));
            // Translation
            pos = O + pos;
        }


        private Quaternion mapToOrigin(ref Vector3 target, Vector3 L1, Vector3 right, Vector3 origin)
        {
            // Translation:
            target = target.translate(origin);
            L1 = L1.translate(origin);
            right = right.translate(origin);
            // Rotation:
            Quaternion rot1 = QuaternionHelper.getRotation(L1, Vector3.UnitZ);
            right = Vector3.Transform(right, rot1);
            Quaternion rot2 = QuaternionHelper.getRotation(right, Vector3.UnitX);
            Quaternion rotation = rot2 * rot1;

            target = Vector3.Transform(target, rotation);
            return rotation;
        }

        private void findAngles(ref RotationalConstraint rc, Vector3 target, out float ax, out float ay)
        {
            // Locate target in a particular quadrant
            if (target.X >= 0 && target.Y >= 0)
            {
                // x, y quadrant
                // Ellipse defined by q1, q2
                ax = rc.getAngle(1);
                ay = rc.getAngle(2);
            }
            else if (target.X < 0 && target.Y >= 0)
            {
                // -x, y quadrant
                // Ellipse defined by q2, q3
                ay = rc.getAngle(2);
                ax = rc.getAngle(3);
            }
            else if (target.X <= 0 && target.Y < 0)
            {
                // -x,-y quadrant
                // Ellipse defined by q3, q0
                ax = rc.getAngle(3);
                ay = rc.getAngle(0);
            }
            else /*if (target.X > 0 && target.Y < 0)*/
            {
                // x, -y quadrant
                // Ellipse defined by q0, q1
                ay = rc.getAngle(0);
                ax = rc.getAngle(1);
            }
        }

        private void moveTarget(ref Vector3 target, Vector2 newTarget)
        {
            float x = newTarget.X;
            float y = newTarget.Y;
            if (target.X < 0)
                x = -x;
            if (target.Y < 0)
                y = -y;

            target.X = x;
            target.Y = y;
        }
#endif
    }
}
