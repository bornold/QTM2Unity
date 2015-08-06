#region --- LINCENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Jonas Bornold

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
using UnityEngine;
namespace QTM2Unity
{

    class RTCharacerStream : RTSkeleton
    {
        private CharacterGameObjects charactersJoints = new CharacterGameObjects();
        private CharactersModel cf = CharactersModel.Model1;
        private float pelvisHeight;
        public override void StartNext()
        {
            charactersJoints.SetLimbs(
                this.transform,
                debug.jointsRot.UseFingers
                );
            charactersJoints.PrintAll();
            pelvisHeight = 0;
            var trans = charactersJoints.pelvis;
            while (trans.parent)
            {
                pelvisHeight += trans.localPosition.y;
                trans = trans.parent;
            }
            pelvisHeight -= skeleton.Root.Data.Pos.Y;
        }

        public override void UpdateNext()
        {
            if (cf != debug.jointsRot.model)
            {
                switch (debug.jointsRot.model)
                {
                    case CharactersModel.Model1:
                        debug.jointsRot.rots = new Model1();
                        break;
                    case CharactersModel.Model2:
                        debug.jointsRot.rots = new Model2();
                        break;
                    case CharactersModel.Model3:
                        debug.jointsRot.rots = new Model3();
                        break;
                    case CharactersModel.Model4:
                        debug.jointsRot.rots = new Model4();
                        break;
                    case CharactersModel.Model5:
                        debug.jointsRot.rots = new Model5();
                        break;
                    default:
                        break;
                }
                cf = debug.jointsRot.model;
            }
            SetAll();
        }
         
        private void SetAll()
        {
            foreach (var b in skeleton.Root)
            {
                switch (b.Data.Name)
                {
                    case Joint.PELVIS:
                        SetJointRotation(charactersJoints.pelvis, b.Data, debug.jointsRot.rots.hip);
                        if (!b.Data.Pos.IsNaN())
                        {
                            Vector3 p = b.Data.Pos.Convert();
                            charactersJoints.pelvis.position = 
                                charactersJoints.root.position 
                                + new Vector3(p.x, p.y + pelvisHeight, p.z);
                        }
                        break;
                    case Joint.SPINE0:
                        if (charactersJoints.spine.Length > 0)
                            SetJointRotation(charactersJoints.spine[0], b.Data, debug.jointsRot.rots.spine);
                        break;
                    case Joint.SPINE1:
                        if (charactersJoints.spine.Length > 1)
                            SetJointRotation(charactersJoints.spine[1], b.Data, debug.jointsRot.rots.spine);
                        break;
                    case Joint.SPINE3:
                        if (charactersJoints.spine.Length > 2)
                            SetJointRotation(charactersJoints.spine[2], b.Data, debug.jointsRot.rots.spine);
                        break;
                    case Joint.NECK:
                        SetJointRotation(charactersJoints.neck, b.Data, debug.jointsRot.rots.neck);
                        break;
                    case Joint.HEAD:
                        SetJointRotation(charactersJoints.head, b.Data, debug.jointsRot.rots.head);
                        break;
                    case Joint.HIP_L:
                        SetJointRotation(charactersJoints.leftThigh, b.Data, debug.jointsRot.rots.legUpperLeft);
                        break;
                    case Joint.HIP_R:
                        SetJointRotation(charactersJoints.rightThigh, b.Data, debug.jointsRot.rots.legUpperRight);
                        break;
                    case Joint.KNEE_L:
                        SetJointRotation(charactersJoints.leftCalf, b.Data, debug.jointsRot.rots.legLowerLeft);
                        break;
                    case Joint.KNEE_R:
                        SetJointRotation(charactersJoints.rightCalf, b.Data, debug.jointsRot.rots.legLowerRight);
                        break;
                    case Joint.FOOTBASE_L:
                        SetJointRotation(charactersJoints.leftFoot, b.Data, debug.jointsRot.rots.footLeft);
                        break;
                    case Joint.FOOTBASE_R:
                        SetJointRotation(charactersJoints.rightFoot, b.Data, debug.jointsRot.rots.footRight);
                        break;
                    case Joint.CLAVICLE_L:
                        SetJointRotation(charactersJoints.leftClavicle, b.Data, debug.jointsRot.rots.clavicleLeft);
                        break;
                    case Joint.CLAVICLE_R:
                        SetJointRotation(charactersJoints.rightClavicle, b.Data, debug.jointsRot.rots.clavicleRight);
                        break;
                    case Joint.SHOULDER_L:
                        SetJointRotation(charactersJoints.leftUpperArm, b.Data, debug.jointsRot.rots.armUpperLeft);
                        break;
                    case Joint.SHOULDER_R:
                        SetJointRotation(charactersJoints.rightUpperArm, b.Data, debug.jointsRot.rots.armUpperRight);
                        break;
                    case Joint.ELBOW_L:
                        SetJointRotation(charactersJoints.leftForearm, b.Data, debug.jointsRot.rots.armLowerLeft);
                        break;
                    case Joint.ELBOW_R:
                        SetJointRotation(charactersJoints.rightForearm, b.Data, debug.jointsRot.rots.armLowerRight);
                        break;
                    case Joint.WRIST_L:
                        SetJointRotation(charactersJoints.leftHand, b.Data, debug.jointsRot.rots.handLeft);
                        break;
                    case Joint.WRIST_R:
                        SetJointRotation(charactersJoints.rightHand, b.Data, debug.jointsRot.rots.handRight);
                        break;
                    case Joint.HAND_L:
                        if (debug.jointsRot.UseFingers && charactersJoints.fingersLeft != null) 
                            foreach (var fing in charactersJoints.fingersLeft) 
                                SetJointRotation(fing, b.Data, debug.jointsRot.rots.fingersLeft);
                        break;
                    case Joint.HAND_R:
                        if (debug.jointsRot.UseFingers && charactersJoints.fingersRight != null) 
                            foreach (var fing in charactersJoints.fingersRight) 
                                SetJointRotation(fing, b.Data, debug.jointsRot.rots.fingersRight);
                        break;
                    case Joint.TRAP_L:
                        if (debug.jointsRot.UseFingers) 
                            SetJointRotation(charactersJoints.thumbLeft, b.Data, debug.jointsRot.rots.thumbLeft);
                        break;
                    case Joint.TRAP_R:
                        if (debug.jointsRot.UseFingers) 
                            SetJointRotation(charactersJoints.thumbRight, b.Data, debug.jointsRot.rots.thumbRight);
                        break;
                    case Joint.ANKLE_L:
                    case Joint.ANKLE_R:
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// Sets the rotation of the Transform from Bone object to 
        /// </summary>
        /// <param name="go">Joint Transform</param>
        /// <param name="b">Bone</param>
        /// <param name="euler">Euler angels</param>
        private void SetJointRotation(Transform go, Bone b, Vector3 euler)
        {
            if (b != null && !b.Orientation.IsNaN() && go)
            {
                go.rotation =
                    transform.rotation
                    * b.Orientation.Convert()
                    * Quaternion.Euler(euler)
                    * Quaternion.Euler(debug.jointsRot.rots.root)
                    ;

            }
        }
    }
}


