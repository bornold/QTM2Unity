using UnityEngine;
using QTM2Unity.Unity;
namespace QTM2Unity
{
    class JCTest : RT
    {
        public float markerScale = 0.015f;
        public bool showRotationTrace = false;
        public bool debug = false;
        private JointLocalization joints;
        private BipedSkeleton skeleton;
        Vector3 thisPos;
        // Use this for initialization
        public override void StartNext()
        {
            joints = new JointLocalization();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            if (debug)
                foreach (LabeledMarker lm in markerData)
                    if (lm.position.IsNaN())
                        Debug.Log(
                            string.Format(
                                    "{0} marker is missing from frame {1}",
                                    lm.label,
                                    rtClient.getFrame()
                            )
                        );
            thisPos = this.transform.position;
            if (joints == null) joints = new JointLocalization();
            BipedSkeleton lastSkel = skeleton;
            skeleton = joints.GetJointLocation(markerData);

        }
        void OnDrawGizmos()
        {
            if (skeleton != null)
            {
                foreach (Bone b in skeleton.Bones)
                {
                    Vector3 v = cv(b.Pos) + thisPos;

                    Gizmos.DrawSphere(v, markerScale);
                    if (showRotationTrace)
                        drawRays(b.Orientation, cv(b.Pos));
                    if (b.Children != null)
                    {
                        foreach (Bone b2 in b.Children)
                        {
                            drawLine(b.Pos, b2.Pos);
                        }
                    }
                }
            }
        }
    }
}

