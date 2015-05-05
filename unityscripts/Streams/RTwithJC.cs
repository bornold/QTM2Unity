using UnityEngine;
namespace QTM2Unity
{
    class RTwithJC : RT
    {
        public bool showSkeleton = true;
        public Color skelettColor = Color.white;
        public bool showJoints = false;
        public float jointScale = 0.015f;
        public bool showRotationTrace = false;
        public float traceLength = 0.08f;
        public Vector3 debugOffset = new Vector3(0, 0, 0);
        protected BipedSkeleton skeleton;
        protected BipedSkeleton skeletonBuffer;

        protected JointLocalization joints;
        
        public override void StartNext()
        {
            joints = new JointLocalization();
            skeleton = new BipedSkeleton();
            skeletonBuffer = new BipedSkeleton();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            if (joints == null) joints = new JointLocalization(); 
            if (skeleton == null) skeleton = new BipedSkeleton();
            if (skeletonBuffer == null) skeletonBuffer = new BipedSkeleton();

            BipedSkeleton temp = skeleton;
            skeleton = skeletonBuffer;
            skeletonBuffer = temp;
            joints.GetJointLocation(markerData, ref skeleton);
            pos += debugOffset.Convert();
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            if (skeleton != null)
            {
                if (showJoints)
                {
                    foreach (TreeNode<Bone> b in skeleton)
                    {
                        Gizmos.DrawSphere((b.Data.Pos + pos).Convert(), jointScale);
                    }
                }
                Draw();
            }
        }
        public void Draw()
        {
            foreach (TreeNode<Bone> b in skeleton)
            {
                if (showRotationTrace && (!b.IsLeaf || b.Data.Name.Equals(BipedSkeleton.HEAD)))
                {
                    UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos + pos, traceLength);
                }

                if (showSkeleton)
                {
                    foreach (TreeNode<Bone> child in b.Children)
                    {
                        UnityDebug.DrawLine(b.Data.Pos + pos, child.Data.Pos + pos, skelettColor);
                    }
                }
            }
        }
    }
}

