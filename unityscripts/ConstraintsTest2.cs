using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using UnityEngine;
namespace QTM2Unity
{
    class ConstraintsTest2 : MonoBehaviour
    {
        public bool debug = false;
        public bool spin = false;
        public UnityEngine.Vector3 Target = new UnityEngine.Vector3(1,2,3);
        public UnityEngine.Vector3 CurrentJoint = new UnityEngine.Vector3(1,1,0);
        public UnityEngine.Vector3 ParentJoint = new UnityEngine.Vector3(-2,-2,-1);
        public UnityEngine.Vector4 Constraints = new UnityEngine.Vector3(10,20,30);
        public float targetScale = 0.15f;
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
            if (spin)
            {
                targ = UpdateTarget(targ, parentPos, jointPos);
            }
            UpdateGOS("Target", UnityDebug.cv(targ));

            OpenTK.Vector3 rotConts = RotationalConstraints(targ, parentPos, jointPos, constr);
        }
        OpenTK.Vector3 UpdateTarget(OpenTK.Vector3 targetPos, OpenTK.Vector3 parentPos, OpenTK.Vector3 jointPos)
        {
            OpenTK.Vector3 L1 = jointPos - parentPos;
            OpenTK.Vector3 joint2Target = (targetPos - jointPos);
            float angle = OpenTK.Vector3.CalculateAngle(L1, OpenTK.Vector3.UnitZ);
            OpenTK.Vector3 axis = OpenTK.Vector3.Cross(L1, OpenTK.Vector3.UnitZ);
            OpenTK.Quaternion rotation = OpenTK.Quaternion.FromAxisAngle(axis, angle);
            OpenTK.Vector3 TRotated = OpenTK.Vector3.Transform(joint2Target, rotation);
            OpenTK.Vector2 target2D = new OpenTK.Vector2(TRotated.X, TRotated.Y);
            target2D = spinIt(target2D);
            return OpenTK.Vector3.Transform(new OpenTK.Vector3(target2D.X, target2D.Y, targetPos.Z), OpenTK.Quaternion.Invert(rotation));
        }

        public OpenTK.Vector3 RotationalConstraints(OpenTK.Vector3 targetPos, OpenTK.Vector3 parentPos, OpenTK.Vector3 jointPos, OpenTK.Vector4 constraints)
        {
            //3.1 Find the line equation L1
            OpenTK.Vector3 L1 = jointPos - parentPos;
            UnityDebug.DrawRay(parentPos, L1,Color.gray);
            //3.2 Find the projection O of the target t on line L1
            OpenTK.Vector3 joint2Target = (targetPos - jointPos);
            OpenTK.Vector3 O = Vector3Helper.Project(joint2Target, L1);
            if (OpenTK.Vector3.Dot(O, L1) == 0)
            {
                Debug.Log("Target at 90 degrees angle ");
            }
            if (OpenTK.Vector3.Dot(O, L1) == O.Length * L1.Length) // not same direction
            {
                if (debug) Debug.Log("Target behind");
               //O = -O;
            }

            UnityEngine.Vector3 O2 = new UnityEngine.Vector3(O.X, O.Y, O.Z) + this.transform.Search("CurrentJoint").localPosition;


            //3.3 Find the distance between the point O and the joint position
            float S = (jointPos - O).Length;
            if (debug) UnityEngine.Debug.Log(string.Format("S: {0}", S)); // what if s = 0??;

            //3.4 Map the target (rotate and translate) in such a
            //way that O is now located at the axis origin and oriented
            //according to the x and y-axis ) Now it is a 2D simplified problem

            float angle = OpenTK.Vector3.CalculateAngle(O, OpenTK.Vector3.UnitZ);
//            UnityEngine.Debug.Log(string.Format("angle: {0}", angle));

            OpenTK.Vector3 axis = OpenTK.Vector3.Cross(O, OpenTK.Vector3.UnitZ);
//            UnityEngine.Debug.Log(string.Format("axis: {0}", axis));
            OpenTK.Quaternion rotation = OpenTK.Quaternion.FromAxisAngle(axis, angle);
//            UnityEngine.Debug.Log(string.Format("joint2Target: {0}", joint2Target));
            UnityDebug.CreateIrregularCone(constraints, jointPos, O, OpenTK.Quaternion.Invert(rotation), coneResolution);

            OpenTK.Vector3 TRotated = OpenTK.Vector3.Transform(joint2Target, rotation);
            OpenTK.Vector2 target2D = new OpenTK.Vector2(TRotated.X,TRotated.Y);
            if (debug) UnityEngine.Debug.Log(string.Format("target2d: {0}", target2D));

            //3.5 Find in which quadrant the target belongs 
            // Locate target in a particular quadrant
            OpenTK.Vector2 radius;
            Q q;
            if (target2D.X >= 0 && target2D.Y >= 0)
            {
               if(debug) UnityEngine.Debug.Log(string.Format(" x, y Q1 quadrant\n Ellipse defined by X Y"));
                radius = new OpenTK.Vector2(constraints.X, constraints.Y);
                q = Q.q1;
            }
            else if (target2D.X >= 0 && target2D.Y < 0)
            {
                q = Q.q2;
                if (debug) UnityEngine.Debug.Log(string.Format(" x, -y Q2 quadrant\n Ellipse defined by Y Z"));
                radius = new OpenTK.Vector2(constraints.X, constraints.W);
            }
            else if (target2D.X < 0 && target2D.Y < 0)
            {
                q = Q.q3;
                if (debug) UnityEngine.Debug.Log(string.Format(" -x,-y Q3 quadrant\n Ellipse defined by Z W"));
                radius = new OpenTK.Vector2(constraints.Z, constraints.W);
            }
            else /*if (target.X > 0 && target.Y < 0)*/
            {
                q = Q.q4;
                if (debug) UnityEngine.Debug.Log(string.Format(" x, -y Q4 quadrant\n Ellipse defined by Z Y "));
                radius = new OpenTK.Vector2(constraints.Z, constraints.Y);
            }
            //3.6 Find what conic section describes the allowed
            //range of motion

            //3.7 Find the conic section which is associated with
            //that quadrant using the distances qj = Stanhj, where
            //j = 1,..,4
            if (debug) UnityEngine.Debug.Log(string.Format("radius.X: {0} radius.Y: {1}", radius.X, radius.Y));
            float radiusX = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.X));
            float radiusY = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.Y));
            if (debug) UnityEngine.Debug.Log(string.Format("radiusX: {0} radiusY: {1}", radiusX, radiusY));

            UnityDebug.CreateEllipse(radiusX, radiusY, OpenTK.Vector3.Zero,
               UnityEngine.Quaternion.identity,
                coneResolution,Color.cyan);

            //3.8 % Check whether the target is within the conic
            //section or not
            bool inside = (target2D.X * target2D.X) / (radiusX * radiusX) +
                (target2D.Y * target2D.Y) / (radiusY * radiusY) <= 1;


            OpenTK.Vector3 resultPos;

            //3.9 if within the conic section then
            if (inside) // target not inside
            {
                //3.10 use the true target position t
                if (debug) UnityEngine.Debug.Log(string.Format("Inside!"));
                resultPos = targetPos;
            }
            //3.11 else
            else
            {
                //3.12 Find the nearest point on that conic section
                //from the target
                    float eMax, eMin, x, y;
                if (radiusX >= radiusY)
                {
                    eMax = radiusX;
                    eMin = radiusY;
                    x = Math.Abs(target2D.X);
                    y = Math.Abs(target2D.Y);
                }
                else
                {
                    eMax = radiusY;
                    eMin = radiusX;
                    x = Math.Abs(target2D.Y);
                    y = Math.Abs(target2D.X);
                }
               // UnityDebug.CreateEllipse(eMax, eMin, OpenTK.Vector3.Zero, OpenTK.Quaternion.Identity, coneResolution,Color.green);

                OpenTK.Vector2 newPoint = 
                    QTM2UnityMath.findNearestPointOnEllipse(eMax, eMin, new OpenTK.Vector2(x, y));
                if (radiusX < radiusY)
                {
                    //Debug.Log("radiusX < radiusY");
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
                UnityDebug.DrawLine(new OpenTK.Vector3(target2D), new OpenTK.Vector3(newPoint), Color.white);
                if (debug) Debug.Log("newPoint:" + newPoint);
                OpenTK.Vector3 newPointV3 = new OpenTK.Vector3(newPoint);
                //3.13 Map (rotate and translate) that point on the
                //conic section via reverse of 3.4 and use that point as
                //the new target position
                rotation = OpenTK.Quaternion.Invert(rotation);

                OpenTK.Vector3 moveTo = OpenTK.Vector3.Transform(newPointV3, rotation);
                moveTo += jointPos + O;
                if (debug) Debug.Log("movetoRotated:" + moveTo);
                if (debug) UnityEngine.Debug.Log(string.Format("Outside!"));
                
                UnityDebug.DrawLine(targetPos, moveTo, Color.white);
                UnityDebug.DrawLine(jointPos, jointPos+joint2Target, Color.white);

                OpenTK.Vector3 vectorToMoveTo = (moveTo - jointPos);

                axis = OpenTK.Vector3.Cross(joint2Target, vectorToMoveTo);

                angle = OpenTK.Vector3.CalculateAngle(joint2Target, vectorToMoveTo);

                OpenTK.Quaternion rot = OpenTK.Quaternion.FromAxisAngle(axis, angle);

                resultPos = OpenTK.Vector3.Transform(jointPos + joint2Target, rot);
            }

            UpdateGOS("Target", UnityDebug.cv(targetPos));
            UpdateGOS("Replaced", UnityDebug.cv(resultPos));
            UnityDebug.DrawLine(jointPos, resultPos, Color.black);
            //3.14 end
            return resultPos;
        }
        int d = 0;
        private OpenTK.Vector2 spinIt(OpenTK.Vector2 target2D)
        {
            d = d + 1 % spins;
            float angle = (float)d / (float)spins * 2.0f * Mathf.PI;
            return new OpenTK.Vector2(target2D.X * Mathf.Cos(angle), target2D.Y * Mathf.Sin(angle));
        }

    }
}


