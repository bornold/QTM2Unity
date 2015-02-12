using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QTMRealTimeSDK;
using OTK = OpenTK;
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
            if (rtClient == null)
            {
                rtClient = RTClient.getInstance();
            }
			if (rtClient.Bones != null)
			{
				var boneData = rtClient.Bones;
				Gizmos.color = Color.yellow;
				for (int i = 0; i < boneData.Count; i++)
	            {
	                if (visibleBones)
					{
						Gizmos.DrawLine(convertFromSlimDXVector(boneData[i].fromMarker.position) + this.transform.position,
                                        convertFromSlimDXVector(boneData[i].toMarker.position) + this.transform.position);
					}
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

