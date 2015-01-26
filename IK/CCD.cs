using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using QTM2Unity.Unity;

namespace Assets.QTM2Unity.IK
{
    class CCD
    {
        // TODO what about constraints?
        float threshold = 0.1f; // TODO define a good default threshold value 

        
        /// <summary>
        /// Solves a biped character. 
        /// </summary>
        /// <param name="joints">a dictionary with joint name and position</param>
        /// <param name="targets">a Dictionary with target position for the end effector with the 
        /// given name</param>
        /// <returns>new positions for each joint of the character</returns>
        public Dictionary<string, Vector3> solveBiped(Dictionary<string, Vector3> joints, 
            Dictionary<string, Vector3> targets)
        {
            // TODO: Not sure how to treat the chains here. Are the root (hip?)
            // fixed?
            // Right now assuming that all joints exist
            // (Hip, RightHip, RightKnee, RightAnkle, RightToes, 
            // LeftHip, LeftKnee, LeftAnkle, LeftToes, 
            // Spine, Neck, Head, RightShoulder, RightElbow, RightWrist, RightHand,
            // LeftShoulder, LeftElbow, LeftWrist, LeftHand
            // And that the end effectors Head, RightHand, LeftHand, RightToes, LeftToes has 
            // corresponding targets. I'm guessing that this is not always the case...
            // And what happens when we don't have a position for some joint?

            // Treating the hip as root
            // TODO
            // This is just an outline (it's not correct and it won't actually do anything right now):
            solveChain(new Vector3[]{joints["Hip"], joints["RightHip"], joints["RightKnee"], 
                joints["RightAnkle"], joints["RightToes"]}, targets["RightToes"]);
            solveChain(new Vector3[]{joints["Hip"], joints["LeftHip"], joints["LeftKnee"], 
                joints["LeftAnkle"], joints["LeftToes"]}, targets["LeftToes"]);
            solveChain(new Vector3[]{joints["Hip"], joints["Spine"], joints["Neck"], 
                joints["RightShoulder"], joints["RightElbow"], joints["RightWrist"], 
                joints["RightHand"]}, targets["RightHand"]);
            solveChain(new Vector3[]{joints["Hip"], joints["Spine"], joints["Neck"], 
                joints["LeftShoulder"], joints["LeftElbow"], joints["LeftWrist"], 
                joints["LeftHand"]}, targets["LeftHand"]);
            solveChain(new Vector3[]{joints["Hip"], joints["Spine"], joints["Neck"], 
                joints["Head"]}, targets["Head"]);

            // Not sure how to solve for multiple end effectors

            return null;
        }

        // Note: The end effector is assumed to be the last element in jointPositions
        // TODO maximum number of iterations?
        private Vector3[] solveChain(Vector3[] jointPositions, Vector3 target)
        {
            int numberOfJoints = jointPositions.Length - 1;
            while ((target - jointPositions[jointPositions.Length - 1]).Length > threshold)
            {
                for (int i = numberOfJoints - 1; i >= 0; i--)
                // for each joint, starting with the one closest to the end effector
                {
                    // Get the vectors between the points
                    Vector3 a = jointPositions[jointPositions.Length - 1] - jointPositions[i];
                    Vector3 b = target - jointPositions[i];
                    
                    // Make a rotation quarternion and rotate 
                    // - first the endEffector
                    // - then the rest of the affected joints
                    Quaternion rotation = getRotation(a, b); 
                    for (int j = numberOfJoints; j >= i; j--)
                    {
                        jointPositions[j] = Vector3.Transform(jointPositions[j], rotation);
                    }
                }
            }
                return jointPositions; 
        }

        // Returns a quaternion representing the rotation from vector a to b
        private Quaternion getRotation(Vector3 a, Vector3 b)
        {
            a.Normalize();
            b.Normalize();

            float precision = 0.99f; // TODO not sure if good value
            if (Vector3.Dot(a,b) > precision) // a and b are parallel
            {
                return Quaternion.Identity;
            }
            if (Vector3.Dot(a,b) < -precision) // a and b are opposite
            {
                return Quaternion.Normalize(Quaternion.FromAxisAngle(new Vector3(1, 1, 1), Mathf.PI));
            }
  
            float angle = Vector3.CalculateAngle(a, b);
            Vector3 axis = Vector3.Cross(a, b);
            axis.Normalize();

            return Quaternion.Normalize(Quaternion.FromAxisAngle(axis, angle));
        }

    }
}
