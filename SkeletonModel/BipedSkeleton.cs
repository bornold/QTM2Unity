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
            root = new TreeNode<Bone>(new Bone(PELVIS, 
                new Vector3(-0.003823973f, 0.8768474f, -0.513153f), new Quaternion(-0.03161663f, 0.9953802f, -0.02883431f, 0.08595051f)));
            #region bone structure
            {
                #region upper body 
                #region spine and head
                TreeNode<Bone> spine0 = root.AddChild(new Bone(SPINE0, new Vector3(-0.01663341f, 0.9617411f, -0.4820441f), new Quaternion(-0.056099f, 0.9937615f, -0.04236513f, 0.0865812f)));
                {
                    TreeNode<Bone> spine1 = spine0.AddChild(new Bone(SPINE1, new Vector3(-0.02889477f, 1.078292f, -0.4930993f), new Quaternion(0.01864406f, 0.9958602f, -0.007795251f, 0.0886244f)));
                    {
                    //     TreeNode<Bone> spine2 = spine1.AddChild(new Bone(SPINE2));
                    //     {
                        TreeNode<Bone> spine3 = spine1.AddChild(new Bone(SPINE3, new Vector3(-0.01626727f, 1.405879f, -0.4971061f), new Quaternion(0.01717401f, 0.9989401f, -0.02307167f, 0.03593515f)));
                    {
                        TreeNode<Bone> neck = spine3.AddChild(new Bone(NECK, new Vector3(-0.01447183f, 1.455711f, -0.4993453f), new Quaternion(-0.0005198121f, 0.9992219f, 0.01704936f, 0.03556345f)));
                        {
                            neck.AddChild(new Bone(HEAD, new Vector3(-0.0147269f, 1.568939f, -0.4954894f), new Quaternion(-0.01027027f, 0.991541f, 0.1226489f, 0.04121165f)));
                        }
                #endregion
                        #region arm left
                        TreeNode<Bone> shoulderleft = spine3.AddChild(new Bone(SHOULDER_L, new Vector3(-0.01626727f, 1.405879f, -0.4971061f), new Quaternion(0.7841284f, 0.6194713f, 0.009648148f, 0.03612403f)));
                        {
                            TreeNode<Bone> upperarmleft = shoulderleft.AddChild(new Bone(UPPERARM_L, new Vector3(0.1330705f, 1.370513f, -0.4865525f), new Quaternion(0.9811957f, 0.06779382f, -0.1090409f, 0.1441148f)));
                            {
                                TreeNode<Bone> lowerarmleft = upperarmleft.AddChild(new Bone(LOWERARM_L, new Vector3(0.1790416f, 1.105177f, -0.4116354f), new Quaternion(-0.8948241f, -0.1834709f, 0.3508964f, 0.2061552f)));
                                {
                                    TreeNode<Bone> handLeft = lowerarmleft.AddChild(new Bone(HAND_L, new Vector3(0.2253258f, 0.8915657f, -0.537055f), new Quaternion(-0.9102481f, -0.1362202f, 0.3528639f, 0.1684623f)));
                                    {
                                        handLeft.AddChild(new Bone(FINGER_L, new Vector3(0.2346929f, 0.8258195f, -0.5662824f), new Quaternion(0f, 0f, 0f, 0f)));
                                    }
                                }
                            }
                        }
                        #endregion
                        #region arm right
                        TreeNode<Bone> shoulderRight = spine3.AddChild(new Bone(SHOULDER_R, new Vector3(-0.01626727f, 1.405879f, -0.4971061f), new Quaternion(0.7779536f, -0.6264737f, 0f, 0.04815642f)));
                        {
                            TreeNode<Bone> upperarmleft = shoulderRight.AddChild(new Bone(UPPERARM_R, new Vector3(-0.2103209f, 1.365934f, -0.4823359f), new Quaternion(0.9916836f, 0.01800236f, 0.1035977f, 0.07420892f)));
                            {
                                TreeNode<Bone> lowerarmleft = upperarmleft.AddChild(new Bone(LOWERARM_R, new Vector3(-0.2047825f, 1.096681f, -0.4412225f), new Quaternion(-0.9450463f, 0.1278477f, -0.2100029f, 0.2155019f)));
                                {
                                    TreeNode<Bone> handLeft = lowerarmleft.AddChild(new Bone(HAND_R, new Vector3(-0.2424411f, 0.8787935f, -0.556097f), new Quaternion(-0.9354771f, 0.03534341f, -0.2250094f, 0.2701929f)));
                                    {
                                        handLeft.AddChild(new Bone(FINGER_R, new Vector3(-0.2384833f, 0.8180345f, -0.5933036f), new Quaternion(0f, 0f, 0f, 0f)));
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
                #region legs left
                TreeNode<Bone> upperlegleft = root.AddChild(new Bone(UPPERLEG_L,
                new Vector3(-8816423f, 0.8816423f, -0.5027573f), new Quaternion(-0.9931152f, 0.020120982f, -0.09769622f, 0.06142269f)));
                {

                    TreeNode<Bone> lowerlegleft = upperlegleft.AddChild(new Bone(LOWERLEG_L,
                new Vector3(-0.003823973f, 0.8768474f, -0.513153f), new Quaternion(-0.03161663f, 0.9953802f, -0.02883431f, 0.08595051f)));
                    {
                        TreeNode<Bone> footleft = lowerlegleft.AddChild(new Bone(FOOT_L,
                new Vector3(-0.003823973f, 0.8768474f, -0.513153f), new Quaternion(-0.03161663f, 0.9953802f, -0.02883431f, 0.08595051f)));
                        {
                            footleft.AddChild(new Bone(TOE_L, 
                new Vector3(-0.003823973f, 0.8768474f, -0.513153f), new Quaternion(-0.03161663f, 0.9953802f, -0.02883431f, 0.08595051f)));
                        }
                    }
                }
                #endregion
                #region legs right
                TreeNode<Bone> upperlegright = root.AddChild(new Bone(UPPERLEG_R, new Vector3(-0.07612621f, 0.8716684f, -0.5274489f), new Quaternion(0.9710155f, -0.04120144f, 0.2117789f, 0.1028632f)));
                {
                    TreeNode<Bone> lowerlegright = upperlegright.AddChild(new Bone(LOWERLEG_R, new Vector3(-0.1217456f, 0.5115939f, -0.4601502f), new Quaternion(-0.9894352f, 0.02428037f, -0.1322857f, 0.05412039f)));
                    {
                        TreeNode<Bone> footright = lowerlegright.AddChild(new Bone(FOOT_R, new Vector3(-0.1366511f, 0.0727815f, -0.5103177f), new Quaternion(-0.7891393f, -0.08197654f, -0.1741873f, 0.5832649f)));
                        {
                            footright.AddChild(new Bone(TOE_R, new Vector3(-0.1041819f, 0.04289091f, -0.5974027f), new Quaternion(0f, 0f, 0f, 0f)));
                        }
                    }
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
