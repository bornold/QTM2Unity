using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    abstract class Skeleton : IEnumerable<TreeNode<Bone>>
    {
        protected TreeNode<Bone> root;

        public Bone this[string key]
        {
            get
            {
                return root.FindTreeNode(node => node.Data != null && node.Data.Name.Equals(key)).Data;
            }
            set
            {
                root.FindTreeNode(node => node.Data.Name.Equals(key)).Data = value;
            }
        }
        public Bone this[int key]
        {
            get
            {
                return root.FindTreeNode(node => node.Data != null && node.Level == key).Data;
            }
            set
            {
                root.FindTreeNode(node =>  node.Level == key).Data = value;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return root.GetEnumerator();

        }

        IEnumerator<TreeNode<Bone>> IEnumerable<TreeNode<Bone>>.GetEnumerator()
        {
            return root.GetEnumerator();
        }
    }
}
