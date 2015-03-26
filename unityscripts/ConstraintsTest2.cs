using System;
using OpenTK;
namespace QTM2Unity
{
    class ConstraintsTest2 : UnityEngine.MonoBehaviour
    {
        public float targetScale = 0.05f;
        public bool printCone = true;
        public int coneResolution = 60;
        public bool spinAroundX = false;
        public bool spinAroundY = false;
        public bool spinAroundZ = false;
        public int spins = 360;
        public UnityEngine.Vector4 Constraints = new UnityEngine.Vector4(110, 20, 30, 40);
        public float twist = 0;
        
        private UnityEngine.GameObject targetGO = new UnityEngine.GameObject();
        private UnityEngine.GameObject replacedGO = new UnityEngine.GameObject();
        private UnityEngine.GameObject parentGO = new UnityEngine.GameObject();
        private UnityEngine.GameObject currentGO = new UnityEngine.GameObject();
        private Bone joint = new Bone("Current Joint");

        private enum Q { q1, q2, q3, q4 };
        void Start()
        {
            SetGO(targetGO, "Target", UnityEngine.Vector3.up, UnityEngine.Color.white);
            SetGO(replacedGO, "Replaced", UnityEngine.Vector3.zero, UnityEngine.Color.black);
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
            UnityEngine.Material newMaterial = new UnityEngine.Material(UnityEngine.Shader.Find("Transparent/Diffuse"));
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
            OpenTK.Vector3 L1;


            Vector3 parentPos;
            Vector3 targ;
            joint = new Bone("Current Joint");
            joint.Constraints = new Vector4(Constraints.x, Constraints.y, Constraints.z, Constraints.w);

            targ = transform.Search("Target").position.Convert();
            parentPos = transform.Search("ParentJoint").position.Convert();
            joint.Pos = transform.Search("CurrentJoint").position.Convert();

            L1 = joint.Pos - parentPos;
            float angle = Vector3.CalculateAngle(L1, Vector3.UnitY);
            Vector3 axis = Vector3.Cross(L1, Vector3.UnitY);
            OpenTK.Quaternion rot = Quaternion.FromAxisAngle(axis, angle);
            rot = OpenTK.Quaternion.Invert(rot);
            OpenTK.Quaternion rot2 = Quaternion.FromAxisAngle(L1, MathHelper.DegreesToRadians(twist));
            rot = rot2 * rot;
            joint.Orientation = rot;

            if (spinAroundX || spinAroundY || spinAroundZ) targ = UpdateTarget(targ, joint.Pos);
            L1 = joint.Pos - parentPos;
            Vector3 res;

            //OpenTK.Quaternion rot = transform.Search("CurrentJoint").rotation.Convert();
            
            UnityDebug.DrawLine(parentPos, joint.Pos, UnityEngine.Color.white);
            UnityDebug.DrawRay(joint.Pos, L1, UnityEngine.Color.black);
            UnityDebug.DrawLine(joint.Pos, targ, UnityEngine.Color.magenta);
            UnityDebug.DrawRays(joint.Orientation, joint.Pos, 2f);
            if (printCone)
            {
                UnityDebug.CreateIrregularCone3(
                        joint.Constraints,
                        joint.Pos,
                        L1,
                        joint.Orientation,
                        coneResolution,
                        (float)(targ - joint.Pos).Length
                        );
            }
            if (Constraint.CheckRotationalConstraints(joint, targ, L1, out res)) targ = res;

            UnityDebug.DrawLine(joint.Pos, res, UnityEngine.Color.cyan);
            UpdateGOS("Replaced", UnityDebug.cv(targ));
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
    }
}


