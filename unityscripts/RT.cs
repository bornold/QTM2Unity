using System.Collections.Generic;
using UnityEngine;
using QTM2Unity.Unity;
namespace QTM2Unity
{
    public abstract class RT : MonoBehaviour
    {
        public RTClient rtClient;
        public float traceScale = 0.15f;
        protected bool streaming = false;
        protected List<LabeledMarker> markerData;

        // Use this for initialization
        public abstract void UpdateNext();
        public abstract void StartNext();
        void Start()
        {
            rtClient = RTClient.getInstance();
            StartNext();
        }

        // Update is called once per frame
        void Update()
        {
            if (rtClient == null)
            {
                rtClient = RTClient.getInstance();
                streaming = false;
            }
            if (rtClient.getStreamingStatus() && !streaming)
            {
                streaming = true;
            }
            markerData = rtClient.Markers;
            if (markerData == null && markerData.Count == 0) return;

            UpdateNext();
        }

        protected void drawRays(OpenTK.Quaternion rot, Vector3 pos)
        {
            pos += this.transform.position;
            OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot);
            OpenTK.Vector3 up = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, rot);
            OpenTK.Vector3 forward = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, rot);
            Debug.DrawRay(pos, cv(up) * traceScale, Color.green);
            Debug.DrawRay(pos, cv(right) * traceScale, Color.red);
            Debug.DrawRay(pos, cv(forward) * traceScale, Color.blue);

        }
        protected void drawLine(OpenTK.Vector3 start, OpenTK.Vector3 end)
        {
            Debug.DrawLine(cv(start) + this.transform.position, cv(end) + this.transform.position, Color.white);
        }

        public Vector3 cv(OpenTK.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        public Quaternion cq(OpenTK.Quaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
    }
}


