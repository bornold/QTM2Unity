using System.Collections.Generic;
using UnityEngine;
namespace QTM2Unity
{
    [System.Serializable]
    public class BodyRig
    {
        public string bodyPrefix = "";
        public bool showSkeleton = false;
        public Color skelettColor = Color.black;
        public bool showJoints = false;
        [Range(0.01f, 0.05f)]
        public float jointScale = 0.015f;
        public Color jointColor = Color.green;
        public bool showRotationTrace = false;
        [Range(0.01f, 0.5f)]
        public float traceLength = 0.08f;
        public bool resetSkeleton = false;
    }
    class RT_JC : RT
    {
        public BodyRig bodyRig;
        protected BipedSkeleton skeleton;
        protected BipedSkeleton skeletonBuffer;
        private MarkersPreprocessor mp;
        private JointLocalization joints;
        private Dictionary<string, OpenTK.Vector3> markers;


        public override void StartNext()
        {
            base.StartNext();
            skeleton = new BipedSkeleton();
            skeletonBuffer = new BipedSkeleton();
            mp = new MarkersPreprocessor();
            joints = new JointLocalization();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            base.UpdateNext();
            if ((bodyRig != null && bodyRig.resetSkeleton) || joints == null || skeleton == null || skeletonBuffer == null || mp == null)
            {
                UnityEngine.Debug.LogWarning("Reseting");
                skeleton = new BipedSkeleton();
                skeletonBuffer = new BipedSkeleton();
                mp = new MarkersPreprocessor(bodyPrefix: bodyRig.bodyPrefix);
                joints = new JointLocalization();
                bodyRig.resetSkeleton = false;
                return;
            }
            if (!mp.ProcessMarkers(markerData, out markers))
            {
                Debug.LogError("markers...");
                return;
            }
            if (!debugFlag)
            {
                var temp = skeleton;
                skeleton = skeletonBuffer;
                skeletonBuffer = temp;
                joints.GetJointLocation(markers, ref skeleton);
            }
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
            if (bodyRig != null && skeleton != null && 
                (bodyRig.showSkeleton || bodyRig.showRotationTrace || bodyRig.showJoints))
            {
                Gizmos.color = bodyRig.jointColor;

                foreach (TreeNode<Bone> b in skeleton.Root)
                {
                    if (bodyRig.showSkeleton)
                    {
                        foreach (TreeNode<Bone> child in b.Children)
                        {
                            UnityDebug.DrawLine(b.Data.Pos + pos, child.Data.Pos + pos, bodyRig.skelettColor);
                        }
                    }
                    if (bodyRig.showRotationTrace && (!b.IsLeaf))
                    {
                        UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos + pos, bodyRig.traceLength);
                    }
                    if (bodyRig.showJoints)
                    {
                        Gizmos.DrawSphere((b.Data.Pos + pos).Convert(), bodyRig.jointScale);
                    }
                }
            }
        }
    }
}

