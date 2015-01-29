using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using QTM2Unity.Unity;

namespace QTM2Unity.IK
{
    class CCD
    {
        // TODO what about constraints?
        float threshold = 0.1f; // TODO define a good default threshold value 

       // And what happens when we don't have a position for some joint?


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
                    // I think we need to do this check here <-- TODO check som I'm right
                    if ((target - jointPositions[jointPositions.Length - 1]).Length > threshold)
                    {
                        return jointPositions;
                    }
                }
            }
                return jointPositions; 
        }

        // TODO move to quaternion helper or something
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
