using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace QTM2Unity.Unity
{
    class PseudoJointLocalization
    {
        private Dictionary<string, Vector3> markers;
        public Dictionary<string, Vector3> joints;
        List<MarkersForJoints> markersForJoints = new List<MarkersForJoints>
            ()
                {
                    new MarkersForJoints("Hip", "L_IAS",  "R_IAS", "SACR"),
                   // new MarkersForJoints("Spine", "TV12"),
                    new MarkersForJoints("Neck", "SME", "TV2"),
                    new MarkersForJoints("Head", "SGL", "L_HEAD", "R_HEAD"),

                    new MarkersForJoints("LeftHip", "L_IAS"),
                    new MarkersForJoints("LeftKnee", "L_PAS", "L_TTC"),
                    new MarkersForJoints("LeftAnkle", "L_FAL", "L_FCC"),
                    new MarkersForJoints("LeftToes", "L_FM2"),

                    new MarkersForJoints("RightHip", "R_IAS"),
                    new MarkersForJoints("RightKnee", "R_PAS", "R_TTC"),
                    new MarkersForJoints("RightAnkle", "R_FAL", "R_FCC"),
                    new MarkersForJoints("RightToes", "R_FM2"),

                    new MarkersForJoints("LeftShoulder", "L_SAE"),
                    new MarkersForJoints("LeftElbow", "L_UOA", "L_HLE", "L_HME"),
                    new MarkersForJoints("LeftWrist", "L_RSP", "L_USP"),
                    new MarkersForJoints("LeftHand", "L_HM2"),

                    new MarkersForJoints("RightShoulder", "R_SAE"),
                    new MarkersForJoints("RightElbow", "R_UOA", "R_HLE", "R_HME"),
                    new MarkersForJoints("RightWrist", "R_RSP", "R_USP"),
                    new MarkersForJoints("RightHand", "R_HM2")
            };

        public PseudoJointLocalization(List<LabeledMarker> markerData)
        {
            markers = markerData.ToDictionary(k => k.label, v => v.position);
            joints = markersForJoints.ToDictionary(k => k.Name, v => getPoint(v));
        }

        protected Vector3 getPoint(MarkersForJoints group)
        {
            Vector3 value = Vector3.Zero;
            switch (group.Count)
            {
                case 1:
                    if (markers.ContainsKey(group.Forward))
                    {
                        value = markers[group.Forward];
                    }
                    break;
                case 2:
                    if (markers.ContainsKey(group.Left) ) {
                        if (markers.ContainsKey(group.Right))
                        {
                            value = getMid(
                            markers[group.Left],
                            markers[group.Right]);
                        }
                        else
                        {
                            value = markers[group.Left];
                        }
                    }
                    break;
                case 3:
                    if (markers.ContainsKey(group.Forward) &&
                        markers.ContainsKey(group.Left) &&
                        markers.ContainsKey(group.Right))
                    {
                        value = getMid(
                                    markers[group.Forward],
                                    markers[group.Left],
                                    markers[group.Right]);
                    }
                    else if (markers.ContainsKey(group.Left) &&
                             markers.ContainsKey(group.Right))
                    {
                        value = getMid(
                            markers[group.Left],
                            markers[group.Right]);
                    }
                    break;
                default: break;
            }
            return value;
        }
        protected Vector3 getMid(Vector3 leftVect, Vector3 rightVect)
        {
            return (leftVect - rightVect) * 0.5f + rightVect;
        }

        protected Vector3 getMid(Vector3 forwardVect, Vector3 leftVect, Vector3 rightVect) {
            Vector3 backMid = getMid(leftVect, rightVect);
            return forwardVect + (backMid - forwardVect) * 2 / 3;
        }

    }

    public class MarkersForJoints 
    {
        private readonly string jointName;
        private readonly string forwardMarker;
        private readonly string leftMarker;
        private readonly string rightMarker;
        private readonly int numberOfVectors;
        public MarkersForJoints(string name, string middle) {
            this.numberOfVectors = 1;
            forwardMarker = middle;
            this.jointName = name;
        }
        public MarkersForJoints(string name, string left, string right) {
            this.jointName = name;
            this.leftMarker = left;
            this.rightMarker = right;
            this.numberOfVectors = 2;

        }
        public MarkersForJoints(string name, string left, string right, string forward) {
            this.numberOfVectors = 3;
            this.leftMarker = left;
            this.rightMarker = right;
            this.forwardMarker = forward;
            this.jointName = name;

        }
        public int Count {
            get { return numberOfVectors;}
        }
        public string Forward {
            get { return forwardMarker;}
        }
        public string Left {
            get { return leftMarker;}
        }
        public string Right {
            get { return rightMarker;}
        }
        public string Name {
            get { return jointName;}
        }
    }
}
