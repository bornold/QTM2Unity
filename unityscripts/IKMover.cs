using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QTM2Unity
{
    class IKMover : StandardUnityModel
    {
        public IK ik = IK.CCD;
        public bool useConstraints = false;
        public bool showConstraints = false;
        public float coneSize = 0.05f;
        public int coneRes = 50;
        private Vector3 thisPos;
        private IKApplier ikApplier = new IKApplier();
        private ConstraintsExamples constex = new ConstraintsExamples();
        public override void UpdateNext()
        {
            base.UpdateNext();
            thisPos = this.transform.position;
            skeleton = joints.GetJointLocation(markerData);
            if (useConstraints) constex.SetConstraints(ref skeleton);
            switch (ik)
            {
                case IK.CCD:
                    skeleton = ikApplier.ApplyIK(skeleton, new CCD());
                    break;
                case IK.FABRIK:
                    skeleton = ikApplier.ApplyIK(skeleton, new FABRIK());
                    break;
                case IK.DLS:
                    skeleton = ikApplier.ApplyIK(skeleton, new DampedLeastSquares());
                    break;
                case IK.TRANSPOSE:
                    skeleton = ikApplier.ApplyIK(skeleton, new JacobianTranspose());
                    break;
                case IK.TT:
                    skeleton = ikApplier.ApplyIK(skeleton, new TargetTriangleIK());
                    break;
                default:
                    break;
            }
            SetAll();
        }
        public void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (skeleton != null && showConstraints)
            {
                foreach (TreeNode<Bone> b in skeleton)
                {
                    if (!b.IsRoot && b.Data.Constraints != OpenTK.Vector4.Zero)
                    {
                        Bone referenceBone;
                        if (b.Data.Name.Equals(BipedSkeleton.UPPERLEG_L)
                            || b.Data.Name.Equals(BipedSkeleton.UPPERLEG_R))
                        {
                            OpenTK.Quaternion happy = b.Parent.Data.Orientation * QuaternionHelper.RotationZ(OpenTK.MathHelper.Pi);
                            referenceBone = new Bone(
                                "",
                                b.Parent.Data.Pos,
                                happy);
                        }
                        else if (b.Data.Name.Equals(BipedSkeleton.SHOULDER_R))
                        {
                            referenceBone = new Bone(
                                "",
                                b.Parent.Data.Pos,
                                b.Parent.Data.Orientation * QuaternionHelper.RotationZ(-OpenTK.MathHelper.PiOver2));
                        }
                        else if (b.Data.Name.Equals(BipedSkeleton.SHOULDER_L))
                        {
                            referenceBone = new Bone(
                                "",
                                b.Parent.Data.Pos,
                                b.Parent.Data.Orientation * QuaternionHelper.RotationZ(OpenTK.MathHelper.PiOver2));
                        }
                        else
                        {
                            referenceBone = b.Parent.Data;
                        }
                        Bone c = b.Data;
                        OpenTK.Vector3 L1 = referenceBone.GetDirection();
                        OpenTK.Vector3 pos = c.Pos + thisPos.Convert();
                        UnityDebug.DrawLine(pos, pos + L1 * 0.2f, UnityEngine.Color.black);
                        UnityDebug.CreateIrregularCone3(c.Constraints, pos, L1, referenceBone.Orientation, coneRes, coneSize);
                    }
                }
            }
        }
    }
}
