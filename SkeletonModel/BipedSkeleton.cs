#region --- LINCENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Jonas Bornold

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
using OpenTK;

namespace QTM2Unity
{
    /// <summary>
    /// Joint identifiers
    /// </summary>
    public enum Joint
    {
        PELVIS,
        // Left leg chain
        HIP_L,
        KNEE_L,
        ANKLE_L,
        FOOTBASE_L,
        TOE_L,
        // Right leg chain
        HIP_R,
        KNEE_R,
        ANKLE_R,
        FOOTBASE_R,
        TOE_R,
        //Spine chain
        SPINE0,
        SPINE1,
        SPINE2,
        SPINE3,
        NECK,
        HEAD,
        HEADTOP,
        //Left arm chain
        CLAVICLE_L,
        SHOULDER_L,
        ELBOW_L,
        WRIST_L,
        TRAP_L,
        THUMB_L,
        HAND_L,
        INDEX_L,
        //Right arm chain
        CLAVICLE_R,
        SHOULDER_R,
        ELBOW_R,
        WRIST_R,
        TRAP_R,
        THUMB_R,
        HAND_R,
        INDEX_R
    };
    class BipedSkeleton
    {
        protected TreeNode<Bone> root;
        public TreeNode<Bone> Root { get { return root; } }
        private ConstraintsExamples constraints = new ConstraintsExamples();
        /// <summary>
        /// Constructur returns a skeleton in standrad ISO/IEC FCD 19774 specification
        /// http://h-anim.org/Specifications/H-Anim200x/ISO_IEC_FCD_19774/
        /// </summary>
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
            Quaternion spine0Rot = QuaternionHelper2.LookAtRight(spine0Pos, spine1Pos, -Vector3.UnitX);
            Quaternion spine3Rot = QuaternionHelper2.LookAtRight(spine1Pos, spine3Pos, -Vector3.UnitX);
            Quaternion spine1Rot = QuaternionHelper2.LookAtRight(spine3Pos, neckPos, -Vector3.UnitX);
            Quaternion neckRot = QuaternionHelper2.LookAtRight(neckPos, headPos, -Vector3.UnitX);
            Quaternion headRot = QuaternionHelper2.LookAtRight(headPos, headTopPos, -Vector3.UnitX);

            Quaternion clavicleLeftRot = QuaternionHelper2.LookAtUp(spine3Pos, shoulderLeftPos, Vector3.UnitZ);
            Quaternion shoulderLeftRot = QuaternionHelper2.LookAtRight(shoulderLeftPos, elbowLeftPos, Vector3.UnitX);
            Quaternion elbowLeftRot = QuaternionHelper2.LookAtRight(elbowLeftPos, wristLeftPos, Vector3.UnitX); ;
            Quaternion wristLeftRot = QuaternionHelper2.LookAtRight(wristLeftPos, handLeftPos, Vector3.UnitX);
            Quaternion trapezoidLeftRot = QuaternionHelper2.LookAtRight(wristLeftPos, thumbLeftPos, Vector3.UnitX);
            Quaternion handLeftRot = QuaternionHelper2.LookAtRight(handLeftPos, indexLeftPos, Vector3.UnitX);


            Quaternion clavicleRightRot = QuaternionHelper2.LookAtUp(spine3Pos, shoulderRightPos, Vector3.UnitZ);
            Quaternion shoulderRightRot = QuaternionHelper2.LookAtRight(shoulderRightPos, elbowRightPos, Vector3.UnitX);
            Quaternion elbowRightRot = QuaternionHelper2.LookAtRight(elbowRightPos, wristRightPos, Vector3.UnitX);
            Quaternion wristRightRot = QuaternionHelper2.LookAtRight(wristRightPos, handRightPos, Vector3.UnitX);
            Quaternion trapezoidRightRot = QuaternionHelper2.LookAtRight(wristRightPos, thumbRightPos, Vector3.UnitX);
            Quaternion handRightRot = QuaternionHelper2.LookAtRight(handRightPos, indexRightPos, Vector3.UnitX);

            Quaternion hipLeftRot = QuaternionHelper2.LookAtRight(hipLeftPos, kneeLeftPos, Vector3.UnitX);
            Quaternion kneeLeftRot = QuaternionHelper2.LookAtRight(kneeLeftPos, ankleLeftPos, Vector3.UnitX);
            Quaternion ankleLeftRot = QuaternionHelper2.LookAtUp(ankleLeftPos, toeLeftPos, (kneeLeftPos - ankleLeftPos));
            Quaternion footBaseLeftRot = QuaternionHelper2.LookAtUp(footBaseLeftPos, toeLeftPos, (ankleLeftPos - footBaseLeftPos));

            Quaternion hipRightRot = QuaternionHelper2.LookAtRight(hipRightPos, kneeRightPos, Vector3.UnitX);
            Quaternion kneeRightRot = QuaternionHelper2.LookAtRight(kneeRightPos, ankleRightPos, Vector3.UnitX);
            Quaternion ankleRightRot = QuaternionHelper2.LookAtUp(ankleRightPos, toeRightPos, (kneeRightPos - ankleRightPos));
            Quaternion footBaseRightRot = QuaternionHelper2.LookAtUp(footBaseRightPos, toeRightPos, (ankleRightPos - footBaseRightPos));

            #endregion

            root = new TreeNode<Bone>(new Bone(Joint.PELVIS,
                pelvisPos, Quaternion.Identity));
            #region bone structure
            {
                #region upper body 
                #region spine and head
                TreeNode<Bone> spine0 = root.AddChild(new Bone(Joint.SPINE0, 
                    spine0Pos, 
                    spine0Rot));
                {
                    TreeNode<Bone> spine1 = spine0.AddChild(new Bone(Joint.SPINE1, 
                        spine1Pos,
                        spine1Rot));
                    {
                        TreeNode<Bone> spine3 = spine1.AddChild(new Bone(Joint.SPINE3, 
                            spine3Pos,
                            spine3Rot));
                    {
                        TreeNode<Bone> neck = spine3.AddChild(new Bone(Joint.NECK, 
                            neckPos, 
                            neckRot));
                        {
                            TreeNode<Bone> head = neck.AddChild(new Bone(Joint.HEAD, 
                                headPos, 
                                headRot));
                            {
                                head.AddChild(new Bone(Joint.HEADTOP, headTopPos, QuaternionHelper2.Zero));
                            }
                        }
                #endregion
                        #region arm left
                        TreeNode<Bone> clavicleLeft = spine3.AddChild(new Bone(Joint.CLAVICLE_L, 
                            spine3Pos,
                            clavicleLeftRot));
                        {
                            TreeNode<Bone> shoulderLeft = clavicleLeft.AddChild(new Bone(Joint.SHOULDER_L,
                                shoulderLeftPos, 
                                shoulderLeftRot));
                            {
                                TreeNode<Bone> elbowLeft = shoulderLeft.AddChild(new Bone(Joint.ELBOW_L,
                                    elbowLeftPos, 
                                    elbowLeftRot));
                                {
                                    TreeNode<Bone> wristLeft = elbowLeft.AddChild(new Bone(Joint.WRIST_L, 
                                        wristLeftPos,
                                        wristLeftRot));
                                    {
                                        TreeNode<Bone> handLeft = wristLeft.AddChild(new Bone(Joint.HAND_L,
                                           handLeftPos,
                                           handLeftRot));
                                        {
                                            handLeft.AddChild(new Bone(Joint.INDEX_L,
                                           indexLeftPos,
                                           QuaternionHelper2.Zero));
                                        }
                                        TreeNode<Bone> trapezoidLeft = wristLeft.AddChild(new Bone(Joint.TRAP_L,
                                                wristLeftPos,
                                                trapezoidLeftRot));
                                        {
                                            trapezoidLeft.AddChild(new Bone(Joint.THUMB_L,
                                            thumbLeftPos,
                                            QuaternionHelper2.Zero));
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region arm right
                        TreeNode<Bone> clavicleRight = spine3.AddChild(new Bone(Joint.CLAVICLE_R, 
                            spine3Pos, 
                            clavicleRightRot));
                        {
                            TreeNode<Bone> shoulderRight = clavicleRight.AddChild(new Bone(Joint.SHOULDER_R,
                                shoulderRightPos, 
                                shoulderRightRot));
                            {
                                TreeNode<Bone> elbowRight = shoulderRight.AddChild(new Bone(Joint.ELBOW_R,
                                    elbowRightPos, 
                                    elbowRightRot));
                                {
                                    TreeNode<Bone> wristRight = elbowRight.AddChild(new Bone(Joint.WRIST_R, 
                                        wristRightPos, 
                                        wristRightRot));    
                                    {
                                        TreeNode<Bone> handRight = wristRight.AddChild(new Bone(Joint.HAND_R, 
                                            handRightPos,
                                            handRightRot));
                                        {
                                            handRight.AddChild(new Bone(Joint.INDEX_R,
                                            indexRightPos,
                                            QuaternionHelper2.Zero));
                                        TreeNode<Bone> trapezoidRight = wristRight.AddChild(new Bone(Joint.TRAP_R,
                                            wristRightPos,
                                            trapezoidRightRot));
                                        {
                                            trapezoidRight.AddChild(new Bone(Joint.THUMB_R,
                                            thumbRightPos,
                                            QuaternionHelper2.Zero));
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
                TreeNode<Bone> hipLeft = root.AddChild(new Bone(Joint.HIP_L,
                hipLeftPos, 
                hipLeftRot));
                {

                    TreeNode<Bone> kneeLeft = hipLeft.AddChild(new Bone(Joint.KNEE_L,
                        kneeLeftPos, 
                        kneeLeftRot));
                    {
                        TreeNode<Bone> ankleLeft = kneeLeft.AddChild(new Bone(Joint.ANKLE_L,
                            ankleLeftPos, 
                            ankleLeftRot));
                        {
                            TreeNode<Bone> footBaseLeft = ankleLeft.AddChild(new Bone(Joint.FOOTBASE_L, footBaseLeftPos, footBaseLeftRot));
                            {
                                footBaseLeft.AddChild(new Bone(Joint.TOE_L, toeLeftPos, QuaternionHelper2.Zero));
                            }
                        }
                    }
                }
                #endregion
                #region legs right
                TreeNode<Bone> hipRight = root.AddChild(new Bone(Joint.HIP_R, 
                   hipRightPos, 
                    hipRightRot));
                {
                    TreeNode<Bone> kneeRight = hipRight.AddChild(new Bone(Joint.KNEE_R,
                           kneeRightPos, 
                            kneeRightRot));
                    {
                        TreeNode<Bone> ankleRight = kneeRight.AddChild(new Bone(Joint.ANKLE_R,
                            ankleRightPos, 
                            ankleRightRot));
                        {
                            TreeNode<Bone> footBaseRight = ankleRight.AddChild(new Bone(Joint.FOOTBASE_R, footBaseRightPos, footBaseRightRot));
                            {
                                footBaseRight.AddChild(new Bone(Joint.TOE_R, toeRightPos, QuaternionHelper2.Zero));
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
        public Bone Find(Joint key)
        {
            foreach (var b in root)
                if (b.Data.Name == key) return b.Data;
            return null;
        }
        public Bone this[Joint key]
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
