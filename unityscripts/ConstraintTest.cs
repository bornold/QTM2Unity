using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QTM2Unity.Unity;
using OpenTK;
using UnityEngine;
using UnityEditor;
#if flase
namespace QTM2Unity
{
    class ConstraintTest : MonoBehaviour
    {
        private CCD ccd = new CCD();
        //private FABRIK fabrik = new FABRIK();
        private Bone[] bones = new Bone[4];
        private GameObject[] joints;

        private float markerScale = 0.3f;

        private void initializeBones()
        {
            OpenTK.Vector3 pos0 = new OpenTK.Vector3(0, 2, 0);
            OpenTK.Vector3 pos1 = new OpenTK.Vector3(2, 2, 0);
            OpenTK.Vector3 pos2 = new OpenTK.Vector3(4, 2, 0);
            OpenTK.Vector3 pos3 = new OpenTK.Vector3(5, 1, 1);

            OpenTK.Quaternion rot0 = OpenTK.Quaternion.FromAxisAngle(new OpenTK.Vector3(0, 1, 0),
                MathHelper.DegreesToRadians(-90));
            OpenTK.Quaternion rot2 = QuaternionHelper.getRotation((pos2 - pos1), (pos3 - pos2)) * rot0;

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
            //bones[1].SetRotationalConstraint(45f, 45f, 45f, 45f, bones[0].getDirection, bones[0].getRight);
            //bones[2].SetRotationalConstraint(0.1f, 0.1f, 0.1f, 0.1f, bones[1].getDirection, bones[1].getRight);
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
                    + ccd.getTwistAngle(bones[i], bones[i - 1]));
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

            //Rotation around direction?
            for (int i = 1; i < bones.Length; i++)
            {
                Debug.Log(bones[i].Name + " angle around dir: "
                    + ccd.getTwistAngle(bones[i], bones[i - 1]));
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

            for (int i = 0; i < bones.Length - 1; i++)
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

            // Draw direction vector of constraint
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
            // constraint calculated in solver
            Gizmos.color = Color.blue;
            // Find parents constraint cone
            OpenTK.Vector3 L1 = bones[1].GetDirection();
            L1.Normalize();
            L1 = L1 * (bones[3].Pos - bones[2].Pos).Length; // makes sure L1 has sufficient length
            // find the projection O of the target on line L1
            OpenTK.Vector3 O = bones[2].Pos + Vector3Helper.ProjectAndCreate(bones[3].Pos - bones[2].Pos, L1);
            Gizmos.DrawRay(new UnityEngine.Vector3(bones[2].Pos.X, bones[2].Pos.Y, bones[2].Pos.Z),
                new UnityEngine.Vector3(L1.X, L1.Y, L1.Z));
            Gizmos.DrawSphere(new UnityEngine.Vector3(O.X, O.Y, O.Z), 0.1f);

            // translated pos to have O as origin
            OpenTK.Vector3 target = bones[3].Pos;
            target = target.translate(O);
            
            OpenTK.Vector3 right2 = bones[1].GetRight();
            OpenTK.Quaternion rot1 = QuaternionHelper.getRotation(L1, OpenTK.Vector3.UnitZ);
            right2 = OpenTK.Vector3.Transform(right2, rot1);
            OpenTK.Quaternion rot2 = QuaternionHelper.getRotation(right2, OpenTK.Vector3.UnitX);
            OpenTK.Quaternion rotation = rot2 * rot1;

            target = OpenTK.Vector3.Transform(target, rotation);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(new UnityEngine.Vector3(target.X, target.Y, target.Z), 0.1f);
            Gizmos.DrawLine(new UnityEngine.Vector3(0, 0, 0),
                new UnityEngine.Vector3(target.X, target.Y, target.Z));
    }

        public void checkRotationalConstraint(ref Bone[] bs, int current)
        {
            if (current == 0)
                return; // Don't check constraint for root
            RotationalConstraint rc = bs[current - 1].RotationalConstraint;
            if (rc != null) // there exist a constraint on parent
            {
                OpenTK.Vector3 pos;
                calculatePosition(out pos, ref bs[current], ref bs[current - 1]);

                // Rotate b to the nearest point 
                // Rotate b's children with the same rotation
                if (pos != bs[current].Pos) // the position has moved
                {
                    Debug.Log("Position before: " +
                        bs[current].Pos.X + ", " + bs[current].Pos.Y + ", " + bs[current].Pos.Z);
                    // Rotate b towards pos and follow with all its children
                    OpenTK.Vector3 parent = bs[current].Pos - bs[current - 1].Pos;
                    OpenTK.Vector3 newParent = pos - bs[current - 1].Pos;
                    OpenTK.Vector3 axis =
                        OpenTK.Vector3.Cross(parent, newParent);
                    axis.Normalize();
                    float angle = OpenTK.Vector3.CalculateAngle(parent, newParent);
                    OpenTK.Quaternion rot = OpenTK.Quaternion.FromAxisAngle(axis, angle);

                    Debug.Log("Rotate " + bs[current].Name + " " + angle + " around "
                        + axis.X + ", " + axis.Y + ", " + axis.Z);

                    for (int i = current - 1; i < bs.Length; i++)
                    {
                        bs[i].Rotate(rot);
                    }
                    Debug.Log("Position after: " +
                        bs[current].Pos.X + ", " + bs[current].Pos.Y + ", " + bs[current].Pos.Z);
                }
            }
        }

        // denna kan kanske bli finare
        private void calculatePosition(out OpenTK.Vector3 pos, ref Bone b, ref Bone parent)
        {
            RotationalConstraint rc = parent.RotationalConstraint;
            pos = b.Pos;
            // Find parents constraint cone
            OpenTK.Vector3 L1 = rc.getDirection();
            L1.Normalize();
            L1 = L1 * (pos - parent.Pos).Length; // makes sure L1 has sufficient length
            // find the projection O of the target on line L1
            //OpenTK.Vector3 O = Vector3Helper.ProjectAndCreate(pos, L1);
            OpenTK.Vector3 O = parent.Pos + Vector3Helper.ProjectAndCreate(pos - parent.Pos, L1);
            // find the distance S between O and the joint position
            float S = (O - pos).Length;
            // Map the target in such a way that O is located at the origin
            // and the axes defining the constraints are aligned with x,y
            OpenTK.Quaternion mappingQuat = mapToOrigin(ref pos, L1, rc.getRight(), O);

            // Angles defining ellipse radii
            float ax, ay;
            findAngles(ref rc, pos, out ax, out ay);

            // Calculate ellipse radius for x and y
            float radiusX = S * Mathf.Tan(ax);
            float radiusY = S * Mathf.Tan(ay);

            // If b's pos is outside the parents constraint
            Debug.Log("The ellipse radius is x " + radiusX + ", y " + radiusY +
                " and the position is x " + pos.X + " y " + pos.Y);
            if (!(Math.Abs(pos.X) <= radiusX && Math.Abs(pos.Y) <= radiusY)) // target not inside
            {
                Debug.Log(b.Name + " not inside constraint");
                // Find nearest point from b on ellipse defined by radiusX and radiusY
                OpenTK.Vector2 newPoint = Mathf.FindNearestPointOnEllipse
                    (Math.Max(radiusX, radiusY), Math.Min(radiusX, radiusY),
                    new OpenTK.Vector2(Math.Abs(pos.X), Math.Abs(pos.Y)));

                // Move target to nearest point on the ellipse
                moveTarget(ref pos, newPoint);

            }
            else
                Debug.Log(b.Name + " is inside constraint");

            // Undo the origin mapping
            // Rotation
            pos = OpenTK.Vector3.Transform(pos, OpenTK.Quaternion.Invert(mappingQuat));
            // Translation
            pos = O + pos;
        }

        private OpenTK.Quaternion mapToOrigin(ref OpenTK.Vector3 target, OpenTK.Vector3 L1, 
            OpenTK.Vector3 right, OpenTK.Vector3 origin)
        {
            // Translation:
            Debug.Log("target before translation " + target.X + ", " + target.Y + ", " + target.Z);
            target = target.translate(origin);
            Debug.Log("target after translation " + target.X + ", " + target.Y + ", " + target.Z);
            //L1 = L1.translate(origin);
            //right = right.translate(origin);
            // Rotation:
            OpenTK.Quaternion rot1 = QuaternionHelper.getRotation(L1, OpenTK.Vector3.UnitZ);
            right = OpenTK.Vector3.Transform(right, rot1);
            OpenTK.Quaternion rot2 = QuaternionHelper.getRotation(right, OpenTK.Vector3.UnitX);
            OpenTK.Quaternion rotation = rot2 * rot1;

            target = OpenTK.Vector3.Transform(target, rotation);
            Debug.Log("target after rotation " + target.X + ", " + target.Y + ", " + target.Z);
            return rotation;
        }

        private void findAngles(ref RotationalConstraint rc, OpenTK.Vector3 target, out float ax, out float ay)
        {
            // Locate target in a particular quadrant
            if (target.X >= 0 && target.Y >= 0)
            {
                // x, y quadrant
                // Ellipse defined by q1, q2
                ax = rc.getAngle(1);
                ay = rc.getAngle(2);
            }
            else if (target.X < 0 && target.Y >= 0)
            {
                // -x, y quadrant
                // Ellipse defined by q2, q3
                ay = rc.getAngle(2);
                ax = rc.getAngle(3);
            }
            else if (target.X <= 0 && target.Y < 0)
            {
                // -x,-y quadrant
                // Ellipse defined by q3, q0
                ax = rc.getAngle(3);
                ay = rc.getAngle(0);
            }
            else /*if (target.X > 0 && target.Y < 0)*/
            {
                // x, -y quadrant
                // Ellipse defined by q0, q1
                ay = rc.getAngle(0);
                ax = rc.getAngle(1);
            }
        }

        private void moveTarget(ref OpenTK.Vector3 target, OpenTK.Vector2 newTarget)
        {
            float x = newTarget.X;
            float y = newTarget.Y;
            if (target.X < 0)
                x = -x;
            if (target.Y < 0)
                y = -y;

            target.X = x;
            target.Y = y;
        }

        public void checkRotational()
        {
            checkRotationalConstraint(ref bones, 2);
            //ccd.checkRotationalConstraint(ref bones, 2);
            updateJoints();
        }

    }


    [CustomEditor(typeof(ConstraintTest))]
    class ConstraintTestEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            ConstraintTest myTarget = (ConstraintTest)target;

            if (GUILayout.Button("Rotational"))
            {
                myTarget.checkRotational();
            }
        }
    }
}
#endif