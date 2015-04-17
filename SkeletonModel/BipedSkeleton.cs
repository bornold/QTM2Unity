using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    class BipedSkeleton : Skeleton
    {
        // String constants for bones in a biped skeleton
        #region joint Names
        //Root
        public const string PELVIS = "pelvis";
        // Left leg chain
        public const string UPPERLEG_L = "upperLeg_L";
        public const string LOWERLEG_L = "lowerLeg_L";
        public const string FOOT_L = "foot_L";
        public const string TOE_L = "toe_L";
        // Right leg chain
        public const string UPPERLEG_R = "upperLeg_R";
        public const string LOWERLEG_R = "lowerLeg_R";
        public const string FOOT_R = "foot_R";
        public const string TOE_R = "toe_R";
        //Spine chain
        public const string SPINE0 = "spine0";
        public const string SPINE1 = "spine1";
        public const string SPINE2 = "spine2";
        public const string SPINE3 = "spine3";
        public const string NECK = "neck";
        public const string HEAD = "head";
        //Left arm chain
        public const string SHOULDER_L = "shoulder_L";
        public const string UPPERARM_L = "upperArm_L";
        public const string LOWERARM_L = "lowerArm_L";
        public const string HAND_L = "hand_L";
        public const string FINGER_L = "finger_L";
        //Right arm chain
        public const string SHOULDER_R = "shoulder_R";
        public const string UPPERARM_R = "upperArm_R";
        public const string LOWERARM_R = "lowerArm_R";
        public const string HAND_R = "hand_R";
        public const string FINGER_R = "finger_R";
        #endregion

        public BipedSkeleton()
        {
            root = new TreeNode<Bone>(new Bone(PELVIS));
            #region bone structure
            {
                #region legs left
                TreeNode<Bone> upperlegleft = root.AddChild(new Bone(UPPERLEG_L));
                {
                    TreeNode<Bone> lowerlegleft = upperlegleft.AddChild(new Bone(LOWERLEG_L));
                    {
                        TreeNode<Bone> footleft = lowerlegleft.AddChild(new Bone(FOOT_L));
                        {
                            footleft.AddChild(new Bone(TOE_L));
                        }
                    }
                }
                #endregion
                #region legs right
                TreeNode<Bone> upperlegright = root.AddChild(new Bone(UPPERLEG_R));
                {
                    TreeNode<Bone> lowerlegright = upperlegright.AddChild(new Bone(LOWERLEG_R));
                    {
                        TreeNode<Bone> footright = lowerlegright.AddChild(new Bone(FOOT_R));
                        {
                             footright.AddChild(new Bone(TOE_R));
                        }
                    }
                }
                #endregion
                #region upper body
                #region spine and head
                TreeNode<Bone> spine0 = root.AddChild(new Bone(SPINE0));
                {
                    TreeNode<Bone> spine1 = spine0.AddChild(new Bone(SPINE1));
                    {
                    //     TreeNode<Bone> spine2 = spine1.AddChild(new Bone(SPINE2));
                    //     {
                    TreeNode<Bone> spine3 = spine1.AddChild(new Bone(SPINE3));
                    {
                        TreeNode<Bone> neck = spine3.AddChild(new Bone(NECK));
                        {
                            neck.AddChild(new Bone(HEAD));
                        }
                #endregion
                        #region arm left
                        TreeNode<Bone> shoulderleft = spine3.AddChild(new Bone(SHOULDER_L));
                        {
                            TreeNode<Bone> upperarmleft = shoulderleft.AddChild(new Bone(UPPERARM_L));
                            {
                                TreeNode<Bone> lowerarmleft = upperarmleft.AddChild(new Bone(LOWERARM_L));
                                {
                                    TreeNode<Bone> handLeft = lowerarmleft.AddChild(new Bone(HAND_L));
                                    {
                                         handLeft.AddChild(new Bone(FINGER_L));
                                    }
                                }
                            }
                        }
                        #endregion
                        #region arm right
                        TreeNode<Bone> shoulderRight = spine3.AddChild(new Bone(SHOULDER_R));
                        {
                            TreeNode<Bone> upperarmleft = shoulderRight.AddChild(new Bone(UPPERARM_R));
                            {
                                TreeNode<Bone> lowerarmleft = upperarmleft.AddChild(new Bone(LOWERARM_R));
                                {
                                    TreeNode<Bone> handLeft = lowerarmleft.AddChild(new Bone(HAND_R));
                                    {
                                        handLeft.AddChild(new Bone(FINGER_R));
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                          }
            //    }
                }
                #endregion
            }
            #endregion
        }
        public BipedSkeleton(TreeNode<Bone> pelvis)
        {
            root = pelvis;
        }
    }
}
