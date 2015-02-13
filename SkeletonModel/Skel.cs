using System.Collections.Generic;
using System.Collections;
namespace QTM2Unity
{
    class Skel : IEnumerable
    {
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

        private TreeNode<Bon> root;
        public Skel()
        {
            #region bone structure
            root = new TreeNode<Bon>(new Bon(PELVIS));
            {
                #region legs left
                TreeNode<Bon> upperlegleft = root.AddChild(new Bon(UPPERLEG_L));
                {
                    TreeNode<Bon> lowerlegleft = upperlegleft.AddChild(new Bon(LOWERLEG_L));
                    {
                        TreeNode<Bon> footleft = lowerlegleft.AddChild(new Bon(FOOT_L));
                        {
                            //TreeNode<Bon> toeleft = footleft.AddChild(new Bon(TOE_L));
                        }
                    }
                }
                #endregion
                #region legs right
                TreeNode<Bon> upperlegright = root.AddChild(new Bon(UPPERLEG_R));
                {
                    TreeNode<Bon> lowerlegright = upperlegright.AddChild(new Bon(LOWERLEG_R));
                    {
                        TreeNode<Bon> footright = lowerlegright.AddChild(new Bon(FOOT_R));
                        {
                           // TreeNode<Bon> toeright = footright.AddChild(new Bon(TOE_R));
                        }
                    }
                }
                #endregion
                #region upper body
                #region spine and head
                TreeNode<Bon> spine0 = root.AddChild(new Bon(SPINE0));
                {
                   //  TreeNode<Bon> spine1 = spine0.AddChild(new Bon(SPINE1));
                   //  {
                   //     TreeNode<Bon> spine2 = spine1.AddChild(new Bon(SPINE2));
                   //     {
                           TreeNode<Bon> spine3 = spine0.AddChild(new Bon(SPINE3));
                           {
                                TreeNode<Bon> neck = spine3.AddChild(new Bon(NECK));
                                {
                                    TreeNode<Bon> head = neck.AddChild(new Bon(HEAD));
                                }
                                #endregion
                                #region arm left
                                TreeNode<Bon> shoulderleft = spine3.AddChild(new Bon(SHOULDER_L));
                                {
                                    TreeNode<Bon> upperarmleft = shoulderleft.AddChild(new Bon(UPPERARM_L));
                                    {
                                        TreeNode<Bon> lowerarmleft = upperarmleft.AddChild(new Bon(LOWERARM_L));
                                        {
                                            TreeNode<Bon> handLeft = lowerarmleft.AddChild(new Bon(HAND_L));
                                            {
                                               // TreeNode<Bon> fingerLeft = handLeft.AddChild(new Bon(FINGER_L));
                                            }
                                        }
                                    }
                                }
                                #endregion
                                #region arm right
                                TreeNode<Bon> shoulderRight = spine3.AddChild(new Bon(SHOULDER_R));
                                {
                                    TreeNode<Bon> upperarmleft = shoulderRight.AddChild(new Bon(UPPERARM_R));
                                    {
                                        TreeNode<Bon> lowerarmleft = upperarmleft.AddChild(new Bon(LOWERARM_R));
                                        {
                                            TreeNode<Bon> handLeft = lowerarmleft.AddChild(new Bon(HAND_R));
                                            {
                                                //TreeNode<Bon> fingerLeft = handLeft.AddChild(new Bon(FINGER_R));
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                 //      }
                //    }
                }
                #endregion
            }
            #endregion
        }

        public Bon this[string key]
        {
            get
            {
                return root.FindTreeNode(node => node.Data != null && node.Data.Name.Equals(key)).Data;
            }
            set
            {
                root.FindTreeNode(node => node.Data != null && node.Data.Name.Equals(key)).Data = value;
            }
        }
        public IEnumerator GetEnumerator()
        {
            return root.GetEnumerator();

        }
    }
}
