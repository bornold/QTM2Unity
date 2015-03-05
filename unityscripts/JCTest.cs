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
            if (debug )
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
                foreach (TreeNode<Bone> b in skeleton)
                {
                    Gizmos.DrawSphere(cv(b.Data.Pos) + thisPos, markerScale);
                    if (showRotationTrace)
                        drawRays(b.Data.Orientation, cv(b.Data.Pos));
                    foreach (TreeNode<Bone> b1 in b.Children)
                    {
                        drawLine(b.Data.Pos, b1.Data.Pos);
                    }
                }
            }    
        }
    }
}

