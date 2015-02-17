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
            skeleton = new BipedSkeleton();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            if (debug && false  )
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
            if (skeleton == null) skeleton = new BipedSkeleton();
            BipedSkeleton lastSkel = skeleton;
            joints.GetJointLocation(ref skeleton, markerData);            
        }

        void OnDrawGizmos()
        {
            if (skeleton != null)
            {
                foreach (TreeNode<Bone> b in skeleton)
                {
                    if (debug) Debug.Log(string.Format("Name: {0}, Pos: {1}",b.Data.Name,b.Data.Pos));
                    Gizmos.DrawSphere(cv(b.Data.Pos) + thisPos, markerScale);
                    foreach (TreeNode<Bone> b1 in b.Children)
                    {
                        drawLine(b.Data.Pos, b1.Data.Pos);
                    }
                }
            }
            else
            {
                Debug.Log(" NOOOOOO! Skeleton is null!");
            }
            
        }
    }
}

