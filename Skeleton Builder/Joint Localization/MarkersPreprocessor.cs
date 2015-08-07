#region --- LINCENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using OpenTK;
using QualisysRealTime.Unity;
using System.Collections.Generic;
using System.Linq;

namespace QualisysRealTime.Unity.Skeleton
{
    class MarkersPreprocessor
    {
        private Dictionary<string, Vector3> markers = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector3> markersLastFrame = new Dictionary<string, Vector3>();
        private string prefix;
        private bool sacrumBetween = false;
        private bool frontHeadBetween = false;

        private string[] sacrumBetweenMarkers;
        private string[] frontHeadBetweenMarkers;
        /// <summary>
        /// The ma
        /// </summary>
        private Vector3
            lastSACRUMknown = Vector3Helper.MidPoint(new Vector3(0.0774f, 1.0190f, -0.1151f), new Vector3(-0.0716f, 1.0190f, -0.1138f)),
            lastRIASknown = new Vector3(0.0925f, 0.9983f, 0.1052f),
            lastLIASknown = new Vector3(-0.0887f, 1.0021f, 0.1112f);

        private MarkersNames m;
        /// <summary>
        /// Constructor sets the markers name in the MarkesName class used for joint localization
        /// </summary>
        /// <param name="labelMarkers">The list of labelmarkets</param>
        /// <param name="markerNames">A reference to the markers names</param>
        /// <param name="bodyPrefix">Any possible prefix of the markersname</param>
        public MarkersPreprocessor(List<LabeledMarker> labelMarkers, out MarkersNames markerNames, string bodyPrefix = "")
        {
            this.prefix = bodyPrefix;
            markers = new Dictionary<string, Vector3>();
            for (int i = 0; i < labelMarkers.Count; i++)
            {
                markers.Add(labelMarkers[i].Label, labelMarkers[i].Position.Convert());
            }
            markerNames  = NameSet(markers.Keys);
            //foreach (var n in markerNames) UnityEngine.Debug.Log(n);
            m = markerNames;
            markersLastFrame = new Dictionary<string, Vector3>();
            foreach (var mark in markerNames)
            {
                 markersLastFrame.Add(mark, Vector3Helper.NaN);
            }
            markersLastFrame[m.bodyBase] = lastSACRUMknown;
            markersLastFrame[m.leftHip] = lastLIASknown;
            markersLastFrame[m.rightHip] = lastRIASknown;
        }
        /// <summary>
        /// Prepare the markerset for Joint localization, predicts the 
        /// </summary>
        /// <param name="labelMarkers">The list of labelmarkets</param>
        /// <param name="newMarkers">a reference to the dictionary to be </param>
        /// <param name="prefix">The possible prefix of all markers</param>
        public void ProcessMarkers(List<LabeledMarker> labelMarkers, out Dictionary<string,Vector3> newMarkers, string prefix)
        {
            var temp = markers;
            markers = markersLastFrame;
            markersLastFrame = temp;
            markers.Clear();
            for (int i = 0; i < labelMarkers.Count; i++)
            {
                markers.Add(labelMarkers[i].Label, labelMarkers[i].Position.Convert());
            }

            foreach (var markername in m)
            {
                if (!markers.ContainsKey(markername))
                {
                    markers.Add(markername, Vector3Helper.NaN);
                }
            }
            // sacrum can be defined by two markers
            if (sacrumBetween)
            {
                markers[m.bodyBase] = 
                    Vector3Helper.MidPoint(markers[sacrumBetweenMarkers[0]],
                                            markers[sacrumBetweenMarkers[1]]);
            }
            // 
            if (frontHeadBetween)
            {
                markers[m.head] = 
                        Vector3Helper.MidPoint(markers[frontHeadBetweenMarkers[0]],
                                            markers[frontHeadBetweenMarkers[1]]);
            }
            if (markers[m.leftHip].IsNaN()
                || markers[m.rightHip].IsNaN()
                || markers[m.bodyBase].IsNaN())
            {
                MissingEssientialMarkers(markers);
            }
            else
            {
                lastSACRUMknown = markers[m.bodyBase];
                lastRIASknown = markers[m.rightHip];
                lastLIASknown = markers[m.leftHip];
            }
            //UnityDebug.DrawLine(markers[m.bodyBase], markers[m.rightHip]);
            //UnityDebug.DrawLine(markers[m.bodyBase], markers[m.leftHip]);
            //UnityDebug.DrawLine(markers[m.leftHip], markers[m.rightHip]);
            newMarkers = markers;
        }
        /// <summary>
        /// If any of the hip markers are missing, we predict them using the last position
        /// </summary>
        /// <param name="markers">The dictionary of markers</param>
        private void MissingEssientialMarkers(Dictionary<string,Vector3> markers)
        {
            Vector3 dirVec1, dirVec2, possiblePos1, possiblePos2,
                    sacrumLastFrame = lastSACRUMknown,
                    liasLastFrame   = lastLIASknown,
                    riasLastFrame   = lastRIASknown;

            Vector3
                Sacrum = markers[m.bodyBase],
                RIAS = markers[m.rightHip],
                LIAS = markers[m.leftHip];
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
                                QuaternionHelper2.GetRotationBetween(
                                (RIAS - Sacrum), (riasLastFrame - sacrumLastFrame))
                                );
                    Vector3 transVec1 = Vector3.Transform(dirVec1, (between));
                    Vector3 transVec2 = Vector3.Transform(dirVec2, (between));
                    possiblePos1 = Sacrum + transVec1; // add vector from sacrum too lias last frame to this frames' sacrum
                    possiblePos2 = RIAS + transVec2;
                    markers[m.leftHip] = DontMovedToMuch(markersLastFrame[m.leftHip], Vector3Helper.MidPoint(possiblePos1, possiblePos2)); // get mid point of possible positions

                }
                else if (l) // sacrum  and lias exists, rias missing
                {
                    dirVec1 = riasLastFrame - sacrumLastFrame;
                    dirVec2 = riasLastFrame - liasLastFrame;
                    Quaternion between = Quaternion.Invert(
                                            QuaternionHelper2.GetRotationBetween(
                                            (LIAS - Sacrum), (liasLastFrame - sacrumLastFrame))
                                            );
                    Vector3 transVec1 = Vector3.Transform(dirVec1, (between));
                    Vector3 transVec2 = Vector3.Transform(dirVec2, (between));
                    possiblePos1 = Sacrum + transVec1;
                    possiblePos2 = LIAS + transVec2;
                    markers[m.rightHip] = DontMovedToMuch(markersLastFrame[m.rightHip], Vector3Helper.MidPoint(possiblePos1, possiblePos2));
                }
                else // only sacrum exists, lias and rias missing
                {
                    markers[m.rightHip] = DontMovedToMuch(markersLastFrame[m.rightHip] , Sacrum + riasLastFrame - sacrumLastFrame);
                    markers[m.leftHip] = DontMovedToMuch(markersLastFrame[m.leftHip], Sacrum + liasLastFrame - sacrumLastFrame);
                }
            }
            else if (r) // rias exists, sacrum missing
            {
                if (l) // rias and ias exists, sacrum missing
                {
                    dirVec1 = sacrumLastFrame - riasLastFrame;
                    dirVec2 = sacrumLastFrame - liasLastFrame;

                    Quaternion between = Quaternion.Invert(
                        QuaternionHelper2.GetRotationBetween(
                        (LIAS - RIAS), (liasLastFrame - riasLastFrame))
                        );
                    Vector3 transVec1 = Vector3.Transform(dirVec1, (between));
                    Vector3 transVec2 = Vector3.Transform(dirVec2, (between));
                    possiblePos1 = RIAS + transVec1;
                    possiblePos2 = LIAS + transVec2;
                    markers[m.bodyBase] = DontMovedToMuch(markersLastFrame[m.bodyBase] ,Vector3Helper.MidPoint(possiblePos1,possiblePos2));
                }
                else // only rias exists, lias and sacrum missing
                {
                    markers[m.bodyBase] = DontMovedToMuch(markersLastFrame[m.bodyBase], RIAS + sacrumLastFrame - riasLastFrame);
                    markers[m.leftHip] = DontMovedToMuch(markersLastFrame[m.leftHip], RIAS + liasLastFrame - riasLastFrame);
                }
            }
            else if (l) // only lias exists, rias and sacrum missing
            {
                markers[m.bodyBase] = DontMovedToMuch(markersLastFrame[m.bodyBase], LIAS + sacrumLastFrame - liasLastFrame);
                markers[m.rightHip] = DontMovedToMuch(markersLastFrame[m.rightHip], LIAS + riasLastFrame - liasLastFrame);
            }
            else // all markers missing
            {
                string first = null;
                foreach (var mName in m)
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

                    markers[m.rightHip] = DontMovedToMuch(markersLastFrame[m.rightHip], riasLastFrame + offset);
                    markers[m.leftHip] = DontMovedToMuch(markersLastFrame[m.leftHip], liasLastFrame + offset);
                    markers[m.bodyBase] = DontMovedToMuch(markersLastFrame[m.bodyBase], sacrumLastFrame + offset);
                }
                else
                {
                    markers[m.rightHip] = markersLastFrame[m.rightHip];
                    markers[m.leftHip] = markersLastFrame[m.leftHip];
                    markers[m.bodyBase] = markersLastFrame[m.bodyBase];
                }
            }
        }

        /// <summary>
        /// Finds aliases of different markers and replaces the names
        /// </summary>
        /// <param name="markersNames">A collection of the names of the markers</param>
        /// <returns>The set marker names</returns>
        private MarkersNames NameSet(ICollection<string> markersNames)
        {
            MarkersNames m = new MarkersNames();
            #region hip
            var quary = MarkerNames.bodyBaseAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            if (quary == null)
            {
                var q2 = MarkerNames.bodyBasebetween.FirstOrDefault(n => markersNames.Contains(prefix + n[0]) && markersNames.Contains(prefix + n[1]));
                if (q2 != null)
                {
                    sacrumBetween = true;
                    sacrumBetweenMarkers = new string[2];
                    sacrumBetweenMarkers[0] = prefix + q2[0];
                    sacrumBetweenMarkers[1] = prefix + q2[1];

                }
            }
            else
            {
                m.bodyBase = prefix + quary;
            }

            quary = MarkerNames.leftHipAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftHip = ((quary == null) ? m.leftHip : prefix + quary);

            quary = MarkerNames.rightHipAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightHip = ((quary == null) ? m.rightHip : prefix + quary);
            #endregion

            #region upperbody
            quary = MarkerNames.spineAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.spine = ((quary == null) ? m.spine :prefix + quary);

            quary = MarkerNames.neckAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.neck = ((quary == null) ? m.neck :prefix + quary);

            quary = MarkerNames.chestAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.chest = ((quary == null) ? m.chest :prefix + quary);

            quary = MarkerNames.leftShoulderAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftShoulder = ((quary == null) ? m.leftShoulder :prefix + quary);

            quary = MarkerNames.rightShoulderAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightShoulder = ((quary == null) ? m.rightShoulder :prefix + quary);
            #endregion

            #region head
            quary = MarkerNames.headAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            if (quary == null)
            {
                var q2 = MarkerNames.headBetween.FirstOrDefault(n => markersNames.Contains(prefix + n[0]) && markersNames.Contains(prefix + n[1]));
                if (q2 != null)
                {
                    frontHeadBetween = true;
                    frontHeadBetweenMarkers = new string[2];;
                    frontHeadBetweenMarkers[0] = prefix + q2[0];
                    frontHeadBetweenMarkers[1] = prefix + q2[1];
                }
            } else m.head = prefix + quary;

            quary = MarkerNames.leftHeadAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            if (quary == null)  m.leftHead = prefix + quary;

            quary = MarkerNames.rightHeadAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightHead = ((quary == null) ? m.rightHead :prefix + quary);
            #endregion

            #region legs
            quary = MarkerNames.leftUpperKneeAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftUpperKnee = ((quary == null) ? m.leftUpperKnee :prefix + quary);

            quary = MarkerNames.rightUpperKneeAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightUpperKnee = ((quary == null) ? m.rightUpperKnee :prefix + quary);

            quary = MarkerNames.leftOuterKneeAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftOuterKnee = ((quary == null) ? m.leftOuterKnee :prefix + quary);

            quary = MarkerNames.rightOuterKneeAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightOuterKnee = ((quary == null) ? m.rightOuterKnee :prefix + quary);

            quary = MarkerNames.leftLowerKneeAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftLowerKnee = ((quary == null) ? m.leftLowerKnee :prefix + quary);

            quary = MarkerNames.rightLowerKneeAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightLowerKnee = ((quary == null) ? m.rightLowerKnee :prefix + quary);
            #endregion

            #region foot
            quary = MarkerNames.leftOuterAnkleAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftOuterAnkle = ((quary == null) ? m.leftOuterAnkle :prefix + quary);

            quary = MarkerNames.rightOuterAnkleAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightOuterAnkle = ((quary == null) ? m.rightOuterAnkle :prefix + quary);

            quary = MarkerNames.leftHeelAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftHeel = ((quary == null) ? m.leftHeel :prefix + quary);

            quary = MarkerNames.rightHeelAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightHeel = ((quary == null) ? m.rightHeel :prefix + quary);

            quary = MarkerNames.leftToe2AKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            if (quary == null)
            {
                quary = MarkerNames.leftToe1AKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            }
            m.leftToe2 = ((quary == null) ? m.leftToe2 :prefix + quary);

            quary = MarkerNames.rightToe2AKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            if (quary == null)
            {
                quary = MarkerNames.rightToe1AKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            }
            m.rightToe2 = ((quary == null) ? m.rightToe2 :prefix + quary);
            #endregion

            #region arms
            quary = MarkerNames.leftElbowAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftElbow = ((quary == null) ? m.leftElbow :prefix + quary);

            quary = MarkerNames.rightElbowAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightElbow = ((quary == null) ? m.rightElbow :prefix + quary);

            quary = MarkerNames.leftInnerElbowAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftInnerElbow = ((quary == null) ? m.leftInnerElbow :prefix + quary);

            quary = MarkerNames.rightInnerElbowAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightInnerElbow = ((quary == null) ? m.rightInnerElbow :prefix + quary);

            quary = MarkerNames.leftOuterElbowAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftOuterElbow = ((quary == null) ? m.leftOuterElbow :prefix + quary);

            quary = MarkerNames.rightOuterElbowAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightOuterElbow = ((quary == null) ? m.rightOuterElbow :prefix + quary);

            quary = MarkerNames.leftWristAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftWrist = ((quary == null) ? m.leftWrist :prefix + quary);

            quary = MarkerNames.rightWristAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightWrist = ((quary == null) ? m.rightWrist :prefix + quary);

            quary = MarkerNames.leftWristRadiusAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftWristRadius = ((quary == null) ? m.leftWristRadius :prefix + quary);

            quary = MarkerNames.rightWristRadiusAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightWristRadius = ((quary == null) ? m.rightWristRadius :prefix + quary);
            #endregion

            #region hands
            quary = MarkerNames.leftHandAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftHand = ((quary == null) ? m.leftHand :prefix + quary);

            quary = MarkerNames.rightHandAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightHand = ((quary == null) ? m.rightHand :prefix + quary);
            
            quary = MarkerNames.rightIndexAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightIndex = ((quary == null) ? m.rightIndex :prefix + quary);

            quary = MarkerNames.leftIndexAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftIndex = ((quary == null) ? m.leftIndex :prefix + quary);

            quary = MarkerNames.rightThumbAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.rightThumb = ((quary == null) ? m.rightThumb :prefix + quary);

            quary = MarkerNames.leftThumbAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            m.leftThumb = ((quary == null) ? m.leftThumb :prefix + quary);
            #endregion
            return m;
        }

        private void SetName(ICollection<string> markerNames, ref List<string> alias, ref string name, string prefix = "")
        {
            var quary = alias.FirstOrDefault(n => markerNames.Contains(prefix + n));
            name = ((quary == null) ? name : prefix + quary);
        }
        private Vector3 DontMovedToMuch(Vector3 from, Vector3 to)
        {
            Vector3 move = (to - from);
            if (move.Length > 0.02f)
            {
                move.NormalizeFast();
                move *= 0.02f;
                return from + move;
            }
            return to;
        }
    }
}