using UnityEngine;
namespace QTM2Unity
{
    class RTStandardUnityModel : RT_IK_Constrained
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

        public void SetAll()
        {
            setGO(hips, BipedSkeleton.PELVIS, setPos: true);
            setGO(spine, BipedSkeleton.SPINE0);
            setGO(spine1, BipedSkeleton.SPINE1);
            setGO(spine2, BipedSkeleton.SPINE3);

            setGO(neck, BipedSkeleton.NECK);
            setGO(head, BipedSkeleton.HEAD);

            setGOLeg(upLegLeft, BipedSkeleton.HIP_L);
            setGOLeg(upLegRight, BipedSkeleton.HIP_R);
            setGOLeg(legLeft, BipedSkeleton.KNEE_L);
            setGOLeg(legRight, BipedSkeleton.KNEE_R);
            setGOFoot(footLeft, BipedSkeleton.FOOTBASE_L);
            setGOFoot(footRight, BipedSkeleton.FOOTBASE_R);


            setGOArmLeft(shoulderLeft, BipedSkeleton.CLAVICLE_L);
            setGOArmLeft(armLeft, BipedSkeleton.SHOULDER_L);
            setGOArmLeft(foreArmLeft, BipedSkeleton.ELBOW_L);
            setGOHandLeft(handLeft, BipedSkeleton.WRIST_L);
            foreach (var fing in fingersLeft)
            {
                setGOHandLeft(fing, BipedSkeleton.HAND_L);
            }
            setGOThumbLeft(thumbLeft, BipedSkeleton.TRAP_L);

            setGOArmRight(shoulderRight, BipedSkeleton.CLAVICLE_R);
            setGOArmRight(armRight, BipedSkeleton.SHOULDER_R);
            setGOArmRight(foreArmRight, BipedSkeleton.ELBOW_R);
            setGOHandRight(handRight, BipedSkeleton.WRIST_R);
            foreach (var fing in fingersRight)
            {
                setGOHandRight(fing, BipedSkeleton.HAND_R);
            }
            setGOThumbRight(thumbRight, BipedSkeleton.TRAP_R);
        }
        private void setGO(Transform go, string name, Quaternion rot, bool setPos)
        {
            Bone b = skeleton[name];
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
        private void setGOLeg(Transform go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.forward * 180));
        }
        private void setGOFoot(Transform go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.right * 90) * Quaternion.Euler(Vector3.up * 180));
        }

        private void setGOArmLeft(Transform go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.forward * 270));
        }

        private void setGOArmRight(Transform go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.forward * 90));
        }

        private void setGOHandLeft(Transform go, string name)
        {
            setGO(go, name,  Quaternion.Euler(Vector3.forward * 300));
        }
        private void setGOHandRight(Transform go, string name)
        {
            setGO(go, name,  Quaternion.Euler(Vector3.forward * 60)); //30?
        }

        private void setGOThumbLeft(Transform go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.right * 330) * Quaternion.Euler(Vector3.forward * 270));
        }
        private void setGOThumbRight(Transform go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.right * 330) * Quaternion.Euler(Vector3.forward * 90));
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


