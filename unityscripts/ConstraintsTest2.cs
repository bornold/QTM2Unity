using System;
using System.Collections;
using OpenTK;
namespace QTM2Unity
{
    class ConstraintsTest2 : UnityEngine.MonoBehaviour
    {
        public float targetScale = 0.05f;
        public bool printCone = true;
        public bool showRotation = false;
        public bool backwards = false;
        public bool pause = false;
        public int coneResolution = 60;
        public bool spinAroundX = false;
        public bool spinAroundY = false;
        public bool spinAroundZ = false;
        public int spins = 360;
        public UnityEngine.Vector4 Constraints = new UnityEngine.Vector4(110, 20, 30, 40);
        public float twist = 0;

        private UnityEngine.LineRenderer lineRenderer;

        private UnityEngine.GameObject targetGO = new UnityEngine.GameObject();
        private UnityEngine.GameObject nextGO = new UnityEngine.GameObject();
        private UnityEngine.GameObject parentGO = new UnityEngine.GameObject();
        private UnityEngine.GameObject currentGO = new UnityEngine.GameObject();
        private Bone joint = new Bone("Current Joint");

        void Start()
        {
            lineRenderer = GetComponent<UnityEngine.LineRenderer>();
            SetGO(targetGO, "Target", UnityEngine.Vector3.up, UnityEngine.Color.white);
            SetGO(nextGO, "NextJoint", UnityEngine.Vector3.zero, UnityEngine.Color.black);
            SetGO(currentGO, "CurrentJoint", UnityEngine.Vector3.zero, UnityEngine.Color.gray);
            SetGO(parentGO, "ParentJoint", UnityEngine.Vector3.right, UnityEngine.Color.gray);
            joint = new Bone("Current Joint");
        }
        private void SetGO(UnityEngine.GameObject go, string name, UnityEngine.Vector3 pos, UnityEngine.Color c)
        {
            go = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = UnityEngine.Vector3.one * targetScale;
            go.transform.parent = this.gameObject.transform;
            UnityEngine.MeshRenderer gameObjectRenderer = go.GetComponent<UnityEngine.MeshRenderer>();
            UnityEngine.Material newMaterial = new UnityEngine.Material(UnityEngine.Shader.Find("Specular"));
            newMaterial.color = c;
            gameObjectRenderer.material = newMaterial;
            go.SetActive(true);
        }
        private void UpdateGOS(string name, UnityEngine.Vector3 pos)
        {
            UnityEngine.Transform t = transform.Search(name).transform;
            t.position = pos;
            t.localScale = UnityEngine.Vector3.one * targetScale;
        }
        void Update()
        {
            Vector3 nextJointPos = transform.Search("NextJoint").position.Convert();
            Vector3 targ = transform.Search("Target").position.Convert();

            joint = new Bone("Current Joint");
            joint.Constraints = new Vector4(Constraints.x, Constraints.y, Constraints.z, Constraints.w);
            UnityEngine.Transform jointTrans = transform.Search("CurrentJoint");
            joint.Pos = jointTrans.position.Convert();
            Vector3 y = jointTrans.up.Convert();
            joint.Orientation = QuaternionHelper.LookAtUp(joint.Pos, nextJointPos, y);

            Bone propparent = new Bone("PropParent");
            if (backwards)
            {
                propparent.Pos = targ;
                propparent.Orientation = QuaternionHelper.LookAtUp(targ, joint.Pos, y);
            }

            UnityEngine.Transform parentTrans = transform.Search("ParentJoint");
            y = parentTrans.up.Convert();

            Bone parent = new Bone("Parent", parentTrans.position.Convert(), QuaternionHelper.LookAtUp(parentTrans.position.Convert(), joint.Pos, y));
            parent.Rotate(QuaternionHelper.RotationX(MathHelper.DegreesToRadians(twist)));

            if (spinAroundX || spinAroundY || spinAroundZ) targ = UpdateTarget(targ, joint.Pos);


            Vector3 res = joint.Pos;
            Quaternion rot;
            if (backwards && !pause)
            {
                if (Constraint.CheckRotationalConstraints(joint, propparent, nextJointPos, out res, out rot))
                { }
                else { UnityEngine.Debug.Log("inside"); }
                Vector3 test = propparent.Pos - joint.Pos;
                Vector3 testInvert = joint.Pos + Vector3.Transform(test, Quaternion.Invert(rot));
                UnityDebug.DrawLine(joint.Pos, testInvert, UnityEngine.Color.white);
                UpdateGOS("ParentJoint", testInvert.Convert());

            }
            else if (!pause)
            {
                if (Constraint.CheckRotationalConstraints(joint, parent, targ, out res, out rot))
                {  }
                UpdateGOS("NextJoint", res.Convert());
                nextJointPos = res;
            }
            
            //UnityDebug.DrawRays(parentRot, parentPos, 2f);
            UnityDebug.DrawLine(joint.Pos, targ, UnityEngine.Color.cyan);

            if (printCone)
            {
                UnityDebug.CreateIrregularCone3(
                        joint.Constraints,
                        joint.Pos,
                        parent.GetYAxis(),
                        parent.Orientation,
                        coneResolution,
                        (float)(nextJointPos - joint.Pos).Length * 0.8f
                );
            }
            if (backwards)
                {
                    Vector4 constraints = parent.Constraints;
                    MathHelper.Swap(ref constraints.X, ref constraints.Z); MathHelper.Swap(ref constraints.Y, ref constraints.W);

                    UnityDebug.CreateIrregularCone3(
                            joint.Constraints,
                            joint.Pos,
                            propparent.GetYAxis(),
                            propparent.Orientation,
                            coneResolution,
                            (float)(nextJointPos - joint.Pos).Length * 0.8f
                    );
                    //pause = true;
                }
            if (showRotation)
            {
                UnityDebug.DrawRays(joint.Orientation, joint.Pos, (joint.Pos-parent.Pos).Length * 0.5f);
                UnityDebug.DrawRays(parent.Orientation, parent.Pos, (joint.Pos - parent.Pos).Length*0.5f);
            }

            UnityDebug.DrawLine(joint.Pos, parent.Pos, UnityEngine.Color.black);
            UnityDebug.DrawLine(joint.Pos, nextJointPos, UnityEngine.Color.black);
            UnityDebug.DrawLine(joint.Pos, res, UnityEngine.Color.magenta);
            //DrawLine(joint.Pos);
        }
        Vector3 UpdateTarget(Vector3 targetPos, Vector3 jointPos)
        {
            Vector3 joint2Target = (targetPos - jointPos);
            Vector3 spinned = spinIt(joint2Target.X, joint2Target.Y, joint2Target.Z);
            if (!spinAroundX) spinned.X = joint2Target.X;
            if (!spinAroundY) spinned.Y = joint2Target.Y;
            if (!spinAroundZ) spinned.Z = joint2Target.Z;

            return spinned + jointPos;
        }
        int d = 0;
        private Vector3 spinIt(float X, float Y, float Z)
        {
            spins = (spins > 10) ? spins : 10;
            d = d + 1 % spins;
            float angle = (float)d / spins * 2.0f * Mathf.PI;
            return new Vector3(X * Mathf.Cos(angle), Y * Mathf.Sin(angle), Z * Mathf.Cos(angle));
        }
        private void DrawLine(Vector3 test)
        {
            lineRenderer.SetPosition(0, test.Convert());
        }
    }
}
