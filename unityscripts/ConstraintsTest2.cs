using System;
using OpenTK;
using UnityEngine;
namespace QTM2Unity
{
    class ConstraintsTest2 : MonoBehaviour
    {
        public bool printCone = true;
        public bool debug = false;
        public bool debug2 = false;
        public bool spin = false;
        public bool spinAroundX = false;
        public bool spinAroundY = false;
        public bool spinAroundZ = false;

        public UnityEngine.Vector3 Target = new UnityEngine.Vector3(1,2,3);
        public UnityEngine.Vector3 CurrentJoint = new UnityEngine.Vector3(1,1,0);
        public UnityEngine.Vector3 ParentJoint = new UnityEngine.Vector3(-2,-2,-1);
        public UnityEngine.Vector4 Constraints = new UnityEngine.Vector4(110,20,30,40);
        public float targetScale = 0.05f;
        public float precision = 0.001f;
        public int coneResolution = 60;
        public int spins = 360;
        private GameObject targetGO = new GameObject();
        private GameObject replacedGO = new GameObject();
        private GameObject parentGO = new GameObject();
        private GameObject currentGO = new GameObject();

        private enum Q { q1, q2, q3, q4 };
        void Start()
        {
            SetGO(targetGO,"Target",Target);
            SetGO(replacedGO, "Replaced", Target);
            SetGO(currentGO,"CurrentJoint",CurrentJoint);
            SetGO(parentGO,"ParentJoint",ParentJoint);

        }
        private void SetGO(GameObject go, string name, UnityEngine.Vector3 pos )
        {
            go = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = UnityEngine.Vector3.one * targetScale;
            go.transform.parent = this.gameObject.transform;
            go.SetActive(true);
        }
        private void UpdateGOS(string name, UnityEngine.Vector3 pos)
        {
            Transform t = this.gameObject.transform.Search(name).transform;
            t.position = pos;
            t.localScale =  UnityEngine.Vector3.one * targetScale;
        }
        void Update()
        {
            UpdateGOS("CurrentJoint", CurrentJoint);
            UpdateGOS("ParentJoint", ParentJoint);
            OpenTK.Vector3 jointPos = new OpenTK.Vector3(CurrentJoint.x, CurrentJoint.y, CurrentJoint.z);
            OpenTK.Vector3 parentPos =  new OpenTK.Vector3(ParentJoint.x, ParentJoint.y, ParentJoint.z);
            OpenTK.Vector3 targ = new OpenTK.Vector3(Target.x, Target.y, Target.z);
            OpenTK.Vector4 constr = new OpenTK.Vector4(Constraints.x,Constraints.y,Constraints.z,Constraints.w);
            if (spin) targ = UpdateTarget(targ, parentPos, jointPos);
            UpdateGOS("Target", UnityDebug.cv(targ));
            OpenTK.Vector3 newPos = RotationalConstraints(targ, parentPos, jointPos, constr);
            UpdateGOS("Replaced", UnityDebug.cv(newPos));
        }
        OpenTK.Vector3 UpdateTarget(OpenTK.Vector3 targetPos, OpenTK.Vector3 parentPos, OpenTK.Vector3 jointPos)
        {
            OpenTK.Vector3 L1 = jointPos - parentPos;
            OpenTK.Vector3 joint2Target = (targetPos - jointPos);
            float angle = OpenTK.Vector3.CalculateAngle(L1, OpenTK.Vector3.UnitZ);
            OpenTK.Vector3 axis = OpenTK.Vector3.Cross(L1, OpenTK.Vector3.UnitZ);
            OpenTK.Quaternion rotation = OpenTK.Quaternion.FromAxisAngle(axis, angle);
            OpenTK.Vector3 TRotated = OpenTK.Vector3.Transform(joint2Target, rotation);

            OpenTK.Vector3 spinned = spinIt(TRotated.X, TRotated.Y, TRotated.Z);
            if (!spinAroundX) spinned.X = TRotated.X;
            if (!spinAroundY) spinned.Y = TRotated.Y;
            if (!spinAroundZ) spinned.Z = TRotated.Z;

            return OpenTK.Vector3.Transform(spinned, OpenTK.Quaternion.Invert(rotation)) + jointPos;
        }
        int d = 0;
        private OpenTK.Vector3 spinIt(float X, float Y, float Z)
        {
            spins = (spins > 10) ? spins : 10;
            d = d + 1 % spins;
            float angle = (float) d / spins * 2.0f * Mathf.PI;
            return new OpenTK.Vector3(X * Mathf.Cos(angle), Y * Mathf.Sin(angle), Z * Mathf.Sin(angle));
        }
        public OpenTK.Vector3 RotationalConstraints(OpenTK.Vector3 targetPos, OpenTK.Vector3 parentPos, OpenTK.Vector3 jointPos, OpenTK.Vector4 constraints)
        {
            UnityDebug.DrawLine(jointPos, targetPos, Color.white);
            OpenTK.Vector3 joint2Target = (targetPos - jointPos);

            bool behind = false;
            bool reverseCone = false;
            bool wierdCone = false;
            bool orthogonal = false;
            
            //3.1 Find the line equation L1
            OpenTK.Vector3 L1 = jointPos - parentPos;
            UnityDebug.DrawRay(parentPos, L1, Color.black);

            if (Math.Abs(OpenTK.Vector3.Dot(L1, joint2Target)) < precision)
            {
                if (debug2) Debug.Log("Target and Joint are orthogonal ");
                orthogonal = true;
                jointPos += L1 * precision;
                //L1 = jointPos - parentPos;
            }
            //3.2 Find the projection O of the target t on line L1
            OpenTK.Vector3 O = Vector3Helper.Project(joint2Target, L1);
            OpenTK.Vector3 OPos = O + jointPos; 
            
            if (Math.Abs(OpenTK.Vector3.Dot(O, L1) - O.Length * L1.Length) >= precision) // not same direction
            {
                if (debug2) Debug.Log("BEHIND");
                behind = true;
            }
            else
            {
                if (debug2) Debug.Log("INFRONT  ");
            }

            //3.3 Find the distance between the point O and the joint position
            float S = (jointPos - OPos).Length;
            //3.4 Map the target (rotate and translate) in such a
            //way that O is now located at the axis origin and oriented
            //according to the x and y-axis ) Now it is a 2D simplified problem
            float angle = OpenTK.Vector3.CalculateAngle(L1, OpenTK.Vector3.UnitZ);

            OpenTK.Vector3 axis = OpenTK.Vector3.Cross(L1, OpenTK.Vector3.UnitZ);
            OpenTK.Quaternion rotation = OpenTK.Quaternion.FromAxisAngle(axis, angle);
            OpenTK.Vector3 TRotated = OpenTK.Vector3.Transform(joint2Target, rotation);
            OpenTK.Vector2 target2D = new OpenTK.Vector2(TRotated.X,TRotated.Y);
            if (debug2) UnityEngine.Debug.Log(string.Format("target2d: {0}", target2D));
            //3.5 Find in which quadrant the target belongs 
            // Locate target in a particular quadrant
            OpenTK.Vector2 radius;
            Q q;
            bool bug = debug2;
            if (target2D.X >= 0 && target2D.Y >= 0)
            {
               if(bug) UnityEngine.Debug.Log(string.Format(" x, y Q1 quadrant\n Ellipse defined by X Y"));
                radius = new OpenTK.Vector2(constraints.X, constraints.Y);
                q = Q.q1;
            }
            else if (target2D.X >= 0 && target2D.Y < 0)
            {
                q = Q.q2;
                if (bug) UnityEngine.Debug.Log(string.Format(" x, -y Q2 quadrant\n Ellipse defined by Y Z"));
                radius = new OpenTK.Vector2(constraints.X, constraints.W);
            }
            else if (target2D.X < 0 && target2D.Y < 0)
            {
                q = Q.q3;
                if (bug) UnityEngine.Debug.Log(string.Format(" -x,-y Q3 quadrant\n Ellipse defined by Z W"));
                radius = new OpenTK.Vector2(constraints.Z, constraints.W);
            }
            else /*if (target.X > 0 && target.Y < 0)*/
            {
                q = Q.q4;
                if (bug) UnityEngine.Debug.Log(string.Format(" x, -y Q4 quadrant\n Ellipse defined by Z Y "));
                radius = new OpenTK.Vector2(constraints.Z, constraints.Y);
            }

            if (radius.X > 90 && radius.Y > 90)
            {
                reverseCone = true;
            }
            else if (behind && (radius.X > 90 || radius.Y > 90))
            {
                if (debug) UnityEngine.Debug.Log(string.Format("Wierd Cone Detected!"));
                wierdCone = true;
                OpenTK.Quaternion inverRot = OpenTK.Quaternion.Invert(rotation);
                OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, inverRot);
                OpenTK.Vector3 up = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, inverRot);
                OpenTK.Vector3 L2 = right;
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

                angle = OpenTK.Vector3.CalculateAngle(L2, OpenTK.Vector3.UnitZ);

                axis = OpenTK.Vector3.Cross(L2, OpenTK.Vector3.UnitZ);
                rotation = OpenTK.Quaternion.FromAxisAngle(axis, angle);

                TRotated = OpenTK.Vector3.Transform(joint2Target, rotation);
                target2D = new OpenTK.Vector2(TRotated.Xy);
                O = Vector3Helper.Project(joint2Target, L2);

                if (debug) UnityEngine.Debug.Log(string.Format("OT: {0}", O));
                OPos = O + jointPos;
                if (debug) UnityEngine.Debug.Log(string.Format("OTPos: {0}", OPos));
                S = (jointPos - OPos).Length;
                if (debug) UnityEngine.Debug.Log(string.Format("ST: {0}", S));

                float testX = radius.X;
                float testY = radius.Y;
                if (testX > 90)
                {
                    radius.X = (testX - 90);
                }
                else
                {
                    radius.Y = (testY - 90);
                }
            }            
            else if (behind && radius.X <= 90 && radius.Y <= 90)
            {
                O = -O;
                OPos = O + jointPos;
            }
            if (!behind)// || (behind && wierdCone))  // not behind
            {
                // if 90, Tan gets to big, not good, clamp it to 89
               radius.X = Math.Min(90 - precision, radius.X); // clamp it so if 90 -> 89.999, 
               radius.Y = Math.Min(90 - precision, radius.Y);
            }
            radius.X = Math.Max(precision, radius.X); // clamp it so if 0 -> 0.001, 
            radius.Y = Math.Max(precision, radius.Y);


            if (printCone)
            {
                if (reverseCone)
                {
                    OpenTK.Vector4 constraitnsCopy = new OpenTK.Vector4(
                       (constraints.X > 90) ? 90 - (constraints.X - 90) : constraints.X,
                       (constraints.Y > 90) ? 90 - (constraints.Y - 90) : constraints.Y,
                       (constraints.Z > 90) ? 90 - (constraints.Z - 90) : constraints.Z,
                       (constraints.W > 90) ? 90 - (constraints.W - 90) : constraints.W);
                    OpenTK.Vector3 oc = O;
                    if (!behind)oc = -O;
                    UnityDebug.CreateIrregularCone(constraitnsCopy, jointPos, oc, OpenTK.Quaternion.Invert(rotation), coneResolution);
                }
                else if (wierdCone)
                {
                    OpenTK.Vector4 constraitnsCopy = new OpenTK.Vector4(
                       (constraints.X > 90) ? (constraints.X - 90) : constraints.X,
                       (constraints.Y > 90) ? (constraints.Y - 90) : constraints.Y,
                       (constraints.Z > 90) ? (constraints.Z - 90) : constraints.Z,
                       (constraints.W > 90) ? (constraints.W - 90) : constraints.W);
                    OpenTK.Vector3 oc = O;
                    UnityDebug.CreateIrregularCone(constraitnsCopy, jointPos, oc, OpenTK.Quaternion.Invert(rotation), coneResolution);
                }
                else
                {
                    UnityDebug.CreateIrregularCone(constraints, jointPos, O, OpenTK.Quaternion.Invert(rotation), coneResolution);
                }
            }

            //else

            //3.6 Find what conic section describes the allowed
            //range of motion

            //3.7 Find the conic section which is associated with
            //that quadrant using the distances qj = Stanhj, where
            //j = 1,..,4
            if (debug) UnityEngine.Debug.Log(string.Format("DEFREES radius.X: {0} radius.Y: {1}", radius.X, radius.Y));
            if (S < precision) S = precision;
            if (debug) UnityEngine.Debug.Log(string.Format("S: {0}", S));
            float radiusX = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.X));
            float radiusY = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.Y));
            if (debug) UnityEngine.Debug.Log(string.Format("UNITS radiusX: {0} radiusY: {1}", radiusX, radiusY));
            UnityDebug.CreateEllipse(radiusX, radiusY, OpenTK.Vector3.Zero,
               UnityEngine.Quaternion.identity,
                coneResolution,Color.cyan);

            //3.8 % Check whether the target is within the conic
            //section or not
            bool inside = (target2D.X * target2D.X) / (radiusX * radiusX) +
                (target2D.Y * target2D.Y) / (radiusY * radiusY) <= 1 + precision;

            OpenTK.Vector3 resultPos;

            if (debug2) UnityEngine.Debug.Log(
                string.Format(
                    "Inside:{0} behind: {2} reverseCone:{1} WeirdCone: {3}",
                    inside, reverseCone, behind , wierdCone));
            //3.9 if within the conic section then
            if (
                (inside && !reverseCone && !behind) || 
                (!inside && reverseCone && behind) ||
                (inside && reverseCone && !behind) ||
                (!inside && reverseCone && behind)
               )
            {

                //3.10 use the true target position t
                if (debug2) UnityEngine.Debug.Log(string.Format("Inside!"));
                resultPos = targetPos;
            }
            //3.11 else
            else
            {   
                if (debug2) UnityEngine.Debug.Log(string.Format("Outside!"));

                //3.12 Find the nearest point on that conic section
                //from the target
                OpenTK.Vector2 newPoint;
                if (debug) UnityEngine.Debug.Log(string.Format(
                            "radiusX: {0} radiusY: {1} target2D.X: {2} target2D.Y: {3}", 
                            radiusX, radiusY, Math.Abs(target2D.X), Math.Abs(target2D.Y)));
    
                newPoint = NearestPoint(radiusX, radiusY, target2D, q);
                UnityDebug.DrawLine(new OpenTK.Vector3(target2D), new OpenTK.Vector3(newPoint), Color.magenta);
                if (debug) Debug.Log("newPoint:" + newPoint);
                OpenTK.Vector3 newPointV3 = new OpenTK.Vector3(newPoint);
                //3.13 Map (rotate and translate) that point on the
                //conic section via reverse of 3.4 and use that point as
                //the new target position
                rotation = OpenTK.Quaternion.Invert(rotation);

                OpenTK.Vector3 moveTo = OpenTK.Vector3.Transform(newPointV3, rotation);
                moveTo += OPos;
                //moveTo = (behind)
                if (debug) Debug.Log("movetoRotated:" + moveTo);
                
                if (debug) UnityDebug.DrawLine(targetPos, moveTo, Color.white);

                OpenTK.Vector3 vectorToMoveTo = (moveTo - jointPos);

                axis = OpenTK.Vector3.Cross(joint2Target, vectorToMoveTo);

                angle = OpenTK.Vector3.CalculateAngle(joint2Target, vectorToMoveTo);

                OpenTK.Quaternion rot = OpenTK.Quaternion.FromAxisAngle(axis, angle);

                resultPos = OpenTK.Vector3.Transform(joint2Target, rot) + jointPos;
            }


            UnityDebug.DrawLine(jointPos, resultPos, Color.black);
            //3.14 end
            return resultPos;
        }
        private OpenTK.Vector2 NearestPoint(float radiusX, float radiusY, OpenTK.Vector2 target2D, Q q)
        {
            OpenTK.Vector2 newPoint;
            if (radiusX >= radiusY)
            {
                newPoint = QTM2UnityMath.findNearestPointOnEllipse(
                    Math.Abs(radiusX),
                    Math.Abs(radiusY),
                    new OpenTK.Vector2(
                        Math.Abs(target2D.X),
                        Math.Abs(target2D.Y)));
            }
            else
            {
                newPoint = QTM2UnityMath.findNearestPointOnEllipse(
                    Math.Abs(radiusY),
                    Math.Abs(radiusX),
                    new OpenTK.Vector2(
                        Math.Abs(target2D.Y),
                        Math.Abs(target2D.X)));
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


