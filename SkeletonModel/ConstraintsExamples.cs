using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace QTM2Unity
{
    class ConstraintsExamples
    {

        public Vector4 Femur = new Vector4(60, 160, 60, 60);
        public Vector2 FemurTwist = new Vector2(45, 45);
        public Vector4 Knee = new Vector4(10, 10, 10, 160);
        public Vector2 KneeTwist = new Vector2(45, 45);
        public Vector4 Ankle = new Vector4(30, 110, 30, 0);
        public Vector2 AnkleTwist = new Vector2(45, 45);
        public Vector4 Spine = new Vector4(20, 20, 20, 20);
        public Vector2 SpineTwist = new Vector2(45, 45);
        public Vector4 Neck = new Vector4(60, 60, 60, 60);
        public Vector2 NeckTwist = new Vector2(90, 90);
        public Vector4 KeyBone = new Vector4(60, 40, 60, 20);
        public Vector2 KeyBoneTwist = new Vector2(0, 0);
        public Vector4 Shoulder = new Vector4(110, 110, 110, 110);
        public Vector2 ShoulderTwist = new Vector2(90, 90);
        public Vector4 Elbow = new Vector4(30, 160, 30, 30);
        public Vector2 ElbowTwist = new Vector2(90, 180);
        public Vector4 Wrist = new Vector4(60, 60, 60, 60);
        public Vector2 WristTwist = new Vector2(10, 10);

        public void SetConstraints(ref BipedSkeleton skeleton)
        {
            skeleton[BipedSkeleton.SPINE0].SetRotationalConstraints(Spine.Convert());
            skeleton[BipedSkeleton.SPINE1].SetRotationalConstraints(Spine.Convert());
            skeleton[BipedSkeleton.NECK].SetRotationalConstraints(Neck.Convert());
            skeleton[BipedSkeleton.UPPERLEG_L].SetRotationalConstraints(Femur.Convert());
            skeleton[BipedSkeleton.UPPERLEG_R].SetRotationalConstraints(Femur.Convert());
            skeleton[BipedSkeleton.LOWERLEG_L].SetRotationalConstraints(Knee.Convert());
            skeleton[BipedSkeleton.LOWERLEG_R].SetRotationalConstraints(Knee.Convert());
            skeleton[BipedSkeleton.FOOT_L].SetRotationalConstraints(Ankle.Convert());
            skeleton[BipedSkeleton.FOOT_R].SetRotationalConstraints(Ankle.Convert());


            skeleton[BipedSkeleton.SHOULDER_L].SetRotationalConstraints(KeyBone.Convert());
            skeleton[BipedSkeleton.SHOULDER_R].SetRotationalConstraints(KeyBone.Convert());

            skeleton[BipedSkeleton.UPPERARM_L].SetRotationalConstraints(Shoulder.Convert());
            skeleton[BipedSkeleton.UPPERARM_R].SetRotationalConstraints(Shoulder.Convert());

            skeleton[BipedSkeleton.LOWERARM_L].SetRotationalConstraints(Elbow.Convert());
            skeleton[BipedSkeleton.LOWERARM_R].SetRotationalConstraints(Elbow.Convert());

            skeleton[BipedSkeleton.HAND_L].SetRotationalConstraints(Wrist.Convert());
            skeleton[BipedSkeleton.HAND_R].SetRotationalConstraints(Wrist.Convert());

            skeleton[BipedSkeleton.SPINE0].SetOrientationalConstraints(SpineTwist.Convert());
            skeleton[BipedSkeleton.SPINE1].SetOrientationalConstraints(SpineTwist.Convert());

            skeleton[BipedSkeleton.NECK].SetOrientationalConstraints(NeckTwist.Convert());
            //skeleton[BipedSkeleton.HEAD].SetOrientationalConstraints(HeadTwist.Convert());

            skeleton[BipedSkeleton.UPPERLEG_L].SetOrientationalConstraints(FemurTwist.Convert());
            skeleton[BipedSkeleton.UPPERLEG_R].SetOrientationalConstraints(FemurTwist.Convert());
            skeleton[BipedSkeleton.LOWERLEG_L].SetOrientationalConstraints(KneeTwist.Convert());
            skeleton[BipedSkeleton.LOWERLEG_R].SetOrientationalConstraints(KneeTwist.Convert());
            skeleton[BipedSkeleton.FOOT_L].SetOrientationalConstraints(AnkleTwist.Convert());
            skeleton[BipedSkeleton.FOOT_R].SetOrientationalConstraints(AnkleTwist.Convert());

            skeleton[BipedSkeleton.SHOULDER_L].SetOrientationalConstraints(KeyBoneTwist.Convert());
            skeleton[BipedSkeleton.SHOULDER_R].SetOrientationalConstraints(KeyBoneTwist.Convert());

            skeleton[BipedSkeleton.UPPERARM_L].SetOrientationalConstraints(ShoulderTwist.Convert());
            skeleton[BipedSkeleton.UPPERARM_R].SetOrientationalConstraints(ShoulderTwist.Convert());

            skeleton[BipedSkeleton.LOWERARM_L].SetOrientationalConstraints(ElbowTwist.Convert());
            skeleton[BipedSkeleton.LOWERARM_R].SetOrientationalConstraints(ElbowTwist.Convert());

            skeleton[BipedSkeleton.HAND_L].SetOrientationalConstraints(WristTwist.Convert());
            skeleton[BipedSkeleton.HAND_R].SetOrientationalConstraints(WristTwist.Convert());
        }

    }
}
