using System.Collections.Generic;
using UnityEngine;
using QTM2Unity.Unity;
namespace QTM2Unity
{
    public abstract class RT : MonoBehaviour
    {
        public RTClient rtClient;
        public float traceScale = 0.07f;
        public Color skelettColor = Color.white;
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
            UnityDebug.DrawRays(rot, pos);
        }
        protected void drawLine(OpenTK.Vector3 start, OpenTK.Vector3 end)
        {
            Debug.DrawLine(cv(start) + this.transform.position, cv(end) + this.transform.position, skelettColor);
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


