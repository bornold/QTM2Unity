using UnityEngine;
namespace QTM2Unity
{
    class RTStandardUnityModel : RT_IK
    {
        Quaternion localRotation;
        Transform hips;
        Transform upLegLeft;
        Transform upLegRight;
        Transform legLeft;
        Transform legRight;
        Transform footLeft;
        Transform footRight;
        Transform spine;
        Transform spine1;
        Transform spine2;
        Transform neck;
        Transform head;
        Transform shoulderLeft;
        Transform shoulderRight;
        Transform armLeft;
        Transform armRight;
        Transform foreArmLeft;
        Transform foreArmRight;
        Transform handLeft;
        Transform handRight;
        Transform thumbLeft;
        Transform thumbRight;
        Transform[] fingersLeft;
        Transform[] fingersRight;

        // Use this for initialization
        public override void StartNext()
        {
            base.StartNext();
            FindTransform();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            if (streaming || debugFlag)
            {
                base.UpdateNext();
                localRotation = transform.gameObject.transform.rotation;
                SetAll();
            }
        }

        Quaternion footRot = Quaternion.Euler(Vector3.right * 90) * Quaternion.Euler(Vector3.up * 180),
            armLeftRot = Quaternion.Euler(Vector3.forward * 270),
            armRightRot = Quaternion.Euler(Vector3.forward * 90),
            handLeftRot = Quaternion.Euler(Vector3.forward * 300),
            handRightRot = Quaternion.Euler(Vector3.forward * 60),
            thumbLeftRot = Quaternion.Euler(Vector3.right * 330) * Quaternion.Euler(Vector3.forward * 270),
            thumbRightRot = Quaternion.Euler(Vector3.right * 330) * Quaternion.Euler(Vector3.forward * 90),
            legRot = Quaternion.Euler(Vector3.forward * 180)
            ;
        public void SetAll()
        {
            setGO(hips, BipedSkeleton.PELVIS, setPos: true);
            setGO(spine, BipedSkeleton.SPINE0);
            setGO(spine1, BipedSkeleton.SPINE1);
            setGO(spine2, BipedSkeleton.SPINE3);
            setGO(neck, BipedSkeleton.NECK);
            setGO(head, BipedSkeleton.HEAD);
            setGO(upLegLeft, BipedSkeleton.HIP_L, legRot);
            setGO(upLegRight, BipedSkeleton.HIP_R, legRot);
            setGO(legLeft, BipedSkeleton.KNEE_L, legRot);
            setGO(legRight, BipedSkeleton.KNEE_R, legRot);
            setGO(footLeft, BipedSkeleton.FOOTBASE_L, footRot);
            setGO(footRight, BipedSkeleton.FOOTBASE_R, footRot);
            setGO(shoulderLeft, BipedSkeleton.CLAVICLE_L, armLeftRot);
            setGO(armLeft, BipedSkeleton.SHOULDER_L, armLeftRot);
            setGO(foreArmLeft, BipedSkeleton.ELBOW_L, armLeftRot);
            setGO(handLeft, BipedSkeleton.WRIST_L, handLeftRot);
            foreach (var fing in fingersLeft) setGO(fing, BipedSkeleton.HAND_L, handLeftRot);
            setGO(thumbLeft, BipedSkeleton.TRAP_L, thumbLeftRot);
            setGO(shoulderRight, BipedSkeleton.CLAVICLE_R, armRightRot);
            setGO(armRight, BipedSkeleton.SHOULDER_R, armRightRot);
            setGO(foreArmRight, BipedSkeleton.ELBOW_R, armRightRot);
            setGO(handRight, BipedSkeleton.WRIST_R, handRightRot);
            foreach (var fing in fingersRight) setGO(fing, BipedSkeleton.HAND_R, handRightRot);
            setGO(thumbRight, BipedSkeleton.TRAP_R, thumbRightRot);
        }
        private void setGO(Transform go, string name, Quaternion rot, bool setPos)
        {
            Bone b;
            b = skeleton[name];
            //b = skeleton.Find(name);
            if (b != null && !float.IsNaN(b.Orientation.W))
            {
                go.rotation = localRotation * b.Orientation.Convert() * rot ;
                if (setPos && !float.IsNaN(b.Pos.X)) go.position = go.parent.position + b.Pos.Convert();
            }
        }
        private void setGO(Transform go, string name, Quaternion rot)
        {
            setGO(go, name, rot, false);
        }
        private void setGO(Transform go, string name, bool setPos)
        {
            setGO(go, name, Quaternion.identity, setPos: setPos);
        }
        private void setGO(Transform go, string name)
        {
            setGO(go, name, Quaternion.identity, false);
        }



        private void FindTransform()
        {
            hips = transform.Search("Hips");
            upLegLeft = transform.Search("LeftUpLeg");
            upLegRight = transform.Search("RightUpLeg");
            legLeft = transform.Search("LeftLeg");
            legRight = transform.Search("RightLeg");
            footLeft = transform.Search("LeftFoot");
            footRight = transform.Search("RightFoot");
            spine = transform.Search("Spine");
            spine1 = transform.Search("Spine1");
            spine2 = transform.Search("Spine2");
            neck = transform.Search("Neck");
            head = transform.Search("Head");
            shoulderLeft = transform.Search("LeftShoulder");
            shoulderRight = transform.Search("RightShoulder");
            armLeft = transform.Search("LeftArm");
            armRight = transform.Search("RightArm");
            foreArmLeft = transform.Search("LeftForeArm");
            foreArmRight = transform.Search("RightForeArm");
            handLeft = transform.Search("LeftHand");
            handRight = transform.Search("RightHand");

            thumbLeft = transform.Search("LeftHandThumb1");
            thumbRight = transform.Search("RightHandThumb1");

            fingersLeft = new Transform[4];
            fingersLeft[0] = transform.Search("LeftHandIndex1");
            fingersLeft[1] = transform.Search("LeftHandMiddle1");
            fingersLeft[3] = transform.Search("LeftHandPinky1");
            fingersLeft[2] = transform.Search("LeftHandRing1");

            fingersRight = new Transform[4];
            fingersRight[0] = transform.Search("RightHandIndex1");
            fingersRight[1] = transform.Search("RightHandMiddle1");
            fingersRight[3] = transform.Search("RightHandPinky1");
            fingersRight[2] = transform.Search("RightHandRing1");
        }
    }
}


