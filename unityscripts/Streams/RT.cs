using System.Collections.Generic;
using UnityEngine;
using QTM2Unity.Unity;
namespace QTM2Unity
{
    public class RT : MonoBehaviour
    {
        public Vector3 offset = new Vector3(0, 0, 0);
        public Color gizmosColor = Color.cyan;
        public bool debug = false;
        public bool drawMarkers = false;
        public bool drawMarkerBones = false;
        public Color markerBonesColor = Color.blue;
        public float markersScale = 0.01f;
        protected RTClient rtClient;
        protected OpenTK.Vector3 pos;
        protected bool streaming = false;
        protected List<LabeledMarker> markerData;
        public virtual void StartNext(){}
        public virtual void UpdateNext(){}
        void Start()
        {
            rtClient = RTClient.getInstance();
            StartNext();
        }
        bool _connected;
        int _pickedServer;
        short _udpPort;
        int _streammode;
        int _streamval;
        bool _stream6d;
        bool _stream3d;
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
                markerData = rtClient.Markers;
                if (markerData == null && markerData.Count == 0) return;
                pos = this.transform.position.Convert();
                pos += offset.Convert();
                UpdateNext();
            }
            else if (_connected)
                _connected = rtClient.connect(_pickedServer, _udpPort, _streammode, _streamval, _stream6d, _stream3d);
        }
        public virtual void Draw()
        {
            Gizmos.color = gizmosColor;
            if (markerData != null)
            {
                if (drawMarkers)
                {
                    foreach (var lb in markerData)
                    {
                        Gizmos.DrawSphere((lb.position + pos ).Convert(), markersScale);
                    }
                }
                if (drawMarkerBones && rtClient.Bones != null)
                {
				    var boneData = rtClient.Bones;
				    foreach( var bd in boneData)
	                {
					    Debug.DrawLine(bd.fromMarker.position.Convert() + pos.Convert(),
                                        bd.toMarker.position.Convert() + pos.Convert(), markerBonesColor);
	                }
                }
            }
        }
        void OnDrawGizmos()
        {
            Draw();
        }
    }
}


