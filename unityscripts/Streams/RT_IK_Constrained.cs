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
            if (showConstraints || showParentRot || showL1)
            {
                foreach (TreeNode<Bone> b in skeleton)
                {
                    if (!b.IsRoot && b.Data.Constraints != OpenTK.Vector4.Zero)
                    {
                        OpenTK.Quaternion parentRotation = b.Parent.Data.Orientation * b.Data.ParentPointer;
                        Bone c = b.Data;
                        OpenTK.Vector3 L1 = OpenTK.Vector3.Normalize(OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, parentRotation));  //referenceBone.GetYAxis();
                        OpenTK.Vector3 poss = c.Pos + pos;
                        if (showConstraints) UnityDebug.CreateIrregularCone3(c.Constraints, poss, L1, parentRotation, coneRes, coneSize);
                        if (showL1) UnityDebug.DrawLine(poss, poss + L1 * traceScale, UnityEngine.Color.black);
                        if (showParentRot) UnityDebug.DrawRays2(parentRotation, poss, traceScale);

                    }
                }
            }
        }
    }
}
