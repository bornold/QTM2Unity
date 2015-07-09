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
        public Vector3 lowerLegUpLeft;
        public Vector3 lowerLegUpRight;
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
        public Vector3 footBaseRight;
        public Vector3 footBaseLeft;
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
        private Vector3 ZeroVector3 = Vector3.Zero;
        private Quaternion ZeroQuaternion = QuaternionHelper.Zero;
        private Vector3 UnitX = Vector3.UnitX;
        private Vector3 UnitY = Vector3.UnitY;
        private Vector3 UnitZ = Vector3.UnitZ;
        #endregion
        public JointLocalization()
        {
            jointFunctions = new List<Action<Bone>>() {
                    //GC 0.7kB
                    (b) => Plevis(b), 
                    //GC 0.9kB
                    (b) => SpineRoot(b), 
                    // GC 1.1kB
                    (b) => MidSpine(b),
                    // GC 1.2kB
                    (b) => SpineEnd(b),
                    // GC 1.4kB
                    (b) => Neck(b),
                    // GC 1.5kB
                    (b) => GetHead(b),
                    // GC 1.7kB
                    (b) => GetHeadTop(b),
                    // GC 1.8kB
                    (b) => GetShoulderLeft(b),
                    // GC 2.0kB
                    (b) => GetUpperArmLeft(b),
                    // GC 2.2kB
                    (b) => GetLowerArmLeft(b),
                    (b) => GetWristLeft(b),
                    (b) => GetTrapLeft(b),
                    (b) => GetThumbLeft(b),
                    (b) => GetHandLeft(b),
                    (b) => GetIndexLeft(b),

                    (b) => GetShoulderRight(b), 
                    (b) => GetUpperArmRight(b),
                    (b) => GetLowerArmRight(b),
                    (b) => GetWristRight(b),
                    (b) => GetTrapRight(b),
                    (b) => GetThumbRight(b),
                    (b) => GetHandRight(b),
                    (b) => GetIndexRight(b),

                    (b) => UpperLegLeft(b),       
                    (b) => LowerLegLeft(b),
                    (b) => GetAnkleLeft(b),
                    (b) => GetFootBaseLeft(b),
                    (b) => GetFootLeft(b),
                    (b) => UpperLegRight(b),
                    (b) => LowerLegRight(b), 
                    (b) => GetAnkleRight(b),    
                    (b) => GetFootBaseRight(b),
                    (b) => GetFootRight(b),
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
                if (o.hipOrientation == ZeroQuaternion)
                {
                    Vector3 front = Vector3Helper.MidPoint(markers[MarkerNames.leftHip], markers[MarkerNames.rightHip])
                                    - markers[MarkerNames.bodyBase];
                    Quaternion frontRot = QuaternionHelper.GetRotation2(UnitZ, front);
                    o.hipOrientation = QuaternionHelper.GetRotation2(
                                            Vector3.Transform(UnitY, frontRot),
                                            Vector3.Cross((markers[MarkerNames.leftHip] - markers[MarkerNames.rightHip]), front))
                                        * frontRot;
                }

                return o.hipOrientation;
            }
        }
        private Quaternion ChestOrientation
        {
            get
            {
                if (o.chestOrientation == ZeroQuaternion)
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
                        Yaxis = Vector3.Transform(UnitY, Quaternion.Slerp(prevChestOri, HipOrientation, 0.5f));
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
                            Xaxis = -Vector3.Transform(UnitX, Quaternion.Slerp(prevChestOri, HipOrientation, 0.5f));
                        }
                    }
                    else // last resort, use hip prev orientation
                    {
                        Xaxis = -Vector3.Transform(UnitX, Quaternion.Slerp(prevChestOri, HipOrientation, 0.5f));
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
                if (o.headOrientation == ZeroQuaternion)
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
                if (o.hipForward == ZeroVector3)
                    o.hipForward = Vector3.Transform(UnitZ, HipOrientation);
                return o.hipForward;
            }
        }
        private Vector3 ChestForward
        {
            get
            {
                if (o.chestForward == ZeroVector3)
                {
                    o.chestForward = Vector3.Transform(UnitZ, ChestOrientation);
                }
                return o.chestForward;
            }
        }
        private Vector3 UpperArmForwardLeft
        {
            get
            {
                if (o.upperArmForwardLeft == ZeroVector3)
                {

                    Vector3 midPoint = Vector3Helper.MidPoint(markers[MarkerNames.leftInnerElbow], markers[MarkerNames.leftOuterElbow]);
                    o.upperArmForwardLeft = Vector3.NormalizeFast(midPoint - markers[MarkerNames.leftElbow]);
                }
                return o.upperArmForwardLeft;
            }
        }
        private Vector3 UpperArmForwardRight
        {
            get
            {
                if (o.upperArmForwardRight == ZeroVector3)
                {
                    Vector3 midPoint = Vector3Helper.MidPoint(markers[MarkerNames.rightInnerElbow], markers[MarkerNames.rightOuterElbow]);
                    o.upperArmForwardRight = Vector3.Normalize(midPoint - markers[MarkerNames.rightElbow]);
                }
                return o.upperArmForwardRight;
            }
        }
        private Vector3 LowerArmForwardLeft
        {
            get
            {
                if (o.lowerArmForwardLeft == ZeroVector3)
                {
                    o.lowerArmForwardLeft = Vector3.NormalizeFast(markers[MarkerNames.leftWristRadius] - markers[MarkerNames.leftWrist]);
                }
                return o.lowerArmForwardLeft;
            }
        }
        private Vector3 LowerArmForwardRight
        {
            get
            {
                if (o.lowerArmForwardRight == ZeroVector3)
                {
                    o.lowerArmForwardRight = Vector3.NormalizeFast(markers[MarkerNames.rightWristRadius] - markers[MarkerNames.rightWrist]);
                }
                return o.lowerArmForwardRight;
            }
        }
        private Vector3 KneeForwardLeft
        {
            get
            {
                if (o.kneeForwardLeft == ZeroVector3)
                {
                    Vector3 knee = KneeLeft;
                    Vector3 kneeOuter = markers[MarkerNames.leftOuterKnee];
                    o.kneeForwardLeft = knee - kneeOuter;
                }
                return o.kneeForwardLeft;
            }
        }
        private Vector3 KneeForwardRight
        {
            get
            {
                if (o.kneeForwardRight == ZeroVector3)
                {
                    Vector3 knee = KneeRight;
                    Vector3 kneeOuter = markers[MarkerNames.rightOuterKnee];
                    o.kneeForwardRight = kneeOuter - knee;
                    //kneeForwardRight = GetKneeForwardRight();
                }
                return o.kneeForwardRight;
            }
        }
        private Vector3 LowerLegUpLeft
        {
            get
            {
                if (o.lowerLegUpLeft == ZeroVector3)
                {
                    Vector3 belove =
                       !FootBaseLeft.IsNaN() ? FootBaseLeft :
                       !AnkleLeft.IsNaN() ? AnkleLeft :
                       !markers[MarkerNames.leftHeel].IsNaN() ? markers[MarkerNames.leftHeel] :
                       !markers[MarkerNames.leftOuterAnkle].IsNaN() ? markers[MarkerNames.leftOuterAnkle] :
                       !markers[MarkerNames.leftInnerAnkle].IsNaN() ? markers[MarkerNames.leftInnerAnkle] :
                       !markers[MarkerNames.leftToe2].IsNaN() ? markers[MarkerNames.leftToe2] :
                       HipJointLeft - (Vector3.Transform(UnitY, HipOrientation) * HipJointLeft.LengthFast);
                    Vector3 above =
                        !KneeLeft.IsNaN() ? KneeLeft :
                        !markers[MarkerNames.leftLowerKnee].IsNaN() ? markers[MarkerNames.leftLowerKnee] :
                        !markers[MarkerNames.leftUpperKnee].IsNaN() ? markers[MarkerNames.leftUpperKnee] :
                        !markers[MarkerNames.leftInnerKnee].IsNaN() ? markers[MarkerNames.leftInnerKnee] :
                        !markers[MarkerNames.leftOuterKnee].IsNaN() ? markers[MarkerNames.leftOuterKnee] :
                        HipJointLeft;
                    o.lowerLegUpLeft = above - belove;
                }
                return o.lowerLegUpLeft;
            }
        }
        private Vector3 LowerLegUpRight
        {
            get
            {
                if (o.lowerLegUpRight == ZeroVector3)
                {
                    Vector3 belove =
                        !FootBaseRight.IsNaN() ? FootBaseRight :
                        !AnkleRight.IsNaN() ? AnkleRight :
                        !markers[MarkerNames.rightHeel].IsNaN() ? markers[MarkerNames.rightHeel] :
                        !markers[MarkerNames.rightOuterAnkle].IsNaN() ? markers[MarkerNames.rightOuterAnkle] :
                        !markers[MarkerNames.rightInnerAnkle].IsNaN() ? markers[MarkerNames.rightInnerAnkle] :
                        !markers[MarkerNames.leftToe2].IsNaN() ? markers[MarkerNames.leftToe2] :
                        HipJointRight - (Vector3.Transform(UnitY,HipOrientation) * HipJointRight.LengthFast );
                    Vector3 above =
                        !KneeRight.IsNaN() ? KneeRight :
                        !markers[MarkerNames.rightLowerKnee].IsNaN() ? markers[MarkerNames.rightLowerKnee] :
                        !markers[MarkerNames.rightUpperKnee].IsNaN() ? markers[MarkerNames.rightUpperKnee] :
                        !markers[MarkerNames.rightInnerKnee].IsNaN() ? markers[MarkerNames.rightInnerKnee] :
                        !markers[MarkerNames.rightOuterKnee].IsNaN() ? markers[MarkerNames.rightOuterKnee] :
                        HipJointRight;
                    o.lowerLegUpRight = above - belove;
                }
                return o.lowerLegUpRight;
            }
        }
        #endregion
        #region Special joints position
        private Vector3 HipJointRight
        {
            get {
                if (o.rightHipPos == ZeroVector3)
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
                if (o.leftHipPos == ZeroVector3)
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
                if (o.sternumClacicle == ZeroVector3)
                {
                    Vector3 back = markers[MarkerNames.neck];
                    Vector3 front = markers[MarkerNames.chest];
                    Vector3 neckPos;
                    Vector3 neck2ChestVector = bd.NeckToChestVector;
                    Vector3 transformedNeckToChestVector = Vector3.Transform(neck2ChestVector, ChestOrientation) / 2;
                    if (!back.IsNaN() && neck2ChestVector != ZeroVector3)
                    {
                        neckPos = back + transformedNeckToChestVector;

                    }
                    else if (!front.IsNaN() && neck2ChestVector != ZeroVector3)
                    {
                        neckPos = front - transformedNeckToChestVector;
                    }
                    else
                    {
                        neckPos = Vector3.NormalizeFast(back) * BodyData.MarkerToSpineDist;
                    }
                    o.sternumClacicle = neckPos;
                }
                return o.sternumClacicle;
            }
        }
        private Vector3 Spine1
        {
            get {
                if (o.spine1 == ZeroVector3)
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
                Vector3 front = Vector3.Transform(UnitZ, QuaternionHelper.LookAtUp(pos, target, ChestForward));
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
                if (o.head == ZeroVector3)
                {
                    Vector3 headPos = Vector3Helper.MidPoint(markers[MarkerNames.leftHead], markers[MarkerNames.rightHead]);
                    //Move head position down
                    Vector3 down = -Vector3.Transform(UnitY, HeadOrientation);
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
                if (o.shoulderLeft == ZeroVector3)
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
                if (o.shoulderRight == ZeroVector3)
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
                if (o.elbowLeft == ZeroVector3)
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
                if (o.elbowRight == ZeroVector3)
                {
                    o.elbowRight = Vector3Helper.MidPoint(markers[MarkerNames.rightInnerElbow], markers[MarkerNames.rightOuterElbow]);
                }
                return o.elbowRight;
            }
        }
        private Vector3 WristLeft
        {
            get
            {
                if (o.handLeft == ZeroVector3)
                {
                    o.handLeft = Vector3Helper.MidPoint(markers[MarkerNames.leftWrist], markers[MarkerNames.leftWristRadius]);
                }
                return o.handLeft;
            }
        }
        private Vector3 WristRight
        {
            get
            {
                if (o.handRight == ZeroVector3)
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
                if (o.kneeRight == ZeroVector3)
                {
                    if (markers[MarkerNames.rightInnerKnee].IsNaN())
                    {
                        o.kneeRight = GetKneePos(true);
                    }
                    else
                    {
                        o.kneeRight = Vector3Helper.MidPoint(markers[MarkerNames.rightOuterKnee], markers[MarkerNames.rightInnerKnee]);
                    }
                }
                return o.kneeRight;
            }
        }
        private Vector3 KneeLeft
        {
            get
            {
                if (o.kneeLeft  == ZeroVector3)
                {
                    if (markers[MarkerNames.leftInnerKnee].IsNaN())
                    {
                        o.kneeLeft = GetKneePos(false);
                    }
                    else
                    {
                        o.kneeLeft = Vector3Helper.MidPoint(markers[MarkerNames.leftOuterKnee], markers[MarkerNames.leftInnerKnee]);
                    }
                }
                return o.kneeLeft;
            }
        }
        private Vector3 AnkleLeft
        {
            get
            {
                    if (o.ankleLeft == ZeroVector3)
                    {
                        if (markers[MarkerNames.leftInnerAnkle].IsNaN())
                        {
                            o.ankleLeft = GetAnklePos(false);
                        }
                        else
                        {
                            o.ankleLeft = Vector3Helper.MidPoint(markers[MarkerNames.leftOuterAnkle], markers[MarkerNames.leftInnerAnkle]);
                        }
                    }
                return o.ankleLeft;
            }
        }
        private Vector3 AnkleRight
        {
            get
            {
                if (o.ankleRight == ZeroVector3)
                {
                    if (markers[MarkerNames.rightInnerAnkle].IsNaN())
                    {
                        o.ankleRight = GetAnklePos(true);
                    }
                    else
                    {
                        o.ankleRight = Vector3Helper.MidPoint(markers[MarkerNames.rightOuterAnkle], markers[MarkerNames.rightInnerAnkle]);
                    }
                }
                return o.ankleRight;
            }
        }
        private Vector3 FootBaseLeft
        {
            get
            {
                if (o.footBaseLeft == ZeroVector3)
                {
                    o.footBaseLeft = Vector3Helper.PointBetween(markers[MarkerNames.leftHeel], markers[MarkerNames.leftToe2], 0.4f);
                }
                return o.footBaseLeft;
            }
        }
        private Vector3 FootBaseRight
        {
            get
            {
                if (o.footBaseRight == ZeroVector3)
                {
                    o.footBaseRight = Vector3Helper.PointBetween(markers[MarkerNames.rightHeel], markers[MarkerNames.rightToe2], 0.4f);
                }
                return o.footBaseRight;
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
            return ASISMid + Vector3.Transform((new Vector3(X, Y, Z) / 1000), HipOrientation);
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
            res = Vector3.Transform(res, ChestOrientation); //QuaternionHelper.Rotate(ChestOrientation, res);
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
            Vector3 up = Vector3.Transform(UnitY, ChestOrientation);
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
            b.Pos = Head + Vector3.Transform(UnitY, HeadOrientation) * (BodyData.MidHeadToHeadJoint * 2);
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
            b.Orientation = QuaternionHelper.LookAtUp(AnkleLeft, FootBaseLeft, up);
        }
        private void GetAnkleRight(Bone b)
        {
            Vector3 up = KneeRight - AnkleRight;
            b.Pos = AnkleRight;
            b.Orientation = QuaternionHelper.LookAtUp(AnkleRight, FootBaseRight, up);
        }
        private void GetFootBaseLeft(Bone b)
        {
            b.Pos = FootBaseLeft;
            b.Orientation = QuaternionHelper.LookAtUp(b.Pos, markers[MarkerNames.leftToe2], LowerLegUpLeft);
        }
        private void GetFootBaseRight(Bone b)
        {
            b.Pos = FootBaseRight;
            b.Orientation = QuaternionHelper.LookAtUp(b.Pos, markers[MarkerNames.rightToe2], LowerLegUpRight);
        }
        private void GetFootLeft(Bone b)
        {
            b.Pos = markers[MarkerNames.leftToe2];
        }
        private void GetFootRight(Bone b)
        {
            b.Pos = markers[MarkerNames.rightToe2];
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
            b.Orientation = QuaternionHelper.LookAtUp(ElbowLeft, WristLeft, LowerArmForwardLeft);
        }
        private void GetLowerArmRight(Bone b)
        {
            b.Pos = ElbowRight;
            b.Orientation = QuaternionHelper.LookAtUp(ElbowRight, WristRight, LowerArmForwardRight);
        }
        private void GetWristLeft(Bone b)
        {
            b.Pos = WristLeft;
            b.Orientation = QuaternionHelper.LookAtUp(WristLeft, markers[MarkerNames.leftHand], LowerArmForwardLeft);
        }
        private void GetWristRight(Bone b)
        {
            b.Pos = WristRight;
            b.Orientation = QuaternionHelper.LookAtUp(WristRight, markers[MarkerNames.rightHand], LowerArmForwardRight);
        }
        #region hand getters
        private void GetTrapLeft(Bone b)
        {
            b.Pos = WristLeft;
            b.Orientation = QuaternionHelper.LookAtUp(WristLeft, markers[MarkerNames.leftThumb], LowerArmForwardLeft);
        }
        private void GetTrapRight(Bone b)
        {
            b.Pos = WristRight;
            b.Orientation = QuaternionHelper.LookAtUp(WristRight, markers[MarkerNames.rightThumb], LowerArmForwardRight);
        }
        private void GetHandLeft(Bone b)
        {
            b.Pos = markers[MarkerNames.leftHand];
            b.Orientation = QuaternionHelper.LookAtUp(markers[MarkerNames.leftHand], markers[MarkerNames.leftIndex], LowerArmForwardLeft);
        }
        private void GetHandRight(Bone b)
        {
            b.Pos = markers[MarkerNames.rightHand];
            b.Orientation = QuaternionHelper.LookAtUp(markers[MarkerNames.rightHand], markers[MarkerNames.rightIndex], LowerArmForwardRight);
        }
        private void GetThumbLeft(Bone b)
        {
            b.Pos = markers[MarkerNames.leftThumb];
        }
        private void GetThumbRight(Bone b)
        {
            b.Pos = markers[MarkerNames.rightThumb];
        }
        private void GetIndexLeft(Bone b)
        {
            b.Pos = markers[MarkerNames.leftIndex];
        }
        private void GetIndexRight(Bone b)
        {
            b.Pos = markers[MarkerNames.rightIndex];
        }
        #endregion
        #endregion
        #endregion
    }
}