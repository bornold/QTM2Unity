using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace QTM2Unity.Unity
{
	public class RTMarkerStream : RT
	{
		private List<LabeledMarker> markerData;
		private GameObject markerRoot;
		private List<GameObject> markers;

		public bool visibleMarkers;
        public bool gizmo;
		public float markerScale = 0.01f;

        private bool streaming = false;

		// Use this for initialization
        public override void StartNext()
        {
			markers = new List<GameObject>();
			markerRoot = this.gameObject;
            initiateMarkers();
        }


        private void initiateMarkers()
        {
            markers.Clear();
            markerData = rtClient.Markers;
            foreach (Transform child in gameObject.transform)
            {
                Destroy(child.gameObject    );
            }
            for (int i = 0; i < markerData.Count; i++)
            {
                if (!GameObject.Find(markerData[i].label))
                {
                    GameObject newMarker = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    newMarker.name = markerData[i].label;
                    newMarker.transform.parent = markerRoot.transform;
                    newMarker.transform.localScale = Vector3.one * markerScale;
                    newMarker.SetActive(false);
                    markers.Add(newMarker);
                }
            }
        }

		// Update is called once per frame
        public override void UpdateNext()
		{
            markerData = rtClient.Markers;

            if (markerData == null && markerData.Count == 0)
                return;
              
            if (markers.Count != markerData.Count)
			{
                StartNext();
			}
            if (visibleMarkers)
            {
			    for (int i = 0; i < markerData.Count; i++)
			    {
				    if(markerData[i].position.Length > 0)
				    {
					    markers[i].name = markerData[i].label;
                        markers[i].transform.localPosition = cv(markerData[i].position);
					    markers[i].SetActive(true);
					    markers[i].GetComponent<Renderer>().enabled = visibleMarkers;
                        markers[i].transform.localScale = Vector3.one * markerScale;
                    }
                    else
                    {
                        //hide markers if we cant find them.
                        markers[i].SetActive(false);
                    }
                }
		    }
		}
        void OnDrawGizmos()
        {
            if (markerData != null && gizmo)
            {
                for (int i = 0; i < markerData.Count; i++)
                {
                    if (markerData[i].position.Length > 0)
                    {
                        Gizmos.DrawSphere(cv(markerData[i].position) + this.transform.position, markerScale);
                    }
                }
            }
        }

    }
}