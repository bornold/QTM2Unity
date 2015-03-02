using System.Collections.Generic;
using UnityEngine;
using QTM2Unity.Unity;
namespace QTM2Unity
{
    class JC2ModelTest : StandardUnityModel
    {
        // Update is called once per frame
        public override void UpdateNext()
        {
            base.UpdateNext();
            skeleton = joints.GetJointLocation(markerData);
            SetAll();
        }
    
    }
}


