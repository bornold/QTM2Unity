using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Debug = UnityEngine.Debug;

namespace QTM2Unity
{
    class JacobianTranspose : IKSolver
    {
        private static float threshold = 0.0001f;

        override public Bone[] solveBoneChain(Bone[] bones, Bone target, Vector3 L1)
        {
            // Calculate the distances
            float[] distances;
            getDistances(out distances, ref bones);

            // J[rows][columns]
            int k = 1; // only one end effector now
            Vector3[,] J = new Vector3[k, bones.Length - 1];

            int iter = 0;
            while ((bones[bones.Length - 1].Pos - target.Pos).Length > threshold && iter < 10000)
            {
                // Create Jacobian matrix J(theta) = (ds[i]/dtheta[j])[ij]
                // ds[i]/dtheta[j] = v[j] x (s[i]-p[j])
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < bones.Length - 1; j++)
                    {
                        //J[i, j] = Vector3.Cross(bones[j].getRight(), bones[bones.Length - 1].Pos - bones[j].Pos);
                        /*Vector3 rotationAxis;
                        float angle;
                        bones[j].Orientation.ToAxisAngle(out rotationAxis, out angle);*/
                        J[i, j] = Vector3.Cross(bones[j].GetRight(), bones[bones.Length - 1].Pos - bones[j].Pos);
                        // Obs: bones[bones.Length-1] is the last in the chain, the end effector. 
                        // Will be different when we have several end effectors
                        //Debug.Log("J[" + i + ", " + j + "] = " + J[i, j].X + ", " + J[i, j].Y + ", " + J[i, j].Z);
                    }
                }

                Vector3[,] JT = transpose(J);
                Vector3 e = target.Pos - bones[bones.Length - 1].Pos;
                float JJT = mult(J, JT)[0, 0];

                // alpha = dot(e, J * JT * e) / dot(J * JT * e, J * JT * e);
                float alpha = Vector3.Dot(e, JJT * e) / Vector3.Dot(JJT * e, JJT * e);

                float[,] dTheta = mult(alpha, mult(JT, e));

                //print dTheta
               /* string s = "";
                for (int i = 0; i < dTheta.GetLength(0); i++)
                {
                    for (int j = 0; j < dTheta.GetLength(1); j++)
                    {
                        s += " " + dTheta[i, j];
                    }
                }
                Debug.Log(s);*/

                // Let's try
                for (int i = 0; i < bones.Length - 1; i++) // go through all joints (not end effector)
                {
                   // Debug.Log("Rotate " + bones[i].Name + " " + dTheta[i, 0] + " degrees");
                   
                    Quaternion q = Quaternion.FromAxisAngle(bones[i].GetRight(), dTheta[i, 0]);
                    bones[i].Rotate(q);
                    // Need to set new position for bone[i+1]
                    //bones[i+1].Pos = Vector3.Transform(bones[i + 1].Pos, q);
                }

                // set all positions
                for (int i = 1; i < bones.Length; i++)
                {
                    bones[i].Pos = bones[i - 1].Pos + distances[i - 1] * bones[i - 1].GetDirection();
                }
                iter++;
            }
            Debug.Log("Iterations " + iter);
            return bones;
        }

        private float[,] add(float[,] m1, float[,] m2)
        {
            if (m1.GetLength(0) != m2.GetLength(0) && m1.GetLength(1) != m2.GetLength(1))
                return null; // TODO exception

            float[,] res = new float[m1.GetLength(0), m1.GetLength(1)];
            for (int i = 0; i < m1.GetLength(0); i++)
            {
                for (int j = 0; j < m1.GetLength(1); j++)
                {
                    res[i, j] = m1[i, j] + m2[i, j];
                }
            }
            return res;
        }

        private Vector3[,] add(Vector3[,] m1, Vector3[,] m2)
        {
            if (m1.GetLength(0) != m2.GetLength(0) && m1.GetLength(1) != m2.GetLength(1))
                return null; // TODO exception

            Vector3[,] res = new Vector3[m1.GetLength(0), m1.GetLength(1)];
            for (int i = 0; i < m1.GetLength(0); i++)
            {
                for (int j = 0; j < m1.GetLength(1); j++)
                {
                    res[i, j] = m1[i, j] + m2[i, j];
                }
            }
            return res;
        }

        private float mult(Vector3[] v1, Vector3[] v2)
        {
            if (v1.Length != v2.Length)
                return -1; // TODO exception

            float res = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                res += Vector3.Dot(v1[i], v2[i]);
            }
            return res;
        }

        private float[,] mult(Vector3[,] m1, Vector3[,] m2)
        {
            if (m1.GetLength(0) != m2.GetLength(1) || m1.GetLength(1) != m2.GetLength(0))
                return null; // TODO exception

            float[,] res = new float[m1.GetLength(0), m2.GetLength(1)];
            for (int i = 0; i < m1.GetLength(0); i++)
            {
                for (int j = 0; j < m2.GetLength(1); j++)
                {
                    // Row i and column j of res
                    float r = 0;
                    for (int k = 0; k < m1.GetLength(1); k++)
                    {
                        r += Vector3.Dot(m1[i, k], m2[k, i]);
                    }
                    //Debug.Log("r (at i=" + i + ", j=" + j + "): " + r);
                    res[i, j] = r;
                }
            }
            return res;
        }

        private float[,] mult(float scalar, float[,] m)
        {
            float[,] res = new float[m.GetLength(0), m.GetLength(1)];
            for (int i = 0; i < m.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    res[i, j] = scalar * m[i, j];
                }
            }
            return res;
        }

        private Vector3[] mult(float scalar, Vector3[] v)
        {
            Vector3[] res = new Vector3[v.Length];
            for (int i = 0; i < v.Length - 1; i++)
            {
                res[i] = scalar * v[i];
            }
            return res;
        }

        private Vector3[] mult(Vector3[] v, float scalar)
        {
            return mult(scalar, v);
        }

        private float[] mult(Vector3[] vArray, Vector3 v)
        {
            float[] res = new float[vArray.Length];
            for (int i = 0; i < vArray.Length; i++)
            {
                res[i] = Vector3.Dot(vArray[i], v);
            }
            return res;
        }

        private Vector3[,] mult(float[,] fs, Vector3 v)
        {
            Vector3[,] res = new Vector3[fs.GetLength(0), fs.GetLength(1)];
            for (int i = 0; i < fs.GetLength(0); i++)
            {
                for (int j = 0; j < fs.GetLength(1); j++)
                {
                    res[i, j] = fs[i, j] * v;
                }
            }
            return res;
        }

        private Vector3[,] mult(float scalar, Vector3[,] m)
        {
            Vector3[,] res = new Vector3[m.GetLength(0), m.GetLength(1)];
            for (int i = 0; i < m.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    res[i, j] = scalar * m[i, j];
                }
            }
            return res;
        }

        private Vector3[,] mult(Vector3[,] m, float scalar)
        {
            return mult(scalar, m);
        }

        private float[,] mult(Vector3[,] m, Vector3 v)
        {
            float[,] res = new float[m.GetLength(0), m.GetLength(1)];
            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    res[i, j] = Vector3.Dot(m[i, j], v);
                }
            }
            return res;
        }

        private Vector3[,] transpose(Vector3[,] J)
        {
            Vector3[,] transpose = new Vector3[J.GetLength(1), J.GetLength(0)];
            for (int i = 0; i < J.GetLength(0); i++)
            {
                for (int j = 0; j < J.GetLength(1); j++)
                {
                    transpose[j, i] = J[i, j];
                }
            }
            return transpose;
        }
    }
}
