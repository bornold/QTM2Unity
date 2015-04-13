using System.Collections.Generic;
using UnityEngine;
using QTM2Unity.Unity;
namespace QTM2Unity
{
    class JC2ModelTest : RTStandardUnityModel
    {
        // Update is called once per frame
        public override void UpdateNext()
        {
            base.UpdateNext();
            skeleton = new BipedSkeleton();
            joints.GetJointLocation(markerData, ref skeleton);
            SetAll();
        }
    }
}


