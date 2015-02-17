using System.Collections.Generic;
using UnityEngine;
using QTM2Unity.Unity;
namespace QTM2Unity
{
    class JC2ModelTest : RT {
        GameObject hips;
        GameObject upLegLeft;
        GameObject upLegRight;
        GameObject legLeft;
        GameObject legRight;
        GameObject footLeft;
        GameObject footRight;
        GameObject spine;
        GameObject spine1;
        GameObject spine2;
        GameObject neck;
        GameObject neck1;
        GameObject neck2;
        GameObject head;
        GameObject shoulderLeft;
        GameObject shoulderRight;
        GameObject armLeft;
        GameObject armRight;
        GameObject foreArmLeft;
        GameObject foreArmRight;
        GameObject handLeft;
        GameObject handRight;


        private JointLocalization joints;
        private BipedSkeleton skeleton;
        // Use this for initialization
        public override void StartNext()
        {
            joints = new JointLocalization();
            skeleton = new BipedSkeleton();
            FindGameObjects();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {

            List<LabeledMarker> markerData = rtClient.Markers;
            if (markerData == null && markerData.Count == 0) return;
            if (joints == null) joints = new JointLocalization();
            if (skeleton == null) skeleton = new BipedSkeleton();

            FindGameObjects();
            joints.GetJointLocation(ref skeleton, markerData);
            
            setGO(hips, BipedSkeleton.PELVIS, true);
            setGO(spine, BipedSkeleton.SPINE0,false);
            setGO(spine2, BipedSkeleton.SPINE1, false);
            setGO(neck, BipedSkeleton.NECK, false);
            setGO(head, BipedSkeleton.HEAD, false);

            setGOLeg(upLegLeft,BipedSkeleton.UPPERLEG_L);
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
        private void setGO(GameObject go, string name, Quaternion rot, bool setPos)
        {
            Bone b = skeleton[name];
            if (b != null && !float.IsNaN(b.Orientation.W))
            {
                go.transform.rotation = cq(b.Orientation) * rot;
                if (setPos && !float.IsNaN(b.Pos.X)) go.transform.position = go.transform.parent.position + cv(b.Pos);
            }
        }
        private void setGO(GameObject go, string name, bool setPos)
        {
            setGO(go, name, Quaternion.identity, setPos);
        }
        private void setGOLeg(GameObject go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.forward * 180), false);
        }

        private void setGOFoot(GameObject go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.right * 90) * Quaternion.Euler(Vector3.up * 180), false);
        }
        private void setGOArmRight(GameObject go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.forward * 90), false);
        }

        private void setGOArmLeft(GameObject go, string name)
        {
            setGO(go, name, Quaternion.Euler(Vector3.forward * 270), false);
        }
        private void FindGameObjects()
        {
            hips = GameObject.Find("Hips");
            upLegLeft = GameObject.Find("LeftUpLeg");
            upLegRight = GameObject.Find("RightUpLeg");
            legLeft = GameObject.Find("LeftLeg");
            legRight = GameObject.Find("RightLeg");
            footLeft = GameObject.Find("LeftFoot");
            footRight = GameObject.Find("RightFoot");
            spine = GameObject.Find("Spine");
            spine1 = GameObject.Find("Spine1");
            spine2 = GameObject.Find("Spine2");
            neck = GameObject.Find("Neck");
            neck1 = GameObject.Find("Neck1");
            neck2 = GameObject.Find("Neck2");
            head = GameObject.Find("Head");
            shoulderLeft = GameObject.Find("LeftShoulder");
            shoulderRight = GameObject.Find("RightShoulder");
            armLeft = GameObject.Find("LeftArm");
            armRight = GameObject.Find("RightArm");
            foreArmLeft = GameObject.Find("LeftForeArm");
            foreArmRight = GameObject.Find("RightForeArm");
            handLeft = GameObject.Find("LeftHand");
            handRight = GameObject.Find("RightHand");
        }
    }
}


