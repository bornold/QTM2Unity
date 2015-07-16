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
        private Dictionary<string, Vector3> markers = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector3> markersLastFrame = new Dictionary<string, Vector3>();
        private bool nameSet = false;
        private string prefix;
        private bool sacrumBetween = false;
        private bool frontHeadBetween = false;

        private string[] sacrumBetweenMarkers;
        private string[] frontHeadBetweenMarkers;

        private Vector3 lastSACRUMknown = Vector3Helper.MidPoint(new Vector3(0.0774f, 1.0190f, -0.1151f), new Vector3(-0.0716f, 1.0190f, -0.1138f));
        private Vector3 lastRIASknown = new Vector3(0.0925f, 0.9983f, 0.1052f);
        private Vector3 lastLIASknown = new Vector3(-0.0887f, 1.0021f, 0.1112f);
        private Vector3 offset = Vector3.Zero;

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
        public bool ProcessMarkers(List<LabeledMarker> labelMarkers, out Dictionary<string,Vector3> newMarkers, string prefix)
        {
            var temp = markers;
            markers = markersLastFrame;
            markersLastFrame = temp;
            markers.Clear();
            for (int i = 0; i < labelMarkers.Count; i++)
            {
                markers.Add(labelMarkers[i].label, labelMarkers[i].position);
            }

            if (!nameSet || this.prefix != prefix)
            {
                this.prefix = prefix;
                NameSet(markers);
                markersList.ForEach(z => z = prefix + z);
                markersLastFrame = new Dictionary<string, Vector3>();
                foreach (var mark in markersList)
                {
                    if (!markersLastFrame.ContainsKey(mark))
                    {
                        markersLastFrame.Add(mark, Vector3Helper.NaN);
                    }
                    if (!markersLastFrame.ContainsKey(mark))
                    {
                        markersLastFrame.Add(mark, Vector3Helper.NaN);
                    }
                }
                markersLastFrame[MarkerNames.bodyBase] = lastSACRUMknown;
                markersLastFrame[MarkerNames.leftHip] = lastLIASknown;
                markersLastFrame[MarkerNames.rightHip] = lastRIASknown;
                nameSet = true;
                //UnityEngine.Debug.Log(prefix);
                //foreach (var mn in markersList) UnityEngine.Debug.Log(mn);
                //if (sacrumBetween)
                //{
                //    UnityEngine.Debug.Log(sacrumBetweenMarkers[0]);
                //    UnityEngine.Debug.Log(sacrumBetweenMarkers[1]);
                //}
                //else { UnityEngine.Debug.LogError("erroll HIP"); }
                //if (frontHeadBetween) 
                //{
                //    UnityEngine.Debug.Log(frontHeadBetweenMarkers[0]);
                //    UnityEngine.Debug.Log(frontHeadBetweenMarkers[1]);
                //}
                //else { UnityEngine.Debug.LogError("erroll HEAD"); }
            }
            // GC: 40B GC 
            for (int i = 0; i < markersList.Count; i++)
            {
                if (!markers.ContainsKey(markersList[i]))
                {
                    markers.Add(markersList[i], Vector3Helper.NaN);
                }
            }

            if (sacrumBetween)
            {
                markers[MarkerNames.bodyBase] = 
                    Vector3Helper.MidPoint(markers[sacrumBetweenMarkers[0]],
                                            markers[sacrumBetweenMarkers[1]]);
                //UnityDebug.DrawLine(markers[sacrumBetweenMarkers[0]], markers[sacrumBetweenMarkers[1]], UnityEngine.Color.red);
                //UnityDebug.DrawLine(Vector3.Zero, markers[MarkerNames.bodyBase], UnityEngine.Color.red);

            }

            if (frontHeadBetween)
            {
                markers[MarkerNames.head] = 
                        Vector3Helper.MidPoint(markers[frontHeadBetweenMarkers[0]],
                                            markers[frontHeadBetweenMarkers[1]]);
            }
            if (markers[MarkerNames.leftHip].IsNaN()
                || markers[MarkerNames.rightHip].IsNaN()
                || markers[MarkerNames.bodyBase].IsNaN())
            {
                MissingEssientialMarkers(markers);
                //UnityDebug.DrawLine(markers[MarkerNames.bodyBase], markers[MarkerNames.rightHip]);
                //UnityDebug.DrawLine(markers[MarkerNames.bodyBase], markers[MarkerNames.leftHip]);
                //UnityDebug.DrawLine(markers[MarkerNames.leftHip], markers[MarkerNames.rightHip]);
            }
            //else
            {
                lastSACRUMknown = markers[MarkerNames.bodyBase];
                lastRIASknown = markers[MarkerNames.rightHip];
                lastLIASknown = markers[MarkerNames.leftHip];
            }
            newMarkers = markers;
            //MoveLegMarkers(ref markers, true);
            //MoveLegMarkers(ref markers, false);
            return true;
        }
        private void MissingEssientialMarkers(Dictionary<string,Vector3> markers)
        {
            Vector3 dirVec1, dirVec2, possiblePos1, possiblePos2,
                    sacrumLastFrame = lastSACRUMknown,
                    liasLastFrame   = lastLIASknown,
                    riasLastFrame   = lastRIASknown;

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
                    dirVec1 = liasLastFrame - sacrumLastFrame; // vector from sacrum too lias in last frame
                    dirVec2 = liasLastFrame - riasLastFrame;
                    Quaternion between = Quaternion.Invert(
                                QuaternionHelper.GetRotationBetween(
                                (RIAS - Sacrum), (riasLastFrame - sacrumLastFrame))
                                );
                    Vector3 transVec1 = Vector3.Transform(dirVec1, (between));
                    Vector3 transVec2 = Vector3.Transform(dirVec2, (between));
                    possiblePos1 = Sacrum + transVec1; // add vector from sacrum too lias last frame to this frames' sacrum
                    possiblePos2 = RIAS + transVec2;
                    markers[MarkerNames.leftHip] =  Vector3Helper.MidPoint(possiblePos1,possiblePos2); // get mid point of possible positions

                }
                else if (l) // sacrum  and lias exists, rias missing
                {
                    dirVec1 = riasLastFrame - sacrumLastFrame;
                    dirVec2 = riasLastFrame - liasLastFrame;
                    Quaternion between = Quaternion.Invert(
                                            QuaternionHelper.GetRotationBetween(
                                            (LIAS - Sacrum), (liasLastFrame - sacrumLastFrame))
                                            );
                    Vector3 transVec1 = Vector3.Transform(dirVec1, (between));
                    Vector3 transVec2 = Vector3.Transform(dirVec2, (between));
                    possiblePos1 = Sacrum + transVec1;
                    possiblePos2 = LIAS + transVec2;
                    markers[MarkerNames.rightHip] = Vector3Helper.MidPoint(possiblePos1,possiblePos2);
                }
                else // only sacrum exists, lias and rias missing
                {
                    markers[MarkerNames.rightHip] = Sacrum + riasLastFrame - sacrumLastFrame;
                    markers[MarkerNames.leftHip] = Sacrum + liasLastFrame - sacrumLastFrame;
                }
            }
            else if (r) // rias exists, sacrum missing
            {
                if (l) // rias and ias exists, sacrum missing
                {
                    dirVec1 = sacrumLastFrame - riasLastFrame;
                    dirVec2 = sacrumLastFrame - liasLastFrame;

                    Quaternion between = Quaternion.Invert(
                        QuaternionHelper.GetRotationBetween(
                        (LIAS - RIAS), (liasLastFrame - riasLastFrame))
                        );
                    Vector3 transVec1 = Vector3.Transform(dirVec1, (between));
                    Vector3 transVec2 = Vector3.Transform(dirVec2, (between));
                    possiblePos1 = RIAS + transVec1;
                    possiblePos2 = LIAS + transVec2;
                    markers[MarkerNames.bodyBase] =Vector3Helper.MidPoint(possiblePos1,possiblePos2);
                }
                else // only rias exists, lias and sacrum missing
                {
                    markers[MarkerNames.bodyBase] = RIAS + sacrumLastFrame - riasLastFrame;
                    markers[MarkerNames.leftHip] = RIAS + liasLastFrame - riasLastFrame;
                }
            }
            else if (l) // only lias exists, rias and sacrum missing
            {
                markers[MarkerNames.bodyBase] = LIAS + sacrumLastFrame - liasLastFrame;
                markers[MarkerNames.rightHip] = LIAS + riasLastFrame - liasLastFrame;
            }
            else // all markers missing
            {
                string first = null;
                foreach (var mName in markersList)
                {
                    if (!markers[mName].IsNaN() && !markersLastFrame[mName].IsNaN())
                    {
                        first = mName;
                        break;
                    }
                }
                if (first != null)
                {
                    Vector3 offset = markers[first] - markersLastFrame[first];

                    markers[MarkerNames.rightHip] = riasLastFrame + offset;
                    markers[MarkerNames.leftHip] = liasLastFrame + offset;
                    markers[MarkerNames.bodyBase] = sacrumLastFrame + offset;
                }
                else
                {
                    markers[MarkerNames.rightHip] = riasLastFrame;
                    markers[MarkerNames.leftHip] = liasLastFrame;
                    markers[MarkerNames.bodyBase] = sacrumLastFrame;
                }
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
            if (M1.IsNaN() || M2.IsNaN() || M3.IsNaN()) return;
            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M1 - M2;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            Vector3 trans = (isRightKnee) ? 
                new Vector3(-BodyData.MarkerCentreToSkinSurface * 0.7071f, -BodyData.MarkerCentreToSkinSurface * 0.7071f, 0f) :
                new Vector3(-BodyData.MarkerCentreToSkinSurface * 0.7071f, BodyData.MarkerCentreToSkinSurface * 0.7071f, 0f);
            Vector3 newM1 = Vector3.TransformVector(
                (isRightKnee) ?
                new Vector3(-BodyData.MarkerCentreToSkinSurface * 0.7071f, -BodyData.MarkerCentreToSkinSurface * 0.7071f, 0f) :
                new Vector3(-BodyData.MarkerCentreToSkinSurface * 0.7071f, BodyData.MarkerCentreToSkinSurface * 0.7071f, 0f), 
                R) + M1;

            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M2 - M1;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            //if (isRightKnee) trans = Vector3.Multiply(trans, negateY);
            //Vector3 newM2 = Vector3.TransformVector(trans, R) + M2;

            if (isRightKnee)
            {
                markers[MarkerNames.rightOuterKnee] = newM1;//FLE
                markers[MarkerNames.rightOuterAnkle] = Vector3.TransformVector(Vector3.Multiply(trans, negateY), R) + M2;//FAL
            }
            else
            {
                markers[MarkerNames.leftOuterKnee] = newM1;//FLE
                markers[MarkerNames.leftOuterAnkle] = Vector3.TransformVector(trans, R) + M2;//FAL
            }
        }
        private void NameSet(Dictionary<string, Vector3> llm)
        {
            
            #region hip
            var quary = MarkerNames.bodyBaseAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            if (quary == null)
            {
                var q2 = MarkerNames.bodyBasebetween.FirstOrDefault(n => llm.ContainsKey(prefix + n[0]) && llm.ContainsKey(prefix + n[1]));
                if (q2 != null)
                {
                    sacrumBetween = true;
                    sacrumBetweenMarkers = new string[2];
                    sacrumBetweenMarkers[0] = prefix + q2[0];
                    sacrumBetweenMarkers[1] = prefix + q2[1];

                }
            } else MarkerNames.bodyBase = prefix + quary;

            quary = MarkerNames.leftHipAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftHip = ((quary == null) ? MarkerNames.leftHip : prefix + quary);

            quary = MarkerNames.rightHipAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightHip = ((quary == null) ? MarkerNames.rightHip : prefix + quary);
            #endregion

            #region upperbody
            quary = MarkerNames.spineAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.spine = ((quary == null) ? MarkerNames.spine :prefix + quary);

            quary = MarkerNames.neckAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.neck = ((quary == null) ? MarkerNames.neck :prefix + quary);

            quary = MarkerNames.chestAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.chest = ((quary == null) ? MarkerNames.chest :prefix + quary);

            quary = MarkerNames.leftShoulderAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftShoulder = ((quary == null) ? MarkerNames.leftShoulder :prefix + quary);

            quary = MarkerNames.rightShoulderAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightShoulder = ((quary == null) ? MarkerNames.rightShoulder :prefix + quary);
            #endregion

            #region head
            quary = MarkerNames.headAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            if (quary == null)
            {
                var q2 = MarkerNames.headBetween.FirstOrDefault(n => llm.ContainsKey(prefix + n[0]) && llm.ContainsKey(prefix + n[1]));
                if (q2 != null)
                {
                    frontHeadBetween = true;
                    frontHeadBetweenMarkers = new string[2];;
                    frontHeadBetweenMarkers[0] = prefix + q2[0];
                    frontHeadBetweenMarkers[1] = prefix + q2[1];
                }
            } else MarkerNames.head = prefix + quary;

            quary = MarkerNames.leftHeadAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftHead = ((quary == null) ? MarkerNames.leftHead :prefix + quary);

            quary = MarkerNames.rightHeadAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightHead = ((quary == null) ? MarkerNames.rightHead :prefix + quary);
            #endregion

            #region legs
            quary = MarkerNames.leftUpperKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftUpperKnee = ((quary == null) ? MarkerNames.leftUpperKnee :prefix + quary);

            quary = MarkerNames.rightUpperKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightUpperKnee = ((quary == null) ? MarkerNames.rightUpperKnee :prefix + quary);

            quary = MarkerNames.leftOuterKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftOuterKnee = ((quary == null) ? MarkerNames.leftOuterKnee :prefix + quary);

            quary = MarkerNames.rightOuterKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightOuterKnee = ((quary == null) ? MarkerNames.rightOuterKnee :prefix + quary);

            quary = MarkerNames.leftLowerKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftLowerKnee = ((quary == null) ? MarkerNames.leftLowerKnee :prefix + quary);

            quary = MarkerNames.rightLowerKneeAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightLowerKnee = ((quary == null) ? MarkerNames.rightLowerKnee :prefix + quary);
            #endregion

            #region foot
            quary = MarkerNames.leftOuterAnkleAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftOuterAnkle = ((quary == null) ? MarkerNames.leftOuterAnkle :prefix + quary);

            quary = MarkerNames.rightOuterAnkleAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightOuterAnkle = ((quary == null) ? MarkerNames.rightOuterAnkle :prefix + quary);

            quary = MarkerNames.leftHeelAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftHeel = ((quary == null) ? MarkerNames.leftHeel :prefix + quary);

            quary = MarkerNames.rightHeelAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightHeel = ((quary == null) ? MarkerNames.rightHeel :prefix + quary);

            quary = MarkerNames.leftToe2AKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            if (quary == null)
            {
                quary = MarkerNames.leftToe1AKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            }
            MarkerNames.leftToe2 = ((quary == null) ? MarkerNames.leftToe2 :prefix + quary);

            quary = MarkerNames.rightToe2AKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            if (quary == null)
            {
                quary = MarkerNames.rightToe1AKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            }
            MarkerNames.rightToe2 = ((quary == null) ? MarkerNames.rightToe2 :prefix + quary);
            #endregion

            #region arms
            quary = MarkerNames.leftElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftElbow = ((quary == null) ? MarkerNames.leftElbow :prefix + quary);

            quary = MarkerNames.rightElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightElbow = ((quary == null) ? MarkerNames.rightElbow :prefix + quary);

            quary = MarkerNames.leftInnerElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftInnerElbow = ((quary == null) ? MarkerNames.leftInnerElbow :prefix + quary);

            quary = MarkerNames.rightInnerElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightInnerElbow = ((quary == null) ? MarkerNames.rightInnerElbow :prefix + quary);

            quary = MarkerNames.leftOuterElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftOuterElbow = ((quary == null) ? MarkerNames.leftOuterElbow :prefix + quary);

            quary = MarkerNames.rightOuterElbowAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightOuterElbow = ((quary == null) ? MarkerNames.rightOuterElbow :prefix + quary);

            quary = MarkerNames.leftWristAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftWrist = ((quary == null) ? MarkerNames.leftWrist :prefix + quary);

            quary = MarkerNames.rightWristAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightWrist = ((quary == null) ? MarkerNames.rightWrist :prefix + quary);

            quary = MarkerNames.leftWristRadiusAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftWristRadius = ((quary == null) ? MarkerNames.leftWristRadius :prefix + quary);

            quary = MarkerNames.rightWristRadiusAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightWristRadius = ((quary == null) ? MarkerNames.rightWristRadius :prefix + quary);
            #endregion

            #region hands
            quary = MarkerNames.leftHandAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftHand = ((quary == null) ? MarkerNames.leftHand :prefix + quary);

            quary = MarkerNames.rightHandAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightHand = ((quary == null) ? MarkerNames.rightHand :prefix + quary);
            
            quary = MarkerNames.rightIndexAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightIndex = ((quary == null) ? MarkerNames.rightIndex :prefix + quary);

            quary = MarkerNames.leftIndexAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftIndex = ((quary == null) ? MarkerNames.leftIndex :prefix + quary);

            quary = MarkerNames.rightThumbAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.rightThumb = ((quary == null) ? MarkerNames.rightThumb :prefix + quary);

            quary = MarkerNames.leftThumbAKA.FirstOrDefault(n => llm.ContainsKey(prefix + n));
            MarkerNames.leftThumb = ((quary == null) ? MarkerNames.leftThumb :prefix + quary);
            #endregion
        }
    }
}
