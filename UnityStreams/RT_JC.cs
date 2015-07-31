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
        private string prefix;
        public override void StartNext()
        {
            base.StartNext();
            skeleton = new BipedSkeleton();
            skeletonBuffer = new BipedSkeleton();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            base.UpdateNext();

            if ((bodyRig != null && bodyRig.resetSkeleton) || skeleton == null || skeletonBuffer == null || //reset || 
                bodyRig.bodyPrefix != prefix)
            {
                UnityEngine.Debug.LogWarning("Reseting");
                skeleton = new BipedSkeleton();
                skeletonBuffer = new BipedSkeleton();
                mp = null;
                joints = null;
                bodyRig.resetSkeleton = false;
                prefix = bodyRig.bodyPrefix;
                return;
            }
            if (debugFlag) return;
            if (mp == null || joints == null)
            {
                Markers markersMap;
                mp = new MarkersPreprocessor(markerData, out markersMap, bodyPrefix: bodyRig.bodyPrefix);
                joints = new JointLocalization(markersMap);
            }
            Dictionary<string, OpenTK.Vector3> markers;

            if (!mp.ProcessMarkers(markerData, out markers, bodyRig.bodyPrefix))
            {
                return;
            }
            var temp = skeleton;
            skeleton = skeletonBuffer;
            skeletonBuffer = temp;
            joints.GetJointLocation(markers, ref skeleton);
        }
        void OnDrawGizmos()
        {
            ShouldWeDraw();
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
                            UnityEngine.Debug.DrawLine(b.Data.Pos.Convert() + pos, child.Data.Pos.Convert() + pos, bodyRig.skelettColor);
                        }
                    }
                    if (bodyRig.showRotationTrace && (!b.IsLeaf))
                    {
                        UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos.Convert() + pos, bodyRig.traceLength);
                    }
                    if (bodyRig.showJoints)
                    {
                        Gizmos.DrawSphere(b.Data.Pos.Convert() + pos, bodyRig.jointScale);
                    }
                }
            }
        }
    }
}

