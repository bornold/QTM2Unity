﻿#region --- LINCENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable 0649
namespace QualisysRealTime.Unity.Skeleton
{
    class RTCharacerStream : MonoBehaviour
    {
        public string MarkerPrefix = "";
        public bool UseFingers = false;
        public bool UseIK = true;
        public bool IKInterpolation = false;
        private bool fingerUse;
        public bool LockPosition = false;
        public bool ResetSkeleton = false;
        public CharactersModel model = CharactersModel.Model1;
        public BoneRotations boneRotatation = new Model1();
        public Debugging debug;

        private RTClient rtClient;
        private Vector3 pos;
        private bool streaming = false;
        private List<LabeledMarker> markerData;
        private SkeletonBuilder skeletonBuilder;
        private BipedSkeleton skeleton;
        private CharacterGameObjects charactersJoints = new CharacterGameObjects();
        private CharactersModel cf = CharactersModel.Model1;
        private float scale = 0;
        private bool jointsFound = false;
        /// <summary>
        /// Locating the joints of the character and find the scale by which the position should be applied to
        /// </summary>
        public void Start()
        {
            rtClient = RTClient.GetInstance();
            fingerUse = UseFingers;
            charactersJoints.SetLimbs(this.transform, fingerUse);
            if (!charactersJoints.IsAllJointsSet(fingerUse))
            {
                charactersJoints.PrintAll();
                UnityEngine.Debug.LogError("Could not find all necessary joints");
            } else
            {
                jointsFound = true;
            }
            var animation = this.GetComponent<Animation>();
            if (animation) animation.enabled = false;
        }
        /// <summary>
        /// Updates the rotation and position of the Character
        /// </summary>
        public void Update()
        {
            if (!jointsFound) return;
            if (rtClient == null) rtClient = RTClient.GetInstance();
            streaming = rtClient.GetStreamingStatus();
            if (!streaming && !LockPosition) return;
            markerData = rtClient.Markers;
            if ((markerData == null || markerData.Count == 0) && !LockPosition)
            {
                UnityEngine.Debug.LogWarning("Stream does not contain any markers");
                return;
            }
            if (ResetSkeleton || skeletonBuilder == null)
            {
                if (fingerUse != UseFingers)
                {
                    charactersJoints.SetLimbs(this.transform, fingerUse);
                    charactersJoints.PrintAll();
                }
                skeletonBuilder = new SkeletonBuilder(rtClient, MarkerPrefix);
                if (LockPosition) skeleton = new BipedSkeleton();
                ResetSkeleton = false;
            }
            if (!LockPosition)
            {
                skeletonBuilder.Interpolation = IKInterpolation;
                skeletonBuilder.SolveWithIK = UseIK;
                skeleton = skeletonBuilder.SolveSkeleton(markerData);
            }
            if (scale == 0) scale = FindScale(charactersJoints.pelvis);
            SetModelRotation();
            SetAll();
        }
         
        /// <summary>
        /// Maps the built skeleton joint rotation to the characeters joints
        /// </summary>
        private void SetAll()
        {
            foreach (var b in skeleton.Root)
            {
                switch (b.Data.Name)
                {
                    case Joint.PELVIS:
                        SetJointRotation(charactersJoints.pelvis, b.Data, boneRotatation.hip);
                        if (charactersJoints.pelvis && !b.Data.Pos.IsNaN())
                        {
                            Vector3 positionChange = (b.Data.Pos * scale * transform.localScale.magnitude).Convert();
                            charactersJoints.pelvis.position = transform.position + positionChange;
                        }
                        break;
                    case Joint.SPINE0:
                        if (charactersJoints.spine.Length > 0)
                            SetJointRotation(charactersJoints.spine[0], b.Data, boneRotatation.spine);
                        break;
                    case Joint.SPINE1:
                        if (charactersJoints.spine.Length > 1)
                            SetJointRotation(charactersJoints.spine[1], b.Data, boneRotatation.spine);
                        break;
                    case Joint.SPINE3:
                        if (charactersJoints.spine.Length > 2)
                            SetJointRotation(charactersJoints.spine[2], b.Data, boneRotatation.spine);
                        break;
                    case Joint.NECK:
                        SetJointRotation(charactersJoints.neck, b.Data, boneRotatation.neck);
                        break;
                    case Joint.HEAD:
                        SetJointRotation(charactersJoints.head, b.Data, boneRotatation.head);
                        break;
                    case Joint.HIP_L:
                        SetJointRotation(charactersJoints.leftThigh, b.Data, boneRotatation.legUpperLeft);
                        break;
                    case Joint.HIP_R:
                        SetJointRotation(charactersJoints.rightThigh, b.Data, boneRotatation.legUpperRight);
                        break;
                    case Joint.KNEE_L:
                        SetJointRotation(charactersJoints.leftCalf, b.Data, boneRotatation.legLowerLeft);
                        break;
                    case Joint.KNEE_R:
                        SetJointRotation(charactersJoints.rightCalf, b.Data, boneRotatation.legLowerRight);
                        break;
                    case Joint.FOOTBASE_L:
                        SetJointRotation(charactersJoints.leftFoot, b.Data, boneRotatation.footLeft);
                        break;
                    case Joint.FOOTBASE_R:
                        SetJointRotation(charactersJoints.rightFoot, b.Data, boneRotatation.footRight);
                        break;
                    case Joint.CLAVICLE_L:
                        SetJointRotation(charactersJoints.leftClavicle, b.Data, boneRotatation.clavicleLeft);
                        break;
                    case Joint.CLAVICLE_R:
                        SetJointRotation(charactersJoints.rightClavicle, b.Data, boneRotatation.clavicleRight);
                        break;
                    case Joint.SHOULDER_L:
                        SetJointRotation(charactersJoints.leftUpperArm, b.Data, boneRotatation.armUpperLeft);
                        break;
                    case Joint.SHOULDER_R:
                        SetJointRotation(charactersJoints.rightUpperArm, b.Data, boneRotatation.armUpperRight);
                        break;
                    case Joint.ELBOW_L:
                        SetJointRotation(charactersJoints.leftForearm, b.Data, boneRotatation.armLowerLeft);
                        break;
                    case Joint.ELBOW_R:
                        SetJointRotation(charactersJoints.rightForearm, b.Data, boneRotatation.armLowerRight);
                        break;
                    case Joint.WRIST_L:
                        SetJointRotation(charactersJoints.leftHand, b.Data, boneRotatation.handLeft);
                        break;
                    case Joint.WRIST_R:
                        SetJointRotation(charactersJoints.rightHand, b.Data, boneRotatation.handRight);
                        break;
                    case Joint.HAND_L:
                        if (fingerUse && charactersJoints.fingersLeft != null) 
                            foreach (var fing in charactersJoints.fingersLeft) 
                                SetJointRotation(fing, b.Data, boneRotatation.fingersLeft);
                        break;
                    case Joint.HAND_R:
                        if (fingerUse && charactersJoints.fingersRight != null) 
                            foreach (var fing in charactersJoints.fingersRight) 
                                SetJointRotation(fing, b.Data, boneRotatation.fingersRight);
                        break;
                    case Joint.TRAP_L:
                        if (fingerUse) 
                            SetJointRotation(charactersJoints.thumbLeft, b.Data, boneRotatation.thumbLeft);
                        break;
                    case Joint.TRAP_R:
                        if (fingerUse) 
                            SetJointRotation(charactersJoints.thumbRight, b.Data, boneRotatation.thumbRight);
                        break;
                    case Joint.ANKLE_L:
                    case Joint.ANKLE_R:
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// Find the scale by which the characters positions should change
        /// </summary>
        /// <param name="pelvis">The pelvis where the root position change is applied</param>
        /// <returns>A scaling factor to be applied to the poistional vector</returns>
        private float FindScale(Transform pelvis)
        {
            float pelvisHeight = 0;
            var trans = pelvis;
            do
            {
                pelvisHeight += trans.localPosition.y;
                trans = trans.parent;
            } while (trans.parent && trans.parent != this);
            float s = pelvisHeight / skeleton.Root.Data.Pos.Y;
            s /= transform.localScale.magnitude;
            return s;
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
                    * Quaternion.Euler(boneRotatation.root)
                    ;
            }
        }
        /// <summary>
        /// Checks whether the model has been changed since last and change the model
        /// </summary>
        private void SetModelRotation()
        {
            if (cf != model)
            {
                switch (model)
                {
                    case CharactersModel.Model1:
                        boneRotatation = new Model1();
                        break;
                    case CharactersModel.Model2:
                        boneRotatation = new Model2();
                        break;
                    case CharactersModel.Model3:
                        boneRotatation = new Model3();
                        break;
                    case CharactersModel.Model4:
                        boneRotatation = new Model4();
                        break;
                    case CharactersModel.Model5:
                        boneRotatation = new Model5();
                        break;
                    case CharactersModel.Model6:
                        boneRotatation = new Model6();
                        break;
                    case CharactersModel.Model7:
                        boneRotatation = new Model7();
                        break;
                    default:
                        break;
                }
                cf = model;
            }
        }
        void OnDrawGizmos()
        {
            if (Application.isPlaying && ((debug != null) && streaming && markerData != null && markerData.Count > 0) || LockPosition)
            {
                pos = this.transform.position + debug.Offset;
                if (debug.markers.ShowMarkers)
                {
                    foreach (var lb in markerData)
                    {
                        Gizmos.color = new Color(lb.Color.r, lb.Color.g, lb.Color.b);
                        Gizmos.DrawSphere(lb.Position + pos, debug.markers.MarkerScale);
                    }
                }

                if (debug.markers.MarkerBones && rtClient.Bones != null)
                {
                    foreach (var lb in rtClient.Bones)
                    {
                        var from = markerData.Find(md => md.Label == lb.From).Position + pos;
                        var to = markerData.Find(md => md.Label == lb.To).Position + pos;
                        Debug.DrawLine(from, to, debug.markers.boneColor);
                    }
                }
                if (skeleton == null) return;
                if (debug.showSkeleton || debug.showRotationTrace || debug.showJoints || debug.showConstraints || debug.showTwistConstraints)
                {
                    Gizmos.color = debug.jointColor;

                    foreach (TreeNode<Bone> b in skeleton.Root)
                    {
                        if (debug.showSkeleton)
                        {
                            foreach (TreeNode<Bone> child in b.Children)
                            {
                                UnityEngine.Debug.DrawLine(b.Data.Pos.Convert() + pos, child.Data.Pos.Convert() + pos, debug.skelettColor);
                            }
                        }
                        if (debug.showRotationTrace && (!b.IsLeaf))
                        {
                            UnityDebug.DrawRays(b.Data.Orientation, b.Data.Pos.Convert() + pos, debug.traceLength);
                        }
                        if (debug.showJoints)
                        {
                            Gizmos.DrawSphere(b.Data.Pos.Convert() + pos, debug.jointSize);
                        }
                        if ((debug.showConstraints || debug.showTwistConstraints) && b.Data.HasConstraints)
                        {
                            OpenTK.Quaternion parentRotation =
                                b.Parent.Data.Orientation * b.Data.ParentPointer;
                            OpenTK.Vector3 poss = b.Data.Pos + pos.Convert();
                            if (debug.showConstraints)
                            {
                                UnityDebug.CreateIrregularCone(
                                    b.Data.Constraints, poss,
                                    OpenTK.Vector3.NormalizeFast(
                                        OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, parentRotation)),
                                    parentRotation,
                                    50,//debug.jointsConstrains.coneResolution,
                                    debug.traceLength//debug.jointsConstrains.coneSize
                                    );
                            }
                            if (debug.showTwistConstraints)
                            {
                                UnityDebug.DrawTwistConstraints(b.Data, b.Parent.Data, poss, debug.traceLength);
                            }
                        }
                    }
                }
            }
        }
    }
}



