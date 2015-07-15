using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    class BipedSkeleton
    {
        protected TreeNode<Bone> root;
        public TreeNode<Bone> Root { get { return root; } }
        private ConstraintsExamples constraints = new ConstraintsExamples();
        // String constants for bones in a biped skeleton
        #region joint Names
        //Root
        public const string PELVIS = "pelvis";
        // Left leg chain
        public const string HIP_L = "hip_L";
        public const string KNEE_L = "knee_L";
        public const string ANKLE_L = "ankle_L";
        public const string FOOTBASE_L = "footBase_L";
        public const string TOE_L = "toe_L";

        // Right leg chain
        public const string HIP_R = "hip_R";
        public const string KNEE_R = "knee_R";
        public const string ANKLE_R = "ankle_R";
        public const string FOOTBASE_R = "footBase_R";
        public const string TOE_R = "toe_R";

        //Spine chain
        public const string SPINE0 = "spine0";
        public const string SPINE1 = "spine1";
        public const string SPINE2 = "spine2";
        public const string SPINE3 = "spine3";
        public const string NECK = "neck";
        public const string HEAD = "head";
        public const string HEADTOP = "headtop";
        //Left arm chain
        public const string CLAVICLE_L = "clavicle_L";
        public const string SHOULDER_L = "shoulder_L";
        public const string ELBOW_L = "elbow_L";
        public const string WRIST_L = "wrist_L";
        public const string TRAP_L = "trap_L";
        public const string THUMB_L = "thumb_L";
        public const string HAND_L = "hand_L";
        public const string INDEX_L = "index_L";

        //Right arm chain
        public const string CLAVICLE_R = "clavicle_R";
        public const string SHOULDER_R = "shoulder_R";
        public const string ELBOW_R = "elbow_R";
        public const string WRIST_R = "wrist_R";
        public const string TRAP_R = "trap_R";
        public const string THUMB_R = "thumb_R";
        public const string HAND_R = "hand_R";
        public const string INDEX_R = "index_R";

        #endregion
        public BipedSkeleton()
        {

            #region JointPos
            Vector3 spine0Pos = new Vector3(0.0028f, 1.0568f, -0.0776f);
            Vector3 spine1Pos = new Vector3(0.0051f, 1.2278f, -0.0808f);
            Vector3 spine3Pos = new Vector3(0.0063f, 1.4761f, -0.0484f);
            Vector3 neckPos = new Vector3(0.0066f, 1.5132f, -0.0301f);
            Vector3 headPos = new Vector3(0.0044f, 1.6209f, 0.0236f);
            Vector3 headTopPos = new Vector3(0.0050f, 1.7504f, 0.0055f);

            Vector3 shoulderLeftPos = 
                new Vector3(-0.1907f, 1.4407f, -0.0325f);
            Vector3 elbowLeftPos = 
                new Vector3(-0.1949f, 1.1388f, -0.0620f);
            Vector3 wristLeftPos =
                new Vector3(-0.1959f, 0.8694f, -0.0521f);
            Vector3 handLeftPos = 
                new Vector3(-0.1961f, 0.8055f, -0.0218f);
            Vector3 indexLeftPos =
                new Vector3(-0.1945f, 0.7169f, -0.0173f);
            Vector3 thumbLeftPos =
                new Vector3(-0.1864f, 0.8190f, 0.0506f);

            Vector3 shoulderRightPos = 
                new Vector3(0.2029f, 1.4376f, -0.0387f);
            Vector3 elbowRightPos =
                new Vector3(0.2014f, 1.1357f, -0.0682f);
            Vector3 wristRightPos = 
                new Vector3(0.1984f, 0.8663f, -0.0583f);
            Vector3 handRightPos = 
                new Vector3(0.1983f, 0.8024f, -0.0280f);
            Vector3 indexRightPos =
                new Vector3(0.2028f, 0.7139f, -0.0236f);
            Vector3 thumbRightPos =
                new Vector3(0.1955f, 0.8159f, 0.0464f);

            Vector3 hipLeftPos = 
                new Vector3(-0.0950f, 0.9171f, 0.0029f);
            Vector3 kneeLeftPos = 
                new Vector3(-0.0867f, 0.4913f, 0.0318f);
            Vector3 ankleLeftPos = 
                new Vector3(-0.0801f, 0.0712f, -0.0766f);
            Vector3 toeLeftPos = 
                new Vector3(-0.0801f, 0.0039f, 0.0732f);
            Vector3 footBaseLeftPos =
                Vector3Helper.MidPoint(toeLeftPos, new Vector3(-0.0692f, 0.0297f, -0.1221f));

            Vector3 hipRightPos = 
                new Vector3(0.0961f, 0.9124f, -0.0001f);
            Vector3 kneeRightPos = 
                new Vector3(0.1040f, 0.4867f, 0.0308f);
            Vector3 ankleRightPos =
                new Vector3(0.1101f, 0.0656f, -0.0736f);
            Vector3 toeRightPos = 
                new Vector3(0.1086f, 0.0000f, 0.0762f);
            Vector3 footBaseRightPos =
                Vector3Helper.MidPoint(toeRightPos, new Vector3(0.0974f, 0.0259f, -0.1171f));

            Vector3 pelvisPos = Vector3Helper.MidPoint(hipLeftPos, hipRightPos); //new Vector3(0f, 0.8240f, 0.0277f);
            #endregion JointPos
            #region JointRot
            Quaternion spine0Rot = QuaternionHelper.LookAtRight(spine0Pos, spine1Pos, -Vector3.UnitX);
            Quaternion spine3Rot = QuaternionHelper.LookAtRight(spine1Pos, spine3Pos, -Vector3.UnitX);
            Quaternion spine1Rot = QuaternionHelper.LookAtRight(spine3Pos, neckPos, -Vector3.UnitX);
            Quaternion neckRot = QuaternionHelper.LookAtRight(neckPos, headPos, -Vector3.UnitX);
            Quaternion headRot = QuaternionHelper.LookAtRight(headPos, headTopPos, -Vector3.UnitX);

            Quaternion clavicleLeftRot = QuaternionHelper.LookAtUp(spine3Pos, shoulderLeftPos, Vector3.UnitZ);
            Quaternion shoulderLeftRot = QuaternionHelper.LookAtRight(shoulderLeftPos, elbowLeftPos, Vector3.UnitX);
            Quaternion elbowLeftRot = QuaternionHelper.LookAtRight(elbowLeftPos, wristLeftPos, Vector3.UnitX); ;
            Quaternion wristLeftRot = QuaternionHelper.LookAtRight(wristLeftPos, handLeftPos, Vector3.UnitX);
            Quaternion trapezoidLeftRot = QuaternionHelper.LookAtRight(wristLeftPos, thumbLeftPos, Vector3.UnitX);
            Quaternion handLeftRot = QuaternionHelper.LookAtRight(handLeftPos, indexLeftPos, Vector3.UnitX);


            Quaternion clavicleRightRot = QuaternionHelper.LookAtUp(spine3Pos, shoulderRightPos, Vector3.UnitZ);
            Quaternion shoulderRightRot = QuaternionHelper.LookAtRight(shoulderRightPos, elbowRightPos, Vector3.UnitX);
            Quaternion elbowRightRot = QuaternionHelper.LookAtRight(elbowRightPos, wristRightPos, Vector3.UnitX);
            Quaternion wristRightRot = QuaternionHelper.LookAtRight(wristRightPos, handRightPos, Vector3.UnitX);
            Quaternion trapezoidRightRot = QuaternionHelper.LookAtRight(wristRightPos, thumbRightPos, Vector3.UnitX);
            Quaternion handRightRot = QuaternionHelper.LookAtRight(handRightPos, indexRightPos, Vector3.UnitX);

            Quaternion hipLeftRot = QuaternionHelper.LookAtRight(hipLeftPos, kneeLeftPos, Vector3.UnitX);
            Quaternion kneeLeftRot = QuaternionHelper.LookAtRight(kneeLeftPos, ankleLeftPos, Vector3.UnitX);
            Quaternion ankleLeftRot = QuaternionHelper.LookAtUp(ankleLeftPos, toeLeftPos, (kneeLeftPos - ankleLeftPos));
            Quaternion footBaseLeftRot = QuaternionHelper.LookAtUp(footBaseLeftPos, toeLeftPos, (ankleLeftPos - footBaseLeftPos));

            Quaternion hipRightRot = QuaternionHelper.LookAtRight(hipRightPos, kneeRightPos, Vector3.UnitX);
            Quaternion kneeRightRot = QuaternionHelper.LookAtRight(kneeRightPos, ankleRightPos, Vector3.UnitX);
            Quaternion ankleRightRot = QuaternionHelper.LookAtUp(ankleRightPos, toeRightPos, (kneeRightPos - ankleRightPos));
            Quaternion footBaseRightRot = QuaternionHelper.LookAtUp(footBaseRightPos, toeRightPos, (ankleRightPos - footBaseRightPos));

            #endregion

            root = new TreeNode<Bone>(new Bone(PELVIS,
                pelvisPos, Quaternion.Identity));
            #region bone structure
            {
                #region upper body 
                #region spine and head
                TreeNode<Bone> spine0 = root.AddChild(new Bone(SPINE0, 
                    spine0Pos, 
                    spine0Rot));
                {
                    TreeNode<Bone> spine1 = spine0.AddChild(new Bone(SPINE1, 
                        spine1Pos,
                        spine1Rot));
                    {
                        TreeNode<Bone> spine3 = spine1.AddChild(new Bone(SPINE3, 
                            spine3Pos,
                            spine3Rot));
                    {
                        TreeNode<Bone> neck = spine3.AddChild(new Bone(NECK, 
                            neckPos, 
                            neckRot));
                        {
                            TreeNode<Bone> head = neck.AddChild(new Bone(HEAD, 
                                headPos, 
                                headRot));
                            {
                                head.AddChild(new Bone(HEADTOP, headTopPos, QuaternionHelper.Zero));
                            }
                        }
                #endregion
                        #region arm left
                        TreeNode<Bone> clavicleLeft = spine3.AddChild(new Bone(CLAVICLE_L, 
                            spine3Pos,
                            clavicleLeftRot));
                        {
                            TreeNode<Bone> shoulderLeft = clavicleLeft.AddChild(new Bone(SHOULDER_L,
                                shoulderLeftPos, 
                                shoulderLeftRot));
                            {
                                TreeNode<Bone> elbowLeft = shoulderLeft.AddChild(new Bone(ELBOW_L,
                                    elbowLeftPos, 
                                    elbowLeftRot));
                                {
                                    TreeNode<Bone> wristLeft = elbowLeft.AddChild(new Bone(WRIST_L, 
                                        wristLeftPos,
                                        wristLeftRot));
                                    {
                                        TreeNode<Bone> handLeft = wristLeft.AddChild(new Bone(HAND_L,
                                           handLeftPos,
                                           handLeftRot));
                                        {
                                            handLeft.AddChild(new Bone(INDEX_L,
                                           indexLeftPos,
                                           QuaternionHelper.Zero));
                                        }
                                        TreeNode<Bone> trapezoidLeft = wristLeft.AddChild(new Bone(TRAP_L,
                                                wristLeftPos,
                                                trapezoidLeftRot));
                                        {
                                            trapezoidLeft.AddChild(new Bone(THUMB_L,
                                            thumbLeftPos,
                                            QuaternionHelper.Zero));
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region arm right
                        TreeNode<Bone> clavicleRight = spine3.AddChild(new Bone(CLAVICLE_R, 
                            spine3Pos, 
                            clavicleRightRot));
                        {
                            TreeNode<Bone> shoulderRight = clavicleRight.AddChild(new Bone(SHOULDER_R,
                                shoulderRightPos, 
                                shoulderRightRot));
                            {
                                TreeNode<Bone> elbowRight = shoulderRight.AddChild(new Bone(ELBOW_R,
                                    elbowRightPos, 
                                    elbowRightRot));
                                {
                                    TreeNode<Bone> wristRight = elbowRight.AddChild(new Bone(WRIST_R, 
                                        wristRightPos, 
                                        wristRightRot));    
                                    {
                                        TreeNode<Bone> handRight = wristRight.AddChild(new Bone(HAND_R, 
                                            handRightPos,
                                            handRightRot));
                                        {
                                            handRight.AddChild(new Bone(INDEX_R,
                                            indexRightPos,
                                            QuaternionHelper.Zero));
                                        TreeNode<Bone> trapezoidRight = wristRight.AddChild(new Bone(TRAP_R,
                                            wristRightPos,
                                            trapezoidRightRot));
                                        {
                                            trapezoidRight.AddChild(new Bone(THUMB_R,
                                            thumbRightPos,
                                            QuaternionHelper.Zero));
                                        }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                }
                #endregion
                #region legs left
                TreeNode<Bone> hipLeft = root.AddChild(new Bone(HIP_L,
                hipLeftPos, 
                hipLeftRot));
                {

                    TreeNode<Bone> kneeLeft = hipLeft.AddChild(new Bone(KNEE_L,
                        kneeLeftPos, 
                        kneeLeftRot));
                    {
                        TreeNode<Bone> ankleLeft = kneeLeft.AddChild(new Bone(ANKLE_L,
                            ankleLeftPos, 
                            ankleLeftRot));
                        {
                            TreeNode<Bone> footBaseLeft = ankleLeft.AddChild(new Bone(FOOTBASE_L, footBaseLeftPos, footBaseLeftRot));
                            {
                                footBaseLeft.AddChild(new Bone(TOE_L, toeLeftPos, QuaternionHelper.Zero));
                            }
                        }
                    }
                }
                #endregion
                #region legs right
                TreeNode<Bone> hipRight = root.AddChild(new Bone(HIP_R, 
                   hipRightPos, 
                    hipRightRot));
                {
                    TreeNode<Bone> kneeRight = hipRight.AddChild(new Bone(KNEE_R,
                           kneeRightPos, 
                            kneeRightRot));
                    {
                        TreeNode<Bone> ankleRight = kneeRight.AddChild(new Bone(ANKLE_R,
                            ankleRightPos, 
                            ankleRightRot));
                        {
                            TreeNode<Bone> footBaseRight = ankleRight.AddChild(new Bone(FOOTBASE_R, footBaseRightPos, footBaseRightRot));
                            {
                                footBaseRight.AddChild(new Bone(TOE_R, toeRightPos, QuaternionHelper.Zero));
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion
            constraints.SetConstraints(this);
        }
        public BipedSkeleton(TreeNode<Bone> pelvis)
        {
            root = pelvis;
        }
        public Bone Find(string key)
        {
            foreach (var b in root)
                if (b.Data.Name == key) return b.Data;
            return null;
        }
        public Bone this[string key]
        {
            get
            {
                return root.FindTreeNode(node => node.Data != null && node.Data.Name.Equals(key)).Data;
            }
            set
            {
                root.FindTreeNode(node => node.Data.Name.Equals(key)).Data = value;
            }
        }
    }
}
