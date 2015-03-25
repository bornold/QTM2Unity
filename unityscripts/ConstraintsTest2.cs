using System;
using OpenTK;
namespace QTM2Unity
{
    class ConstraintsTest2 : UnityEngine.MonoBehaviour
    {
        public bool chooseTarget = false;
        public bool printCone = true;
        public bool debug = false;
        public bool spin = false;
        public bool spinAroundX = false;
        public bool spinAroundY = false;
        public bool spinAroundZ = false;

        public UnityEngine.Vector3 Target = new UnityEngine.Vector3(1,2,3);
        public UnityEngine.Vector3 CurrentJoint = new UnityEngine.Vector3(1,1,0);
        public UnityEngine.Vector3 ParentJoint = new UnityEngine.Vector3(-2,-2,-1);
        public UnityEngine.Vector4 Constraints = new UnityEngine.Vector4(110,20,30,40);
        public float targetScale = 0.05f;
        public float coneScale = 1f;
        public float precision = 0.001f;
        public int coneResolution = 60;
        public int spins = 360;
        private UnityEngine.GameObject targetGO = new UnityEngine.GameObject();
        private UnityEngine.GameObject replacedGO = new UnityEngine.GameObject();
        private UnityEngine.GameObject parentGO = new UnityEngine.GameObject();
        private UnityEngine.GameObject currentGO = new UnityEngine.GameObject();

        private enum Q { q1, q2, q3, q4 };
        void Start()
        {
            SetGO(targetGO,"Target",Target, UnityEngine.Color.white);
            SetGO(replacedGO, "Replaced", Target, UnityEngine.Color.black);
            SetGO(currentGO, "CurrentJoint", CurrentJoint, UnityEngine.Color.gray);
            SetGO(parentGO,"ParentJoint",ParentJoint,UnityEngine.Color.gray);

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
            UnityEngine.Transform t = this.gameObject.transform.Search(name).transform;
            t.position = pos;
            t.localScale =  UnityEngine.Vector3.one * targetScale;
        }
        void Update()
        {
            Vector3 jointPos;
            Vector3 parentPos;
            Vector3 targ;
            Vector4 constr = new Vector4(Constraints.x, Constraints.y, Constraints.z, Constraints.w);
            if (!chooseTarget)
            {
                jointPos = new Vector3(CurrentJoint.x, CurrentJoint.y, CurrentJoint.z);
                parentPos = new Vector3(ParentJoint.x, ParentJoint.y, ParentJoint.z);
                targ = new Vector3(Target.x, Target.y, Target.z);
                UpdateGOS("Target", UnityDebug.cv(targ));
                UpdateGOS("CurrentJoint", CurrentJoint);
                UpdateGOS("ParentJoint", ParentJoint);
            }
            else
            {
                targ = transform.Search("Target").position.Convert();
                jointPos = transform.Search("CurrentJoint").position.Convert();
                parentPos = transform.Search("ParentJoint").position.Convert();
            }   
            if (spin) targ = UpdateTarget(targ, parentPos, jointPos);
            
            Vector3 res;
            OpenTK.Vector3 L1 = jointPos - parentPos;
            float angle = Vector3.CalculateAngle(L1, Vector3.UnitY);
            Vector3 axis = Vector3.Cross(L1, Vector3.UnitY);
            OpenTK.Quaternion rot = Quaternion.FromAxisAngle(axis, angle);
            rot = OpenTK.Quaternion.Invert(rot);
            if (debug)
            {
                UnityDebug.DrawLine(parentPos, jointPos, UnityEngine.Color.white);
                UnityDebug.DrawRay(jointPos, L1, UnityEngine.Color.black);
                UnityDebug.DrawLine(jointPos, targ, UnityEngine.Color.magenta);
                UnityDebug.DrawRays(rot, jointPos, coneScale*2);
            }
            if (printCone)
            {
                UnityDebug.CreateIrregularCone3(
                        constr,
                        jointPos,
                        rot,
                        coneResolution,
                        (float)(targ - jointPos).Length
                        );
            }
            if (Constraint.CheckRotationalConstraints(targ, jointPos, L1, constr, out res)) targ = res;
            if (debug)
            {
                UnityDebug.DrawLine(jointPos, res, UnityEngine.Color.cyan);
            }

            UpdateGOS("Replaced", UnityDebug.cv(targ));
        }
        Vector3 UpdateTarget(Vector3 targetPos, Vector3 parentPos, Vector3 jointPos)
        {
            Vector3 L1 = jointPos - parentPos;
            Vector3 joint2Target = (targetPos - jointPos);
            float angle = Vector3.CalculateAngle(L1, Vector3.UnitZ);
            Vector3 axis = Vector3.Cross(L1, Vector3.UnitZ);
            Quaternion rotation = Quaternion.FromAxisAngle(axis, angle);
            Vector3 TRotated = Vector3.Transform(joint2Target, rotation);

            Vector3 spinned = spinIt(TRotated.X, TRotated.Y, TRotated.Z);
            if (!spinAroundX) spinned.X = TRotated.X;
            if (!spinAroundY) spinned.Y = TRotated.Y;
            if (!spinAroundZ) spinned.Z = TRotated.Z;

            return Vector3.Transform(spinned, Quaternion.Invert(rotation)) + jointPos;
        }
        int d = 0;
        private Vector3 spinIt(float X, float Y, float Z)
        {
            spins = (spins > 10) ? spins : 10;
            d = d + 1 % spins;
            float angle = (float) d / spins * 2.0f * Mathf.PI;
            return new Vector3(X * Mathf.Cos(angle), Y * Mathf.Sin(angle), Z * Mathf.Cos(angle));
        }
    }
}


