﻿using System;
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
        public int coneResolution = 50;
        //public bool showL1 = false;
        public bool showParentRotation = false;
        public bool showTwistConstraints = false;
        public bool leftSide = true;
        public bool midToo = true;
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
            if (!skeleton[1].HasConstraints) 
            {
                constraints.SetConstraints(ref skeleton);
                constraints.SetConstraints(ref skeletonBuffer);
            }
        }
        public override void Draw()
        {
            base.Draw();
            if (showConstraints || showParentRotation || showTwistConstraints)// || showL1)
            {
                foreach (TreeNode<Bone> b in skeleton)
                {
                    if (!b.IsRoot && b.Data.HasConstraints)
                    {
                        OpenTK.Quaternion parentRotation = b.Parent.Data.Orientation * b.Data.ParentPointer;
                        Bone c = b.Data;
                        OpenTK.Vector3 L1 = OpenTK.Vector3.Normalize(OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, parentRotation));
                        OpenTK.Vector3 poss = c.Pos + pos;
                        if (showConstraints )//&& b.Data.Name.EndsWith("L")) 
                        {
                            UnityDebug.CreateIrregularCone(c.Constraints, poss, L1, parentRotation, coneResolution, coneSize);

                        }
//                        if (showL1) UnityDebug.DrawLine(poss, poss + L1 * traceLength, UnityEngine.Color.black);
                        if (showParentRotation)
                            // && b.Data.Name.EndsWith("L")) 
                        {
                            UnityDebug.DrawRays2(parentRotation, poss, traceLength);
                        }
                        if (showTwistConstraints)
                        {
                            if (midToo)
                            { 
                                UnityDebug.DrawTwistConstraints(c, b.Parent.Data, poss, traceLength * 1.1f);
                            } else if (leftSide ? b.Data.Name.EndsWith("L") : b.Data.Name.EndsWith("R"))
                            {
                                UnityDebug.DrawTwistConstraints(c, b.Parent.Data, poss, traceLength * 1.1f);
                            }
                        }
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying && streaming)
            {
                Draw();
            }
        }
    }
}