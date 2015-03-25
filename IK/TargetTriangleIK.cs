using OpenTK;
using QTM2Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTM2Unity
{
    class TargetTriangleIK : IKSolver
    {
        private void RotateTowards(ref Bone[] bones, Vector3 toRotate, Vector3 towards, int numberOfBones, int i)
        {
            float angle = Vector3.CalculateAngle(toRotate, towards);
            Vector3 axis = Vector3.Cross(toRotate, towards);
            Quaternion rotation = Quaternion.FromAxisAngle(axis, angle);
            ForwardKinematics(ref bones, rotation, i);
        }
        public override Bone[] SolveBoneChain(Bone[] bones, Bone target, Bone parent)
        {
            int numberOfBones = bones.Length;
            for (int i = 0; i < numberOfBones - 1; i++)
            {
                //UnityEngine.Debug.Log(bones[i].Name + " nr: " + i);
                Vector3 aVector = (bones[i+1].Pos - bones[i].Pos);
                float a = aVector.Length;
                float b = 0;// (bones[numberOfBones - 1].Pos - bones[i + 1].Pos).Length;
                for (int j = i+1; j < numberOfBones-1; j++)
                {
                    b += (bones[j].Pos - bones[j + 1].Pos).Length;
                }
                Vector3 cVector = (target.Pos - bones[i].Pos);
                float c = cVector.Length;
                if (c >= a + b)
                {
                    //aVector = cVector;
                    //UnityEngine.Debug.Log(string.Format("A triangle cannot be formed: (C:{2} >= A:{0} + B:{1})", a, b, c));
                    ForwardKinematics(ref bones, QuaternionHelper.RotationBetween(aVector, cVector), i);
                }
                else if (c < Math.Abs(a-b)) 
                {
                    //aVector = -cVector;
                    //UnityEngine.Debug.Log(string.Format("A triangle cannot be formed 2: (C:{2} < |A:{0} - B:{1}|)", a, b, c));
                    if (b == 0)
                    {
                        ForwardKinematics(ref bones, QuaternionHelper.RotationBetween(aVector, cVector), i);
                    }
                    else
                    {
                        ForwardKinematics(ref bones, QuaternionHelper.RotationBetween(aVector, -cVector), i);
                    }
                }
                else
                {
                    //UnityEngine.Debug.Log(string.Format("Full triangle has been formed A:{0} B:{1} C:{2}", a, b, c));
                    float triangleAngel = 
                        Mathf.Acos((-(b*b-a*a-c*c))/ (2*a*c)
                        );
                    //UnityEngine.Debug.Log(string.Format("TriangleAngle:{0}", triangleAngel));
                    float dotAngle = Mathf.Acos(Vector3.Dot(aVector, cVector));
                    dotAngle = Vector3.CalculateAngle(aVector, cVector);
                    float omegaRotAngle = dotAngle - triangleAngel;
                    //UnityEngine.Debug.Log(string.Format("Triangle angle: {0}", MathHelper.RadiansToDegrees(triangleAngel)));
                    //UnityEngine.Debug.Log(string.Format("Dotangle angle: {0}", MathHelper.RadiansToDegrees(dotAngle)));
                    //UnityEngine.Debug.Log(string.Format("Rotat angle: {0}", MathHelper.RadiansToDegrees(omegaRotAngle)));
                    Vector3 rotationVector;
                    if (aVector.Equals(-cVector) || aVector.Equals(cVector))
                    {
                        //UnityEngine.Debug.Log(" Undefined Axis of Rotation");
                        rotationVector = Vector3.UnitY;
                    }
                    else
                    {
                        rotationVector = Vector3.Cross(aVector, cVector);
                        //UnityEngine.Debug.Log(string.Format("Rotation VECTOR: {0}", rotationVector));

                    }
                    UnityDebug.DrawVector(bones[i].Pos, rotationVector);
                    if (omegaRotAngle > 0)
                    {
                        ForwardKinematics(ref bones, Quaternion.FromAxisAngle(rotationVector, omegaRotAngle), i);
                    }
                }
            }
            return bones;
        } 
    }
}