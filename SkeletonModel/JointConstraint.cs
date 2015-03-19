﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
        // TODO
        // What kinds of constraints do we have?

        // Orientational: The twist around the bones direction vector
        // described by rotor, or maybe an angle around direction vector?
        // like a range. from angle to angle?


    public class OrientationalConstraint
    {
        // An orientational constraint is the twist of the bone around its own direction vector
        // with respect to its parent
        // It is defined as a range betwen angles [right,left]
        private float right;
        public float Right
        {
            get { return right; }
        }
        private float left;
        public float Left
        {
            get { return left; }
        }

        public OrientationalConstraint(float from, float to)
        {
            this.right = from;
            this.left = to;
        }

    }

    public class RotationalConstraint
    {
        private enum Q { q1, q2, q3, q4 };
        private float precision = 0.001f;
        // A constraint modeled as an irregular cone
        // The direction vector is the direction the cone is opening up at
        // The four angles define the shape of the cone
        // angle 0 is the angle to the parent's right
        // angle 1 is the angle to the parent's down
        // angle 2 is the angle to the parent's left
        // angle 3 is the angle to the parent's up
        Func<Vector3> directionMethod;
        Func<Vector3> rightMethod;
        private float right, up, left, down;
        public Vector4 Constraints
        {
            get { return new Vector4(right,up,left,down); }
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
        
        public bool RotationalConstraints(Vector3 target, Vector3 jointPos, Vector3 L1, Vector4 constraints, out Vector3 res)
        {
            Vector3 targetPos = new Vector3(target.X, target.Y, target.Z);
            Vector3 joint2Target = (targetPos - jointPos);

            bool behind = false;
            bool reverseCone = false;
            bool orthogonal = false;

            //3.1 Find the line equation L1
            //Vector3 L1 = jointPos - parentPos;
            //3.2 Find the projection O of the target t on line L1
            Vector3 O = Vector3Helper.Project(joint2Target, L1);
            Vector3 OPos = O + jointPos;
            if (Math.Abs(Vector3.Dot(L1, joint2Target)) < precision)
            {
                orthogonal = true;
                //behind = true;
                O = Vector3.Normalize(L1) * precision * 10;
                OPos = O + jointPos;
            }
            else if (Math.Abs(Vector3.Dot(O, L1) - O.Length * L1.Length) >= precision) // not same direction
            {
                behind = true;
            }

            //3.3 Find the distance between the point O and the joint position
            float S = (OPos - jointPos).Length;

            //3.4 Map the target (rotate and translate) in such a
            //way that O is now located at the axis origin and oriented
            //according to the x and y-axis ) Now it is a 2D simplified problem
            float angle = Vector3.CalculateAngle(L1, Vector3.UnitZ);
            Vector3 axis = Vector3.Cross(L1, Vector3.UnitZ);
            Quaternion rotation = Quaternion.FromAxisAngle(axis, angle);
            Quaternion orgiRot = rotation;
            Vector3 TRotated = Vector3.Transform(joint2Target, rotation);
            Vector2 target2D = new Vector2(TRotated.X, TRotated.Y);

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
            #endregion
            #region check cone
            if (radius.X > 90 && radius.Y > 90) // cone is reversed
            {
                reverseCone = true;
            }
            else if ((behind) && (radius.X > 90 || radius.Y > 90)) // has one angle > 90, other not, very speciall case
            {
                Quaternion inverRot = Quaternion.Invert(rotation);
                Vector3 right = Vector3.Transform(Vector3.UnitX, inverRot);
                Vector3 up = Vector3.Transform(Vector3.UnitY, inverRot);
                Vector3 L2 = right;
                switch (q)
                {
                    case Q.q1:
                        if (radius.X > 90) L2 = right;
                        else L2 = up;
                        break;
                    case Q.q2:
                        if (radius.X > 90) L2 = right;
                        else L2 = -up;
                        break;
                    case Q.q3:
                        if (radius.X > 90) L2 = -right;
                        else L2 = -up;
                        break;
                    case Q.q4:
                        if (radius.X > 90) L2 = -right;
                        else L2 = up;
                        break;
                    default:
                        break;
                }
                angle = Vector3.CalculateAngle(L2, Vector3.UnitZ);

                axis = Vector3.Cross(L2, Vector3.UnitZ);
                rotation = Quaternion.FromAxisAngle(axis, angle);

                TRotated = Vector3.Transform(joint2Target, rotation);
                target2D = new Vector2(TRotated.Xy);
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
            else if (behind && !orthogonal && radius.X <= 90 && radius.Y <= 90) // behind and cone i front
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
            if ((inside && !reverseCone && !behind)
                || (!inside && reverseCone && behind)
                || (inside && reverseCone && !behind)
                || (!inside && reverseCone && behind)
               )
            {
                //3.10 use the true target position t
                res = target;
                return false;
            }
            //3.11 else
            else
            {

                //3.12 Find the nearest point on that conic section
                //from the target
                Vector2 newPoint = NearestPoint(radiusX, radiusY, target2D, q, reverseCone);

                Vector3 newPointV3 = new Vector3(newPoint);

                //3.13 Map (rotate and translate) that point on the
                //conic section via reverse of 3.4 and use that point as
                //the new target position
                rotation = Quaternion.Invert(rotation);
                UnityDebug.CreateEllipse(radiusX, radiusY,  OPos.Convert(), rotation.Convert(), 500, UnityEngine.Color.black);
                Vector3 moveTo = Vector3.Transform(newPointV3, rotation);
                moveTo += OPos;
                Vector3 vectorToMoveTo = (moveTo - jointPos);
                axis = Vector3.Cross(joint2Target, vectorToMoveTo);
                angle = Vector3.CalculateAngle(joint2Target, vectorToMoveTo);
                Quaternion rot = Quaternion.FromAxisAngle(axis, angle);
                res = Vector3.Transform(joint2Target, rot) + jointPos;

                return true;    
            }
            //3.14 end
        }
        private Vector2 NearestPoint(float radiusX, float radiusY, Vector2 target2D, Q q, bool reverseCone)
        {
            Vector2 newPoint;
            float xRad, yRad, pX, pY;

            if (radiusX >= radiusY ^ reverseCone)
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
