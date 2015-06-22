using OpenTK;
namespace QTM2Unity
{
    class ConstraintsExamples
    {
        //Vector4(blue, red, green, yellow);
        public Vector4 Femur = new Vector4(15, 150, 40, 55);
        public Vector4 Knee = new Vector4(15, 0, 15, 160);
        //public Vector4 Ankle = new Vector4(30, 0, 60, 60);
        public Vector4 Ankle = new Vector4(60, 0, 30, 60);

        public Vector2 FemurTwist = new Vector2(335, 25);
        public Vector2 KneeTwist = new Vector2(330, 30);
        public Vector2 AnkleTwist = new Vector2(340, 20);

        public Vector4 Spine = new Vector4(10, 40, 10, 20);
        public Vector2 SpineTwist = new Vector2(350, 10);
        public Vector4 Neck = new Vector4(60, 85, 60, 85); // right, front, left, back
        public Vector2 NeckTwist = new Vector2(270, 90);

        public Vector4 Clavicula = new Vector4(15, 40, 30, 15); //down, front, up, back
        public Vector4 Shoulder = new Vector4(80, 100, 120, 70); // down, front, up, back 
        public Vector4 Elbow = new Vector4(10, 175, 10, 5); // Adduktion, , Abduktion 
        public Vector4 Wrist = new Vector4(75, 45, 85, 45); //dorsalflexion, radialflexion, palmarflexion, ulnarflexion
        public Vector2 ClaviculaTwist = new Vector2(350, 10);
        public Vector2 ShoulderTwist = new Vector2(280, 80);
        public Vector2 ElbowTwist = new Vector2(300, 60);
        public Vector2 WristTwist = new Vector2(350, 10);

        private Vector2 NoTwist = new Vector2(359, 1);
        private Vector4 NoMovment = new Vector4(1,1,1,1);
        private float verylight = 1.2f;
        private float light = 1.1f;
        private float heavy = 0.9f;
        private float veryheavy = 0.8f;
        
        public void SetConstraints(ref BipedSkeleton skeleton)
        {
            skeleton[BipedSkeleton.SPINE3].Constraints = (NoMovment);
            skeleton[BipedSkeleton.SPINE3].TwistLimit = (NoTwist);
            #region Cone constraints
            #region Spine too head
            skeleton[BipedSkeleton.SPINE0].Constraints = (Spine);
            skeleton[BipedSkeleton.SPINE1].Constraints = (Spine);
            skeleton[BipedSkeleton.NECK].Constraints = (Neck);
            #endregion
            #region Legs
            skeleton[BipedSkeleton.UPPERLEG_L].Constraints = (SwapXZ(Femur));
            skeleton[BipedSkeleton.UPPERLEG_R].Constraints = (Femur);
            skeleton[BipedSkeleton.LOWERLEG_L].Constraints = (SwapXZ(Knee));
            skeleton[BipedSkeleton.LOWERLEG_R].Constraints = (Knee);
            skeleton[BipedSkeleton.FOOT_L].Constraints = (SwapXZ(Ankle));
            skeleton[BipedSkeleton.FOOT_R].Constraints = (Ankle);
            #endregion
            #region Arms
            skeleton[BipedSkeleton.SHOULDER_L].Constraints = (SwapXZ(Clavicula));
            skeleton[BipedSkeleton.SHOULDER_R].Constraints = (Clavicula);
            skeleton[BipedSkeleton.UPPERARM_L].Constraints = (SwapXZ(Shoulder));
            skeleton[BipedSkeleton.UPPERARM_R].Constraints = (Shoulder);
            skeleton[BipedSkeleton.LOWERARM_L].Constraints = (SwapXZ(Elbow));
            skeleton[BipedSkeleton.LOWERARM_R].Constraints = (Elbow);
            skeleton[BipedSkeleton.HAND_L].Constraints = (SwapXZ(Wrist));
            skeleton[BipedSkeleton.HAND_R].Constraints = (Wrist);
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
            skeleton[BipedSkeleton.SPINE0].TwistLimit = (SpineTwist);
            skeleton[BipedSkeleton.SPINE1].TwistLimit = (SpineTwist);
            skeleton[BipedSkeleton.NECK].TwistLimit = (NeckTwist);
            #endregion
            #region Legs
            skeleton[BipedSkeleton.UPPERLEG_L].TwistLimit = (FemurTwist);
            skeleton[BipedSkeleton.UPPERLEG_R].TwistLimit = (FemurTwist);
            skeleton[BipedSkeleton.LOWERLEG_L].TwistLimit = (KneeTwist);
            skeleton[BipedSkeleton.LOWERLEG_R].TwistLimit = (KneeTwist);
            skeleton[BipedSkeleton.FOOT_L].TwistLimit = (AnkleTwist);
            skeleton[BipedSkeleton.FOOT_R].TwistLimit = (AnkleTwist);
            #endregion
            #region Arms
            skeleton[BipedSkeleton.SHOULDER_L].TwistLimit = (ClaviculaTwist);
            skeleton[BipedSkeleton.UPPERARM_L].TwistLimit = (ShoulderTwist);
            skeleton[BipedSkeleton.LOWERARM_L].TwistLimit = (ElbowTwist);
            skeleton[BipedSkeleton.HAND_L].TwistLimit = (WristTwist);
            skeleton[BipedSkeleton.SHOULDER_R].TwistLimit = (ClaviculaTwist);
            skeleton[BipedSkeleton.UPPERARM_R].TwistLimit = (ShoulderTwist);
            skeleton[BipedSkeleton.LOWERARM_R].TwistLimit = (ElbowTwist);
            skeleton[BipedSkeleton.HAND_R].TwistLimit = (WristTwist);
            #endregion
            #endregion
            #region weights
      /*
        */
            #region Arms
            skeleton[BipedSkeleton.SHOULDER_L].Weight = (veryheavy);
            //skeleton[BipedSkeleton.UPPERARM_L].Weight = (heavy);
            skeleton[BipedSkeleton.LOWERARM_L].Weight = (light);
            skeleton[BipedSkeleton.HAND_L].Weight = (heavy);
            skeleton[BipedSkeleton.SHOULDER_R].Weight = (veryheavy);
            //skeleton[BipedSkeleton.UPPERARM_R].Weight = (heavy);
            skeleton[BipedSkeleton.LOWERARM_R].Weight = (light);
            skeleton[BipedSkeleton.HAND_R].Weight = (veryheavy);
            #endregion
            #region Legs
            skeleton[BipedSkeleton.UPPERLEG_L].Weight = verylight;
            skeleton[BipedSkeleton.UPPERLEG_R].Weight = (verylight);
            skeleton[BipedSkeleton.LOWERLEG_L].Weight = (light);
            skeleton[BipedSkeleton.LOWERLEG_R].Weight = (light);
            skeleton[BipedSkeleton.FOOT_L].Weight = (veryheavy);
            skeleton[BipedSkeleton.FOOT_R].Weight = (veryheavy);
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
