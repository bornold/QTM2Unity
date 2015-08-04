using System;
using UnityEngine;

namespace QTM2Unity {

   [System.Serializable]
    public class Debugging
    {
       public bool debugFlag = false;
        public Vector3 offset = new Vector3(0, 0, 0);
        public Markers markers;
        public BodyRig bodyRig;
        public JointsConstrains jointsConstrains;
        public JointFix jointsRot;
    }
   [System.Serializable]
   public class Markers
   {
       public bool markers = false;
       [Range(0.001f, 0.05f)]
       public float scale = 0.01f;
       public bool bones = false;
       public Color boneColor = Color.blue;
   }
    [System.Serializable]
    public class BodyRig
    {
        public string bodyPrefix = "";
        public bool showSkeleton = false;
        public Color skelettColor = Color.black;
        public bool showJoints = false;
        [Range(0.01f, 0.05f)]
        public float jointScale = 0.015f;
        public Color jointColor = Color.green;
        public bool showRotationTrace = false;
        [Range(0.01f, 0.5f)]
        public float traceLength = 0.08f;
        public bool resetSkeleton = false;
        public bool Extrapolate = false;

    }
    [System.Serializable]
    public class JointsConstrains
    {
        public bool showConstraints = false;
        [Range(0.01f, 0.5f)]
        public float coneSize = 0.05f;
        [Range(1, 150)]
        public int coneResolution = 50;
        public bool showTwistConstraints = false;
    }
    [System.Serializable]
    public class JointFix
    {
        public bool UseFingers = false;
        public Vector3
            root = new Vector3(0f, 0f, 0f),
            hip = new Vector3(0f, 0f, 0f),
            spine = new Vector3(0f, 0f, 0f),
            neck = new Vector3(0f, 0f, 0f),
            head = new Vector3(0f, 0f, 0f),
            legUpperLeft = new Vector3(0f, 0f, 180f),
            legLowerLeft = new Vector3(0f, 0f, 180f),
            footLeft = new Vector3(270f, 0f, 180f),
            legUpperRight = new Vector3(0f, 0f, 180f),
            legLowerRight = new Vector3(0f, 0f, 180f),
            footRight = new Vector3(270f, 0f, 180f),
            clavicleLeft = new Vector3(0f, 0f, 270f),
            armUpperLeft = new Vector3(0f, 0f, 270f),
            armLowerLeft = new Vector3(0f, 0f, 270f),
            handLeft = new Vector3(0f, 0f, 270f),
            thumbLeft = new Vector3(330f, 0f, 270f),
            fingersLeft = new Vector3(330f, 0f, 270f),
            clavicleRight = new Vector3(0f, 0f, 270f),
            armUpperRight = new Vector3(0f, 0f, 90f),
            armLowerRight = new Vector3(0f, 0f, 90f),
            handRight = new Vector3(0f, 0f, 90f),
            thumbRight = new Vector3(330f, 0f, 90f),
            fingersRight = new Vector3(330f, 0f, 270f);

    }
}
