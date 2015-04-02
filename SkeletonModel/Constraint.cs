using System;
using OpenTK;
using Debug = UnityEngine.Debug;

namespace QTM2Unity
{
    public static class Constraint
    {
        // An orientational constraint is the twist of the bone around its own direction vector
        // with respect to its parent
        // It is defined as an allowed range betwen angles [start,end]
        // where start != end && 0 < start, end <= 360
        // If both start and end is 0 no twist constraint exist
        public static bool CheckOrientationalConstraint(Bone b, Bone refBone, out Quaternion rotation)
        {
            Vector3 direction = b.GetYAxis();
            float twistAngle = GetTwistAngle(b, refBone);

            float startLimit = b.StartTwistLimit;
            float endLimit = b.EndTwistLimit;

            if (!InsideConstraints(twistAngle, startLimit, endLimit)) // not inside constraints 
            {
                // Create a rotation to the closest limit
                float toLeft = Math.Min(360 - Math.Abs(twistAngle - startLimit), Math.Abs(twistAngle - startLimit));
                float toRight = Math.Min(360 - Math.Abs(twistAngle - endLimit), Math.Abs(twistAngle - endLimit));
                if (toLeft < toRight)
                {
                    // Anti-clockwise rotation to left limit
                    rotation = Quaternion.FromAxisAngle(direction, -MathHelper.DegreesToRadians(toLeft));
                    Debug.Log("Rotating " + toLeft + " degrees clockwise");
                    return true;
                }
                else
                {
                    // Clockwise to right limit
                    rotation = Quaternion.FromAxisAngle(direction, MathHelper.DegreesToRadians(toRight));
                    Debug.Log("Rotating " + toRight + " degrees clockwise");
                    return true;
                }
            }
            rotation = Quaternion.Identity;
            return false;
        }

        // Checks if the twist is inside the allowed range +/- 0.5 degrees
        private static bool InsideConstraints(float twistAngle, float leftLimit, float rightLimit)
        {
            float precision = 0.5f;
            if (leftLimit >= rightLimit) // The allowed range is on both sides of the reference vector
            {
                return twistAngle - leftLimit >= precision || twistAngle - rightLimit <= precision;
            }
            else
            {
                return twistAngle - leftLimit >= precision && twistAngle - rightLimit <= precision;
            }
        }

        // Calculates the angle b is twisted around its direction vector with respect to refBone (in radians)
        // TODO make private. Only public for testing purposes.
        public static float GetTwistAngle(Bone b, Bone refBone)
        {
            Vector3 direction = b.GetYAxis();
            Vector3 up = b.GetZAxis();
            Vector3 x = b.GetXAxis();

            Vector3 reference = refBone.GetZAxis();
            Quaternion rot = QuaternionHelper.GetRotationBetween(refBone.GetYAxis(), direction);
            reference = Vector3.Transform(reference, rot);
            reference.Normalize();

            // TODO will the above work for all cases?

            float twistAngle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(reference, up));

            if (Vector3.CalculateAngle(reference, x) > Mathf.PI / 2) // b is twisted left with respect to parent
                return 360 - twistAngle;

            return twistAngle;
        }

#if f
        // Calculates the angle b is twisted around its direction vector with respect to refBone (in radians)
        // TODO make private. Only public for testing purposes.
        public static float GetTwistAngle2(Bone b, Bone refBone)
        {
            Vector3 direction = b.GetYAxis();
            Vector3 up = b.GetZAxis();
            Vector3 right = b.GetXAxis();

            // construct a reference vector which the twist/orientation will depend on
            // The reference is the parents up vector projected on the same plane as the 
            // current bone's up vector
            Vector3 reference = Vector3Helper.ProjectOnPlane(refBone.GetZAxis(), direction);
            reference.Normalize(); 
            // TODO the above won't work if parent.up is perpendicular to the plane
            // and that vill happen if the angle between b and parent is 90 degrees

            float twistAngle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(reference, up));

            if (Vector3.CalculateAngle(reference, right) > Mathf.PI / 2) // b is twisted left with respect to parent
                return 360 - twistAngle;

            return twistAngle;
        }
#endif

        private enum Q { q1, q2, q3, q4 };
        private static float precision = 0.01f;
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
            bool some90 = false;
            //3.1 Find the line equation L1
            //Vector3 L1 = jointPos - parentPos;
            //3.2 Find the projection O of the target t on line L1

            Vector3 O = Vector3Helper.Project(joint2Target, L1);

            Vector3 OPos = O + jointPos;

            if (Math.Abs(Vector3.Dot(L1, joint2Target)) < precision) // target is ortogonal with L1
            {
                O = Vector3.Normalize(L1) * precision;
                OPos = O + jointPos;
            }
            else if (Math.Abs(Vector3.Dot(O, L1) - O.Length * L1.Length) >= precision) // O not same direction as L1
            {
                behind = true;
                some90 = constraints.X > 90 || constraints.Y > 90 || constraints.Z > 90 || constraints.W > 90;
            }

            //if (some90) UnityEngine.Debug.Log("we are behind and some angle is over 90");
            //3.3 Find the distance between the point O and the joint position
            float S = (OPos - jointPos).Length;
            //UnityEngine.Debug.Log("O: " + O);
            //UnityEngine.Debug.Log("S: " + S);
            //UnityEngine.Debug.Log("OPos: " + OPos);
            //UnityEngine.Debug.Log("jointPos: " + jointPos);
            //UnityEngine.Debug.Log("first S: " + S);

            //3.4 Map the target (rotate and translate) in such a
            //way that O is now located at the axis origin and oriented
            //according to the x and y-axis ) Now it is a 2D simplified problem

            float angle = Vector3.CalculateAngle(L1, Vector3.UnitY);
            Vector3 axis = Vector3.Cross(L1, Vector3.UnitY);
            Quaternion rotation = Quaternion.FromAxisAngle(axis, angle);

            //Caclulating twist angle, this is a wierd way to do it, but i think it works.
            angle = Vector3.CalculateAngle(joint.GetYAxis(), Vector3.UnitY); // diff in Y axis
            axis = Vector3.Cross(joint.GetYAxis(), Vector3.UnitY);
            Quaternion yAligned = Quaternion.FromAxisAngle(axis, angle); // rotation so that Y axis align
            Vector3 rigthNow = Vector3.Transform(joint.GetXAxis(), yAligned); // Get X axis such that is is when Y aligned
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
                //O = -O;
                O = -Vector3.Normalize(O) * precision;
                OPos = O + jointPos;
                S = (OPos - jointPos).Length;
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
            //if (S < precision) S = precision;
            float radiusX = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.X));
            float radiusY = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.Y));

            //3.8 Check whether the target is within the conic section or not
            bool inside = (target2D.X * target2D.X) / (radiusX * radiusX) +
                (target2D.Y * target2D.Y) / (radiusY * radiusY) <= 1 + precision;

            //UnityEngine.Debug.Log("degree X: " + radius.X);
            //UnityEngine.Debug.Log("degree Y: " + radius.Y);
            //UnityEngine.Debug.Log("m x: " + radiusX);
            //UnityEngine.Debug.Log("m y: " + radiusY);
            //UnityEngine.Debug.Log("target2D.X: " + target2D.X);
            //UnityEngine.Debug.Log("target2D.Y: " + target2D.Y);
            //UnityEngine.Debug.Log("used S: " + S);
            //UnityEngine.Debug.Log("used O: " + O);
            //UnityEngine.Debug.Log(
            //    " inside: " + inside +
            //    " behind: " + behind +
            //    " reverseCone: " + reverseCone +
            //    " sidecone: " + sideCone +
            //    " some90: " + some90);

            //UnityEngine.Debug.Log(" inside: " + inside);
            //UnityEngine.Debug.Log(" behind: " + behind);
            //UnityEngine.Debug.Log(" reverseCone: " + reverseCone);
            //UnityEngine.Debug.Log(" sidecone: " + sideCone);
            //UnityEngine.Debug.Log(" some90: " + some90);

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
                //if (res.IsNaN()) UnityEngine.Debug.LogError("jointPos " + jointPos + "constraints " + constraints + " L1 " + L1 + " targetPos " + targetPos);

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

#if f
        // An orientational constraint is the twist of the bone around its own direction vector
        // with respect to its parent
        // It is defined as a range betwen angles [left,right]
        public static bool CheckOrientationalConstraint(Bone b, Bone refer, out Quaternion rotation)
        {
            Vector3 reference = refer.GetZAxis();
            Quaternion q = QuaternionHelper.LookAtUp(refer.Pos, b.Pos, b.GetZAxis());
            float z2z = MathHelper.RadiansToDegrees(
                    Vector3.CalculateAngle(Vector3.Transform(Vector3.UnitZ, q), reference));
            float x2z = MathHelper.RadiansToDegrees(
                    Vector3.CalculateAngle(Vector3.Transform(Vector3.UnitX, q), reference));

            Vector3 direction = b.GetYAxis();
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

        public static bool CheckOrientationalConstraint2(Bone b, Bone refBone, out Quaternion rotation)
        {
            Vector3 direction = b.GetYAxis();
            float twistAngle = GetTwistAngle(b, refBone);

            // The left limit is negative if it is less than 180 degrees, otherwise positive
            float left = b.LeftTwist < 180 ? -b.LeftTwist : 360 - b.LeftTwist;
            // The right limit is positive if it is less than 180 degrees, otherwise negative
            float right = b.RightTwist < 180 ? b.RightTwist : -360 - b.RightTwist;

            if (!(twistAngle >= left && twistAngle <= right)) // not inside constraints TODO: add some precision
            {
                // TODO: BLAH FUNKAR EJ

                // rotate the bone around its direction vector to be inside
                // the constraints (rotate it from its current angle to left or right angles)
                /*if (Math.Abs(left - twistAngle) < Math.Abs(right - twistAngle))
                {
                    // Rotate to the left limit
                    rotation = Quaternion.FromAxisAngle(direction, Math.Abs(MathHelper.DegreesToRadians(left - twistAngle)));
                    Debug.Log("Rotating " + Math.Abs(left - twistAngle) + " degrees");
                    return true;
                }
                else // Rotate to the right limit
                {
                    rotation = Quaternion.FromAxisAngle(direction, Math.Abs(MathHelper.DegreesToRadians(right - twistAngle)));
                    Debug.Log("Rotating " + Math.Abs(left - twistAngle) + " degrees");
                    return true;
                }*/

                if (twistAngle < left) // TODO add some precision (so it doesn't need to rotate eg 0,000324)
                {
                    // rotate clockwise
                    rotation = Quaternion.FromAxisAngle(direction, -Math.Abs(MathHelper.DegreesToRadians(twistAngle - left)));
                    Debug.Log("Rotating " + Math.Abs(twistAngle - left) + " degrees clockwise");
                    return true;
                }
                else if (twistAngle > right)
                {
                    // rotate anticlockwise
                    rotation = Quaternion.FromAxisAngle(direction, Math.Abs(MathHelper.DegreesToRadians(twistAngle - right)));
                    Debug.Log("Rotating " + Math.Abs(twistAngle - right) + " degrees anticlockwise");
                    return true;
                }
            }
            rotation = Quaternion.Identity;
            return false;
        }
#endif
}
