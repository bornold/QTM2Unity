using System.Collections.Generic;
using UnityEngine;
using QTM2Unity.Unity;
using System.Linq;
namespace QTM2Unity
{

    [System.Serializable]
    public class Debugging
    {
        public Vector3 offset = new Vector3(0, 0, 0);
        public Markers markers;
        [System.Serializable]
        public class Markers 
        {
            public bool markers = false;
            [Range(0.001f, 0.1f)]
            public float scale = 0.01f;
            public Color markerColor = Color.cyan;
            public bool bones = false;
            public Color boneColor = Color.blue;
        }
    }
    public class RT : MonoBehaviour
    {
        public bool debugFlag = false;

        public Debugging debug;
        protected RTClient rtClient;
        protected OpenTK.Vector3 pos;
        protected bool streaming = false;
        protected List<LabeledMarker> markerData;
        protected List<string> markersLabels;
        public virtual void StartNext(){}
        public virtual void UpdateNext(){}
        void Start()
        {
            rtClient = RTClient.getInstance();
            StartNext();
        }
        void Update()
        {
            if (rtClient == null) rtClient = RTClient.getInstance();
            streaming = rtClient.getStreamingStatus();
            if (streaming)
            {
                markerData = rtClient.Markers;
                if (markerData == null || markerData.Count == 0 ) return;
            }
            if (streaming || debugFlag)
            {
                pos = (this.transform.position + debug.offset).Convert();
                UpdateNext();
            }
        }
        void OnDrawGizmos()
        {
            if (Application.isPlaying && streaming && markerData != null && markerData.Count > 0 )
            {
                Draw();
            }
        }
        public virtual void Draw()
        {
            if (debug == null) return;
            if (debug.markers.markers)
            {
                Gizmos.color = debug.markers.markerColor;

                //var items = markerData.Values.ToList();
                foreach (var lb in markerData)
                {
                    Gizmos.DrawSphere((lb.position + pos).Convert(), debug.markers.scale);
                }
            }

            if (debug.markers.bones && rtClient.Bones != null)
            {
                foreach (var lb in rtClient.Bones)
	            {
                    OpenTK.Vector3 from = markerData.Find(md => md.label == lb.from).position + pos;
                    OpenTK.Vector3 to = markerData.Find(md => md.label == lb.to).position + pos;
                    Debug.DrawLine(from.Convert(),
                                    to.Convert(), debug.markers.boneColor);
                }
	        }
        }
    }
}


