using System.Collections.Generic;
using OpenTK;
using System.Linq;
using System.Collections;
using System;
namespace QTM2Unity
{
    class JointLocalization
    {
        #region varible necessary to estimate joints
        public float markerCentreToSkinSurface =  0.009f;
        public float markerToSpineDist = 0.1f; // m
        public float midHeadToHeadJoint = 0.05f; // m
        public float spineLength = 0.05f; // m
        public float BMI = 24;

        // these are removed once first complete frame is taken.
        private float height = 175; // cm
        private float mass = 75; // kg
        private float chestDepth = 240; //mm
        private Vector3 neck2ChestVector = Vector3.Zero; 
        private float shoulderWidth = 400; // mm
        private Quaternion prevChestOri = Quaternion.Identity;
        #endregion

        #region Getters and Setters used for joint localization
        private Quaternion hipOrientation;
        private Quaternion HipOrientation
        {
            get
            {                
                if (hipOrientation == Quaternion.Identity)
                {
                    hipOrientation = QuaternionHelper.GetHipOrientation(Sacrum, LIAS, RIAS);
                }
                return hipOrientation;
            }
            set { hipOrientation = value; }
        }
        private Quaternion chestOrientation;
        private Quaternion ChestOrientation
        {
            get
            {
                if (chestOrientation == Quaternion.Identity)
                {
                    chestOrientation = GetChestOrientation();
                }
                return chestOrientation;
            }
            set { chestOrientation = value; }
        }
        private Quaternion headOrientation;
        private Quaternion HeadOrientation
        {
            get
            {
                if (headOrientation == Quaternion.Identity)
                {
                    headOrientation = QuaternionHelper.GetOrientation(markers[head], markers[leftHead], markers[rightHead]);
                }
                return headOrientation;
            }
            set { headOrientation = value; }
        }
        private Vector3 hipForward;
        private Vector3 HipForward
        {
            get
            {
                if (hipForward == Vector3.Zero)
                    hipForward = Vector3.Transform(Vector3.UnitZ, HipOrientation);
                return hipForward;
            }
            set { hipForward = value; }
        }
        private Vector3 chestForward;
        private Vector3 ChestForward
        {
            get
            {
                if (chestForward == Vector3.Zero)
                {
                    chestForward = Vector3.Transform(Vector3.UnitZ, ChestOrientation);
                }
                return chestForward;
            }
            set { chestForward = value; }
        }
        private Vector3 upperArmForwardLeft;
        private Vector3 UpperArmForwardLeft
        {
            get
            {
                if (upperArmForwardLeft == Vector3.Zero)
                {

                    Vector3 midPoint = Vector3Helper.MidPoint(markers[leftInnerElbow], markers[leftOuterElbow]);
                    upperArmForwardLeft = Vector3.NormalizeFast(midPoint - markers[leftElbow]);
                }
                return upperArmForwardLeft;
            }
            set { upperArmForwardLeft = value; }
        }
        private Vector3 upperArmForwardRight;
        private Vector3 UpperArmForwardRight
        {
            get
            {
                if (upperArmForwardRight == Vector3.Zero)
                {
                    Vector3 midPoint = Vector3Helper.MidPoint(markers[rightInnerElbow], markers[rightOuterElbow]);
                    upperArmForwardRight = Vector3.Normalize(midPoint - markers[rightElbow]);
                }
                return upperArmForwardRight;
            }
            set { upperArmForwardRight = value; }
        }
        private Vector3 lowerArmForwardLeft;
        private Vector3 LowerArmForwardLeft
        {
            get
            {
                if (lowerArmForwardLeft == Vector3.Zero)
                {
                    lowerArmForwardLeft = Vector3.NormalizeFast(markers[leftWristRadius] - markers[leftWrist]);
                }
                return lowerArmForwardLeft;
            }
            set { lowerArmForwardLeft = value; }
        }
        private Vector3 lowerArmForwardRight;
        private Vector3 LowerArmForwardRight
        {
            get
            {
                if (lowerArmForwardRight == Vector3.Zero)
                {
                    lowerArmForwardRight = Vector3.NormalizeFast(markers[rightWristRadius] - markers[rightWrist]);
                }
                return lowerArmForwardRight;
            }
            set { lowerArmForwardRight = value; }
        }
        private Vector3 kneeForwardLeft;
        private Vector3 KneeForwardLeft
        {
            get
            {
                if (kneeForwardLeft == Vector3.Zero)
                {
                    Vector3 knee = joints[BipedSkeleton.LOWERLEG_L];
                    Vector3 kneeOuter = markers[leftOuterKnee];
                    kneeForwardLeft = knee - kneeOuter;
                    //kneeForwardLeft = GetKneeForwardLeft();
                }
                return kneeForwardLeft;
            }
            set { kneeForwardLeft = value; }
        }
        private Vector3 kneeForwardRight;
        private Vector3 KneeForwardRight
        {
            get
            {
                if (kneeForwardRight == Vector3.Zero)
                {
                    Vector3 knee = joints[BipedSkeleton.LOWERLEG_R];
                    Vector3 kneeOuter = markers[rightOuterKnee];
                    kneeForwardRight = kneeOuter - knee;
                    //kneeForwardRight = GetKneeForwardRight();
                }
                return kneeForwardRight;
            }
            set { kneeForwardRight = value; }
        }
        #endregion


        #region important markers for hip joint
        private Vector3 RIAS
        {
            get
            {
                if (markers[rightHip].IsNaN())
                    MissingEssientialMarkers();
                return markers[rightHip];
            }
        }
        private Vector3 LIAS
        {
            get
            {
                if (markers[leftHip].IsNaN())
                    MissingEssientialMarkers();
                return markers[leftHip];
            }
        }
        private Vector3 Sacrum
        {
            get
            {
                if (markers[bodyBase].IsNaN())
                    MissingEssientialMarkers();
                return markers[bodyBase];
            }
        }
        #endregion

        private Dictionary<string, Vector3> joints;
        private Dictionary<string, Vector3> markers;
        private Dictionary<string, Vector3> markersLastFrame;
        private List<Action<Bone>> jointFunctions;
        private Dictionary<string, Vector3> markersComplete;
        private List<string> markersList;
        public JointLocalization()
        {
            jointFunctions = new List<Action<Bone>>() {
                    (b) => GetPlevis(b), 
                    (b) => GetSpineRoot(b), 
                    (b) => GetSpine1(b),
                    (b) => GetSpineEnd(b),      
                    (b) => GetNeck(b),
                    (b) => GetHead(b),
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
                    (b) => GetUpperLegLeft(b),       
                    (b) => GetLowerLegLeft(b),
                    (b) => GetAnkleLeft(b),
                    (b) => GetFootLeft(b),
                    (b) => GetUpperLegRight(b),
                    (b) => GetLowerLegRight(b), 
                    (b) => GetAnkleRight(b),    
                    (b) => GetFootRight(b),
                };
            // order here are semi important when all hip markers are gone (see MissingEssientialMarkers() )
            markersList = new List<string>()
            {
             bodyBase, 
             leftHip, rightHip,
             spine,  neck, chest,
             leftShoulder, rightShoulder,
             head, leftHead, rightHead,
             leftUpperKnee, rightUpperKnee,
             leftOuterKnee, rightOuterKnee,
             leftLowerKnee, rightLowerKnee, 
             leftAnkle, rightAnkle,
             leftHeel, rightHeel,
             leftFoot, rightFoot,
             leftElbow, rightElbow,
             leftInnerElbow, rightInnerElbow, 
             leftOuterElbow, rightOuterElbow,
             leftWrist, rightWrist,
             leftWristRadius, rightWristRadius,
             leftHand, rightHand,
            };
            markersComplete = markersList.ToDictionary(k => k, v => new Vector3(float.NaN, float.NaN, float.NaN));
            markers = markersComplete;
        }
        public void GetJointLocation(List<LabeledMarker> markerData, ref BipedSkeleton skeleton)
        {
            HipOrientation = Quaternion.Identity;
            ChestOrientation = Quaternion.Identity;
            HeadOrientation = Quaternion.Identity;
            HipForward = Vector3.Zero;
            ChestForward = Vector3.Zero;
            UpperArmForwardLeft = Vector3.Zero;
            UpperArmForwardRight = Vector3.Zero;
            LowerArmForwardLeft = Vector3.Zero;
            LowerArmForwardRight = Vector3.Zero;
            KneeForwardLeft = Vector3.Zero;
            KneeForwardRight = Vector3.Zero;

            // Copy last frames markers
            markersLastFrame = markers;
            // Copy new markers to dictionary
            markers = markerData.ToDictionary(k => k.label, v => v.position);
            markers = markers.Concat(markersComplete.Where(kvp => !markers.ContainsKey(kvp.Key))).ToDictionary(kv => kv.Key, kv => kv.Value);

            // collect data from markers about body proportions
            // this is necessary for shoulder joint localization 
            // Locate hiporientation, hip orientation is important for IK solver,
            BodyData();
            // get all joints
            joints = GetJointPossitions();

            IEnumerator it = skeleton.GetEnumerator();
            it.MoveNext();
            foreach (var action in jointFunctions)
            {
                action(((TreeNode<Bone>)it.Current).Data);
                it.MoveNext();
            }
        }

        private uint chestsFrames = 0;
        private uint shoulderFrames = 0;
        private uint heightFrames = 0;
        private void BodyData()
        {
            // set chest depth
            var tmpV = (markers[chest] - markers[neck]); // to mm
            tmpV = Vector3.Transform(tmpV, Quaternion.Invert(ChestOrientation));
            if (!tmpV.IsNaN())
            {
                neck2ChestVector = (neck2ChestVector * chestsFrames + tmpV) / (chestsFrames + 1);
                chestDepth = neck2ChestVector.Length * 1000;  // to mm
                chestsFrames++;
            }

            // set shoulder width
            float tmp = (markers[leftShoulder] - markers[rightShoulder]).Length * 500; // to mm half the width
            if (!float.IsNaN(tmp) && tmp < 500)
            {
                shoulderWidth = (shoulderWidth * shoulderFrames + tmp) / (shoulderFrames + 1);
                shoulderFrames++;
            }
            // height and mass
            tmp = (
                    (markers[rightAnkle] - markers[rightOuterKnee]).Length +
                    (markers[rightOuterKnee] - RIAS).Length +
                    (Sacrum - markers[spine]).Length +
                    (markers[spine] - markers[neck]).Length +
                    (markers[neck] - markers[head]).Length
                  ) * 100; // cm
            if (!float.IsNaN(tmp) && tmp < 250)
            {
                height = (height * heightFrames + tmp) / (heightFrames + 1);
                mass = (height / 100) * (height / 100) * BMI; // BMI
                heightFrames++;
            }
        }
        #region JointPossitions
        private Dictionary<string, Vector3> GetJointPossitions()
        {
            var dic = new Dictionary<string, Vector3>();

            Vector3 pos, target, right, left, front, back, up;
            /////////////// FEMUR ///////////////
            Vector3 lf = GetFemurJoint(false);
            dic.Add(BipedSkeleton.UPPERLEG_L, lf);
            Vector3 rf = GetFemurJoint(true);
            dic.Add(BipedSkeleton.UPPERLEG_R, rf);
            //////////////////////////////////////////

            /////////////// HIP ///////////////
           // dic.Add(BipedSkeleton.PELVIS, Vector3Helper.MidPoint(lf,rf)); //TODO Set pos as midpoint between LIAS and RIAS instead
            //////////////////////////////////////////

            /////////////// SPINE0 //////////////
            //Vector3 spine0 = Sacrum + HipForward * markerToSpineDist;
            ///*
            // * x = 0.0045 * pelvic depth
            // * y = 0.0349 * pelvic width (rias-lias).length
            // * z = 0.7006 * pelvic depth
            // * eller
            // * x = 0.0045 * pelvic depth
            // * Y = 0.0545 * pelvic depth
            // * z = 0.7006 * pelvic depth  
            // */
            //dic.Add(BipedSkeleton.SPINE0, spine0);
            //////////////////////////////////////////

            /////////////// Spine end ///////////////
            back = markers[neck];
            front = markers[chest];
            Vector3 neckPos;
            Vector3 neckToSternumVector = Vector3.Normalize(Vector3.Transform(neck2ChestVector, ChestOrientation));
            if (!back.IsNaN() && neck2ChestVector != Vector3.Zero)
                neckPos = back + neckToSternumVector * markerToSpineDist;
            else if (!front.IsNaN() && neck2ChestVector != Vector3.Zero)
                neckPos = front + (-neckToSternumVector * ((chestDepth / 1000) - markerToSpineDist));
            else
                neckPos = Vector3Helper.MidPoint(markers[leftShoulder], markers[rightShoulder]);
            dic.Add(BipedSkeleton.SPINE3, neckPos);
            //////////////////////////////////////////

            /////////////// SPINE1 //////////////
            /*
                Offset med z axel från ryggbas orientering med y axel roterad så TV12 mot TV2
             */
            //if (markers[spine].IsNaN())
            //{
            //    pos = Vector3Helper.MidPoint(neckPos, spine0);
            //}
            //else
            {
                if (markers[neck].IsNaN())
                {
                    pos = markers[bodyBase];
                    target = markers[spine];
                }
                else
                {
                    pos = markers[spine];
                    target = markers[neck];
                }
                front = Vector3.Transform(Vector3.UnitZ, QuaternionHelper.LookAtUp(pos, target, ChestForward));
                front.Normalize();
                pos = markers[spine];
                pos += front * markerToSpineDist;
            }
            dic.Add(BipedSkeleton.SPINE1, pos);
            //////////////////////////////////////////

            /////////////// neck ///////////////
            //up = Vector3.Transform(Vector3.UnitY, ChestOrientation);
            //up.NormalizeFast();
            //pos = neckPos + up * spineLength;
            //dic.Add(BipedSkeleton.NECK, pos);
            //////////////////////////////////////////

            /////////////// HEAD ///////////////
            front = markers[head];
            right = markers[rightHead];
            left = markers[leftHead];
            Vector3 headPos = Vector3Helper.MidPoint(left, right);
            //Move head position down
            Vector3 down = -Vector3.Transform(Vector3.UnitY, HeadOrientation);
            down.NormalizeFast();
            headPos += down * midHeadToHeadJoint;
            dic.Add(BipedSkeleton.HEAD, headPos);
            //////////////////////////////////////////

            /////////////// SHOUDLERs ///////////////
            //dic.Add(BipedSkeleton.SHOULDER_L, neckPos);
           // dic.Add(BipedSkeleton.SHOULDER_R, neckPos);
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
            pos = Vector3Helper.MidPoint(left, right);//, front); // stop changing this to left,right stupid, no arm ori will be

            dic.Add(BipedSkeleton.LOWERARM_L, pos);
            //////////////////////////////////////////

            /////////////// ELBOW RIGHT ///////////////
            front = markers[rightElbow];
            left = markers[rightInnerElbow];
            right = markers[rightOuterElbow];
            pos = Vector3Helper.MidPoint(left, right);//, front);
            dic.Add(BipedSkeleton.LOWERARM_R, pos);
            //////////////////////////////////////////

            /////////////// KNEE LEFT ///////////////
            dic.Add(BipedSkeleton.LOWERLEG_L, GetKneePos(false));
            //////////////////////////////////////////

            /////////////// KNEE RIGHT ///////////////
            dic.Add(BipedSkeleton.LOWERLEG_R, GetKneePos(true));
            //////////////////////////////

            /////////////// FOOT LEft ///////////////
            dic.Add(BipedSkeleton.FOOT_L, GetAnklePos(false));
            //////////////////////////////

            /////////////// FOOT RIGHT ///////////////
            dic.Add(BipedSkeleton.FOOT_R, GetAnklePos(true));
            //////////////////////////////

            return dic;
        }
        #endregion
        #region Joint orientation 
        private void MissingEssientialMarkers()
        {
            Vector3 dirVec1, dirVec2, possiblePos1, possiblePos2,
                    sacrumlastFrame = markersLastFrame[bodyBase],
                    liasLastFrame   = markersLastFrame[leftHip],
                    riasLastFrame   = markersLastFrame[rightHip];
            Vector3 
                Sacrum = markers[bodyBase],
                RIAS = markers[rightHip],
                LIAS = markers[leftHip];
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
                string first = markersList.FirstOrDefault(name => !markers[name].IsNaN() && !markersLastFrame[name].IsNaN());
                if (first != null)
                {
                        Vector3 firstHitLastFrame = markersLastFrame[first],
                                firstHit = markers[first];
                        markers[rightHip] = riasLastFrame - firstHitLastFrame + firstHit;
                        markers[leftHip] = liasLastFrame - firstHitLastFrame + firstHit;
                        markers[bodyBase] = sacrumlastFrame - firstHitLastFrame + firstHit;
                }
                else
                {
                    markers[rightHip] = riasLastFrame;
                    markers[leftHip] = liasLastFrame;
                    markers[bodyBase] = sacrumlastFrame;
                }
            }
        }
        private Quaternion GetChestOrientation()
        {
            Vector3 neckPos = markers[neck];
            Vector3 chestPos = markers[chest];
            Vector3 rightShoulderPos = markers[rightShoulder];
            Vector3 leftShoulderPos = markers[leftShoulder];
            Vector3 backspinePos = markers[spine];
            Vector3 Yaxis, Xaxis;
            Vector3 mid = Vector3Helper.MidPoint(rightShoulderPos, leftShoulderPos);
            Quaternion rotation;
            // Find Y axis
            if (!backspinePos.IsNaN() && !neckPos.IsNaN()) // prio 1, 12th Thoracic to 2nd Thoracic
            {
                Yaxis = neckPos - backspinePos;
            }
            else if (!neckPos.IsNaN()) // prio 2, Sacrum to 2nd Thoracic
            {
                Yaxis = neckPos - Sacrum;
            }
            else if (!backspinePos.IsNaN()) // prio 3, Sacrum to 12th Thoracic
            {
                Yaxis = backspinePos - Sacrum;
            }
            else if (!mid.IsNaN()) // prio 4, middle of left and right Scapula to 
            {
                Vector3 backMid = (LIAS - RIAS) * 0.5f + RIAS;
                Yaxis = mid - (Sacrum + (backMid - Sacrum) * 2 / 3);
            }
            else // last resort, use hip orientation
            {
                Yaxis = Vector3.Transform(Vector3.UnitY, HipOrientation);
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
                    //if (!rightShoulderPos.IsNaN()) // prio 4, from right scapula to projected line sacrum to right scapula on Yaxis 
                    //{
                    //    mid = Sacrum + Vector3Helper.Project((rightShoulderPos - Sacrum), Yaxis);
                    //    Xaxis = mid - rightShoulderPos;
                    //}
                    //else // prio 5, from left scapula to projected line sacrum to left scapula on Yaxis
                    //{
                    //    mid = Sacrum + Vector3Helper.Project((leftShoulderPos - Sacrum), Yaxis);
                    //    Xaxis = leftShoulderPos - mid;
                    //}
                    //Xaxis = -Vector3.Transform(Vector3.UnitX, HipOrientation);
                    Xaxis = -Vector3.Transform(Vector3.UnitX, prevChestOri);
                }
            }
            else // last resort, use hip prev orientation
            {
                Xaxis = -Vector3.Transform(Vector3.UnitX, prevChestOri);
            }
            rotation = QuaternionHelper.GetOrientationFromYX(Yaxis, Xaxis);
            prevChestOri = rotation;
            return rotation;
        }
        #endregion
        #region pelsvis too head getters
        private void GetPlevis(Bone b)
        {
            b.Pos = Vector3Helper.MidPoint(joints[BipedSkeleton.UPPERLEG_L], joints[BipedSkeleton.UPPERLEG_R]); 
            b.Orientation = HipOrientation;
        }
        private void GetSpineRoot(Bone b)
        {
            Vector3 target = joints[BipedSkeleton.SPINE1].IsNaN() ? joints[BipedSkeleton.SPINE3] : joints[BipedSkeleton.SPINE1];
            Vector3 pos = Sacrum + HipForward * markerToSpineDist;//joints[BipedSkeleton.SPINE0];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, HipForward);
        }
        private void GetSpine1(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.SPINE1];
            Vector3 target = joints[BipedSkeleton.SPINE3];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, HipForward);
        }
        private void GetSpineEnd(Bone b)
        {
            b.Pos = joints[BipedSkeleton.SPINE3];
            b.Orientation = ChestOrientation;
        }
        private void GetNeck(Bone b)
        {
            Vector3 up = Vector3.Transform(Vector3.UnitY, ChestOrientation);
            up.NormalizeFast();
            Vector3 neckPos = joints[BipedSkeleton.SPINE3];
            Vector3 pos = neckPos + up * spineLength;
            Vector3 target = joints[BipedSkeleton.HEAD];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, ChestForward);
        }
        private void GetHead(Bone b)
        {
            b.Pos = joints[BipedSkeleton.HEAD];
            b.Orientation = HeadOrientation;
        }
        #endregion
        #region leg getters
        private void GetUpperLegLeft(Bone b)
        {
            Vector3 pos =  joints[BipedSkeleton.UPPERLEG_L]; 
            Vector3 target = joints[BipedSkeleton.LOWERLEG_L];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtRight(pos, target, KneeForwardLeft);
        }
        private void GetUpperLegRight(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.UPPERLEG_R];
            Vector3 target = joints[BipedSkeleton.LOWERLEG_R];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtRight(pos, target, KneeForwardRight);
        }
        private void GetLowerLegLeft(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.LOWERLEG_L];
            Vector3 target = joints[BipedSkeleton.FOOT_L];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtRight(pos, target, KneeForwardLeft); 
        }
        private void GetLowerLegRight(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.LOWERLEG_R];
            Vector3 target = joints[BipedSkeleton.FOOT_R];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtRight(pos, target, kneeForwardRight);
        }
        private void GetAnkleLeft(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.FOOT_L];
            Vector3 up = joints[BipedSkeleton.LOWERLEG_L] - pos;
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, markers[leftFoot], up);
        }
        private void GetAnkleRight(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.FOOT_R];
            Vector3 up = joints[BipedSkeleton.LOWERLEG_R] - pos;
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, markers[rightFoot], up);
        }
        private void GetFootLeft(Bone b)
        {
            b.Pos = markers[leftFoot];
        }
        private void GetFootRight(Bone b)
        {
            b.Pos = markers[rightFoot];
        }
        #endregion
        #region arm getters
        private void GetShoulderLeft(Bone b)
        {
            Vector3 pos = new Vector3(joints[BipedSkeleton.SPINE3]);// joints[BipedSkeleton.SHOULDER_L];
            Vector3 target = joints[BipedSkeleton.UPPERARM_L];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, ChestForward);
        }
        private void GetShoulderRight(Bone b)
        {
            Vector3 pos = new Vector3(joints[BipedSkeleton.SPINE3]); //joints[BipedSkeleton.SHOULDER_R];
            Vector3 target = joints[BipedSkeleton.UPPERARM_R];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, ChestForward);
        }
        private void GetUpperArmLeft(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.UPPERARM_L];
            Vector3 target = joints[BipedSkeleton.LOWERARM_L];
            b.Pos = pos;
            //if (!UpperArmForwardLeft.IsNaN())
            //{
            //    b.Orientation = QuaternionHelper.LookAtUp(pos, target, UpperArmForwardLeft);
            //}
            //else
            {
                b.Orientation = QuaternionHelper.LookAtRight(pos, target, markers[leftInnerElbow] - markers[leftOuterElbow]);
            }
        }
        private void GetUpperArmRight(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.UPPERARM_R];
            Vector3 target = joints[BipedSkeleton.LOWERARM_R];
            b.Pos = pos; 
            //if (!UpperArmForwardRight.IsNaN())
            //{
            //    b.Orientation = QuaternionHelper.LookAtUp(pos, target, UpperArmForwardRight);
            //}
            //else
            {
                b.Orientation = QuaternionHelper.LookAtRight(pos, target, markers[rightOuterElbow] - markers[rightInnerElbow]);
            }
        }
        private void GetLowerArmLeft(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.LOWERARM_L];
            Vector3 target = joints[BipedSkeleton.HAND_L];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, LowerArmForwardLeft);
        }
        private void GetLowerArmRight(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.LOWERARM_R];
            Vector3 target = joints[BipedSkeleton.HAND_R];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, LowerArmForwardRight);
        }
        private void GetWristLeft(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.HAND_L];
            Vector3 target = markers[leftHand];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, LowerArmForwardLeft);
        }
        private void GetWristRight(Bone b)
        {
            Vector3 pos = joints[BipedSkeleton.HAND_R];
            Vector3 target = markers[rightHand];
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, LowerArmForwardRight);
        }
        private void GetHandLeft(Bone b)
        {
            b.Pos = markers[leftHand];
        }
        private void GetHandRight(Bone b)
        {
            b.Pos = markers[rightHand];
        }
        #endregion
        #region Special joints position
        private Vector3 GetFemurJoint(bool isRightHip)
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
                x = 96.2f - 0.302f * chestDepth - 0.364f * height + 0.385f * mass,
                y = -66.32f + 0.30f * chestDepth - 0.432f * mass,
                z = 66.468f - 0.531f * shoulderWidth + 0.571f * mass;
            if (isRightShoulder) z = -z;
            Vector3 res = new Vector3(x, y, z) / 1000; // to mm
            res = QuaternionHelper.Rotate(ChestOrientation, res);
            res += isRightShoulder ? markers[rightShoulder] : markers[leftShoulder];
            return res;
        }
        private Vector3 GetKneePos(bool isRightKnee)
        {
            // Stolen from Visual3D
            Vector3 x, z, M1, M2, M3, negateY = new Vector3(1f, -1f, 1f);
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
            Vector3 trans = new Vector3(-markerCentreToSkinSurface * 0.7071f, markerCentreToSkinSurface * 0.7071f, 0f);
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
        private Vector3 GetAnklePos(bool isRightAnkle)
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
            //return isRightAnkle ? markers[rightHeel] : markers[leftHeel];
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