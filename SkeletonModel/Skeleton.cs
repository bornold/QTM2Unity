using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    abstract class Skeleton : IEnumerable
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
                root.FindTreeNode(node => node.Data != null && node.Data.Name.Equals(key)).Data = value;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return root.GetEnumerator();

        }
        
    }
}
