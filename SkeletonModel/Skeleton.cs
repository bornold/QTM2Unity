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
        }
        
        private OrientationalConstraint orientationalConstr;
        public OrientationalConstraint OrientationalConstraint
        {
            get { return orientationalConstr; }
            // TODO set?
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

        public bool isRoot()
        {
            return parent == null;
        }

        public bool isEndEffector()
        {
            return (children == null || children.Count == 0);
        }

    }
}
