using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace QTM2Unity
{
    [System.Serializable]
    public class JointsConstrains
    {
        public bool showConstraints = false;
        [Range(0.01f, 0.5f)]
        public float coneSize = 0.05f;
        [Range(1, 150)]
        public int coneResolution = 50;
        public bool showTwistConstraints = false;
    }
    class RT_IK_Constrained : RT_IK
    {
        public JointsConstrains jointsConstrains;
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
            if (!skeleton[BipedSkeleton.HIP_L].HasConstraints)
            {
                constraints.SetConstraints(ref skeleton);
                constraints.SetConstraints(ref skeletonBuffer);
            }
        }
        void OnDrawGizmos()
        {
            if (debugFlag || Application.isPlaying && streaming && skeleton != null)
            {
                Draw();
            }
        }
        public override void Draw()
        {
            base.Draw();
            if (jointsConstrains != null && 
                (jointsConstrains.showConstraints || 
                jointsConstrains.showTwistConstraints))
            {
                foreach (TreeNode<Bone> b in skeleton.Root)
                {
                    if (b.Data.HasConstraints)
                    {
                        OpenTK.Quaternion parentRotation =
                            b.Parent.Data.Orientation * b.Data.ParentPointer;
                        OpenTK.Vector3 poss = b.Data.Pos + pos;
                        if (jointsConstrains.showConstraints)
                        {
                            UnityDebug.CreateIrregularCone(
                                b.Data.Constraints, poss, 
                                OpenTK.Vector3.NormalizeFast(
                                    OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, parentRotation)),
                                parentRotation, 
                                jointsConstrains.coneResolution,
                                jointsConstrains.coneSize);
                        }
                        if (jointsConstrains.showTwistConstraints)
                        {
                            UnityDebug.DrawTwistConstraints(b.Data, b.Parent.Data, poss, bodyRig.traceLength);
                        }
                    }
                }
            }
        }
    }
}
