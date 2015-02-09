using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using QTM2Unity.Unity;
using QTM2Unity.JCL;
using QTM2Unity.SkeletonModel;
namespace QTM2Unity
{
    class JCTest : MonoBehaviour {
        private RTClient rtClient;
        public bool debug = false;
        public float markerScale = 0.015f;
        public float traceScale = 0.15f;
        private bool streaming = false;
        private JointLocalization joints;
        private BipedSkeleton skeleton;
        // Use this for initialization
        void Start()
        {
            rtClient = RTClient.getInstance();
            joints = new JointLocalization();
        }

        // Update is called once per frame
        void Update()
        {
            if (rtClient == null)
            {
                rtClient = RTClient.getInstance();
                joints = new JointLocalization();
                streaming = false;
            }
            if (rtClient.getStreamingStatus() && !streaming)
            {
                streaming = true;
            }
            List<LabeledMarker> markerData = rtClient.Markers;
            if (markerData == null && markerData.Count == 0) return;
            if (joints == null) joints = new JointLocalization();
            skeleton = joints.getJointLocazion(markerData);
        }
        void OnDrawGizmos()
        {
            foreach (Bone b in skeleton.Bones)
            {
                Vector3 v = OpenTKV2UEV(b.Pos);
                Gizmos.DrawSphere(v, markerScale);
                if (debug )
                    drawRays(b.Orientation,v);
                if (b.Children != null)
                {
                    foreach (Bone b2 in b.Children)
                    {
                        drawLine(b.Pos, b2.Pos);
                    }
                }
            }
        }
        private void drawRays(OpenTK.Quaternion rot, Vector3 pos)
        {
            OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot);
            OpenTK.Vector3 up = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, rot);
            OpenTK.Vector3 forward = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, rot);
            Debug.DrawRay(pos, OpenTKV2UEV(up) * traceScale, Color.green);
            Debug.DrawRay(pos, OpenTKV2UEV(right) * traceScale, Color.red);
            Debug.DrawRay(pos, OpenTKV2UEV(forward) * traceScale, Color.blue);
            
        }
        private void drawLine(OpenTK.Vector3 start, OpenTK.Vector3 end)
        {
            Debug.DrawLine(OpenTKV2UEV(start), OpenTKV2UEV(end),Color.white);
        }

        // TODO write a converter https://msdn.microsoft.com/en-us/library/ayybcxe5.aspx
        private Vector3 OpenTKV2UEV(OpenTK.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }


        }
    }


