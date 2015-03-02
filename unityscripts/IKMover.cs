using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QTM2Unity
{
    class IKMover : StandardUnityModel
    {
        private IKApplier ikApplier = new IKApplier();
        public override void UpdateNext()
        {
            base.UpdateNext();
            skeleton = joints.GetJointLocation(markerData);
            skeleton = ikApplier.ApplyIK(skeleton);            
            SetAll();
        }
    }
}
