using System.Collections.Generic;
using UnityEngine;
using QTM2Unity.Unity;
using System.Linq;
namespace QTM2Unity
{

    [System.Serializable]
    public class Markers
    {
        public Vector3 offset = new Vector3(0, 0, 0);
        public bool showMarkers = false;
        public Color markersColor = Color.cyan;
        public float markersScale = 0.01f;
        public bool showMarkerBones = false;
        public Color markerBonesColor = Color.blue;
    }
    public class RT : MonoBehaviour
    {
        public bool debug = false;

        public Markers markers;
        protected RTClient rtClient;
        protected OpenTK.Vector3 pos;
        protected bool streaming = false;
        protected Dictionary<string,OpenTK.Vector3> markerData;
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
            if (streaming || debug)
            {
                pos = (this.transform.position + markers.offset).Convert();
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
            if (markers == null) return;
            if (markers.showMarkers)
            {
                Gizmos.color = markers.markersColor;

                //var items = markerData.Values.ToList();
                foreach (var lb in markerData.Values)
                {
                    Gizmos.DrawSphere((lb + pos).Convert(), markers.markersScale);
                }
            }

            if (markers.showMarkerBones && rtClient.Bones != null)
            {
                foreach (var lb in rtClient.Bones)
	            {
                    OpenTK.Vector3 from = markerData[lb.from] + pos;
                    OpenTK.Vector3 to = markerData[lb.to] + pos;
                    Debug.DrawLine(from.Convert(),
                                    to.Convert(), markers.markerBonesColor);
                }
	        }
        }
    }
}


