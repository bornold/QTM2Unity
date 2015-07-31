using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

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
        public Transform[] spine = new Transform[0];
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
            bool valids =
                root  &&
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
                spine != null;
            foreach (var s in spine) if (s == null) return false;
            if (useFingers) valids &= IsFingersValid();
            return valids;
        }
            /// <summary>
        /// Check for null references among fingers.
        /// </summary>
        public bool IsFingersValid()
        {
            if (fingersLeft == null) return false;
            foreach (Transform s in fingersLeft) if (s == null) return false;
            if (fingersRight == null) return false;
            foreach (Transform s in fingersRight) if (s == null) return false;
            if (thumbLeft == null) return false;
            if (thumbRight == null) return false;
            return true;
        }


        /// <summary>
        /// Params for automatic biped recognition. (Using a struct here because I might need to add more parameters in the future).
        /// </summary>
        public struct Params
        {
            /// <summary>
            /// Should the immediate parent of the legs be included in the spine?.
            /// </summary>
            public bool legsParentInSpine;
            public bool useFingers;
            public Params(bool legsParentInSpine, bool useFingers)
            {
                this.legsParentInSpine = legsParentInSpine;
                this.useFingers = useFingers;
            }
        }

        /// <summary>
        /// Automatically detects biped bones. Returns true if a valid biped has been referenced.
        /// </summary>
        public static bool FindLimbs(ref CharacterGameObjects references, Transform root, Params autoDetectParams)
        {
            if (references == null) references = new CharacterGameObjects();
            references.root = root;

            // Find with the help of animator
            AssignHumanoidReferences(ref references, root.GetComponent<Animator>(), autoDetectParams);
            bool isValid = references.IsValid(autoDetectParams.useFingers);
            UnityEngine.Debug.LogWarning("BY animator");
            if (isValid)
            {
                return true;
            }
            else
            {
                references.PrintAll();

                // Try to find by names
                DetectReferencesByNaming(ref references, root, autoDetectParams);

                UnityEngine.Debug.LogWarning("BY name");
                isValid = references.IsValid(autoDetectParams.useFingers);

                if (!isValid)
                {
                    UnityEngine.Debug.LogWarningFormat("{0} contains one or more missing Transforms.", root);
                }

                return isValid;
            }
        }

        /// <summary>
        /// Detects the references based on naming and hierarchy.
        /// </summary>
        public static void DetectReferencesByNaming(ref CharacterGameObjects references, Transform root, Params autoDetectParams)
        {
            if (references == null) references = new CharacterGameObjects();

            Transform[] children = root.GetComponentsInChildren<Transform>();

            // Find limbs
            Transform[] results = BipedNaming.GetBonesOfTypeAndSide(BipedNaming.BoneType.Arm, BipedNaming.BoneSide.Left, children);
            if (results.Length == 4)
            {
                if(references.leftClavicle == null) references.leftClavicle = results[0];
                if (references.leftUpperArm == null) references.leftUpperArm = results[1];
                if (references.leftForearm == null) references.leftForearm = results[2];
                if (references.leftHand == null) references.leftHand = results[3];
            }

            results = BipedNaming.GetBonesOfTypeAndSide(BipedNaming.BoneType.Arm, BipedNaming.BoneSide.Right, children);
            if (results.Length == 4)
            {
                if (references.rightClavicle == null) references.rightClavicle = results[0];
                if (references.rightUpperArm == null) references.rightUpperArm = results[1];
                if (references.rightForearm == null) references.rightForearm = results[2];
                if (references.rightHand == null) references.rightHand = results[3];
            }
            else
            {
                foreach (var r in results) UnityEngine.Debug.LogWarning(r);
            }
            results = BipedNaming.GetBonesOfTypeAndSide(BipedNaming.BoneType.Leg, BipedNaming.BoneSide.Left, children);
            if (results.Length == 3 || results.Length == 4)
            {
                if (references.leftThigh == null) references.leftThigh = results[0];
                if (references.leftCalf == null) references.leftCalf = results[1];
                if (references.leftFoot == null) references.leftFoot = results[2];
            }
            else
            {
                foreach (var r in results) UnityEngine.Debug.LogWarning(r);
            }
            results = BipedNaming.GetBonesOfTypeAndSide(BipedNaming.BoneType.Leg, BipedNaming.BoneSide.Right, children);
            if (results.Length == 3 || results.Length == 4)
            {
                if (references.rightThigh == null) references.rightThigh = results[0];
                if (references.rightCalf == null) references.rightCalf = results[1];
                if (references.rightFoot == null) references.rightFoot = results[2];
            }
            else
            {
                foreach (var r in results) UnityEngine.Debug.LogWarning(r);
            }

            // Find fingers
            if (autoDetectParams.useFingers && !references.IsFingersValid())
            {
                AddFingers(children, ref references);
            }
            // Find head bone
            if (!references.head) references.head = BipedNaming.GetBone(children, BipedNaming.BoneType.Head);
            // Find Neck
            if (!references.neck) references.neck = BipedNaming.GetBone(children, BipedNaming.BoneType.Neck);
            // Find Pelvis
            if (!references.pelvis) references.pelvis = BipedNaming.GetNamingMatch(children, BipedNaming.pelvis);
            //// If pelvis is not an ancestor of a leg, it is not a valid pelvis
            if (references.pelvis == null || !IsAncestor(references.leftThigh, references.pelvis))
            {
                if (references.leftThigh != null) references.pelvis = references.leftThigh.parent;
            }

            // Find spine and head bones
            if (references.leftUpperArm && references.rightUpperArm && references.pelvis && references.leftThigh)
            {
                Transform lastSpine = GetFirstCommonAncestor(references.leftUpperArm, references.rightUpperArm);

                if (lastSpine)
                {
                    Transform[] inverseSpine = new Transform[1] { lastSpine };
                    AddAncestors(inverseSpine[0], references.pelvis, ref inverseSpine);

                    references.spine = new Transform[0];
                    for (int i = inverseSpine.Length - 1; i > -1; i--)
                    {
                        if (AddBoneToSpine(inverseSpine[i], ref references, autoDetectParams))
                        {
                            Array.Resize(ref references.spine, references.spine.Length + 1);
                            references.spine[references.spine.Length - 1] = inverseSpine[i];
                        }
                    }
                    if (lastSpine == references.neck) 
                    // Head
                    if (!references.head)
                    {
                        for (int i = 0; i < lastSpine.childCount; i++)
                        {
                            Transform child = lastSpine.GetChild(i);

                            if (!ContainsChild(child, references.leftUpperArm) && !ContainsChild(child, references.rightUpperArm))
                            {
                                references.head = child;
                                references.neck = lastSpine;
                            }
                        }
                    } else if (!references.neck ){
                        // Neck
                        if (IsAncestor(references.head, lastSpine))
                        {
                            references.neck = lastSpine;
                        }
                        else
                        {
                            for (int i = 0; i < lastSpine.childCount; i++)
                            {
                                Transform child = lastSpine.GetChild(i);

                                if (!ContainsChild(child, references.leftUpperArm) && !ContainsChild(child, references.rightUpperArm))
                                {
                                    references.neck = child;
                                }
                            }
                        }
                    }
                }
                if (lastSpine == references.neck) Array.Resize(ref references.spine, references.spine.Length - 1);
            }

        }

        /// <summary>
        /// Fills in BipedReferences using Animator.GetBoneTransform().
        /// </summary>
        public static void AssignHumanoidReferences(ref CharacterGameObjects references, Animator animator, Params autoDetectParams)
        {
            if (references == null) references = new CharacterGameObjects();
            if (!animator) return;

            if (!references.head) references.head = animator.GetBoneTransform(HumanBodyBones.Head);

            if (!references.leftThigh) references.leftThigh = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            if (!references.leftCalf) references.leftCalf = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            if (!references.leftFoot) references.leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

            if (!references.rightThigh) references.rightThigh = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            if (!references.rightCalf) references.rightCalf = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            if (!references.rightFoot) references.rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            if (!references.leftClavicle) references.leftClavicle = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            if (!references.leftUpperArm) references.leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            if (!references.leftForearm) references.leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            if (!references.leftHand) references.leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

            if (!references.rightClavicle) references.rightClavicle = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            if (!references.rightUpperArm) references.rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            if (!references.rightForearm) references.rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            if (!references.rightHand) references.rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

            if (!references.pelvis) references.pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (!references.neck) references.neck = animator.GetBoneTransform(HumanBodyBones.Neck);

            if (!references.leftClavicle) references.leftClavicle = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            if (!references.rightClavicle) references.rightClavicle = animator.GetBoneTransform(HumanBodyBones.RightShoulder);

            // Add fingers
            if (autoDetectParams.useFingers)
            {
                if (!references.thumbRight) references.thumbRight = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
                if (!references.thumbLeft) references.thumbLeft = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);

                if (references.fingersLeft == null) references.fingersLeft = new Transform[4];
                if (!references.fingersLeft[0]) references.fingersLeft[0] = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                if (!references.fingersLeft[1]) references.fingersLeft[1] = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
                if (!references.fingersLeft[2]) references.fingersLeft[2] = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
                if (!references.fingersLeft[3]) references.fingersLeft[3] = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);

                if (references.fingersRight == null) references.fingersRight = new Transform[4];
                if (!references.fingersRight[0]) references.fingersRight[0] = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                if (!references.fingersRight[1]) references.fingersRight[1] = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                if (!references.fingersRight[2]) references.fingersRight[2] = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
                if (!references.fingersRight[3]) references.fingersRight[3] = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
            }
            if (references.spine == null) {
                references.spine = new Transform[0];
                AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Spine));
                AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform(HumanBodyBones.Chest));
            }
        }

        private static bool AddFingers(Transform[] children, ref CharacterGameObjects references) 
        {
            references.thumbLeft = BipedNaming.GetBone(children, BipedNaming.BoneType.Thumb, BipedNaming.BoneSide.Left);
            references.thumbRight = BipedNaming.GetBone(children, BipedNaming.BoneType.Thumb, BipedNaming.BoneSide.Right);

            Transform[] results = BipedNaming.GetBonesOfTypeAndSide(BipedNaming.BoneType.Fingers, BipedNaming.BoneSide.Left, children);
            if (results.Length == 4)
            {
                references.fingersLeft = new Transform[4];
                references.fingersLeft[0] = results[0];
                references.fingersLeft[1] = results[1];
                references.fingersLeft[2] = results[2];
                references.fingersLeft[3] = results[3];
                UnityEngine.Debug.LogWarning(references.fingersLeft[3]);
            }
            else if (references.leftHand && references.leftHand.childCount >= 5)
            {
                if (!references.thumbLeft) references.thumbLeft = references.leftHand.GetChild(0);
                    
                references.fingersLeft = new Transform[4];
                references.fingersLeft[0] = references.leftHand.GetChild(1);
                references.fingersLeft[1] = references.leftHand.GetChild(2);
                references.fingersLeft[2] = references.leftHand.GetChild(3);
                references.fingersLeft[3] = references.leftHand.GetChild(4);
            }
            else
            {
                references.fingersLeft = new Transform[1];
                references.fingersLeft[0] = results[0];
            }

            results = BipedNaming.GetBonesOfTypeAndSide(BipedNaming.BoneType.Fingers, BipedNaming.BoneSide.Right, children);
            if (results.Length == 4)
            {
                references.fingersRight = new Transform[4];
                references.fingersRight[0] = results[0];
                references.fingersRight[1] = results[1];
                references.fingersRight[2] = results[2];
                references.fingersRight[3] = results[3];
            }
            else if (references.rightHand && references.rightHand.childCount >= 5)
            {
                if (!references.thumbRight) references.thumbRight = references.leftHand.GetChild(0);
                references.fingersRight = new Transform[4];
                references.fingersRight[0] =references.leftHand.GetChild(1);
                references.fingersRight[1] = references.leftHand.GetChild(2);
                references.fingersRight[2] = references.leftHand.GetChild(3);
                references.fingersRight[3] = references.leftHand.GetChild(4);
            }
            else
            {
                references.fingersLeft = new Transform[1];
                references.fingersLeft[0] = results[0];
            }
            return references.IsFingersValid();
        }

        // Determines whether a bone is valid for being added into the spine
        private static bool AddBoneToSpine(Transform bone, ref CharacterGameObjects references, Params autoDetectParams)
        {
            if (bone == references.root) return false;

            bool isLegsParent = bone == references.leftThigh.parent;
            if (isLegsParent && !autoDetectParams.legsParentInSpine) return false;

            if (references.pelvis != null)
            {
                if (bone == references.pelvis) return false;
                if (IsAncestor(references.pelvis, bone)) return false;
            }

            return true;
        }

        // Adds transform to hierarchy if not null
        private static void AddBoneToHierarchy(ref Transform[] bones, Transform transform)
        {
            if (transform == null) return;

            Array.Resize(ref bones, bones.Length + 1);
            bones[bones.Length - 1] = transform;
        }


        /// <summary>
        /// Determines whether the second Transform is an ancestor to the first Transform.
        /// </summary>
        public static bool IsAncestor(Transform transform, Transform ancestor)
        {
            if (transform == null) return true;
            if (ancestor == null) return true;
            if (transform.parent == null) return false;
            if (transform.parent == ancestor) return true;
            return IsAncestor(transform.parent, ancestor);
        }

        /// <summary>
        /// Returns true if the transforms contains the child
        /// </summary>
        public static bool ContainsChild(Transform transform, Transform child)
        {
            if (transform == child) return true;

            Transform[] children = transform.GetComponentsInChildren<Transform>() as Transform[];
            foreach (Transform c in children) if (c == child) return true;
            return false;
        }

        /// <summary>
        /// Adds all Transforms until the blocker to the array
        /// </summary>
        public static void AddAncestors(Transform transform, Transform blocker, ref Transform[] array)
        {
            if (transform.parent != null && transform.parent != blocker)
            {
                if (transform.parent.position != transform.position && transform.parent.position != blocker.position)
                {
                    Array.Resize(ref array, array.Length + 1);
                    array[array.Length - 1] = transform.parent;
                }
                AddAncestors(transform.parent, blocker, ref array);
            }
        }

        /// <summary>
        /// Gets the first common ancestor up the hierarchy
        /// </summary>
        public static Transform GetFirstCommonAncestor(Transform t1, Transform t2)
        {
            if (t1 == null) return null;
            if (t2 == null) return null;
            if (t1.parent == null) return null;
            if (t2.parent == null) return null;

            if (IsAncestor(t2, t1.parent)) return t1.parent;
            return GetFirstCommonAncestor(t1.parent, t2);
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
