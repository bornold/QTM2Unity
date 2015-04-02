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
        // It is defined as an allowed range betwen angles [start,end]
        // where start != end && 0 < start, end <= 360
        // If both start and end is 0 no twist constraint exist
        private float startTwistLimit = -1;
        public float StartTwistLimit
        {
            get { return startTwistLimit; }
        }
        private float endTwistLimit = -1;
        public float EndTwistLimit
        {
            get { return endTwistLimit; }
        }

        public void SetOrientationalConstraints(float startAngle, float endAngle)
        {
            this.startTwistLimit = startAngle;
            this.endTwistLimit = endAngle;
        }
        public void SetOrientationalConstraints(Vector2 twist)
        {
            this.startTwistLimit = twist.X;
            this.endTwistLimit = twist.Y;
        }

        private float right, up, left, down;
        public Vector4 Constraints
        {
            get { return new Vector4(right, up, left, down); }
            set { right = value.X; up = value.Y; left = value.Z; down = value.W; }
        }
        public void SetRotationalConstraints(float _right, float _up, float _left, float _down)
        {
            this.endTwistLimit = _right;
            this.up = _up;
            this.startTwistLimit = _left;
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
            startTwistLimit = 0;
            endTwistLimit = 0;
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
        public Vector3 GetYAxis()
        {
            return Vector3.Normalize(Vector3.Transform(Vector3.UnitY, orientation));
        }

        public Vector3 GetZAxis()
        {
            return Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, orientation));
        }

        public Vector3 GetXAxis()
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
            Quaternion rot = QuaternionHelper.GetRotationBetween(GetYAxis(), v);
            Rotate(rot);

            /*float angle = Vector3.CalculateAngle(GetYAxis(), v);
            Vector3 axis = Vector3.Cross(GetYAxis(), v);
            Rotate(angle, axis);*/
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
