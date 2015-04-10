using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QTMRealTimeSDK;
namespace QTM2Unity
{
	public class RTBones : RT
	{

		public bool visibleBones = true;

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
						Gizmos.DrawLine(boneData[i].fromMarker.position.Convert() + this.transform.position,
                                        boneData[i].toMarker.position.Convert() + this.transform.position);
					}
	            }
			}
        }

        public override void UpdateNext()
        {
        }

        public override void StartNext()
        {
        }
    }
}

