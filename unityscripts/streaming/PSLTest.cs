using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OTK = OpenTK;

namespace QTM2Unity.Unity
{
    public class PSLTest : MonoBehaviour
    {
        private List<LabeledMarker> markerData;
        private RTClient rtClient;
        private Dictionary<string, Vector3> joints = new Dictionary<string, Vector3>();

        public bool debug = false;
        public float markerScale = 0.015f;
        private bool streaming = false;

        // Use this for initialization
        void Start()
        {
            rtClient = RTClient.getInstance();
        }

        // Update is called once per frame
        void Update()
        {
            if ( rtClient == null)
            {
                Start();
                //rtClient = RTClient.getInstance();
                streaming = false;
            }
            if (rtClient.getStreamingStatus() && !streaming)
            {
                streaming = true;
            }

            markerData = rtClient.Markers;

            if (markerData == null && markerData.Count == 0) return;

            joints = new PseudoJointLocalization(markerData).joints.ToDictionary(x => x.Key, x => convertFromSlimDXVector(x.Value));
            
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            Gizmos.color = Color.cyan;

            // Hip to Left toes
            if (joints.ContainsKey("Hip") && joints.ContainsKey("LeftHip"))
                Gizmos.DrawLine(joints["Hip"], joints["LeftHip"]);

            if (joints.ContainsKey("LeftHip") && joints.ContainsKey("LeftKnee"))
                Gizmos.DrawLine(joints["LeftHip"], joints["LeftKnee"]);

            if (joints.ContainsKey("LeftKnee") && joints.ContainsKey("LeftAnkle"))
                Gizmos.DrawLine(joints["LeftKnee"], joints["LeftAnkle"]);

            if (joints.ContainsKey("LeftAnkle") && joints.ContainsKey("LeftToes"))
                Gizmos.DrawLine(joints["LeftAnkle"], joints["LeftToes"]);

            // Hip to Right toes
            if (joints.ContainsKey("Hip") && joints.ContainsKey("RightHip"))
                Gizmos.DrawLine(joints["Hip"], joints["RightHip"]);

            if (joints.ContainsKey("RightHip") && joints.ContainsKey("RightKnee"))
                Gizmos.DrawLine(joints["RightHip"], joints["RightKnee"]);

            if (joints.ContainsKey("RightKnee") && joints.ContainsKey("RightAnkle"))
                Gizmos.DrawLine(joints["RightKnee"], joints["RightAnkle"]);

            if (joints.ContainsKey("RightAnkle") && joints.ContainsKey("RightToes"))
                Gizmos.DrawLine(joints["RightAnkle"], joints["RightToes"]);


            // Hip to head
            if (joints.ContainsKey("Hip") ) {
                if (joints.ContainsKey("Spine")) {
                    Gizmos.DrawLine(joints["Hip"], joints["Spine"]);
                }
                if (joints.ContainsKey("Neck")) {
                    Gizmos.DrawLine(joints["Hip"], joints["Neck"]);
                }
            }
            
            if (joints.ContainsKey("Spine") && joints.ContainsKey("Neck")) {
                    Gizmos.DrawLine(joints["Spine"], joints["Neck"]);
            }

            if (joints.ContainsKey("Neck") && joints.ContainsKey("Head"))
                Gizmos.DrawLine(joints["Neck"], joints["Head"]);


            // Neck to Right Hand
            if (joints.ContainsKey("Neck") && joints.ContainsKey("RightShoulder"))
                Gizmos.DrawLine(joints["Neck"], joints["RightShoulder"]);

            if (joints.ContainsKey("RightShoulder") && joints.ContainsKey("RightElbow"))
                Gizmos.DrawLine(joints["RightShoulder"], joints["RightElbow"]);

            if (joints.ContainsKey("RightElbow") && joints.ContainsKey("RightWrist"))
                Gizmos.DrawLine(joints["RightElbow"], joints["RightWrist"]);

            if (joints.ContainsKey("RightWrist") && joints.ContainsKey("RightHand"))
                Gizmos.DrawLine(joints["RightWrist"], joints["RightHand"]);


            // Neck to Left hand
            if (joints.ContainsKey("Neck") && joints.ContainsKey("LeftShoulder"))
                Gizmos.DrawLine(joints["Neck"], joints["LeftShoulder"]);

            if (joints.ContainsKey("LeftShoulder") && joints.ContainsKey("LeftElbow"))
                Gizmos.DrawLine(joints["LeftShoulder"], joints["LeftElbow"]);

            if (joints.ContainsKey("LeftElbow") && joints.ContainsKey("LeftWrist"))
                Gizmos.DrawLine(joints["LeftElbow"], joints["LeftWrist"]);

            if (joints.ContainsKey("LeftWrist") && joints.ContainsKey("LeftHand"))
                Gizmos.DrawLine(joints["LeftWrist"], joints["LeftHand"]);

    
            foreach (KeyValuePair<string,Vector3> kvp in joints)
            {
                Gizmos.DrawSphere(kvp.Value, markerScale);   
            }

        }
        // TODO write a converter https://msdn.microsoft.com/en-us/library/ayybcxe5.aspx
        private Vector3 convertFromSlimDXVector(OTK.Vector3 v) {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}
