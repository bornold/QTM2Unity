using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QTM2Unity.Unity
{
    public class Skelett : MonoBehaviour
    {
        private List<LabeledMarker> markerData;
        private RTClient rtClient;
        private Dictionary<string, Vector3> markers = new Dictionary<string, Vector3>();
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


            markers = markerData.ToDictionary(v => v.label, v => new Vector3(v.position.X,v.position.Y,v.position.Z));
            joints.Clear();
            //joints = new PseudoJointLocalization(markerData).joints.ToDictionary(x => x.Key, x => convertFromSlimDXVector(x.Value));
            
            // HIPS
//            string[] hips = {"SACR","L_IAS","R_IAS"};
            if (markers.ContainsKey("SACR") && markers.ContainsKey("L_IAS") && markers.ContainsKey("R_IAS"))
            {
                //Matrix4x4 hip = 
                getHipOrientation(markers["SACR"], markers["L_IAS"], markers["R_IAS"]);
                joints.Add( "Hips",
                    getMid(markers["SACR"], markers["L_IAS"], markers["R_IAS"]));
            }


            //HEAD
//            string[] heads = { "SGL", "L_HEAD", "R_HEAD" };
            if (markers.ContainsKey("SGL") && markers.ContainsKey("L_HEAD") && markers.ContainsKey("R_HEAD"))
            {
                //Matrix4x4 head = 
                getOrientation(markers["SGL"], markers["L_HEAD"], markers["R_HEAD"]);
                joints.Add("HeadTop_End", getMid(markers["SGL"], markers["L_HEAD"], markers["R_HEAD"]));
            }

            //WRIST
            if (markers.ContainsKey("R_RSP") && markers.ContainsKey("R_USP"))
            {
                joints.Add("RightHand", getMid(markers["R_RSP"], markers["R_USP"]));
            }
            if (markers.ContainsKey("L_RSP") && markers.ContainsKey("L_USP"))
            {
                joints.Add("LeftHand", getMid(markers["L_RSP"], markers["L_USP"]));
            }


            //HANDs
            if (markers.ContainsKey("R_HM2"))
            {
                joints.Add("RightHandRing1", markers["R_HM2"]);
            }
 

            if (markers.ContainsKey("L_HM2"))
            {
                joints.Add("LeftHandRing1", markers["L_HM2"]);
            }


            // ELBOW
            //RIGHT
            if (markers.ContainsKey("R_UOA") && markers.ContainsKey("R_HLE") && markers.ContainsKey("R_HME"))
            {
                joints.Add("RightForeArm", getMid(markers["R_UOA"],markers["R_HLE"],markers["R_HME"]));
            }

            //LEFT
            if (markers.ContainsKey("L_UOA") && markers.ContainsKey("L_HLE") && markers.ContainsKey("L_HME"))
            {
                joints.Add("LeftForeArm", getMid(markers["L_UOA"],markers["L_HLE"],markers["L_HME"]));
            }



            // Shoulders
            //Right
            if (markers.ContainsKey("R_SAE"))
            {
                joints.Add("RightArm", markers["R_SAE"]);
            }
            //LEFT
            if (markers.ContainsKey("L_SAE"))
            {
                joints.Add("LeftArm", markers["L_SAE"]);
            }



            // NECK
            if (markers.ContainsKey("SME"))
            {
                if (markers.ContainsKey("TV2"))
                {
                    joints.Add("Neck", getMid(markers["SME"], markers["TV2"]));
                }
                else
                {
                    joints.Add("Neck", markers["SME"]);
                }
            }


            //SPINE
            if (markers.ContainsKey("TV12"))
            {
                joints.Add("Spine", markers["TV12"]);
            }

            // HIP
            // RIGHT
            if ( markers.ContainsKey("R_IAS") ) 
            {
                joints.Add("RightUpLeg", markers["R_IAS"]);
            }
            //LEFT
            if ( markers.ContainsKey("L_IAS") ) 
            {
                joints.Add("LeftUpLeg", markers["L_IAS"]);
            }


            // KNEE
            //RIGHT
            if ( markers.ContainsKey("R_PAS") && markers.ContainsKey("R_TTC")) 
            {
                joints.Add("RightLeg", getMid(markers["R_PAS"], markers["R_TTC"]));
            }

            if ( markers.ContainsKey("L_PAS") && markers.ContainsKey("L_TTC")) 
            {
                joints.Add("LeftLeg", getMid(markers["L_PAS"], markers["L_TTC"]));
            }


            // FOOT
            // RIGHT
            if (markers.ContainsKey("R_FAL") && markers.ContainsKey("R_FCC")) 
            {
                joints.Add("RightFoot", getMid(markers["R_FAL"],markers["R_FCC"]));
            }
            //LEFT
            if (markers.ContainsKey("L_FAL") && markers.ContainsKey("L_FCC"))
            {
                joints.Add("LeftFoot", getMid(markers["L_FAL"], markers["L_FCC"]));
            }

            // TOES
            //RIGHT
            if (markers.ContainsKey("R_FM2"))
            {
                joints.Add("RightToeBase", markers["R_FM2"]);
            }
            //LEFT
            if (markers.ContainsKey("L_FM2"))
            {
                joints.Add("LeftToeBase", markers["L_FM2"]);
            }
            
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            Gizmos.color = Color.cyan;

            if (joints.ContainsKey("Hips") && joints.ContainsKey("LeftUpLeg"))
                Gizmos.DrawLine(joints["Hips"], joints["LeftUpLeg"]);

            if (joints.ContainsKey("LeftUpLeg") && joints.ContainsKey("LeftLeg"))
                Gizmos.DrawLine(joints["LeftUpLeg"], joints["LeftLeg"]);

            if (joints.ContainsKey("LeftLeg") && joints.ContainsKey("LeftFoot"))
                Gizmos.DrawLine(joints["LeftLeg"], joints["LeftFoot"]);

            if (joints.ContainsKey("LeftFoot") && joints.ContainsKey("LeftToeBase"))
                Gizmos.DrawLine(joints["LeftFoot"], joints["LeftToeBase"]);



            if (joints.ContainsKey("Hips") && joints.ContainsKey("RightUpLeg"))
                Gizmos.DrawLine(joints["Hips"], joints["RightUpLeg"]);

            if (joints.ContainsKey("RightUpLeg") && joints.ContainsKey("RightLeg"))
                Gizmos.DrawLine(joints["RightUpLeg"], joints["RightLeg"]);

            if (joints.ContainsKey("RightLeg") && joints.ContainsKey("RightFoot"))
                Gizmos.DrawLine(joints["RightLeg"], joints["RightFoot"]);

            if (joints.ContainsKey("RightFoot") && joints.ContainsKey("RightToeBase"))
                Gizmos.DrawLine(joints["RightFoot"], joints["RightToeBase"]);



            if ( joints.ContainsKey("Hips") && joints.ContainsKey("Spine") )
            {
                Gizmos.DrawLine(joints["Hips"], joints["Spine"]);
                if (joints.ContainsKey("Neck"))
                    Gizmos.DrawLine(joints["Spine"], joints["Neck"]);
            }
            else if (joints.ContainsKey("Hips") && joints.ContainsKey("Neck"))
                Gizmos.DrawLine(joints["Hips"], joints["Neck"]);
            


            if (joints.ContainsKey("Neck") && joints.ContainsKey("HeadTop_End"))
                Gizmos.DrawLine(joints["Neck"], joints["HeadTop_End"]);

            if (joints.ContainsKey("Neck") && joints.ContainsKey("RightArm"))
                Gizmos.DrawLine(joints["Neck"], joints["RightArm"]);

            if (joints.ContainsKey("RightArm") && joints.ContainsKey("RightForeArm"))
                Gizmos.DrawLine(joints["RightArm"], joints["RightForeArm"]);

            if (joints.ContainsKey("RightForeArm") && joints.ContainsKey("RightHand"))
                Gizmos.DrawLine(joints["RightForeArm"], joints["RightHand"]);

            if (joints.ContainsKey("RightHand") && joints.ContainsKey("RightHandRing1"))
                Gizmos.DrawLine(joints["RightHand"], joints["RightHandRing1"]);



            if (joints.ContainsKey("Neck") && joints.ContainsKey("LeftArm"))
                Gizmos.DrawLine(joints["Neck"], joints["LeftArm"]);

            if (joints.ContainsKey("LeftArm") && joints.ContainsKey("LeftForeArm"))
                Gizmos.DrawLine(joints["LeftArm"], joints["LeftForeArm"]);

            if (joints.ContainsKey("LeftForeArm") && joints.ContainsKey("LeftHand"))
                Gizmos.DrawLine(joints["LeftForeArm"], joints["LeftHand"]);

            if (joints.ContainsKey("LeftHand") && joints.ContainsKey("LeftHandRing1"))
                Gizmos.DrawLine(joints["LeftHand"], joints["LeftHandRing1"]);

    
            foreach (KeyValuePair<string,Vector3> kvp in joints)
            {
                Gizmos.DrawSphere(kvp.Value, markerScale);   
            }

        }


        /// <summary>
        /// Get hip orientation 
        /// </summary>
        /// <param name="sacrum">sacrum marker</param>
        /// <param name="leftHip">left hip marker</param>
        /// <param name="rightHip">right hip marker</param>
        /// <returns></returns>
        Matrix4x4 getHipOrientation(Vector3 sacrum, Vector3 leftHip, Vector3 rightHip)
        {
            Vector3 hipMarkerMid = getMid(leftHip, rightHip);
            Vector3 hipMid = sacrum   + (hipMarkerMid - sacrum) * 2 / 3;

            Vector3 right =  rightHip - hipMarkerMid;
            Vector3 front =  hipMid   - sacrum;
            
            front.Normalize();
            right.Normalize();

            Vector3 up = Vector3.Cross(front, right);

            Matrix4x4 hip = getMatrix(hipMid, front, up, right);

            if (debug)
            {
                Debug.DrawRay(hipMid, up    * 0.5f, Color.green);
                Debug.DrawRay(hipMid, right * 0.5f, Color.red);
                Debug.DrawRay(hipMid, front * 0.5f, Color.blue);
            }
            return hip;
        }

        /// <summary>
        /// Create a matrix for a coordinate system based off of three game objects
        /// </summary>
        /// <param name="forwardVect">gameobject in forward direction</param>
        /// <param name="leftVect">gameobject in left direction</param>
        /// <param name="rightVect">gameobject in right direction</param>
        /// <returns>matrix coordinate system based on gameobjects</returns>
        protected Matrix4x4 getOrientation(Vector3 forwardVect, Vector3 leftVect, Vector3 rightVect)
        {
            Vector3 backMid = getMid(leftVect, rightVect);
            Vector3 mid = forwardVect + (backMid - forwardVect) * 2 / 3;
            
            Vector3 front = mid - backMid;
            Vector3 right = rightVect - backMid;

            front.Normalize();
            right.Normalize();

            Vector3 up = Vector3.Cross(front, right);

            Matrix4x4 mat = getMatrix(mid, front, up, right);

            if (debug)
            {
                Debug.DrawRay(mid, up    * 0.5f, Color.green);
                Debug.DrawRay(mid, right * 0.5f, Color.red);
                Debug.DrawRay(mid, front * 0.5f, Color.blue);
            }

            return mat;
        }
        /// <summary>
        /// Create matrix from position and three vectors
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="front">forward vector</param>
        /// <param name="up">up vector</param>
        /// <param name="right">right vector</param>
        /// <returns>matrix with cooridnate system based on vectors</returns>
        protected Matrix4x4 getMatrix(Vector3 position, Vector3 front, Vector3 up, Vector3 right)
        {
            Matrix4x4 mat = Matrix4x4.identity;

            mat.SetTRS(position, Quaternion.identity, Vector3.one);

            Vector4 up4v = new Vector4(up.x, up.y, up.z, 0);
            Vector4 front4v = new Vector4(front.x, front.y, front.z, 0);
            Vector4 right4v = new Vector4(right.x, right.y, right.z, 0);

            mat.SetColumn(0, right4v);
            mat.SetColumn(1, up4v);
            mat.SetColumn(2, front4v);
            return mat;

        }

        protected Vector3 getMid(Vector3 leftVect, Vector3 rightVect)
        {
            return (leftVect - rightVect) * 0.5f + rightVect;
        }

        protected Vector3 getMid(Vector3 forwardVect, Vector3 leftVect, Vector3 rightVect) {
            Vector3 backMid = getMid(leftVect, rightVect);
            return getCenter(forwardVect, backMid);
        }
        protected Vector3 getCenter(Vector3 forwardVect, Vector3 backMid)
        {
            return forwardVect + (backMid - forwardVect) * 2 / 3;
        }
    }
}
