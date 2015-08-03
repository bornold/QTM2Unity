namespace QTM2Unity
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
    public static class Extensions
    {
        //public static Transform Search(this UnityEngine.Transform target, string name)
        //{
        //    if (target.name == name) return target;

        //    for (int i = 0; i < target.childCount; ++i)
        //    {
        //        var result = Search(target.GetChild(i), name);

        //        if (result != null) return result;
        //    }
        //    return null;
        //}
        /// <summary>
        /// Returns every direct child of a GameObject
        /// </summary>
        /// <param name="parent">The parent of the childs</param>
        /// <returns>Array of direct children</returns>
        public static Transform[] DirectChildren(this Transform parent)
        {
            List<Transform> res = new List<Transform>();
            foreach (Transform child in parent) res.Add(child);
            return res.ToArray();
        }
        /// <summary>
        /// Converts a OpenTK Vector3 to a Unity Vector3
        /// </summary>
        /// <param name="q">A OpenTK Vector3 to be converted</param>
        /// <returns>A Unity Vector3</returns>
        public static Vector3 Convert(this OpenTK.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        /// <summary>
        /// Converts a Unity Vector3 to a OpenTK Vector3
        /// </summary>
        /// <param name="q">A Unity Vector3 to be converted</param>
        /// <returns>A OpenTK Vector3</returns>
        public static OpenTK.Vector3 Convert(this UnityEngine.Vector3 v)
        {
            return new OpenTK.Vector3(v.x, v.y, v.z);
        }
        /// <summary>
        /// Converts a OpenTK Quaternion to a Unity Quaternion
        /// </summary>
        /// <param name="q">A OpenTK Quaternion to be converted</param>
        /// <returns>A Unity Quaternion</returns>
        public static Quaternion Convert(this OpenTK.Quaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
        /// <summary>
        /// Converts a Unity Quaternion to a OpenTK Quaternion
        /// </summary>
        /// <param name="q">A Unity Quaternion to be converted</param>
        /// <returns>A OpenTK Quaternion</returns>
        public static OpenTK.Quaternion Convert(this UnityEngine.Quaternion q)
        {
            return new OpenTK.Quaternion(q.x, q.y, q.z, q.w);
        }
        /// <summary>
        /// Determines whether the second Transform is an ancestor to the first Transform.
        /// </summary>
        public static bool IsAncestorOf(this Transform transform, Transform ancestor)
        {
            return
                !transform ||
                !ancestor ||
                (transform.parent && transform.parent == ancestor) ||
                IsAncestorOf(transform.parent, ancestor);
        }

        /// <summary>
        /// Returns true if the transforms contains the child
        /// </summary>
        public static bool ContainsChild(this Transform transform, Transform child)
        {
            return
                (transform == child) ||
                transform.GetComponentsInChildren<Transform>().Contains(child);
        }

        /// <summary>
        /// Gets the first common ancestor up the hierarchy
        /// </summary>
        public static Transform CommonAncestorOf(this Transform t1, Transform t2)
        {
            if (!(t1 || t2 || t1.parent || t2.parent)) return null;
            return (IsAncestorOf(t2, t1.parent)) ? t1.parent : CommonAncestorOf(t1.parent, t2);
        }
    }
}