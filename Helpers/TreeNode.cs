using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QTM2Unity
{
    public class TreeNode<T> : IEnumerable<TreeNode<T>>
    {
        /// <summary>
        /// The data contained in the TreeNode
        /// </summary>
        public T Data { get; set; }
        /// <summary>
        /// A pointer to the parent Node
        /// </summary>
        public TreeNode<T> Parent { get; set; }
        /// <summary>
        /// A collection of children Node
        /// </summary>
        public ICollection<TreeNode<T>> Children { get; set; }
        /// <summary>
        /// Returns if Node has no Parent 
        /// </summary>
        public Boolean IsRoot
        {
            get { return Parent == null; }
        }
        /// <summary>
        /// Returns true if Node has no Children
        /// </summary>
        public Boolean IsLeaf
        {
            get { return Children.Count == 0; }
        }
        ///// <summary>
        ///// Returns the depth from the parent node
        ///// </summary>
        //public int Level
        //{
        //    get
        //    {
        //        if (this.IsRoot)
        //            return 0;
        //        return Parent.Level + 1;
        //    }
        //}

        /// <summary>
        /// Constructor for a new node
        /// </summary>
        /// <param name="data">The Data the node should contain</param>
        public TreeNode(T data)
        {
            this.Data = data;
            this.Children = new List<TreeNode<T>>();
        }

        /// <summary>
        /// Adding a new node as child to this node
        /// </summary>
        /// <param name="child">The Data of the child node</param>
        /// <returns>A reference to the child node</returns>
        public TreeNode<T> AddChild(T child)
        {
            TreeNode<T> childNode = new TreeNode<T>(child) { Parent = this };
            this.Children.Add(childNode);

            //this.RegisterChildForSearch(childNode);

            return childNode;
        }

        /// <summary>
        /// Apply an action to all the Data in the tree
        /// </summary>
        /// <param name="action">The action to be applied on the Data</param>
        public void Traverse(Action<T> action)
        {
            action(Data);
            foreach (var child in Children)
                child.Traverse(action);
        }
        /// <summary>
        /// Apply and action on all the Nodes in the tree
        /// </summary>
        /// <param name="action">The action on wich to be applied to the tree</param>
        public void Traverse(Action<TreeNode<T>> action)
        {
            action(this);
            foreach (var child in Children)
                child.Traverse(action);
        }

        public override string ToString()
        {
            return Data != null ? Data.ToString() : "[data null]";
        }


        #region searching
        

        /// <summary>
        /// Returns the first TreeNode of which the first predicate is true, otherwise default
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public TreeNode<T> FindTreeNode(Func<TreeNode<T>, bool> predicate)
        {
            if (predicate(this)) return this;
            else foreach (var child in Children)
                {
                    var res = child.FindTreeNode(predicate);
                    if (res != null) return res;
                }
            return null;
        }
        #endregion


        #region iterating
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TreeNode<T>> GetEnumerator()
        {
            yield return this;
            foreach (var directChild in this.Children)
            {
                foreach (var anyChild in directChild)
                    yield return anyChild;
            }
        }

        #endregion
    }
}
