using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
        // TODO
        // What kinds of constraints do we have?

        // Orientational: The twist around the bones direction vector
        // described by rotor, or maybe an angle around direction vector?
        // like a range. from angle to angle?


    public class OrientationalConstraint
    {
        // An orientational constraint is the twist of the bone around its own direction vector
        // with respect to its parent
        // It is defined as a range betwen angles [from,to]
        private float from;
        public float From
        {
            get { return from; }
        }
        private float to;
        public float To
        {
            get { return to; }
        }

        public OrientationalConstraint(float from, float to)
        {
            this.from = from;
            this.to = to;
        }

    }

    public class RotationalConstraint
    {
        // A constraint modeled as an irregular cone
        // The direction vector is the direction the cone is opening up at
        // The four angles define the shape of the cone
        // angle 0 is the angle to the parent's right
        // angle 1 is the angle to the parent's down
        // angle 2 is the angle to the parent's left
        // angle 3 is the angle to the parent's up
        Func<Vector3> directionMethod;
        Func<Vector3> rightMethod;
        private float[] angles = new float[4];
        public float[] Angles
        {
            get { return angles; }
        }
        
        public float getAngle(int i)
        {
            return angles[i];
        }

        public RotationalConstraint(float[] angles, Func<Vector3> directionMethod,
            Func<Vector3> rightMethod)
        {
            this.directionMethod = directionMethod;
            this.rightMethod = rightMethod;
            for (int i = 0; i < 4; i++)
            {
                this.angles[i] = angles[i];
            }
        }

        public RotationalConstraint(float a0, float a1, float a2, float a3,
            Func<Vector3> directionMethod, Func<Vector3> rightMethod)
        {
            this.directionMethod = directionMethod;
            this.rightMethod = rightMethod;
            angles[0] = a0;
            angles[1] = a1;
            angles[2] = a2;
            angles[3] = a3;
        }

        public Vector3 getDirection()
        {
            return directionMethod();
        }

        public Vector3 getRight()
        {
            return rightMethod();
        }
    }
}
