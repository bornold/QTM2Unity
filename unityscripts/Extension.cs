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
    }
}