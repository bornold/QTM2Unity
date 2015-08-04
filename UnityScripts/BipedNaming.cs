using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace QTM2Unity {
	/// <summary>
	/// Class for identifying biped bones based on most common naming conventions.
	/// </summary>
	public class JointNamings {
		
		/// <summary>
		/// Type of the bone.
		/// </summary>
		[System.Serializable]
		public enum JointObject {
			Unassigned,
			Spine,
            Neck,
			Head,
			Arm,
			Leg,
            Fingers,
            Thumb
		}
        /// <summary>
        /// Bone side: Left and Right for limbs and Center for spine, head and tail.
        /// </summary>
        [System.Serializable]
        public enum Side
        {
            Center,
            Left,
            Right
        }
		
		// Bone identifications
        public static string[]
            LeftSide = { " L ", "_L_", "-L-", " l ", "_l_", "-l-", "Left", "left", "CATRigL", "CATL" },
            RightSide = { " R ", "_R_", "-R-", " r ", "_r_", "-r-", "Right", "right", "CATRigR", "CATR" },

            pelvisAlias = { "Pelvis", "pelvis", "Hip", "hip" },
            handAlias = { "Hand", "hand", "Wrist", "wrist", "Palm", "palm" },
            footAlias = { "Foot", "foot", "Ankle", "ankle" },

            spineAlias = { "Spine", "spine", "Pelvis", "pelvis", "Root", "root", "Torso", "torso", "Body", "body", "Hips", "hips", "Chest", "chest" },
            neckAlias = { "Neck", "neck" },
            headAlias = { "Head", "head" },

            armAlias = { "Shoulder", "shoulder", "Collar", "collar", "Clavicle", "clavicle", "Arm", "arm", "Hand", "hand", "Wrist", "wrist", "Elbow", "elbow", "Palm", "palm" },

            legAlias = { "Leg", "leg", "Thigh", "thigh", "Calf", "calf", "Femur", "femur", "Knee", "knee", "Shin", "shin", "Foot", "foot", "Ankle", "ankle", "Hip", "hip" },

            fingerAlias = { "Finger", "finger", "Index", "index", "Mid", "mid", "Pinky", "pinky", "Ring", "ring" },
            thumbAlias = { "Thumb", "thumb", "Finger0", "finger0" },

            exludeName = { "Nub", "Dummy", "dummy", "Tip", "IK", "Mesh", "mesh", "Goal", "goal", "Pole", "pole", "slot" },
            spineExclude = { "Head", "head" },
            headExclude = { "Top", "End" },
            armExclude = { "Finger", "finger", "Index", "index", "Point", "point", "Mid", "mid", "Pinky", "pinky", "Pink", "pink", "Ring", "Thumb", "thumb", "Adjust", "adjust", "Twist", "twist" },
            fingerExclude = { "Thumb", "thumb", "Adjust", "adjust", "Twist", "twist", "Medial", "medial", "Distal", "distal", "Finger0",
                                    "02",
                                    "11","12",
                                    "21","22",
                                    "31","32",
                                    "41","42",
                                    "51","52",
                                },
            thumbExclude = { "Adjust", "adjust", "Twist", "twist", "Medial", "medial", "Distal", "distal" },
            legExclude = { "Toe", "toe", "Platform", "Adjust", "adjust", "Twist", "twist", "Digit", "digit" },
            neckExclude = { };		
		/// <summary>
        /// Returns only the bones with the specified JointObject.
		/// </summary>
		public static Transform[] GetBonesOfType(JointObject type, Transform[] bones) {
            return bones.Where(b => (b != null && GetType(b.name) == type)).ToArray();
		}
		
		/// <summary>
		/// Returns only the bones with the specified Side.
		/// </summary>
		public static Transform[] GetBonesOfSide(Side boneSide, Transform[] bones) {
            return bones.Where(b => (b != null && GetSide(b.name) == boneSide)).ToArray();
		}
		
		/// <summary>
		/// Gets the bones of type and side.
		/// </summary>
		public static Transform[] GetTypeAndSide(JointObject boneType, Side boneSide, Transform[] bones) {
			return GetBonesOfSide(boneSide, GetBonesOfType(boneType, bones));
		}
		
		/// <summary>
		/// Returns only the bones that match all the namings in params string[][] namings
		/// </summary>
		/// <returns>
		/// The matching Transforms
		/// </returns>
		/// <param name='transforms'>
		/// Transforms.
		/// </param>
		/// <param name='namings'>
		/// Namings.
		/// </param>
		public static Transform GetMatch(Transform[] transforms, params string[][] namings) {
           return transforms.FirstOrDefault(t => namings.All(n => Matches(t.name, n)));
		}
		
		/// <summary>
		/// Gets the type of the bone.
		/// </summary>
		public static JointObject GetType(string boneName) {
            // Type Neck
            if (IsOfType(boneName, neckAlias, neckExclude)) return JointObject.Neck;
            // Type Spine
            if (IsOfType(boneName, spineAlias, spineExclude)) return JointObject.Spine;
            // Type Head
            if (IsOfType(boneName, headAlias, headExclude)) return JointObject.Head;
            // Type Arm
            if (IsOfType(boneName, armAlias, armExclude)) return JointObject.Arm;
            // Type Leg
            if (IsOfType(boneName, legAlias, legExclude)) return JointObject.Leg;
            // Type Finger
            if (IsOfType(boneName, fingerAlias, fingerExclude)) return JointObject.Fingers;
            // Type Thumb
            if (IsOfType(boneName, thumbAlias, thumbExclude)) return JointObject.Thumb;
			return JointObject.Unassigned;
		}
		
		/// <summary>
		/// Gets the bone side.
		/// </summary>
		public static Side GetSide(string boneName) {
            if (Matches(boneName, LeftSide) || LastLetter(boneName) == "L" || FirstLetter(boneName) == "L")  return Side.Left;
            else if (Matches(boneName, RightSide) || LastLetter(boneName) == "R" || FirstLetter(boneName) == "R") return Side.Right;
			else return Side.Center;
		}

		/// <summary>
		/// Returns the bone of type and side with additional naming parameters.
		/// </summary>
		public static Transform GetBone(Transform[] transforms, JointObject boneType, Side boneSide = Side.Center, params string[][] namings) {
            return GetMatch(GetTypeAndSide(boneType, boneSide, transforms), namings);
		}
		
        private static bool IsOfType(string boneName, string[] aliases, string[] excusions)
        {
            return Matches(boneName, aliases) && !Exclude(boneName, excusions);
        }
		private static bool Matches(string boneName, string[] possibleNames) {
            return !Exclude(boneName, exludeName) 
                && possibleNames.Any(nc => boneName.Contains(nc));
		}
		
		private static bool Exclude(string boneName, string[] possibleNames) {
            return possibleNames.Any(nc => boneName.Contains(nc));
		}
		
		private static string FirstLetter(string boneName) {
			return (boneName.Length > 0) ? boneName.Substring(0, 1) : "";
		}
		
		private static string LastLetter(string boneName) {
		    return (boneName.Length > 0) ? boneName.Substring(boneName.Length - 1, 1) : "";
		}
	}
}
