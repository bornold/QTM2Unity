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
        private Dictionary<string, Vector3> markersComplete;
        private Dictionary<string, Vector3> markers;
        private Dictionary<string, Vector3> markersLastFrame;
        public MarkersPreprocessor()
        {
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
             MarkerNames.leftAnkle, MarkerNames.rightAnkle,
             MarkerNames.leftHeel, MarkerNames.rightHeel,
             MarkerNames.leftFoot, MarkerNames.rightFoot,
             MarkerNames.leftElbow, MarkerNames.rightElbow,
             MarkerNames.leftInnerElbow, MarkerNames.rightInnerElbow, 
             MarkerNames.leftOuterElbow, MarkerNames.rightOuterElbow,
             MarkerNames.leftWrist, MarkerNames.rightWrist,
             MarkerNames.leftWristRadius, MarkerNames.rightWristRadius,
             MarkerNames.leftHand, MarkerNames.rightHand,
            };
            markersComplete = markersList.ToDictionary(k => k, v => new Vector3(float.NaN, float.NaN, float.NaN));
            markers = markersComplete;
        }
        public Dictionary<string, Vector3> ProcessMarkers(List<LabeledMarker> newMarkers)
        {
            // Copy last frames markers
            markersLastFrame = markers;
            // Copy new markers to dictionary
            markers = newMarkers.ToDictionary(k => k.label, v => v.position);
            markers = markers.Concat(markersComplete.Where(kvp => !markers.ContainsKey(kvp.Key))).ToDictionary(kv => kv.Key, kv => kv.Value);
            if (markers[MarkerNames.leftHip].IsNaN() 
                || markers[MarkerNames.rightHip].IsNaN()
                || markers[MarkerNames.bodyBase].IsNaN())
            {
                MissingEssientialMarkers();
            }
            return markers;

        }
        private void MissingEssientialMarkers()
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
                string first = markersList.FirstOrDefault(name => !markers[name].IsNaN() && !markersLastFrame[name].IsNaN());
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
        }
        private void MoveMarkerToSkin()
        {
            test(true);
            test(false);
        }
        private void test(bool isRightKnee)
        {
            // Stolen from Visual3D
            Vector3 x, z, M1, M2, M3, negateY = new Vector3(1f, -1f, 1f);
            Matrix4 R;
            if (isRightKnee)
            {
                M1 = markers[MarkerNames.rightOuterKnee];//FLE
                M3 = markers[MarkerNames.rightLowerKnee];//TTC
                M2 = markers[MarkerNames.rightAnkle];//FAL
            }
            else
            {
                M1 = markers[MarkerNames.leftOuterKnee];//FLE
                M3 = markers[MarkerNames.leftLowerKnee];//TTC
                M2 = markers[MarkerNames.leftAnkle];//FAL
            }
            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M1 - M2;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            Vector3 trans = new Vector3(-BodyData.MarkerCentreToSkinSurface * 0.7071f, BodyData.MarkerCentreToSkinSurface * 0.7071f, 0f);
            if (isRightKnee) Vector3.Multiply(ref trans, ref negateY, out trans);
            Vector3 newM1 = Vector3.TransformVector(trans, R) + M1;

            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M2 - M1;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            if (isRightKnee) Vector3.Multiply(ref trans, ref negateY, out trans);
            Vector3 newM2 = Vector3.TransformVector(trans, R) + M2;

            if (isRightKnee)
            {
                markers[MarkerNames.rightOuterKnee] = newM1;//FLE
                markers[MarkerNames.rightAnkle] = newM2;//FAL
            }
            else
            {
                markers[MarkerNames.leftOuterKnee] = newM1;//FLE
                markers[MarkerNames.leftAnkle] = newM2;//FAL
            }
        }
    }
}
