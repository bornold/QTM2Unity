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
        public float traceScale = 0.02f;
        protected BipedSkeleton skeleton;
        protected JointLocalization joints;
        
        public override void StartNext()
        {
            joints = new JointLocalization();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            if (joints == null) joints = new JointLocalization();
            BipedSkeleton lastSkel = skeleton;
            if (debug) { foreach (LabeledMarker LM in markerData) Debug.Log(LM.label); debug = false; }
            skeleton = joints.GetJointLocation(markerData);
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
                if (showRotationTrace)
                {
                    UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos.Convert(), traceScale);
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

