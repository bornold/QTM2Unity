using UnityEngine;
namespace QTM2Unity
{
    class RTwithJC : RT
    {
        public bool showSkeleton = true;
        public Color skelettColor = Color.white;
        public bool showMarkers = false;
        public float markerScale = 0.015f;
        public bool showRotationTrace = false;
        public float traceScale = 0.08f;
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
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            if (skeleton != null)
            {
                if (showMarkers)
                {
                    foreach (TreeNode<Bone> b in skeleton)
                    {
                        Gizmos.DrawSphere((b.Data.Pos + pos).Convert(), markerScale);
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
                    UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos + pos, traceScale);
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

