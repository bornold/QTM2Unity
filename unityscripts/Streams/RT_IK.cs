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
        private IKSolver solver = new CCD();
        public IK ik = IK.CCD;

        public override void UpdateNext()
        {
            base.UpdateNext();
            switch (ik)
            {
                case IK.CCD:
                    solver = new CCD();
                    break;
                case IK.FABRIK:
                    solver = new FABRIK();
                    break;
                case IK.DLS:
                    solver = new DampedLeastSquares();
                    break;
                case IK.TRANSPOSE:
                    solver = new JacobianTranspose();
                    break;
                case IK.TT:
                    solver = new TargetTriangleIK();
                    break;
                default:
                    break;
            }
            skeleton = ikApplier.ApplyIK(skeleton, solver);
        }
    }
}


