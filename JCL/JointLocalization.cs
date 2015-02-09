using System;
using System.Collections.Generic;
using System.Linq;
using QTM2Unity.SkeletonModel;
using OpenTK;
using QTM2Unity.Unity;
namespace QTM2Unity.JCL
{
    class JointLocalization
    {
        public bool debug = false;

        public float height = 140;
        public const float mass = 76;
        private float chestDepth;
        private float shoulderWidth;

        private float legLenght;
        private float pelvisDepth;
        private float pelvisWidth;
        private Vector3 ASIS;
        private Vector3 PSIS;
        private Vector3 ASISMid;
        private Vector3 RIAS;
        private Vector3 LIAS;
        private Vector3 Sacrum;

        private Dictionary<string, Vector3> markers;
        private Dictionary<string, Vector3> someJoints;
        private Quaternion pelvisOrientation;
        private Vector3 kneeForwardOrientationRight;
        private Vector3 kneeForwardOrientationLeft;
        private Vector3 armForwardOrientationRight;
        private Vector3 armForwardOrientationLeft;
        private Vector3 rightWristOrientation;
        private Vector3 leftWristOrientation;
        private BipedSkeleton lastFrame;
        private BipedSkeleton currentFrame;

        private const float blend = 0.1f;
        public BipedSkeleton getJointLocazion(List<LabeledMarker> markerData)
        {
            lastFrame = currentFrame;
            markers = markerData.ToDictionary(k => k.label, v => v.position);
            someJoints = JointPossitions();
            ShoulderData();
            HipOrientation();
            KneeOrientation();
            ArmOrientation();
            WritsOrientation();

            Bone pelvis = GetPlevis();

            Bone spine0 = GetSpine0();
            spine0.Parent = pelvis;
            Bone spine1 = GetSpine1();
            spine1.Parent = spine0;
            Bone spine3 = GetSpine3();
            spine3.Parent = spine1;
            Bone neck = GetNeck();
            neck.Parent = spine3;
            Bone head = GetHead();
            head.Parent = spine3;
            
            Bone leftUpperLeg = GetUpperLegLeft();
            leftUpperLeg.Parent = pelvis;
            Bone leftLowerLeg = GetLowerLegLeft();
            leftLowerLeg.Parent = leftUpperLeg;
            Bone leftFoot = GetFootLeft();
            leftFoot.Parent = leftLowerLeg;
            
            Bone rightUpperLeg = GetUpperLegRight();
            rightUpperLeg.Parent = pelvis;
            Bone rightLowerLeg = GetLowerLegRight();
            rightLowerLeg.Parent = rightUpperLeg;
            Bone rightFoot = GetFootRight();
            rightFoot.Parent = rightLowerLeg;

            Bone leftShoulder = GetShoulderLeft();
            leftShoulder.Parent = neck;
            Bone leftUpperArm = GetUpperArmLeft();
            leftUpperArm.Parent = leftShoulder;
            Bone leftLowerArm = GetLowerArmLeft();
            leftLowerArm.Parent = leftUpperArm;
            Bone leftHand = GetHandLeft();
            leftHand.Parent = leftLowerArm;

            Bone rightShoulder = GetShoulderRight();
            rightShoulder.Parent = neck;
            Bone rightUpperArm = GetUpperArmRight();
            rightUpperArm.Parent = rightShoulder;
            Bone rightLowerArm = GetLowerArmRight();
            rightLowerArm.Parent = rightUpperArm;
            Bone rightHand = GetHandRight();
            rightHand.Parent = rightLowerArm;
            
            currentFrame = new BipedSkeleton(new List<Bone>()
                {
                    pelvis, 
                    spine0, spine1, spine3,     
                    neck,         head,
                    rightUpperLeg, rightLowerLeg, rightFoot,
                    leftUpperLeg,  leftLowerLeg,  leftFoot,
                    
                    leftShoulder,  leftUpperArm,  leftLowerArm, leftHand,
                    rightShoulder, rightUpperArm, rightLowerArm, rightHand
                    
                });
            return currentFrame;
        }

        private Bone GetPlevis()
        {
            float Y = FemurYOffset();// -0.30f * pelvisWidth - 10.9f;
            Vector3 off = new Vector3(0, Y, 0) / 1000;
            off = QuaternionHelper.Rotate(pelvisOrientation, off);
            Vector3 pos = Sacrum + off;
            return new Bone(BipedSkeleton.PELVIS, pos, pelvisOrientation);
        }
        private Bone GetSpine0()
        {
            Vector3 pos = Sacrum;
            Vector3 target = markers[spine];
            Vector3 front = Vector3.Transform(Vector3.UnitZ,pelvisOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.SPINE0, pos, rot); //TODO Fix spine rot and better pos
        }
        private Bone GetSpine1()
        {
            Vector3 pos = markers[spine];
            Vector3 target = markers[neck];
            Vector3 front = Vector3.Transform(Vector3.UnitZ, pelvisOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.SPINE1, pos, rot); //TODO Fix spine rot and better pos        }
        }
        private Bone GetSpine3()
        {
            Vector3 pos = markers[neck];
            Vector3 target = someJoints[BipedSkeleton.HEAD];
            Vector3 front = Vector3.Transform(Vector3.UnitZ, pelvisOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.SPINE3, pos, rot); //TODO Fix spine rot and better pos        }
        }
        private Bone GetHead()
        {
            Vector3 headV = markers[head];
            Vector3 rightHeadV = markers[rightHead];
            Vector3 leftHeadV = markers[leftHead];
            Vector3 pos = someJoints[BipedSkeleton.HEAD]; //TODO Head joint lower, but how?
            Quaternion orientation = QuaternionHelper.GetOrientation(headV, leftHeadV, rightHeadV);
            return new Bone(BipedSkeleton.HEAD, pos, orientation);

        }
        private Bone GetNeck()
        {
            Vector3 root;
            if (markers.ContainsKey(neck))
            {
                root = someJoints[BipedSkeleton.NECK];
            }
            else
            {
                root = markers[chest];
            }
            Vector3 source = markers[neck];
            Vector3 target = markers[head];
            Vector3 right = markers[leftShoulder] - markers[rightShoulder];
            Quaternion rot = QuaternionHelper.LookAtRight(source, target, right); //TODO Fix good neckrotation
            return new Bone(BipedSkeleton.NECK, root, rot);
        }
        private Bone GetUpperLegLeft()
        {

            Vector3 pos = GetFemurJoint(false); 
            Vector3 target = someJoints[BipedSkeleton.LOWERLEG_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationLeft);
            return new Bone(BipedSkeleton.UPPERLEG_L, pos, rot);
        }
        private Bone GetUpperLegRight()
        {
            Vector3 pos = GetFemurJoint(true);
            Vector3 target = someJoints[BipedSkeleton.LOWERLEG_R];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationRight); 
            return new Bone(BipedSkeleton.UPPERLEG_R, pos, rot);
        }

        private Bone GetLowerLegLeft()
        {
            Vector3 pos = someJoints[BipedSkeleton.LOWERLEG_L];
            Vector3 target = someJoints[BipedSkeleton.FOOT_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationLeft);
            return new Bone(BipedSkeleton.LOWERLEG_R, pos, rot);
        }
        private Bone GetLowerLegRight()
        {
            Vector3 pos = someJoints[BipedSkeleton.LOWERLEG_R];
            Vector3 target = someJoints[BipedSkeleton.FOOT_R];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationRight);
            return new Bone(BipedSkeleton.LOWERLEG_R, pos, rot);
        }
        private Bone GetFootLeft()
        {
            Vector3 pos = someJoints[BipedSkeleton.FOOT_L];
            Vector3 target = markers[leftFoot];
            Vector3 up = someJoints[BipedSkeleton.LOWERLEG_L] - pos;
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, up);
            return new Bone(BipedSkeleton.FOOT_L, pos, rot);
        }
        private Bone GetFootRight()
        {
            Vector3 pos = someJoints[BipedSkeleton.FOOT_R];
            Vector3 target = markers[rightFoot];
            Vector3 up = someJoints[BipedSkeleton.LOWERLEG_R] - pos;
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, up);
            return new Bone(BipedSkeleton.FOOT_R, pos, rot);
        }
        private Bone GetShoulderLeft()
        {
            Vector3 pos = someJoints[BipedSkeleton.NECK];
            Quaternion rot = Quaternion.Identity;
            return new Bone(BipedSkeleton.SHOULDER_L, pos, rot);
        }
        private Bone GetShoulderRight()
        {
            Vector3 pos = someJoints[BipedSkeleton.NECK];
            Quaternion rot = Quaternion.Identity; 
            return new Bone(BipedSkeleton.SHOULDER_R, pos, rot);
        }
        private Bone GetUpperArmLeft()
        {
            Vector3 pos = GetUpperarmJoint(false);
            Vector3 target = someJoints[BipedSkeleton.LOWERARM_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, armForwardOrientationLeft);
            return new Bone(BipedSkeleton.SHOULDER_L, pos, rot);
        }
        private Bone GetUpperArmRight()
        {
            Vector3 pos = GetUpperarmJoint(true);
            Vector3 target = someJoints[BipedSkeleton.LOWERARM_R];

            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, armForwardOrientationRight);
            return new Bone(BipedSkeleton.SHOULDER_R, pos, rot);
        }
        private Bone GetLowerArmLeft()
        {
            Vector3 pos = someJoints[BipedSkeleton.LOWERARM_L];
            Vector3 target = someJoints[BipedSkeleton.HAND_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, leftWristOrientation);
            return new Bone(BipedSkeleton.LOWERARM_L, pos, rot);
        }
        private Bone GetLowerArmRight()
        {
            Vector3 pos = someJoints[BipedSkeleton.LOWERARM_R];
            Vector3 target = someJoints[BipedSkeleton.HAND_R];

            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, rightWristOrientation);
            return new Bone(BipedSkeleton.LOWERARM_R, pos, rot);
        }
        private Bone GetHandLeft()
        {
            Vector3 pos = someJoints[BipedSkeleton.HAND_L];
            Vector3 target = markers[leftHand];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, leftWristOrientation);
            return new Bone(BipedSkeleton.HAND_L, pos, rot);
        }
        private Bone GetHandRight()
        {
            Vector3 pos = someJoints[BipedSkeleton.HAND_R];
            Vector3 target = markers[rightHand];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, rightWristOrientation);
            return new Bone(BipedSkeleton.HAND_R, pos, rot);
        }



        private Vector3 GetFemurJoint(bool isRightHip)
        {
            float Z, X, Y;
            X = 0.33f * pelvisWidth - 7.3f;
            Y = FemurYOffset();// -0.30f * pelvisWidth - 10.9f;
            Z = -0.24f * pelvisDepth - 9.9f;
            if (!isRightHip) X = -X;
            Vector3 pos = new Vector3(X, Y, Z) / 1000;
            pos = QuaternionHelper.Rotate(pelvisOrientation, pos);
            pos = ASISMid + pos;
            return pos;
        }
        private float FemurYOffset(){
            return -0.30f * pelvisWidth - 10.9f;
        }

        private Vector3 GetUpperarmJoint(bool isRightShoulder)
        {
            float x, y, z;

            x = 96.2f - 0.302f * chestDepth - 0.364f * height + 0.385f * mass;
            y = -66.32f + 0.30f * chestDepth - 0.432f * mass;
            z = 66.468f - 0.531f * shoulderWidth + 0.571f * mass;
            Vector3 res = new Vector3(x, y, z) / 1000;
            //TODO use shoulder orientation for something
            res += isRightShoulder ? markers[rightShoulder] : markers[leftShoulder];
            return res;

        }

        //TODO reflect rias/lias through plane YZ if lias/rias is missing
        private void HipOrientation()
        {
            Sacrum = markers[bodyBase];
            LIAS = markers[leftHip];
            RIAS = markers[rightHip];
            ASISMid = Vector3Helper.MidPoint(RIAS, LIAS);
            pelvisWidth = (LIAS - RIAS).Length * 1000; // To mm
            pelvisDepth = (ASISMid - Sacrum).Length * 1000; // To mm
            pelvisOrientation = QuaternionHelper.GetHipOrientation(Sacrum, LIAS, RIAS);
        }
        private void ArmOrientation()
        {

            Vector3 ulna = markers[rightElbow];
            Vector3 medialHumerus = markers[rightInnerElbow];
            Vector3 lateralHumerus = markers[rightOuterElbow];
            Vector3 midPoint = Vector3Helper.MidPoint(medialHumerus, lateralHumerus);
            Vector3 front = midPoint - ulna;
            front.Normalize();
            armForwardOrientationRight = front;

            ulna = markers[leftElbow];
            medialHumerus = markers[leftInnerElbow];
            lateralHumerus = markers[leftOuterElbow];
            midPoint = Vector3Helper.MidPoint(medialHumerus, lateralHumerus);
            front = midPoint - ulna;
            front.Normalize();
            armForwardOrientationLeft = front;

        }
        private void KneeOrientation()
        {

            Vector3 lower = markers[leftHeel];
            Vector3 upper = markers[bodyBase];
            Vector3 outer = someJoints[BipedSkeleton.LOWERLEG_L];
            Vector3 mid = Vector3Helper.MidPoint(upper, lower);
            Vector3 vec = outer - mid;
            vec.Normalize();

            kneeForwardOrientationLeft = vec;

            lower = markers[rightHeel];
            outer = someJoints[BipedSkeleton.LOWERLEG_R];
            mid = Vector3Helper.MidPoint(upper, lower);
            vec = outer - mid;
            Vector3 target = someJoints[BipedSkeleton.LOWERLEG_R];
            vec.Normalize();
            kneeForwardOrientationRight = vec;

        }
        private void WritsOrientation()
        {
            Vector3 littleFingerSide = markers[leftWrist];
            Vector3 thumbSide = markers[leftWristRadius];
            Vector3 front = thumbSide - littleFingerSide;
            front.Normalize();

            leftWristOrientation = front;

            littleFingerSide = markers[rightWrist];
            thumbSide = markers[rightWristRadius];
            front = thumbSide - littleFingerSide;
            front.Normalize();

            rightWristOrientation = front;

        }
        private void ShoulderData()
        {
            Vector3 chestV = markers[chest];
            Vector3 neckV = markers[neck];
            chestDepth = (chestV - neckV).Length * 1000;
            shoulderWidth = (Vector3Helper.MidPoint(chestV, neckV) - markers[leftShoulder]).Length * 1000;

            Vector3 headV = markers[head];
            Vector3 rightFootV = markers[rightFoot];
            float dist = ((rightFootV - headV).Length * 100) + 5;
            height = dist > height ? dist : height;
        }

        private Dictionary<string, Vector3> JointPossitions()
        {
            Dictionary<string, Vector3> dic = new Dictionary<string, Vector3>();
            ///////// HEAD ///////////////
            Vector3 front = markers[head];
            Vector3 right = markers[rightHead];
            Vector3 left = markers[leftHead];
            Vector3 pos = Vector3Helper.MidPoint(left, right, front); //TODO fix better head position
            dic.Add(BipedSkeleton.HEAD, pos);
            //////////////////////////////

            ///////// NECK ///////////////
            left = markers[neck];
            right = markers[chest];
            pos = Vector3Helper.MidPoint(left, right);
            dic.Add(BipedSkeleton.NECK, pos);
            //////////////////////////////

            ///////// HAND LEFT ///////////////
            pos = Vector3Helper.MidPoint(markers[leftWrist], markers[leftWristRadius]);
            dic.Add(BipedSkeleton.HAND_L,pos);
            //////////////////////////////

            /////////////// HAND RIGHT ///////////////
            pos = Vector3Helper.MidPoint(markers[rightWrist], markers[rightWristRadius]);
            dic.Add(BipedSkeleton.HAND_R, pos);
            //////////////////////////////

            /////////////// ELBOW LEFT ///////////////
            front = markers[leftElbow];
            left = markers[leftInnerElbow];
            right = markers[leftOuterElbow];
            pos = Vector3Helper.MidPoint(left, right, front);
            dic.Add(BipedSkeleton.LOWERARM_L, pos);
            //////////////////////////////

            /////////////// ELBOW RIGHT ///////////////
            front = markers[rightElbow];
            left = markers[rightInnerElbow];
            right = markers[rightOuterElbow];
            pos = Vector3Helper.MidPoint(left, right, front);
            dic.Add(BipedSkeleton.LOWERARM_R, pos);
            //////////////////////////////

            ///////// KNEE LEFT ///////////////
            front = markers[leftOuterKnee];
            left = markers[leftLowerKnee];
            right = markers[leftUpperKnee];
            pos = Vector3Helper.MidPoint(left, right, front);
            dic.Add(BipedSkeleton.LOWERLEG_L, pos);

            ///////// KNEE RIGHT ///////////////
            front = markers[rightOuterKnee];
            left = markers[rightLowerKnee];
            right = markers[rightUpperKnee];
            pos = Vector3Helper.MidPoint(left, right, front);
            dic.Add(BipedSkeleton.LOWERLEG_R, pos);
            /////////////// WRIST RIGHT ///////////////
            left = markers[rightFoot];
            right = markers[rightHeel];
            pos = Vector3Helper.MidPoint(left, right);
            dic.Add(BipedSkeleton.FOOT_R,pos);
            //////////////////////////////

            ///////// WRIST LEft ///////////////
            left = markers[leftFoot];
            right = markers[leftHeel];
            pos = Vector3Helper.MidPoint(left, right);
            dic.Add(BipedSkeleton.FOOT_L, pos);
            //////////////////////////////

            return dic;
        }





        string bodyBase = "SACR";
        string chest = "SME";
        string neck = "TV2";
        string spine = "TV12";
        string head = "SGL";
        string leftHead = "L_HEAD";
        string rightHead = "R_HEAD";

        string leftShoulder = "L_SAE";
        string leftElbow = "L_UOA";
        string leftInnerElbow = "L_HME";
        string leftOuterElbow = "L_HLE";
        string leftWrist = "L_USP";
        string leftWristRadius = "L_RSP";
        string leftHand = "L_HM2";

        string rightShoulder = "R_SAE";
        string rightElbow = "R_UOA";
        string rightInnerElbow = "R_HME";
        string rightOuterElbow = "R_HLE";
        string rightWrist = "R_USP";
        string rightWristRadius = "R_RSP";
        string rightHand = "R_HM2";

        string leftHip = "L_IAS";
        string leftUpperKnee = "L_PAS";
        string leftOuterKnee = "L_FLE";
        string leftLowerKnee = "L_TTC";
        string leftAnkle = "L_FAL";
        string leftHeel = "L_FCC";
        string leftFoot = "L_FM2";

        string rightHip = "R_IAS";
        string rightUpperKnee = "R_PAS";
        string rightOuterKnee = "R_FLE";
        string rightLowerKnee = "R_TTC";
        string rightAnkle = "R_FAL";
        string rightHeel = "R_FCC";
        string rightFoot = "R_FM2";
    }
}
