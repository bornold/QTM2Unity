using System;
using System.Collections.Generic;
using System.Linq;
using QTM2Unity.SkeletonModel;
using OpenTK;
using QTM2Unity.Unity;
namespace QTM2Unity.JCL
{
    class JointLocalization
    {
        public bool debug = false;

        private float length;
        private float mass;
        private float legLenght;
        private float pelvisDepth;
        private float pelvisWidth;
        private Vector3 ASIS;
        private Vector3 PSIS;
        Vector3 ASISMid;
        Vector3 RIAS;
        Vector3 LIAS;
        Vector3 Sacrum;

        private Dictionary<string, Vector3> markers;
        private Quaternion pelvisOrientation;
        private BipedSkeleton lastFrame;
        private BipedSkeleton currentFrame;

        private const float blend = 0.1f;
        public BipedSkeleton getJointLocazion(List<LabeledMarker> markerData)
        {
            lastFrame = currentFrame;
            markers = markerData.ToDictionary(k => k.label, v => v.position);
            HipOrianteation();
            Bone pelvis = GetPlevis();
            /*
            Bone spine = GetSpine();
            spine.Parent = pelvis;
            Bone neck = GetNeck();
            neck.Parent = spine;
            Bone head = GetHead();
            head.Parent = neck;

            /*
            Bone leftUpperLeg = GetUpperLegLeft();
            leftUpperLeg.Parent = pelvis;
            Bone leftLowerLeg = GetLowerLegLeft();
            leftLowerLeg.Parent = leftUpperLeg;
            Bone leftFoot = GetFootLeft();
            leftFoot.Parent = leftLowerLeg;
            
            Bone rightUpperLeg = GetUpperLegRight();
            rightUpperLeg.Parent = pelvis;
            Bone rightLowerLeg = GetLowerLegRight();
            rightLowerLeg.Parent = rightUpperLeg;
            /*
            Bone rightFoot = GetFootRight();
            rightFoot.Parent = rightLowerLeg;

            Bone leftShoulder = GetShoulderLeft();
            leftShoulder.Parent = neck;
            Bone leftUpperArm = GetUpperArmLeft();
            leftUpperArm.Parent = leftShoulder;
            Bone leftLowerArm = GetLowerArmLeft();
            leftLowerArm.Parent = leftUpperArm;
            Bone leftHand = GetHandLeft();
            leftHand.Parent = leftLowerArm;

            Bone rightShoulder = GetShoulderRight();
            rightShoulder.Parent = neck;
            Bone rightUpperArm = GetUpperArmRight();
            rightUpperArm.Parent = rightShoulder;
            Bone rightLowerArm = GetLowerArmRight();
            rightLowerArm.Parent = rightUpperArm;
            Bone rightHand = GetHandRight();
            rightHand.Parent = rightLowerArm;
            */
            currentFrame = new BipedSkeleton(new List<Bone>()
                {
                    pelvis,        
                    /*
                    spine,     neck,         head,
                    rightUpperLeg, rightLowerLeg, rightFoot,
                    leftUpperLeg,  leftLowerLeg,  leftFoot,
                    leftShoulder,  leftUpperArm,  leftLowerArm, leftHand,
                    rightShoulder, rightUpperArm, rightLowerArm, rightHand
                    */
                });
            return currentFrame;
        }
        private Bone GetPlevis()
        {
            return new Bone(BipedSkeleton.PELVIS, Sacrum, pelvisOrientation);
        }
        private Bone GetSpine()
        {
            Vector3 spinev = markers[spine];
            Vector3 neckv = markers[neck];
            Vector3 up = new Vector3(Vector4.Transform(Vector4.UnitZ, pelvisOrientation));
            Matrix4 m = Matrix4Helper.LookAtUp(spinev,neckv,up);
            Vector3 z = new Vector3(Vector4.Transform(Vector4.UnitZ, m));
            Vector3 v = spinev + blend * z;//Vector3.Lerp(spinev, z, blend);
            //TODO Lerp spinev along z axis
            return new Bone(BipedSkeleton.SPINE0,v,QuaternionHelper.FromMatrix(m));
        }
        private Bone GetHead()
        {
            Vector3 headV = markers[head];
            Vector3 rightHeadV = markers[rightHead];
            Vector3 leftHeadV = markers[leftHead];
            Matrix4 orientation = Matrix4Helper.GetOrientation(headV, leftHeadV, rightHeadV);
            Vector3 pos = new Vector3 (Vector4.Transform(Vector4.UnitW,orientation));
            //Vector3 Y = new Vector3(Vector4.Transform(Vector4.UnitY, orientation));
            //Vector3 v = Vector3.Lerp(pos, Y, blend);
            //TODO Lerp pos along -y axis
            return new Bone(BipedSkeleton.HEAD, pos, QuaternionHelper.FromMatrix(orientation));

        }
        private Bone GetNeck()
        {
            Vector3 root;
            if (markers.ContainsKey(neck))
            {
            Vector3 neckv = markers[neck];
            Vector3 chestv = markers[chest];

            root = Vector3Helper.MidPoint(neckv, chestv); //Vector3.Lerp(markers[neck],markers[chest], blend);
            }
            else
            {
                root = markers[chest];
            }

            Vector3 target = markers[head]; //TODO target head bone instead of marker
            Vector3 right = markers[leftShoulder] - markers[rightShoulder];
            Matrix4 rotationMatrix = Matrix4Helper.LookAtRight(root, target, right);
            return new Bone(BipedSkeleton.NECK, root, QuaternionHelper.FromMatrix(rotationMatrix));
        }
        private Bone GetUpperLegLeft()
        {
            throw new NotImplementedException();
        }
        private Bone GetUpperLegRight() 
        {
            float x, y, z, front, right, up;
            x = -0.24f * pelvisDepth -  9.9f;
            y = -0.30f * pelvisWidth - 10.9f;
            z =  0.33f * pelvisWidth -  7.3f;

            front = -0.24f * pelvisDepth - 9.9f;
            up = -0.30f * pelvisWidth - 10.9f;
            right = 0.33f * pelvisWidth - 7.3f;


            Vector3 pos = new Vector3(right, up, front) / 1000;
            
            string p = string.Format("x:{0}\ty:{1}\tz:{2}\n",pos.X,pos.Y,pos.Z);
            string q = string.Format("ax:{0}\tay:{1}\taz:{2}", ASISMid.X, ASISMid.Y, ASISMid.Z);
            //UnityEngine.Debug.Log(p+q);
            
            pos = ASISMid + pos;
            return new Bone(BipedSkeleton.UPPERLEG_R,pos,pelvisOrientation);
        }
        private Bone GetLowerLegLeft()
        {
            throw new NotImplementedException();
        }
        private Bone GetLowerLegRight()
        {
            Vector3 knee1 = markers[rightOuterKnee];
            Vector3 knee2 = markers[rightLowerKnee];
            Vector3 knee3 = markers[rightUpperKnee];
            Matrix4 m4 = Matrix4Helper.GetOrientation(knee1, knee2, knee3);
            Vector3 pos = Vector3Helper.MidPoint(knee2, knee3);
            Quaternion rot = QuaternionHelper.FromMatrix(m4);
            return new Bone(BipedSkeleton.LOWERLEG_R, pos, rot);
        }
        private Bone GetFootLeft()
        {
            throw new NotImplementedException();
        }
        private Bone GetFootRight()
        {
            throw new NotImplementedException();
        }
        private Bone GetShoulderLeft()
        {
            throw new NotImplementedException();
        }
        private Bone GetShoulderRight()
        {
            throw new NotImplementedException();
        }
        private Bone GetUpperArmLeft()
        {
            throw new NotImplementedException();
        }
        private Bone GetUpperArmRight()
        {
            throw new NotImplementedException();
        }
        private Bone GetLowerArmLeft()
        {
            throw new NotImplementedException();
        }
        private Bone GetLowerArmRight()
        {
            throw new NotImplementedException();
        }
        private Bone GetHandLeft()
        {
            throw new NotImplementedException();
        }
        private Bone GetHandRight()
        {
            throw new NotImplementedException();
        }

        //TODO reflect rias/lias through plane YZ if lias/rias is missing
        private void HipOrianteation()
        {
            Sacrum = markers[bodyBase];
            LIAS = markers[leftHip];
            RIAS = markers[rightHip];
            ASISMid= Vector3Helper.MidPoint(RIAS, LIAS);
            pelvisWidth = (LIAS - RIAS).Length * 1000; // To mm
            pelvisDepth = (ASISMid - Sacrum).Length * 1000; // To mm
            pelvisOrientation = QuaternionHelper.GetHipOrientation(Sacrum,LIAS,RIAS);
        }
        string bodyBase = "SACR";
        string chest = "SME";
        string neck = "TV2";
        string spine = "TV12";
        string head = "SGL";
        string leftHead = "L_HEAD";
        string rightHead = "R_HEAD";

        string leftShoulder = "L_SAE";
        string leftElbow = "L_UOA";
        string leftInnerElbow = "L_HME";
        string leftOuterElbow = "L_HLE";
        string leftWrist = "L_USP";
        string leftWristRadius = "L_RSP";
        string leftHand = "L_HM2";

        string rightShoulder = "R_SAE";
        string rightElbow = "R_UOA";
        string rightInnerElbow = "R_HME";
        string rightOuterElbow = "R_HLE";
        string rightWrist = "R_USP";
        string rightWristRadius = "R_RSP";
        string rightHand = "R_HM2";

        string leftHip = "L_IAS";
        string leftUpperKnee = "L_PAS";
        string leftOuterKnee = "L_FLE";
        string leftLowerKnee = "L_TTC";
        string leftAnkle = "L_FAL";
        string leftHeel = "L_FCC";
        string leftFoot = "L_FM2";

        string rightHip = "R_IAS";
        string rightUpperKnee = "R_PAS";
        string rightOuterKnee = "R_FLE";
        string rightLowerKnee = "R_TTC";
        string rightAnkle = "R_FAL";
        string rightHeel = "R_FCC";
        string rightFoot = "R_FM2";
    }
}
