using OpenTK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTM2Unity
{
    class MarkersPreprocessor
    {
        private List<string> markersList;
        private Dictionary<string, Vector3> markersLastFrame;
        private bool nameSet = false;
        private string prefix;
        public MarkersPreprocessor(string bodyPrefix = "")
        {
            this.prefix = bodyPrefix;
            // order here are semi important when all hip markers are gone (see MissingEssientialMarkers() )
            markersList = new List<string>()
            {
             MarkerNames.bodyBase, 
             MarkerNames.leftHip, MarkerNames.rightHip,
             MarkerNames.spine,  MarkerNames.neck, MarkerNames.chest,
             MarkerNames.leftShoulder, MarkerNames.rightShoulder,
             MarkerNames.head, MarkerNames.leftHead, MarkerNames.rightHead,
             MarkerNames.leftUpperKnee, MarkerNames.rightUpperKnee,
             MarkerNames.leftOuterKnee, MarkerNames.rightOuterKnee,
             MarkerNames.leftLowerKnee, MarkerNames.rightLowerKnee, 
             MarkerNames.leftInnerKnee, MarkerNames.rightInnerKnee, 
             MarkerNames.leftOuterAnkle, MarkerNames.rightOuterAnkle,
             MarkerNames.leftInnerAnkle, MarkerNames.rightInnerAnkle,
             MarkerNames.leftHeel, MarkerNames.rightHeel,
             MarkerNames.leftToe2, MarkerNames.rightToe2,
             MarkerNames.leftElbow, MarkerNames.rightElbow,
             MarkerNames.leftInnerElbow, MarkerNames.rightInnerElbow, 
             MarkerNames.leftOuterElbow, MarkerNames.rightOuterElbow,
             MarkerNames.leftWrist, MarkerNames.rightWrist,
             MarkerNames.leftWristRadius, MarkerNames.rightWristRadius,
             MarkerNames.leftHand, MarkerNames.rightHand,
            };
            markersList.ForEach(z => z = prefix + z);
            markersLastFrame = markersList.ToDictionary(kv => kv, kv => Vector3Helper.NaN);
        }
        public bool ProcessMarkers(Dictionary<string,Vector3> markers)
        {
            if (Object.ReferenceEquals(markers, markersLastFrame))
            {
                return false;
            }
            if (!nameSet)
            {
                NameSet(markers, prefix);
                markersLastFrame[MarkerNames.bodyBase] = new Vector3(0f, 1.0f, 0f);
                markersLastFrame[MarkerNames.leftHip] =  new Vector3(-0.1f, 1.0f, 0.15f);
                markersLastFrame[MarkerNames.rightHip] = new Vector3(0.1f, 1.0f, 0.15f);
                nameSet = true;
            }
            if (markersLastFrame[MarkerNames.leftHip].IsNaN()
              || markersLastFrame[MarkerNames.rightHip].IsNaN()
              || markersLastFrame[MarkerNames.bodyBase].IsNaN())
            {
                UnityEngine.Debug.LogWarning(markersLastFrame[MarkerNames.leftHip]);
                UnityEngine.Debug.LogWarning(markersLastFrame[MarkerNames.rightHip]);
                UnityEngine.Debug.LogWarning(markersLastFrame[MarkerNames.bodyBase]);
                UnityEngine.Debug.LogWarning("MISSING Essential markers from last frame");
            }
            // GC: 40B GC 
            foreach (var name in markersList)
            {
                if (!markers.ContainsKey(name))
                {
                    markers.Add(name, Vector3Helper.NaN);
                }
            }
            // GC END
            if (markers[MarkerNames.leftHip].IsNaN()
                || markers[MarkerNames.rightHip].IsNaN()
                || markers[MarkerNames.bodyBase].IsNaN())
            {
                MissingEssientialMarkers(markers);

            }
            markersLastFrame = markers;
            //MoveLegMarkers(ref newMarkers, true);
            //MoveLegMarkers(ref newMarkers, false);
            return true;
        }
        private void MissingEssientialMarkers(Dictionary<string,Vector3> markers)
        {
            Vector3 dirVec1, dirVec2, possiblePos1, possiblePos2,
                    sacrumlastFrame = markersLastFrame[MarkerNames.bodyBase],
                    liasLastFrame = markersLastFrame[MarkerNames.leftHip],
                    riasLastFrame = markersLastFrame[MarkerNames.rightHip];
            Vector3
                Sacrum = markers[MarkerNames.bodyBase],
                RIAS = markers[MarkerNames.rightHip],
                LIAS = markers[MarkerNames.leftHip];
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
                    markers[MarkerNames.leftHip] = possiblePos1.MidPoint(possiblePos2); // get mid point of possible positions

                }
                else if (l) // sacrum  and lias exists, rias missing
                {
                    dirVec1 = riasLastFrame - sacrumlastFrame;
                    dirVec2 = riasLastFrame - liasLastFrame;
                    possiblePos1 = Sacrum + dirVec1;
                    possiblePos2 = LIAS + dirVec2;
                    markers[MarkerNames.rightHip] = possiblePos1.MidPoint(possiblePos2);
                }
                else // only sacrum exists, lias and rias missing
                {
                    dirVec1 = riasLastFrame - sacrumlastFrame;
                    markers[MarkerNames.rightHip] = Sacrum + dirVec1;
                    dirVec2 = liasLastFrame - sacrumlastFrame;
                    markers[MarkerNames.leftHip] = Sacrum + dirVec2;
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
                    markers[MarkerNames.bodyBase] = possiblePos1.MidPoint(possiblePos2);
                }
                else // only rias exists, lias and sacrum missing
                {
                    dirVec1 = sacrumlastFrame - riasLastFrame;
                    markers[MarkerNames.bodyBase] = RIAS + dirVec1;
                    dirVec2 = liasLastFrame - riasLastFrame;
                    markers[MarkerNames.leftHip] = RIAS + dirVec2;
                }
            }
            else if (l) // only lias exists, rias and sacrum missing
            {
                dirVec1 = sacrumlastFrame - liasLastFrame;
                markers[MarkerNames.bodyBase] = LIAS + dirVec1;
                dirVec2 = riasLastFrame - liasLastFrame;
                markers[MarkerNames.rightHip] = LIAS + dirVec2;
            }
            else // all markers missing
            {
                string first = null;
                foreach (var mName in markersList)
                {
                    if (!markers[mName].IsNaN() && markersLastFrame[mName].IsNaN())
                    {
                        first = mName;
                        break;
                    }
                }
                if (first != null)
                {
                    Vector3 firstHitLastFrame = markersLastFrame[first],
                            firstHit = markers[first];
                    markers[MarkerNames.rightHip] = riasLastFrame - firstHitLastFrame + firstHit;
                    markers[MarkerNames.leftHip] = liasLastFrame - firstHitLastFrame + firstHit;
                    markers[MarkerNames.bodyBase] = sacrumlastFrame - firstHitLastFrame + firstHit;
                }
                else
                {
                    markers[MarkerNames.rightHip] = riasLastFrame;
                    markers[MarkerNames.leftHip] = liasLastFrame;
                    markers[MarkerNames.bodyBase] = sacrumlastFrame;
                }
            }
            if (markers[MarkerNames.rightHip].IsNaN()
                || markers[MarkerNames.leftHip].IsNaN()
                || markers[MarkerNames.bodyBase].IsNaN())
            {
                //UnityEngine.Debug.LogError("LAST FRAME ERROR");
                UnityEngine.Debug.LogWarningFormat("Missing even after prediction: RIAS:{0}\nLIAS{1}\nSACRUM{2}",
                    markers[MarkerNames.rightHip],markers[MarkerNames.leftHip],markers[MarkerNames.bodyBase]);
            }
        }
        private void MoveLegMarkers(ref Dictionary<string,Vector3> markers, bool isRightKnee)
        {
            // Stolen from Visual3D
            Vector3 x, z, M1, M2, M3, negateY = new Vector3(1f, -1f, 1f);
            Matrix4 R;
            if (isRightKnee)
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
            z = M1 - M2;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            Vector3 trans = (isRightKnee) ? 
                new Vector3(-BodyData.MarkerCentreToSkinSurface * 0.7071f, -BodyData.MarkerCentreToSkinSurface * 0.7071f, 0f) :
                new Vector3(-BodyData.MarkerCentreToSkinSurface * 0.7071f, BodyData.MarkerCentreToSkinSurface * 0.7071f, 0f);
            //if (isRightKnee) trans = Vector3.Multiply(trans, negateY);//Vector3.Multiply(ref trans, ref negateY, out trans);
            Vector3 newM1 = Vector3.TransformVector(trans, R) + M1;

            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M2 - M1;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            if (isRightKnee) trans = Vector3.Multiply(trans, negateY); //Vector3.Multiply(ref trans, ref negateY, out trans);
            Vector3 newM2 = Vector3.TransformVector(trans, R) + M2;

            if (isRightKnee)
            {
                markers[MarkerNames.rightOuterKnee] = newM1;//FLE
                markers[MarkerNames.rightOuterAnkle] = newM2;//FAL
            }
            else
            {
                markers[MarkerNames.leftOuterKnee] = newM1;//FLE
                markers[MarkerNames.leftOuterAnkle] = newM2;//FAL
            }
        }
        private void NameSet(Dictionary<string,Vector3> llm, string prefix = "")
        {
            var quary = MarkerNames.bodyBaseAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.bodyBase = prefix + ((quary == null) ? MarkerNames.bodyBase : quary);

            quary = MarkerNames.leftHipAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftHip = prefix + ((quary == null) ? MarkerNames.leftHip : quary);

            quary = MarkerNames.rightHipAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightHip = prefix + ((quary == null) ? MarkerNames.rightHip : quary);

            quary = MarkerNames.spineAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.spine = prefix + ((quary == null) ? MarkerNames.spine : quary);

            quary = MarkerNames.neckAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.neck = prefix + ((quary == null) ? MarkerNames.neck : quary);

            quary = MarkerNames.chestAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.chest = prefix + ((quary == null) ? MarkerNames.chest : quary);

            quary = MarkerNames.leftShoulderAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftShoulder = prefix + ((quary == null) ? MarkerNames.leftShoulder : quary);

            quary = MarkerNames.rightShoulderAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightShoulder = prefix + ((quary == null) ? MarkerNames.rightShoulder : quary);

            quary = MarkerNames.headAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.head = prefix + ((quary == null) ? MarkerNames.head : quary);

            quary = MarkerNames.leftHeadAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftHead = prefix + ((quary == null) ? MarkerNames.leftHead : quary);

            quary = MarkerNames.rightHeadAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightHead = prefix + ((quary == null) ? MarkerNames.rightHead : quary);

            quary = MarkerNames.leftUpperKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftUpperKnee = prefix + ((quary == null) ? MarkerNames.leftUpperKnee : quary);

            quary = MarkerNames.rightUpperKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightUpperKnee = prefix + ((quary == null) ? MarkerNames.rightUpperKnee : quary);

            quary = MarkerNames.leftOuterKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftOuterKnee = prefix + ((quary == null) ? MarkerNames.leftOuterKnee : quary);

            quary = MarkerNames.rightOuterKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightOuterKnee = prefix + ((quary == null) ? MarkerNames.rightOuterKnee : quary);

            quary = MarkerNames.leftLowerKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftLowerKnee = prefix + ((quary == null) ? MarkerNames.leftLowerKnee : quary);

            quary = MarkerNames.rightLowerKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightLowerKnee = prefix + ((quary == null) ? MarkerNames.rightLowerKnee : quary);

            quary = MarkerNames.leftOuterAnkleAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftOuterAnkle = prefix + ((quary == null) ? MarkerNames.leftOuterAnkle : quary);

            quary = MarkerNames.rightOuterAnkleAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightOuterAnkle = prefix + ((quary == null) ? MarkerNames.rightOuterAnkle : quary);

            quary = MarkerNames.leftHeelAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftHeel = prefix + ((quary == null) ? MarkerNames.leftHeel : quary);

            quary = MarkerNames.rightHeelAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightHeel = prefix + ((quary == null) ? MarkerNames.rightHeel : quary);

            quary = MarkerNames.leftToe2AKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftToe2 = prefix + ((quary == null) ? MarkerNames.leftToe2 : quary);

            quary = MarkerNames.rightToe2AKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightToe2 = prefix + ((quary == null) ? MarkerNames.rightToe2 : quary);

            quary = MarkerNames.leftElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftElbow = prefix + ((quary == null) ? MarkerNames.leftElbow : quary);

            quary = MarkerNames.rightElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightElbow = prefix + ((quary == null) ? MarkerNames.rightElbow : quary);

            quary = MarkerNames.leftInnerElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftInnerElbow = prefix + ((quary == null) ? MarkerNames.leftInnerElbow : quary);

            quary = MarkerNames.rightInnerElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightInnerElbow = prefix + ((quary == null) ? MarkerNames.rightInnerElbow : quary);

            quary = MarkerNames.leftOuterElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftOuterElbow = prefix + ((quary == null) ? MarkerNames.leftOuterElbow : quary);

            quary = MarkerNames.rightOuterElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightOuterElbow = prefix + ((quary == null) ? MarkerNames.rightOuterElbow : quary);

            quary = MarkerNames.leftWristAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftWrist = prefix + ((quary == null) ? MarkerNames.leftWrist : quary);

            quary = MarkerNames.rightWristAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightWrist = prefix + ((quary == null) ? MarkerNames.rightWrist : quary);

            quary = MarkerNames.leftWristRadiusAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftWristRadius = prefix + ((quary == null) ? MarkerNames.leftWristRadius : quary);

            quary = MarkerNames.rightWristRadiusAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightWristRadius = prefix + ((quary == null) ? MarkerNames.rightWristRadius : quary);

            quary = MarkerNames.leftHandAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftHand = prefix + ((quary == null) ? MarkerNames.leftHand : quary);

            quary = MarkerNames.rightHandAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightHand = prefix + ((quary == null) ? MarkerNames.rightHand : quary);
            
            quary = MarkerNames.rightFingerAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightFinger = prefix + ((quary == null) ? MarkerNames.rightHand : quary);

            quary = MarkerNames.leftFingerAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftFinger = prefix + ((quary == null) ? MarkerNames.rightHand : quary);

        }
        //private void NameSet(List<LabeledMarker> llm)
        //{
        //    var quary = llm.FirstOrDefault(z => MarkerNames.bodyBaseAKA.Contains(z.label));
        //    MarkerNames.bodyBase = (quary == null) ? MarkerNames.bodyBase : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftHipAKA.Contains(z.label));
        //    MarkerNames.leftHip = (quary == null) ? MarkerNames.leftHip : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightHipAKA.Contains(z.label));
        //    MarkerNames.rightHip = (quary == null) ? MarkerNames.rightHip : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.spineAKA.Contains(z.label));
        //    MarkerNames.spine = (quary == null) ? MarkerNames.spine : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.neckAKA.Contains(z.label));
        //    MarkerNames.neck = (quary == null) ? MarkerNames.neck : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.chestAKA.Contains(z.label));
        //    MarkerNames.chest = (quary == null) ? MarkerNames.chest : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftShoulderAKA.Contains(z.label));
        //    MarkerNames.leftShoulder = (quary == null) ? MarkerNames.leftShoulder : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightShoulderAKA.Contains(z.label));
        //    MarkerNames.rightShoulder = (quary == null) ? MarkerNames.rightShoulder : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.headAKA.Contains(z.label));
        //    MarkerNames.head = (quary == null) ? MarkerNames.head : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftHeadAKA.Contains(z.label));
        //    MarkerNames.leftHead = (quary == null) ? MarkerNames.leftHead : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightHeadAKA.Contains(z.label));
        //    MarkerNames.rightHead = (quary == null) ? MarkerNames.rightHead : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftUpperKneeAKA.Contains(z.label));
        //    MarkerNames.leftUpperKnee = (quary == null) ? MarkerNames.leftUpperKnee : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightUpperKneeAKA.Contains(z.label));
        //    MarkerNames.rightUpperKnee = (quary == null) ? MarkerNames.rightUpperKnee : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftOuterKneeAKA.Contains(z.label));
        //    MarkerNames.leftOuterKnee = (quary == null) ? MarkerNames.leftOuterKnee : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightOuterKneeAKA.Contains(z.label));
        //    MarkerNames.rightOuterKnee = (quary == null) ? MarkerNames.rightOuterKnee : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftLowerKneeAKA.Contains(z.label));
        //    MarkerNames.leftLowerKnee = (quary == null) ? MarkerNames.leftLowerKnee : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightLowerKneeAKA.Contains(z.label));
        //    MarkerNames.rightLowerKnee = (quary == null) ? MarkerNames.rightLowerKnee : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftOuterAnkleAKA.Contains(z.label));
        //    MarkerNames.leftOuterAnkle = (quary == null) ? MarkerNames.leftOuterAnkle  : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightOuterAnkleAKA.Contains(z.label));
        //    MarkerNames.rightOuterAnkle = (quary == null) ? MarkerNames.rightOuterAnkle : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftHeelAKA.Contains(z.label));
        //    MarkerNames.leftHeel = (quary == null) ? MarkerNames.leftHeel : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightHeelAKA.Contains(z.label));
        //    MarkerNames.rightHeel = (quary == null) ? MarkerNames.rightHeel : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftToe2AKA.Contains(z.label));
        //    MarkerNames.leftToe2 = (quary == null) ? MarkerNames.leftToe2 : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightToe2AKA.Contains(z.label));
        //    MarkerNames.rightToe2 = (quary == null) ? MarkerNames.rightToe2 : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftElbowAKA.Contains(z.label));
        //    MarkerNames.leftElbow = (quary == null) ? MarkerNames.leftElbow : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightElbowAKA.Contains(z.label));
        //    MarkerNames.rightElbow = (quary == null) ? MarkerNames.rightElbow : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftInnerElbowAKA.Contains(z.label));
        //    MarkerNames.leftInnerElbow = (quary == null) ? MarkerNames.leftInnerElbow : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightInnerElbowAKA.Contains(z.label));
        //    MarkerNames.rightInnerElbow = (quary == null) ? MarkerNames.rightInnerElbow : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftOuterElbowAKA.Contains(z.label));
        //    MarkerNames.leftOuterElbow = (quary == null) ? MarkerNames.leftOuterElbow : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightOuterElbowAKA.Contains(z.label));
        //    MarkerNames.rightOuterElbow = (quary == null) ? MarkerNames.rightOuterElbow : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftWristAKA.Contains(z.label));
        //    MarkerNames.leftWrist = (quary == null) ? MarkerNames.leftWrist : quary.label; 

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightWristAKA.Contains(z.label));
        //    MarkerNames.rightWrist = (quary == null) ? MarkerNames.rightWrist : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftWristRadiusAKA.Contains(z.label));
        //    MarkerNames.leftWristRadius = (quary == null) ? MarkerNames.leftWristRadius : quary.label; 

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightWristRadiusAKA.Contains(z.label));
        //    MarkerNames.rightWristRadius = (quary == null) ? MarkerNames.rightWristRadius : quary.label;

        //    quary = llm.FirstOrDefault(z => MarkerNames.leftHandAKA.Contains(z.label));
        //    MarkerNames.leftHand = (quary == null) ? MarkerNames.leftHand : quary.label; 

        //    quary = llm.FirstOrDefault(z => MarkerNames.rightHandAKA.Contains(z.label));
        //    MarkerNames.rightHand = (quary == null) ? MarkerNames.rightHand : quary.label;

        //}
    }
}
