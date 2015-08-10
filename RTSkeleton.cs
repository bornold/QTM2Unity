#region --- LINCENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System.Collections.Generic;
using QualisysRealTime.Unity;
using UnityEngine;
#pragma warning disable 0649

namespace QualisysRealTime.Unity.Skeleton
{
 /// <summary>
 /// Calculates a skeleton and contains debug for viewing the skeleton
 /// NOTICE: This does not include mapping to a Unity Character
/// </summary>
    class RTSkeleton : MonoBehaviour
    {
        public Debugging debug;
        public string MarkerPrefix = "";
        public bool LockPosition = false;
        public bool ResetSkeleton = false;

        protected RTClient rtClient;
        protected Vector3 pos;
        protected bool streaming = false;
        protected List<LabeledMarker> markerData;
        public virtual void StartNext() { }
        public virtual void UpdateNext() { }

        protected BipedSkeleton skeleton;
        protected BipedSkeleton skeletonBuffer;
        private MarkersPreprocessor mp;
        private JointLocalization joints;
        private IKApplier ikApplier;
        void Start()
        {
            rtClient = RTClient.GetInstance();
            skeleton = new BipedSkeleton();
            skeletonBuffer = new BipedSkeleton();
            ikApplier = new IKApplier();
            StartNext();
        }
        /// <summary>
        /// Updates ones per frame, sets the joints position of the character
        /// </summary>
        void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();
            streaming = rtClient.GetStreamingStatus();
            if (!streaming && !LockPosition) return;
            markerData = rtClient.Markers;
            if ((markerData == null || markerData.Count == 0) && !LockPosition) 
            {
                UnityEngine.Debug.LogWarning("Stream does not contain any markers");
                return;
            }
            if (ResetSkeleton
                || skeleton == null
                || skeletonBuffer == null
                || mp == null
                || joints == null
                || ikApplier == null)
            {
                UnityEngine.Debug.LogWarning("Reseting");
                skeleton = new BipedSkeleton();
                skeletonBuffer = new BipedSkeleton();
                MarkersNames markersMap;
                mp = new MarkersPreprocessor(markerData, out markersMap, bodyPrefix: MarkerPrefix);;
                joints = new JointLocalization(markersMap);
                ikApplier = new IKApplier();
                ResetSkeleton = false;
            }
            if (LockPosition)
            {
                UpdateNext();
                return;
            }
            Dictionary<string, OpenTK.Vector3> markers;
            mp.ProcessMarkers(markerData, out markers, MarkerPrefix);
            var temp = skeleton;
            skeleton = skeletonBuffer;
            skeletonBuffer = temp;
            joints.GetJointLocation(markers, ref skeleton);
            ikApplier.ApplyIK(ref skeleton);
            UpdateNext();
        }
        void OnDrawGizmos()
        {
            if (Application.isPlaying && (streaming && markerData != null && markerData.Count > 0) || LockPosition)
            {
                Draw();
            }
        }
        private void Draw()
        {
            if (debug == null) return;
            pos = this.transform.position + debug.Offset;
            if (debug.markers.ShowMarkers)
            {
                foreach (var lb in markerData)
                {
                    Gizmos.color = new Color(lb.Color.r, lb.Color.g, lb.Color.b);
                    Gizmos.DrawSphere(lb.Position + pos, debug.markers.MarkerScale);
                }
            }

            if (debug.markers.MarkerBones && rtClient.Bones != null)
            {
                foreach (var lb in rtClient.Bones)
                {
                    var from = markerData.Find(md => md.Label == lb.From).Position + pos;
                    var to = markerData.Find(md => md.Label == lb.To).Position + pos;
                    Debug.DrawLine(from, to, debug.markers.boneColor);
                }
            }
            if (skeleton == null) return;
            if (debug.showSkeleton || debug.showRotationTrace || debug.showJoints || debug.showConstraints || debug.showTwistConstraints)
            {
                Gizmos.color = debug.jointColor;

                foreach (TreeNode<Bone> b in skeleton.Root)
                {
                    if (debug.showSkeleton)
                    {
                        foreach (TreeNode<Bone> child in b.Children)
                        {
                            UnityEngine.Debug.DrawLine(b.Data.Pos.Convert() + pos, child.Data.Pos.Convert() + pos, debug.skelettColor);
                        }
                    }
                    if (debug.showRotationTrace && (!b.IsLeaf))
                    {
                        UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos.Convert() + pos, debug.traceLength);
                    }
                    if (debug.showJoints)
                    {
                        Gizmos.DrawSphere(b.Data.Pos.Convert() + pos, debug.jointSize);
                    }
                    if ((debug.showConstraints || debug.showTwistConstraints) && b.Data.HasConstraints)
                    {
                        OpenTK.Quaternion parentRotation =
                            b.Parent.Data.Orientation * b.Data.ParentPointer;
                        OpenTK.Vector3 poss = b.Data.Pos + pos.Convert();
                        if (debug.showConstraints)
                        {
                            UnityDebug.CreateIrregularCone(
                                b.Data.Constraints, poss,
                                OpenTK.Vector3.NormalizeFast(
                                    OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, parentRotation)),
                                parentRotation,
                                50,//debug.jointsConstrains.coneResolution,
                                debug.traceLength//debug.jointsConstrains.coneSize
                                );
                        }
                        if (debug.showTwistConstraints)
                        {
                            UnityDebug.DrawTwistConstraints(b.Data, b.Parent.Data, poss, debug.traceLength);
                        }
                    }
                }
            }
        }
    }
}

