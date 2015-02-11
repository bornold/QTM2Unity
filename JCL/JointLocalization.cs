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
        private Quaternion chestOrientation;
        private Vector3 kneeForwardOrientationRight;
        private Vector3 kneeForwardOrientationLeft;
        private Vector3 armForwardOrientationRight;
        private Vector3 armForwardOrientationLeft;
        private Vector3 rightWristOrientation;
        private Vector3 leftWristOrientation;
        private BipedSkeleton lastFrame;
        private BipedSkeleton currentFrame;

        private const float blend = 0.1f;
        public BipedSkeleton GetJointLocation(List<LabeledMarker> markerData)
        {
            lastFrame = currentFrame;
            markers = markerData.ToDictionary(k => k.label, v => v.position);
            HipOrientation();
            ShoulderData();
            someJoints = JointPossitions();
            ChestOrientation(); // Set after Joints!
            KneeOrientation();
            ArmOrientation();
            WritsOrientation();

            Bone pelvis = GetPlevis();

            Bone spine0 = GetSpine0();
            spine0.Parent = pelvis;
            Bone spine1 = GetSpine1();
            spine1.Parent = spine0;
            Bone neck = GetNeck();
            neck.Parent = spine1;
            Bone head = GetHead();
            head.Parent = neck;
            
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
                    spine0, spine1,   
                    neck,         head,
                    rightUpperLeg, rightLowerLeg, rightFoot,
                    leftUpperLeg,  leftLowerLeg,  leftFoot,
                    leftShoulder,  
                    leftUpperArm,  leftLowerArm, leftHand,
                    rightShoulder, 
                    rightUpperArm, rightLowerArm, rightHand
                    
                });
            return currentFrame;
        }

        private Bone GetPlevis()
        {
            return new Bone(BipedSkeleton.PELVIS, someJoints[BipedSkeleton.PELVIS], pelvisOrientation);
        }
        private Bone GetSpine0()
        {
            Vector3 target = someJoints[BipedSkeleton.SPINE1];
            Vector3 front = Vector3.Transform(Vector3.UnitZ,pelvisOrientation);
            Vector3 pos = someJoints[BipedSkeleton.SPINE0];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.SPINE0, pos, rot);
        }
        private Bone GetSpine1()
        {
            Vector3 pos = someJoints[BipedSkeleton.SPINE1];
            Vector3 target = someJoints[BipedSkeleton.NECK];
            Vector3 front = Vector3.Transform(Vector3.UnitZ, pelvisOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.SPINE1, pos, rot);
        }
        private Bone GetNeck()
        {
            return new Bone(BipedSkeleton.NECK, someJoints[BipedSkeleton.NECK], chestOrientation);
        }
        // SET after joints!!
        private void GetNeckRotation() {
            Vector3 pos = someJoints[BipedSkeleton.NECK];
            Vector3 target = someJoints[BipedSkeleton.HEAD];
            Vector3 front = markers[chest] - markers[neck];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front); //TODO Fix good neck rotation
        }

        private Bone GetHead()
        {
            Vector3 headV = markers[head];
            Vector3 rightHeadV = markers[rightHead];
            Vector3 leftHeadV = markers[leftHead];
            Vector3 pos = someJoints[BipedSkeleton.HEAD];
            Quaternion orientation = QuaternionHelper.GetOrientation(headV, leftHeadV, rightHeadV);
            return new Bone(BipedSkeleton.HEAD, pos, orientation);

        }
        private Bone GetUpperLegLeft()
        {

            Vector3 pos = someJoints[BipedSkeleton.UPPERLEG_L]; ; 
            Vector3 target = someJoints[BipedSkeleton.LOWERLEG_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationLeft);
            return new Bone(BipedSkeleton.UPPERLEG_L, pos, rot);
        }
        private Bone GetUpperLegRight()
        {
            Vector3 pos = someJoints[BipedSkeleton.UPPERLEG_R];
            Vector3 target = someJoints[BipedSkeleton.LOWERLEG_R];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationRight); 
            return new Bone(BipedSkeleton.UPPERLEG_R, pos, rot);
        }

        private Bone GetLowerLegLeft()
        {
            Vector3 pos = someJoints[BipedSkeleton.LOWERLEG_L];
            Vector3 target = someJoints[BipedSkeleton.FOOT_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationLeft);
            return new Bone(BipedSkeleton.LOWERLEG_L, pos, rot);
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
            Vector3 pos = someJoints[BipedSkeleton.SHOULDER_L];
            Vector3 target = someJoints[BipedSkeleton.UPPERARM_L];
            Vector3 up = Vector3.Transform(Vector3.UnitZ, chestOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, up);
            return new Bone(BipedSkeleton.SHOULDER_L, pos, rot);
        }
        private Bone GetShoulderRight()
        {
            Vector3 pos = someJoints[BipedSkeleton.SHOULDER_R];
            Vector3 target = someJoints[BipedSkeleton.UPPERARM_R];
            Vector3 up = Vector3.Transform(Vector3.UnitZ, chestOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, up);
            return new Bone(BipedSkeleton.SHOULDER_R, pos, rot);
        }
        private Bone GetUpperArmLeft()
        {
            Vector3 pos = someJoints[BipedSkeleton.UPPERARM_L];
            Vector3 target = someJoints[BipedSkeleton.LOWERARM_L];
            Quaternion rot = QuaternionHelper.LookAtRight(pos, target, armForwardOrientationLeft);
            return new Bone(BipedSkeleton.UPPERARM_L, pos, rot);
        }
        private Bone GetUpperArmRight()
        {
            Vector3 pos = someJoints[BipedSkeleton.UPPERARM_R];
            Vector3 target = someJoints[BipedSkeleton.LOWERARM_R];
            Quaternion rot = QuaternionHelper.LookAtRight(pos, target, armForwardOrientationRight);
            return new Bone(BipedSkeleton.UPPERARM_R, pos, rot);
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
            X = FemurXOffset(); //0.33f * pelvisWidth - 7.3f;
            Y = FemurYOffset(); //-0.30f * pelvisWidth - 10.9f;
            Z = FemurZOffset(); //-0.24f * pelvisDepth - 9.9f;
            if (!isRightHip) X = -X;
            Vector3 pos = new Vector3(X, Y, Z) / 1000;
            pos = QuaternionHelper.Rotate(pelvisOrientation, pos);
            pos = ASISMid + pos;
            return pos;
        }
        private float FemurXOffset()
        {
            return 0.33f * pelvisWidth - 7.3f;
        }
        private float FemurYOffset(){
            return -0.30f * pelvisWidth - 10.9f;
        }
        private float FemurZOffset()
        {
            return -0.24f * pelvisDepth - 9.9f;
        }
        private Vector3 GetUpperarmJoint(bool isRightShoulder)
        {
            float x, y, z;

            x = 96.2f - 0.302f * chestDepth - 0.364f * height + 0.385f * mass;
            y = -66.32f + 0.30f * chestDepth - 0.432f * mass;
            z = 66.468f - 0.531f * shoulderWidth + 0.571f * mass;
            Vector3 res = new Vector3(x, y, z) / 1000;
            res = QuaternionHelper.Rotate(chestOrientation, res);
            res += isRightShoulder ? markers[rightShoulder] : markers[leftShoulder];
            return res;
        }
        private Vector3 GetShoulderJoint(bool isRightShoulder)
        {
            float x, y, z;

            x = 50;
            x = isRightShoulder ? x : -x;
            y = -50f;
            z = 0;
            Vector3 res = new Vector3(x, y, z) / 1000;
            res = QuaternionHelper.Rotate(chestOrientation, res);
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
        // SET after joints!!
        private void ChestOrientation()
        {

            //TODO New chest orientation
            Vector3 pos = someJoints[BipedSkeleton.NECK];
            Vector3 target = someJoints[BipedSkeleton.HEAD];
            Vector3 front = markers[chest] - markers[neck];
            chestOrientation = QuaternionHelper.LookAtUp(pos, target, front); //TODO Fix good neck rotation
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
            Vector3 lower = markers[leftAnkle]; //markers[leftHeel];
            Vector3 upper = someJoints[BipedSkeleton.UPPERLEG_L];//markers[bodyBase];
            Vector3 forward = someJoints[BipedSkeleton.LOWERLEG_L];
            Vector3 mid = Vector3Helper.MidPoint(upper, lower);
            Vector3 vec = forward - mid;
            
            kneeForwardOrientationLeft = vec;

            upper = someJoints[BipedSkeleton.UPPERLEG_R];//markers[bodyBase];
            forward = someJoints[BipedSkeleton.LOWERLEG_R];
            lower = markers[rightAnkle];
            mid = Vector3Helper.MidPoint(upper, lower);
            vec = forward - mid;
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
            chestDepth = (chestV - neckV).Length * 1000; // to mm
            shoulderWidth = (Vector3Helper.MidPoint(chestV, neckV) - markers[leftShoulder]).Length * 1000; // * 1000 to mm

            Vector3 headV = markers[head];
            Vector3 rightFootV = markers[rightFoot];
            float dist = ((rightFootV - headV).Length * 100) + 5; // * 100 to cm, + 5 just a guess
            height = dist > height ? dist : height;
        }

        private Dictionary<string, Vector3> JointPossitions()
        {
            Dictionary<string, Vector3> dic = new Dictionary<string, Vector3>();

            ///////// NECK ///////////////
            Vector3 left = markers[neck];
            Vector3 right = markers[chest];
            Vector3 neckPos = Vector3Helper.PointBetween(left, right, 0.8f);
            dic.Add(BipedSkeleton.NECK, neckPos);
            //////////////////////////////

            // SHOUDLER
            dic.Add(BipedSkeleton.SHOULDER_L, GetShoulderJoint(false) + neckPos);
            dic.Add(BipedSkeleton.SHOULDER_R, GetShoulderJoint(true) + neckPos);
            dic.Add(BipedSkeleton.UPPERARM_L, GetUpperarmJoint(false));
            dic.Add(BipedSkeleton.UPPERARM_R, GetUpperarmJoint(true));
            ///////// HEAD ///////////////
            Vector3 front = markers[head];
            right = markers[rightHead];
            left = markers[leftHead];
            Vector3 pos = Vector3Helper.MidPoint(left, right, front); //TODO fix better head position
            pos = Vector3Helper.PointBetween(neckPos, pos, 0.5f);
            dic.Add(BipedSkeleton.HEAD, pos);

            /////////////// FEMUR LEFT ///////////////
            Vector3 lf = GetFemurJoint(false);
            dic.Add(BipedSkeleton.UPPERLEG_L, lf);
            /////////////// FEMUR RIGHT ///////////////
            Vector3 rf = GetFemurJoint(true);
            dic.Add(BipedSkeleton.UPPERLEG_R, rf);

            /////////////// HIP ///////////////
            Vector3 pp = Vector3Helper.MidPoint(rf,lf);
            dic.Add(BipedSkeleton.PELVIS, pp);

            /////////////// SPINE1 //////////////
            Vector3 pelvY = Vector3.Transform(Vector3.UnitY, pelvisOrientation);
            Vector3 hip2spine = Sacrum - pp;
            Vector3 project = Vector3Helper.ProjectAndCreate(hip2spine, pelvY);
            Vector3 spine1 = pp + project;
            dic.Add(BipedSkeleton.SPINE0, spine1);


            /////////////// SPINE3 //////////////
            Vector3 spine1Toneck = neckPos - spine1;
            Vector3 spineMarkerToNeckMarker = markers[neck] - markers[spine];
            project = Vector3Helper.ProjectAndCreate(spineMarkerToNeckMarker, spine1Toneck);

            pos = markers[spine];
            front = Vector3.Transform(Vector3.UnitZ, pelvisOrientation);
            pos = pos + front * 0.1f;
            dic.Add(BipedSkeleton.SPINE1, pos);


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


            /////////////// FOOT RIGHT ///////////////
            left = markers[rightAnkle];
            right = markers[rightHeel];
            pos = Vector3Helper.MidPoint(left, right);
            dic.Add(BipedSkeleton.FOOT_R,pos);
            //////////////////////////////

            ///////// FOOT LEft ///////////////
            left = markers[leftAnkle];
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
