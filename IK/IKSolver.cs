using OpenTK;
using System;
namespace QTM2Unity
{
    abstract class IKSolver
    {
        abstract public bool SolveBoneChain(Bone[] bones, Bone target, Bone parent);
        protected float threshold = 0.01f;
        protected int maxIterations = 100;
        public int MaxIterations 
        {
            get { return maxIterations; }
            set { maxIterations = value;}
        }
        
        protected bool IsReachable(Bone[] bones, Bone target)
        {
            float acc = 0;
            for (int i = 0; i < bones.Length - 1; i++)
            {
                acc += (bones[i].Pos - bones[i + 1].Pos).Length;
            }
            float dist = System.Math.Abs((bones[0].Pos - target.Pos).Length);
            return dist < acc; // the target is unreachable
        }

        protected void GetDistances(out float[] distances, ref Bone[] bones)
        {
            distances = new float[bones.Length - 1];
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = (bones[i].Pos - bones[i + 1].Pos).Length;
            }
        }
        protected Bone[] TargetUnreachable(Bone[] bones, Vector3 target, Bone grandparent)
        {
            float[] distances;
            GetDistances(out distances, ref bones);
            
            for (int i = 0; i < distances.Length; i++)
            {
                // Position
                float r = (target - bones[i].Pos).Length;
                float l = distances[i] / r;
                Vector3 newPos = ((1 - l) * bones[i].Pos) + (l * target);
                bones[i + 1].Pos = newPos;
                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
                if (bones[i].HasTwistConstraints)
                {
                    Quaternion rotation2;
                    if (CheckOrientationalConstraint(bones[i], (i > 0) ? bones[i - 1] : grandparent, out rotation2))
                    {
                        ForwardKinematics(ref bones, rotation2, i);
                    }
                }
            }
            return bones;
        }
        protected void ForwardKinematics(ref Bone[] bones, Quaternion rotation, int i = 0)
        {
            ForwardKinematics(ref bones, rotation, i, bones.Length-1);
        }
        protected void ForwardKinematics(ref Bone[] bones, Quaternion rotation, int i, int length)
        {
            for (int j = length; j >= i; j--)
            {
                if (j > i)
                {
                    bones[j].Pos = bones[i].Pos +
                        Vector3.Transform((bones[j].Pos - bones[i].Pos), rotation);
                }
                // rotate orientation
                bones[j].Rotate(rotation);
            }
        }
        protected void ForwardRotation(ref Bone[] bones, Quaternion rotation, int i = 0)
        {
            ForwardRotation(ref bones, rotation, i, bones.Length - 1);
        }
        protected void ForwardRotation(ref Bone[] bones, Quaternion rotation, int i, int length)
        {
            for (int j = i; j < length; j++) bones[j].Rotate(rotation);
        }

        // Returns true if target is located on the chain
        // Assumes target is reachable
        protected bool IsTargetOnChain(ref Bone[] bones, ref Bone target)
        {
            // If every joint in the chain (except end effector) has the same direction vector
            // the chain is straight
            for (int i = 0; i < bones.Length - 2; i++)
            {
                Vector3 y1 = bones[i].GetYAxis();
                Vector3 y2 = bones[i + 1].GetYAxis();
                if (y1.X - y2.X > 0.001 && y1.Y - y2.Y > 0.001 && y1.Z - y2.Z > 0.001)
                {
                    return false;
                }
            }
            Vector3 a = bones[bones.Length - 1].Pos; // end effector
            Vector3 b = 2 * bones[0].Pos - a; // end effector reflected in root (ref = 2*root - endef)

            if (Vector3Helper.Parallel(a - target.Pos, b - target.Pos,0.001f))
            {
                // Since target is reachable it is on the line
                return true;
            }
            return false;
        }

        // An orientational constraint is the twist of the bone around its own direction vector
        // with respect to its parent
        // It is defined as an allowed range betwen angles [start,end]
        // where start != end && 0 < start, end <= 360
        // If both start and end is 0 no twist constraint exist
        public bool CheckOrientationalConstraint(Bone b, Bone refBone, out Quaternion rotation)
        {
            if (b.Orientation.Xyz.IsNaN() || refBone.Orientation.Xyz.IsNaN())
            {
                rotation = Quaternion.Identity;
                return false;
            }
            Vector3 thisY = b.GetYAxis();
            Quaternion referenceRotation = refBone.Orientation * b.ParentPointer;
            Vector3 reference = Vector3.Transform(
                    Vector3.Transform(Vector3.UnitZ, referenceRotation),
                    QuaternionHelper.GetRotationBetween(
                            Vector3.Transform(Vector3.UnitY, referenceRotation),
                            thisY));

            float twistAngle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(reference, b.GetZAxis()));

            if (Vector3.CalculateAngle(reference, b.GetXAxis()) > Mathf.PI / 2) // b is twisted left with respect to parent
                twistAngle = 360 - twistAngle;

            float leftLimit = b.StartTwistLimit;
            float rightLimit = b.EndTwistLimit;

            float precision = 0.5f;
            bool inside = (leftLimit >= rightLimit) ? // The allowed range is on both sides of the reference vector
                    inside = twistAngle - leftLimit >= precision || twistAngle - rightLimit <= precision :
                    inside = twistAngle - leftLimit >= precision && twistAngle - rightLimit <= precision;

            if (!inside)//InsideConstraints(twistAngle, startLimit, endLimit)) // not inside constraints 
            {
                // Create a rotation to the closest limit
                float toLeft = Math.Min(360 - Math.Abs(twistAngle - leftLimit), Math.Abs(twistAngle - leftLimit));
                float toRight = Math.Min(360 - Math.Abs(twistAngle - rightLimit), Math.Abs(twistAngle - rightLimit));
                if (toLeft < toRight)
                {
                    // Anti-clockwise rotation to left limit
                    rotation = Quaternion.FromAxisAngle(thisY, -MathHelper.DegreesToRadians(toLeft));
                    return true;
                }
                else
                {
                    // Clockwise to right limit
                    rotation = Quaternion.FromAxisAngle(thisY, MathHelper.DegreesToRadians(toRight));
                    return true;
                }
            }
            rotation = Quaternion.Identity;
            return false;
        }
        private enum Quadrant { q1, q2, q3, q4 };
        private float precision = 0.01f;
        public bool CheckRotationalConstraints(Bone joint, Quaternion parentsRots, Vector3 target, out Vector3 res, out Quaternion rot)
        {
            Quaternion referenceRotation = parentsRots * joint.ParentPointer;
            Vector3 L1 = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, referenceRotation));

            Vector3 jointPos = joint.Pos;
            Vector4 constraints = joint.Constraints;
            Vector3 targetPos = new Vector3(target.X, target.Y, target.Z);
            Vector3 joint2Target = (targetPos - jointPos);

            bool behind = false;
            bool reverseCone = false;
            bool sideCone = false;
            //3.1 Find the line equation L1
            //3.2 Find the projection O of the target t on line L1
            Vector3 O = Vector3Helper.Project(joint2Target, L1);
            if (Math.Abs(Vector3.Dot(L1, joint2Target)) < precision) // target is ortogonal with L1
            {
                O = Vector3.NormalizeFast(L1) * precision;
            }
            else if (Math.Abs(Vector3.Dot(O, L1) - O.LengthFast * L1.LengthFast) >= precision) // O not same direction as L1
            {
                behind = true;
            }
            //3.3 Find the distance between the point O and the joint position
            float S = O.Length;

            //3.4 Map the target (rotate and translate) in such a
            //way that O is now located at the axis origin and oriented
            //according to the x and y-axis ) Now it is a 2D simplified problem
            Quaternion rotation = Quaternion.Invert(referenceRotation);//Quaternion.Invert(parent.Orientation);
            Vector3 TRotated = Vector3.Transform(joint2Target, rotation); // align joint2target vector to  y axis get x z offset
            Vector2 target2D = new Vector2(TRotated.X, TRotated.Z); //only intrested in the X Z cordinates
            //3.5 Find in which quadrant the target belongs 
            // Locate target in a particular quadrant
            //3.6 Find what conic section describes the allowed
            //range of motion
            Vector2 radius;
            Quadrant q;
            #region find Quadrant
            if (target2D.X >= 0 && target2D.Y >= 0)
            {
                radius = new Vector2(constraints.X, constraints.Y);
                q = Quadrant.q1;
            }
            else if (target2D.X >= 0 && target2D.Y < 0)
            {
                q = Quadrant.q2;
                radius = new Vector2(constraints.X, constraints.W);
            }
            else if (target2D.X < 0 && target2D.Y < 0)
            {
                q = Quadrant.q3;
                radius = new Vector2(constraints.Z, constraints.W);
            }
            else /*if (target.X > 0 && target.Y < 0)*/
            {
                q = Quadrant.q4;
                radius = new Vector2(constraints.Z, constraints.Y);
            }

            #endregion
            #region check cone
            if (radius.X > 90 && radius.Y > 90) // cone is reversed if  both angles are larget then 90 degrees
            {
                reverseCone = true;
                radius.X = 90 - (radius.X - 90);
                radius.Y = 90 - (radius.Y - 90);
            }
            else if (behind && radius.X <= 90 && radius.Y <= 90) // target behind and cone i front
            {
                O = -Vector3.NormalizeFast(O) * precision;
                S = O.Length;
            }
            else if (radius.X > 90 || radius.Y > 90) // has one angle > 90, other not, very speciall case
            {
                Vector3 L2 = GetNewL(rotation, q, radius);
                if (!behind) L2 = (L2 - L1) / 2 + L1;
                float angle = Vector3.CalculateAngle(L2, L1);
                Vector3 axis = Vector3.Cross(L2, L1);
                rotation = rotation * Quaternion.FromAxisAngle(axis, angle);
                TRotated = Vector3.Transform(joint2Target, rotation);
                target2D = new Vector2(TRotated.X, TRotated.Z);
                O = Vector3Helper.Project(joint2Target, L2);
                if (Math.Abs(Vector3.Dot(L2, joint2Target)) < precision) // target is ortogonal with L2
                {
                    O = Vector3.Normalize(L2) * precision;
                }
                S = behind ? O.Length : O.Length * 1.4f; //magic number
                if (behind)
                {
                    sideCone = true;
                    if (radius.X > 90)
                    {
                        radius.X = (radius.X - 90);
                    }
                    else
                    {
                        radius.Y = (radius.Y - 90);
                    }
                }
            }
            #endregion

            radius.X = Mathf.Clamp(radius.X, precision, 90 - precision);  // clamp it so if <=0 -> 0.001, >=90 -> 89.999
            radius.Y = Mathf.Clamp(radius.Y, precision, 90 - precision);

            //3.7 Find the conic section which is associated with
            //that quadrant using the distances qj = Stanhj, where
            //j = 1,..,4
            float radiusX = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.X));
            float radiusY = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.Y));

            //3.8 Check whether the target is within the conic section or not
            bool inside = (target2D.X * target2D.X) / (radiusX * radiusX) +
                (target2D.Y * target2D.Y) / (radiusY * radiusY) <= 1 + precision;
            //3.9 if within the conic section then         
            if ((inside && !behind)
                || (!inside && reverseCone)
                || (inside && sideCone)
               )
            {
                //3.10 use the true target position t
                res = target;
                rot = Quaternion.Identity;
                return false;
            }
            //3.11 else
            else
            {
                //3.12 Find the nearest point on that conic section from the target
                Vector2 newPoint = NearestPoint(radiusX, radiusY, target2D);
                Vector3 newPointV3 = new Vector3(newPoint.X, 0.0f, newPoint.Y);

                //3.13 Map (rotate and translate) that point on the
                //conic section via reverse of 3.4 and use that point as
                //the new target position
                rotation = Quaternion.Invert(rotation);
                Vector3 vectorToMoveTo = Vector3.Transform(newPointV3, rotation) + O;
                Vector3 axis = Vector3.Cross(joint2Target, vectorToMoveTo);
                float angle = Vector3.CalculateAngle(joint2Target, vectorToMoveTo);
                rot = Quaternion.FromAxisAngle(axis, angle);
                res = Vector3.Transform(joint2Target, rot) + jointPos;
                return true;
            }
            //3.14 end
        }
        private Vector3 GetNewL(Quaternion rotation, Quadrant q, Vector2 radius)
        {
            Quaternion inverRot = Quaternion.Invert(rotation);
            Vector3 right = Vector3.Transform(Vector3.UnitX, inverRot);
            Vector3 forward = Vector3.Transform(Vector3.UnitZ, inverRot);
            Vector3 L2;
            switch (q)
            {
                case Quadrant.q1:
                    if (radius.X > 90) L2 = right;
                    else L2 = forward;
                    break;
                case Quadrant.q2:
                    if (radius.X > 90) L2 = right;
                    else L2 = -forward;
                    break;
                case Quadrant.q3:
                    if (radius.X > 90) L2 = -right;
                    else L2 = -forward;
                    break;
                case Quadrant.q4:
                    if (radius.X > 90) { L2 = -right; }
                    else L2 = forward;
                    break;
                default:
                    L2 = right;
                    break;
            }
            L2.NormalizeFast();
            return L2;
        }
        private Vector2 NearestPoint(float radiusX, float radiusY, Vector2 target2D)
        {
            Vector2 newPoint;
            float xRad, yRad, pX, pY;
            if (radiusX >= radiusY)
            {
                xRad = Math.Abs(radiusX);
                yRad = Math.Abs(radiusY);
                pX = Math.Abs(target2D.X);
                pY = Math.Abs(target2D.Y);
                newPoint =
                    Mathf.FindNearestPointOnEllipse
                    (xRad, yRad, new Vector2(pX, pY));
            }
            else
            {
                xRad = Math.Abs(radiusY);
                yRad = Math.Abs(radiusX);
                pX = Math.Abs(target2D.Y);
                pY = Math.Abs(target2D.X);
                newPoint =
                    Mathf.FindNearestPointOnEllipse
                    (xRad, yRad, new Vector2(pX, pY));
                MathHelper.Swap(ref newPoint.X, ref newPoint.Y);
            }
            if (target2D.X < 0) newPoint.X = -newPoint.X;
            if (target2D.Y < 0) newPoint.Y = -newPoint.Y;
            return newPoint;
        }
    }
}
