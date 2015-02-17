using System.Collections.Generic;
using OpenTK;
using System.Linq;
namespace QTM2Unity
{
    class JointLocalization
    {
        #region varible necessary to estimate joints
        public float height = 150;
        public const float mass = 76; //TODO think about how to estimate mass
        private float chestDepth = 10;
        private float shoulderWidth = 15;
        // varibles necessary to estimate hip joints
        private float pelvisDepth = 10;
        private float pelvisWidth = 20;
        #endregion

        #region important markers for hip joint
        private Vector3 ASISMid;
        private Vector3 RIAS;
        private Vector3 LIAS;
        private Vector3 Sacrum;
        #endregion
        private Dictionary<string, Vector3> joints;
        private Dictionary<string, Vector3> markers;
        private Dictionary<string, Vector3> markersLastFrame;
        private BipedSkeleton currentFrame;
        //private BipedSkeleton lastFrame;

        #region  important rotations to estimate joint
        private Quaternion pelvisOrientation;
        private Quaternion chestOrientation;
        private Quaternion headOrientation;
        #endregion

        #region important orientations for estimating rotations
        private Vector3 kneeForwardOrientationRight;
        private Vector3 kneeForwardOrientationLeft;
        private Vector3 armForwardOrientationRight;
        private Vector3 armForwardOrientationLeft;
        private Vector3 rightWristOrientation;
        private Vector3 leftWristOrientation;
        #endregion

        public void GetJointLocation(ref BipedSkeleton skel, List<LabeledMarker> markerData)
        {
            // Copy last frames markers
            markersLastFrame = markers;

            // Copy new markers to dictionary
            markers = markerData.ToDictionary(k => k.label, v => v.position);
            
            // Locate hiporientation, hip orientation is important for IK solver,
            // therefore, if markers are missing, locations are estimatetd
            HipOrientation();
            // collect data shoulder width, chest depth and subject length
            // this is necessary for shoulder joint localization 
            ShoulderData();

            // Head
            HeadOrientation();
            // get all joints, these are necessary for 
            joints = JointPossitions();

            ArmOrientation();
            WritsOrientation();
            // these orientation must be set after joints have been localized
            ChestOrientation(); 
            KneeOrientation();

            Bone pelvis = GetPlevis();

            Bone spine0 = GetSpine0();
            
            Bone spine1 = GetSpine1();
            
            Bone neck = GetNeck();
            
            Bone head = GetHead();
            
            
            Bone leftUpperLeg = GetUpperLegLeft();
            
            Bone leftLowerLeg = GetLowerLegLeft();
            
            Bone leftAnkle = GetAnkleLeft();
            
            Bone leftFoot = GetFootLeft();

            Bone rightUpperLeg = GetUpperLegRight();
            
            Bone rightLowerLeg = GetLowerLegRight();
            
            Bone rightAnkle = GetAnkelRight();
            
            Bone rightFoot = GetFootRight();


            Bone leftShoulder = GetShoulderLeft();
            
            Bone leftUpperArm = GetUpperArmLeft();
            
            Bone leftLowerArm = GetLowerArmLeft();
            
            Bone leftWrist = GetWristLeft();
            
            Bone leftHand = GetHandLeft();


            Bone rightShoulder = GetShoulderRight();
            
            Bone rightUpperArm = GetUpperArmRight();
            
            Bone rightLowerArm = GetLowerArmRight();
            
            Bone rightWrist = GetWristRight();
            
            Bone rightHand = GetHandRight();

            List<Bone> skele = new List<Bone>()
                {
                    pelvis, 
                    spine0,         spine1,   
                    neck,           head,
                    rightUpperLeg,  rightLowerLeg,  rightAnkle, rightFoot,
                    leftUpperLeg,   leftLowerLeg,   leftAnkle, leftFoot,
                    leftShoulder,   leftUpperArm,   leftLowerArm,   leftWrist, leftHand,
                    rightShoulder,  rightUpperArm,  rightLowerArm,  rightWrist, rightHand
                    
                };

            foreach (Bone b in skele) //TODO Better way to add bones
            {
                skel[b.Name] = b;
            }
        }

        #region pelsvis too head getters
        private Bone GetPlevis()
        {
            return new Bone(BipedSkeleton.PELVIS, joints[BipedSkeleton.PELVIS], pelvisOrientation);
        }
        private Bone GetSpine0()
        {
            Vector3 target = joints[BipedSkeleton.SPINE1];
            Vector3 front = Vector3.Transform(Vector3.UnitZ,pelvisOrientation);
            Vector3 pos = joints[BipedSkeleton.SPINE0];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.SPINE0, pos, rot);
        }
        private Bone GetSpine1()
        {
            Vector3 pos = joints[BipedSkeleton.SPINE1];
            Vector3 target = joints[BipedSkeleton.NECK];
            Vector3 front = Vector3.Transform(Vector3.UnitZ, pelvisOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.SPINE1, pos, rot);
        }
        private Bone GetLastSpine()
        {
            return new Bone(BipedSkeleton.SPINE3, joints[BipedSkeleton.SPINE3], chestOrientation);
        }

        private Bone GetNeck()
        {
            return new Bone(BipedSkeleton.NECK, joints[BipedSkeleton.NECK], chestOrientation);
        }
        private Bone GetHead()
        {
            Vector3 pos = joints[BipedSkeleton.HEAD];
            return new Bone(BipedSkeleton.HEAD, pos, headOrientation);
        }
        #endregion
        #region leg getters
        private Bone GetUpperLegLeft()
        {

            Vector3 pos = joints[BipedSkeleton.UPPERLEG_L]; ; 
            Vector3 target = joints[BipedSkeleton.LOWERLEG_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationLeft);
            return new Bone(BipedSkeleton.UPPERLEG_L, pos, rot);
        }
        private Bone GetUpperLegRight()
        {
            Vector3 pos = joints[BipedSkeleton.UPPERLEG_R];
            Vector3 target = joints[BipedSkeleton.LOWERLEG_R];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationRight); 
            return new Bone(BipedSkeleton.UPPERLEG_R, pos, rot);
        }

        private Bone GetLowerLegLeft()
        {
            Vector3 pos = joints[BipedSkeleton.LOWERLEG_L];
            Vector3 target = joints[BipedSkeleton.FOOT_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationLeft);
            return new Bone(BipedSkeleton.LOWERLEG_L, pos, rot);
        }
        private Bone GetLowerLegRight()
        {
            Vector3 pos = joints[BipedSkeleton.LOWERLEG_R];
            Vector3 target = joints[BipedSkeleton.FOOT_R];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationRight);
            return new Bone(BipedSkeleton.LOWERLEG_R, pos, rot);
        }
        private Bone GetAnkleLeft()
        {
            Vector3 pos = joints[BipedSkeleton.FOOT_L];
            Vector3 target = markers[leftFoot];
            Vector3 up = joints[BipedSkeleton.LOWERLEG_L] - pos;
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, up);
            return new Bone(BipedSkeleton.FOOT_L, pos, rot);
        }
        private Bone GetAnkelRight()
        {
            Vector3 pos = joints[BipedSkeleton.FOOT_R];
            Vector3 target = markers[rightFoot];
            Vector3 up = joints[BipedSkeleton.LOWERLEG_R] - pos;
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, up);
            return new Bone(BipedSkeleton.FOOT_R, pos, rot);
        }
        private Bone GetFootLeft()
        {
            Vector3 pos = markers[leftFoot];
            return new Bone(BipedSkeleton.FOOT_L, pos, Quaternion.Identity); //TODO Remove Quaternion
        }
        private Bone GetFootRight()
        {
            Vector3 pos = markers[rightFoot];
            return new Bone(BipedSkeleton.FOOT_L, pos, Quaternion.Identity); //TODO Remove Quaternion

        }
        #endregion
        #region arm getters
        private Bone GetShoulderLeft()
        {
            Vector3 pos = joints[BipedSkeleton.SHOULDER_L];
            Vector3 target = joints[BipedSkeleton.UPPERARM_L];
            Vector3 up = Vector3.Transform(Vector3.UnitZ, chestOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, up);
            return new Bone(BipedSkeleton.SHOULDER_L, pos, rot);
        }
        private Bone GetShoulderRight()
        {
            Vector3 pos = joints[BipedSkeleton.SHOULDER_R];
            Vector3 target = joints[BipedSkeleton.UPPERARM_R];
            Vector3 up = Vector3.Transform(Vector3.UnitZ, chestOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, up);
            return new Bone(BipedSkeleton.SHOULDER_R, pos, rot);
        }
        private Bone GetUpperArmLeft()
        {
            Vector3 pos = joints[BipedSkeleton.UPPERARM_L];
            Vector3 target = joints[BipedSkeleton.LOWERARM_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, armForwardOrientationLeft);
            return new Bone(BipedSkeleton.UPPERARM_L, pos, rot);
        }
        private Bone GetUpperArmRight()
        {
            Vector3 pos = joints[BipedSkeleton.UPPERARM_R];
            Vector3 target = joints[BipedSkeleton.LOWERARM_R];
            Quaternion rot = QuaternionHelper.LookAtRight(pos, target, armForwardOrientationRight);
            return new Bone(BipedSkeleton.UPPERARM_R, pos, rot);
        }
        private Bone GetLowerArmLeft()
        {
            Vector3 pos = joints[BipedSkeleton.LOWERARM_L];
            Vector3 target = joints[BipedSkeleton.HAND_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, leftWristOrientation);
            return new Bone(BipedSkeleton.LOWERARM_L, pos, rot);
        }
        private Bone GetLowerArmRight()
        {
            Vector3 pos = joints[BipedSkeleton.LOWERARM_R];
            Vector3 target = joints[BipedSkeleton.HAND_R];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, rightWristOrientation);
            return new Bone(BipedSkeleton.LOWERARM_R, pos, rot);
        }
        private Bone GetWristLeft()
        {
            Vector3 pos = joints[BipedSkeleton.HAND_L];
            Vector3 target = markers[leftHand];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, leftWristOrientation);
            return new Bone(BipedSkeleton.HAND_L, pos, rot);
        }
        private Bone GetWristRight()
        {
            Vector3 pos = joints[BipedSkeleton.HAND_R];
            Vector3 target = markers[rightHand];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, rightWristOrientation);
            return new Bone(BipedSkeleton.HAND_R, pos, rot);
        }
        private Bone GetHandLeft()
        {
            Vector3 pos = markers[leftHand];
            return new Bone(BipedSkeleton.HAND_L, pos, Quaternion.Identity);
        }
        private Bone GetHandRight()
        {
            Vector3 pos = markers[rightHand];
            return new Bone(BipedSkeleton.HAND_R, pos, Quaternion.Identity);
        }
        #endregion
        #region Special joints position Getters
        private Vector3 GetFemurJoint(bool isRightHip)
        {
            float Z, X, Y;
            X = 0.33f * pelvisWidth - 7.3f;
            Y = -0.30f * pelvisWidth - 10.9f;
            Z = -0.24f * pelvisDepth - 9.9f;
            if (!isRightHip) X = -X;
            Vector3 pos = new Vector3(X, Y, Z) / 1000;
            pos = QuaternionHelper.Rotate(pelvisOrientation, pos);
            pos = ASISMid + pos;
            return pos;
        }
        private Vector3 GetUpperarmJoint(bool isRightShoulder)
        {
            float
                x = 96.2f - 0.302f * chestDepth - 0.364f * height + 0.385f * mass,
                y = -66.32f + 0.30f * chestDepth - 0.432f * mass,
                z = 66.468f - 0.531f * shoulderWidth + 0.571f * mass;
            Vector3 res = new Vector3(x, y, z) / 1000;
            res = QuaternionHelper.Rotate(pelvisOrientation, res);
            res += isRightShoulder ? markers[rightShoulder] : markers[leftShoulder];
            return res;
        }
        private Vector3 GetShoulderJoint(bool isRightShoulder)
        {
            float y = -50,
                  x = isRightShoulder ? -y : y,
                  z = 0;
            Vector3 res = new Vector3(x, y, z) / 1000;
            res = QuaternionHelper.Rotate(chestOrientation, res);
            return res;
        }
        // specific for shoulder localization data, 
        //TODO FIX insane values
        private void ShoulderData()
        {
            Vector3 chestV = markers[chest];
            if (!markers.ContainsKey(neck))
            {
                markers.Add(neck, markers[chest]);
            }
            Vector3 neckV = markers[neck];

            // set chest depth
            var tmp = (chestV - neckV).Length * 1000;
            //if (tmp > chestDepth) UnityEngine.Debug.Log("Chest depth: " + tmp + "mm");
            chestDepth = tmp > chestDepth ? tmp : chestDepth; // to mm

            // set shoulder width
            tmp = (Vector3Helper.MidPoint(chestV, neckV) - markers[leftShoulder]).Length * 1000;
            //if (tmp > shoulderWidth) UnityEngine.Debug.Log("Shoulder width: " + tmp + "mm");
            shoulderWidth = tmp > shoulderWidth ? tmp : shoulderWidth;

            Vector3 headV = markers[head];
            Vector3 rightFootV = markers[rightFoot];
            tmp = ((rightFootV - headV).Length * 100) + 5; // * 100 to cm, + 5 just a guess
            //if (tmp > height) UnityEngine.Debug.Log("Height:" + tmp + "cm");
            height = tmp > height ? tmp : height;
        }
        #endregion
        #region Joint orientation 
        private void HipOrientation()
        {
            Sacrum = markers[bodyBase];
            LIAS = markers[leftHip];
            RIAS = markers[rightHip];
            if (Sacrum.IsNaN() || LIAS.IsNaN() || RIAS.IsNaN())
            {
                MissingEssientialMarkers();
            }
            ASISMid = Vector3Helper.MidPoint(RIAS, LIAS);
            pelvisWidth = (LIAS - RIAS).Length * 1000; // To mm
            pelvisDepth = (ASISMid - Sacrum).Length * 1000; // To mm
            pelvisOrientation = QuaternionHelper.GetHipOrientation(Sacrum, LIAS, RIAS);
        }


        #region Hip prediction wizard
        private void MissingEssientialMarkers()
        {
            Vector3 dirVec1, dirVec2, possiblePos1, possiblePos2,
                    sacrumlastFrame = markersLastFrame[bodyBase],
                    liasLastFrame   = markersLastFrame[leftHip],
                    riasLastFrame   = markersLastFrame[rightHip];
            bool s = !Sacrum.IsNaN(),
                 r = !RIAS.IsNaN(),
                 l = !LIAS.IsNaN();
            if (s) // sacrum exists
            {
                if (r) // sacrum and rias exist, lias missing
                {
                    dirVec1 = liasLastFrame - sacrumlastFrame; // vector from sacrum too lias in last frame
                    dirVec2 = liasLastFrame - riasLastFrame;
                    possiblePos1 = Sacrum + dirVec1; // add vector from sacrum too lias last frame to this frames' sacrum
                    possiblePos2 = RIAS + dirVec2;
                    markers[leftHip] = possiblePos1.MidPoint(possiblePos2); // get mid point of possible positions

                }
                else if (l) // sacrum  and lias exists, rias missing
                {
                    dirVec1 = riasLastFrame - sacrumlastFrame;
                    dirVec2 = riasLastFrame - liasLastFrame;
                    possiblePos1 = Sacrum + dirVec1;
                    possiblePos2 = LIAS + dirVec2;
                    markers[rightHip] = possiblePos1.MidPoint(possiblePos2);
                    //UnityEngine.Debug.Log(markers[rightHip]);
                }
                else // only sacrum exists, lias and rias missing
                {
                    dirVec1 = riasLastFrame - sacrumlastFrame;
                    markers[rightHip] = Sacrum + dirVec1;
                    dirVec2 = liasLastFrame- sacrumlastFrame;
                    markers[leftHip] = Sacrum + dirVec2;
                }
            }
            else if (r) // rias exists, sacrum missing
            {
                if (l) // rias and ias exists, sacrum missing
                {
                    dirVec1 = sacrumlastFrame - riasLastFrame;
                    dirVec2 = sacrumlastFrame - liasLastFrame;
                    possiblePos1 = RIAS + dirVec1;
                    possiblePos2 = LIAS + dirVec2;
                    markers[rightHip] = possiblePos1.MidPoint(possiblePos2);
                }
                else // only rias exists, lias and sacrum missing
                {
                    dirVec1 = sacrumlastFrame - riasLastFrame;
                    markers[bodyBase] = RIAS + dirVec1;
                    dirVec2 = liasLastFrame - riasLastFrame;
                    markers[leftHip] = RIAS + dirVec2;
                }
            }
            else if (l) // only lias exists, rias and sacrum missing
            {
                dirVec1 = sacrumlastFrame - liasLastFrame;
                markers[bodyBase] = LIAS + dirVec1;
                dirVec2 = riasLastFrame - liasLastFrame;
                markers[rightHip] = LIAS + dirVec2;
            }
            else // all markers missing
            {
                //UnityEngine.Debug.LogError("FUUUUUCK! EVERYTHING IS GONE");
                markers[rightHip] = riasLastFrame;
                markers[leftHip] = liasLastFrame;
                markers[bodyBase] = sacrumlastFrame;
            }
            RIAS = markers[rightHip];
            LIAS = markers[leftHip];
            Sacrum = markers[bodyBase];
        }
        #endregion
        // SET after joints!!
        private void ChestOrientation()
        {
            //TODO New chest orientation

            Vector3 pos = joints[BipedSkeleton.NECK];
            Vector3 target = joints[BipedSkeleton.HEAD];
            Vector3 front = markers[chest] - markers[neck];
            if (!front.IsNaN())
            {
                chestOrientation = QuaternionHelper.LookAtUp(pos, target, front);
            }
            else
            {
                Vector3 right = markers[rightShoulder] - markers[leftShoulder];
                chestOrientation = QuaternionHelper.LookAtRight(pos, target, right);
            }
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
            Vector3 lower = markers[leftAnkle]; 
            Vector3 upper = joints[BipedSkeleton.UPPERLEG_L];
            Vector3 forward = joints[BipedSkeleton.LOWERLEG_L];
            Vector3 mid = Vector3Helper.MidPoint(upper, lower);
            Vector3 vec = forward - mid;
            
            kneeForwardOrientationLeft = vec;

            upper = joints[BipedSkeleton.UPPERLEG_R];
            forward = joints[BipedSkeleton.LOWERLEG_R];
            lower = markers[rightAnkle];
            mid = Vector3Helper.MidPoint(upper, lower);
            vec = forward - mid;
            Vector3 target = joints[BipedSkeleton.LOWERLEG_R];
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
        private void HeadOrientation()
        {
            Vector3 headV = markers[head];
            Vector3 rightHeadV = markers[rightHead];
            Vector3 leftHeadV = markers[leftHead];
            headOrientation = QuaternionHelper.GetOrientation(headV, leftHeadV, rightHeadV);
        }
        #endregion
        #region JointPossitions
        private Dictionary<string, Vector3> JointPossitions()
        {
            Dictionary<string, Vector3> dic = new Dictionary<string, Vector3>();
            Vector3 pos;
            Vector3 front;
            Vector3 right;
            Vector3 left;

            /////////////// FEMUR LEFT ///////////////
            Vector3 lf = GetFemurJoint(false);
            dic.Add(BipedSkeleton.UPPERLEG_L, lf);
            //////////////////////////////

            /////////////// FEMUR RIGHT ///////////////
            Vector3 rf = GetFemurJoint(true);
            dic.Add(BipedSkeleton.UPPERLEG_R, rf);
            //////////////////////////////

            /////////////// HIP ///////////////
            Vector3 pp = Vector3Helper.MidPoint(rf,lf);
            dic.Add(BipedSkeleton.PELVIS, pp);
            //////////////////////////////

            /////////////// SPINE0 //////////////
            Vector3 pelvY = Vector3.Transform(Vector3.UnitY, pelvisOrientation);
            Vector3 hip2spine = Sacrum - pp;
            Vector3 project = Vector3Helper.ProjectAndCreate(hip2spine, pelvY);
            Vector3 spine1 = pp + project;
            dic.Add(BipedSkeleton.SPINE0, spine1);
            //////////////////////////////

            /////////////// SPINE1 //////////////
            if (!markers.ContainsKey(spine)) markers.Add(spine,markers[bodyBase]);
            pos = markers[spine];
            front = Vector3.Transform(Vector3.UnitZ, pelvisOrientation);
            pos = pos + front * 0.1f;
            dic.Add(BipedSkeleton.SPINE1, pos);
            //////////////////////////////

            /////////////// HEAD ///////////////
            front = markers[head];
            right = markers[rightHead];
            left = markers[leftHead];
            pos = Vector3Helper.MidPoint(left, right, front);
            //Move head position down
            Vector3 down = -Vector3.Transform(Vector3.UnitZ, headOrientation);
            down.Normalize();
            pos += pos + down * 0.05f; // 5 cm? maybe
            dic.Add(BipedSkeleton.HEAD, pos);
            //////////////////////////////

            /////////////// Spine end ///////////////
            left = markers[neck];
            right = markers[chest];
            Vector3 neckPos = Vector3Helper.PointBetween(left, right, 0.8f);
            dic.Add(BipedSkeleton.SPINE3, neckPos);
            //////////////////////////////

            /////////////// SHOUDLERs ///////////////
            dic.Add(BipedSkeleton.SHOULDER_L, neckPos);// GetShoulderJoint(false) + neckPos);
            dic.Add(BipedSkeleton.SHOULDER_R, neckPos);//GetShoulderJoint(true) + neckPos);
            //////////////////////////////

            /////////////// UPPER ARMS ///////////////
            dic.Add(BipedSkeleton.UPPERARM_L, GetUpperarmJoint(false));
            dic.Add(BipedSkeleton.UPPERARM_R, GetUpperarmJoint(true));
            //////////////////////////////


            /////////////// HAND LEFT ///////////////
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

            /////////////// KNEE LEFT ///////////////
            front = markers[leftOuterKnee];
            left = markers[leftLowerKnee];
            right = markers[leftUpperKnee];
            pos = Vector3Helper.MidPoint(left, right, front);
            dic.Add(BipedSkeleton.LOWERLEG_L, pos);

            /////////////// KNEE RIGHT ///////////////
            front = markers[rightOuterKnee];
            left = markers[rightLowerKnee];
            right = markers[rightUpperKnee];
            pos = Vector3Helper.MidPoint(left, right, front);
            dic.Add(BipedSkeleton.LOWERLEG_R, pos);
            //////////////////////////////

            /////////////// FOOT RIGHT ///////////////
            left = markers[rightAnkle];
            right = markers[rightHeel];
            pos = Vector3Helper.MidPoint(left, right);
            dic.Add(BipedSkeleton.FOOT_R,pos);
            //////////////////////////////

            /////////////// FOOT LEft ///////////////
            left = markers[leftAnkle];
            right = markers[leftHeel];
            pos = Vector3Helper.MidPoint(left, right);
            dic.Add(BipedSkeleton.FOOT_L, pos);
            //////////////////////////////

            return dic;
        }
        #endregion
        #region skin markers
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
        #endregion
    }
}
