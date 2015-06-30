using System.Collections.Generic;
using OpenTK;
using System.Linq;
using System.Collections;
using System;
namespace QTM2Unity
{
    class Values
    {
        public Quaternion hipOrientation;
        public Quaternion chestOrientation;
        public Quaternion headOrientation;
        public Vector3 hipForward;
        public Vector3 chestForward;
        public Vector3 upperArmForwardLeft;
        public Vector3 upperArmForwardRight;
        public Vector3 lowerArmForwardLeft;
        public Vector3 lowerArmForwardRight;
        public Vector3 kneeForwardLeft;
        public Vector3 kneeForwardRight;
        public Vector3 rightHipPos;
        public Vector3 leftHipPos;
        public Vector3 sternumClacicle;
        public Vector3 spine1;
        public Vector3 head;
        public Vector3 shoulderRight;
        public Vector3 shoulderLeft;
        public Vector3 elbowRight;
        public Vector3 elbowLeft;
        public Vector3 handRight;
        public Vector3 handLeft;
        public Vector3 ankleRight;
        public Vector3 ankleLeft;
        public Vector3 kneeRight;
        public Vector3 kneeLeft;
    }
    class JointLocalization
    {
        private Values o = new Values();

        // Contains all functions for finding joint position
        private List<Action<Bone>> jointFunctions;
        private Dictionary<string, Vector3> markers;
        #region varible necessary to estimate joints
        private BodyData bd = new BodyData();
        private Quaternion prevChestOri = Quaternion.Identity;
        #endregion
        public JointLocalization()
        {
            jointFunctions = new List<Action<Bone>>() {
                    (b) => Plevis(b), 
                    (b) => SpineRoot(b), 
                    (b) => MidSpine(b),
                    (b) => SpineEnd(b),      
                    (b) => Neck(b),
                    (b) => GetHead(b),
                    (b) => GetHeadTop(b),
                    (b) => GetShoulderLeft(b),  
                    (b) => GetUpperArmLeft(b),
                    (b) => GetLowerArmLeft(b),
                    (b) => GetWristLeft(b),
                    (b) => GetHandLeft(b),
                    (b) => GetShoulderRight(b), 
                    (b) => GetUpperArmRight(b),
                    (b) => GetLowerArmRight(b),
                    (b) => GetWristRight(b),
                    (b) => GetHandRight(b),
                    (b) => UpperLegLeft(b),       
                    (b) => LowerLegLeft(b),
                    (b) => GetAnkleLeft(b),
                    (b) => GetFootLeft(b),
                    (b) => GetHeelLeft(b),
                    (b) => UpperLegRight(b),
                    (b) => LowerLegRight(b), 
                    (b) => GetAnkleRight(b),    
                    (b) => GetFootRight(b),
                    (b) => GetHeelRight(b),
                };
        }
        public void GetJointLocation(Dictionary<string, Vector3> markerData, ref BipedSkeleton skeleton)
        {
            o = new Values(); // reset joint pos and orientations
            markers = markerData;
            // collect data from markers about body proportions
            // this is necessary for shoulder joint localization 
            // Locate hiporientation, hip orientation is important for IK solver,
            bd.CalculateBodyData(markers, ChestOrientation);
            // get all joints
            IEnumerator it = skeleton.GetEnumerator();
            it.MoveNext();
            foreach (var action in jointFunctions)
            {
                action(((TreeNode<Bone>)it.Current).Data);
                it.MoveNext();
            }
        }
        #region Getters and Setters used for joint localization
        private Quaternion HipOrientation
        {
            get
            {
                if (o.hipOrientation == QuaternionHelper.Zero)
                {
                    o.hipOrientation = QuaternionHelper.GetHipOrientation(
                        markers[MarkerNames.bodyBase], 
                        markers[MarkerNames.leftHip], 
                        markers[MarkerNames.rightHip]);
                }
                return o.hipOrientation;
            }
        }
        private Quaternion ChestOrientation
        {
            get
            {
                if (o.chestOrientation == QuaternionHelper.Zero)
                {
                    Vector3 neckPos = markers[MarkerNames.neck];
                    Vector3 chestPos = markers[MarkerNames.chest];
                    Vector3 rightShoulderPos = markers[MarkerNames.rightShoulder];
                    Vector3 leftShoulderPos = markers[MarkerNames.leftShoulder];
                    Vector3 backspinePos = markers[MarkerNames.spine];
                    Vector3 Yaxis, Xaxis;
                    Vector3 mid = Vector3Helper.MidPoint(rightShoulderPos, leftShoulderPos);
                    Quaternion rotation;
                    bool sleart = true;// mid.IsNaN() || leftShoulderPos.IsNaN() || rightShoulderPos.IsNaN();
                    // Find Y axis
                    if (!mid.IsNaN())
                    {
                        Yaxis = mid - markers[MarkerNames.bodyBase];
                        sleart = false;
                    }
                    else if (!backspinePos.IsNaN() && !neckPos.IsNaN()) // prio 1, 12th Thoracic to 2nd Thoracic
                    {
                        Yaxis = Vector3Helper.MidPoint(neckPos, backspinePos) - markers[MarkerNames.bodyBase];
                    }
                    else if (!neckPos.IsNaN()) // prio 2, Sacrum to 2nd Thoracic
                    {
                        Yaxis = neckPos - markers[MarkerNames.bodyBase];
                    }
                    else if (!backspinePos.IsNaN()) // prio 3, Sacrum to 12th Thoracic
                    {
                        Yaxis = backspinePos - markers[MarkerNames.bodyBase];
                    }
                    else // last resort, use hip orientation
                    {
                        Yaxis = Vector3.Transform(Vector3.UnitY, prevChestOri);
                    }

                    if (!rightShoulderPos.IsNaN() || !leftShoulderPos.IsNaN())
                    {
                        if (!rightShoulderPos.IsNaN() && !leftShoulderPos.IsNaN()) // prio 1, use left and right scapula
                        {
                            Xaxis = leftShoulderPos - rightShoulderPos;
                        }
                        else if (!chestPos.IsNaN() && !neckPos.IsNaN())
                        {
                            mid = Vector3Helper.MidPoint(chestPos, neckPos);
                            if (!rightShoulderPos.IsNaN()) // prio 2, use right scapula and mid of Sternum and 2nd Thoracic
                            {
                                Xaxis = mid - rightShoulderPos;
                            }
                            else // prio 3, use left scapula and mid of Sternum and 2nd Thoracic
                            {
                                Xaxis = leftShoulderPos - mid;
                            }
                        }
                        else
                        {
                            //Quaternion.Slerp(prevChestOri, HipOrientation, 0.5f);
                            Xaxis = -Vector3.Transform(Vector3.UnitX, Quaternion.Slerp(prevChestOri, HipOrientation, 0.5f));
                        }
                    }
                    else // last resort, use hip prev orientation
                    {
                        Xaxis = -Vector3.Transform(Vector3.UnitX, prevChestOri);
                    }
                    rotation = sleart ? 
                        Quaternion.Slerp(QuaternionHelper.GetOrientationFromYX(Yaxis, Xaxis), prevChestOri, 0.8f) : 
                        QuaternionHelper.GetOrientationFromYX(Yaxis, Xaxis);
                    prevChestOri = rotation;
                    o.chestOrientation = rotation;
                }
                return o.chestOrientation;
            }
        }
        private Quaternion HeadOrientation
        {
            get
            {
                if (o.headOrientation == QuaternionHelper.Zero)
                {
                    o.headOrientation = QuaternionHelper.GetOrientation(markers[MarkerNames.head], markers[MarkerNames.leftHead], markers[MarkerNames.rightHead]);
                }
                return o.headOrientation;
            }
        }
        private Vector3 HipForward
        {
            get
            {
                if (o.hipForward == Vector3.Zero)
                    o.hipForward = Vector3.Transform(Vector3.UnitZ, HipOrientation);
                return o.hipForward;
            }
            set { o.hipForward = value; }
        }
        private Vector3 ChestForward
        {
            get
            {
                if (o.chestForward == Vector3.Zero)
                {
                    o.chestForward = Vector3.Transform(Vector3.UnitZ, ChestOrientation);
                }
                return o.chestForward;
            }
            set { o.chestForward = value; }
        }
        private Vector3 UpperArmForwardLeft
        {
            get
            {
                if (o.upperArmForwardLeft == Vector3.Zero)
                {

                    Vector3 midPoint = Vector3Helper.MidPoint(markers[MarkerNames.leftInnerElbow], markers[MarkerNames.leftOuterElbow]);
                    o.upperArmForwardLeft = Vector3.NormalizeFast(midPoint - markers[MarkerNames.leftElbow]);
                }
                return o.upperArmForwardLeft;
            }
            set { o.upperArmForwardLeft = value; }
        }
        private Vector3 UpperArmForwardRight
        {
            get
            {
                if (o.upperArmForwardRight == Vector3.Zero)
                {
                    Vector3 midPoint = Vector3Helper.MidPoint(markers[MarkerNames.rightInnerElbow], markers[MarkerNames.rightOuterElbow]);
                    o.upperArmForwardRight = Vector3.Normalize(midPoint - markers[MarkerNames.rightElbow]);
                }
                return o.upperArmForwardRight;
            }
            set { o.upperArmForwardRight = value; }
        }
        private Vector3 LowerArmForwardLeft
        {
            get
            {
                if (o.lowerArmForwardLeft == Vector3.Zero)
                {
                    o.lowerArmForwardLeft = Vector3.NormalizeFast(markers[MarkerNames.leftWristRadius] - markers[MarkerNames.leftWrist]);
                }
                return o.lowerArmForwardLeft;
            }
            set { o.lowerArmForwardLeft = value; }
        }
        private Vector3 LowerArmForwardRight
        {
            get
            {
                if (o.lowerArmForwardRight == Vector3.Zero)
                {
                    o.lowerArmForwardRight = Vector3.NormalizeFast(markers[MarkerNames.rightWristRadius] - markers[MarkerNames.rightWrist]);
                }
                return o.lowerArmForwardRight;
            }
            set { o.lowerArmForwardRight = value; }
        }
        private Vector3 KneeForwardLeft
        {
            get
            {
                if (o.kneeForwardLeft == Vector3.Zero)
                {
                    Vector3 knee = KneeLeft;
                    Vector3 kneeOuter = markers[MarkerNames.leftOuterKnee];
                    o.kneeForwardLeft = knee - kneeOuter;
                }
                return o.kneeForwardLeft;
            }
            set { o.kneeForwardLeft = value; }
        }
        private Vector3 KneeForwardRight
        {
            get
            {
                if (o.kneeForwardRight == Vector3.Zero)
                {
                    Vector3 knee = KneeRight;
                    Vector3 kneeOuter = markers[MarkerNames.rightOuterKnee];
                    o.kneeForwardRight = kneeOuter - knee;
                    //kneeForwardRight = GetKneeForwardRight();
                }
                return o.kneeForwardRight;
            }
            set { o.kneeForwardRight = value; }
        }
        #endregion
        #region Special joints position
        private Vector3 HipJointRight
        {
            get {
                if (o.rightHipPos == Vector3.Zero)
                {
                    o.rightHipPos = GetHipJoint(true);
                }
                return o.rightHipPos;
            }
        }
        private Vector3 HipJointLeft
        {
            get
            {
                if (o.leftHipPos == Vector3.Zero)
                {
                    o.leftHipPos = GetHipJoint(false);
                }
                return o.leftHipPos;
            }
        }
        private Vector3 SternumClavicle
        {
            get
            {
                if (o.sternumClacicle == Vector3.Zero)
                {
                    Vector3 back = markers[MarkerNames.neck];
                    Vector3 front = markers[MarkerNames.chest];
                    Vector3 neckPos;
                    Vector3 neck2ChestVector = bd.NeckToChestVector;
                    Vector3 transformedNeckToChestVector = Vector3.Transform(neck2ChestVector, ChestOrientation) / 2;
                    if (!back.IsNaN() && neck2ChestVector != Vector3.Zero)
                    {
                        neckPos = back + transformedNeckToChestVector;

                    }
                    else if (!front.IsNaN() && neck2ChestVector != Vector3.Zero)
                    {
                        neckPos = front - transformedNeckToChestVector;
                    }
                    else
                    {
                        back.Normalize();
                        neckPos = back * BodyData.MarkerToSpineDist;
                    }
                    o.sternumClacicle = neckPos;
                }
                return o.sternumClacicle;
            }
        }
        private Vector3 Spine1
        {
            get {
                if (o.spine1 == Vector3.Zero)
                {
                    Vector3 pos;
                    Vector3 target;
                    if (markers[MarkerNames.neck].IsNaN())
                    {
                        pos = markers[MarkerNames.bodyBase];
                        target = markers[MarkerNames.spine];
                    }
                    else
                    {
                        pos = markers[MarkerNames.spine];
                        target = markers[MarkerNames.neck];
                    }
                Vector3 front = Vector3.Transform(Vector3.UnitZ, QuaternionHelper.LookAtUp(pos, target, ChestForward));
                front.Normalize();
                pos = markers[MarkerNames.spine];
                pos += front * BodyData.MarkerToSpineDist;
                o.spine1 = pos;
                }
                return o.spine1;
            }
        }
        private Vector3 Head
        {
            get
            {
                if (o.head == Vector3.Zero)
                {
                    Vector3 headPos = Vector3Helper.MidPoint(markers[MarkerNames.leftHead], markers[MarkerNames.rightHead]);
                    //Move head position down
                    Vector3 down = -Vector3.Transform(Vector3.UnitY, HeadOrientation);
                    //down.NormalizeFast();
                    headPos += down * BodyData.MidHeadToHeadJoint;
                    o.head = headPos;
                }
                return o.head;
            }
        }
        private Vector3 ShoulderLeft
        {
            get
            {
                if (o.shoulderLeft == Vector3.Zero)
                {
                    o.shoulderLeft = GetUpperarmJoint(false);
                }
                return o.shoulderLeft;
            }
        }
        private Vector3 ShoulderRight
        {
            get
            {
                if (o.shoulderRight == Vector3.Zero)
                {
                    o.shoulderRight = GetUpperarmJoint(true);
                }
                return o.shoulderRight;
            }
        }
        private Vector3 ElbowLeft
        {
            get
            {
                if (o.elbowLeft == Vector3.Zero)
                {
                    o.elbowLeft = Vector3Helper.MidPoint(markers[MarkerNames.leftInnerElbow], markers[MarkerNames.leftOuterElbow]);
                }
                return o.elbowLeft;
            }
        }
        private Vector3 ElbowRight
        {
            get
            {
                if (o.elbowRight == Vector3.Zero)
                {
                    o.elbowRight = Vector3Helper.MidPoint(markers[MarkerNames.rightInnerElbow], markers[MarkerNames.rightOuterElbow]);
                }
                return o.elbowRight;
            }
        }
        private Vector3 HandLeft
        {
            get
            {
                if (o.handLeft == Vector3.Zero)
                {
                    o.handLeft = Vector3Helper.MidPoint(markers[MarkerNames.leftWrist], markers[MarkerNames.leftWristRadius]);
                }
                return o.handLeft;
            }
        }
        private Vector3 HandRight
        {
            get
            {
                if (o.handRight == Vector3.Zero)
                {
                    o.handRight = Vector3Helper.MidPoint(markers[MarkerNames.rightWrist], markers[MarkerNames.rightWristRadius]);
                }
                return o.handRight;
            }
        }
        private Vector3 KneeRight
        {
            get
            {
                if (o.kneeRight == Vector3.Zero)
                {
                    o.kneeRight = GetKneePos(true);
                }
                return o.kneeRight;
            }
        }
        private Vector3 KneeLeft
        {
            get
            {
                if (o.kneeLeft  == Vector3.Zero)
                {
                    o.kneeLeft = GetKneePos(false);
                }
                return o.kneeLeft;
            }
        }
        private Vector3 AnkleLeft
        {
            get
            {
                if (o.ankleLeft == Vector3.Zero)
                {
                    o.ankleLeft = GetAnklePos(false);
                }
                return o.ankleLeft;
            }
        }
        private Vector3 AnkleRight
        {
            get
            {
                if (o.ankleRight == Vector3.Zero)
                {
                    o.ankleRight = GetAnklePos(true);
                }
                return o.ankleRight;
            }
        }

        private Vector3 GetHipJoint(bool isRightHip)
        {
            // as described by Harrington et al. 2006
            // Prediction of the hip joint centre in adults, children, and patients with
            // cerebral palsy based on magnetic resonance imaging
            Vector3 ASISMid = Vector3Helper.MidPoint(markers[MarkerNames.rightHip], markers[MarkerNames.leftHip]);
            float Z, X, Y,
                pelvisDepth = (ASISMid - markers[MarkerNames.bodyBase]).Length * 1000,
                pelvisWidth = (markers[MarkerNames.leftHip] - markers[MarkerNames.rightHip]).Length * 1000;
            X = 0.33f * pelvisWidth - 7.3f;
            Y = -0.30f * pelvisWidth - 10.9f;
            Z = -0.24f * pelvisDepth - 9.9f;
            if (!isRightHip) X = -X;
            Vector3 offset = new Vector3(X, Y, Z) / 1000;
            offset = QuaternionHelper.Rotate(HipOrientation, offset);
            return ASISMid + offset;
        }
        private Vector3 GetUpperarmJoint(bool isRightShoulder)
        {
            // as described by Campbell et al. 2009 in 
            // MRI development and validation of two new predictive methods of
            // glenohumeral joint centre location identification and comparison with
            // established techniques
            float
                x = 96.2f - 0.302f * (bd.NeckToChestVector.Length*1000) - 0.364f * bd.Height + 0.385f * bd.Mass,
                y = -66.32f + 0.30f * (bd.NeckToChestVector.Length * 1000) - 0.432f * bd.Mass,
                z = 66.468f - 0.531f * bd.ShoulderWidth + 0.571f * bd.Mass;
            if (isRightShoulder) z = -z;
            Vector3 res = new Vector3(x, y, z) / 1000; // to mm
            res = QuaternionHelper.Rotate(ChestOrientation, res);
            res += isRightShoulder ? markers[MarkerNames.rightShoulder] : markers[MarkerNames.leftShoulder];
            return res;
        }
        private Vector3 GetKneePos(bool isRightKnee)
        {
            Vector3 x, z, M1, M2, M3, negateY = new Vector3(1f, -1f, 1f);
                if (isRightKnee)
                {
                   M1 =  markers[MarkerNames.rightOuterKnee];//FLE
                   M2 = markers[MarkerNames.rightOuterAnkle];//FAL
                   M3 = markers[MarkerNames.rightLowerKnee];//TTC
                }
                else
                {
                    M1 = markers[MarkerNames.leftOuterKnee];//FLE
                    M2 = markers[MarkerNames.leftOuterAnkle];//FAL
                    M3 = markers[MarkerNames.leftLowerKnee];//TTC
            }
            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M1 - M2;
            float scalingFactor = z.Length;
            Matrix4 R = Matrix4Helper.GetOrientationMatrix(x, z);
            Vector3 trans = new Vector3(
                -0.1033f*scalingFactor,
                -0.09814f*scalingFactor,
                0.0597f*scalingFactor);
            if (isRightKnee) Vector3.Multiply(ref trans, ref negateY, out trans);
            return Vector3.TransformVector(trans, R) + M1;
        }
        private Vector3 GetAnklePos(bool isRightAnkle)
        {
            //Stolen from Visual3d
            Vector3 x, z, M1, M2, M3, negateY = new Vector3(1f, -1f, 1f);
            Matrix4 R;
            if (isRightAnkle)
            {
                M1 = markers[MarkerNames.rightOuterKnee];//FLE
                M3 = markers[MarkerNames.rightLowerKnee];//TTC
                M2 = markers[MarkerNames.rightOuterAnkle];//FAL
            }
            else
            {
                M1 = markers[MarkerNames.leftOuterKnee];//FLE
                M3 = markers[MarkerNames.leftLowerKnee];//TTC
                M2 = markers[MarkerNames.leftOuterAnkle];//FAL
            }
            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M2 - M1;
            float scalefactor = z.Length;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            Vector3 trans = new Vector3(
                -0.07675f * scalefactor, 
                0.05482f * scalefactor, 
                -0.02741f * scalefactor);
            if (!isRightAnkle) Vector3.Multiply(ref trans, ref negateY, out trans);
            return Vector3.TransformVector(trans, R) + M2;
        } 
        #endregion


        // Functions for filling bones
        #region Funcions
        #region pelsvis too head getters
        private void Plevis(Bone b)
        {
            b.Pos = Vector3Helper.MidPoint(HipJointRight, HipJointLeft); 
            b.Orientation = HipOrientation;
        }
        private void SpineRoot(Bone b)
        {
            Vector3 target = Spine1.IsNaN() ? SternumClavicle : Spine1;
            Vector3 pos = markers[MarkerNames.bodyBase] + HipForward * BodyData.MarkerToSpineDist;
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, HipForward);
        }
        private void MidSpine(Bone b)
        {
            b.Pos = Spine1;
            b.Orientation = QuaternionHelper.LookAtRight(Spine1, SternumClavicle,  HipJointLeft - HipJointRight);
        }
        private void SpineEnd(Bone b)
        {
            b.Pos = SternumClavicle;
            b.Orientation = ChestOrientation;
        }
        private void Neck(Bone b)
        {
            Vector3 up = Vector3.Transform(Vector3.UnitY, ChestOrientation);
            Vector3 neckPos = SternumClavicle;
            Vector3 pos = neckPos + up * BodyData.SpineLength * 2;
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, Head, ChestForward);
        }
        private void GetHead(Bone b)
        {
            b.Pos = Head;
            b.Orientation = HeadOrientation;
        }
        private void GetHeadTop(Bone b)
        {
            Vector3 plus = Vector3.Transform(Vector3.UnitY,HeadOrientation)*(BodyData.MidHeadToHeadJoint*2);
            b.Pos = Head+plus;
        }
        
        #endregion
        #region leg getters
        private void UpperLegLeft(Bone b)
        {
            b.Pos = HipJointLeft;
            b.Orientation = QuaternionHelper.LookAtRight(HipJointLeft, KneeLeft, KneeForwardLeft);
        }
        private void UpperLegRight(Bone b)
        {
            b.Pos = HipJointRight;
            b.Orientation = QuaternionHelper.LookAtRight(HipJointRight, KneeRight, KneeForwardRight);
        }
        private void LowerLegLeft(Bone b)
        {
            b.Pos = KneeLeft;
            b.Orientation = QuaternionHelper.LookAtRight(KneeLeft, AnkleLeft, KneeForwardLeft); 
        }
        private void LowerLegRight(Bone b)
        {
            b.Pos = KneeRight;
            b.Orientation = QuaternionHelper.LookAtRight(KneeRight, AnkleRight, KneeForwardRight);
        }
        private void GetAnkleLeft(Bone b)
        {
            Vector3 up = KneeLeft - AnkleLeft;
            b.Pos = AnkleLeft;
            b.Orientation = QuaternionHelper.LookAtUp(AnkleLeft, markers[MarkerNames.leftToe2], up);
        }
        private void GetAnkleRight(Bone b)
        {
            Vector3 up = KneeRight - AnkleRight;
            b.Pos = AnkleRight;
            b.Orientation = QuaternionHelper.LookAtUp(AnkleRight, markers[MarkerNames.rightToe2], up);
        }
        private void GetFootLeft(Bone b)
        {
            b.Pos = markers[MarkerNames.leftToe2];
        }
        private void GetFootRight(Bone b)
        {
            b.Pos = markers[MarkerNames.rightToe2];
        }
        private void GetHeelLeft(Bone b)
        {
            b.Pos = markers[MarkerNames.leftHeel];
        }
        private void GetHeelRight(Bone b)
        {
            b.Pos = markers[MarkerNames.rightHeel];
        }
        
        #endregion
        #region arm getters
        private void GetShoulderLeft(Bone b)
        {
            b.Pos = new Vector3(SternumClavicle);
            b.Orientation = QuaternionHelper.LookAtUp(SternumClavicle, ShoulderLeft, ChestForward);
        }
        private void GetShoulderRight(Bone b)
        {
            b.Pos = new Vector3(SternumClavicle);
            b.Orientation = QuaternionHelper.LookAtUp(SternumClavicle, ShoulderRight, ChestForward);
        }
        private void GetUpperArmLeft(Bone b)
        {
            b.Pos = ShoulderLeft;
            b.Orientation = QuaternionHelper.LookAtRight(ShoulderLeft, ElbowLeft, markers[MarkerNames.leftInnerElbow] - markers[MarkerNames.leftOuterElbow]);
        }
        private void GetUpperArmRight(Bone b)
        {
            b.Pos = ShoulderRight;
            b.Orientation = QuaternionHelper.LookAtRight(ShoulderRight, ElbowRight, markers[MarkerNames.rightOuterElbow] - markers[MarkerNames.rightInnerElbow]);
        }
        private void GetLowerArmLeft(Bone b)
        {
            b.Pos = ElbowLeft;
            b.Orientation = QuaternionHelper.LookAtUp(ElbowLeft, HandLeft, LowerArmForwardLeft);
        }
        private void GetLowerArmRight(Bone b)
        {
            b.Pos = ElbowRight;
            b.Orientation = QuaternionHelper.LookAtUp(ElbowRight, HandRight, LowerArmForwardRight);
        }
        private void GetWristLeft(Bone b)
        {
            b.Pos = HandLeft;
            b.Orientation = QuaternionHelper.LookAtUp(HandLeft, markers[MarkerNames.leftHand], LowerArmForwardLeft);
        }
        private void GetWristRight(Bone b)
        {
            b.Pos = HandRight;
            b.Orientation = QuaternionHelper.LookAtUp(HandRight, markers[MarkerNames.rightHand], LowerArmForwardRight);
        }
        private void GetHandLeft(Bone b)
        {
            b.Pos = markers[MarkerNames.leftHand];
        }
        private void GetHandRight(Bone b)
        {
            b.Pos = markers[MarkerNames.rightHand];
        }
        #endregion
        #endregion
    }
}