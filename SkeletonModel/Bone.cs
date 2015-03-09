using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    class Bone : IEquatable<Bone>
    {
        private bool exists = false;
        public bool Exists
        {
            get { return exists; }
        }

        private string name;
        public string Name
        {
            get { return name; }
        }

        private Vector3 pos;
        public Vector3 Pos
        {
            get { return pos; }
            set { 
                pos = value;
                flagExists();
            }
        }

        private Quaternion orientation;
        public Quaternion Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        private OrientationalConstraint orientationalConstr;
        public OrientationalConstraint OrientationalConstraint
        {
            get { return orientationalConstr; }
        }
        public void setOrientationalConstraint(float from, float to)
        {
            orientationalConstr = new OrientationalConstraint(from, to);
        }

        private RotationalConstraint rotationalConstr;
        public RotationalConstraint RotationalConstraint
        {
            get { return rotationalConstr; }
        }
        public void setRotationalConstraint(float a0, float a1, float a2, float a3, 
            Func<Vector3> directionMethod, Func<Vector3> rightMethod)
        {
            rotationalConstr = new RotationalConstraint(a0, a1, a2, a3, directionMethod, rightMethod);
        }

        public Bone(string name)
        {
            this.name = name;
        }

        public Bone(string name, Vector3 position) 
            : this(name)
        {
            pos = position;
            flagExists();
        }

        public Bone(string name, Vector3 position, Quaternion orientation) 
            : this (name, position)
        {
            this.orientation = orientation;
        }

        public bool Equals(Bone other)
        {
            return name.Equals(other.Name) && orientation.Equals(other.Orientation) && pos.Equals(other.Pos);
        }

        public string ToString()
        {
            return string.Format("{0} at position: {1} with orientation: {2}", name, pos, orientation);
        }

        public Vector3 getDirection()
        {
            // The identity quaternion is associated with the direction
            Vector3 identityDirection = Vector3.UnitY;

            return Vector3.Normalize(Vector3.Transform(identityDirection, orientation));
        }

        public Vector3 getUp()
        {
            Vector3 identityUp = Vector3.UnitZ;
            return Vector3.Normalize(Vector3.Transform(identityUp, orientation));
        }

        public Vector3 getRight()
        {
            Vector3 identityRight = Vector3.UnitX;
            return Vector3.Normalize(Vector3.Transform(identityRight, orientation));
        }

        // TODO rotateDegrees, rotateRadians
        // rotates the bone with angle in radians!
        public void rotate(float angle, Vector3 axis)
        {
            Quaternion rotation = Quaternion.FromAxisAngle(axis, angle);
            rotate(rotation);
        }

        public void rotate(Quaternion rotation)
        {
            orientation = rotation * orientation;
        }

        public void rotateTowards(Vector3 v)
        {
            float angle = Vector3.CalculateAngle(getDirection(), v);
            Vector3 axis = Vector3.Cross(getDirection(), v);
            rotate(angle, axis);
        }

        private void flagExists()
        {
            exists = true;
            if (pos.IsNaN())
            {
                exists = false;
            }
        }
        bool IEquatable<Bone>.Equals(Bone other)
        {
            return this.Name.Equals(other.Name);
        }
    }
}
