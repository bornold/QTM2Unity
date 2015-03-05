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
        private void ForwardKinematics(ref Bone[] bones, Quaternion rotation, int numberOfBones, int i)
        {
            for (int j = numberOfBones - 2; j >= i; j--)
            {
                if (j > i)
                {
                    bones[j].Pos = bones[i].Pos +
                        Vector3.Transform((bones[j].Pos - bones[i].Pos), rotation);
                }
                // rotate orientation
                bones[j].rotate(rotation);
            }
        }
        public override Bone[] solveBoneChain(Bone[] bones, Vector3 target)
        {
            int numberOfBones = bones.Length;
            for (int i = 0; i < numberOfBones - 2; i++)
            {
                UnityEngine.Debug.Log(bones[i].Name);
                Vector3 aVector = (bones[i+1].Pos - bones[i].Pos);
                float a = aVector.Length;
                float b = 0;
                for (int j = i; j < numberOfBones-2; j++)
                {
                    b += (bones[j].Pos - bones[j + 1].Pos).Length;
                }
                Vector3 cVector = (target - bones[i].Pos);
                float c = cVector.Length;
                UnityEngine.Debug.Log(string.Format("A:{0} B:{1} C:{2}", a,b,c));
                if (c >= a + b)
                {
                    UnityEngine.Debug.Log(string.Format("({2} >= {0} + {1})", a, b, c));
                    //aVector = cVector;
                }
                else if (c < Math.Abs(a-b)) 
                {
                    UnityEngine.Debug.Log(string.Format("({2} < |{0} - {1}|)", a, b, c));

                    //aVector = -cVector;

                }
                else
                {
                    UnityEngine.Debug.Log(string.Format("({2} < {0} + {1})", a, b, c));
                    float triangleAngel = 
                        Mathf.Acos(
                                    (-(b*b-a*a-c*c)) 
                                    / 2*a*c
                        );
                    UnityEngine.Debug.Log(string.Format("TriangleAngle:{0}", triangleAngel));
                    float omegaRotAngle = Mathf.Acos(Vector3.Dot(aVector,cVector)) - triangleAngel;
                    UnityEngine.Debug.Log(string.Format("Rotat angle: {0})", omegaRotAngle));
                    Vector3 rotationVector;
                    if (aVector.Equals(-cVector) || aVector.Equals(cVector))
                    {
                        rotationVector = Vector3.UnitY;
                    }
                    else
                    {
                        rotationVector = Vector3.Cross(aVector, cVector);
                    }
                    UnityDebug.DrawVector(bones[i].Pos, rotationVector);
                    ForwardKinematics(ref bones, Quaternion.FromAxisAngle(rotationVector, omegaRotAngle), numberOfBones, i);
                }
            }
            return bones;
        }
        public Bone[] solveBoneChain2(Bone[] bones, Vector3 target)
        {
            Vector3 endEffector = bones.Last().Pos;
            int numberOfBones = bones.Length;
            for (int i = numberOfBones - 2; i > 0; i--)
            {
                Vector3 Ve = target - bones[i].Pos;
                Vector3 Vt = endEffector - bones[i].Pos;
                Vector3 VeVt = Vector3.Cross(Ve, Vt);
                Vector3 Vr = VeVt / VeVt.Length;
                float omega = Vector3.CalculateAngle(Ve, Vt);
                float a, b = 0, c;
                a = (bones[i].Pos - bones[i + 1].Pos).Length;
                for (int j = i; j > 0; j--)
                {
                    b += (bones[j].Pos - bones[j + 1].Pos).Length;
                }
                c = Ve.Length;
                if (c > a + b)
                {
                    UnityEngine.Debug.Log("(c > a + b)");
                    //Ji rotate omega, rotation axis is Vr
                    ForwardKinematics(ref bones, Quaternion.FromAxisAngle(Vr,omega), numberOfBones, i);
                }
                else if (c < Math.Abs(a-b))
                {
                    UnityEngine.Debug.Log("(c < Math.Abs(a-b))");
                    //Ji rotate -omega, rotation axis is Vr
                    ForwardKinematics(ref bones, Quaternion.FromAxisAngle(Vr, -omega), numberOfBones, i);
                }
                else if (a*a + b*b - c*c > 0)
                {
                float phib = 0, phic = 0, betat = 0, omegai = 90; //omegai rotlimit
                    UnityEngine.Debug.Log("(a*a + b*b - c*c > 0)");
                    phib = Mathf.Acos((a * a + c * c - b * b) / 2 * a * c);
                    phic = Mathf.Acos((a * a + b * b - c * c) / 2 * a * b);
                    betat = Mathf.PI - phic;
                    float rad = MathHelper.DegreesToRadians(10);
                    phib -= (betat > omegai) ? rad : -rad;
                    float betai = omega - phib;
                    //Ji rotate betai, rotate axis Vr
                    ForwardKinematics(ref bones, Quaternion.FromAxisAngle(Vr, betai), numberOfBones, i);
                }
                else
                {
                    UnityEngine.Debug.Log("Nothing is true");
                }
            }
            return bones;
        }
    }
}