using UnityEngine;
namespace QTM2Unity
{
    public static class Extensions
    {
        public static Transform Search(this Transform target, string name)
        {
            if (target.name == name) return target;

            for (int i = 0; i < target.childCount; ++i)
            {
                var result = Search(target.GetChild(i), name);

                if (result != null) return result;
            }
            return null;
        }
        public static Vector3 Convert(this OpenTK.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        public static OpenTK.Vector3 Convert(this Vector3 v)
        {
            return new OpenTK.Vector3(v.x, v.y, v.z);
        }
        public static Quaternion Convert(this OpenTK.Quaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
        public static OpenTK.Quaternion Convert(this Quaternion q)
        {
            return new OpenTK.Quaternion(q.x, q.y, q.z, q.w);
        }
        public static Vector4 Convert(this OpenTK.Vector4 v)
        {
            return new Vector4(v.X, v.Y, v.Z, v.W);
        }
        public static OpenTK.Vector4 Convert(this Vector4 v)
        {
            return new OpenTK.Vector4(v.x, v.y, v.z, v.w);
        }
    }
}