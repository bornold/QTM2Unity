using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
namespace QTM2Unity
{
    class RT_IK : RT_JC
    {
        private IKApplier ikApplier;
        public IK ikAlgorithm = IK.CCD;
        public override void StartNext()
        {
            base.StartNext();
            ikApplier = new IKApplier(new CCD());
        }
        public override void UpdateNext()
        {
            base.UpdateNext();
            if (ikApplier == null) ikApplier = new IKApplier(new CCD());
                switch (ikAlgorithm)
                {
                    case IK.CCD:
                        ikApplier.IKSolver = new CCD();
                        break;
                    case IK.FABRIK:
                        ikApplier.IKSolver = new FABRIK();
                        break;
                    case IK.DLS:
                        ikApplier.IKSolver = new DampedLeastSquares();
                        break;
                    case IK.TRANSPOSE:
                        ikApplier.IKSolver = new JacobianTranspose();
                        break;
                    case IK.TT:
                        ikApplier.IKSolver = new TargetTriangleIK();
                        break;
                    default:
                        break;
            }
            ikApplier.ApplyIK(ref skeleton);
        }
        public override void Draw()
        {
            base.Draw();
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying || !streaming) return;            
            Draw();
        }
    }
}


