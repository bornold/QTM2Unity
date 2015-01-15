using UnityEngine;
using System.Collections;
using QTMRealTimeSDK;

namespace QTM2Unity.Unity
{
	class RTObject : MonoBehaviour
	{
		public string meshName;
		public Vector3 xyzOffset;
		public Vector3 eulerOffset;

        private RTClient proto;
		private sixDOFBody body;

		// Use this for initialization
		void Start ()
		{
            proto = RTClient.getInstance();
		}

		// Update is called once per frame
		void Update ()
		{
			body = proto.getBody(meshName);
			if(body != null)
			{
				if(body.position.magnitude > 0) //just to avoid error when position is NaN
				{
					transform.position = body.position + xyzOffset;
                    transform.rotation = body.rotation * Quaternion.Euler(eulerOffset);
				}
			}
		}
	}
}