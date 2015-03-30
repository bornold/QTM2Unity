using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
namespace QTM2Unity
{
    class IKTest : RT {
        private JointLocalization joints;
        private BipedSkeleton skeleton;
        private Vector3 thisPos;
        private IKApplier ikApplier = new IKApplier();
        public bool showRotationTrace;
        public float markerScale;
        public IK ik = IK.CCD;
        // Use this for initialization
        public override void StartNext()
        {
            joints = new JointLocalization();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            thisPos = this.transform.position;
            if (joints == null) joints = new JointLocalization();
            skeleton = joints.GetJointLocation(markerData);
            if (ik == IK.CCD)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new CCD());
            }
            else if (ik == IK.FABRIK)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new FABRIK());
            }
            else if (ik == IK.TRANSPOSE)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new JacobianTranspose());
            }
            else if (ik == IK.DLS)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new DampedLeastSquares());
            }
            else
            {
                skeleton = ikApplier.ApplyIK(skeleton, new TargetTriangleIK());
            }
        }
        void OnDrawGizmos()
        {
            if (skeleton != null)
            {
                foreach (TreeNode<Bone> b in skeleton)
                {
                    Vector3 v = cv(b.Data.Pos) + thisPos;
                
                    Gizmos.DrawSphere(v, markerScale);
                    if (showRotationTrace )
                        drawRays(b.Data.Orientation, cv(b.Data.Pos));
                    if (!b.IsLeaf)
                    {
                        foreach (TreeNode<Bone> b2 in b.Children)
                        {
                            drawLine(b.Data.Pos, b2.Data.Pos);
                        }
                    }
                }
            }
        }
    }
}


