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
                UpdateNext();
            }
            else if (_connected)
                _connected = rtClient.connect(_pickedServer, _udpPort, _streammode, _streamval, _stream6d, _stream3d);
        }
    }
}


