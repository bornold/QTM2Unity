using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
namespace QTM2Unity
{
    class RT_IK : RTwithJC
    {
        private IKApplier ikApplier = new IKApplier();
        public IK ik = IK.CCD;

        public override void UpdateNext()
        {
            base.UpdateNext();
            if (ik == IK.CCD)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new CCD());
            }
            else if (ik == IK.FABRIK)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new FABRIK());
            }
            else if (ik == IK.TRANSPOSE)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new JacobianTranspose());
            }
            else if (ik == IK.DLS)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new DampedLeastSquares());
            }
            else
            {
                skeleton = ikApplier.ApplyIK(skeleton, new TargetTriangleIK());
            }
        }
    }
}


