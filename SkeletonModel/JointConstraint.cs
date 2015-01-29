using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTM2Unity.SkeletonModel
{
    class JointConstraint
    {
        // TODO
        // What kinds of constraints do we have?

        // Orientational: The twist around the bones direction vector
        // described by rotor, or maybe an angle around direction vector?
        // like a range. from angle to angle?
        private OrientationalConstraint oConstr;

        // Rotational: The rotation to the bones final direction vector 
        // described by angles t1...t4 in an irregular cone
        private RotationalConstraint rotConstr;

        // TODO should these be private maybe? Or maybe not classes at aall. might be unnecessary.
        public class OrientationalConstraint
        {
            private float from;
            private float to;

            public OrientationalConstraint(float from, float to)
            {
                this.from = from;
                this.to = to;
            }
        }

        public class RotationalConstraint
        {
            // A constraint modeled as an irregular cone
            // The four angles define the shape of the cone
            private float[] angles = new float[4];

        }
    }
}
