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
        public CharacterModel CharacterModel;
        private CharacterGameObjects charactersJoints = new CharacterGameObjects();
        private CharactersModel cf = CharactersModel.Model1;
        private float scale;
        public override void StartNext()
        {
            charactersJoints.SetLimbs(
                this.transform,
                UseFingers
                );
            //charactersJoints.PrintAll();
            float pelvisHeight = 0;
            var trans = charactersJoints.pelvis;
            while (trans.parent && trans.parent != this)
            {
                pelvisHeight += trans.localPosition.y;
                trans = trans.parent;
            }
            scale = pelvisHeight / skeleton.Root.Data.Pos.Y;

            scale /= transform.localScale.magnitude;
            UnityEngine.Debug.LogFormat("name: {0} Scale: {1} Magnutude: {2}", this.name, scale, transform.localScale.magnitude);
        }

        public override void UpdateNext()
        {
            if (cf != CharacterModel.model)
            {
                switch (CharacterModel.model)
                {
                    case CharactersModel.Model1:
                        CharacterModel.boneRotatation = new Model1();
                        break;
                    case CharactersModel.Model2:
                        CharacterModel.boneRotatation = new Model2();
                        break;
                    case CharactersModel.Model3:
                        CharacterModel.boneRotatation = new Model3();
                        break;
                    case CharactersModel.Model4:
                        CharacterModel.boneRotatation = new Model4();
                        break;
                    case CharactersModel.Model5:
                        CharacterModel.boneRotatation = new Model5();
                        break;
                    case CharactersModel.Model6:
                        CharacterModel.boneRotatation = new Model6();
                        break;
                    default:
                        break;
                }
                cf = CharacterModel.model;
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
                        SetJointRotation(charactersJoints.pelvis, b.Data, CharacterModel.boneRotatation.hip);
                        if (!b.Data.Pos.IsNaN())
                        {
                            OpenTK.Vector3 p = b.Data.Pos * scale * transform.localScale.magnitude;
                            charactersJoints.pelvis.position =
                                transform.position
                                + p.Convert()
                                ;
                        }
                        break;
                    case Joint.SPINE0:
                        if (charactersJoints.spine.Length > 0)
                            SetJointRotation(charactersJoints.spine[0], b.Data, CharacterModel.boneRotatation.spine);
                        break;
                    case Joint.SPINE1:
                        if (charactersJoints.spine.Length > 1)
                            SetJointRotation(charactersJoints.spine[1], b.Data, CharacterModel.boneRotatation.spine);
                        break;
                    case Joint.SPINE3:
                        if (charactersJoints.spine.Length > 2)
                            SetJointRotation(charactersJoints.spine[2], b.Data, CharacterModel.boneRotatation.spine);
                        break;
                    case Joint.NECK:
                        SetJointRotation(charactersJoints.neck, b.Data, CharacterModel.boneRotatation.neck);
                        break;
                    case Joint.HEAD:
                        SetJointRotation(charactersJoints.head, b.Data, CharacterModel.boneRotatation.head);
                        break;
                    case Joint.HIP_L:
                        SetJointRotation(charactersJoints.leftThigh, b.Data, CharacterModel.boneRotatation.legUpperLeft);
                        break;
                    case Joint.HIP_R:
                        SetJointRotation(charactersJoints.rightThigh, b.Data, CharacterModel.boneRotatation.legUpperRight);
                        break;
                    case Joint.KNEE_L:
                        SetJointRotation(charactersJoints.leftCalf, b.Data, CharacterModel.boneRotatation.legLowerLeft);
                        break;
                    case Joint.KNEE_R:
                        SetJointRotation(charactersJoints.rightCalf, b.Data, CharacterModel.boneRotatation.legLowerRight);
                        break;
                    case Joint.FOOTBASE_L:
                        SetJointRotation(charactersJoints.leftFoot, b.Data, CharacterModel.boneRotatation.footLeft);
                        break;
                    case Joint.FOOTBASE_R:
                        SetJointRotation(charactersJoints.rightFoot, b.Data, CharacterModel.boneRotatation.footRight);
                        break;
                    case Joint.CLAVICLE_L:
                        SetJointRotation(charactersJoints.leftClavicle, b.Data, CharacterModel.boneRotatation.clavicleLeft);
                        break;
                    case Joint.CLAVICLE_R:
                        SetJointRotation(charactersJoints.rightClavicle, b.Data, CharacterModel.boneRotatation.clavicleRight);
                        break;
                    case Joint.SHOULDER_L:
                        SetJointRotation(charactersJoints.leftUpperArm, b.Data, CharacterModel.boneRotatation.armUpperLeft);
                        break;
                    case Joint.SHOULDER_R:
                        SetJointRotation(charactersJoints.rightUpperArm, b.Data, CharacterModel.boneRotatation.armUpperRight);
                        break;
                    case Joint.ELBOW_L:
                        SetJointRotation(charactersJoints.leftForearm, b.Data, CharacterModel.boneRotatation.armLowerLeft);
                        break;
                    case Joint.ELBOW_R:
                        SetJointRotation(charactersJoints.rightForearm, b.Data, CharacterModel.boneRotatation.armLowerRight);
                        break;
                    case Joint.WRIST_L:
                        SetJointRotation(charactersJoints.leftHand, b.Data, CharacterModel.boneRotatation.handLeft);
                        break;
                    case Joint.WRIST_R:
                        SetJointRotation(charactersJoints.rightHand, b.Data, CharacterModel.boneRotatation.handRight);
                        break;
                    case Joint.HAND_L:
                        if (UseFingers && charactersJoints.fingersLeft != null) 
                            foreach (var fing in charactersJoints.fingersLeft) 
                                SetJointRotation(fing, b.Data, CharacterModel.boneRotatation.fingersLeft);
                        break;
                    case Joint.HAND_R:
                        if (UseFingers && charactersJoints.fingersRight != null) 
                            foreach (var fing in charactersJoints.fingersRight) 
                                SetJointRotation(fing, b.Data, CharacterModel.boneRotatation.fingersRight);
                        break;
                    case Joint.TRAP_L:
                        if (UseFingers) 
                            SetJointRotation(charactersJoints.thumbLeft, b.Data, CharacterModel.boneRotatation.thumbLeft);
                        break;
                    case Joint.TRAP_R:
                        if (UseFingers) 
                            SetJointRotation(charactersJoints.thumbRight, b.Data, CharacterModel.boneRotatation.thumbRight);
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
                    * Quaternion.Euler(CharacterModel.boneRotatation.root)
                    ;

            }
        }
    }
}


