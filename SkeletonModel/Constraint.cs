using System;
using OpenTK;

namespace QTM2Unity
{
    public static class Constraint
    {
        // An orientational constraint is the twist of the bone around its own direction vector
        // with respect to its parent
        // It is defined as a range betwen angles [right,left]
        public static bool CheckOrientationalConstraint(Bone b, Bone refer, out Quaternion rotation)
        {
            Vector3 reference = refer.GetUp();
            Quaternion q = QuaternionHelper.LookAtUp(refer.Pos, b.Pos, b.GetUp());
            //UnityDebug.DrawRays2(q, parent.Pos, 2f);
            float z2z = MathHelper.RadiansToDegrees(
                    Vector3.CalculateAngle(Vector3.Transform(Vector3.UnitZ, q), reference));
            //UnityEngine.Debug.Log("z2z: " + z2z);
            float x2z = MathHelper.RadiansToDegrees(
                    Vector3.CalculateAngle(Vector3.Transform(Vector3.UnitX, q), reference));
            //UnityEngine.Debug.Log("x2z: " + x2z);


            //float angle = Vector3.CalculateAngle(Vector3.UnitZ, parent.GetUp());
            //Vector3 axis = Vector3.Cross(Vector3.UnitZ, parent.GetUp());
            //Quaternion rot = Quaternion.FromAxisAngle(axis, angle);
            //float yc = Mathf.Cos(MathHelper.DegreesToRadians(b.OrientationalConstraint.Right));
            //float xc = Mathf.Sin(MathHelper.DegreesToRadians(b.OrientationalConstraint.Right));
            //Vector3 r = new Vector3(-xc, 0, yc);
            //r = Vector3.Transform(r, rot);
            //r.Normalize();
            //UnityDebug.DrawLine(b.Pos, b.Pos + r, UnityEngine.Color.yellow);
            //yc = Mathf.Cos(MathHelper.DegreesToRadians(b.OrientationalConstraint.Left));
            //xc = Mathf.Sin(MathHelper.DegreesToRadians(b.OrientationalConstraint.Left));
            //Vector3 l = new Vector3(xc, 0, yc);
            //l = Vector3.Transform(l, rot);
            //l.Normalize();
            //UnityDebug.DrawLine(b.Pos, b.Pos + l, UnityEngine.Color.cyan);
            //UnityDebug.DrawLine(Vector3.UnitZ, Vector3.UnitZ + b.GetUp(), UnityEngine.Color.blue);

            Vector3 direction = b.GetDirection();
            if (x2z >= 90) // Z left of reference
            {
                //UnityEngine.Debug.Log(string.Format("Z left of reference: x2z({0})>90 ", x2z));
                float pew = z2z - b.LeftTwist;
                if (pew > 1f) // angle larger then constraints angle
                {
                    //UnityEngine.Debug.Log(string.Format("outside left constraintspew({0})>1 ", pew));
                    //UnityEngine.Debug.Log(string.Format("Rotate: {0} degrees ", -pew));
                    pew = MathHelper.DegreesToRadians(-pew);
                    rotation = Quaternion.FromAxisAngle(direction, pew); ;
                    return true;
                    //UnityDebug.DrawRays2(rotation * b.Orientation, b.Pos, 0.5f);
                }
                //else
                //{
                //    UnityEngine.Debug.Log(string.Format("Outside right constriants pew({0})<1 ", pew));
                //}
            }
            else // Z right of reference
            {
                //UnityEngine.Debug.Log(string.Format("Z right of reference :  x2z({0})<90 ", x2z));
                //UnityEngine.Debug.Log(string.Format("b.OrientationalConstraint.right :  {0}", b.OrientationalConstraint.right));
                float pew = z2z - b.RightTwist;
                if (pew > 1f) // angle larger constraints angle
                {
                    //UnityEngine.Debug.Log(string.Format("Outside right constriants pew({0})>1 ", pew));
                    pew = MathHelper.DegreesToRadians(pew);
                    rotation = Quaternion.FromAxisAngle(direction, pew);
                    return  true;
                    //UnityDebug.DrawRays2(rotation * b.Orientation, b.Pos, 0.5f);
                }
                //else
                //{
                //    UnityEngine.Debug.Log(string.Format("Inside right constraints pew({0})<1 ", pew));
                //}
            }
            rotation = b.Orientation;
            return false;
        }

        public static void CheckOrientationalConstraint2(ref Bone b, Bone parent, float Left, float Right)
        {

            Vector3 direction = b.GetDirection();
            float twistAngle = GetTwistAngle(b, parent);

            float from = Right;
            float to = Left;

            if (!(twistAngle >= from && twistAngle <= to)) // not inside constraints
            {
                // rotate the bone around its direction vector to be inside
                // the constraints (rotate it from its current angle to from or to)
                // TODO rotating the right directio? (-/+)
                if (twistAngle < from) // TODO add some precision (so it doesn't need to rotate eg 0,000324)
                {
                    // rotate clockwise
                    /*Debug.Log("Twistangle is " + twistAngle + ". Rotating " + b.Name + 
                        " " + (twistAngle - from) + " clockwise around itself.");*/
                    b.Rotate(Math.Abs(MathHelper.DegreesToRadians(twistAngle - from)), direction);
                }
                else if (twistAngle > to)
                {
                    // rotate anticlockwise
                    /*Debug.Log("Twistangle is " + twistAngle + ". Rotating " + b.Name +
                        " " + (twistAngle - to) + " anticlockwise around itself.");*/
                    b.Rotate(-Math.Abs(MathHelper.DegreesToRadians(twistAngle - to)), direction);
                }
            }

        }

        // Calculates the angle b is twisted around its direction vector in radians
        // TODO make private. Only public for testing purposes.
        public static float GetTwistAngle(Bone b, Bone parent)
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

        private enum Q { q1, q2, q3, q4 };
        private static float precision = 0.001f;
        // A constraint modeled as an irregular cone
        // The direction vector is the direction the cone is opening up at
        // The four angles define the shape of the cone
        // angle 0 is the angle to the parent's right
        // angle 1 is the angle to the parent's down
        // angle 2 is the angle to the parent's left
        // angle 3 is the angle to the parent's up
#if false
        Func<Vector3> directionMethod;
        Func<Vector3> rightMethod;
        private float right, up, left, down;
        public Vector4 Constraints
        {
            get { return new Vector4(right, up, left, down); }
        }
        public RotationalConstraint(float _right, float _up, float _left, float _down)
        {
            this.right = _right;
            this.up = _up;
            this.left = _left;
            this.down = _down;
        }
        public RotationalConstraint(Vector4 givenConstraints)
        {
            this.right = givenConstraints.X;
            this.up = givenConstraints.Y;
            this.left = givenConstraints.Z;
            this.down = givenConstraints.W;
        }
#endif
        public static bool CheckRotationalConstraints(Bone joint, Vector3 target, Vector3 L1, out Vector3 res)
        {
            Vector3 jointPos = joint.Pos;
            Vector4 constraints = joint.Constraints;
            Vector3 targetPos = new Vector3(target.X, target.Y, target.Z);
            Vector3 joint2Target = (targetPos - jointPos);

            bool behind = false;
            bool reverseCone = false;
            bool sideCone = false;

            //3.1 Find the line equation L1
            //Vector3 L1 = jointPos - parentPos;
            //3.2 Find the projection O of the target t on line L1

            Vector3 O = Vector3Helper.Project(joint2Target, L1);

            Vector3 OPos = O + jointPos;

            if (Math.Abs(Vector3.Dot(L1, joint2Target)) < precision) // target is ortogonal with L1
            {
                O = Vector3.Normalize(L1) * precision * 10;
                OPos = O + jointPos;

            }
            else if (Math.Abs(Vector3.Dot(O, L1) - O.Length * L1.Length) >= precision) // O not same direction as L1
            {
                behind = true;
            }

            //3.3 Find the distance between the point O and the joint position
            float S = (OPos - jointPos).Length;

            //3.4 Map the target (rotate and translate) in such a
            //way that O is now located at the axis origin and oriented
            //according to the x and y-axis ) Now it is a 2D simplified problem

            /*
                    Vector3 reference = refer.GetUp();
                    Quaternion q = QuaternionHelper.LookAtUp(refer.Pos, b.Pos, b.GetUp());
                    
                    float z2z = MathHelper.RadiansToDegrees(
                    Vector3.CalculateAngle(Vector3.Transform(Vector3.UnitZ, q), reference));
             */

            float angle = Vector3.CalculateAngle(L1, Vector3.UnitY);
            Vector3 axis = Vector3.Cross(L1, Vector3.UnitY);
            Quaternion rotation = Quaternion.FromAxisAngle(axis, angle);

            angle = Vector3.CalculateAngle(joint.GetDirection(), Vector3.UnitY);
            axis = Vector3.Cross(joint.GetDirection(), Vector3.UnitY);
            Quaternion what = Quaternion.FromAxisAngle(axis, angle);

            Vector3 xxx = Vector3.Transform(joint.GetRight(), what);

            float twist = Vector3.CalculateAngle(Vector3.UnitX, xxx);

            //UnityEngine.Debug.Log(MathHelper.RadiansToDegrees(test));

            rotation = rotation * Quaternion.Invert(Quaternion.FromAxisAngle(L1, twist)) ;
            
            Vector3 TRotated = Vector3.Transform(joint2Target, rotation);
            Vector2 target2D = new Vector2(TRotated.X, TRotated.Z);

            //3.5 Find in which quadrant the target belongs 
            // Locate target in a particular quadrant
            //3.6 Find what conic section describes the allowed
            //range of motion
            Vector2 radius;
            Q q;
            #region find Quadrant
            if (target2D.X >= 0 && target2D.Y >= 0)
            {
                radius = new Vector2(constraints.X, constraints.Y);
                q = Q.q1;
            }
            else if (target2D.X >= 0 && target2D.Y < 0)
            {
                q = Q.q2;
                radius = new Vector2(constraints.X, constraints.W);
            }
            else if (target2D.X < 0 && target2D.Y < 0)
            {
                q = Q.q3;
                radius = new Vector2(constraints.Z, constraints.W);
            }
            else /*if (target.X > 0 && target.Y < 0)*/
            {
                q = Q.q4;
                radius = new Vector2(constraints.Z, constraints.Y);
            }
            //UnityEngine.Debug.Log(q + " " + radius);
            #endregion
            #region check cone
            if (radius.X > 90 && radius.Y > 90) // cone is reversed
            {
                reverseCone = true;
                radius.X = 90 - (radius.X - 90);
                radius.Y = 90 - (radius.Y - 90);
            }
            else if ((behind) && (radius.X > 90 || radius.Y > 90)) // has one angle > 90, other not, very speciall case
            {
                sideCone = true;
                Quaternion inverRot = Quaternion.Invert(rotation);
                Vector3 right = Vector3.Transform(Vector3.UnitX, inverRot);
                Vector3 forward = Vector3.Transform(Vector3.UnitY, inverRot);
                Vector3 L2;
                switch (q)
                {
                    case Q.q1:
                        if (radius.X > 90) L2 = right;
                        else L2 = forward;
                        break;
                    case Q.q2:
                        if (radius.X > 90) L2 = right;
                        else L2 = -forward;
                        break;
                    case Q.q3:
                        if (radius.X > 90) L2 = -right;
                        else L2 = -forward;
                        break;
                    case Q.q4:
                        if (radius.X > 90) { L2 = -right; }
                        else L2 = forward;
                        break;
                    default:
                        L2 = right;
                        break;
                }
                L2.Normalize();

                angle = Vector3.CalculateAngle(L2, Vector3.UnitY);
                axis = Vector3.Cross(L2, Vector3.UnitY);
                rotation = Quaternion.FromAxisAngle(axis, angle);

                TRotated = Vector3.Transform(joint2Target, rotation);
                target2D = new Vector2(TRotated.X, TRotated.Z);
                O = Vector3Helper.Project(joint2Target, L2);

                OPos = O + jointPos;
                S = (jointPos - OPos).Length;

                if (radius.X > 90)
                {
                    radius.X = (radius.X - 90);
                }
                else
                {
                    radius.Y = (radius.Y - 90);
                }
            }
            else if (behind && radius.X <= 90 && radius.Y <= 90) // behind and cone i front
            {
                O = -O;
                OPos = O + jointPos;
                radius.X = Math.Min(90 - precision, radius.X); // clamp it so if 90 -> 89.999, 
                radius.Y = Math.Min(90 - precision, radius.Y);
            }
            #endregion

            if (!behind) // or just infront
            {
                // if 90, Tan gets to big, not good, clamp it to 89
                radius.X = Math.Min(90 - precision, radius.X); // clamp it so if 90 -> 89.999, 
                radius.Y = Math.Min(90 - precision, radius.Y);
            }

            radius.X = Mathf.Clamp(radius.X, precision, 90 - precision);  // clamp it so if <=0 -> 0.001, >=90 -> 89.999
            radius.Y = Mathf.Clamp(radius.Y, precision, 90 - precision);

            //3.7 Find the conic section which is associated with
            //that quadrant using the distances qj = Stanhj, where
            //j = 1,..,4
            if (S < precision) S = precision;
            float radiusX = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.X));
            float radiusY = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.Y));

            //3.8 % Check whether the target is within the conic
            //section or not
            bool inside = (target2D.X * target2D.X) / (radiusX * radiusX) +
                (target2D.Y * target2D.Y) / (radiusY * radiusY) <= 1 + precision;

            //3.9 if within the conic section then         
            //UnityEngine.Debug.Log(" inside: " + inside + " reverseCone: " + reverseCone + " behind: " + behind + " sidecone: " + sideCone);
            if (
                   (inside && !reverseCone && !behind)
                || (inside && reverseCone && !behind)
                || (!inside && reverseCone && behind)
                || (!inside && reverseCone && !behind)
                || (inside && !reverseCone && behind && sideCone)
               )
            {
                //3.10 use the true target position t
                res = target;
                return false;
            }
            //3.11 else
            else
            {

                //3.12 Find the nearest point on that conic section from the target
                Vector2 newPoint = NearestPoint(radiusX, radiusY, target2D, q, reverseCone);

                Vector3 newPointV3 = new Vector3(newPoint.X, 0.0f, newPoint.Y);
                //UnityDebug.CreateEllipse(radiusX, radiusY, 500, UnityEngine.Color.black);
                //UnityDebug.DrawLine(new Vector3(target2D.X,0,target2D.Y), newPointV3);

                //3.13 Map (rotate and translate) that point on the
                //conic section via reverse of 3.4 and use that point as
                //the new target position
                rotation = Quaternion.Invert(rotation);
                Vector3 moveTo = Vector3.Transform(newPointV3, rotation);
                moveTo += OPos;
                //UnityDebug.CreateIrregularCone3(constraints, jointPos, rotation, 50, 0.5f);
                //UnityDebug.CreateEllipse(radiusX, radiusY,  OPos.Convert(), rotation.Convert(), 400, UnityEngine.Color.cyan);
                //UnityDebug.DrawLine(target, moveTo, UnityEngine.Color.magenta);
                Vector3 vectorToMoveTo = (moveTo - jointPos);
                axis = Vector3.Cross(joint2Target, vectorToMoveTo);
                angle = Vector3.CalculateAngle(joint2Target, vectorToMoveTo);
                Quaternion rot = Quaternion.FromAxisAngle(axis, angle);
                res = Vector3.Transform(joint2Target, rot) + jointPos;

                //UnityEngine.Debug.Log("joint2res " + (res - jointPos).Length);
                //UnityEngine.Debug.Log("joint2Target " + (joint2Target).Length);

                return true;
            }
            //3.14 end
        }
        private static Vector2 NearestPoint(float radiusX, float radiusY, Vector2 target2D, Q q, bool reverseCone)
        {
            Vector2 newPoint;
            float xRad, yRad, pX, pY;

            if (radiusX >= radiusY)//^ reverseCone)
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
            switch (q)
            {
                case Q.q1:
                    break;
                case Q.q2:
                    newPoint.Y = -newPoint.Y;
                    break;
                case Q.q3:
                    newPoint.X = -newPoint.X;
                    newPoint.Y = -newPoint.Y;
                    break;
                case Q.q4:
                    newPoint.X = -newPoint.X;
                    break;
                default:
                    break;
            }
            return newPoint;
        }
    }
}
