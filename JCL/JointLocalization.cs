using System.Collections.Generic;
using OpenTK;
using System.Linq;
namespace QTM2Unity
{
    class JointLocalization
    {
        #region varible necessary to estimate joints
        public float height = 150; // cm
        public float mass = 76; //TODO think about how to estimate mass
        public float marker2SpineDist = 0.05f; // m
        public float midHead2HeadJoint = 0.05f; // m
        public float spineLength = 0.05f; // m
        private float chestDepth = 100; //mm
        private float shoulderWidth = 150; // mm
        // varibles necessary to estimate hip joints
        private float pelvisDepth = 100; // mm
        private float pelvisWidth = 200; // mm
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
        private Vector3 pelvisfront;
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
            markersLastFrame = markers; //TODO Only uses SACRUM LIAS and RIAS as for now, only copy them? or just keep the Vectors RIAS L

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
            ChestOrientation(); 
            // get all joints, these are necessary for 
            joints = JointPossitions();

            ArmOrientation();
            WritsOrientation();
            // these orientation must be set after joints have been localized
            KneeOrientation();
            List<Bone> skele = new List<Bone>()
                {
                    GetPlevis(), 
                    GetSpineRoot(),     GetSpine1(),        GetSpineEnd(),      GetNeck(),          GetHead(),
                    GetUpperLegRight(), GetLowerLegRight(), GetAnkleRight(),    GetFootRight(),
                    GetUpperLegLeft(),  GetLowerLegLeft(),  GetAnkleLeft(),     GetFootLeft(),
                    GetShoulderRight(), GetUpperArmRight(), GetLowerArmRight(), GetWristRight(),    GetHandRight(),
                    GetShoulderLeft(),  GetUpperArmLeft(),  GetLowerArmLeft(),  GetWristLeft(),     GetHandLeft(),
                    
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
        private Bone GetSpineRoot()
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
            Vector3 target = joints[BipedSkeleton.SPINE3];
            Vector3 front = Vector3.Transform(Vector3.UnitZ, pelvisOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.SPINE1, pos, rot);
        }
        private Bone GetSpineEnd()
        {
            return new Bone(BipedSkeleton.SPINE3, joints[BipedSkeleton.SPINE3], chestOrientation);
        }

        private Bone GetNeck()
        {
            Vector3 pos = joints[BipedSkeleton.NECK];
            Vector3 target = joints[BipedSkeleton.HEAD];
            Vector3 front = Vector3.Transform(Vector3.UnitZ, chestOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.NECK, pos, rot);
        }
        private Bone GetHead()
        {
            return new Bone(BipedSkeleton.HEAD, joints[BipedSkeleton.HEAD], headOrientation);
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
        private Bone GetAnkleRight()
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
            return new Bone(BipedSkeleton.TOE_L, pos);
        }
        private Bone GetFootRight()
        {
            Vector3 pos = markers[rightFoot];
            return new Bone(BipedSkeleton.TOE_R, pos);
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
            return new Bone(BipedSkeleton.FINGER_L, pos);
        }
        private Bone GetHandRight()
        {
            Vector3 pos = markers[rightHand];
            return new Bone(BipedSkeleton.FINGER_R, pos);
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
            float y = 0,
                  x = isRightShoulder ? 50 : -50,
                  z = -chestDepth/2;
            Vector3 res = new Vector3(x, y, z) / 1000;
            res = QuaternionHelper.Rotate(chestOrientation, res);
            return res;
        }
        // specific for shoulder localization data, 
        private void ShoulderData()
        {
            Vector3 chestV = markers[chest];
            Vector3 neckV = markers[neck];

            // set chest depth
            var tmp = (chestV - neckV).Length * 1000;
            chestDepth = tmp > chestDepth && tmp < 500 ? tmp : chestDepth; // to mm

            // set shoulder width
            tmp = (Vector3Helper.MidPoint(chestV, neckV) - markers[leftShoulder]).Length * 1000;
            shoulderWidth = tmp > shoulderWidth && tmp < 400 ? tmp : shoulderWidth;

            Vector3 headV = markers[head];
            Vector3 rightFootV = markers[rightFoot];
            tmp = ((rightFootV - headV).Length * 100) + 10; // * 100 to cm, + 10 cm just a guess
            height = tmp > height && tmp < 2500 ? tmp : height;
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
                if (Sacrum.IsNaN() || LIAS.IsNaN() || RIAS.IsNaN())
                {
                    //TODO The classic this should not happen case, send NaN or throw something?
                }
            }
            ASISMid = Vector3Helper.MidPoint(RIAS, LIAS);
            pelvisWidth = (LIAS - RIAS).Length * 1000; //TODO set to constant sometime? same as all other constant length To mm 
            pelvisDepth = (ASISMid - Sacrum).Length * 1000; // To mm
            pelvisOrientation = QuaternionHelper.GetHipOrientation(Sacrum, LIAS, RIAS);
            pelvisfront = Vector3.Transform(Vector3.UnitZ, pelvisOrientation);
            pelvisfront.Normalize();

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
                    markers[bodyBase] = possiblePos1.MidPoint(possiblePos2);
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
            /*
            Fram:  TV2 till SME elller TV12 till SME
            Höger: LSAE (axel) till RSAE / LSEA till nacke / nacke till RSAE
            UP: bodybase till spineEnd/huvud/ryggrad
                ryggrad till nacke/huvud
                nacke till huvud
    */
            Vector3 neckPos = markers[neck];
            Vector3 chestPos = markers[chest];
            Vector3 rightShoulderPos = markers[rightShoulder];
            Vector3 leftShoulderPos = markers[leftShoulder];
            //Vector3 midspinePos = markers[spine];
            //SACRUM
            Vector3 front, up, right;

            Vector3 mid;

            /*
            up = neckPos - markers[spine];
            right = rightShoulderPos - leftShoulderPos;
            chestOrientation = QuaternionHelper.GetOrientationFromXY(up, right);
            Vector3 x = Vector3.Transform(Vector3.UnitX,q),
                y = Vector3.Transform(Vector3.UnitY,q),
                z = Vector3.Transform(Vector3.UnitZ,q);
            UnityEngine.Debug.Log(string.Format("right:{1} up:{0}", up, right));
            UnityEngine.Debug.Log(string.Format("X:{0} Y:{1} Z:{2}",x,y,z));
            return;
            */
            if (!chestPos.IsNaN() && !neckPos.IsNaN() && !rightShoulderPos.IsNaN() && !leftShoulderPos.IsNaN())
            {
                front = chestPos - neckPos;
                right = rightShoulderPos - leftShoulderPos;
                chestOrientation = QuaternionHelper.GetOrientationFromZX(front, right);
                return;
            }

            if (!chestPos.IsNaN() && !neckPos.IsNaN())
            {
                mid = Vector3Helper.MidPoint(chestPos,neckPos);
                front = chestPos - neckPos;
                if (!rightShoulderPos.IsNaN())
                {
                    right = rightShoulderPos - mid;
                    chestOrientation = QuaternionHelper.GetOrientationFromZX(front, right);
                    return;
                }
                if (!leftShoulderPos.IsNaN())
                {
                    right = mid - leftShoulderPos;
                    chestOrientation = QuaternionHelper.GetOrientationFromZX(front, right);
                    return;
                }
            }
            if (!rightShoulderPos.IsNaN() && !leftShoulderPos.IsNaN())
            {
                right = rightShoulderPos - leftShoulderPos;
                mid = Vector3Helper.MidPoint(rightShoulderPos, leftShoulderPos); 
                if (!chestPos.IsNaN() )
                {
                    front = chestPos - mid;
                    chestOrientation = QuaternionHelper.GetOrientationFromZX(front, right);
                    return;
                }
                if (!neckPos.IsNaN())
                {
                    front = mid - neckPos;
                    chestOrientation = QuaternionHelper.GetOrientationFromZX(front, right);
                    return;
                }
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

            /*
            Vector3 PAS = markers[leftUpperKnee];
            Vector3 TCC = markers[leftLowerKnee];
            Vector3 FLE = markers[leftOuterKnee];

            Vector3 mid = Vector3Helper.MidPoint(PAS, TCC);
            Vector3 right = mid - FLE;
            Vector3 up = PAS - TCC;

            Vector3 forward = Vector3.Cross(right, up);
            forward.Normalize();
            kneeForwardOrientationLeft = forward;

            PAS = markers[rightUpperKnee];
            TCC = markers[rightLowerKnee];
            FLE = markers[rightOuterKnee];

            mid = Vector3Helper.MidPoint(PAS, TCC);
            right = FLE - mid;
            up = PAS - TCC;

            forward = Vector3.Cross(right, up);
            forward.Normalize();
            kneeForwardOrientationRight = forward;
            */
             
            Vector3 upper = joints[BipedSkeleton.UPPERLEG_L];
            Vector3 lower = markers[leftAnkle]; 
            Vector3 forward = joints[BipedSkeleton.LOWERLEG_L];
            Vector3 mid = Vector3Helper.MidPoint(upper, lower);
            Vector3 vec = forward - mid;
            kneeForwardOrientationLeft = vec;

            upper = joints[BipedSkeleton.UPPERLEG_R];
            lower = markers[rightAnkle];
            forward = joints[BipedSkeleton.LOWERLEG_R];
            mid = Vector3Helper.MidPoint(upper, lower);
            vec = forward - mid;
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
            headOrientation = QuaternionHelper.GetOrientation(markers[head], markers[leftHead], markers[rightHead]);
        }
        #endregion
        #region JointPossitions
        private Dictionary<string, Vector3> JointPossitions()
        {
            Dictionary<string, Vector3> dic = new Dictionary<string, Vector3>();

            Vector3 pos, target,
                right, left, front, back,
                up, forward;


            /////////////// FEMUR LEFT ///////////////
            Vector3 lf = GetFemurJoint(false);
            dic.Add(BipedSkeleton.UPPERLEG_L, lf);
            //////////////////////////////////////////

            /////////////// FEMUR RIGHT ///////////////
            Vector3 rf = GetFemurJoint(true);
            dic.Add(BipedSkeleton.UPPERLEG_R, rf);
            //////////////////////////////////////////

            /////////////// SPINE0 //////////////
            Vector3 spine0 = Sacrum + pelvisfront * marker2SpineDist;
            /*
            Vector3 pelvY = Vector3.Transform(Vector3.UnitY, pelvisOrientation);
            Vector3 project = Vector3Helper.ProjectAndCreate(hip2spine, pelvY);
            Vector3 spine1 = pp + project;
             */
            dic.Add(BipedSkeleton.SPINE0, spine0);
            //////////////////////////////////////////

            /////////////// HIP ///////////////
            /*
             * i höjd med höfter, men så att Y axel pekar på Ryggbas
             */
            Vector3 pp = Vector3Helper.MidPoint(rf,lf);
            Vector3 pelvY = Vector3.Transform(Vector3.UnitY, pelvisOrientation);
            Vector3 hip2spine = spine0 - pp;
            Vector3 project = Vector3Helper.ProjectAndCreate(hip2spine, pelvY);
            pos = pp + hip2spine - project;
            dic.Add(BipedSkeleton.PELVIS, pos);
            //////////////////////////////////////////

            /////////////// SPINE1 //////////////
            /*
                Offset med z axel från ryggbas orientering med y axel roterad så TV12 mot TV2
             */
            pos = markers[spine];
            target = markers[neck];
            if (!target.IsNaN())
            {
                front = Vector3.Transform(Vector3.UnitZ, QuaternionHelper.LookAtUp(pos, target, pelvisfront));
                front.Normalize();
            }
            else
            {
                front = pelvisfront;
            }
            pos += front * marker2SpineDist;
            dic.Add(BipedSkeleton.SPINE1, pos);
            //////////////////////////////////////////

            /////////////// Spine end ///////////////
            back = markers[neck];
            front = markers[chest];
            Vector3 neckPos = back;
            if (!back.IsNaN())
            {
                if (!front.IsNaN())
                {
                    forward = front - back;
                    forward.Normalize();
                }
                else
                {
                    forward = Vector3.Transform(Vector3.UnitZ,chestOrientation);
                    forward.Normalize();
                }
                    neckPos += forward * marker2SpineDist;
            }
            else if (!front.IsNaN())
            {
                forward = Vector3.Transform(Vector3.UnitZ, chestOrientation);
                forward.Normalize();
                //neckPos = front + -forward * marker2SpineDist*3;
            }
            dic.Add(BipedSkeleton.SPINE3, neckPos);
            //////////////////////////////////////////


            /////////////// HEAD ///////////////
            front = markers[head];
            right = markers[rightHead];
            left = markers[leftHead];
            Vector3 headPos = Vector3Helper.MidPoint(left, right, front);
            //Move head position down
            Vector3 down = -Vector3.Transform(Vector3.UnitY, headOrientation);
            down.Normalize();
            headPos += down * midHead2HeadJoint;
            dic.Add(BipedSkeleton.HEAD, headPos);
            //////////////////////////////////////////

            /////////////// neck ///////////////
            up = Vector3.Transform(Vector3.UnitY, chestOrientation);
            up.Normalize();
            pos = neckPos + up * spineLength;
            dic.Add(BipedSkeleton.NECK, pos);
            //////////////////////////////////////////


            /////////////// SHOUDLERs ///////////////
            dic.Add(BipedSkeleton.SHOULDER_L, GetShoulderJoint(false) + markers[chest]); //neckPos); //Vector3.One);// 
            dic.Add(BipedSkeleton.SHOULDER_R, GetShoulderJoint(true) + markers[chest]); //neckPos); // Vector3.One);//
            //////////////////////////////

            /////////////// UPPER ARMS ///////////////
            dic.Add(BipedSkeleton.UPPERARM_L, GetUpperarmJoint(false));
            dic.Add(BipedSkeleton.UPPERARM_R, GetUpperarmJoint(true));
            //////////////////////////////////////////


            /////////////// HAND LEFT ///////////////
            pos = Vector3Helper.MidPoint(markers[leftWrist], markers[leftWristRadius]);
            dic.Add(BipedSkeleton.HAND_L,pos);
            //////////////////////////////////////////

            /////////////// HAND RIGHT ///////////////
            pos = Vector3Helper.MidPoint(markers[rightWrist], markers[rightWristRadius]);
            dic.Add(BipedSkeleton.HAND_R, pos);
            //////////////////////////////////////////

            /////////////// ELBOW LEFT ///////////////
            front = markers[leftElbow];
            left = markers[leftInnerElbow];
            right = markers[leftOuterElbow];
            pos = Vector3Helper.MidPoint(left, right, front);
            dic.Add(BipedSkeleton.LOWERARM_L, pos);
            //////////////////////////////////////////

            /////////////// ELBOW RIGHT ///////////////
            front = markers[rightElbow];
            left = markers[rightInnerElbow];
            right = markers[rightOuterElbow];
            pos = Vector3Helper.MidPoint(left, right, front);
            dic.Add(BipedSkeleton.LOWERARM_R, pos);
            //////////////////////////////////////////

            /////////////// KNEE LEFT ///////////////
            front = markers[leftOuterKnee];
            left = markers[leftLowerKnee];
            right = markers[leftUpperKnee];
            pos = Vector3Helper.MidPoint(left, right, front);
            dic.Add(BipedSkeleton.LOWERLEG_L, pos);
            //////////////////////////////////////////

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
