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

        private Bone[] bones = new Bone[4];
        private GameObject[] joints;

        private OpenTK.Vector3 target = new OpenTK.Vector3(5, 4, 0);

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
                        
            bones[0] = new Bone("arm_root", pos0, rot0);
            bones[1] = new Bone("arm_1", pos1, rot0);
            bones[2] = new Bone("arm_2", pos2, rot2);
            bones[3] = new Bone("arm_end", pos3, rot2);

            bones[3].rotate(OpenTK.Quaternion.FromAxisAngle(bones[3].getDirection(), UnityEngine.Mathf.PI / 4));

            //Constraints
            foreach (Bone b in bones)
            {
                b.setOrientationalConstraint(10, 45);
            }
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
                    + CCD.getTwistAngle(bones[i], bones[i-1]));
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
                    + CCD.getTwistAngle(bones[i], bones[i-1]));
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
            
            foreach (Bone b in bones)
            {
                // draw orientations
                Gizmos.color = Color.cyan;
                var pos = new UnityEngine.Vector3(b.Pos.X, b.Pos.Y, b.Pos.Z);
                OpenTK.Vector3 d = b.getDirection();
                var direction = new UnityEngine.Vector3(d.X, d.Y, d.Z);
                Gizmos.DrawRay(pos, direction);

                Gizmos.color = Color.magenta;
                OpenTK.Vector3 u = b.getUp();
                var up = new UnityEngine.Vector3(u.X, u.Y, u.Z);
                Gizmos.DrawRay(pos, up);

                Gizmos.color = Color.green;
                OpenTK.Vector3 r = b.getRight();
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
            Gizmos.color = Color.yellow;
            for (int i = 1; i < bones.Length; i++)
            {
                OpenTK.Vector3 reference = 
                    Vector3Helper.ProjectOnPlane(bones[i-1].getUp(), bones[i].getDirection());

                Gizmos.DrawRay(new UnityEngine.Vector3(bones[i].Pos.X, bones[i].Pos.Y, bones[i].Pos.Z),
                    new UnityEngine.Vector3(reference.X, reference.Y, reference.Z));
            }
           /* Gizmos.color = Color.red;
            Gizmos.DrawRay(UnityEngine.Vector3.zero, new UnityEngine.Vector3(parentUp.X, parentUp.Y, parentUp.Z));*/
        }

        public void runCCD()
        {
            bones = CCD.solveBoneChain(bones, target);

            //CCD.checkOrientationalConstraint(ref bones[1], bones[0]);
            //CCD.checkOrientationalConstraint(ref bones[2], bones[1]);
            //CCD.checkOrientationalConstraint(ref bones[3], bones[2]);
            
            //******************
            updateJoints();
        }

        public Bone[] solveBoneChain(Bone[] bones, OpenTK.Vector3 target)
        {
            int numberOfIterations = 20;
            float threshold = 0.1f;
            int numberOfBones = bones.Length;
            int iter = 0;
            while ((target - (bones[numberOfBones - 1].Pos)).Length > threshold
                && iter < numberOfIterations)
            {
                // for each bone, starting with the one closest to the end effector 
                // (but not the end effector itself)
                for (int i = numberOfBones - 2; i >= 0; i--)
                {
                    // Get the vectors between the points
                    OpenTK.Vector3 a = bones[numberOfBones - 1].Pos - bones[i].Pos;
                    OpenTK.Vector3 b = target - bones[i].Pos;

                    // Make a rotation quaternion and rotate 
                    // - first the endEffector
                    // - then the rest of the affected joints
                    OpenTK.Quaternion rotation = QuaternionHelper.getRotation(a, b);
                    for (int j = numberOfBones - 1; j >= i; j--)
                    {
                        if (j > i)
                        {
                            Debug.Log("rotating bone " + j + "around " + i);
                            bones[j].Pos = bones[i].Pos +
                                OpenTK.Vector3.Transform((bones[j].Pos - bones[i].Pos), rotation);
                        }

                        // rotate orientation
                        bones[j].rotate(rotation);

                        // Check constraints
                        //checkOrientationalConstraint(ref bones[j]);
                    }
                    // I think we need to do this check here <-- TODO check that I'm right
                    if ((target - bones[numberOfBones - 1].Pos).Length <= threshold)
                    {
                        Debug.Log("Converging because close enough to target (" +
                            (target - bones[numberOfBones - 1].Pos).Length + ")" 
                            + " iter = " + iter);
                        return bones;
                    }
                }
                iter++;
            }
            Debug.Log("Converging because close enough to target (" +
                            (target - bones[numberOfBones - 1].Pos).Length + ")"
                            + "or because reached max number of iterations (" + iter + ")");
            return bones;
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

        }
    }
}