using UnityEngine;
namespace QTM2Unity
{
    class RT_JC : RT
    {
        public bool resetSkeleton = false;
        public bool showSkeleton = true;
        public Color skelettColor = Color.white;
        public bool showJoints = false;
        public float jointScale = 0.015f;
        public Color jointColor = Color.green;
        public bool showRotationTrace = false;
        public float traceLength = 0.08f;
        protected BipedSkeleton skeleton;
        protected BipedSkeleton skeletonBuffer;

        protected MarkersPreprocessor mp;
        protected JointLocalization joints;
        
        public override void StartNext()
        {
            skeleton = new BipedSkeleton();
            skeletonBuffer = new BipedSkeleton();
            mp = new MarkersPreprocessor();
            joints = new JointLocalization();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            if (resetSkeleton || joints == null || skeleton == null || skeletonBuffer == null || mp == null)
            {
                skeleton = new BipedSkeleton();
                skeletonBuffer = new BipedSkeleton();
                mp = new MarkersPreprocessor();
                joints = new JointLocalization(); 
                resetSkeleton = false;
            }
            BipedSkeleton temp = skeleton;
            skeleton = skeletonBuffer;
            skeletonBuffer = temp;
           joints.GetJointLocation(mp.ProcessMarkers(markerData), ref skeleton);
            
        }
        void OnDrawGizmos()
        {
            if (Application.isPlaying && streaming && skeleton != null)
            {
                Draw();
            }
        }
        public override void Draw()
        {
            base.Draw();
            foreach (TreeNode<Bone> b in skeleton)
            {

                if (showSkeleton)
                {
                    foreach (TreeNode<Bone> child in b.Children)
                    {
                        UnityDebug.DrawLine(b.Data.Pos + pos, child.Data.Pos + pos, skelettColor);
                    }
                }
                if (showRotationTrace && (!b.IsLeaf || b.Data.Name.Equals(BipedSkeleton.HEAD)) )//&& b.Data.Name.EndsWith("L"))
                {
                    UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos + pos, traceLength);
                }
                if (showJoints)
                {
                    Gizmos.color = jointColor;
                    Gizmos.DrawSphere((b.Data.Pos + pos).Convert(), jointScale);
                }
            }
        }
    }
}

