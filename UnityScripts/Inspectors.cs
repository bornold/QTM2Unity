using System;
using UnityEngine;

namespace QTM2Unity {

   [System.Serializable]
    public class Debugging
    {
        public string bodyPrefix = "";
        public bool debugFlag = false;
        public bool resetSkeleton = false;
        public JointFix jointsRot;
        public Vector3 offset = new Vector3(0, 0, 0);
        public Markers markers;
        public BodyRig bodyRig;
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
        public bool Extrapolate = false;
        public bool showSkeleton = false;
        public Color skelettColor = Color.black;
        public bool showJoints = false;
        [Range(0.01f, 0.05f)]
        public float jointScale = 0.015f;
        public Color jointColor = Color.green;
        public bool showRotationTrace = false;
        [Range(0.01f, 0.5f)]
        public float traceLength = 0.08f;
        public JointsConstrains jointsConstrains;
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
    }
    [System.Serializable]
    public class JointFix
    {
        public bool UseFingers = false;
        public CharactersFixs charactersFixs = CharactersFixs.Model1;
        public TheRots rots = new Model1();
    }
    [System.Serializable]
    public enum CharactersFixs
    {
        Model1, Model2, Model3
    }
    [System.Serializable]
    public class TheRots
        {
            public Vector3
                root ,
                hip ,
                spine ,
                neck ,
                head ,

                legUpperLeft ,
                legLowerLeft ,
                footLeft ,

                legUpperRight ,
                legLowerRight ,
                footRight ,

                clavicleLeft ,
                armUpperLeft ,
                armLowerLeft ,
                handLeft ,
                thumbLeft ,
                fingersLeft,

                clavicleRight ,
                armUpperRight ,
                armLowerRight ,
                handRight ,
                thumbRight ,
                fingersRight;
        }
    [System.Serializable]
    public class Model1 : TheRots
    {
        public Model1 ()
        {
            root = new Vector3(0f, 0f, 0f);
            hip = new Vector3(0f, 0f, 0f);
            spine = new Vector3(0f, 0f, 0f);
            neck = new Vector3(0f, 0f, 0f);
            head = new Vector3(0f, 0f, 0f);

            legUpperLeft = new Vector3(0f, 0f, 180f);
            legLowerLeft = new Vector3(0f, 0f, 180f);
            footLeft = new Vector3(270f, 0f, 180f);

            legUpperRight = new Vector3(0f, 0f, 180f);
            legLowerRight = new Vector3(0f, 0f, 180f);
            footRight = new Vector3(270f, 0f, 180f);

            clavicleLeft = new Vector3(0f, 0f, 270f);
            armUpperLeft = new Vector3(0f, 0f, 260f);
            armLowerLeft = new Vector3(0f, 0f, 270f);
            handLeft = new Vector3(345f, 0f, 270f);
            thumbLeft = new Vector3(270f, 0f, 270f);
            fingersLeft = new Vector3(10f, 0f, 270f);

            clavicleRight = new Vector3(0f, 0f, 90f);
            armUpperRight = new Vector3(0f, 0f, 100f);
            armLowerRight = new Vector3(0f, 0f, 90f);
            handRight = new Vector3(345f, 0f, 90f);
            thumbRight = new Vector3(270f, 0f, 90f);
            fingersRight = new Vector3(0f, 0f, 90f);
        }
    }
    [System.Serializable]
    public class Model2 : TheRots
    {
        public Model2()
        {
            root = new Vector3(0f, 0f, 0f);
            hip = new Vector3(90f, 180f, 0f);
            spine = new Vector3(90f, 180f, 0f);
            neck = new Vector3(90f, 180f, 0f);
            head = new Vector3(90f, 180f, 0f);

            legUpperLeft = new Vector3(90f, 0f, 0f);
            legLowerLeft = new Vector3(90f, 0f, 0f);
            footLeft = new Vector3(40f, 0f, 0f);

            legUpperRight = new Vector3(90f, 0f, 0f);
            legLowerRight = new Vector3(90f, 0f, 0f);
            footRight = new Vector3(40f, 0f, 0f);

            clavicleLeft = new Vector3(90f, 0f, 0f);
            armUpperLeft = new Vector3(90f, 0f, 0f);
            armLowerLeft = new Vector3(90f, 0f, 0f);
            handLeft = new Vector3(90f, 90f, 0f);
            thumbLeft = new Vector3(90f, 0f, 0f);
            fingersLeft = new Vector3(90f, 0f, 0f);

            clavicleRight = new Vector3(90f, 0f, 0f);
            armUpperRight = new Vector3(90f, 0f, 0f);
            armLowerRight = new Vector3(90f, 0f, 0f);
            handRight = new Vector3(90f, 0f, 0f);
            thumbRight = new Vector3(90f, 0f, 0f);
            fingersRight = new Vector3(90f, 0f, 0f);
        }
    }
    [System.Serializable]
    public class Model3 : TheRots
    {
        public Model3()
        {
            root = new Vector3(0f, 0f, 270f);
            hip = new Vector3(0f, 270f, 0f);
            spine = new Vector3(0f, 270f, 0f);
            neck = new Vector3(0f, 270f, 0f);
            head = new Vector3(0f, 270f, 0f);

            legUpperLeft = new Vector3(0f, 90f, 0f);
            legLowerLeft = new Vector3(0f, 90f, 0f);
            footLeft = new Vector3(0f, 270f, 90f);

            legUpperRight = new Vector3(0f, 90f, 0f);
            legLowerRight = new Vector3(0f, 90f, 0f);
            footRight = new Vector3(0f, 270f, 90f);

            clavicleLeft = new Vector3(0f, 0f, 0f);
            armUpperLeft = new Vector3(0f, 90f, 0f);
            armLowerLeft = new Vector3(0f, 90f, 0f);
            handLeft = new Vector3(0f, 180f, 0f);
            thumbLeft = new Vector3(0f, 0f, 0f);
            fingersLeft = new Vector3(0f, 0f, 0f);

            clavicleRight = new Vector3(0f, 0f, 0f);
            armUpperRight = new Vector3(0f, 90f, 0f);
            armLowerRight = new Vector3(0f, 90f, 0f);
            handRight = new Vector3(0f, 0f, 0f);
            thumbRight = new Vector3(0f, 0f, 0f);
            fingersRight = new Vector3(0f, 0f, 0f);
        }
    }
}
