using UnityEngine;
namespace QTM2Unity
{
    class RTStandardUnityModel : RT_IK_Constrained
    {
        public bool firstSpine = false;

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
        //Transform neck;
        Transform neck1;
        //Transform neck2;
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
            base.UpdateNext();
            SetAll();
        }

        public void SetAll()
        {
            setGO(hips, BipedSkeleton.PELVIS, true);
            setGO(spine, BipedSkeleton.SPINE0, false);
            if (firstSpine) setGO(spine1, BipedSkeleton.SPINE1, false);
            else setGO(spine2, BipedSkeleton.SPINE1, false);
            
            setGO(neck1, BipedSkeleton.NECK, false);
            setGO(head, BipedSkeleton.HEAD, false);

            setGOLeg(upLegLeft, BipedSkeleton.UPPERLEG_L);
            setGOLeg(upLegRight, BipedSkeleton.UPPERLEG_R);
            setGOLeg(legLeft, BipedSkeleton.LOWERLEG_L);
            setGOLeg(legRight, BipedSkeleton.LOWERLEG_R);
            setGOFoot(footLeft, BipedSkeleton.FOOT_L);
            setGOFoot(footRight, BipedSkeleton.FOOT_R);


            setGOArmLeft(shoulderLeft, BipedSkeleton.SHOULDER_L);
            setGOArmLeft(armLeft, BipedSkeleton.UPPERARM_L);
            setGOArmLeft(foreArmLeft, BipedSkeleton.LOWERARM_L);
            setGOArmLeft(handLeft, BipedSkeleton.HAND_L);

            setGOArmRight(shoulderRight, BipedSkeleton.SHOULDER_R);
            setGOArmRight(armRight, BipedSkeleton.UPPERARM_R);
            setGOArmRight(foreArmRight, BipedSkeleton.LOWERARM_R);
            setGOArmRight(handRight, BipedSkeleton.HAND_R);
        }
        private void setGO(Transform go, string name, Quaternion rot, bool setPos)
        {
            Bone b = skeleton[name];
            if (b != null && !float.IsNaN(b.Orientation.W))
            {
                go.rotation = b.Orientation.Convert() * rot;
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
            setGO(go, name, Quaternion.Euler(Vector3.right * 90) * Quaternion.Euler(Vector3.up * 180) * Quaternion.Euler(Vector3.right * -30), false);
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
            //neck = transform.Search("Neck");
            neck1 = transform.Search("Neck1");
            //neck2 = transform.Search("Neck2");
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


