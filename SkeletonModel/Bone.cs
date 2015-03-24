using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    public class Bone : IEquatable<Bone>
    {
        #region Vars getters and setters
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
                FlagExists();
            }
        }

        private Quaternion orientation;
        public Quaternion Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }
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
            : this (name, position)
        {
            this.orientation = orientation;
        }
        public Bone(string name, Vector3 position, Quaternion orientation, 
            float constraintRight,float constraintUp, float constraintLeft, float constraintDown)
            : this(name, position, orientation)
        {
            SetRotationalConstraint(constraintRight, constraintUp, constraintLeft, constraintDown);
        }
        public Bone(string name, Vector3 position, Quaternion orientation, Vector4 constriants)
            : this(name, position, orientation, constriants.X, constriants.Y, constriants.Z, constriants.W) 
        { }
#endregion
#region Orientational constraints
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
        #endregion
        #region RotationalConstraints
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
            if (orientationalConstr != null && checkRot)
            {
                //UnityEngine.Debug.Log(string.Format("orientational constraining {0} with regards to {1}", target.Name, this.Name));
                orientationalConstr.checkOrientationalConstraint(ref target, this);
            }
            if (rotationalConstr != null)
            {
                Vector4 constraints = false ?
                    new Vector4(rotationalConstr.Constraints.Z, 
                        rotationalConstr.Constraints.W, 
                        rotationalConstr.Constraints.X, 
                        rotationalConstr.Constraints.Y)
                    : rotationalConstr.Constraints;
                Vector3 res;
                if (rotationalConstr.RotationalConstraints(target.Pos, pos, L1, constraints, out res)) 
                    {
                        target.Pos = res;
                        RotateTowards(target.Pos - this.Pos);
                        if (orientationalConstr != null && checkRot)
                        {
                            orientationalConstr.checkOrientationalConstraint(ref target, this);
                        }
                        return true;
                    }
            }
            return false;
        }
        #endregion
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
        // TODO rotateDegrees, rotateRadians
        // rotates the bone with angle in radians!
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
