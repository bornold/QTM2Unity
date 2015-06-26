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
        //private Dictionary<string, Vector3> joints;
        private List<string> markersList;
        private Dictionary<string, Vector3> markers;
        private Dictionary<string, Vector3> markersLastFrame;
        private Dictionary<string, Vector3> markersComplete;
        #region varible necessary to estimate joints
        public float markerCentreToSkinSurface =  0.009f;
        public float markerToSpineDist = 0.1f; // m
        public float midHeadToHeadJoint = 0.05f; // m
        public float spineLength = 0.0236f; // m
        public float BMI = 24;
        private float height = 175; // cm
        private float mass = 75; // kg
        private Vector3 neck2ChestVector;
        private float shoulderWidth = 400; // mm
        private Quaternion prevChestOri = Quaternion.Identity;
        // counters for average values
        private uint chestsFrames = 0;
        private uint shoulderFrames = 0;
        private uint heightFrames = 0;
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
                    (b) => UpperLegRight(b),
                    (b) => LowerLegRight(b), 
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
            o = new Values(); // reset joint pos and orientations

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
            IEnumerator it = skeleton.GetEnumerator();
            it.MoveNext();
            foreach (var action in jointFunctions)
            {
                action(((TreeNode<Bone>)it.Current).Data);
                it.MoveNext();
            }
        }

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
        private void MissingEssientialMarkers()
        {
            Vector3 dirVec1, dirVec2, possiblePos1, possiblePos2,
                    sacrumlastFrame = markersLastFrame[bodyBase],
                    liasLastFrame = markersLastFrame[leftHip],
                    riasLastFrame = markersLastFrame[rightHip];
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
                    dirVec2 = liasLastFrame - sacrumlastFrame;
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
        #endregion

        private void BodyData()
        {
            // set chest depth
            var tmpV = (markers[chest] - markers[neck]); // to mm
            tmpV = Vector3.Transform(tmpV, Quaternion.Invert(ChestOrientation));
            if (!tmpV.IsNaN())
            {
                neck2ChestVector = (neck2ChestVector * chestsFrames + tmpV) / (chestsFrames + 1);
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

        #region Getters and Setters used for joint localization
        private Quaternion HipOrientation
        {
            get
            {
                if (o.hipOrientation == Quaternion.Zero)
                {
                    o.hipOrientation = QuaternionHelper.GetHipOrientation(Sacrum, LIAS, RIAS);
                }
                return o.hipOrientation;
            }
        }
        private Quaternion ChestOrientation
        {
            get
            {
                if (o.chestOrientation == Quaternion.Zero)
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
                    if (!mid.IsNaN())
                    {
                        Yaxis = mid - Sacrum;
                    }
                    else if (!backspinePos.IsNaN() && !neckPos.IsNaN()) // prio 1, 12th Thoracic to 2nd Thoracic
                    {
                        Yaxis = Vector3Helper.MidPoint(neckPos, backspinePos) - Sacrum;
                    }
                    else if (!neckPos.IsNaN()) // prio 2, Sacrum to 2nd Thoracic
                    {
                        Yaxis = neckPos - Sacrum;
                    }
                    else if (!backspinePos.IsNaN()) // prio 3, Sacrum to 12th Thoracic
                    {
                        Yaxis = backspinePos - Sacrum;
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
                    o.chestOrientation = rotation;
                }
                return o.chestOrientation;
            }
        }
        private Quaternion HeadOrientation
        {
            get
            {
                if (o.headOrientation == Quaternion.Zero)
                {
                    o.headOrientation = QuaternionHelper.GetOrientation(markers[head], markers[leftHead], markers[rightHead]);
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

                    Vector3 midPoint = Vector3Helper.MidPoint(markers[leftInnerElbow], markers[leftOuterElbow]);
                    o.upperArmForwardLeft = Vector3.NormalizeFast(midPoint - markers[leftElbow]);
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
                    Vector3 midPoint = Vector3Helper.MidPoint(markers[rightInnerElbow], markers[rightOuterElbow]);
                    o.upperArmForwardRight = Vector3.Normalize(midPoint - markers[rightElbow]);
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
                    o.lowerArmForwardLeft = Vector3.NormalizeFast(markers[leftWristRadius] - markers[leftWrist]);
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
                    o.lowerArmForwardRight = Vector3.NormalizeFast(markers[rightWristRadius] - markers[rightWrist]);
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
                    Vector3 kneeOuter = markers[leftOuterKnee];
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
                    Vector3 kneeOuter = markers[rightOuterKnee];
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
                    o.rightHipPos = GetFemurJoint(true);
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
                    o.leftHipPos = GetFemurJoint(false);
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
                    Vector3 back = markers[neck];
                    Vector3 front = markers[chest];
                    Vector3 neckPos;
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
                        neckPos = back * markerToSpineDist;
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
                Vector3 front = Vector3.Transform(Vector3.UnitZ, QuaternionHelper.LookAtUp(pos, target, ChestForward));
                front.Normalize();
                pos = markers[spine];
                pos += front * markerToSpineDist;
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
                    Vector3 headPos = Vector3Helper.MidPoint(markers[leftHead], markers[rightHead]);
                    //Move head position down
                    Vector3 down = -Vector3.Transform(Vector3.UnitY, HeadOrientation);
                    //down.NormalizeFast();
                    headPos += down * midHeadToHeadJoint;
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
                    o.elbowLeft = Vector3Helper.MidPoint(markers[leftInnerElbow], markers[leftOuterElbow]);
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
                    o.elbowRight = Vector3Helper.MidPoint(markers[rightInnerElbow], markers[rightOuterElbow]);
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
                    o.handLeft = Vector3Helper.MidPoint(markers[leftWrist], markers[leftWristRadius]);
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
                    o.handRight = Vector3Helper.MidPoint(markers[rightWrist], markers[rightWristRadius]);
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
                x = 96.2f - 0.302f * (neck2ChestVector.Length*1000) - 0.364f * height + 0.385f * mass,
                y = -66.32f + 0.30f * (neck2ChestVector.Length * 1000) - 0.432f * mass,
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
            Vector3 pos = Sacrum + HipForward * markerToSpineDist;
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, target, HipForward);
        }
        private void MidSpine(Bone b)
        {
            b.Pos = Spine1;
            b.Orientation = QuaternionHelper.LookAtUp(Spine1, SternumClavicle, HipForward);
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
            Vector3 pos = neckPos + up * spineLength*2;
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, Head, ChestForward);
        }
        private void GetHead(Bone b)
        {
            b.Pos = Head;
            b.Orientation = HeadOrientation;
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
            Vector3 pos = AnkleLeft;
            Vector3 up = KneeLeft - pos;
            b.Pos = pos;
            b.Orientation = QuaternionHelper.LookAtUp(pos, markers[leftFoot], up);
        }
        private void GetAnkleRight(Bone b)
        {
            Vector3 pos = AnkleRight;
            Vector3 up = KneeRight - pos;
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
            b.Orientation = QuaternionHelper.LookAtRight(ShoulderLeft, ElbowLeft, markers[leftInnerElbow] - markers[leftOuterElbow]);
        }
        private void GetUpperArmRight(Bone b)
        {
            b.Pos = ShoulderRight;
            b.Orientation = QuaternionHelper.LookAtRight(ShoulderRight, ElbowRight, markers[rightOuterElbow] - markers[rightInnerElbow]);
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
            b.Orientation = QuaternionHelper.LookAtUp(HandLeft, markers[leftHand], LowerArmForwardLeft);
        }
        private void GetWristRight(Bone b)
        {
            b.Pos = HandRight;
            b.Orientation = QuaternionHelper.LookAtUp(HandRight, markers[rightHand], LowerArmForwardRight);
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
        #endregion
        #region skin markers names
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