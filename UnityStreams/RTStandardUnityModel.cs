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

        // Use this for initialization
        public override void StartNext()
        {
            base.StartNext();
            FindTransform();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            if (streaming)
            {
                base.UpdateNext();
                localRotation = transform.gameObject.transform.rotation;
                SetAll();
            }
        }

        public void SetAll()
        {
            setGO(hips, BipedSkeleton.PELVIS, true);
            setGO(spine, BipedSkeleton.SPINE0, false);
            setGO(spine1, BipedSkeleton.SPINE1, false);
            setGO(spine2, BipedSkeleton.SPINE3, false);

            setGO(neck, BipedSkeleton.NECK, false);
            setGO(head, BipedSkeleton.HEAD, false);

            setGOLeg(upLegLeft, BipedSkeleton.HIP_L);
            setGOLeg(upLegRight, BipedSkeleton.HIP_R);
            setGOLeg(legLeft, BipedSkeleton.KNEE_L);
            setGOLeg(legRight, BipedSkeleton.KNEE_R);
            setGOFoot(footLeft, BipedSkeleton.FOOTBASE_L);
            setGOFoot(footRight, BipedSkeleton.FOOTBASE_R);


            setGOArmLeft(shoulderLeft, BipedSkeleton.CLAVICLE_L);
            setGOArmLeft(armLeft, BipedSkeleton.SHOULDER_L);
            setGOArmLeft(foreArmLeft, BipedSkeleton.ELBOW_L);
            setGOArmLeft(handLeft, BipedSkeleton.WRIST_L);

            setGOArmRight(shoulderRight, BipedSkeleton.CLAVICLE_R);
            setGOArmRight(armRight, BipedSkeleton.SHOULDER_R);
            setGOArmRight(foreArmRight, BipedSkeleton.ELBOW_R);
            setGOArmRight(handRight, BipedSkeleton.WRIST_R);
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
        private void setGO(Transform go, string name, bool setPos)
        {
            setGO(go, name, Quaternion.identity, setPos);
        }
        private void setGOLeg(Transform go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.forward * 180), false);
        }

        private void setGOFoot(Transform go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.right * 90) * Quaternion.Euler(Vector3.up * 180), false);
        }
        private void setGOArmRight(Transform go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.forward * 90), false);
        }

        private void setGOArmLeft(Transform go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.forward * 270), false);
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
        }
    }
}


