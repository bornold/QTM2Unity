using System.Collections.Generic;

namespace QualisysRealTime.Unity.Skeleton
{
    class SkeletonBuilder
    {
        private RTClient rtClient;
        private BipedSkeleton skeleton;
        private BipedSkeleton skeletonBuffer;
        private MarkersPreprocessor mp;
        private JointLocalization joints;
        private IKApplier ikApplier;
        private string MarkerPrefix;
        public bool SolveWithIK = true;
        public bool Interpolation = false;
        public SkeletonBuilder(RTClient rtClient, string markerPrefix)
        {
            this.rtClient = rtClient;
            MarkerPrefix = markerPrefix;
        }
        public BipedSkeleton SolveSkeleton(List<LabeledMarker> markerData)
        {
            if (   skeleton == null
                || skeletonBuffer == null
                || mp == null
                || joints == null
                || ikApplier == null)
            {
                skeleton = new BipedSkeleton();
                skeletonBuffer = new BipedSkeleton();
                MarkersNames markersMap;
                mp = new MarkersPreprocessor(markerData, out markersMap, bodyPrefix: MarkerPrefix); ;
                joints = new JointLocalization(markersMap);
                ikApplier = new IKApplier();
            }
            Dictionary<string, OpenTK.Vector3> markers;
            mp.ProcessMarkers(markerData, out markers, MarkerPrefix);
            var temp = skeleton;
            skeleton = skeletonBuffer;
            skeletonBuffer = temp;
            joints.GetJointLocation(markers, ref skeleton);
            ikApplier.Interpolation = Interpolation;
            if (SolveWithIK) ikApplier.ApplyIK(ref skeleton);
            return skeleton;
        }
    }
}
