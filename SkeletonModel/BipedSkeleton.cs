using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity.SkeletonModel
{
    class BipedSkeleton : Skeleton
    {
        // String constants for bones in a biped skeleton
        // (or should we model it as an enumeration or something?)
        // TODO do we want to model the skeleton like this?
        // How many spines do we want, and do we want to model neck, toes etc?
        // Something to experiment with
        public const string PELVIS      = "pelvis";
        public const string UPPERLEG_L  = "upperLeg_L";
        public const string LOWERLEG_L  = "lowerLeg_L";
        public const string FOOT_L      = "foot_L";
        public const string TOE_L       = "toe_L";
        public const string UPPERLEG_R  = "upperLeg_R";
        public const string LOWERLEG_R  = "lowerLeg_R";
        public const string FOOT_R      = "foot_R";
        public const string TOE_R       = "toe_R";
        public const string SPINE0      = "spine0";
        public const string SPINE1      = "spine1";
        public const string SPINE2      = "spine2";
        public const string SPINE3      = "spine3";
        public const string NECK        = "neck";
        public const string HEAD        = "head";
        public const string SHOULDER_L  = "shoulder_L";
        public const string UPPERARM_L  = "upperArm_L";
        public const string LOWERARM_L  = "lowerArm_L";
        public const string HAND_L      = "hand_L";
        public const string SHOULDER_R  = "shoulder_R";
        public const string UPPERARM_R  = "upperArm_R";
        public const string LOWERARM_R  = "lowerArm_R";
        public const string HAND_R      = "hand_R";

        // TODO should we restrict BipedSkeleton to only contain above names?
        public BipedSkeleton(List<Bone> bones)
        {
            this.bones = bones;
        }

    }
}
