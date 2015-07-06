using System.Collections.Generic;
using UnityEngine;
using QTM2Unity.Unity;
using System.Linq;
namespace QTM2Unity
{
    [System.Serializable]
    public class Markers
    {
        public bool showMarkers = false;
        public Color markersColor = Color.cyan;
        public float markersScale = 0.01f;
        public bool showMarkerBones = false;
        public Color markerBonesColor = Color.blue;
        public bool doRemoveMarkers = false;
        public string removeMarker;
    }
    public class RT : MonoBehaviour
    {
        public Vector3 offset = new Vector3(0, 0, 0);
        public Markers markers;
        protected RTClient rtClient;
        protected OpenTK.Vector3 pos;
        protected bool streaming = false;
        //protected List<LabeledMarker> markerData;
        protected Dictionary<string,OpenTK.Vector3> markerData;
        public virtual void StartNext(){}
        public virtual void UpdateNext(){}
        void Start()
        {
            rtClient = RTClient.getInstance();
            StartNext();
        }
        private bool _connected;
        private int _pickedServer;
        private short _udpPort;
        private int _streammode;
        private int _streamval;
        private bool _stream6d;
        private bool _stream3d;
        void Update()
        {

            if (rtClient == null)
            {
                rtClient = RTClient.getInstance();
                streaming = false;
            }
            if (rtClient.getStreamingStatus() && !streaming)
            {
                _pickedServer = rtClient._pickedServer;
                _udpPort = rtClient._udpPort;
                _streammode = rtClient._streammode;
                _streamval = rtClient._streamval;
                _stream6d = rtClient._stream6d;
                _stream3d = rtClient._stream3d;
                _connected = true;

                streaming = true;
            }
            if (streaming)
            {
                //var list = rtClient.Markers.ToList();
                //markerData = list.ConvertAll(kvp => new LabeledMarker(kvp.Key, kvp.Value));
                markerData = rtClient.Markers;
                //foreach (var v in markerData)
                //{
                //    UnityEngine.Debug.Log(v.label + " " + v.position );
                //}
                if (markerData == null && markerData.Count == 0) return;
                pos = (this.transform.position + offset).Convert();
                UpdateNext();


            }
            else if (_connected)
            {
                _connected = rtClient.connect(_pickedServer, _udpPort, _streammode, _streamval, _stream6d, _stream3d);
            }
        }
        void OnDrawGizmos()
        {
            if (Application.isPlaying && streaming && markerData != null)
            {
                Draw();
            }
        }
        public virtual void Draw()
        {
            Gizmos.color = markers.markersColor;
            if (markers.showMarkers)
            {
                var items = markerData.Values.ToList();
                foreach (var lb in items)
                {
                    //Gizmos.DrawSphere((lb.position + pos).Convert(), markers.markersScale);
                    Gizmos.DrawSphere((lb + pos).Convert(), markers.markersScale);
                }
            }

            if (markers.showMarkerBones && rtClient.Bones != null)
            {
                foreach (var lb in rtClient.Bones)
	            {
                    //OpenTK.Vector3 from = markerData.Find(g => g.label == lb.from).position;
                    //OpenTK.Vector3 to = markerData.Find(g => g.label == lb.to).position;
                    OpenTK.Vector3 from = markerData[lb.from] + pos;
                    OpenTK.Vector3 to = markerData[lb.to] + pos;
                    Debug.DrawLine(from.Convert(),
                                    to.Convert(), markers.markerBonesColor);
                }
	        }
        }
    }
}


