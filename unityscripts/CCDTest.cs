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
    class CCDTest : MonoBehaviour
    {
        private CCD ccd = new CCD();
        private FABRIK fabrik = new FABRIK();
        private Bone[] bones = new Bone[4];
        private GameObject[] joints;

        private OpenTK.Vector3 target = new OpenTK.Vector3(5, 4, -1);//(7, 2, 0); //unreachable //(5, 4, -1);

        private float markerScale = 0.3f;

        private void initializeBones()
        {
            OpenTK.Vector3 pos0 = new OpenTK.Vector3(0, 2, 0);
            OpenTK.Vector3 pos1 = new OpenTK.Vector3(2, 2, 0);
            OpenTK.Vector3 pos2 = new OpenTK.Vector3(4, 2, 0);
            OpenTK.Vector3 pos3 = new OpenTK.Vector3(5, 1, 1);
        
            OpenTK.Quaternion rot0 = OpenTK.Quaternion.FromAxisAngle(new OpenTK.Vector3(0,1,0), 
                MathHelper.DegreesToRadians(-90));
            OpenTK.Quaternion rot2 = QuaternionHelper.getRotation((pos2 - pos1), (pos3 - pos2)) * rot0;

            /*target = OpenTK.Vector3.Transform(pos3 - pos2,
                OpenTK.Quaternion.FromAxisAngle(OpenTK.Vector3.Cross(pos3-pos2, pos2-pos1), 
                    MathHelper.DegreesToRadians(60)));*/
                        
            bones[0] = new Bone("arm_root", pos0, rot0);
            bones[1] = new Bone("arm_1", pos1, rot0);
            bones[2] = new Bone("arm_2", pos2, rot2);
            bones[3] = new Bone("arm_end", pos3/*, rot2*/);

            bones[3].Rotate(OpenTK.Quaternion.FromAxisAngle(bones[3].GetDirection(), UnityEngine.Mathf.PI / 4));

            //Constraints
            foreach (Bone b in bones)
            {
                b.setOrientationalConstraint(10, 45);
            }
            bones[1].SetRotationalConstraint(45f, 45f, 45f, 45f);
            bones[2].SetRotationalConstraint(0.5f, 0.5f, 0.5f, 0.5f);
            //bones[3].setRotationalConstraint(0.5f, 0.5f, 0.5f, 0.5f);
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

            //Rotation around direction?
            for (int i = 1; i < bones.Length; i++)
            {
                Debug.Log(bones[i].Name + " angle around dir: "
                    + ccd.getTwistAngle(bones[i], bones[i-1]));
            }
        }

        private void updateJoints()
        {
            /*CCD.checkOrientationalConstraint(ref bones[2], bones[1]);
            CCD.checkOrientationalConstraint(ref bones[3], bones[2]);*/

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
                Debug.Log("Length between " + joints[i].name + " and " + joints[i+1].name + ": "
                    + (joints[i+1].transform.localPosition - joints[i].transform.localPosition).magnitude);
            }

            Debug.Log("Length between end effector and target: "
                + (joints[joints.Length - 1].transform.localPosition
                - new UnityEngine.Vector3(target.X, target.Y, target.Z)).magnitude);

            /*CCD.checkOrientationalConstraint(ref bones[1], bones[0]);
            CCD.checkOrientationalConstraint(ref bones[2], bones[1]);
            CCD.checkOrientationalConstraint(ref bones[3], bones[2]);*/

            //Rotation around direction?
            for (int i = 1; i < bones.Length; i++)
            {
                Debug.Log(bones[i].Name + " angle around dir: "
                    + ccd.getTwistAngle(bones[i], bones[i-1]));
            }
            
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

            for (int i = 0; i < bones.Length - 1; i++ )
            {
                Bone b = bones[i];
                // draw orientations
                Gizmos.color = Color.cyan;
                var pos = new UnityEngine.Vector3(b.Pos.X, b.Pos.Y, b.Pos.Z);
                OpenTK.Vector3 d = b.GetDirection();
                var direction = new UnityEngine.Vector3(d.X, d.Y, d.Z);
                Gizmos.DrawRay(pos, direction);

                Gizmos.color = Color.magenta;
                OpenTK.Vector3 u = b.GetUp();
                var up = new UnityEngine.Vector3(u.X, u.Y, u.Z);
                Gizmos.DrawRay(pos, up);

                Gizmos.color = Color.green;
                OpenTK.Vector3 r = b.GetRight();
                var right = new UnityEngine.Vector3(r.X, r.Y, r.Z);
                Gizmos.DrawRay(pos, right);

            }

            //draw "bones"
            for (int i = 0; i < bones.Length - 1; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(joints[i].transform.localPosition, joints[i + 1].transform.localPosition);
            }

            // draw target
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(new UnityEngine.Vector3(target.X, target.Y, target.Z), 0.1f);

            // draw reference vector
            /*Gizmos.color = Color.yellow;
            for (int i = 1; i < bones.Length; i++)
            {
                OpenTK.Vector3 reference = 
                    Vector3Helper.ProjectOnPlane(bones[i-1].getUp(), bones[i].getDirection());

                Gizmos.DrawRay(new UnityEngine.Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z),
                    new UnityEngine.Vector3(reference.X, reference.Y, reference.Z));
            }*/
           /* Gizmos.color = Color.red;
            Gizmos.DrawRay(UnityEngine.Vector3.zero, new UnityEngine.Vector3(parentUp.X, parentUp.Y, parentUp.Z));*/

            // Draw direction vector of constraint
                    /*
            Gizmos.color = Color.yellow;
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i].RotationalConstraint != null)
                {
                    OpenTK.Vector3 dir = bones[i].RotationalConstraint.getDirection();
                    Gizmos.DrawRay(new UnityEngine.Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z),
                    new UnityEngine.Vector3(dir.X, dir.Y, dir.Z));

                    OpenTK.Vector3 right = bones[i].RotationalConstraint.getRight();
                    OpenTK.Vector3 up = OpenTK.Vector3.Cross(dir, right);
                    // TODO not sure rotating the right direction (+ or - angle)
                    OpenTK.Vector3 coneRight = OpenTK.Vector3.Transform(dir,
                        OpenTK.Quaternion.FromAxisAngle(up, bones[i].RotationalConstraint.getAngle(2)));
                    OpenTK.Vector3 coneLeft = OpenTK.Vector3.Transform(dir,
                        OpenTK.Quaternion.FromAxisAngle(up, -bones[i].RotationalConstraint.getAngle(0)));
                    OpenTK.Vector3 coneDown = OpenTK.Vector3.Transform(dir,
                        OpenTK.Quaternion.FromAxisAngle(right, bones[i].RotationalConstraint.getAngle(3)));
                    OpenTK.Vector3 coneUp = OpenTK.Vector3.Transform(dir,
                        OpenTK.Quaternion.FromAxisAngle(right, -bones[i].RotationalConstraint.getAngle(1)));
                    Gizmos.DrawRay(new UnityEngine.Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z),
                    new UnityEngine.Vector3(coneLeft.X, coneLeft.Y, coneLeft.Z));
                    Gizmos.DrawRay(new UnityEngine.Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z),
                    new UnityEngine.Vector3(coneRight.X, coneRight.Y, coneRight.Z));
                    Gizmos.DrawRay(new UnityEngine.Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z),
                    new UnityEngine.Vector3(coneDown.X, coneDown.Y, coneDown.Z));
                    Gizmos.DrawRay(new UnityEngine.Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z),
                    new UnityEngine.Vector3(coneUp.X, coneUp.Y, coneUp.Z));
                }
            }
            */
        }

        public void runCCD()
        {
            bones = ccd.solveBoneChain(bones, new Bone("target", target),OpenTK.Vector3.UnitY);

            //CCD.checkOrientationalConstraint(ref bones[1], bones[0]);
            //CCD.checkOrientationalConstraint(ref bones[2], bones[1]);
            //CCD.checkOrientationalConstraint(ref bones[3], bones[2]);
            
            //******************
            updateJoints();
        }

        public void runFABRIK()
        {
            bones = fabrik.solveBoneChain(bones, new Bone("target", target), OpenTK.Vector3.UnitY);
            updateJoints();
        }

    }


    [CustomEditor(typeof(CCDTest))]
    class CCDTestEditor : Editor {

        public override void OnInspectorGUI()
        {
             CCDTest myTarget = (CCDTest)target;

             if (GUILayout.Button("CCD"))
             {
                 myTarget.runCCD();
             }

             if (GUILayout.Button("FABRIK"))
             {
                 myTarget.runFABRIK();
             }
        }
    }
}