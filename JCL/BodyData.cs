﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTM2Unity
{
    class BodyData
    {
        public static float MarkerCentreToSkinSurface = 0.009f;
        public static float MarkerToSpineDist = 0.1f; // m
        public static float MidHeadToHeadJoint = 0.08f; // m
        public static float SpineLength = 0.0236f; // m
        public static float BMI = 24;
        public float Height { get { return height; } }
        private float height = 175; // cm
        public float Mass { get { return mass; } }
        private float mass = 75; // kg
        public Vector3 NeckToChestVector { get { return neck2ChestVector; } }
        private Vector3 neck2ChestVector;
        public float ShoulderWidth { get {return shoulderWidth;} }
        private float shoulderWidth = 400; // mm
        private uint chestsFrames = 0;
        private uint shoulderFrames = 0;
        private uint heightFrames = 0;
        public void CalculateBodyData(Dictionary<string, Vector3> markers, Quaternion chestOrientation)
        {
            // set chest depth
            var currentNeckToChestVector = (markers[MarkerNames.chest] - markers[MarkerNames.neck]);
            currentNeckToChestVector = Vector3.Transform(currentNeckToChestVector, Quaternion.Invert(chestOrientation));
            if (!currentNeckToChestVector.IsNaN())
            {
                neck2ChestVector = (neck2ChestVector * chestsFrames + currentNeckToChestVector) / (chestsFrames + 1);
                chestsFrames++;
            }

            // set shoulder width
            float tmp = (markers[MarkerNames.leftShoulder] - markers[MarkerNames.rightShoulder]).Length * 500; // to mm half the width
            if (!float.IsNaN(tmp) && tmp < 500)
            {
                shoulderWidth = (shoulderWidth * shoulderFrames + tmp) / (shoulderFrames + 1);
                shoulderFrames++;
            }
            // height and mass
            tmp = ( (markers[MarkerNames.rightOuterAnkle] - markers[MarkerNames.rightOuterKnee]).LengthFast +
                    (markers[MarkerNames.rightOuterKnee] - markers[MarkerNames.rightHip]).LengthFast +
                    (markers[MarkerNames.bodyBase] - markers[MarkerNames.spine]).LengthFast +
                    (markers[MarkerNames.spine] - markers[MarkerNames.neck]).LengthFast +
                    (markers[MarkerNames.neck] - markers[MarkerNames.head]).LengthFast
                  ) * 100; // cm
            if (!float.IsNaN(tmp) && tmp < 250)
            {
                height = (height * heightFrames + tmp) / (heightFrames + 1);
                mass = (height / 100) * (height / 100) * BMI; // BMI
                heightFrames++;
            }
        }
    }
}
