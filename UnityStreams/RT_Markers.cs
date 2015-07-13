using System.Collections.Generic;
using UnityEngine;
namespace QTM2Unity
{
    class RT_Markers : RT
    {
        private MarkersPreprocessor mp;
        protected Dictionary<string, OpenTK.Vector3> markers;

        public override void StartNext()
        {
            mp = new MarkersPreprocessor();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            if (mp == null)
            {
                UnityEngine.Debug.LogWarning("Reseting");
                mp = new MarkersPreprocessor();
                return;
            }
            if (!mp.ProcessMarkers(markerData, out markers))
            {
                Debug.LogError("markers...");
                return;
            }
        }
    }
}

