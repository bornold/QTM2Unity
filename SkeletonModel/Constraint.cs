using System;
using OpenTK;

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
            if (b.Orientation.Xyz.IsNaN() || refBone.Orientation.Xyz.IsNaN())
            {
                rotation = Quaternion.Identity;
                return false;
            }
            Vector3 thisY = b.GetYAxis();
            Vector3 thisZ = b.GetZAxis();
            Vector3 thisX = b.GetXAxis();

            Quaternion referenceRotation = refBone.Orientation * b.ParentPointer;
            Vector3 parentY = Vector3.Transform(Vector3.UnitY, referenceRotation);
            Vector3 parentZ = Vector3.Transform(Vector3.UnitZ, referenceRotation);

            Quaternion rot = QuaternionHelper.GetRotationBetween(parentY, thisY);
            Vector3 reference = Vector3.Transform(parentZ, rot);

            float twistAngle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(reference, thisZ));

            if (Vector3.CalculateAngle(reference, thisX) > Mathf.PI / 2) // b is twisted left with respect to parent
                twistAngle = 360 - twistAngle;

            float startLimit = b.StartTwistLimit;
            float endLimit = b.EndTwistLimit;

            if (!InsideConstraints(twistAngle, startLimit, endLimit)) // not inside constraints 
            {
                // Create a rotation to the closest limit
                float toLeft = Math.Min(360 - Math.Abs(twistAngle - startLimit), Math.Abs(twistAngle - startLimit));
                float toRight = Math.Min(360 - Math.Abs(twistAngle - endLimit), Math.Abs(twistAngle - endLimit));
                //UnityDebug.DrawRay(b.Pos, thisX);
                //UnityDebug.DrawRay(b.Pos, reference, UnityEngine.Color.red);
                if (toLeft < toRight)
                {
                    // Anti-clockwise rotation to left limit
                    rotation = Quaternion.FromAxisAngle(thisY, -MathHelper.DegreesToRadians(toLeft));
                    //Debug.Log("Rotating " + toLeft + " degrees clockwise");
                    return true;
                }
                else
                {
                    // Clockwise to right limit
                    rotation = Quaternion.FromAxisAngle(thisY, MathHelper.DegreesToRadians(toRight));
                    //Debug.Log("Rotating " + toRight + " degrees clockwise");
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

            Quaternion referenceRotation = refBone.Orientation * b.ParentPointer;
            Vector3 parentDir = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, referenceRotation));

            Vector3 reference = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, referenceRotation));// refBone.GetZAxis();
            Quaternion rot = QuaternionHelper.GetRotationBetween(parentDir, direction);
            reference = Vector3.Transform(reference, rot);
            reference.Normalize();

            // TODO will the above work for all cases?
      
            float twistAngle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(reference, up));

            if (Vector3.CalculateAngle(reference, x) > Mathf.PI / 2) // b is twisted left with respect to parent
                return 360 - twistAngle;

            return twistAngle;
        }

        public enum Quadrant { q1, q2, q3, q4 }; //TODO private
        private static float precision = 0.01f;
        public static bool CheckRotationalConstraints(Bone joint, Bone parent, Vector3 target, out Vector3 res, out Quaternion rot)
        {
            Quaternion referenceRotation = parent.Orientation * joint.ParentPointer;
            Vector3 L1 = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, referenceRotation));

            Vector3 jointPos = joint.Pos;
            Vector4 constraints = joint.Constraints;
            Vector3 targetPos = new Vector3(target.X, target.Y, target.Z);
            Vector3 joint2Target = (targetPos - jointPos);

            bool behind = false;
            bool reverseCone = false;
            bool sideCone = false;
            //bool some90 = false;
            //3.1 Find the line equation L1
            //Vector3 L1 = jointPos - parentPos;
            //3.2 Find the projection O of the target t on line L1

            Vector3 O = Vector3Helper.Project(joint2Target, L1);
            if (Math.Abs(Vector3.Dot(L1, joint2Target)) < precision) // target is ortogonal with L1
            {
                O = Vector3.Normalize(L1) * precision;
            }
            else if (Math.Abs(Vector3.Dot(O, L1) - O.Length * L1.Length) >= precision) // O not same direction as L1
            {
                behind = true;
                //some90 = constraints.X > 90 || constraints.Y > 90 || constraints.Z > 90 || constraints.W > 90;
            }
            //3.3 Find the distance between the point O and the joint position
            float S = O.Length;
            //UnityDebug.DrawRay(jointPos, -O, UnityEngine.Color.blue, S*100);
            //UnityEngine.Debug.Log("O: " + O);
            //UnityEngine.Debug.Log("S: " + S);
            //UnityEngine.Debug.Log("OPos: " + OPos);
            //UnityEngine.Debug.Log("jointPos: " + jointPos);
            //UnityEngine.Debug.Log("first S: " + S);

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
                S = O.Length;
            }
            else if (radius.X > 90 || radius.Y > 90) // has one angle > 90, other not, very speciall case
            {
                Vector3 L2 = GetNewL(rotation, q, radius);
                //UnityDebug.DrawRay(jointPos, L2, UnityEngine.Color.red, 100);
                //UnityDebug.DrawRay(jointPos, (L2 - L1) / 2 + L1, UnityEngine.Color.green, 100);

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

            //UnityEngine.Debug.Log("degree X: " + radius.X);
            //UnityEngine.Debug.Log("degree Y: " + radius.Y);
            //UnityEngine.Debug.Log("m x: " + radiusX);
            //UnityEngine.Debug.Log("m y: " + radiusY);
            //UnityEngine.Debug.Log("target2D.X: " + target2D.X);
            //UnityEngine.Debug.Log("target2D.Y: " + target2D.Y);
            //UnityEngine.Debug.Log("used S: " + S);
            //UnityEngine.Debug.Log("used O: " + O);

            //if (inside || behind || reverseCone || sideCone)
            //    UnityEngine.Debug.Log(
            //        (inside ? " inside " : "") +
            //        (behind ? " behind " : "") +
            //        (reverseCone ? " reverseCone " : "") +
            //        (sideCone ? " sideCone " : ""));

            //3.9 if within the conic section then         
            if (
                //   ( inside && !reverseCone && !behind)
                //|| ( inside &&  reverseCone && !behind)
                   (inside  && !behind)
                || (!inside &&  reverseCone)// &&  behind)
//                || (!inside &&  reverseCone && !behind)
                //|| ( inside && !reverseCone &&  behind && sideCone)
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
                //UnityDebug.CreateEllipse(radiusX, radiusY, 400);
                //UnityDebug.DrawLine(new Vector3(target2D.X, 0, target2D.Y), newPointV3);
                rotation = Quaternion.Invert(rotation);
                Vector3 moveTo = Vector3.Transform(newPointV3, rotation);
                moveTo += O + jointPos;
                Vector3 vectorToMoveTo = (moveTo - jointPos);
                Vector3 axis = Vector3.Cross(joint2Target, vectorToMoveTo);
                float angle = Vector3.CalculateAngle(joint2Target, vectorToMoveTo);
                 rot = Quaternion.FromAxisAngle(axis, angle);
                res = Vector3.Transform(joint2Target, rot) + jointPos;

                //UnityDebug.CreateEllipse(radiusX, radiusY, (O + jointPos).Convert(), rotation.Convert(), 400, UnityEngine.Color.cyan);
                //UnityDebug.DrawLine(targetPos, moveTo, UnityEngine.Color.magenta);
                //if (res.IsNaN()) UnityEngine.Debug.LogError("jointPos " + jointPos + "constraints " + constraints + " L1 " + L1 + " targetPos " + targetPos);
                return true;
            }
            //3.14 end
        }
        private static Vector3 GetNewL(Quaternion rotation, Quadrant q, Vector2 radius)
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
            L2.Normalize();
            return L2;
        }
        private static Vector2 NearestPoint(float radiusX, float radiusY, Vector2 target2D)
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
