using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace QTM2Unity
{
    class ConstTest : RT
    {
        public bool showConstraints = true;
        public float coneSize = 0.1f;
        public int coneRes = 50;
        public bool showRotationTrace;
        public float markerScale = 0.01f;
        public IK ik = IK.CCD;

        public Vector4 Femur = new Vector4(60, 60, 60, 160);
        public Vector2 FemurTwist = new Vector2(90, 90);
        public Vector4 Knee = new Vector4(5, 5, 5, 160);
        public Vector2 KneeTwist = new Vector2(90, 90);
        public Vector4 Ankle = new Vector4(40, 110, 20, 10);
        public Vector2 AnkleTwist = new Vector2(90, 90);
        public Vector4 Spine = new Vector4(20, 20, 20, 20);
        public Vector2 SpineTwist = new Vector2(90, 90);
        public Vector4 Neck = new Vector4(60, 60, 60, 60);
        public Vector2 NeckTwist = new Vector2(90, 90);
        public Vector4 KeyBone = new Vector4(40, 40, 40, 40);
        public Vector2 KeyBoneTwist = new Vector2(90, 90);
        public Vector4 Shoulder = new Vector4(160, 85, 50, 85);
        public Vector2 ShoulderTwist = new Vector2(90, 90);
        public Vector4 Elbow = new Vector4(5, 5, 150, 5);
        public Vector2 ElbowTwist = new Vector2(90, 90);
        public Vector4 Wrist = new Vector4(60, 60, 60, 60);
        public Vector2 WristTwist = new Vector2(90, 90);

        private JointLocalization joints;
        private BipedSkeleton skeleton;
        private Vector3 thisPos;
        private IKApplier ikApplier = new IKApplier();
        // Use this for initialization
        public override void StartNext()
        {
            joints = new JointLocalization();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            thisPos = this.transform.position;
            if (joints == null) joints = new JointLocalization();
            skeleton = joints.GetJointLocation(markerData);
            SetConstraints(ref skeleton);
            if (ik == IK.CCD)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new CCD());
            }
            else if (ik == IK.FABRIK)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new FABRIK());
            }
            else if (ik == IK.TRANSPOSE)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new JacobianTranspose());
            }
            else if (ik == IK.DLS)
            {
                skeleton = ikApplier.ApplyIK(skeleton, new DampedLeastSquares());
            }
            else
            {
                skeleton = ikApplier.ApplyIK(skeleton, new TargetTriangleIK());
            }
        }
        void OnDrawGizmos()
        {
            if (skeleton != null)
            {
                foreach (TreeNode<Bone> b in skeleton)
                {
                    Vector3 v = cv(b.Data.Pos) + thisPos;
                
                    Gizmos.DrawSphere(v, markerScale);
                    if (showRotationTrace)
                        UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos + thisPos.Convert(), traceScale);
                    if (!b.IsLeaf)
                    {
                        foreach (TreeNode<Bone> b2 in b.Children)
                        {
                            drawLine(b.Data.Pos, b2.Data.Pos);
                        }
                    }
                    if (showConstraints)
                    {
                        if (!b.IsRoot)
                        {
                            Bone referenceBone;
                            if (   b.Data.Name.Equals(BipedSkeleton.UPPERLEG_L)
                                || b.Data.Name.Equals(BipedSkeleton.UPPERLEG_R))
                            {
                                OpenTK.Quaternion happy = b.Parent.Data.Orientation * QuaternionHelper.RotationX(OpenTK.MathHelper.Pi);
                                //UnityDebug.DrawRays2(happy, b.Parent.Data.Pos + thisPos.Convert(), 3f);
                                referenceBone = new Bone(
                                    "",
                                    b.Parent.Data.Pos,
                                    happy);
                            }
                            else if (b.Data.Name.Equals(BipedSkeleton.SHOULDER_R) )
                                    
                            {
                                referenceBone = new Bone(
                                    "",
                                    b.Parent.Data.Pos,
                                    b.Parent.Data.Orientation * QuaternionHelper.RotationZ(-OpenTK.MathHelper.PiOver2 ));
                            }
                            else if (  b.Data.Name.Equals(BipedSkeleton.SHOULDER_L) )
                            {
                                referenceBone = new Bone(
                                    "",
                                    b.Parent.Data.Pos,
                                    b.Parent.Data.Orientation * QuaternionHelper.RotationZ(OpenTK.MathHelper.PiOver2));
                            }
                            else
                            {
                                referenceBone = b.Parent.Data;
                            }
                            Bone c = b.Data;
                            OpenTK.Vector3 L1 = referenceBone.GetDirection();
                            UnityDebug.CreateIrregularCone3(c.Constraints, c.Pos + thisPos.Convert(), L1, referenceBone.Orientation, coneRes, coneSize);
                        }
                    }
                }
            }
        }
        private void SetConstraints(ref BipedSkeleton skeleton)
        {
            skeleton[BipedSkeleton.SPINE0].SetRotationalConstraints(Spine.Convert());
            skeleton[BipedSkeleton.SPINE1].SetRotationalConstraints(Spine.Convert());
            //skeleton[BipedSkeleton.SPINE3].SetRotationalConstraints(Spine.Convert());

            skeleton[BipedSkeleton.NECK].SetRotationalConstraints(Neck.Convert());
            //skeleton[BipedSkeleton.HEAD].SetRotationalConstraints(Head.Convert());

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
