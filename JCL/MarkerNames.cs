using System;
using System.Collections;
using System.Collections.Generic;
namespace QTM2Unity
{
    class MarkersNames : IEnumerable<string>
    {
        public string
            bodyBase = "Sacrum",
            leftHip = "L_IAS",
            rightHip = "R_IAS",
            spine= "TV12",
            neck =  "TV2",
            chest =  "SME",
            leftShoulder = "L_SAE",
            rightShoulder = "R_SAE",
            head =  "SGL",
            leftHead = "L_HEAD",
            rightHead = "R_HEAD",
            leftElbow  = "L_UOA",
            leftInnerElbow  = "L_HME",
            leftOuterElbow  = "L_HLE",
            rightElbow  = "R_UOA",
            rightInnerElbow  = "R_HME",
            rightOuterElbow  = "R_HLE",
            leftWrist  = "L_USP",
            leftWristRadius  = "L_RSP",
            leftHand  = "L_HM2",
            leftIndex = "L_Index",
            leftThumb = "L_Thumb",
            rightWrist  = "R_USP",
            rightWristRadius = "R_RSP",
            rightHand  = "R_HM2",
            rightIndex = "R_Index",
            rightThumb = "R_Thumb",
            leftUpperKnee  = "L_PAS",
            leftOuterKnee  = "L_FLE",
            leftInnerKnee  = "L_FME",
            leftLowerKnee  = "L_TTC",
            rightUpperKnee = "R_PAS",
            rightOuterKnee  = "R_FLE",
            rightInnerKnee  = "R_FME",
            rightLowerKnee  = "R_TTC",
            leftOuterAnkle  = "L_FAL",
            leftInnerAnkle  = "L_TAM",
            leftHeel  = "L_FCC",
            rightOuterAnkle  = "R_FAL",
            rightInnerAnkle  = "R_TAM",
            rightHeel  = "R_FCC",
            leftToe1  = "L_FM1",
            leftToe2  = "L_FM2",
            leftToe5 = "L_FM5",
            rightToe1  = "R_FM1",
            rightToe2  = "R_FM2",
            rightToe5  = "R_FM5";


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            yield return bodyBase;
            yield return leftHip;
            yield return rightHip;
            yield return spine;
            yield return neck;
            yield return chest;
            yield return leftShoulder;
            yield return rightShoulder;
            yield return head;
            yield return leftHead;
            yield return rightHead;
            yield return leftElbow;
            yield return leftInnerElbow ;
            yield return leftOuterElbow;
            yield return rightElbow;
            yield return rightInnerElbow;
            yield return rightOuterElbow;
            yield return leftWrist;
            yield return leftWristRadius;
            yield return leftHand ;
            yield return leftIndex;
            yield return leftThumb;
            yield return rightWrist;
            yield return rightWristRadius;
            yield return rightHand;
            yield return rightIndex;
            yield return rightThumb;
            yield return leftUpperKnee;
            yield return leftOuterKnee;
            yield return leftInnerKnee;
            yield return leftLowerKnee;
            yield return rightUpperKnee;
            yield return rightOuterKnee;
            yield return rightInnerKnee;
            yield return rightLowerKnee;
            yield return leftOuterAnkle;
            yield return leftInnerAnkle;
            yield return leftHeel;
            yield return rightOuterAnkle;
            yield return rightInnerAnkle;
            yield return rightHeel;
            yield return leftToe1;
            yield return leftToe2;
            yield return leftToe5;
            yield return rightToe1;
            yield return rightToe2;
            yield return rightToe5;
        }
    }
    [System.Serializable]
    static class MarkerNames
    {
        public static readonly List<string>
         BodyBaseAKA = new List<string>() { "SACR", "SACRUM", "LOWER_LUMBAR", "LV5_S1", "S1" },
         HipAKA = new List<string>() { "IAS", "ASIS", "ASIS", "HIP", "ICT", "Hip Front", "FWT" },
         SpineAKA = new List<string>() 
                {   "TV12", "TH12", "D12", "T12",
                    "TV11", "TH11", "D11", "T11", 
                    "TV13", "TH13", "D13", "T13", 
                    "TV10", "TH10", "D10", "T10", 
                    "TV14", "TV14", "D14", "T14",
                    "L1",   "LV3"       },

     NeckAKA = new List<string>() { "TV2", "TV1", "C7", "C7_TOP_SPINE" },
     ChestAKA = new List<string>() { "SME", "SJN", "CLAV" },
     ShoulderAKA = new List<string>() { "SAE", "ACR", "SHOULDER", "ACROMION", "SHO" },
     
     FrontHeadAKA = new List<string>() { "SGL", "Front of Head", "F_HEAD" },
     SideHeadAKA = new List<string>() { "HEAD", "HEADFRONT", "Head", "HEAD", "BHD" },

     ElbowAKA = new List<string>() { "UOA", "ELB", "ELBOW" },
     InnerElbowAKA = new List<string>() { "HME", "ELBOW_MED" },
     OuterElbowAKA = new List<string>() { "HLE", "ELBOW", "Elbow", "ELB" },
     
     WristAKA = new List<string>() { "USP", "ULNA", "WRIST_INNER", "WRIST_MED", "Wrist Ulna", "Ulna", "WRB" },
     WristRadiusAKA = new List<string>() { "RSP", "RADUIS", "WRIST_OUTER", "WRIST_LAT", "Wrist Radius", "Radius", "WRA" },
     
     HandAKA = new List<string>() { "HM2", "3rd Digita", "FIN" },
     IndexAKA = new List<string>() { "Index", "INDEX1" },
     ThumbAKA = new List<string>() { "Thumb", "THUMB" },
     
     UpperKneeAKA = new List<string>() { "PAS", "SUPPAT" },
     OuterKneeAKA = new List<string>() { "FLE", "LKNEE", "KNEE", "knjntln", "Knee", "KNEE_LAT", "KNE" },
     InnerKneeAKA = new List<string>() { "FME", "MKNEE", "MEDIAL_KNEE", "Medial Knee", "KNEE_MED" },
     LowerKneeAKA = new List<string>() { "TTC", "tubtib", "Tibia", "TIB_1" },
     
     OuterAnkleAKA = new List<string>() { "FAL", "LMAL", "ANKLE_OUTER", "ankle", "Ankle", "ANKLE_LAT", "ANK" },
     InnerAnkleAKA = new List<string>() { "TAM", "MMAL", "MEDIAL_ANKLE", "Medial Ankle", "ANKLE_MED" },
     
     HeelAKA = new List<string>() { "FCC", "HEEL_CALC", "HEEL", "heel", "FCC1", "FCC2", "Heel", "HEE" },

     Toe1AKA = new List<string>() { "FM1", "TOE_1_MET", "MT_1", "TOE" },
     Toe2AKA = new List<string>() { "FM2", "toe", "2nd Toe" },
     Toe5AKA = new List<string>() { "FM5", "TOE_5_MET", "5th Toe", "MT_5", "MT5" }
     ;

    public static readonly List<string[]>
        bodyBasebetween = new List<string[]>()
            {
                new string[] { "R_IPS", "L_IPS" }, 
                new string[] { "Rt Lower PSIS", "Lt Lower PSIS" }, 
                new string[] { "L_Sacrum", "R_Sacrum" },
                new string[] { "RBWT", "LBWT"} 
            },
        neckBetween = new List<string[]>() { 
            new string[] { "Lt Up Back", "Rt Up Back" } },
        headBetween = new List<string[]>() 
            {   
                new string[] {"head_left_front", "head_right_front"},
                new string[] {"RFHD", "LFHD"}
            },
        rightToe2Between = new List<string[]>() 
            { 
                new string[] { "FM1", "FM5" }, 
                new string[] { "TOE_1_MET", "TOE_5_MET" }, 
                new string[] { "MT_1", "MT_5" } 
            };

        #region hip
        public static string 
            bodyBase = "SACR";
        public static readonly List<string>
            bodyBaseAKA = new List<string>() { "SACR", "SACRUM", "LOWER_LUMBAR", "LV5_S1", "S1" };
        public static string 
            leftHip = "L_IAS";
        public static readonly List<string> 
            leftHipAKA = new List<string>() { "L_IAS", "L_ASIS","LASIS","LEFT_HIP","L_ICT","Lt Hip Front", "LFWT"};
        public static string 
            rightHip = "R_IAS";
        public static readonly List<string>
            rightHipAKA = new List<string>() { "R_IAS", "R_ASIS", "RASIS", "RIGHT_HIP", "R_ICT", "Rt Hip Front", "RFWT" };
        #endregion
        #region upperbody
        public static string
            spine= "TV12";
        public static readonly List<string>
            spineAKA = new List<string>() 
            {   "TV12", "TH12", "D12", "T12",
                "TV11", "TH11", "D11", "T11", 
                "TV13", "TH13", "D13", "T13", 
                "TV10", "TH10", "D10", "T10", 
                "TV14", "TV14", "D14", "T14",
                "L1",   "LV3"       };
        public static string 
            neck =  "TV2";
        public static readonly List<string> 
            neckAKA = new List<string>() { "TV2", "TV1", "C7", "C7_TOP_SPINE" };

        public static string 
            chest =  "SME";
        public static readonly List<string>
            chestAKA = new List<string>() { "SME", "SJN", "CLAV" };
        public static string 
            leftShoulder = "L_SAE";
        public static readonly List<string>
            leftShoulderAKA = new List<string>() {"L_SAE", "L_ACR", "LEFT_SHOULDER", "L_SHOULDER", "L_ACROMION", "LEFTSHOULDER", "LEFT SHOULDER", "LSHO" };
        public static string 
            rightShoulder = "R_SAE";
        public static readonly List<string>
            rightShoulderAKA = new List<string>() { "R_SAE", "R_ACR", "RIGHT_SHOULDER", "R_SHOULDER", "R_ACROMION", "RIGHTSHOULDER", "RIGHT SHOULDER", "RSHO" };
        #endregion
        #region head
        public static string 
            head =  "SGL";
        public static readonly List<string>
            headAKA = new List<string>() {"SGL", "Front of Head", "F_HEAD" };

        public static string 
            leftHead = "L_HEAD";
        public static readonly List<string>
            leftHeadAKA = new List<string>() { "L_HEAD", "HEAD_LEFT_FRONT", "Left Head", "L_HEAD", "LBHD" };
        public static string 
            rightHead = "R_HEAD";
        public static readonly List<string>
            rightHeadAKA = new List<string>() { "R_HEAD", "HEAD_RIGHT_FRONT", "Right Head", "R_HEAD", "RBHD" };
        #endregion
        #region elbow
        #region left elbow
        public static string 
            leftElbow  = "L_UOA";
        public static readonly List<string> 
            leftElbowAKA = new List<string>() {"L_UOA", "L_ELB", "LEFT_ELBOW" };
        public static string 
            leftInnerElbow  = "L_HME";
        public static readonly List<string> 
            leftInnerElbowAKA = new List<string>() { "L_HME", "L_ELBOW_MED" };
        public static string 
            leftOuterElbow  = "L_HLE";
        public static readonly List<string>
            leftOuterElbowAKA = new List<string>() { "L_HLE", "L_ELBOW", "Lt Elbow", "LELB" };
        #endregion
        #region right elbow

        public static string 
            rightElbow  = "R_UOA";
        public static readonly List<string>
            rightElbowAKA = new List<string>() {"R_UOA", "R_ELB", "RIGHT_ELBOW" }; 
        public static string 
            rightInnerElbow  = "R_HME";
        public static readonly List<string>
            rightInnerElbowAKA = new List<string>() { "R_HME", "R_ELBOW_MED" };
        public static string 
            rightOuterElbow  = "R_HLE";
        public static readonly List<string>
            rightOuterElbowAKA = new List<string>() { "R_HLE", "R_ELBOW", "Rt Elbow", "RELB" };
        #endregion
        #endregion
        #region hand
        #region lefthand
        public static string 
            leftWrist  = "L_USP";
        public static readonly List<string>
            leftWristAKA = new List<string>() { "L_USP", "L_ULNA", "LEFT_WRIST_INNER", "L_WRIST_MED", "Lt Wrist Ulna", "LT Ulna", "LWRB" };
        public static string 
            leftWristRadius  = "L_RSP";
        public static readonly List<string>
            leftWristRadiusAKA = new List<string>() { "L_RSP", "L_RADUIS", "LEFT_WRIST_OUTER", "L_WRIST_LAT", "Lt Wrist Radius", "Lt Radius", "LWRA" };
        public static string 
            leftHand  = "L_HM2";
        public static readonly List<string>
            leftHandAKA = new List<string>() { "L_HM2", "Lt 3rd Digita", "LFIN" };
        public static string
            leftIndex = "L_Index";
        public static readonly List<string>
            leftIndexAKA = new List<string>() { "L_Index", "L_INDEX1" };
        public static string
            leftThumb = "L_Thumb";
        public static readonly List<string>
            leftThumbAKA = new List<string>() { "L_Thumb","L_THUMB" };
        #endregion
        #region right hand
        public static string 
            rightWrist  = "R_USP";
        public static readonly List<string>
            rightWristAKA = new List<string>() { "R_USP", "R_ULNA", "RIGHT_WRIST_INNER", "R_WRIST_MED", "Rt Wrist Ulna", "RT Ulna", "RWRB" };
        public static string 
            rightWristRadius  = "R_RSP";
        public static readonly List<string>
            rightWristRadiusAKA = new List<string>() {"R_RSP", "R_RADUIS", "RIGHT_WRIST_OUTER", "R_WRIST_LAT", "Rt Wrist Radius" ,"RT Radius", "RWRA" };
        public static string 
            rightHand  = "R_HM2";
        public static readonly List<string>
            rightHandAKA = new List<string>() { "R_HM2", "Rt 3rd Digita", "RFIN" };
        public static string
            rightIndex = "R_Index";
        public static readonly List<string>
            rightIndexAKA = new List<string>() { "R_Index", "R_INDEX1" };
        public static string
            rightThumb = "R_Thumb";
        public static readonly List<string>
            rightThumbAKA = new List<string>() { "R_Thumb", "R_THUMB" };
        #endregion
        #endregion
        #region knee
        #region left knee
        public static string 
            leftUpperKnee  = "L_PAS";
        public static readonly List<string>
            leftUpperKneeAKA = new List<string>() { "L_PAS", "L_SUPPAT" };
        public static string 
            leftOuterKnee  = "L_FLE";
        public static readonly List<string>
            leftOuterKneeAKA = new List<string>() { "L_FLE", "L_LKNEE", "LEFT_KNEE", "l_knjntln", "Lt Lat Knee", "L_KNEE_LAT", "LKNE" };
        public static string 
            leftInnerKnee  = "L_FME";
        public static readonly List<string>
            leftInnerKneeAKA = new List<string>() { "L_FME", "L_MKNEE", "LEFT_MEDIAL_KNEE", "Lt Medial Knee", "L_KNEE_MED" };
        public static string 
            leftLowerKnee  = "L_TTC";
        public static readonly List<string>
            leftLowerKneeAKA = new List<string>() { "L_TTC", "l_tubtib", "Lt Tibia", "L_TIB_1" };
        #endregion
        #region right knee

        public static string 
            rightUpperKnee = "R_PAS";
        public static readonly List<string>
            rightUpperKneeAKA = new List<string>() {"R_PAS", "R_SUPPAT" };
        public static string 
            rightOuterKnee  = "R_FLE";
        public static readonly List<string>
            rightOuterKneeAKA = new List<string>() {"R_FLE", "R_LKNEE", "RIGHT_KNEE", "r_knjntln","Rt Lat Knee", "R_KNEE_LAT", "RKNE"};
        public static string 
            rightInnerKnee  = "R_FME";
        public static readonly List<string>
            rightInnerKneeAKA = new List<string>() {"R_FME", "R_MKNEE", "RIGHT_MEDIAL_KNEE","Rt Medial Knee", "L_KNEE_MED" };
        public static string 
            rightLowerKnee  = "R_TTC";
        public static readonly List<string>
            rightLowerKneeAKA = new List<string>() { "R_TTC", "r_tubtib", "Rt Tibia", "R_TIB_1" };

        #endregion
        #endregion
        #region foot
        #region left foot
        public static string 
            leftOuterAnkle  = "L_FAL";
        public static readonly List<string>
            leftOuterAnkleAKA = new List<string>() { "L_FAL", "L_LMAL", "LEFT_ANKLE_OUTER", "l_ankle", "Lt Ankle", "L_ANKLE_LAT", "LANK" };
        public static string 
            leftInnerAnkle  = "L_TAM";
        public static readonly List<string>
            leftInnerAnkleAKA = new List<string>() { "L_TAM", "L_MMAL", "LEFT_MEDIAL_ANKLE", "Lt Medial Ankle", "L_ANKLE_MED" };
        public static string 
            leftHeel  = "L_FCC";
        public static readonly List<string>
            leftHeelAKA = new List<string>() {"L_FCC", "L_HEEL_CALC", "LEFT_HEEL", "l_heel", "L_FCC1", "L_FCC2", "Lt Heel", "LHEE"};
        #endregion
        #region right foot
        public static string    
            rightOuterAnkle  = "R_FAL";
        public static readonly List<string>
            rightOuterAnkleAKA = new List<string>() {"R_FAL", "R_LMAL", "RIGHT_ANKLE_OUTER", "l_ankle", "Rt Ankle", "R_ANKLE_LAT", "RANK" };
        public static string 
            rightInnerAnkle  = "R_TAM";
        public static readonly List<string>
            rightInnerAnkleAKA = new List<string>() { "R_TAM", "R_MMAL", "RIGHT_MEDIAL_ANKLE", "Rt Medial Ankle", "R_ANKLE_MED" };
        public static string 
            rightHeel  = "R_FCC";
        public static readonly List<string>
            rightHeelAKA = new List<string>() {"R_FCC", "R_HEEL_CALC", "RIGHT_HEEL", "r_heel", "R_FCC1", "R_FCC2", "Rt Heel", "RHEE" };
        #endregion
        #region toes
        #region left toes
        public static string
            leftToe1  = "L_FM1";
        public static readonly List<string>
            leftToe1AKA = new List<string>() { "L_FM1", "L_TOE_1_MET", "L_MT_1", "LTOE" };
        public static string 
            leftToe2  = "L_FM2";
        public static readonly List<string>
            leftToe2AKA = new List<string>() { "L_FM2", "l_toe", "Lt 2nd Toe" };

        public static string
            leftToe5 = "L_FM5";
        public static readonly List<string>
            leftToe5AKA = new List<string>() { "L_FM5", "L_TOE_5_MET", "Lt 5th Toe", "L_MT_5", "LMT5" };
        #endregion
        #region right toes
        public static string
            rightToe1  = "R_FM1";
        public static readonly List<string>
            rightToe1AKA = new List<string>() { "R_FM1", "R_TOE_1_MET", "R_MT_1", "RTOE" };
        public static string 
            rightToe2  = "R_FM2";
        public static readonly List<string>
            rightToe2AKA = new List<string>() { "R_FM2", "r_toe","Rt 2nd Toe" };
        public static string
            rightToe5  = "R_FM5";
        public static readonly List<string>
            rightToe5AKA = new List<string>() {"R_FM5", "R_TOE_5_MET","Rt 5th Toe", "R_MT_5", "RMT5" };
        #endregion
        #endregion
        #endregion
    }
}
