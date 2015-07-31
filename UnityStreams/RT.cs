using System.Collections.Generic;
using UnityEngine;
using QTM2Unity;
using System.Linq;
using QualisysRealTime.Unity;


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
            [Range(0.001f, 0.05f)]
            public float scale = 0.01f;
            public bool bones = false;
            public Color boneColor = Color.blue;
        }
    }
    public class RT : MonoBehaviour
    {
        public bool debugFlag = false;

        public Debugging debug;
        protected RTClient rtClient;
        protected Vector3 pos;
        protected bool streaming = false;
        //protected bool fileStreaming = false;
        //protected bool reset = false;
        protected List<LabeledMarker> markerData;
        protected List<string> markersLabels;
        public virtual void StartNext(){}
        public virtual void UpdateNext(){}
        void Start()
        {
            rtClient = RTClient.GetInstance();
            StartNext();
        }
        void Update()
        {
            if (rtClient == null)
            {
                rtClient = RTClient.GetInstance();
            }
            streaming = rtClient.GetStreamingStatus();
            if (streaming)// && fileStreaming)
            {
                markerData = rtClient.Markers;
                if (markerData == null || markerData.Count == 0)
                {
                    return;
                }
            }
            if ((streaming) || debugFlag)// && fileStreaming) || debugFlag)
            {
                pos = this.transform.position + debug.offset;
                UpdateNext();
            }

            //reset = streaming;// && !fileStreaming;
        }
        void OnDrawGizmos()
        {
            ShouldWeDraw();
        }
        protected void ShouldWeDraw()
        {
            if (Application.isPlaying && (streaming && markerData != null && markerData.Count > 0) || debugFlag)
            {
                Draw();
            }
        }
        public virtual void Draw()
        {
            if (debug == null) return;
            if (debug.markers.markers)
            {
                foreach (var lb in markerData)
                {
                    Gizmos.color = new Color(lb.Color.r, lb.Color.g, lb.Color.b);
                    Gizmos.DrawSphere(lb.Position + pos, debug.markers.scale);
                }
            }

            if (debug.markers.bones && rtClient.Bones != null)
            {
                foreach (var lb in rtClient.Bones)
	            {
                    var from = markerData.Find(md => md.Label == lb.From).Position + pos;
                    var to = markerData.Find(md => md.Label == lb.To).Position + pos;
                    Debug.DrawLine(from, to, debug.markers.boneColor);
                }
	        }
        }
    }
}


