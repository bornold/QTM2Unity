using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QTM2Unity {

	/// <summary>
	/// Contains references to bones common to the character.
	/// </summary>
    [System.Serializable]
    public class CharacterGameObjects : IEnumerable<Transform>
    {
        /// <summary>
        /// The root transform is the parent of all the biped's bones and should be located at ground level.
        /// </summary>§
        public Transform root;
        /// <summary>
        /// The pelvis (hip) bone.
        /// </summary>
        public Transform pelvis;
        #region left leg
        /// <summary>
        /// The first bone of the left leg.
        /// </summary>
        public Transform leftThigh;
        /// <summary>
        /// The second bone of the left leg.
        /// </summary>
        public Transform leftCalf;
        /// <summary>
        /// The third bone of the left leg.
        /// </summary>
        public Transform leftFoot;
        #endregion
        #region right leg
        /// <summary>
        /// The first bone of the right leg.
        /// </summary>
        public Transform rightThigh;
        /// <summary>
        /// The second bone of the right leg.
        /// </summary>
        public Transform rightCalf;
        /// <summary>
        /// The third bone of the right leg.
        /// </summary>
        public Transform rightFoot;
        #endregion
        #region left arm
        /// <summary>
        /// The first bone of the left arm.
        /// </summary>
        public Transform leftClavicle;
        /// <summary>
        /// The second bone of the left arm.
        /// </summary>
        public Transform leftUpperArm;
        /// <summary>
        /// The third bone of the left arm.
        /// </summary>
        public Transform leftForearm;
        /// <summary>
        /// The forth bone of the left arm.
        /// </summary>
        public Transform leftHand;
        #endregion
        #region right arm
        /// <summary>
        /// The first bone of the right arm.
        /// </summary>
        public Transform rightClavicle;
        /// <summary>
        /// The second bone of the right arm.
        /// </summary>
        public Transform rightUpperArm;
        /// <summary>
        /// The third bone of the right arm.
        /// </summary>
        public Transform rightForearm;
        /// <summary>
        /// The forth bone of the right arm.
        /// </summary>
        public Transform rightHand;
        #endregion
        /// <summary>
        /// The head.
        /// </summary>
        public Transform head;
        /// <summary>
        /// The first bone after the clavicle joints
        /// </summary>
        public Transform neck;
        /// <summary>
        /// The spine hierarchy. Should not contain any bone deeper in the hierarchy than the arms (neck or head).
        /// </summary>
        public Transform[] spine;
        public Transform shoulderLeft;
        public Transform shoulderRight;

        public Transform thumbLeft;

        public Transform thumbRight;

        public Transform[] fingersLeft;

        public Transform[] fingersRight;

        /// <summary>
        /// Check for null references.
        /// </summary>
        public bool IsValid(bool useFingers)
        {
            return
                root &&
                pelvis &&
                leftThigh &&
                leftCalf &&
                leftFoot &&

                rightThigh &&
                rightCalf &&
                rightFoot &&

                leftClavicle &&
                leftUpperArm &&
                leftForearm &&
                leftHand &&

                rightClavicle &&
                rightUpperArm &&
                rightForearm &&
                rightHand &&
                neck &&
                spine != null &&
                spine.All(s => s) &&
                (useFingers ? IsFingersValid() : true);
        }
            /// <summary>
        /// Check for null references among fingers.
        /// </summary>
        public bool IsFingersValid()
        {
            return
                fingersLeft != null &&
                fingersLeft.All(f => f) &&
                fingersRight != null &&
                fingersRight.All(f => f) &&
                thumbLeft &&
                thumbRight;
        }

        /// <summary>
        /// Sets the references to the joints of a character . Returns true if a valid biped has been referenced.
        /// </summary>
        public bool SetLimbs(Transform root, bool useFingers)
        {
            this.root = root;

            // Find with the help of animator
            AssignHumanoidReferences(root.GetComponent<Animator>(), useFingers);
            if (this.IsValid(useFingers))
            {
                return true;
            }
            else
            {
                this.PrintAll();

                // Try to find by names
                DetectByNaming(root, useFingers);

                if (!IsValid(useFingers))
                {
                    UnityEngine.Debug.LogWarningFormat("{0} contains one or more missing Transforms.", root);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Detects the references based on naming and hierarchy.
        /// </summary>
        public void DetectByNaming(Transform root, bool useFingers)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>();

            // Find limbs
            Transform[] results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Arm, JointNamings.Side.Left, transforms);
            if (results.Length == 4)
            {
                if(!leftClavicle) leftClavicle = results[0];
                if (!leftUpperArm) leftUpperArm = results[1];
                if (!leftForearm) leftForearm = results[2];
                if (!leftHand) leftHand = results[3];
            }

            results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Arm, JointNamings.Side.Right, transforms);
            if (results.Length == 4)
            {
                if (!rightClavicle) rightClavicle = results[0];
                if (!rightUpperArm) rightUpperArm = results[1];
                if (!rightForearm) rightForearm = results[2];
                if (!rightHand) rightHand = results[3];
            }
            else
            {
                foreach (var r in results) UnityEngine.Debug.LogWarning(r);
            }
            results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Leg, JointNamings.Side.Left, transforms);
            if (results.Length == 3 || results.Length == 4)
            {
                if (leftThigh == null) leftThigh = results[0];
                if (leftCalf == null) leftCalf = results[1];
                if (leftFoot == null) leftFoot = results[2];
            }
            else
            {
                foreach (var r in results) UnityEngine.Debug.LogWarning(r);
            }
            results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Leg, JointNamings.Side.Right, transforms);
            if (results.Length == 3 || results.Length == 4)
            {
                if (rightThigh == null) rightThigh = results[0];
                if (rightCalf == null) rightCalf = results[1];
                if (rightFoot == null) rightFoot = results[2];
            }
            else
            {
                foreach (var r in results) UnityEngine.Debug.LogWarning(r);
            }

            // Find fingers
            if (useFingers && !IsFingersValid())
            {
                AddFingers();
            }
            // Find head bone
            if (!head) head = JointNamings.GetBone(transforms, JointNamings.JointObject.Head);
            // Find Neck
            if (!neck) neck = JointNamings.GetBone(transforms, JointNamings.JointObject.Neck);
            // Find Pelvis
            if (!pelvis) pelvis = JointNamings.GetMatch(transforms, JointNamings.pelvisAlias);
            UnityEngine.Debug.LogWarning(pelvis);
            //// If pelvis is not an ancestor of a leg, it is not a valid pelvis
            if (!pelvis || !leftThigh.IsAncestorOf(pelvis))
            {
                UnityEngine.Debug.LogWarning("!!!");
                if (leftThigh) pelvis = leftThigh.parent;
            }
            UnityEngine.Debug.LogWarning(pelvis);
            // Find spine
            Transform left, right;
            if (leftClavicle && rightClavicle)
            {
                left = leftClavicle;
                right = rightClavicle;
            } else
            {
                left = leftUpperArm;
                right = rightUpperArm;
            }

            if (left && right && pelvis)
            {
                Transform lastSpine = left.CommonAncestorOf(right);
                if (lastSpine)
                {
                    spine = GetAncestors(lastSpine, pelvis);
                    // Head is not set
                    if (!head)
                    {
                        for (int i = 0; i < lastSpine.childCount; i++)
                        {
                            Transform child = lastSpine.GetChild(i);

                            if (!child.ContainsChild(left) && !child.ContainsChild(right))
                            {
                                head = child;
                                neck = lastSpine;
                            }
                        }
                    }
                    else if (!neck)
                    {  // if Neck is not  set but head is
                        if (head.IsAncestorOf(lastSpine))
                        {
                            neck = lastSpine;
                        }
                        else
                        {
                            for (int i = 0; i < lastSpine.childCount; i++)
                            {
                                Transform child = lastSpine.GetChild(i);

                                if (!child.ContainsChild(left) && !child.ContainsChild(right))
                                {
                                    neck = child;
                                }
                            }
                        }
                    }
                }
                if (lastSpine == neck) Array.Resize(ref spine, spine.Length - 1);
            }
        }

        /// <summary>
        /// Add gameobjects using the Animator.
        /// </summary>
        public void AssignHumanoidReferences(Animator animator, bool useFingers)
        {
            if (!animator) return;

            if (!pelvis) pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (!head) head = animator.GetBoneTransform(HumanBodyBones.Head);

            if (!neck) neck = animator.GetBoneTransform(HumanBodyBones.Neck);

            if (neck.parent) spine = GetAncestors(neck.parent, pelvis);

            if (!leftClavicle) leftClavicle = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            if (!rightClavicle) rightClavicle = animator.GetBoneTransform(HumanBodyBones.RightShoulder);

            if (!leftThigh) leftThigh = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            if (!leftCalf) leftCalf = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            if (!leftFoot) leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

            if (!rightThigh) rightThigh = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            if (!rightCalf) rightCalf = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            if (!rightFoot) rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            if (!leftClavicle) leftClavicle = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            if (!leftUpperArm) leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            if (!leftForearm) leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            if (!leftHand) leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

            if (!rightClavicle) rightClavicle = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            if (!rightUpperArm) rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            if (!rightForearm) rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            if (!rightHand) rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);


            // Add fingers
            if (useFingers)
            {
                if (!thumbRight) thumbRight = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
                if (!thumbLeft) thumbLeft = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);

                if (fingersLeft == null) fingersLeft = new Transform[4];
                if (!fingersLeft[0]) fingersLeft[0] = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                if (!fingersLeft[1]) fingersLeft[1] = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
                if (!fingersLeft[2]) fingersLeft[2] = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
                if (!fingersLeft[3]) fingersLeft[3] = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);

                if (fingersRight == null) fingersRight = new Transform[4];
                if (!fingersRight[0]) fingersRight[0] = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                if (!fingersRight[1]) fingersRight[1] = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                if (!fingersRight[2]) fingersRight[2] = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
                if (!fingersRight[3]) fingersRight[3] = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
            }
        }

        private bool AddFingers() 
        {
            var children = leftHand.DirectChildren();
            thumbLeft = JointNamings.GetBone(children, JointNamings.JointObject.Thumb, JointNamings.Side.Left);
            Transform[] results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Fingers, JointNamings.Side.Left, children);
            if (results.Length == 4)
            {
                fingersLeft = new Transform[4];
                fingersLeft[0] = results[0];
                fingersLeft[1] = results[1];
                fingersLeft[2] = results[2];
                fingersLeft[3] = results[3];
                UnityEngine.Debug.LogWarning(fingersLeft[3]);
            }
            else if (leftHand && leftHand.childCount >= 5)
            {
                if (!thumbLeft) thumbLeft = leftHand.GetChild(0);
                    
                fingersLeft = new Transform[4];
                fingersLeft[0] = leftHand.GetChild(1);
                fingersLeft[1] = leftHand.GetChild(2);
                fingersLeft[2] = leftHand.GetChild(3);
                fingersLeft[3] = leftHand.GetChild(4);
            }
            else
            {
                fingersLeft = new Transform[1];
                fingersLeft[0] = results[0];
            }

            children = rightHand.DirectChildren();
            thumbRight = JointNamings.GetBone(children, JointNamings.JointObject.Thumb, JointNamings.Side.Right);
            results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Fingers, JointNamings.Side.Right, rightHand.DirectChildren());
            if (results.Length == 4)
            {
                fingersRight = new Transform[4];
                fingersRight[0] = results[0];
                fingersRight[1] = results[1];
                fingersRight[2] = results[2];
                fingersRight[3] = results[3];
            }
            else if (rightHand && rightHand.childCount >= 5)
            {
                if (!thumbRight) thumbRight = leftHand.GetChild(0);
                fingersRight = new Transform[4];
                fingersRight[0] =leftHand.GetChild(1);
                fingersRight[1] = leftHand.GetChild(2);
                fingersRight[2] = leftHand.GetChild(3);
                fingersRight[3] = leftHand.GetChild(4);
            }
            else
            {
                fingersLeft = new Transform[1];
                fingersLeft[0] = results[0];
            }
            return IsFingersValid();
        }

        // Determines whether a bone is valid for being added into the spine
        private bool AddBoneToSpine(Transform bone)
        {
            if (bone == root) return false;

            bool isLegsParent = bone == leftThigh.parent;

            if (pelvis != null)
            {
                if (bone == pelvis) return false;
                if (pelvis.IsAncestorOf(bone)) return false;
            }

            return true;
        }
        /// <summary>
        /// Returns a array of all ancestors of Transform 1 until given Transform 2 or no more parents. Including Transform 1
        /// </summary>
        /// <param name="from">The starting transform, array is inclusive this</param>
        /// <param name="until">The transform to stop at, the array is exclusive this.</param>
        /// <returns>An array with the transform between From and Until</returns>
        public Transform[] GetAncestors(Transform from, Transform until)
        {
            List<Transform> between = new List<Transform> ();
            var temp = from;
            while (temp && temp != until)
            {
                if (temp.position != until.position && temp.parent.position != temp.position)
                {
                    between.Add(temp);
                }
                temp = temp.parent;
            }
            between.Reverse();
            return between.ToArray();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Transform> GetEnumerator()
        {
            yield return root;
            yield return pelvis;
            if (spine != null)
                foreach (var s in spine)
                    yield return s;

            yield return neck;
            yield return head;
            
            yield return leftThigh;
            yield return leftCalf;
            yield return leftFoot;
            
            yield return rightThigh;
            yield return rightCalf;
            yield return rightFoot;
            
            yield return leftClavicle;
            yield return leftUpperArm;
            yield return leftForearm;
            yield return leftHand;
            yield return thumbLeft;
            if (fingersLeft != null) 
                foreach (var f in fingersLeft) 
                    yield return f;

            yield return rightClavicle;
            yield return rightUpperArm;
            yield return rightForearm;
            yield return rightHand;
            yield return thumbRight;
            if (fingersRight != null) 
                foreach (var f in fingersRight) 
                    yield return f;
        }
        public void PrintAll()
        {
            UnityEngine.Debug.LogFormat("root {0}", root);
            UnityEngine.Debug.LogFormat("pelvis {0}", pelvis);
            if (spine != null)
                for (int s = 0; s < spine.Length; s++)
                    UnityEngine.Debug.LogFormat("spine{0} {1}", s, spine[s]);
            UnityEngine.Debug.LogFormat("neck {0}", neck);
            UnityEngine.Debug.LogFormat("head {0}", head);

            UnityEngine.Debug.LogFormat("leftThigh {0}", leftThigh);
            UnityEngine.Debug.LogFormat("leftCalf {0}", leftCalf);
            UnityEngine.Debug.LogFormat("leftFoot {0}", leftFoot);

            UnityEngine.Debug.LogFormat("rightThigh {0}", rightThigh);
            UnityEngine.Debug.LogFormat("rightCalf {0}", rightCalf);
            UnityEngine.Debug.LogFormat("rightFoot {0}", rightFoot);

            UnityEngine.Debug.LogFormat("leftClavicle {0}", leftClavicle);
            UnityEngine.Debug.LogFormat("leftUpperArm {0}", leftUpperArm);
            UnityEngine.Debug.LogFormat("leftForearm {0}", leftForearm);
            UnityEngine.Debug.LogFormat("leftHand {0}", leftHand);
            UnityEngine.Debug.LogFormat("thumbLeft {0}", thumbLeft);
            if (fingersLeft != null)
                for (int f = 0; f < fingersLeft.Length; f++) 
                    UnityEngine.Debug.LogFormat("fingersLeft{0} {1}", f, fingersLeft[f]);

            UnityEngine.Debug.LogFormat("rightClavicle {0}", rightClavicle);
            UnityEngine.Debug.LogFormat("rightUpperArm {0}", rightUpperArm);
            UnityEngine.Debug.LogFormat("rightForearm {0}", rightForearm);
            UnityEngine.Debug.LogFormat("rightHand {0}", rightHand);
            UnityEngine.Debug.LogFormat("thumbRight {0}", thumbRight);
            if (fingersRight != null)
                for (int f = 0; f < fingersRight.Length; f++ ) 
                    UnityEngine.Debug.LogFormat("fingersRight{0} {1}", f, fingersRight[f]);
        }
    }
}
