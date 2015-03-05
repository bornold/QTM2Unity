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
        public IK ik = IK.ccd;
        private IKApplier ikApplier = new IKApplier();
        public override void UpdateNext()
        {
            base.UpdateNext();
            skeleton = joints.GetJointLocation(markerData);
            if (ik == IK.ccd)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new CCD());
            }
            else
            {
                skeleton = ikApplier.ApplyIK(skeleton, new TargetTriangleIK());
            }
            SetAll();
        }
    }
    public enum IK { ccd, TT}
}
