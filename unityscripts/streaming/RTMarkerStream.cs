using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OTK = OpenTK;
namespace QTM2Unity.Unity
{
	public class RTMarkerStream : MonoBehaviour
	{
		private List<LabeledMarker> markerData;
        private RTClient rtClient;
		private GameObject markerRoot;
		private List<GameObject> markers;

		public bool visibleMarkers;

		public float markerScale = 0.01f;

        private bool streaming = false;

		// Use this for initialization
		void Start ()
		{
            rtClient = RTClient.getInstance();
			markers = new List<GameObject>();
			markerRoot = this.gameObject;
        }


        private void initiateMarkers()
        {
            markers.Clear();
            markerData = rtClient.Markers;

            for (int i = 0; i < markerData.Count; i++)
            {
                GameObject newMarker = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Sphere);
                newMarker.name = markerData[i].label;
                newMarker.transform.parent = markerRoot.transform;
                newMarker.transform.localScale = Vector3.one * markerScale;
                newMarker.SetActive(false);
                markers.Add(newMarker);
            }
        }

		// Update is called once per frame
		void Update ()
		{

            if (rtClient == null)
            {
                rtClient = RTClient.getInstance();
            }
            if(rtClient.getStreamingStatus() && !streaming)
            {
                initiateMarkers();
                streaming = true;
            }


            if (rtClient.getStreamingStatus() && !streaming)
            {
                streaming = true;
            }

			markerData = rtClient.Markers;
			
            if (markerData == null && markerData.Count == 0)
                return;

			if (markers.Count != markerData.Count)
			{
				initiateMarkers();
			}

			for (int i = 0; i < markerData.Count; i++)
			{
				if(markerData[i].position.Length > 0)
				{
					markers[i].name = markerData[i].label;
					//markers[i].renderer.material.color = markerData[i].color;
                    markers[i].transform.localPosition = convertFromSlimDXVector(markerData[i].position);
					markers[i].SetActive(true);
					markers[i].renderer.enabled = visibleMarkers;
                    markers[i].transform.localScale = Vector3.one * markerScale;
                }
                else
                {
                    //hide markers if we cant find them.
                    markers[i].SetActive(false);
                }
			}
		}
        // TODO write a converter https://msdn.microsoft.com/en-us/library/ayybcxe5.aspx
        private Vector3 convertFromSlimDXVector(OTK.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
	}
}