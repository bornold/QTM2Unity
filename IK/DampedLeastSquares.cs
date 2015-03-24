using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Debug = UnityEngine.Debug;

namespace QTM2Unity
{
    class DampedLeastSquares : Jacobian
    {

        override protected void calculateDTheta(out float[,] dTheta, ref Vector3[,] J,
            ref Bone[] bones, ref Bone target)
        {
            // dTheta = J.transpose * Inverse(J*J.transpose + lambda^2*I) * e
            float lambda = 1.1f; 
            Vector3 e = target.Pos - bones[bones.Length - 1].Pos;
            Vector3[,] JT = transpose(J);
            float[,] JJT = mult(J, JT);
            // Obs: JTT[0,0] only works because we have one end effector only
            dTheta = mult(mult(JT, (1 / JJT[0,0] * lambda * lambda)), e);
        }

#if t
        private static float threshold = 0.0001f;

       /* override public Bone[] solveBoneChain(Bone[] bones, Vector3 target)
        {
            float[] distances;
            getDistances(out distances, ref bones);

            Vector3[,] J = new Vector3[3, bones.Length - 1];

            int iter = 0;
            while ((bones[bones.Length - 1].Pos - target).Length > threshold && iter < 1000)
            {
                // Create Jacobian matrix J(theta) = (ds[i]/dtheta[j])[ij]
                // ds[i]/dtheta[j] = v[j] x (s[i]-p[j])
                for (int i = 0; i < 3; i++)
                {
                    J[i,0] = Vector3.Cross(bones[i].getRight(), bones[bones.Length - 1].Pos - bones[i].Pos); // x
                    J[i,1] = Vector3.Cross(bones[i].getDirection(), bones[bones.Length - 1].Pos - bones[i].Pos); // y
                    J[i,2] = Vector3.Cross(bones[i].getUp(), bones[bones.Length - 1].Pos - bones[i].Pos); // z
                        // Obs: bones[bones.Length-1] is the last in the chain, the end effector. 
                        // Will be different when we have several end effectors
                        //Debug.Log("J[" + i + "] = " + J[i].X + ", " + J[i].Y + ", " + J[i].Z);
                    
                }

                // dTheta = J.transpose * Inverse(J*J.transpose + lambda^2*I) * e
                float lambda = 1.1f; //TODO
                Vector3 e = target - bones[bones.Length - 1].Pos;
                Vector3[,] JT = transpose(J);
                float[,] JJT = mult(J, JT);
                float[,] I = identity(JJT.GetLength(0));
                /*float[,] inv = inverse(add(JJT, mult(lambda*lambda, I)));
                //Debug.Log("JJT " + JJT);
                float[,] dTheta = mult(JT, mult(inv, e));

                //print dTheta
                /*string s = "";
                for (int i = 0; i < dTheta.GetLength(0); i++)
                {
                    s += " " + dTheta[i];
                }
                Debug.Log(s);*/

                // Let's try
                /*for (int i = 0; i < bones.Length - 1; i++) // go through all joints (not end effector)
                {
                    //Debug.Log("Rotate " + bones[i].Name + " " + dTheta[i, 0] + " degrees");
                    // TODO degrees or radians??
                    Quaternion q = Quaternion.FromAxisAngle(J[i], /*MathHelper.DegreesToRadians(*///dTheta[i]/*)*/);
                    /*bones[i].rotate(q);
                }

                // set all positions
                for (int i = 1; i < bones.Length; i++)
                {
                    bones[i].Pos = bones[i - 1].Pos + distances[i - 1] * bones[i - 1].getDirection();
                }
                iter++;*/
           /* }
            return bones;
        }*/

        override public Bone[] solveBoneChain(Bone[] bones, Bone target, Vector3 L1)
        {
            float[] distances;
            getDistances(out distances, ref bones);

            Vector3[] J = new Vector3[bones.Length-1];
            Vector3[] rotAxis = new Vector3[bones.Length - 1];

            int iter = 0;
            while ((bones[bones.Length - 1].Pos - target.Pos).Length > threshold && iter < 10000)
            {

                // Create Jacobian matrix J(theta) = (ds[i]/dtheta[j])[ij]
                // ds[i]/dtheta[j] = v[j] x (s[i]-p[j])
                for (int i = 0; i < bones.Length - 1; i++)
                {
                    Vector3 a = Vector3.Cross(bones[bones.Length - 1].Pos - bones[i].Pos, target.Pos - bones[i].Pos);
                    //Debug.Log("a: " + a.X + "," + a.Y + "," + a.Z);
                    // If a is the zero vector the end effector and the target are aligned
                    // we choose the cross between the bone itself and the vector to the target 
                    if (a.X == 0 && a.Y == 0 && a.Z == 0)
                    {
                        a = Vector3.Cross(bones[i].GetDirection(), target.Pos - bones[i].Pos);
                    }
                    a.Normalize();

                    rotAxis[i] = a;
                    J[i] = Vector3.Cross(a, bones[bones.Length - 1].Pos - bones[i].Pos);
                    //J[i] = Vector3.Cross(bones[i].GetRight(), bones[bones.Length - 1].Pos - bones[i].Pos);
                    // Obs: bones[bones.Length-1] is the last in the chain, the end effector. 
                    // Will be different when we have several end effectors
                    //Debug.Log("J[" + i + "] = " + J[i].X + ", " + J[i].Y + ", " + J[i].Z);
                }

                // dTheta = J.transpose * Inverse(J*J.transpose + lambda^2*I) * e
                float lambda = 1.1f; //TODO
                Vector3 e = target.Pos - bones[bones.Length - 1].Pos;
                float JJT = mult(J, J);
                //Debug.Log("JJT " + JJT);
                float[] dTheta = mult(mult(J, (1 / JJT * lambda * lambda)), e);

                bool allZero = true;
                for (int i = 0; i < bones.Length - 1; i++) // go through all joints (not end effector)
                {
                    if (dTheta[i] > 0.000001) // good values? TODO
                        allZero = false;
                    Quaternion q = Quaternion.FromAxisAngle(rotAxis[i], dTheta[i]);
                    bones[i].Rotate(q); // Rotate bone i dTheta[i,0] radians around previously calculated axis
                }

                // if we got (almost) no change in the angles we want to force change to not get stuck
                if (allZero)
                {
                    // rotate end effector 2 degrees
                    Quaternion q = Quaternion.FromAxisAngle(rotAxis[bones.Length - 2],
                        MathHelper.DegreesToRadians(2));
                    bones[bones.Length - 2].Rotate(q);
                }

                // set all positions
                for (int i = 1; i < bones.Length; i++)
                {
                    bones[i].Pos = bones[i - 1].Pos + distances[i - 1] * bones[i - 1].GetDirection();
                }
                iter++;
            }
            Debug.Log("Iterations: " + iter);
            return bones;
        }

        private float[,] identity(int dimension)
        {
            float[,] I = new float[dimension, dimension];
            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    if (i == j)
                        I[i, j] = 1;
                    else
                        I[i, j] = 0;
                }
            }
            return I;
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
            for (int i = 0; i < m.GetLength(0)-1; i++)
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
                    res[i,j] = Vector3.Dot(m[i,j], v);
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
#endif
    }
}
