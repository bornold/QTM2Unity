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
        public UnityEngine.Vector3 Target = new UnityEngine.Vector3(1,2,3);
        public UnityEngine.Vector3 CurrentJoint = new UnityEngine.Vector3(1,1,0);
        public UnityEngine.Vector3 ParentJoint = new UnityEngine.Vector3(-2,-2,-1);
        public UnityEngine.Vector4 Constraints = new UnityEngine.Vector3(10,20,30);
        public float targetScale = 0.15f;
        public int coneResolution = 60;
        private GameObject targetGO = new GameObject();
        private GameObject parentGO = new GameObject();
        private GameObject currentGO = new GameObject();
        private GameObject OGO = new GameObject();

        private enum Q { q1, q2, q3, q4 };
        void Start()
        {
            SetGO(targetGO,"Target",Target);
            SetGO(currentGO,"CurrentJoint",CurrentJoint);
            SetGO(parentGO,"ParentJoint",ParentJoint);
            SetGO(OGO, "O", UnityEngine.Vector3.zero);
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
            t.localScale =  UnityEngine.Vector3.one* targetScale;
        }
        void Update()
        {

            UpdateGOS("Target", Target);
            UpdateGOS("CurrentJoint", CurrentJoint);
            UpdateGOS("ParentJoint", ParentJoint);

            OpenTK.Vector3 jp1 = new OpenTK.Vector3(CurrentJoint.x, CurrentJoint.y, CurrentJoint.z);
            OpenTK.Vector3 jp2 =  new OpenTK.Vector3(ParentJoint.x, ParentJoint.y, ParentJoint.z);
            OpenTK.Vector3 targ = new OpenTK.Vector3(Target.x, Target.y, Target.z);
            OpenTK.Vector4 constr = new OpenTK.Vector4(Constraints.x,Constraints.y,Constraints.z,Constraints.w);

            OpenTK.Vector3 rotConts = RotationalConstraints(targ,jp2,jp1,constr);
        }

        public OpenTK.Vector3 RotationalConstraints(OpenTK.Vector3 targetPos, OpenTK.Vector3 parentPos, OpenTK.Vector3 jointPos, OpenTK.Vector4 constraints)
        {
            //3.1 Find the line equation L1
            OpenTK.Vector3 L1 = jointPos - parentPos;
            UnityDebug.DrawRay(parentPos, L1,Color.white);
            //3.2 Find the projection O of the target t on line L1
            OpenTK.Vector3 joint2Target = (targetPos - jointPos);

            OpenTK.Vector3 O = Vector3Helper.Project(joint2Target, L1);

            UnityEngine.Vector3 O2 = new UnityEngine.Vector3(O.X, O.Y, O.Z) + this.transform.Search("CurrentJoint").localPosition;
            UpdateGOS("O", O2);


            //3.3 Find the distance between the point O and the joint position
            float S = (jointPos - O).Length;

            //3.4 Map the target (rotate and translate) in such a
            //way that O is now located at the axis origin and oriented
            //according to the x and y-axis ) Now it is a 2D simplified problem

            float angle = OpenTK.Vector3.CalculateAngle(O, OpenTK.Vector3.UnitZ);
            OpenTK.Vector3 axis = OpenTK.Vector3.Cross(O, OpenTK.Vector3.UnitZ);
            OpenTK.Quaternion rotation = OpenTK.Quaternion.FromAxisAngle(axis, angle);
            OpenTK.Vector3 TRotated = OpenTK.Vector3.Transform(joint2Target, rotation);
            OpenTK.Vector2 target2D = new OpenTK.Vector2(TRotated.X,TRotated.Y);

            //3.5 Find in which quadrant the target belongs 
            // Locate target in a particular quadrant
            OpenTK.Vector2 radius;
            Q q;
            if (target2D.X >= 0 && target2D.Y >= 0)
            {
                UnityEngine.Debug.Log(string.Format(
                    " x, y quadrant\n Ellipse defined by q1, q2"));
                radius = new OpenTK.Vector2(Constraints.x, Constraints.y);
                q = Q.q1;
            }
            else if (target2D.X < 0 && target2D.Y >= 0)
            {
                q = Q.q2;
                UnityEngine.Debug.Log(string.Format(
               " -x, y quadrant\n Ellipse defined by q2, q3"));
                radius = new OpenTK.Vector2(Constraints.z, Constraints.y);
            }
            else if (target2D.X <= 0 && target2D.Y < 0)
            {
                q = Q.q3;
                UnityEngine.Debug.Log(string.Format(
                " -x,-y quadrant\n Ellipse defined by q3, q0 "));
                radius = new OpenTK.Vector2(Constraints.z, Constraints.w);
            }
            else /*if (target.X > 0 && target.Y < 0)*/
            {
                q = Q.q4;
                UnityEngine.Debug.Log(string.Format(
                " x, -y quadrant\n Ellipse defined by q0, q1"));
                radius = new OpenTK.Vector2(Constraints.x, Constraints.w);
            }
            //3.6 Find what conic section describes the allowed
            //range of motion

            float radiusX = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.X));
            float radiusY = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.Y));
            UnityEngine.Debug.Log(string.Format("radiusX: {0} radiusY: {1}", radiusX, radiusY));
            UnityEngine.Debug.Log(string.Format("target2d: {0}", target2D));

            int resolution = 500;
            OpenTK.Vector3 O3 = new OpenTK.Vector3(O2.x, O2.y, O2.z);
            //UnityDebug.CreateEllipse(radiusX, radiusY, O2, 
               //UnityEngine.Quaternion.FromToRotation(UnityEngine.Vector3.forward,UnityDebug.cv(L1)),
                //resolution);
            UnityDebug.CreateEllipse(radiusX, radiusY, OpenTK.Vector3.Zero,
               UnityEngine.Quaternion.identity,
                resolution);
            UnityDebug.DrawLine(OpenTK.Vector3.Zero, new OpenTK.Vector3(target2D),Color.black);
            //3.7 Find the conic section which is associated with
            //that quadrant using the distances qj = Stanhj, where
            //j = 1,..,4

            //3.8 % Check whether the target is within the conic
            //section or not
            bool inside = (target2D.X * target2D.X) / (radiusX * radiusX) +
                (target2D.Y * target2D.Y) / (radiusY * radiusY) <= 1;

            //3.9 if within the conic section then
            if (inside) // target not inside
            {
                //3.10 use the true target position t
                UnityEngine.Debug.Log(string.Format("Inside!"));
            }
            //3.11 else
            else
            {
                UnityEngine.Debug.Log(string.Format("Outside!"));
                //3.12 Find the nearest point on that conic section
                //from the target
                OpenTK.Vector2 newPoint = QTM2UnityMath.findNearestPointOnEllipse
                     (Math.Max(radiusX, radiusY), Math.Min(radiusX, radiusY),
                     new OpenTK.Vector2(Math.Abs(target2D.X), Math.Abs(target2D.Y)));
                if (radiusX <= radiusY)
                {
                    Debug.Log("radiusX <= radiusY");
                    MathHelper.Swap(ref newPoint.X, ref newPoint.Y);
                }
                else
                {
                    Debug.Log("radiusX > radiusY");
                }

                switch (q)
                {
                    case Q.q1:
                        Debug.Log("Q1");
                        break;
                    case Q.q2:
                        Debug.Log("Q2");
                        newPoint.X = -newPoint.X; 
                        break;
                    case Q.q3:
                        Debug.Log("Q3");
                        newPoint.X = -newPoint.X;
                        newPoint.Y = -newPoint.Y; 
                        break;
                    case Q.q4:
                        Debug.Log("Q4");
                        newPoint.Y = -newPoint.Y; 
                        break;
                    default:
                        break;
                }
                
                
                UnityDebug.DrawLine(new OpenTK.Vector3(target2D), new OpenTK.Vector3(newPoint),Color.red);
                Debug.Log("newPoint:" + newPoint);
                OpenTK.Vector3 newPointV3 = new OpenTK.Vector3(newPoint);
                //3.13 Map (rotate and translate) that point on the
                //conic section via reverse of 3.4 and use that point as
                //the new target position
                rotation.Conjugate();

                OpenTK.Vector3 moveTo = OpenTK.Vector3.Transform(newPointV3, rotation);
                moveTo += jointPos + O;
                Debug.Log("movetoRotated:" + moveTo);
                Debug.DrawLine(
                   Target,
                    UnityDebug.cv(moveTo),
                    Color.black);
            }

            UnityDebug.CreateIrregularCone(constraints, jointPos, jointPos + O, rotation, coneResolution, Color.yellow);
            //3.14 end
            return OpenTK.Vector3.Zero;
        }
        private OpenTK.Vector2 FindAngles(OpenTK.Vector4 rc, OpenTK.Vector2 target)
        {
            // Locate target in a particular quadrant
            if (target.X >= 0 && target.Y >= 0)
            {
                UnityEngine.Debug.Log(string.Format(
                    " x, y quadrant\n Ellipse defined by q1, q2"));
                return new OpenTK.Vector2(rc.X, rc.Y);

            }
            else if (target.X < 0 && target.Y >= 0)
            {
                UnityEngine.Debug.Log(string.Format(
               " -x, y quadrant\n Ellipse defined by q2, q3"));
                return new OpenTK.Vector2(rc.Y, rc.Z);
            }
            else if (target.X <= 0 && target.Y < 0)
            {
                UnityEngine.Debug.Log(string.Format(
                " -x,-y quadrant\n Ellipse defined by q3, q0 "));
                return new OpenTK.Vector2(rc.Z, rc.W);
            }
            else /*if (target.X > 0 && target.Y < 0)*/
            {
                UnityEngine.Debug.Log(string.Format(
                " x, -y quadrant\n Ellipse defined by q0, q1"));
                return new OpenTK.Vector2(rc.W, rc.X);
            }
        }
    }
}


