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
        public bool Extrapolate = false;
        public override void StartNext()
        {
            base.StartNext();
            ikApplier = new IKApplier();
        }
        public override void UpdateNext()
        {
            base.UpdateNext();
            if (ikApplier == null) ikApplier = new IKApplier();
            ikApplier.test = Extrapolate;
            // GC 5.1kB
            ikApplier.ApplyIK(ref skeleton);
        }
    }
}


