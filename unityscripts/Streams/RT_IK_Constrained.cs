using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace QTM2Unity
{
    class RT_IK_Constrained : RT_IK
    {
        public bool showConstraints = false;
        public float coneSize = 0.05f;
        public int coneRes = 50;
        public bool showL1 = false;
        public bool showParentRot = false;
        protected ConstraintsExamples constraints = new ConstraintsExamples();
        public override void StartNext()
        {
            base.StartNext();
            constraints.SetConstraints(ref skeleton);
            constraints.SetConstraints(ref skeletonBuffer);
        }
        public override void UpdateNext()
        {
            base.UpdateNext();
            if (skeleton[BipedSkeleton.LOWERLEG_L].Constraints == OpenTK.Vector4.Zero) 
            {
                constraints.SetConstraints(ref skeleton);
                constraints.SetConstraints(ref skeletonBuffer);
            }
        }
        void LateUpdate()
        {
            if (!streaming) return;
            foreach (TreeNode<Bone> b in skeleton)
            {
                if (showConstraints)
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
                        OpenTK.Vector3 L1 = referenceBone.GetYAxis();
                        OpenTK.Vector3 poss = c.Pos + pos;
                        UnityDebug.CreateIrregularCone3(c.Constraints, poss, L1, referenceBone.Orientation, coneRes, coneSize);
                        if (showL1) UnityDebug.DrawLine(poss, poss + L1 * traceScale, UnityEngine.Color.black);
                        if (showParentRot) UnityDebug.DrawRays2(referenceBone.Orientation, c.Pos+pos, traceScale);

                    }
                }
            }
        }
    }
}
