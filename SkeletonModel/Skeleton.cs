using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity.SkeletonModel
{
    abstract class Skeleton
    {
        protected List<Bone> bones;
        public List<Bone> Bones
        {
            get { return bones; }
        }

        public Bone getRoot()
        {
            foreach (Bone b in bones)
            {
                if (b.isRoot())
                    return b;
            }
            return null; // TODO exception?
        }

        public void addBone(Bone b)
        {
            bones.Add(b);
        }

        public Bone getBone(string name)
        {
            foreach (Bone b in bones) {
                if (b.Name.Equals(name))
                {
                    return b;
                }
            }
            return null; // TODO throw exception?
        }
    }

    class Bone
    {
        private string name;
        public string Name
        {
            get { return name; }
        }
        private Vector3 pos;
        public Vector3 Pos
        {
            get { return pos; }
            set { pos = value; }
        }
        private Quaternion orientation;
        public Quaternion Orientation
        {
            get { return orientation; }
            set { orientation = value;  }
        }
        private List<Bone> children;
        public List<Bone> Children
        {
            get { return children; }
        }
        // TODO can a bone have more than one parent?
        private Bone parent;
        public Bone Parent
        {
            get { return parent; }
            set
            {
                this.parent = value;
                if (parent.children == null) parent.children = new List<Bone>() { this };
                else parent.children.Add(this);
            }
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
            // TODO set?
        }

        public Bone(string name, List<Bone> children, Bone parent)
        {
            this.name = name;
            this.children = children;
            this.parent = parent;
        }

        public Bone(string name, Vector3 pos, Quaternion orientation, List<Bone> children, Bone parent)
            : this(name, children, parent)
        {
            this.pos = pos;
            this.orientation = orientation;
        }

        public Bone(String name, Vector3 pos, Quaternion orientation)
        {
            this.name = name;
            this.pos = pos;
            this.orientation = orientation;
        }

        public bool isRoot()
        {
            return parent == null;
        }

        public bool isEndEffector()
        {
            return (children == null || children.Count == 0);
        }

        public Vector3 getDirection()
        {
            // The identity quaternion is associated with the direction
            // (0,0,-1)
            Vector3 identityDirection = new Vector3(0, 0, -1);

            return Vector3.Transform(identityDirection, orientation);
        }

        public Vector3 getUp()
        {
            Vector3 identityUp = new Vector3(0, 1, 0);
            return Vector3.Transform(identityUp, orientation);
        }


        public Vector3 getRight()
        {
            Vector3 identityRight = new Vector3(1, 0, 0);
            return Vector3.Transform(identityRight, orientation);
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
    }
}
