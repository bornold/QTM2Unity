using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTM2Unity.SkeletonModel
{
        // TODO
        // What kinds of constraints do we have?

        // Orientational: The twist around the bones direction vector
        // described by rotor, or maybe an angle around direction vector?
        // like a range. from angle to angle?

        // Rotational: The rotation to the bones final direction vector 
        // described by angles t1...t4 in an irregular cone

    // TODO should these be private maybe? Or maybe not classes at aall. might be unnecessary.
    public class OrientationalConstraint
    {
        // An orientational contraint is defined as a range betwen angle1 and angle2
        private float angle1;
        public float Angle1
        {
            get { return angle1;  }
        }
        private float angle2;
        public float Angle2
        {
            get { return angle2; }
        }

        public OrientationalConstraint(float angle1, float angle2)
        {
            this.angle1 = angle1;
            this.angle2 = angle2;
        }

    }

    public class RotationalConstraint
    {
        // A constraint modeled as an irregular cone
        // The four angles define the shape of the cone
        private float[] angles = new float[4];

    }
}
