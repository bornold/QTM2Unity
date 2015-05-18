using OpenTK;
namespace QTM2Unity
{
    class ConstraintsExamples
    {
        //Vector4(blue, red, green, yellow);
        public Vector4 Femur = new Vector4(15, 150, 40, 55);
        public Vector4 Knee = new Vector4(5, 0, 5, 160);
        public Vector4 Ankle = new Vector4(7, 0, 7, 60);
        public Vector2 FemurTwist = new Vector2(345, 15);
        public Vector2 KneeTwist = new Vector2(345, 15);
        public Vector2 AnkleTwist = new Vector2(320, 40);

        public Vector4 Spine = new Vector4(20, 30, 20, 20);
        public Vector2 SpineTwist = new Vector2(350, 10);
        public Vector4 Neck = new Vector4(60, 85, 60, 85); // right, front, left, back
        public Vector2 NeckTwist = new Vector2(270, 90);

        public Vector4 Clavicula = new Vector4(15, 40, 50, 10); //down, front, up, back
        public Vector4 Shoulder = new Vector4(100, 120, 120, 70); // down, front, up, back 
        public Vector4 Elbow = new Vector4(10, 175, 10, 5); // Adduktion, , Abduktion 
        public Vector4 Wrist = new Vector4(75, 45, 85, 45); //dorsalflexion, radialflexion, palmarflexion, ulnarflexion
        public Vector2 ClaviculaTwist = new Vector2(355, 5);
        public Vector2 ShoulderTwist = new Vector2(330, 30);
        public Vector2 ElbowTwist = new Vector2(300, 60);
        public Vector2 WristTwist = new Vector2(350, 10);

        private Vector2 NoTwist = new Vector2(359, 1);
        private Vector4 NoMovment = new Vector4(1,1,1,1);

        public void SetConstraints(ref BipedSkeleton skeleton)
        {
            skeleton[BipedSkeleton.SPINE3].SetRotationalConstraints(NoMovment);
            skeleton[BipedSkeleton.SPINE3].SetOrientationalConstraints(NoTwist);
            #region Cone constraints
            #region Spine too head
            skeleton[BipedSkeleton.SPINE0].SetRotationalConstraints(Spine);
            skeleton[BipedSkeleton.SPINE1].SetRotationalConstraints(Spine);
            skeleton[BipedSkeleton.NECK].SetRotationalConstraints(Neck);
            #endregion
            #region Legs
            skeleton[BipedSkeleton.UPPERLEG_L].SetRotationalConstraints(SwapXZ(Femur));
            skeleton[BipedSkeleton.UPPERLEG_R].SetRotationalConstraints(Femur);
            skeleton[BipedSkeleton.LOWERLEG_L].SetRotationalConstraints(SwapXZ(Knee));
            skeleton[BipedSkeleton.LOWERLEG_R].SetRotationalConstraints(Knee);
            skeleton[BipedSkeleton.FOOT_L].SetRotationalConstraints(SwapXZ(Ankle));
            skeleton[BipedSkeleton.FOOT_R].SetRotationalConstraints(Ankle);
            #endregion
            #region Arms
            skeleton[BipedSkeleton.SHOULDER_L].SetRotationalConstraints(SwapXZ(Clavicula));
            skeleton[BipedSkeleton.SHOULDER_R].SetRotationalConstraints(Clavicula);
            skeleton[BipedSkeleton.UPPERARM_L].SetRotationalConstraints(SwapXZ(Shoulder));
            skeleton[BipedSkeleton.UPPERARM_R].SetRotationalConstraints(Shoulder);
            skeleton[BipedSkeleton.LOWERARM_L].SetRotationalConstraints(SwapXZ(Elbow));
            skeleton[BipedSkeleton.LOWERARM_R].SetRotationalConstraints(Elbow);
            skeleton[BipedSkeleton.HAND_L].SetRotationalConstraints(SwapXZ(Wrist));
            skeleton[BipedSkeleton.HAND_R].SetRotationalConstraints(Wrist);
            #endregion
            #endregion

            #region ParentPointers
            skeleton[BipedSkeleton.SHOULDER_R].ParentPointer = QuaternionHelper.RotationZ(-MathHelper.PiOver2);
            skeleton[BipedSkeleton.SHOULDER_L].ParentPointer = QuaternionHelper.RotationZ(MathHelper.PiOver2);
            skeleton[BipedSkeleton.UPPERLEG_R].ParentPointer = QuaternionHelper.RotationZ(MathHelper.Pi);
            skeleton[BipedSkeleton.UPPERLEG_L].ParentPointer = QuaternionHelper.RotationZ(MathHelper.Pi);
            skeleton[BipedSkeleton.FOOT_L].ParentPointer = QuaternionHelper.RotationX(MathHelper.PiOver2);
            skeleton[BipedSkeleton.FOOT_R].ParentPointer = QuaternionHelper.RotationX(MathHelper.PiOver2);
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
            skeleton[BipedSkeleton.SHOULDER_L].SetOrientationalConstraints(ClaviculaTwist);
            skeleton[BipedSkeleton.SHOULDER_R].SetOrientationalConstraints(ClaviculaTwist);
            skeleton[BipedSkeleton.UPPERARM_L].SetOrientationalConstraints(ShoulderTwist);
            skeleton[BipedSkeleton.UPPERARM_R].SetOrientationalConstraints(ShoulderTwist);
            skeleton[BipedSkeleton.LOWERARM_L].SetOrientationalConstraints(ElbowTwist);
            skeleton[BipedSkeleton.LOWERARM_R].SetOrientationalConstraints(ElbowTwist);
            skeleton[BipedSkeleton.HAND_L].SetOrientationalConstraints(WristTwist);
            skeleton[BipedSkeleton.HAND_R].SetOrientationalConstraints(WristTwist);
            #endregion
            #endregion
        }
        private Vector4 SwapXZ(Vector4 v)
        {
            return new Vector4(v.Z, v.Y, v.X, v.W);
        }
        private Vector4 SwapXZYW(Vector4 v)
        {
            return new Vector4(v.Z, v.W, v.X, v.Y);
        }
        private Vector4 SwapYW(Vector4 v)
        {
            return new Vector4(v.X, v.W, v.Z, v.Y);
        }
        private Vector2 SwapXY(Vector2 v)
        {
            return new Vector2(v.Y, v.X);
        }
    }
}
