namespace QTM2Unity
{
    public static class Extensions
    {
        public static UnityEngine.Transform Search(this UnityEngine.Transform target, string name)
        {
            if (target.name == name) return target;

            for (int i = 0; i < target.childCount; ++i)
            {
                var result = Search(target.GetChild(i), name);

                if (result != null) return result;
            }
            return null;
        }
        public static UnityEngine.Vector3 Convert(this OpenTK.Vector3 v)
        {
            return new UnityEngine.Vector3(v.X, v.Y, v.Z);
        }
        public static OpenTK.Vector3 Convert(this UnityEngine.Vector3 v)
        {
            return new OpenTK.Vector3(v.x, v.y, v.z);
        }
        public static UnityEngine.Quaternion Convert(this OpenTK.Quaternion q)
        {
            return new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);
        }
        public static OpenTK.Quaternion Convert(this UnityEngine.Quaternion q)
        {
            return new OpenTK.Quaternion(q.x, q.y, q.z, q.w);
        }
        public static UnityEngine.Vector4 Convert(this OpenTK.Vector4 v)
        {
            return new UnityEngine.Vector4(v.X, v.Y, v.Z, v.W);
        }
        public static OpenTK.Vector4 Convert(this UnityEngine.Vector4 v)
        {
            return new OpenTK.Vector4(v.x, v.y, v.z, v.w);
        }
        public static UnityEngine.Vector2 Convert(this OpenTK.Vector2 v)
        {
            return new UnityEngine.Vector2(v.X, v.Y);
        }
        public static OpenTK.Vector2 Convert(this UnityEngine.Vector2 v)
        {
            return new OpenTK.Vector2(v.x, v.y);
        }
    }
}