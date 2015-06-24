using System;
using OpenTK;

namespace QTM2Unity
{
    public class Bone : IEquatable<Bone>
    {
        #region Name, pos, rot and constraints getters and setters
        public bool Exists
        {
            get { return !pos.IsNaN(); }
        }
        public bool HasNaN
        {
            get { return pos.IsNaN() || orientation.IsNaN(); }
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
                pos = new Vector3(value);
            }
        }
        private Quaternion orientation;
        public Quaternion Orientation
        {
            get { return orientation; }
            set { orientation = new Quaternion(new Vector3(value.Xyz), value.W); }
        }
        #region constraints getters and setters

        // An orientational constraint is the twist of the bone around its own direction vector
        // with respect to its parent
        // It is defined as an allowed range betwen angles [start,end]
        // where start != end && 0 < start, end <= 360
        // If both start and end is 0 no twist constraint exist
        private Vector2 twistLimit = new Vector2(-1, -1);

        public Vector2 TwistLimit
        {
            get { return twistLimit; }
            set { twistLimit = new Vector2(value.X,value.Y); }
        }
        public bool HasTwistConstraints
        {
            get { return (twistLimit.X >= 0 && twistLimit.Y >= 0); }
        }
        public float StartTwistLimit
        {
            get { return twistLimit.X; }
        }
        public float EndTwistLimit
        {
            get { return twistLimit.Y; }
        }

        private Vector4 constraints = Vector4.Zero;
        public Vector4 Constraints
        {
            get { return new Vector4(constraints); } //TODO Checka all values > 0
            set { constraints = new Vector4(value); }
        }
        public bool HasConstraints
        {
            get { return (constraints != Vector4.Zero); }
        }
        private Quaternion parentPointer = Quaternion.Identity;
        public Quaternion ParentPointer
        {
            get {return parentPointer;}
            set {parentPointer = value;}
        }
        private float stiffness = 1;
        public float Stiffness
        {
            get { return stiffness; }
            set { stiffness = value; }
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
        }

        public Bone(string name, Vector3 position, Quaternion orientation)
            : this(name, position)
        {
            this.orientation = orientation;
        }
        public Bone(string name, Vector3 position, Quaternion orientation, Vector4 constriants)
            : this(name, position, orientation)
        { 
            Constraints = constriants;
        }
        #endregion

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

        public void RotateTowards(Vector3 v, float stiffness = 1f)
        {
            Rotate(QuaternionHelper.GetRotationBetween(GetYAxis(), v, stiffness = this.stiffness));
        }
        #endregion
        public bool Equals(Bone other)
        {
            return name.Equals(other.Name) && orientation.Equals(other.Orientation) && pos.Equals(other.Pos);
        }

        public override string ToString()
        {
            return string.Format("{0} at position: {1} with orientation: {2}", name, pos, orientation);
        }
    }
}
