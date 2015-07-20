using OpenTK;
namespace QTM2Unity
{
    class ConstraintsExamples
    {

        private Vector2 NoTwist = new Vector2(359, 1);
        //private Vector4 NoMovment = new Vector4(1,1,1,1);
        //Vector4(blue, red, green, yellow);
        public Vector4 Femur = new Vector4(15, 150, 40, 55);
        public Vector4 Knee = new Vector4(15, 0, 15, 160);
        public Vector4 Ankle = new Vector4(10, 30, 10, 30);
        public Vector4 FootBase = new Vector4(20, 10, 30, 10);

        //TWIST(Yellow,magenta);
        public Vector2 FemurTwist = new Vector2(335, 25);
        public Vector2 KneeTwist = new Vector2(320, 40);
        public Vector2 AnkleTwist = new Vector2(345, 15);
        public Vector2 FootBaseTwist = new Vector2(350, 10);

        public Vector4 Spine = new Vector4(20, 30, 20, 20);
        public Vector2 SpineTwist = new Vector2(340, 20);
        public Vector4 Neck = new Vector4(60, 85, 60, 85); // right, front, left, back
        public Vector2 NeckTwist = new Vector2(270, 90);

        public Vector4 Clavicula = new Vector4(15, 40, 30, 15); //down, front, up, back
        public Vector4 Shoulder = new Vector4(80, 95, 120, 120); // down, front, up, back 
        public Vector4 Elbow = new Vector4(10, 175, 10, 5); // Adduktion, , Abduktion 
        public Vector4 Wrist = new Vector4(75, 45, 85, 45); //dorsalflexion, radialflexion, palmarflexion, ulnarflexion
        public Vector2 ClaviculaTwist = new Vector2(350, 10);
        public Vector2 ShoulderTwist = new Vector2(260, 40);
        public Vector2 ElbowTwist = new Vector2(300, 60);
        public Vector2 WristTwist = new Vector2(345, 15);


        private float stiff = 0.7f;
        private float verystiff = 0.5f;
        private float barelymoving = 0.3f;
        
        public void SetConstraints(BipedSkeleton skeleton)
        {
            //skeleton[Joint.SPINE3].Constraints = (NoMovment);
            skeleton[Joint.SPINE3].TwistLimit = (NoTwist);
            #region Cone constraints
            #region Spine too head
            skeleton[Joint.SPINE0].Constraints = (Spine);
            skeleton[Joint.SPINE1].Constraints = (Spine);
            skeleton[Joint.NECK].Constraints = (Neck);
            #endregion
            #region Legs
            skeleton[Joint.HIP_L].Constraints = (SwapXZ(Femur));
            skeleton[Joint.HIP_R].Constraints = (Femur);
            skeleton[Joint.KNEE_L].Constraints = (SwapXZ(Knee));
            skeleton[Joint.KNEE_R].Constraints = (Knee);
            skeleton[Joint.ANKLE_L].Constraints = (SwapXZ(Ankle));
            skeleton[Joint.ANKLE_R].Constraints = (Ankle);
            skeleton[Joint.FOOTBASE_L].Constraints = (SwapXZ(FootBase));
            skeleton[Joint.FOOTBASE_R].Constraints = (FootBase);
            #endregion
            #region Arms
            skeleton[Joint.CLAVICLE_L].Constraints = (SwapXZ(Clavicula));
            skeleton[Joint.CLAVICLE_R].Constraints = (Clavicula);
            skeleton[Joint.SHOULDER_L].Constraints = (SwapXZ(Shoulder));
            skeleton[Joint.SHOULDER_R].Constraints = (Shoulder);
            skeleton[Joint.ELBOW_L].Constraints = (SwapXZ(Elbow));
            skeleton[Joint.ELBOW_R].Constraints = (Elbow);
            skeleton[Joint.WRIST_L].Constraints = (SwapXZ(Wrist));
            skeleton[Joint.WRIST_R].Constraints = (Wrist);
            #endregion
            #endregion

            #region ParentPointers
            skeleton[Joint.CLAVICLE_R].ParentPointer = QuaternionHelper.RotationZ(-MathHelper.PiOver2);
            skeleton[Joint.CLAVICLE_L].ParentPointer = QuaternionHelper.RotationZ(MathHelper.PiOver2);
            skeleton[Joint.HIP_R].ParentPointer = QuaternionHelper.RotationZ(MathHelper.Pi);
            skeleton[Joint.HIP_L].ParentPointer = QuaternionHelper.RotationZ(MathHelper.Pi);
            skeleton[Joint.ANKLE_R].ParentPointer = QuaternionHelper.RotationX(MathHelper.PiOver4) * QuaternionHelper.RotationZ(-MathHelper.PiOver4);
            skeleton[Joint.ANKLE_L].ParentPointer = QuaternionHelper.RotationX(MathHelper.PiOver4) * QuaternionHelper.RotationZ(MathHelper.PiOver4);
            skeleton[Joint.FOOTBASE_L].ParentPointer = QuaternionHelper.RotationX(MathHelper.PiOver4) *
                QuaternionHelper.RotationZ(-MathHelper.PiOver4);// QuaternionHelper.RotationX(MathHelper.Pi + MathHelper.PiOver6);
                skeleton[Joint.FOOTBASE_R].ParentPointer = QuaternionHelper.RotationX(MathHelper.PiOver4) *
                QuaternionHelper.RotationZ(MathHelper.PiOver4);//QuaternionHelper.RotationX(MathHelper.Pi + MathHelper.PiOver6);
            #endregion

            #region TwistConstraints
            #region Spine
            skeleton[Joint.SPINE0].TwistLimit = (SpineTwist);
            skeleton[Joint.SPINE1].TwistLimit = (SpineTwist);
            skeleton[Joint.NECK].TwistLimit = (NeckTwist);
            #endregion
            #region Legs
            skeleton[Joint.HIP_L].TwistLimit = (FemurTwist);
            skeleton[Joint.HIP_R].TwistLimit = (FemurTwist);
            skeleton[Joint.KNEE_L].TwistLimit = (KneeTwist);
            skeleton[Joint.KNEE_R].TwistLimit = (KneeTwist);
            skeleton[Joint.ANKLE_L].TwistLimit = (AnkleTwist);
            skeleton[Joint.ANKLE_R].TwistLimit = (AnkleTwist);

            skeleton[Joint.FOOTBASE_L].TwistLimit = (FootBaseTwist);
            skeleton[Joint.FOOTBASE_R].TwistLimit = (FootBaseTwist);
            #endregion
            #region Arms
            skeleton[Joint.CLAVICLE_L].TwistLimit = (ClaviculaTwist);
            skeleton[Joint.SHOULDER_L].TwistLimit = (ShoulderTwist);
            skeleton[Joint.ELBOW_L].TwistLimit = (ElbowTwist);
            skeleton[Joint.WRIST_L].TwistLimit = (WristTwist);
            skeleton[Joint.CLAVICLE_R].TwistLimit = (ClaviculaTwist);
            skeleton[Joint.SHOULDER_R].TwistLimit = (ShoulderTwist);
            skeleton[Joint.ELBOW_R].TwistLimit = (ElbowTwist);
            skeleton[Joint.WRIST_R].TwistLimit = (WristTwist);
            #endregion
            #endregion
            #region stiffness
            #region Arms
            skeleton[Joint.CLAVICLE_L].Stiffness = (verystiff);
            skeleton[Joint.CLAVICLE_R].Stiffness = (verystiff);

            skeleton[Joint.WRIST_L].Stiffness = (barelymoving);
            skeleton[Joint.WRIST_R].Stiffness = (barelymoving);
            #endregion
            #region Legs
            skeleton[Joint.ANKLE_L].Stiffness = (barelymoving);
            skeleton[Joint.ANKLE_R].Stiffness = (barelymoving);
            skeleton[Joint.FOOTBASE_L].Stiffness = (barelymoving);
            skeleton[Joint.FOOTBASE_R].Stiffness = (barelymoving);
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
