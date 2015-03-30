using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QTM2Unity.Unity;
using OpenTK;
using UnityEngine;
using UnityEditor;

namespace QTM2Unity
{
    class MultipleChainsTest : MonoBehaviour
    {
        private CCD ccd = new CCD();
        private FABRIK fabrik = new FABRIK();
        private DampedLeastSquares dls = new DampedLeastSquares();
        private JacobianTranspose jtr = new JacobianTranspose();
        private Bone[] bones = new Bone[6];
        private TreeNode<Bone> root;
        private GameObject[] joints;

        private Bone[] targets = new Bone[2];

        private float markerScale = 0.3f;

        private void initializeBones()
        {
            // Targets
            targets[0] = new Bone("target0", new OpenTK.Vector3(4, 4, 0), OpenTK.Quaternion.Identity);
            targets[1] = new Bone("target1", new OpenTK.Vector3(4, 2, 0), OpenTK.Quaternion.Identity);

            // Bones
            OpenTK.Vector3 pos0 = new OpenTK.Vector3(0, 2, 0);
            OpenTK.Vector3 pos1 = new OpenTK.Vector3(2, 2, 0);
            OpenTK.Vector3 pos2 = new OpenTK.Vector3(4, 3, 0);
            OpenTK.Vector3 pos3 = new OpenTK.Vector3(5, 3, 0);
            OpenTK.Vector3 pos4 = new OpenTK.Vector3(4, 1, 0);

            bones[0] = new Bone("root", pos0, OpenTK.Quaternion.Identity); 
            bones[1] = new Bone("bone_1.1", pos1, OpenTK.Quaternion.Identity);
            bones[2] = new Bone("bone_1.2", pos1, OpenTK.Quaternion.Identity);
            bones[3] = new Bone("bone_2", pos2, OpenTK.Quaternion.Identity);
            bones[4] = new Bone("bone_3", pos3);
            bones[5] = new Bone("bone_4", pos4);

            bones[0].RotateTowards(pos1 - pos0);
            bones[1].RotateTowards(pos2 - pos1);
            bones[2].RotateTowards(pos4 - pos1);
            bones[3].RotateTowards(pos3 - pos2);

            for (int i = 0; i < bones.Length - 2; i++)
                bones[i].Rotate(OpenTK.Quaternion.FromAxisAngle(bones[i].GetDirection(), -UnityEngine.Mathf.PI / 2));

            root = new TreeNode<Bone>(bones[0]);
            root.AddChild(bones[1]).AddChild(bones[3]).AddChild(bones[4]);
            root.AddChild(bones[2]).AddChild(bones[5]);
        }

        private void drawJoints()
        {
            if (joints == null || joints.Length == 0)
                joints = new GameObject[bones.Length];


            for (int i = 0; i < joints.Length; i++)
            {
                //GameObject joint = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GameObject joint = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);

                joint.name = bones[i].Name;
                joint.transform.localScale = UnityEngine.Vector3.one * markerScale;
                joint.transform.localPosition =
                    new UnityEngine.Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z);
                joint.transform.localRotation = new UnityEngine.Quaternion(
                    bones[i].Orientation.X,
                    bones[i].Orientation.Y,
                    bones[i].Orientation.Z,
                    bones[i].Orientation.W);
                joint.transform.parent = this.gameObject.transform;
                joints[i] = joint;

                Debug.Log(joint.name + " has pos ("
                    + joint.transform.localPosition.x + ", "
                    + joint.transform.localPosition.y + ", "
                    + joint.transform.localPosition.z + ")");


            }

            for (int i = 0; i < joints.Length - 1; i++)
            {
                Debug.Log("Length between " + joints[i].name + " and " + joints[i + 1].name + ": "
                    + (joints[i + 1].transform.localPosition - joints[i].transform.localPosition).magnitude);
            }
        }

        private void updateJoints()
        {
            for (int i = 0; i < joints.Length; i++)
            {
                joints[i].transform.localPosition =
                    new UnityEngine.Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z);
                joints[i].transform.localRotation = new UnityEngine.Quaternion(
                    bones[i].Orientation.X,
                    bones[i].Orientation.Y,
                    bones[i].Orientation.Z,
                    bones[i].Orientation.W);
                Debug.Log(joints[i].name + " has pos ("
                    + joints[i].transform.localPosition.x + ", "
                    + joints[i].transform.localPosition.y + ", "
                    + joints[i].transform.localPosition.z + ")");
            }

            for (int i = 0; i < joints.Length - 1; i++)
            {
                Debug.Log("Length between " + joints[i].name + " and " + joints[i + 1].name + ": "
                    + (joints[i + 1].transform.localPosition - joints[i].transform.localPosition).magnitude);
            }

            Debug.Log("Length between end effector0 and target0: "
                + (bones[4].Pos - targets[0].Pos).Length);
            Debug.Log("Length between end effector1 and target1: "
                + (bones[5].Pos - targets[1].Pos).Length);
        }

        void Start()
        {
            initializeBones();
            drawJoints();
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            for (int i = 0; i < bones.Length - 2; i++)
            {
                Bone b = bones[i];
                // draw orientations
                Gizmos.color = Color.cyan; // direction
                var pos = new UnityEngine.Vector3(b.Pos.X, b.Pos.Y, b.Pos.Z);
                OpenTK.Vector3 d = b.GetDirection();
                var direction = new UnityEngine.Vector3(d.X, d.Y, d.Z);
                Gizmos.DrawRay(pos, direction);

                Gizmos.color = Color.magenta; // up
                OpenTK.Vector3 u = b.GetUp();
                var up = new UnityEngine.Vector3(u.X, u.Y, u.Z);
                Gizmos.DrawRay(pos, up);

                Gizmos.color = Color.green; // right
                OpenTK.Vector3 r = b.GetRight();
                var right = new UnityEngine.Vector3(r.X, r.Y, r.Z);
                Gizmos.DrawRay(pos, right);

            }

            //draw "bones"
            Gizmos.color = Color.red;
            IEnumerator<TreeNode<Bone>> it = root.GetEnumerator();
            while (it.MoveNext())
            {
                Bone b = it.Current.Data;
                foreach (var c in it.Current.Children)
                {
                    Gizmos.DrawLine(opentk2unity(b.Pos), opentk2unity(c.Data.Pos));
                }
            }

            // draw targets
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(new UnityEngine.Vector3(targets[0].Pos.X, targets[0].Pos.Y, targets[0].Pos.Z), 0.1f);
            Gizmos.DrawSphere(new UnityEngine.Vector3(targets[1].Pos.X, targets[1].Pos.Y, targets[1].Pos.Z), 0.1f);

        }

        private UnityEngine.Vector3 opentk2unity(OpenTK.Vector3 v)
        {
            return new UnityEngine.Vector3(v.X, v.Y, v.Z);
        }

        public void runCCD()
        {
            //bones = ccd.solveBoneChain(bones, target, OpenTK.Vector3.Zero);
            updateJoints();
        }

        public void runFABRIK()
        {
            //bones = fabrik.solveBoneChain(bones, target, OpenTK.Vector3.Zero);
            updateJoints();
        }

        public void runDLS()
        {
            //bones = dls.solveBoneChain(bones, target, OpenTK.Vector3.Zero);
            updateJoints();
        }

        public void runTranspose()
        {
            jtr.solveMultipleChains(ref root, ref targets); // TODO will it actually change the bones in the tree?
            updateJoints();
        }
    }


    [CustomEditor(typeof(MultipleChainsTest))]
    class MultipleChainsTestEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            MultipleChainsTest myTarget = (MultipleChainsTest)target;

            if (GUILayout.Button("CCD"))
            {
                myTarget.runCCD();
            }

            if (GUILayout.Button("FABRIK"))
            {
                myTarget.runFABRIK();
            }

            if (GUILayout.Button("DLS"))
            {
                myTarget.runDLS();
            }

            if (GUILayout.Button("Jacobian Transpose"))
            {
                myTarget.runTranspose();
            }
        }
    }
}