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
            float z2z = MathHelper.RadiansToDegrees(
                    Vector3.CalculateAngle(Vector3.Transform(Vector3.UnitZ, q), reference));
            float x2z = MathHelper.RadiansToDegrees(
                    Vector3.CalculateAngle(Vector3.Transform(Vector3.UnitX, q), reference));

            Vector3 direction = b.GetDirection();
            if (x2z >= 90) // Z left of reference
            {
                float pew = z2z - b.LeftTwist;
                if (pew > 1f) // angle larger then constraints angle
                {
                    //UnityEngine.Debug.Log("Z left of reference ROTATING: " + -pew);
                    pew = MathHelper.DegreesToRadians(-pew);
                    rotation = Quaternion.FromAxisAngle(direction, pew); ;
                    return true;
                }
            }
            else // Z right of reference
            {
                float pew = z2z - b.RightTwist;
                if (pew > 1f) // angle larger constraints angle
                {
                    //UnityEngine.Debug.Log("Z right of reference ROTATING: " + pew);
                    pew = MathHelper.DegreesToRadians(pew);
                    rotation = Quaternion.FromAxisAngle(direction, pew);
                    return  true;
                }
            }
            rotation = Quaternion.Identity;
            return false;
        }

        public static bool CheckOrientationalConstraint2(Bone b, Bone parent, out Quaternion rotation)
        {

            Vector3 direction = b.GetDirection();
            float twistAngle = GetTwistAngle(b, parent);

            float from = b.RightTwist;
            float to = b.LeftTwist;

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
                    rotation = Quaternion.FromAxisAngle(direction, Math.Abs(MathHelper.DegreesToRadians(twistAngle - from)));
                    return true;
                }
                else if (twistAngle > to)
                {
                    // rotate anticlockwise
                    /*Debug.Log("Twistangle is " + twistAngle + ". Rotating " + b.Name +
                        " " + (twistAngle - to) + " anticlockwise around itself.");*/
                    rotation = Quaternion.FromAxisAngle(direction, -Math.Abs(MathHelper.DegreesToRadians(twistAngle - to)));
                    return true;
                }
            }
            rotation = Quaternion.Identity;
            return false;
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
        private static float precision = 0.1f;
        // A constraint modeled as an irregular cone
        // The direction vector is the direction the cone is opening up at

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

            float angle = Vector3.CalculateAngle(L1, Vector3.UnitY);
            Vector3 axis = Vector3.Cross(L1, Vector3.UnitY);
            Quaternion rotation = Quaternion.FromAxisAngle(axis, angle);

            //Caclulating twist angle, this is a wierd way to do it, but i think it works.
            angle = Vector3.CalculateAngle(joint.GetDirection(), Vector3.UnitY); // diff in Y axis
            axis = Vector3.Cross(joint.GetDirection(), Vector3.UnitY); 
            Quaternion yAligned = Quaternion.FromAxisAngle(axis, angle); // rotation so that Y axis align
            Vector3 rigthNow = Vector3.Transform(joint.GetRight(), yAligned); // Get X axis such that is is when Y aligned
            float twist = Vector3.CalculateAngle(Vector3.UnitX, rigthNow); // angle between them is the twist angle
            Quaternion twistRot = Quaternion.Invert(Quaternion.FromAxisAngle(L1, twist)); // Quaternion representing the twist angle over L1

            rotation = rotation * twistRot; // apply twist rotation on ordinary rotation
            
            Vector3 TRotated = Vector3.Transform(joint2Target, rotation); // align joint2target vector to  y axis get x z offset
            Vector2 target2D = new Vector2(TRotated.X, TRotated.Z); //only intrested in the X Z cordinates

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
            if (radius.X > 90 && radius.Y > 90) // cone is reversed if  both angles are larget then 90 degrees
            {
                reverseCone = true;
                radius.X = 90 - (radius.X - 90);
                radius.Y = 90 - (radius.Y - 90);
            }
            else if (behind && radius.X <= 90 && radius.Y <= 90) // target behind and cone i front
            {
                O = -O;
                OPos = O + jointPos;
            }
            else if (behind && (radius.X > 90 || radius.Y > 90)) // has one angle > 90, other not, very speciall case
            {
                sideCone = true;
                Quaternion inverRot = Quaternion.Invert(rotation);
                Vector3 right = Vector3.Transform(Vector3.UnitX, inverRot);
                Vector3 forward = Vector3.Transform(Vector3.UnitZ, inverRot);
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
                angle = Vector3.CalculateAngle(L2, L1);
                axis = Vector3.Cross(L2, L1);
                rotation = rotation * Quaternion.FromAxisAngle(axis, angle);
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
            #endregion

            radius.X = Mathf.Clamp(radius.X, precision, 90 - precision);  // clamp it so if <=0 -> 0.001, >=90 -> 89.999
            radius.Y = Mathf.Clamp(radius.Y, precision, 90 - precision);

            //3.7 Find the conic section which is associated with
            //that quadrant using the distances qj = Stanhj, where
            //j = 1,..,4
            if (S < precision) S = precision;
            float radiusX = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.X));
            float radiusY = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.Y));
            
            //3.8 Check whether the target is within the conic section or not
            bool inside = (target2D.X * target2D.X) / (radiusX * radiusX) +
                (target2D.Y * target2D.Y) / (radiusY * radiusY) <= 1 + precision;

            //UnityEngine.Debug.Log("radius.X: " + radius.X);
            //UnityEngine.Debug.Log("radius.Y: " + radius.Y);
            //UnityEngine.Debug.Log("target2D.X: " + target2D.X);
            //UnityEngine.Debug.Log("target2D.Y: " + target2D.Y);
            //UnityEngine.Debug.Log("S: " + S);
            //UnityEngine.Debug.Log(" inside: " + inside + " reverseCone: " + reverseCone + " behind: " + behind + " sidecone: " + sideCone);
            
            //3.9 if within the conic section then         
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
                Vector2 newPoint = NearestPoint(radiusX, radiusY, target2D, q);
                Vector3 newPointV3 = new Vector3(newPoint.X, 0.0f, newPoint.Y);

                //3.13 Map (rotate and translate) that point on the
                //conic section via reverse of 3.4 and use that point as
                //the new target position
                rotation = Quaternion.Invert(rotation);
                Vector3 moveTo = Vector3.Transform(newPointV3, rotation);
                moveTo += OPos;
                Vector3 vectorToMoveTo = (moveTo - jointPos);
                axis = Vector3.Cross(joint2Target, vectorToMoveTo);
                angle = Vector3.CalculateAngle(joint2Target, vectorToMoveTo);
                Quaternion rot = Quaternion.FromAxisAngle(axis, angle);
                res = Vector3.Transform(joint2Target, rot) + jointPos;

                //UnityDebug.CreateEllipse(radiusX, radiusY, OPos.Convert(), rotation.Convert(), 400, UnityEngine.Color.cyan);
                //UnityDebug.DrawLine(targetPos, moveTo, UnityEngine.Color.magenta);
                //UnityEngine.Debug.Log("joint2res " + (res - jointPos).Length);
                //UnityEngine.Debug.Log("joint2Target " + (joint2Target).Length);

                return true;
            }
            //3.14 end
        }
        private static Vector2 NearestPoint(float radiusX, float radiusY, Vector2 target2D, Q q)
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
