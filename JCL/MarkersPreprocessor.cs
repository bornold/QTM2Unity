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
        private Dictionary<string, Vector3> markers;
        private Dictionary<string, Vector3> markersLastFrame;
        private bool nameSet = false;
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
            markersLastFrame = markersList.ToDictionary(kv => kv, kv => Vector3Helper.NaN);
        }
        public Dictionary<string, Vector3> ProcessMarkers(List<LabeledMarker> newMarkers)
        {
            if (!nameSet)
            {
                NameSet(newMarkers);
                markersLastFrame[MarkerNames.bodyBase] = new Vector3(0f, 1.0f, 0f);
                markersLastFrame[MarkerNames.leftHip] =  new Vector3(-0.1f, 1.0f, 0.15f);  // 0.0925 0.9983 0.1052
                markersLastFrame[MarkerNames.rightHip] = new Vector3(0.1f, 1.0f, 0.15f);
                nameSet = true;
            }

            var query =
                from allMarkers in markersList
                join existingMarkers in newMarkers on allMarkers equals existingMarkers.label into everthing
                from ajoin in everthing.DefaultIfEmpty()
                select new KeyValuePair<string, Vector3>(allMarkers, ((ajoin!=null) ? ajoin.position : Vector3Helper.NaN) );

            markers = query.ToDictionary(kv => kv.Key, kv => kv.Value);
            // Copy last frames markers
            if (markers[MarkerNames.leftHip].IsNaN()
                || markers[MarkerNames.rightHip].IsNaN()
                || markers[MarkerNames.bodyBase].IsNaN())
            {
                MissingEssientialMarkers();
            }
            markersLastFrame = markers;
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
            MoveLegMarkers(true);
            MoveLegMarkers(false);
        }
        private void MoveLegMarkers(bool isRightKnee)
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
                markers[MarkerNames.rightOuterAnkle] = newM2;//FAL
            }
            else
            {
                markers[MarkerNames.leftOuterKnee] = newM1;//FLE
                markers[MarkerNames.leftOuterAnkle] = newM2;//FAL
            }
        }
        private void NameSet(List<LabeledMarker> llm)
        {
            var quary = llm.FirstOrDefault(z => MarkerNames.bodyBaseAKA.Contains(z.label));
            MarkerNames.bodyBase = (quary == null) ? MarkerNames.bodyBase : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftHipAKA.Contains(z.label));
            MarkerNames.leftHip = (quary == null) ? MarkerNames.leftHip : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightHipAKA.Contains(z.label));
            MarkerNames.rightHip = (quary == null) ? MarkerNames.rightHip : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.spineAKA.Contains(z.label));
            MarkerNames.spine = (quary == null) ? MarkerNames.spine : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.neckAKA.Contains(z.label));
            MarkerNames.neck = (quary == null) ? MarkerNames.neck : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.chestAKA.Contains(z.label));
            MarkerNames.chest = (quary == null) ? MarkerNames.chest : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftShoulderAKA.Contains(z.label));
            MarkerNames.leftShoulder = (quary == null) ? MarkerNames.leftShoulder : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightShoulderAKA.Contains(z.label));
            MarkerNames.rightShoulder = (quary == null) ? MarkerNames.rightShoulder : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.headAKA.Contains(z.label));
            MarkerNames.head = (quary == null) ? MarkerNames.head : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftHeadAKA.Contains(z.label));
            MarkerNames.leftHead = (quary == null) ? MarkerNames.leftHead : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightHeadAKA.Contains(z.label));
            MarkerNames.rightHead = (quary == null) ? MarkerNames.rightHead : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftUpperKneeAKA.Contains(z.label));
            MarkerNames.leftUpperKnee = (quary == null) ? MarkerNames.leftUpperKnee : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightUpperKneeAKA.Contains(z.label));
            MarkerNames.rightUpperKnee = (quary == null) ? MarkerNames.rightUpperKnee : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftOuterKneeAKA.Contains(z.label));
            MarkerNames.leftOuterKnee = (quary == null) ? MarkerNames.leftOuterKnee : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightOuterKneeAKA.Contains(z.label));
            MarkerNames.rightOuterKnee = (quary == null) ? MarkerNames.rightOuterKnee : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftLowerKneeAKA.Contains(z.label));
            MarkerNames.leftLowerKnee = (quary == null) ? MarkerNames.leftLowerKnee : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightLowerKneeAKA.Contains(z.label));
            MarkerNames.rightLowerKnee = (quary == null) ? MarkerNames.rightLowerKnee : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftOuterAnkleAKA.Contains(z.label));
            MarkerNames.leftOuterAnkle = (quary == null) ? MarkerNames.leftOuterAnkle  : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightOuterAnkleAKA.Contains(z.label));
            MarkerNames.rightOuterAnkle = (quary == null) ? MarkerNames.rightOuterAnkle : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftHeelAKA.Contains(z.label));
            MarkerNames.leftHeel = (quary == null) ? MarkerNames.leftHeel : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightHeelAKA.Contains(z.label));
            MarkerNames.rightHeel = (quary == null) ? MarkerNames.rightHeel : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftToe2AKA.Contains(z.label));
            MarkerNames.leftToe2 = (quary == null) ? MarkerNames.leftToe2 : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightToe2AKA.Contains(z.label));
            MarkerNames.rightToe2 = (quary == null) ? MarkerNames.rightToe2 : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftElbowAKA.Contains(z.label));
            MarkerNames.leftElbow = (quary == null) ? MarkerNames.leftElbow : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightElbowAKA.Contains(z.label));
            MarkerNames.rightElbow = (quary == null) ? MarkerNames.rightElbow : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftInnerElbowAKA.Contains(z.label));
            MarkerNames.leftInnerElbow = (quary == null) ? MarkerNames.leftInnerElbow : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightInnerElbowAKA.Contains(z.label));
            MarkerNames.rightInnerElbow = (quary == null) ? MarkerNames.rightInnerElbow : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftOuterElbowAKA.Contains(z.label));
            MarkerNames.leftOuterElbow = (quary == null) ? MarkerNames.leftOuterElbow : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.rightOuterElbowAKA.Contains(z.label));
            MarkerNames.rightOuterElbow = (quary == null) ? MarkerNames.rightOuterElbow : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftWristAKA.Contains(z.label));
            MarkerNames.leftWrist = (quary == null) ? MarkerNames.leftWrist : quary.label; 

            quary = llm.FirstOrDefault(z => MarkerNames.rightWristAKA.Contains(z.label));
            MarkerNames.rightWrist = (quary == null) ? MarkerNames.rightWrist : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftWristRadiusAKA.Contains(z.label));
            MarkerNames.leftWristRadius = (quary == null) ? MarkerNames.leftWristRadius : quary.label; 

            quary = llm.FirstOrDefault(z => MarkerNames.rightWristRadiusAKA.Contains(z.label));
            MarkerNames.rightWristRadius = (quary == null) ? MarkerNames.rightWristRadius : quary.label;

            quary = llm.FirstOrDefault(z => MarkerNames.leftHandAKA.Contains(z.label));
            MarkerNames.leftHand = (quary == null) ? MarkerNames.leftHand : quary.label; 

            quary = llm.FirstOrDefault(z => MarkerNames.rightHandAKA.Contains(z.label));
            MarkerNames.rightHand = (quary == null) ? MarkerNames.rightHand : quary.label;

        }
    }
}
