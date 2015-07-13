using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTM2Unity
{
    class BodyData
    {
        // Read only data for body proportions
        public readonly static float MarkerCentreToSkinSurface = 0.009f;
        public readonly static float MarkerToSpineDist = 0.08f; // m
        public readonly static float MidHeadToHeadJoint = 0.08f; // m
        public readonly static float SpineLength = 0.0236f; // m
        public readonly static float BMI = 24;
        // The collected body data
        public float Height { get; private set; }
        public float Mass { get; private set; }
        public Vector3 NeckToChestVector { get; private set; }
        public float ShoulderWidth { get; private set; }
        //private frameCounters
        private uint chestsFrames = 0;
        private uint shoulderFrames = 0;
        private uint heightFrames = 0;
        public BodyData()
        {
            Height = 175; // cm
            Mass = 75; //kg
            ShoulderWidth = 400; //mm
        }
        public void CalculateBodyData(Dictionary<string, Vector3> markers, Quaternion chestOrientation)
        {
            // set chest depth
            var currentNeckToChestVector = (markers[MarkerNames.chest] - markers[MarkerNames.neck]);
            if (!currentNeckToChestVector.IsNaN() && !chestOrientation.IsNaN())
            {
                NeckToChestVector = 
                    (NeckToChestVector * chestsFrames
                    + Vector3.Transform(currentNeckToChestVector, Quaternion.Invert(chestOrientation))) 
                    / (++chestsFrames);
            }

            // set shoulder width
            float tmp = (markers[MarkerNames.leftShoulder] - markers[MarkerNames.rightShoulder]).Length * 500; // to mm half the width
            if (!float.IsNaN(tmp))// && tmp < 500)
            {
                ShoulderWidth = (ShoulderWidth * shoulderFrames + tmp) / (++shoulderFrames);
            }
            // height and mass
            tmp = ( (markers[MarkerNames.rightOuterAnkle] - markers[MarkerNames.rightOuterKnee]).LengthFast +
                    (markers[MarkerNames.rightOuterKnee] - markers[MarkerNames.rightHip]).LengthFast +
                    (markers[MarkerNames.bodyBase] - markers[MarkerNames.neck]).LengthFast +
                    (markers[MarkerNames.neck] - markers[MarkerNames.head]).LengthFast
                  ) * 100; // cm
            if (!float.IsNaN(tmp) && tmp < 250)
            {
                Height = (Height * heightFrames + tmp) / (++heightFrames);
                Mass = (Height / 100) * (Height / 100) * BMI; // BMI
            }
        }
    }
}
