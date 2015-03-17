using System.Collections.Generic;
using OpenTK;
using System.Linq;
namespace QTM2Unity
{
    class JointLocalization
    {
        #region varible necessary to estimate joints
        public float height = 150; // cm
        public float mass = 40; 
        public float marker2SpineDist = 0.05f; // m
        public float midHead2HeadJoint = 0.05f; // m
        public float spineLength = 0.05f; // m

        //Max width and depth
        private float chestDepth = 50; //mm
        private float shoulderWidth = 50; // mm
        private float BMI = 24;
        #endregion

        #region important markers for hip joint
        private Vector3 RIAS;
        private Vector3 LIAS;
        private Vector3 Sacrum;
        #endregion

        private Dictionary<string, Vector3> joints;
        private Dictionary<string, Vector3> markers;
        private Dictionary<string, Vector3> markersLastFrame;
        private BipedSkeleton currentFrame;

        public BipedSkeleton GetJointLocation(List<LabeledMarker> markerData)
        {
            // Copy last frames markers
            markersLastFrame = markers;

            // Copy new markers to dictionary
            markers = markerData.ToDictionary(k => k.label, v => v.position);
            // collect data from markers about body proportions
            // this is necessary for shoulder joint localization 
            BodyData();
            // Locate hiporientation, hip orientation is important for IK solver,
            Sacrum = markers[bodyBase];
            LIAS = markers[leftHip];
            RIAS = markers[rightHip];

            // therefore, if markers are missing, locations are estimatetd
            Quaternion pelvisOrientation = HipOrientation();

            Quaternion chestOrientation = ChestOrientation(); 
            // get all joints
            joints = JointPossitions(pelvisOrientation,chestOrientation);

            Vector3 rightKneeRot = KneeOrientationRight();
            Vector3 leftKneeRot = KneeOrientationLeft();
            List<Bone> skele = new List<Bone>()
                {
                    //GetPlevis(pelvisOrientation), 
                    //GetSpineRoot(pelvisOrientation), GetSpine1(pelvisOrientation),
                    //GetSpineEnd(chestOrientation),      GetNeck(chestOrientation),          GetHead(),
                    //GetUpperLegRight(rightKneeRot),     GetLowerLegRight(rightKneeRot), GetAnkleRight(),    GetFootRight(),
                    //GetUpperLegLeft(leftKneeRot),       GetLowerLegLeft(leftKneeRot),   GetAnkleLeft(),     GetFootLeft(),
                   // GetShoulderRight(chestOrientation), GetUpperArmRight(),             GetLowerArmRight(), GetWristRight(),    GetHandRight(),
                   // GetShoulderLeft(chestOrientation),  GetUpperArmLeft(),  GetLowerArmLeft(),  GetWristLeft(),     GetHandLeft(),
                    
                };

            TreeNode<Bone> root = new TreeNode<Bone>(GetPlevis(pelvisOrientation));
            #region bone structure
            {
                #region legs left
                TreeNode<Bone> upperlegleft = root.AddChild(GetUpperLegLeft(leftKneeRot));
                {
                    TreeNode<Bone> lowerlegleft = upperlegleft.AddChild(GetLowerLegLeft(leftKneeRot));
                    {
                        TreeNode<Bone> footleft = lowerlegleft.AddChild(GetAnkleLeft());
                        {
                            TreeNode<Bone> toeleft = footleft.AddChild(GetFootLeft());
                        }
                    }
                }
                #endregion
                #region legs right
                TreeNode<Bone> upperlegright = root.AddChild(GetUpperLegRight(rightKneeRot));
                {
                    TreeNode<Bone> lowerlegright = upperlegright.AddChild(GetLowerLegRight(rightKneeRot));
                    {
                        TreeNode<Bone> footright = lowerlegright.AddChild(GetAnkleRight());
                        {
                            TreeNode<Bone> toeright = footright.AddChild(GetFootRight());
                        }
                    }
                }
                #endregion
                #region upper body
                #region spine and head
                TreeNode<Bone> spine0 = root.AddChild(GetSpineRoot(pelvisOrientation));
                {
                    TreeNode<Bone> spine1 = spine0.AddChild(GetSpine1(pelvisOrientation));
                    {
                        //     TreeNode<Bone> spine2 = spine1.AddChild(new Bone(SPINE2));
                        //     {
                        TreeNode<Bone> spine3 = spine1.AddChild(GetSpineEnd(chestOrientation));
                        {
                            TreeNode<Bone> neck = spine3.AddChild(GetNeck(chestOrientation));
                            {
                                TreeNode<Bone> head = neck.AddChild(GetHead());
                            }
                #endregion
                            #region arm left
                            TreeNode<Bone> shoulderleft = spine3.AddChild(GetShoulderLeft(chestOrientation));
                            {
                                TreeNode<Bone> upperarmleft = shoulderleft.AddChild(GetUpperArmLeft());
                                {
                                    TreeNode<Bone> lowerarmleft = upperarmleft.AddChild(GetLowerArmLeft());
                                    {
                                        TreeNode<Bone> handLeft = lowerarmleft.AddChild(GetWristLeft());
                                        {
                                            TreeNode<Bone> fingerLeft = handLeft.AddChild(GetHandLeft());
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region arm right
                            TreeNode<Bone> shoulderRight = spine3.AddChild(GetShoulderRight(chestOrientation));
                            {
                                TreeNode<Bone> upperarmleft = shoulderRight.AddChild(GetUpperArmRight());
                                {
                                    TreeNode<Bone> lowerarmleft = upperarmleft.AddChild(GetLowerArmRight());
                                    {
                                        TreeNode<Bone> handLeft = lowerarmleft.AddChild(GetWristRight());
                                        {
                                            TreeNode<Bone> fingerLeft = handLeft.AddChild(GetHandRight());
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
            }
            #endregion
            BipedSkeleton skel = new BipedSkeleton();
            foreach (Bone b in skele) //TODO Better way to add bones
            {
                skel[b.Name] = b;
            }
            return new BipedSkeleton(root);
        }

        private void BodyData()
        {
            // set chest depth
            var tmp = (markers[chest] - markers[neck]).Length * 1000; // to mm
            chestDepth = tmp > chestDepth && tmp < 500 ? tmp : chestDepth; // half a meter in chest depth? fatty...

            // set shoulder width
            tmp = (markers[leftShoulder] - markers[rightShoulder]).Length * 500; // to mm half the width
            shoulderWidth = tmp > shoulderWidth && tmp < 500 ? tmp : shoulderWidth; // Half a meter in shoulder widht? Hello andrwe the giant

            tmp = (
                    (markers[rightAnkle] - markers[rightOuterKnee]).Length +
                    (markers[rightOuterKnee] - RIAS).Length +
                    (Sacrum - markers[spine]).Length +
                    (markers[spine] - markers[neck]).Length +
                    (markers[neck] - markers[head]).Length
                  ) * 100; // cm
            if (tmp > height && tmp < 250) // I have never heard of anyone as tall as 250cm
            {
                height = tmp;
                mass = (height / 100) * (height / 100) * BMI; // BMI
            }
        }
        #region Joint orientation 
        private Quaternion HipOrientation()
        {
            if (Sacrum.IsNaN() || LIAS.IsNaN() || RIAS.IsNaN())
            {
                MissingEssientialMarkers();
                if (Sacrum.IsNaN() || LIAS.IsNaN() || RIAS.IsNaN())
                {
                    //TODO The classic this should not happen case, send NaN or throw something?
                }
            }
            return QuaternionHelper.GetHipOrientation(Sacrum, LIAS, RIAS);
        }
        private Vector3 PelvisFront(Quaternion pelvisOrientation)
        {
            pelvisOrientation = QuaternionHelper.GetHipOrientation(Sacrum, LIAS, RIAS);
            Vector3 pelvisfront = Vector3.Transform(Vector3.UnitZ, pelvisOrientation);
            pelvisfront.Normalize();
            return pelvisfront;
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
        private Quaternion ChestOrientation()
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
            Vector3 front, up, right;

            Vector3 mid;

            if (!chestPos.IsNaN() && !neckPos.IsNaN() && !rightShoulderPos.IsNaN() && !leftShoulderPos.IsNaN())
            {
                front = chestPos - neckPos;
                right = rightShoulderPos - leftShoulderPos;
                return QuaternionHelper.GetOrientationFromZX(front, right);
            }

            if (!chestPos.IsNaN() && !neckPos.IsNaN())
            {
                mid = Vector3Helper.MidPoint(chestPos,neckPos);
                front = chestPos - neckPos;
                if (!rightShoulderPos.IsNaN())
                {
                    right = rightShoulderPos - mid;
                    return QuaternionHelper.GetOrientationFromZX(front, right);
                }
                if (!leftShoulderPos.IsNaN())
                {
                    right = mid - leftShoulderPos;
                    return QuaternionHelper.GetOrientationFromZX(front, right);
                }
            }
            if (!rightShoulderPos.IsNaN() && !leftShoulderPos.IsNaN())
            {
                right = rightShoulderPos - leftShoulderPos;
                mid = Vector3Helper.MidPoint(rightShoulderPos, leftShoulderPos); 
                if (!chestPos.IsNaN() )
                {
                    front = chestPos - mid;
                    return QuaternionHelper.GetOrientationFromZX(front, right);
                }
                if (!neckPos.IsNaN())
                {
                    front = mid - neckPos;
                    return QuaternionHelper.GetOrientationFromZX(front, right);
                }
            }
            return QuaternionHelper.GetOrientationFromZX((chestPos - neckPos), (rightShoulderPos - leftShoulderPos));
        }

        private Vector3 ArmForwardOrientationLeft()
        {

            Vector3 ulna = markers[leftElbow];
            Vector3 medialHumerus = markers[leftInnerElbow];
            Vector3 lateralHumerus = markers[leftOuterElbow];
            Vector3 midPoint = Vector3Helper.MidPoint(medialHumerus, lateralHumerus);
            Vector3 front = midPoint - ulna;
            front.Normalize();
            return front;
        }
        private Vector3 ArmForwardOrientationRight()
        {

            Vector3 ulna = markers[rightElbow];
            Vector3 medialHumerus = markers[rightInnerElbow];
            Vector3 lateralHumerus = markers[rightOuterElbow];
            Vector3 midPoint = Vector3Helper.MidPoint(medialHumerus, lateralHumerus);
            Vector3 front = midPoint - ulna;
            front.Normalize();
            return front;
        }
        private Vector3 KneeOrientationRight()
        {
            Vector3 upper = joints[BipedSkeleton.UPPERLEG_R];
            Vector3 lower = markers[rightAnkle];
            Vector3 forward = joints[BipedSkeleton.LOWERLEG_R];
            Vector3 mid = Vector3Helper.MidPoint(upper, lower);
            return forward - mid;
        }

        private Vector3 KneeOrientationLeft()
        {

            Vector3 upper = joints[BipedSkeleton.UPPERLEG_L];
            Vector3 lower = markers[leftAnkle]; 
            Vector3 forward = joints[BipedSkeleton.LOWERLEG_L];
            Vector3 mid = Vector3Helper.MidPoint(upper, lower);
            Vector3 vec = forward - mid;
            return vec;
             
        }

        private Vector3 WritsOrientationRight()
        {
            Vector3 littleFingerSide = markers[rightWrist];
            Vector3 thumbSide = markers[rightWristRadius];
            Vector3 front = thumbSide - littleFingerSide;
            front.Normalize();
            return front;
        }
        private Vector3 WritsOrientationLeft()
        {
            Vector3 littleFingerSide = markers[leftWrist];
            Vector3 thumbSide = markers[leftWristRadius];
            Vector3 front = thumbSide - littleFingerSide;
            front.Normalize();
            return front;
        }
        private Quaternion HeadOrientation()
        {
            return QuaternionHelper.GetOrientation(markers[head], markers[leftHead], markers[rightHead]);
        }
        #endregion
        #region pelsvis too head getters
        private Bone GetPlevis(Quaternion pelvisOrientation)
        {
            return new Bone(BipedSkeleton.PELVIS, joints[BipedSkeleton.PELVIS], pelvisOrientation);
        }
        private Bone GetSpineRoot(Quaternion pelvisOrientation)
        {
            Vector3 target = joints[BipedSkeleton.SPINE1];
            Vector3 front = Vector3.Transform(Vector3.UnitZ,pelvisOrientation);
            Vector3 pos = joints[BipedSkeleton.SPINE0];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.SPINE0, pos, rot);
        }
        private Bone GetSpine1(Quaternion pelvisOrientation)
        {
            Vector3 pos = joints[BipedSkeleton.SPINE1];
            Vector3 target = joints[BipedSkeleton.SPINE3];
            Vector3 front = Vector3.Transform(Vector3.UnitZ, pelvisOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.SPINE1, pos, rot);
        }
        private Bone GetSpineEnd(Quaternion chestOrientation)
        {
            return new Bone(BipedSkeleton.SPINE3, joints[BipedSkeleton.SPINE3], chestOrientation);
        }

        private Bone GetNeck(Quaternion chestOrientation)
        {
            Vector3 pos = joints[BipedSkeleton.NECK];
            Vector3 target = joints[BipedSkeleton.HEAD];
            Vector3 front = Vector3.Transform(Vector3.UnitZ, chestOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, front);
            return new Bone(BipedSkeleton.NECK, pos, rot);
        }
        private Bone GetHead()
        {
            return new Bone(BipedSkeleton.HEAD, joints[BipedSkeleton.HEAD], HeadOrientation());
        }
        #endregion
        #region leg getters
        private Bone GetUpperLegLeft(Vector3 kneeForwardOrientationLeft)
        {
            Vector3 pos = joints[BipedSkeleton.UPPERLEG_L]; ; 
            Vector3 target = joints[BipedSkeleton.LOWERLEG_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationLeft);
            return new Bone(BipedSkeleton.UPPERLEG_L, pos, rot);
        }
        private Bone GetUpperLegRight(Vector3 kneeForwardOrientationRight)
        {
            Vector3 pos = joints[BipedSkeleton.UPPERLEG_R];
            Vector3 target = joints[BipedSkeleton.LOWERLEG_R];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationRight); 
            return new Bone(BipedSkeleton.UPPERLEG_R, pos, rot);
        }

        private Bone GetLowerLegLeft(Vector3 kneeForwardOrientationLeft)
        {
            Vector3 pos = joints[BipedSkeleton.LOWERLEG_L];
            Vector3 target = joints[BipedSkeleton.FOOT_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationLeft);
            return new Bone(BipedSkeleton.LOWERLEG_L, pos, rot);
        }
        private Bone GetLowerLegRight(Vector3 kneeForwardOrientationRight)
        {
            Vector3 pos = joints[BipedSkeleton.LOWERLEG_R];
            Vector3 target = joints[BipedSkeleton.FOOT_R];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, kneeForwardOrientationRight);
            return new Bone(BipedSkeleton.LOWERLEG_R, pos, rot);
        }
        private Bone GetAnkleLeft()
        {
            Vector3 pos = joints[BipedSkeleton.FOOT_L];
            Vector3 up = joints[BipedSkeleton.LOWERLEG_L] - pos;
            //Quaternion rot = QuaternionHelper.LookAtUp(pos, markers[leftFoot], up);
            Quaternion rot = QuaternionHelper.LookAtUp(markers[leftHeel], markers[leftFoot], up);
            return new Bone(BipedSkeleton.FOOT_L, pos, rot);
        }
        private Bone GetAnkleRight()
        {
            Vector3 pos = joints[BipedSkeleton.FOOT_R];
            Vector3 up = joints[BipedSkeleton.LOWERLEG_R] - pos;
            Quaternion rot = QuaternionHelper.LookAtUp(markers[rightHeel], markers[rightFoot], up);
            //Quaternion rot = QuaternionHelper.LookAtUp(pos, markers[rightFoot], up);
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
        private Bone GetShoulderLeft(Quaternion chestOrientation)
        {
            Vector3 pos = joints[BipedSkeleton.SHOULDER_L];
            Vector3 target = joints[BipedSkeleton.UPPERARM_L];
            Vector3 up = Vector3.Transform(Vector3.UnitZ, chestOrientation);
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, up);
            return new Bone(BipedSkeleton.SHOULDER_L, pos, rot);
        }
        private Bone GetShoulderRight(Quaternion chestOrientation)
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
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, ArmForwardOrientationLeft());
            return new Bone(BipedSkeleton.UPPERARM_L, pos, rot);
        }
        private Bone GetUpperArmRight()
        {
            Vector3 pos = joints[BipedSkeleton.UPPERARM_R];
            Vector3 target = joints[BipedSkeleton.LOWERARM_R];
            Quaternion rot = QuaternionHelper.LookAtRight(pos, target, ArmForwardOrientationRight());
            return new Bone(BipedSkeleton.UPPERARM_R, pos, rot);
        }
        private Bone GetLowerArmLeft()
        {
            Vector3 pos = joints[BipedSkeleton.LOWERARM_L];
            Vector3 target = joints[BipedSkeleton.HAND_L];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, WritsOrientationLeft());
            return new Bone(BipedSkeleton.LOWERARM_L, pos, rot);
        }
        private Bone GetLowerArmRight()
        {
            Vector3 pos = joints[BipedSkeleton.LOWERARM_R];
            Vector3 target = joints[BipedSkeleton.HAND_R];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, WritsOrientationRight());
            return new Bone(BipedSkeleton.LOWERARM_R, pos, rot);
        }
        private Bone GetWristLeft()
        {
            Vector3 pos = joints[BipedSkeleton.HAND_L];
            Vector3 target = markers[leftHand];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, WritsOrientationLeft());
            return new Bone(BipedSkeleton.HAND_L, pos, rot);
        }
        private Bone GetWristRight()
        {
            Vector3 pos = joints[BipedSkeleton.HAND_R];
            Vector3 target = markers[rightHand];
            Quaternion rot = QuaternionHelper.LookAtUp(pos, target, WritsOrientationRight());
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
        #region JointPossitions
        private Dictionary<string, Vector3> JointPossitions(Quaternion pelvisOrientation, Quaternion chestOrientation)
        {
            Dictionary<string, Vector3> dic = new Dictionary<string, Vector3>();

            Vector3 
                pos, target,
                right, left, front, back,
                up, forward, 
                pelvisfront = PelvisFront(pelvisOrientation);

            /////////////// FEMUR LEFT ///////////////
            Vector3 lf = GetFemurJoint(pelvisOrientation, false);
            dic.Add(BipedSkeleton.UPPERLEG_L, lf);
            Vector3 rf = GetFemurJoint(pelvisOrientation, true);
            dic.Add(BipedSkeleton.UPPERLEG_R, rf);
            //////////////////////////////////////////

            /////////////// HIP ///////////////
            pos = Vector3Helper.MidPoint(lf,rf); 
            dic.Add(BipedSkeleton.PELVIS, pos);
            //////////////////////////////////////////

            /////////////// SPINE0 //////////////
            Vector3 spine0 = Sacrum + pelvisfront * marker2SpineDist;
            dic.Add(BipedSkeleton.SPINE0, spine0);
            //////////////////////////////////////////

            /////////////// SPINE1 //////////////
            /*
                Offset med z axel från ryggbas orientering med y axel roterad så TV12 mot TV2
             */
            pos = markers[spine];
            target = markers[neck];
            front = Vector3.Transform(Vector3.UnitZ, QuaternionHelper.LookAtUp(pos, target, pelvisfront));
            front.Normalize();
            pos += front * marker2SpineDist;
            dic.Add(BipedSkeleton.SPINE1, pos);
            //////////////////////////////////////////

            /////////////// Spine end ///////////////
            back = markers[neck];
            front = markers[chest];
            forward = Vector3.Transform(Vector3.UnitZ, chestOrientation);
            Vector3 neckPos = back + forward * marker2SpineDist; ;
            dic.Add(BipedSkeleton.SPINE3, neckPos);
            //////////////////////////////////////////

            /////////////// HEAD ///////////////
            front = markers[head];
            right = markers[rightHead];
            left = markers[leftHead];
            Vector3 headPos = Vector3Helper.MidPoint(left, right, front);
            //Move head position down
            Vector3 down = -Vector3.Transform(Vector3.UnitY, HeadOrientation());
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
            dic.Add(BipedSkeleton.SHOULDER_L, GetShoulderJoint(chestOrientation,false) + markers[chest]); //neckPos); //Vector3.One);// 
            dic.Add(BipedSkeleton.SHOULDER_R, GetShoulderJoint(chestOrientation,true) + markers[chest]); //neckPos); // Vector3.One);//
            //////////////////////////////

            /////////////// UPPER ARMS ///////////////
            dic.Add(BipedSkeleton.UPPERARM_L, GetUpperarmJoint(chestOrientation, false));
            dic.Add(BipedSkeleton.UPPERARM_R, GetUpperarmJoint(chestOrientation, true));
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
            pos = KneePos(false);
            dic.Add(BipedSkeleton.LOWERLEG_L, pos);
            //////////////////////////////////////////

            /////////////// KNEE RIGHT ///////////////
            pos = KneePos(true);
            dic.Add(BipedSkeleton.LOWERLEG_R, pos);
            //////////////////////////////

            /////////////// FOOT RIGHT ///////////////
            pos = AnklePos(true);
            dic.Add(BipedSkeleton.FOOT_R,pos);
            //////////////////////////////

            /////////////// FOOT LEft ///////////////
            pos = AnklePos(false);
            dic.Add(BipedSkeleton.FOOT_L, pos);
            //////////////////////////////

            return dic;
        }
        #endregion
        #region Special joints position
        private Vector3 GetFemurJoint(Quaternion pelvisOrientation, bool isRightHip)
        {
            // as described by Harrington et al. 2006
            // Prediction of the hip joint centre in adults, children, and patients with
            // cerebral palsy based on magnetic resonance imaging
            Vector3 ASISMid = Vector3Helper.MidPoint(RIAS, LIAS);
            float Z, X, Y,
                pelvisDepth = (ASISMid - Sacrum).Length * 1000,
                pelvisWidth = (LIAS - RIAS).Length * 1000;
            X = 0.33f * pelvisWidth - 7.3f;
            Y = -0.30f * pelvisWidth - 10.9f;
            Z = -0.24f * pelvisDepth - 9.9f;
            if (!isRightHip) X = -X;
            Vector3 pos = new Vector3(X, Y, Z) / 1000;
            pos = QuaternionHelper.Rotate(pelvisOrientation, pos);
            pos = ASISMid + pos;
            return pos;
        }
        private Vector3 GetUpperarmJoint(Quaternion chestOrientation, bool isRightShoulder)
        {
            // as described by Campbell et al. 2009 in 
            // MRI development and validation of two new predictive methods of
            // glenohumeral joint centre location identification and comparison with
            // established techniques
            float
                x = 96.2f - 0.302f * chestDepth - 0.364f * height + 0.385f * mass,
                y = -66.32f + 0.30f * chestDepth - 0.432f * mass,
                z = 66.468f - 0.531f * shoulderWidth + 0.571f * mass;
            Vector3 res = new Vector3(x, y, z) / 1000; // to mm
            res = QuaternionHelper.Rotate(chestOrientation, res);
            res += isRightShoulder ? markers[rightShoulder] : markers[leftShoulder];
            return res;
        }
        private Vector3 GetShoulderJoint(Quaternion chestOrientation, bool isRightShoulder)
        {
            float y = 0,
                  x = isRightShoulder ? 50 : -50,
                  z = -chestDepth / 2;
            Vector3 res = new Vector3(x, y, z) / 1000;
            res = QuaternionHelper.Rotate(chestOrientation, res);
            return res;
        }


        private Vector3 KneePos(bool isRightKnee)
        {

            // Stolen from Visual3D
            Vector3 x, y, z, M1, M2, M3, negateY = new Vector3(1f, -1f, 1f);
            Matrix4 R;
            if (isRightKnee)
            {
                M1 = markers[rightOuterKnee];//FLE
                M3 = markers[rightLowerKnee];//TTC
                M2 = markers[rightAnkle];//FAL

            }
            else
            {
                M1 = markers[leftOuterKnee];//FLE
                M3 = markers[leftLowerKnee];//TTC
                M2 = markers[leftAnkle];//FAL
            }
            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M1 - M2;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            Vector3 trans = new Vector3(-0.009f*0.7071f, 0.009f*0.7071f, 0f);
            if (isRightKnee) Vector3.Multiply(ref trans, ref negateY, out trans);
            Vector3 newM1 = Vector3.TransformVector(trans, R) + M1;

            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M2 - M1;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            if (isRightKnee) Vector3.Multiply(ref trans, ref negateY, out trans);
            Vector3 newM2 = Vector3.TransformVector(trans, R) + M2;
            
            if (isRightKnee)
            {
                markers[rightOuterKnee] = newM1;//FLE
                markers[rightAnkle] = newM2;//FAL

            }
            else
            {
                markers[leftOuterKnee] = newM1;//FLE
                markers[leftAnkle] = newM2;//FAL
            }

            x = Vector3Helper.MidPoint(newM1, newM2) - M3;
            z = newM1 - newM2;
            float scalingFactor = z.Length;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            trans = new Vector3(
                -0.1033f*scalingFactor,
                -0.09814f*scalingFactor,
                0.0597f*scalingFactor);
            if (isRightKnee) Vector3.Multiply(ref trans, ref negateY, out trans);
            Vector3 knee = Vector3.TransformVector(trans, R) + newM1;
            return knee;
        }
        private Vector3 AnklePos(bool isRightAnkle)
        {
            //Stolen from Visual3d
            Vector3 x, z, M1, M2, M3, negateY = new Vector3(1f, -1f, 1f);
            Matrix4 R;
            if (isRightAnkle)
            {
                M1 = markers[rightOuterKnee];//FLE
                M3 = markers[rightLowerKnee];//TTC
                M2 = markers[rightAnkle];//FAL
            }
            else
            {
                M1 = markers[leftOuterKnee];//FLE
                M3 = markers[leftLowerKnee];//TTC
                M2 = markers[leftAnkle];//FAL
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
            Vector3 ankle = Vector3.TransformVector(trans, R) + M2;
            return ankle;
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
