using UnityEngine;
namespace QTM2Unity
{

    class RTCharacerStream : RTSkeleton
    {

        private Quaternion localRotation;
        private CharacterGameObjects charactersJoints = new CharacterGameObjects();
        public override void StartNext()
        {
            charactersJoints.SetLimbs(
                this.transform,
                debug.jointsRot.UseFingers
                );
            charactersJoints.PrintAll();
        }

        public override void UpdateNext()
        {
            localRotation = transform.gameObject.transform.rotation;
            SetAll();
        }
         
        private void SetAll()
        {
            foreach (var b in skeleton.Root)
            {
                switch (b.Data.Name)
                {
                    case Joint.PELVIS:
                        setGO(charactersJoints.pelvis, b.Data, debug.jointsRot.hip, setPos: true);
                        break;
                    case Joint.SPINE0:
                        if (charactersJoints.spine.Length > 0)
                            setGO(charactersJoints.spine[0], b.Data, debug.jointsRot.spine);
                        break;
                    case Joint.SPINE1:
                        if (charactersJoints.spine.Length > 1)
                            setGO(charactersJoints.spine[1], b.Data, debug.jointsRot.spine);
                        break;
                    case Joint.SPINE3:
                        if (charactersJoints.spine.Length > 2)
                            setGO(charactersJoints.spine[2], b.Data, debug.jointsRot.spine);
                        break;
                    case Joint.NECK:
                        setGO(charactersJoints.neck, b.Data, debug.jointsRot.spine);
                        break;
                    case Joint.HEAD:
                        setGO(charactersJoints.head, b.Data, debug.jointsRot.head);
                        break;
                    case Joint.HIP_L:
                        setGO(charactersJoints.leftThigh, b.Data, debug.jointsRot.legUpperLeft);
                        break;
                    case Joint.HIP_R:
                        setGO(charactersJoints.rightThigh, b.Data, debug.jointsRot.legUpperRight);
                        break;
                    case Joint.KNEE_L:
                        setGO(charactersJoints.leftCalf, b.Data, debug.jointsRot.legLowerLeft);
                        break;
                    case Joint.KNEE_R:
                        setGO(charactersJoints.rightCalf, b.Data, debug.jointsRot.legLowerRight);
                        break;
                    case Joint.FOOTBASE_L:
                        setGO(charactersJoints.leftFoot, b.Data, debug.jointsRot.footLeft);
                        break;
                    case Joint.FOOTBASE_R:
                        setGO(charactersJoints.rightFoot, b.Data, debug.jointsRot.footRight);
                        break;
                    case Joint.CLAVICLE_L:
                        setGO(charactersJoints.shoulderLeft, b.Data, debug.jointsRot.clavicleLeft);
                        break;
                    case Joint.CLAVICLE_R:
                        setGO(charactersJoints.shoulderRight, b.Data, debug.jointsRot.clavicleRight);
                        break;
                    case Joint.SHOULDER_L:
                        setGO(charactersJoints.leftUpperArm, b.Data, debug.jointsRot.armUpperLeft);
                        break;
                    case Joint.SHOULDER_R:
                        setGO(charactersJoints.rightUpperArm, b.Data, debug.jointsRot.armUpperRight);
                        break;
                    case Joint.ELBOW_L:
                        setGO(charactersJoints.leftForearm, b.Data, debug.jointsRot.armLowerLeft);
                        break;
                    case Joint.ELBOW_R:
                        setGO(charactersJoints.rightForearm, b.Data, debug.jointsRot.armLowerRight);
                        break;
                    case Joint.WRIST_L:
                        setGO(charactersJoints.leftHand, b.Data, debug.jointsRot.handLeft);
                        break;
                    case Joint.WRIST_R:
                        setGO(charactersJoints.rightHand, b.Data, debug.jointsRot.handRight);
                        break;
                    case Joint.HAND_L:
                        if (debug.jointsRot.UseFingers) 
                            foreach (var fing in charactersJoints.fingersLeft) 
                                setGO(fing, b.Data, debug.jointsRot.fingersLeft);
                        break;
                    case Joint.HAND_R:
                        if (debug.jointsRot.UseFingers) 
                            foreach (var fing in charactersJoints.fingersRight) 
                                setGO(fing, b.Data, debug.jointsRot.fingersRight);
                        break;
                    case Joint.TRAP_L:
                        if (debug.jointsRot.UseFingers) 
                            setGO(charactersJoints.thumbLeft, b.Data, debug.jointsRot.thumbLeft);
                        break;
                    case Joint.TRAP_R:
                        if (debug.jointsRot.UseFingers) 
                            setGO(charactersJoints.thumbRight, b.Data, debug.jointsRot.thumbRight);
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
            if (b != null && !b.Orientation.IsNaN() && go)
            {
                go.rotation = localRotation * b.Orientation.Convert() *  Quaternion.Euler(euler);
                if (setPos && !b.Pos.IsNaN())
                {
                    go.position = go.parent.position + b.Pos.Convert();
                }
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


