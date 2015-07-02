using OpenTK;
namespace QTM2Unity
{
    class ConstraintsExamples
    {

        private Vector2 NoTwist = new Vector2(359, 1);
        private Vector4 NoMovment = new Vector4(1,1,1,1);
        //Vector4(blue, red, green, yellow);
        public Vector4 Femur = new Vector4(15, 150, 40, 55);
        public Vector4 Knee = new Vector4(15, 0, 15, 160);
        public Vector4 Ankle = new Vector4(45, 45, 45, 45);
        public Vector4 FootBase = new Vector4(20, 10, 30, 10);

        public Vector2 FemurTwist = new Vector2(335, 25);
        public Vector2 KneeTwist = new Vector2(320, 40);
        public Vector2 AnkleTwist = new Vector2(355, 5);
        public Vector2 FootBaseTwist = new Vector2(358, 2);

        public Vector4 Spine = new Vector4(20, 30, 20, 20);
        public Vector2 SpineTwist = new Vector2(340, 20);
        public Vector4 Neck = new Vector4(60, 85, 60, 85); // right, front, left, back
        public Vector2 NeckTwist = new Vector2(270, 90);

        public Vector4 Clavicula = new Vector4(15, 40, 30, 15); //down, front, up, back
        public Vector4 Shoulder = new Vector4(80, 100, 120, 70); // down, front, up, back 
        public Vector4 Elbow = new Vector4(10, 175, 10, 5); // Adduktion, , Abduktion 
        public Vector4 Wrist = new Vector4(75, 45, 85, 45); //dorsalflexion, radialflexion, palmarflexion, ulnarflexion
        public Vector2 ClaviculaTwist = new Vector2(350, 10);
        public Vector2 ShoulderTwist = new Vector2(280, 80);
        public Vector2 ElbowTwist = new Vector2(300, 60);
        public Vector2 WristTwist = new Vector2(345, 15);
        private float veryagile = 1.2f;
        private float agile = 1.1f;
    
        //private float stiffer = 0.8f;
        private float verystiff = 0.7f;
        //private float extremlystiff = 0.6f;
        private float barelymoving = 0.5f;
        
        public void SetConstraints(ref BipedSkeleton skeleton)
        {
            //skeleton[BipedSkeleton.SPINE3].Constraints = (NoMovment);
            skeleton[BipedSkeleton.SPINE3].TwistLimit = (NoTwist);
            #region Cone constraints
            #region Spine too head
            skeleton[BipedSkeleton.SPINE0].Constraints = (Spine);
            skeleton[BipedSkeleton.SPINE1].Constraints = (Spine);
            skeleton[BipedSkeleton.NECK].Constraints = (Neck);
            #endregion
            #region Legs
            skeleton[BipedSkeleton.HIP_L].Constraints = (SwapXZ(Femur));
            skeleton[BipedSkeleton.HIP_R].Constraints = (Femur);
            skeleton[BipedSkeleton.KNEE_L].Constraints = (SwapXZ(Knee));
            skeleton[BipedSkeleton.KNEE_R].Constraints = (Knee);
            skeleton[BipedSkeleton.ANKLE_L].Constraints = (SwapXZ(Ankle));
            skeleton[BipedSkeleton.ANKLE_R].Constraints = (Ankle);
            skeleton[BipedSkeleton.FOOTBASE_L].Constraints = (SwapXZ(FootBase));
            skeleton[BipedSkeleton.FOOTBASE_R].Constraints = (FootBase);
            #endregion
            #region Arms
            skeleton[BipedSkeleton.CLAVICLE_L].Constraints = (SwapXZ(Clavicula));
            skeleton[BipedSkeleton.CLAVICLE_R].Constraints = (Clavicula);
            skeleton[BipedSkeleton.SHOULDER_L].Constraints = (SwapXZ(Shoulder));
            skeleton[BipedSkeleton.SHOULDER_R].Constraints = (Shoulder);
            skeleton[BipedSkeleton.ELBOW_L].Constraints = (SwapXZ(Elbow));
            skeleton[BipedSkeleton.ELBOW_R].Constraints = (Elbow);
            skeleton[BipedSkeleton.WRIST_L].Constraints = (SwapXZ(Wrist));
            skeleton[BipedSkeleton.WRIST_R].Constraints = (Wrist);
            #endregion
            #endregion

            #region ParentPointers
            skeleton[BipedSkeleton.CLAVICLE_R].ParentPointer = QuaternionHelper.RotationZ(-MathHelper.PiOver2);
            skeleton[BipedSkeleton.CLAVICLE_L].ParentPointer = QuaternionHelper.RotationZ(MathHelper.PiOver2);
            skeleton[BipedSkeleton.HIP_R].ParentPointer = QuaternionHelper.RotationZ(MathHelper.Pi);
            skeleton[BipedSkeleton.HIP_L].ParentPointer = QuaternionHelper.RotationZ(MathHelper.Pi);
            skeleton[BipedSkeleton.ANKLE_R].ParentPointer = QuaternionHelper.RotationX(MathHelper.PiOver4) * QuaternionHelper.RotationZ(-MathHelper.PiOver4);
            skeleton[BipedSkeleton.ANKLE_L].ParentPointer = QuaternionHelper.RotationX(MathHelper.PiOver4) * QuaternionHelper.RotationZ(MathHelper.PiOver4);
            skeleton[BipedSkeleton.FOOTBASE_L].ParentPointer = QuaternionHelper.RotationX(MathHelper.PiOver4) *
                QuaternionHelper.RotationZ(-MathHelper.PiOver4);// QuaternionHelper.RotationX(MathHelper.Pi + MathHelper.PiOver6);
                skeleton[BipedSkeleton.FOOTBASE_R].ParentPointer = QuaternionHelper.RotationX(MathHelper.PiOver4) *
                QuaternionHelper.RotationZ(MathHelper.PiOver4);//QuaternionHelper.RotationX(MathHelper.Pi + MathHelper.PiOver6);
            #endregion

            #region TwistConstraints
            #region Spine
            skeleton[BipedSkeleton.SPINE0].TwistLimit = (SpineTwist);
            skeleton[BipedSkeleton.SPINE1].TwistLimit = (SpineTwist);
            skeleton[BipedSkeleton.NECK].TwistLimit = (NeckTwist);
            #endregion
            #region Legs
            skeleton[BipedSkeleton.HIP_L].TwistLimit = (FemurTwist);
            skeleton[BipedSkeleton.HIP_R].TwistLimit = (FemurTwist);
            skeleton[BipedSkeleton.KNEE_L].TwistLimit = (KneeTwist);
            skeleton[BipedSkeleton.KNEE_R].TwistLimit = (KneeTwist);
            skeleton[BipedSkeleton.ANKLE_L].TwistLimit = (AnkleTwist);
            skeleton[BipedSkeleton.ANKLE_R].TwistLimit = (AnkleTwist);

            skeleton[BipedSkeleton.FOOTBASE_L].TwistLimit = (FootBaseTwist);
            skeleton[BipedSkeleton.FOOTBASE_R].TwistLimit = (FootBaseTwist);
            #endregion
            #region Arms
            skeleton[BipedSkeleton.CLAVICLE_L].TwistLimit = (ClaviculaTwist);
            skeleton[BipedSkeleton.SHOULDER_L].TwistLimit = (ShoulderTwist);
            skeleton[BipedSkeleton.ELBOW_L].TwistLimit = (ElbowTwist);
            skeleton[BipedSkeleton.WRIST_L].TwistLimit = (WristTwist);
            skeleton[BipedSkeleton.CLAVICLE_R].TwistLimit = (ClaviculaTwist);
            skeleton[BipedSkeleton.SHOULDER_R].TwistLimit = (ShoulderTwist);
            skeleton[BipedSkeleton.ELBOW_R].TwistLimit = (ElbowTwist);
            skeleton[BipedSkeleton.WRIST_R].TwistLimit = (WristTwist);
            #endregion
            #endregion
            #region stiffness
      /*
        */
            #region Arms
            skeleton[BipedSkeleton.CLAVICLE_L].Stiffness = (verystiff);
            //skeleton[BipedSkeleton.UPPERARM_L].Weight = (heavy);
            skeleton[BipedSkeleton.CLAVICLE_R].Stiffness = (verystiff);
            //skeleton[BipedSkeleton.UPPERARM_R].Weight = (heavy);
            skeleton[BipedSkeleton.ELBOW_L].Stiffness = (agile);
            skeleton[BipedSkeleton.ELBOW_R].Stiffness = (agile);
            skeleton[BipedSkeleton.WRIST_L].Stiffness = (barelymoving);
            skeleton[BipedSkeleton.WRIST_R].Stiffness = (barelymoving);
            #endregion
            #region Legs
            skeleton[BipedSkeleton.HIP_L].Stiffness = veryagile;
            skeleton[BipedSkeleton.HIP_R].Stiffness = (veryagile);
            skeleton[BipedSkeleton.KNEE_L].Stiffness = (agile);
            skeleton[BipedSkeleton.KNEE_R].Stiffness = (agile);
            skeleton[BipedSkeleton.ANKLE_L].Stiffness = (barelymoving);
            skeleton[BipedSkeleton.ANKLE_R].Stiffness = (barelymoving);
            skeleton[BipedSkeleton.FOOTBASE_L].Stiffness = (barelymoving);
            skeleton[BipedSkeleton.FOOTBASE_R].Stiffness = (barelymoving);
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
