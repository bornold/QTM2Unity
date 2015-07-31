using UnityEngine;
namespace QTM2Unity
{
    [System.Serializable]
    public class JointFix
    {
        public bool UseFingers = false;
        public Vector3
            root = new Vector3(0f, 0f, 0f),
            leg = new Vector3(0f, 0f, 180f),
            feet = new Vector3(270f, 0f, 180f),
            armLeft = new Vector3(0f, 0f, 270f),
            armRight = new Vector3(0f, 0f, 90f),
            handLeft = new Vector3(0f, 0f, 0f),
            handRight = new Vector3(0f, 0f, 90f),
            thumbLeft = new Vector3(330f, 0f, 270f),
            thumbRight = new Vector3(330f, 0f, 90f);

    }
    class RTStandardUnityModel : RT_IK
    {
        public JointFix jointsRot;
        Quaternion localRotation;
        CharacterGameObjects bipedReferences;
        // Use this for initialization
        public override void StartNext()
        {
            base.StartNext();
            CharacterGameObjects.FindLimbs(
                ref bipedReferences,
                this.transform,
                new CharacterGameObjects.Params(false, jointsRot.UseFingers)
                );
            bipedReferences.PrintAll();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            base.UpdateNext();
            localRotation = Quaternion.Euler(jointsRot.root) * transform.gameObject.transform.rotation;
            SetAll();
            
        }

        public void SetAll()
        {
            
            foreach (var b in skeleton.Root)
            {
                switch (b.Data.Name)
                {
                    case Joint.PELVIS:
                        setGO(bipedReferences.pelvis, b.Data, setPos: true);
                        break;
                    case Joint.SPINE0:
                        setGO(bipedReferences.spine[0], b.Data);
                        break;
                    case Joint.SPINE1:
                        if (bipedReferences.spine.Length > 1)
                            setGO(bipedReferences.spine[1], b.Data);
                        break;
                    case Joint.SPINE3:
                        if (bipedReferences.spine.Length > 2)
                            setGO(bipedReferences.spine[2], b.Data);                        
                        break;
                    case Joint.NECK:
                        setGO(bipedReferences.spine[bipedReferences.spine.Length-1], b.Data);
                        break;
                    case Joint.HEAD:
                        setGO(bipedReferences.head, b.Data);
                        break;
                    case Joint.HIP_L:
                        setGO(bipedReferences.leftThigh, b.Data, jointsRot.leg);
                        break;
                    case Joint.HIP_R:
                        setGO(bipedReferences.rightThigh, b.Data, jointsRot.leg);
                        break;
                    case Joint.KNEE_L:
                        setGO(bipedReferences.leftCalf, b.Data, jointsRot.leg);
                        break;
                    case Joint.KNEE_R:
                        setGO(bipedReferences.rightCalf, b.Data, jointsRot.leg);
                        break;
                    case Joint.FOOTBASE_L:
                        setGO(bipedReferences.leftFoot, b.Data, jointsRot.feet);
                        break;
                    case Joint.FOOTBASE_R:
                        setGO(bipedReferences.rightFoot, b.Data, jointsRot.feet);
                        break;
                    case Joint.CLAVICLE_L:
                        setGO(bipedReferences.shoulderLeft, b.Data, jointsRot.armLeft);
                        break;
                    case Joint.CLAVICLE_R:
                        setGO(bipedReferences.shoulderRight, b.Data, jointsRot.armRight);
                        break;
                    case Joint.SHOULDER_L:
                        setGO(bipedReferences.leftUpperArm, b.Data, jointsRot.armLeft);
                        break;
                    case Joint.SHOULDER_R:
                        setGO(bipedReferences.rightUpperArm, b.Data, jointsRot.armRight);
                        break;
                    case Joint.ELBOW_L:
                        setGO(bipedReferences.leftForearm, b.Data, jointsRot.armLeft);
                        break;
                    case Joint.ELBOW_R:
                        setGO(bipedReferences.rightForearm, b.Data, jointsRot.armRight);
                        break;
                    case Joint.WRIST_L:
                        if (jointsRot.UseFingers) setGO(bipedReferences.leftHand, b.Data, jointsRot.handLeft);
                        break;
                    case Joint.WRIST_R:
                        setGO(bipedReferences.rightHand, b.Data, jointsRot.handRight);
                        break;
                    case Joint.HAND_L:
                        if (jointsRot.UseFingers) foreach (var fing in bipedReferences.fingersLeft) setGO(fing, b.Data, jointsRot.handLeft);
                        break;
                    case Joint.HAND_R:
                        if (jointsRot.UseFingers) foreach (var fing in bipedReferences.fingersRight) setGO(fing, b.Data, jointsRot.handRight);
                        break;
                    case Joint.TRAP_L:
                        if (jointsRot.UseFingers) setGO(bipedReferences.thumbLeft, b.Data, jointsRot.thumbLeft);
                        break;
                    case Joint.TRAP_R:
                        if (jointsRot.UseFingers) setGO(bipedReferences.thumbRight, b.Data, jointsRot.thumbRight);
                        break;
                    case Joint.ANKLE_L:
                    case Joint.ANKLE_R:
                        break;
                    default:
                        if (!b.IsLeaf)
                            UnityEngine.Debug.LogError(b);
                        break;
                }
            }
        }
        private void setGO(Transform go, Bone b, Vector3 euler, bool setPos)
        {
            if (b != null && !float.IsNaN(b.Orientation.W) && go)
            {
                go.rotation = localRotation * b.Orientation.Convert() *  Quaternion.Euler(euler);
                if (setPos && !float.IsNaN(b.Pos.X)) go.position = go.parent.position + b.Pos.Convert();
            }
            else
            {
                //UnityEngine.Debug.LogWarning(" no go");
                //UnityEngine.Debug.LogWarning(b);
            }
        }
        private void setGO(Transform go, Bone b, bool setPos)
        {
            setGO(go, b, Vector3.zero, setPos);
        }
        private void setGO(Transform go, Bone b)
        {
            setGO(go, b, Vector3.zero, false);
        }
        private void setGO(Transform go, Bone b, Vector3 rot)
        {
            setGO(go, b, rot, false);
        }
    }
}


