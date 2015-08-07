﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QTM2Unity
{
    [System.Serializable]
    public enum CharactersModel
    {
        Model1, Model2, Model3, Model4, Model5, Model6
    }
    [System.Serializable]
    public class BoneRotations
    {
        public Vector3
            root,
            hip,
            spine,
            neck,
            head,

            legUpperLeft,
            legLowerLeft,
            footLeft,

            legUpperRight,
            legLowerRight,
            footRight,

            clavicleLeft,
            armUpperLeft,
            armLowerLeft,
            handLeft,
            thumbLeft,
            fingersLeft,

            clavicleRight,
            armUpperRight,
            armLowerRight,
            handRight,
            thumbRight,
            fingersRight;
    }
    [System.Serializable]
    public class Model1 : BoneRotations
    {
        public Model1()
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
    public class Model2 : BoneRotations
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
    public class Model3 : BoneRotations
    {
        public Model3()
        {
            root = new Vector3(0f, 0f, 270f);
            hip = new Vector3(0f, 270f, 0f);
            spine = new Vector3(0f, 270f, 0f);
            neck = new Vector3(0f, 270f, 0f);
            head = new Vector3(0f, 270f, 0f);

            legUpperLeft = new Vector3(0f, 270f, 0f);
            legLowerLeft = new Vector3(0f, 270f, 0f);
            footLeft = new Vector3(0f, 270f, 90f);

            legUpperRight = new Vector3(0f, 270f, 0f);
            legLowerRight = new Vector3(0f, 270f, 0f);
            footRight = new Vector3(0f, 270f, 90f);

            clavicleLeft = new Vector3(0f, 90f, 0f);
            armUpperLeft = new Vector3(0f, 90f, 0f);
            armLowerLeft = new Vector3(0f, 90f, 0f);
            handLeft = new Vector3(0f, 180f, 0f);
            thumbLeft = new Vector3(0f, 180f, 0f);
            fingersLeft = new Vector3(0f, 180f, 0f);

            clavicleRight = new Vector3(0f, 90f, 0f);
            armUpperRight = new Vector3(0f, 90f, 0f);
            armLowerRight = new Vector3(0f, 90f, 0f);
            handRight = new Vector3(0f, 0f, 0f);
            thumbRight = new Vector3(0f, 0f, 0f);
            fingersRight = new Vector3(0f, 0f, 0f);
        }
    }
    [System.Serializable]
    public class Model4 : BoneRotations
    {
        public Model4()
        {
            root = new Vector3(0f, 90f, 270f);
            hip = new Vector3(0f, 0f, 0f);
            spine = new Vector3(0f, 0f, 0f);
            neck = new Vector3(0f, 0f, 0f);
            head = new Vector3(0f, 0f, 0f);

            legUpperLeft = new Vector3(0f, 0f, 0f);
            legLowerLeft = new Vector3(30f, 0f, 0f);
            footLeft = new Vector3(270f, 0f, 0f);

            legUpperRight = new Vector3(0f, 0f, 0f);
            legLowerRight = new Vector3(30f, 0f, 0f);
            footRight = new Vector3(270f, 0f, 0f);

            clavicleLeft = new Vector3(0f, 180f, 180f);
            armUpperLeft = new Vector3(0f, 180f, 270f);
            armLowerLeft = new Vector3(0f, 180f, 270f);
            handLeft = new Vector3(90f, 270f, 0f);
            thumbLeft = new Vector3(90f, 270f, 0f);
            fingersLeft = new Vector3(90f, 270f, 0f);

            clavicleRight = new Vector3(0f, 180f, 180f);
            armUpperRight = new Vector3(0f, 180f, 90f);
            armLowerRight = new Vector3(0f, 180f, 90f);
            handRight = new Vector3(90f, 90f, 0f);
            thumbRight = new Vector3(90f, 90f, 0f);
            fingersRight = new Vector3(90f, 90f, 0f);
        }
    }
    [System.Serializable]
    public class Model5 : BoneRotations
    {
        public Model5()
        {
            root = new Vector3(0f, 90f, 270f);
            hip = new Vector3(0f, 0f, 90f);
            spine = new Vector3(0f, 0f, 0f);
            neck = new Vector3(0f, 0f, 0f);
            head = new Vector3(90f, 0f, 0f);

            legUpperLeft = new Vector3(0f, 0f, 0f);
            legLowerLeft = new Vector3(0f, 0f, 0f);
            footLeft = new Vector3(0f, 0f, 0f);

            legUpperRight = new Vector3(0f, 180f, 0f);
            legLowerRight = new Vector3(0f, 180f, 0f);
            footRight = new Vector3(0f, 180f, 0f);

            clavicleLeft = new Vector3(0f, 270f, 0f);
            armUpperLeft = new Vector3(0f, 270f, 0f);
            armLowerLeft = new Vector3(0f, 270f, 0f);
            handLeft = new Vector3(0f, 270f, 0f);
            thumbLeft = new Vector3(0f, 0f, 0f);
            fingersLeft = new Vector3(0f, 0f, 0f);

            clavicleRight = new Vector3(0f, 270f, 0f);
            armUpperRight = new Vector3(0f, 270f, 0f);
            armLowerRight = new Vector3(0f, 270f, 0f);
            handRight = new Vector3(0f, 270f, 0f);
            thumbRight = new Vector3(0f, 0f, 0f);
            fingersRight = new Vector3(0f, 0f, 0f);
        }
    }
    public class Model6 : BoneRotations
    {
        public Model6()
        {
            root  = new Vector3(0f, 0f, 0f);
            hip   = new Vector3(0f, 270f, 270f);
            spine = new Vector3(0f, 270f, 270f);
            neck  = new Vector3(0f, 270f, 270f);
            head  = new Vector3(0f, 270f, 270f);

            legUpperLeft = new Vector3(0f, 90f, 270f);
            legLowerLeft = new Vector3(0f, 90f, 270f);
            footLeft     = new Vector3(0f, 270f, 270f);

            legUpperRight = new Vector3(0f, 270f, 90f);
            legLowerRight = new Vector3(0f, 270f, 90f);
            footRight     = new Vector3(0f, 270f, 90f);

            clavicleLeft = new Vector3(180f, 180f, 90f);
            armUpperLeft = new Vector3(180f, 180f, 90f);
            armLowerLeft = new Vector3(180f, 90f, 30f);
            handLeft     = new Vector3(0f, 0f, 270f);
            thumbLeft    = new Vector3(0f, 0f, 270f);
            fingersLeft  = new Vector3(0f, 0f, 270f);

            clavicleRight = new Vector3(0f, 180f, 90f);
            armUpperRight = new Vector3(0f, 180f, 90f);
            armLowerRight = new Vector3(0f, 0f, 90f);
            handRight     = new Vector3(0f, 180f, 90f);
            thumbRight    = new Vector3(0f, 180f, 90f);
            fingersRight  = new Vector3(0f, 180f, 90f);
        }
    }

}
