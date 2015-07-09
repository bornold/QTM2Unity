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
             MarkerNames.leftThumb, MarkerNames.rightThumb,
             MarkerNames.leftIndex, MarkerNames.rightIndex,
            };
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
                markersList.ForEach(z => z = prefix + z);
                markersLastFrame = markersList.ToDictionary(kv => kv, kv => Vector3Helper.NaN);
                markersLastFrame[MarkerNames.bodyBase] = new Vector3(0f, 1.0f, 0f); //mid of (0.0774 1.0190 -0.1151, -0.0716 1.0190 -0.1138)
                markersLastFrame[MarkerNames.leftHip] = new Vector3(-0.1f, 1.0f, 0.15f); // 0.0925 0.9983 0.1052
                markersLastFrame[MarkerNames.rightHip] = new Vector3(0.1f, 1.0f, 0.15f); //-0.0887 1.0021 0.1112
                nameSet = true;
            }
            if (markersLastFrame[MarkerNames.leftHip].IsNaN()
              || markersLastFrame[MarkerNames.rightHip].IsNaN()
              || markersLastFrame[MarkerNames.bodyBase].IsNaN())
            {
                UnityEngine.Debug.LogWarning("MISSING Essential markers from last frame");
                UnityEngine.Debug.Log(markersLastFrame[MarkerNames.leftHip] + "\n" + markersLastFrame[MarkerNames.rightHip] + "\n" + markersLastFrame[MarkerNames.bodyBase]);
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
            
            quary = MarkerNames.rightIndexAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightIndex = prefix + ((quary == null) ? MarkerNames.rightIndex : quary);

            quary = MarkerNames.leftIndexAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftIndex = prefix + ((quary == null) ? MarkerNames.leftIndex : quary);

            quary = MarkerNames.rightThumbAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightThumb = prefix + ((quary == null) ? MarkerNames.rightThumb : quary);

            quary = MarkerNames.leftThumbAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftThumb = prefix + ((quary == null) ? MarkerNames.leftIndex : quary);

        }
    }
}
