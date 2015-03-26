using System;
using OpenTK;

namespace QTM2Unity
{
    public class Bone : IEquatable<Bone>
    {
        #region Name, pos, rot and constraints getters and setters
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
            set
            {
                pos = value;
                FlagExists();
            }
        }
        private Quaternion orientation;
        public Quaternion Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }
        #region constraints getters and setters
                // An orientational constraint is the twist of the bone around its own direction vector
        // with respect to its parent
        // It is defined as a range betwen angles [right,left]
        private float rightTwist;
        public float RightTwist
        {
            get { return rightTwist; }
        }
        private float leftTwist;
        public float LeftTwist
        {
            get { return leftTwist; }
        }

        public void SetOrientationalConstraints(float left, float right)
        {
            this.leftTwist = left;
            this.rightTwist = right;
        }

        private float right, up, left, down;
        public Vector4 Constraints
        {
            get { return new Vector4(right, up, left, down); }
            set { right = value.X; up = value.Y; left = value.Z; down = value.W; }
        }
        public void SetRotationalConstraints(float _right, float _up, float _left, float _down)
        {
            this.rightTwist = _right;
            this.up = _up;
            this.leftTwist = _left;
            this.down = _down;
        }
        public void SetRotationalConstraints(Vector4 givenConstraints)
        {
            this.right = givenConstraints.X;
            this.up = givenConstraints.Y;
            this.left = givenConstraints.Z;
            this.down = givenConstraints.W;
        }
        #endregion
        #endregion

        #region Constructors
        public Bone(string name)
        {
            this.name = name;
        }

        public Bone(string name, Vector3 position)
            : this(name)
        {
            pos = position;
            FlagExists();
        }

        public Bone(string name, Vector3 position, Quaternion orientation)
            : this(name, position)
        {
            this.orientation = orientation;
        }
        public Bone(string name, Vector3 position, Quaternion orientation,
            float constraintRight, float constraintUp, float constraintLeft, float constraintDown)
            : this(name, position, orientation)
        {
            SetRotationalConstraints(constraintRight, constraintUp, constraintLeft, constraintDown);
        }
        public Bone(string name, Vector3 position, Quaternion orientation, Vector4 constriants)
            : this(name, position, orientation, constriants.X, constriants.Y, constriants.Z, constriants.W)
        { }
        #endregion
#if flase
        private Constraint orientationalConstr;
        public Constraint OrientationalConstraint
        {
            get { return orientationalConstr; }
        }
        public void setOrientationalConstraint(float from, float to)
        {
            orientationalConstr = new Constraint(from, to);
        }
        private RotationalConstraint rotationalConstr;
        public RotationalConstraint RotationalConstraint
        {
            get { return rotationalConstr; }
        }
        public void SetRotationalConstraint(float right, float up, float left, float down)
        {
            rotationalConstr = new RotationalConstraint(right, up, left, down);
        }
        public void SetRotationalConstraint(Vector4 constraints)
        {
            rotationalConstr = new RotationalConstraint(constraints.X, constraints.Y, constraints.Z, constraints.W);
        }
        public bool EnsureConstraints(ref Bone target, Vector3 L1, bool checkRot)
        {
             if (checkRot)
            {
                Constraint.CheckOrientationalConstraint(ref target, this, this.leftTwist,this.rightTwist);
            }         
            Vector3 res;
            if (Constraint.CheckRotationalConstraints(target.Pos, this.Pos, L1, Constraints, out res))
            {
                target.Pos = res;
                RotateTowards(target.Pos - this.Pos);
                if (checkRot)
                {
                    Constraint.CheckOrientationalConstraint(ref target, this, this.leftTwist,this.rightTwist);
                }
                return true;
            }
            
            return false;
        }
#endif
        // Directions 
        #region Direction getters
        public Vector3 GetDirection()
        {
            return Vector3.Normalize(Vector3.Transform(Vector3.UnitY, orientation));
        }

        public Vector3 GetUp()
        {
            return Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, orientation));
        }

        public Vector3 GetRight()
        {
            return Vector3.Normalize(Vector3.Transform(Vector3.UnitX, orientation));
        }
        #endregion
        #region rotation methods
        public void Rotate(float angle, Vector3 axis)
        {
            Rotate(Quaternion.FromAxisAngle(axis, angle));
        }

        public void Rotate(Quaternion rotation)
        {
            orientation = rotation * orientation;
        }

        public void RotateTowards(Vector3 v)
        {
            float angle = Vector3.CalculateAngle(GetDirection(), v);
            Vector3 axis = Vector3.Cross(GetDirection(), v);
            Rotate(angle, axis);
        }
        #endregion
        private void FlagExists()
        {
            exists = true;
            if (pos.IsNaN())
            {
                exists = false;
            }
        }
        public bool Equals(Bone other)
        {
            return name.Equals(other.Name) && orientation.Equals(other.Orientation) && pos.Equals(other.Pos);
        }

        public string ToString()
        {
            return string.Format("{0} at position: {1} with orientation: {2}", name, pos, orientation);
        }
    }
}
