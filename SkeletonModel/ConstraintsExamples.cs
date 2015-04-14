using OpenTK;
namespace QTM2Unity
{
    class ConstraintsExamples
    {

        public Vector4 Femur = new Vector4(60, 160, 60, 60);
        public Vector2 FemurTwist = new Vector2(315, 45);
        public Vector4 Knee = new Vector4(10, 10, 10, 160);
        public Vector2 KneeTwist = new Vector2(315, 45);
        public Vector4 Ankle = new Vector4(30, 110, 30, 0);
        public Vector2 AnkleTwist = new Vector2(315, 45);
        public Vector4 Spine = new Vector4(20, 20, 20, 20);
        public Vector2 SpineTwist = new Vector2(315, 45);
        public Vector4 Neck = new Vector4(60, 60, 60, 60);
        public Vector2 NeckTwist = new Vector2(270, 90);
        public Vector4 KeyBone = new Vector4(60, 40, 60, 20);
        public Vector2 KeyBoneTwist = new Vector2(355, 5);
        public Vector4 Shoulder = new Vector4(110, 110, 110, 110);
        public Vector2 ShoulderTwist = new Vector2(270, 90);
        public Vector4 Elbow = new Vector4(10, 160, 10, 5);
        public Vector2 ElbowTwist = new Vector2(270, 180);
        public Vector4 Wrist = new Vector4(60, 60, 60, 60);
        public Vector2 WristTwist = new Vector2(350, 10);
        private Vector4 ElbowRight;
        public ConstraintsExamples()
        {
            ElbowRight = new Vector4(Elbow.W, Elbow.Z, Elbow.Y, Elbow.X);
        }
        public void SetConstraints(ref BipedSkeleton skeleton)
        {
            #region Cone constraints
            #region Spine
            skeleton[BipedSkeleton.SPINE0].SetRotationalConstraints(Spine);
            skeleton[BipedSkeleton.SPINE1].SetRotationalConstraints(Spine);
            skeleton[BipedSkeleton.NECK].SetRotationalConstraints(Neck);
            #endregion
            #region Legs
            skeleton[BipedSkeleton.UPPERLEG_L].SetRotationalConstraints(Femur);
            skeleton[BipedSkeleton.UPPERLEG_R].SetRotationalConstraints(Femur);
            skeleton[BipedSkeleton.LOWERLEG_L].SetRotationalConstraints(Knee);
            skeleton[BipedSkeleton.LOWERLEG_R].SetRotationalConstraints(Knee);
            skeleton[BipedSkeleton.FOOT_L].SetRotationalConstraints(Ankle);
            skeleton[BipedSkeleton.FOOT_R].SetRotationalConstraints(Ankle);
            #endregion
            #region Arms
            skeleton[BipedSkeleton.SHOULDER_L].SetRotationalConstraints(KeyBone);
            skeleton[BipedSkeleton.SHOULDER_R].SetRotationalConstraints(KeyBone);
            skeleton[BipedSkeleton.UPPERARM_L].SetRotationalConstraints(Shoulder);
            skeleton[BipedSkeleton.UPPERARM_R].SetRotationalConstraints(Shoulder);
            skeleton[BipedSkeleton.LOWERARM_L].SetRotationalConstraints(Elbow);
            skeleton[BipedSkeleton.LOWERARM_R].SetRotationalConstraints(ElbowRight);
            skeleton[BipedSkeleton.HAND_L].SetRotationalConstraints(Wrist);
            skeleton[BipedSkeleton.HAND_R].SetRotationalConstraints(Wrist);
            #endregion
            #endregion

            #region TwistConstraints
            #region Spine
            skeleton[BipedSkeleton.SPINE0].SetOrientationalConstraints(SpineTwist);
            skeleton[BipedSkeleton.SPINE1].SetOrientationalConstraints(SpineTwist);
            skeleton[BipedSkeleton.NECK].SetOrientationalConstraints(NeckTwist);
            #endregion
            #region Legs
            skeleton[BipedSkeleton.UPPERLEG_L].SetOrientationalConstraints(FemurTwist);
            skeleton[BipedSkeleton.UPPERLEG_R].SetOrientationalConstraints(FemurTwist);
            skeleton[BipedSkeleton.LOWERLEG_L].SetOrientationalConstraints(KneeTwist);
            skeleton[BipedSkeleton.LOWERLEG_R].SetOrientationalConstraints(KneeTwist);
            skeleton[BipedSkeleton.FOOT_L].SetOrientationalConstraints(AnkleTwist);
            skeleton[BipedSkeleton.FOOT_R].SetOrientationalConstraints(AnkleTwist);
            #endregion
            #region Arms
            skeleton[BipedSkeleton.SHOULDER_L].SetOrientationalConstraints(KeyBoneTwist);
            skeleton[BipedSkeleton.SHOULDER_R].SetOrientationalConstraints(KeyBoneTwist);
            skeleton[BipedSkeleton.UPPERARM_L].SetOrientationalConstraints(ShoulderTwist);
            skeleton[BipedSkeleton.UPPERARM_R].SetOrientationalConstraints(ShoulderTwist);
            skeleton[BipedSkeleton.LOWERARM_L].SetOrientationalConstraints(ElbowTwist);
            skeleton[BipedSkeleton.LOWERARM_R].SetOrientationalConstraints(ElbowTwist);
            skeleton[BipedSkeleton.HAND_L].SetOrientationalConstraints(WristTwist);
            skeleton[BipedSkeleton.HAND_R].SetOrientationalConstraints(WristTwist);
            #endregion
            #endregion
        }

    }
}
