using System;
using System.Collections.Generic;

namespace QTM2Unity
{
    static class MarkerNames
    {
        #region hip
        public static string 
            bodyBase = "SACR";
        public static readonly List<string>
            bodyBaseAKA = new List<string>() { "SACR", "SACRUM", "LOWER_LUMBAR", "LV5_S1" };
        public static readonly List<string>
            bodyBasebetween = new List<string>() { "R_IPS ", "L_IPS", "Rt Lower PSIS", "Lt Lower PSIS" };
        public static string 
            leftHip = "L_IAS";
        public static readonly List<string> 
            leftHipAKA = new List<string>() { "L_IAS", "L_ASIS","LASIS","LEFT_HIP","L_ICT","Lt Hip Front"};
        public static string 
            rightHip = "R_IAS";
        public static readonly List<string>
            rightHipAKA = new List<string>() {"R_IAS", "R_ASIS", "RASIS", "RIGHT_HIP", "R_ICT","Rt Hip Front"  };
        #endregion
        #region upperbody
        public static string
            spine= "TV12";
        public static readonly List<string>
            spineAKA = new List<string>() 
            {   "TV12", "TH12", "D12", 
                "TV11", "TH11", "D11", 
                "TV13", "TH13", "D13", 
                "TV10", "TH10", "D10", 
                "TV14", "TV14", "D14" };
        public static string 
            neck =  "TV2";
        public static readonly List<string> 
            neckAKA = new List<string>() { "TV2", "TV1", "C7", "C7_TOP_SPINE" };
        public static string 
            chest =  "SME";
        public static readonly List<string> 
            chestAKA = new List<string>() { "SME" };
        public static string 
            leftShoulder = "L_SAE";
        public static readonly List<string>
            leftShoulderAKA = new List<string>() {"L_SAE", "L_ACR", "LEFT_SHOULDER", "L_SHOULDER", "L_ACROMION", "LEFTSHOULDER", "LEFT SHOULDER" };
        public static string 
            rightShoulder = "R_SAE";
        public static readonly List<string>
            rightShoulderAKA = new List<string>() {"R_SAE", "R_ACR", "RIGHT_SHOULDER", "R_SHOULDER", "R_ACROMION", "RIGHTSHOULDER", "RIGHT SHOULDER" };
        #endregion
        #region head
        public static string 
            head =  "SGL";
        public static readonly List<string>
            headAKA = new List<string>() {"SGL", "Front of Head" };
        public static readonly List<string>  
            headBetween = new List<string>() { "head_left_front", "head_right_front" };
        public static string 
            leftHead = "L_HEAD";
        public static readonly List<string> 
            leftHeadAKA = new List<string>() {"L_HEAD", "HEAD_LEFT_FRONT" ,"Left Head"};
        public static string 
            rightHead = "R_HEAD";
        public static readonly List<string>
            rightHeadAKA = new List<string>() {"R_HEAD", "HEAD_RIGHT_FRONT", "Right Head" };
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
            leftOuterElbowAKA = new List<string>() {"L_HLE", "L_ELBOW","Lt Elbow"  };
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
            rightOuterElbowAKA = new List<string>() { "R_HLE", "R_ELBOW", "Rt Elbow" };
        #endregion
        #endregion
        #region hand
        #region lefthand
        public static string 
            leftWrist  = "L_USP";
        public static readonly List<string>
            leftWristAKA = new List<string>() { "L_USP","L_ULNA", "LEFT_WRIST_INNER", "L_WRIST_MED", "Lt Wrist Ulna" };
        public static string 
            leftWristRadius  = "L_RSP";
        public static readonly List<string>
            leftWristRadiusAKA = new List<string>() {"L_RSP", "L_RADUIS", "LEFT_WRIST_OUTER", "L_WRIST_LAT", "Lt Wrist Radius" };
        public static string 
            leftHand  = "L_HM2";
        public static readonly List<string> 
            leftHandAKA = new List<string>() {"L_HM2", "Lt 3rd Digita"  };
        #endregion
        #region right hand
        public static string 
            rightWrist  = "R_USP";
        public static readonly List<string> 
            rightWristAKA = new List<string>() {"R_USP", "R_ULNA", "RIGHT_WRIST_INNER", "R_WRIST_MED", "Rt Wrist Ulna" };
        public static string 
            rightWristRadius  = "R_RSP";
        public static readonly List<string>
            rightWristRadiusAKA = new List<string>() {"R_RSP", "R_RADUIS", "RIGHT_WRIST_OUTER", "R_WRIST_LAT", "Rt Wrist Radius" };
        public static string 
            rightHand  = "R_HM2";
        public static readonly List<string>
            rightHandAKA = new List<string>() { "R_HM2", "Rt 3rd Digita" };
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
            leftOuterKneeAKA = new List<string>() { "L_FLE", "L_LKNEE", "LEFT_KNEE", "l_knjntln" };
        public static string 
            leftInnerKnee  = "L_FME";
        public static readonly List<string>
            leftInnerKneeAKA = new List<string>() {"L_FME", "L_MKNEE", "LEFT_MEDIAL_KNEE",  };
        public static string 
            leftLowerKnee  = "L_TTC";
        public static readonly List<string>
            leftLowerKneeAKA = new List<string>() { "L_TTC", "l_tubtib" };
        #endregion
        #region right knee

        public static string 
            rightUpperKnee = "R_PAS";
        public static readonly List<string>
            rightUpperKneeAKA = new List<string>() {"R_PAS", "R_SUPPAT" };
        public static string 
            rightOuterKnee  = "R_FLE";
        public static readonly List<string>
            rightOuterKneeAKA = new List<string>() {"R_FLE", "R_LKNEE", "RIGHT_KNEE", "r_knjntln" };
        public static string 
            rightInnerKnee  = "R_FME";
        public static readonly List<string>
            rightInnerKneeAKA = new List<string>() {"R_FME", "R_MKNEE", "RIGHT_MEDIAL_KNEE" };
        public static string 
            rightLowerKnee  = "R_TTC";
        public static readonly List<string>
            rightLowerKneeAKA = new List<string>() {"R_TTC", "r_tubtib".ToUpper() };

        #endregion
        #endregion
        #region foot
        #region left foot
        public static string 
            leftOuterAnkle  = "L_FAL";
        public static readonly List<string> 
            leftOuterAnkleAKA = new List<string>() {"L_FAL", "L_LMAL", "LEFT_ANKLE_OUTER", "l_ankle" };
        public static string 
            leftInnerAnkle  = "L_TAM";
        public static readonly List<string>
            leftInnerAnkleAKA = new List<string>() {"L_TAM", "L_MMAL", "LEFT_MEDIAL_ANKLE" };
        public static string 
            leftHeel  = "L_FCC";
        public static readonly List<string>
            leftHeelAKA = new List<string>() {"L_FCC", "L_HEEL_CALC", "LEFT_HEEL", "l_heel", "L_FCC1", "L_FCC2", "Lt Heel"};
        #endregion
        #region right foot
        public static string    
            rightOuterAnkle  = "R_FAL";
        public static readonly List<string>
            rightOuterAnkleAKA = new List<string>() {"R_FAL", "R_LMAL", "RIGHT_ANKLE_OUTER", "l_ankle" };
        public static string 
            rightInnerAnkle  = "R_TAM";
        public static readonly List<string>
            rightInnerAnkleAKA = new List<string>() {"R_TAM", "R_MMAL", "RIGHT_MEDIAL_ANKLE" };
        public static string 
            rightHeel  = "R_FCC";
        public static readonly List<string>
            rightHeelAKA = new List<string>() {"R_FCC", "R_HEEL_CALC", "RIGHT_HEEL", "r_heel", "R_FCC1", "R_FCC2", "Rt Heel" };
        #endregion
        #region toes
        #region left toes
        public static string
            leftToe1  = "L_FM1";
        public static readonly List<string>
            leftToe1AKA = new List<string>() { "L_FM1", "L_TOE_1_MET" };
        public static string 
            leftToe2  = "L_FM2";
        public static readonly List<string>
            leftToe2AKA = new List<string>() { "L_FM2", "l_toe", "Lt 2nd Toe" };
        public static string
            leftToe5 = "R_FM1";
        public static readonly List<string>
            leftToe5AKA = new List<string>() { "R_FM1", "L_TOE_5_MET","Lt 5th Toe" };
        #endregion
        #region right toes
        public static string
            rightToe1  = "R_FM1";
        public static readonly List<string>
            rightToe1AKA = new List<string>() { "R_FM1", "R_TOE_1_MET" };
        public static string 
            rightToe2  = "R_FM2";
        public static readonly List<string>
            rightToe2AKA = new List<string>() { "R_FM2", "r_toe","Rt 2nd Toe" };
        public static string
            rightToe5  = "R_FM5";
        public static readonly List<string>
            rightToe5AKA = new List<string>() {"R_FM5", "R_TOE_5_MET","Rt 5th Toe" };
        #endregion
        #endregion
        #endregion
    }
}
