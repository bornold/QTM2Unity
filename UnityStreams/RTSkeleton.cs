using System.Collections.Generic;
using QualisysRealTime.Unity;
using UnityEngine;

namespace QTM2Unity
{
 
    class RTSkeleton : MonoBehaviour
    {
        public Debugging debug;
        protected RTClient rtClient;
        protected Vector3 pos;
        protected bool streaming = false;
        //protected bool fileStreaming = false;
        //protected bool reset = false;
        protected List<LabeledMarker> markerData;
        protected List<string> markersLabels;
        public virtual void StartNext() { }
        public virtual void UpdateNext() { }

        protected BipedSkeleton skeleton;
        protected BipedSkeleton skeletonBuffer;
        private MarkersPreprocessor mp;
        private JointLocalization joints;
        private IKApplier ikApplier;

        private string prefix;
        void Start()
        {
            rtClient = RTClient.GetInstance();
            skeleton = new BipedSkeleton();
            skeletonBuffer = new BipedSkeleton();
            ikApplier = new IKApplier();
            StartNext();
        }

        // Update is called once per frame
        void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();
            streaming = rtClient.GetStreamingStatus();
            if (!streaming && !debug.debugFlag)
            {
                UnityEngine.Debug.LogWarning("Could'nt connect to real time stream.");
                debug.debugFlag = true;
                return;
            }

            markerData = rtClient.Markers;
            if ((markerData == null || markerData.Count == 0) && !debug.debugFlag) 
            {
                UnityEngine.Debug.LogWarning("Stream does not contain any markers");
                return;
            }
            pos = this.transform.position + debug.offset;
            
            if ((debug != null && debug.bodyRig.resetSkeleton) 
                || skeleton == null 
                || skeletonBuffer == null 
                || debug.bodyRig.bodyPrefix != prefix)
            {
                UnityEngine.Debug.LogWarning("Reseting");
                skeleton = new BipedSkeleton();
                skeletonBuffer = new BipedSkeleton();
                mp = null;
                joints = null;
                debug.bodyRig.resetSkeleton = false;
                prefix = debug.bodyRig.bodyPrefix;
                return;
            }
            if (debug.debugFlag)
            {
                UpdateNext();
                return;
            }
            if (mp == null || joints == null)
            {
                MarkersNames markersMap;
                mp = new MarkersPreprocessor(markerData, out markersMap, bodyPrefix: debug.bodyRig.bodyPrefix);
                joints = new JointLocalization(markersMap);
            }
            Dictionary<string, OpenTK.Vector3> markers;

            if (!mp.ProcessMarkers(markerData, out markers, debug.bodyRig.bodyPrefix))
            {
                UnityEngine.Debug.LogError("Markers (TODO FIX ERROR MESSAGE");
                return;
            }
            var temp = skeleton;
            skeleton = skeletonBuffer;
            skeletonBuffer = temp;
            joints.GetJointLocation(markers, ref skeleton);

            if (ikApplier == null)
            {
                ikApplier = new IKApplier();
            }

            ikApplier.test = debug.bodyRig.Extrapolate;
            ikApplier.ApplyIK(ref skeleton);
            UpdateNext();
        }
        void OnDrawGizmos()
        {
            ShouldWeDraw();
        }
        protected void ShouldWeDraw()
        {
            if (Application.isPlaying && (streaming && markerData != null && markerData.Count > 0) || debug.debugFlag)
            {
                Draw();
            }
        }
        public void Draw()
        {
            if (debug == null) return;
            if (debug.markers.markers)
            {
                foreach (var lb in markerData)
                {
                    Gizmos.color = new Color(lb.Color.r, lb.Color.g, lb.Color.b);
                    Gizmos.DrawSphere(lb.Position + pos, debug.markers.scale);
                }
            }

            if (debug.markers.bones && rtClient.Bones != null)
            {
                foreach (var lb in rtClient.Bones)
                {
                    var from = markerData.Find(md => md.Label == lb.From).Position + pos;
                    var to = markerData.Find(md => md.Label == lb.To).Position + pos;
                    Debug.DrawLine(from, to, debug.markers.boneColor);
            }
            } if (debug.bodyRig != null && skeleton != null &&
                (debug.bodyRig.showSkeleton || debug.bodyRig.showRotationTrace || debug.bodyRig.showJoints))
            {
                Gizmos.color = debug.bodyRig.jointColor;

                foreach (TreeNode<Bone> b in skeleton.Root)
                {
                    if (debug.bodyRig.showSkeleton)
                    {
                        foreach (TreeNode<Bone> child in b.Children)
                        {
                            UnityEngine.Debug.DrawLine(b.Data.Pos.Convert() + pos, child.Data.Pos.Convert() + pos, debug.bodyRig.skelettColor);
                        }
                    }
                    if (debug.bodyRig.showRotationTrace && (!b.IsLeaf))
                    {
                        UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos.Convert() + pos, debug.bodyRig.traceLength);
                    }
                    if (debug.bodyRig.showJoints)
                    {
                        Gizmos.DrawSphere(b.Data.Pos.Convert() + pos, debug.bodyRig.jointScale);
                    }
                }
            }
            if (debug.jointsConstrains != null &&
                (debug.jointsConstrains.showConstraints ||
                debug.jointsConstrains.showTwistConstraints))
            {
                foreach (TreeNode<Bone> b in skeleton.Root)
                {
                    if (b.Data.HasConstraints)
                    {
                        OpenTK.Quaternion parentRotation =
                            b.Parent.Data.Orientation * b.Data.ParentPointer;
                        OpenTK.Vector3 poss = b.Data.Pos + pos.Convert();
                        if (debug.jointsConstrains.showConstraints)
                        {
                            UnityDebug.CreateIrregularCone(
                                b.Data.Constraints, poss,
                                OpenTK.Vector3.NormalizeFast(
                                    OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, parentRotation)),
                                parentRotation,
                                debug.jointsConstrains.coneResolution,
                                debug.jointsConstrains.coneSize);
                        }
                        if (debug.jointsConstrains.showTwistConstraints)
                        {
                            UnityDebug.DrawTwistConstraints(b.Data, b.Parent.Data, poss, debug.bodyRig.traceLength);
                        }
                    }
                }
            }
        }
    }
}

