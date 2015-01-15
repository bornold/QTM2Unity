using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QTMRealTimeSDK;

namespace QTM2Unity.Unity
{
	public class RTBones : MonoBehaviour
	{
        /// Stream bones from QTM
		private RTClient rtClient;
		
		public bool visibleBones = true;

		// Use this for initialization
		void Start ()
		{
            rtClient = RTClient.getInstance();
		}

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

			if (rtClient.Bones != null)
			{
				var boneData = rtClient.Bones;
				Gizmos.color = Color.yellow;
				for (int i = 0; i < boneData.Count; i++)
	            {
	                if (visibleBones)
					{
						Gizmos.DrawLine(boneData[i].fromMarker.position, boneData[i].toMarker.position);
					}
	            }
			}
        }
	}
}

