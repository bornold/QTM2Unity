using System.Collections.Generic;
using UnityEngine;
using QTM2Unity.Unity;
namespace QTM2Unity
{
    public abstract class RT : MonoBehaviour
    {
        public bool debug = false;
        protected RTClient rtClient;
        protected OpenTK.Vector3 pos;
        protected bool streaming = false;
        protected List<LabeledMarker> markerData;
        public abstract void StartNext();
        public abstract void UpdateNext();
        void Start()
        {
            rtClient = RTClient.getInstance();
            StartNext();
        }

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
            pos = this.transform.position.Convert();
            UpdateNext();
        }
    }
}


