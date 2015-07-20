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
            base.UpdateNext();
            localRotation = transform.gameObject.transform.rotation;
            SetAll();
            
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
            
            foreach (var b in skeleton.Root)
            {
                switch (b.Data.Name)
                {
                    case Joint.PELVIS:
                        setGO(hips, b.Data, setPos: true);
                        break;
                    case Joint.SPINE0:
                        setGO(spine, b.Data);
                        break;
                    case Joint.SPINE1:
                        setGO(spine1, b.Data);
                        break;
                    case Joint.SPINE3:
                        setGO(spine2, b.Data);
                        break;
                    case Joint.NECK:
                        setGO(neck, b.Data);
                        break;
                    case Joint.HEAD:
                        setGO(head, b.Data);
                        break;
                    case Joint.HIP_L:
                        setGO(upLegLeft, b.Data, legRot);
                        break;
                    case Joint.HIP_R:
                        setGO(upLegRight, b.Data, legRot);
                        break;
                    case Joint.KNEE_L:
                        setGO(legLeft, b.Data, legRot);
                        break;
                    case Joint.KNEE_R:
                        setGO(legRight, b.Data, legRot);
                        break;
                    case Joint.FOOTBASE_L:
                        setGO(footLeft, b.Data, footRot);
                        break;
                    case Joint.FOOTBASE_R:
                        setGO(footRight, b.Data, footRot);
                        break;
                    case Joint.CLAVICLE_L:
                        setGO(shoulderLeft, b.Data, armLeftRot);
                        break;
                    case Joint.SHOULDER_L:
                        setGO(armLeft, b.Data, armLeftRot);
                        break;
                    case Joint.ELBOW_L:
                        setGO(foreArmLeft, b.Data, armLeftRot);
                        break;
                    case Joint.WRIST_L:
                        setGO(handLeft, b.Data, handLeftRot);
                        break;
                    case Joint.HAND_L:
                        foreach (var fing in fingersLeft) setGO(fing, b.Data, handLeftRot);
                        break;
                    case Joint.TRAP_L:
                        setGO(thumbLeft, b.Data, thumbLeftRot);
                        break;
                    case Joint.CLAVICLE_R:
                        setGO(shoulderRight, b.Data, armRightRot);
                        break;
                    case Joint.SHOULDER_R:
                        setGO(armRight, b.Data, armRightRot);
                        break;
                    case Joint.ELBOW_R:
                        setGO(foreArmRight, b.Data, armRightRot);
                        break;
                    case Joint.WRIST_R:
                        setGO(handRight, b.Data, handRightRot);
                        break;
                    case Joint.HAND_R:
                        foreach (var fing in fingersRight) setGO(fing, b.Data, handRightRot);
                        break;
                    case Joint.TRAP_R:
                        setGO(thumbRight, b.Data, thumbRightRot);
                        break;
                    default:
                        break;
                }
            }
                 
            /*
                setGO(hips, Joint.PELVIS, setPos: true);
                setGO(spine, Joint.SPINE0);
                setGO(spine1, Joint.SPINE1);
                setGO(spine2, Joint.SPINE3);
                setGO(neck, Joint.NECK);
                setGO(head, Joint.HEAD);
                setGO(upLegLeft, Joint.HIP_L, legRot);
                setGO(upLegRight, Joint.HIP_R, legRot);
                setGO(legLeft, Joint.KNEE_L, legRot);
                setGO(legRight, Joint.KNEE_R, legRot);
                setGO(footLeft, Joint.FOOTBASE_L, footRot);
                setGO(footRight, Joint.FOOTBASE_R, footRot);
                setGO(shoulderLeft, Joint.CLAVICLE_L, armLeftRot);
                setGO(armLeft, Joint.SHOULDER_L, armLeftRot);
                setGO(foreArmLeft, Joint.ELBOW_L, armLeftRot);
                setGO(handLeft, Joint.WRIST_L, handLeftRot);
                foreach (var fing in fingersLeft) setGO(fing, Joint.HAND_L, handLeftRot);
                setGO(thumbLeft, Joint.TRAP_L, thumbLeftRot);
                setGO(shoulderRight, Joint.CLAVICLE_R, armRightRot);
                setGO(armRight, Joint.SHOULDER_R, armRightRot);
                setGO(foreArmRight, Joint.ELBOW_R, armRightRot);
                setGO(handRight, Joint.WRIST_R, handRightRot);
                foreach (var fing in fingersRight) setGO(fing, Joint.HAND_R, handRightRot);
                setGO(thumbRight, Joint.TRAP_R, thumbRightRot);
            */
                 
        }
        private void setGO(Transform go, Bone b, Quaternion rot, bool setPos)
        {
            if (b != null && !float.IsNaN(b.Orientation.W))
            {
                go.rotation = localRotation * b.Orientation.Convert() * rot ;
                if (setPos && !float.IsNaN(b.Pos.X)) go.position = go.parent.position + b.Pos.Convert();
            }
        }
        private void setGO(Transform go, Bone b, bool setPos)
        {
            setGO(go, b, Quaternion.identity, setPos);
        }
        private void setGO(Transform go, Bone b)
        {
            setGO(go, b, Quaternion.identity, false);
        }
        private void setGO(Transform go, Bone b, Quaternion rot)
        {
            setGO(go, b, rot, false);
        }
        /*
        private void setGO(Transform go, string name, Quaternion rot)
        {
            setGO(go, skeleton[name], rot,  false);
        }
        private void setGO(Transform go, string name)
        {
            setGO(go, skeleton[name], Quaternion.identity, false);
        }
        private void setGO(Transform go, string name, bool setPos)
        {
            setGO(go, skeleton[name], Quaternion.identity, setPos);
        }
        */
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


