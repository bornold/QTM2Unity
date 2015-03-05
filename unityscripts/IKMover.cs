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
        public IK ik = IK.CCD;
        private IKApplier ikApplier = new IKApplier();
        public override void UpdateNext()
        {
            base.UpdateNext();
            skeleton = joints.GetJointLocation(markerData);
            if (ik == IK.CCD)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new CCD());
            }
            else if (ik == IK.TT)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new TargetTriangleIK());
            } else
            {
                skeleton = ikApplier.ApplyIK(skeleton, new FABRIK());
            }
            SetAll();
        }
    }
    public enum IK { CCD, TT, FABRIK}
}
