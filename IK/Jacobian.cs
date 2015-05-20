using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    abstract class Jacobian : IKSolver
    {
        override public Bone[] SolveBoneChain(Bone[] bones, Bone target, Bone parent)
        {
            // Calculate the distances
            float[] distances;
            GetDistances(out distances, ref bones);

            // J[rows][columns]
            int k = 1; // only one end effector now
            Vector3[,] J = new Vector3[k, bones.Length - 1];
            Vector3[,] rotAxis = new Vector3[k, bones.Length - 1];

            int iter = 0;
            int degrees = 2;
            bool toggle = false;
            float lastDistToTarget = float.MaxValue;
            float distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            while (distToTarget > threshold && iter < maxIterations)
            {
                if (distToTarget >= lastDistToTarget)
                {
                    // rotate root 10 degrees
                    if (toggle) degrees = +10;
                    Quaternion q = Quaternion.FromAxisAngle(rotAxis[0, 0],
                        MathHelper.DegreesToRadians((toggle ? -1 : 1) * degrees));
                    Quaternion q2 = QuaternionHelper.RotationY(MathHelper.DegreesToRadians((toggle ? 1 : -1) * degrees));
                    bones[0].Rotate(q*q2);
                    toggle = !toggle;
                    
                }
                fillJacobian(out J, out rotAxis, ref bones, ref target);
                float[,] dTheta;
                calculateDTheta(out dTheta, ref J, ref bones, ref target);
                bool allZero = true;
                for (int i = 0; i < bones.Length - 1; i++) // go through all joints (not end effector)
                {
                    if (dTheta[i, 0] > 0.0001f) // good values? TODO
                        allZero = false;
                    Quaternion q = Quaternion.FromAxisAngle(rotAxis[0, i], dTheta[i, 0]); // ¤ här kan man lägga in en weighted values om man skulle vilja
                    bones[i].Rotate(q); // Rotate bone i dTheta[i,0] radians around previously calculated axis
                    if (bones[i].StartTwistLimit > -1 && bones[i].EndTwistLimit > -1)
                    {
                        Quaternion rot;
                        if (Constraint.CheckOrientationalConstraint(bones[i], (i > 0) ? bones[i - 1] : parent, out rot))
                        {
                            bones[i].Rotate(rot);
                        }
                    }
                }

                // if we got (almost) no change in the angles we want to force change to not get stuck
                if (allZero)
                {
                    // rotate end effector 2 degrees
                    Quaternion q = Quaternion.FromAxisAngle(rotAxis[0, bones.Length - 2],
                        MathHelper.DegreesToRadians(2));
                    bones[bones.Length - 2].Rotate(q);
                }

                // set all positions
                for (int i = 1; i < bones.Length; i++)
                {
                    Vector3 newPos = bones[i - 1].Pos + distances[i - 1] * bones[i - 1].GetYAxis();

                    if (bones[i - 1].Constraints != Vector4.Zero)
                    {
                        Vector3 res;
                        Quaternion rot;
                        Bone prevBone = (i > 1) ? bones[i - 2] : parent;
                        if (Constraint.CheckRotationalConstraints(bones[i - 1], prevBone, newPos, out res, out rot))
                        {
                            newPos = res;
                            bones[i - 1].RotateTowards(newPos - bones[i - 1].Pos);
                        }
                    }
                    bones[i].Pos = newPos;
                }

                lastDistToTarget = distToTarget;
                distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length; 
                iter++;
            } 
            if ( bones[bones.Length - 1].Orientation.Xyz != Vector3.Zero)
            {
                bones[bones.Length - 1].Pos = target.Pos;
                bones[bones.Length - 1].Orientation = target.Orientation;
                bones[bones.Length - 2].RotateTowards(target.Pos - bones[bones.Length - 2].Pos);
            }
            return bones;
        }

        public void solveMultipleChains(ref TreeNode<Bone> root, ref Bone[] targets)
        {
            // Calculate the distances
            float[] distances;
            getDistances(out distances, ref root);

            Bone[] endEffectors = getEndEffectors(ref root);
            Bone[] bones = getNodes(ref root);

            // J[rows][columns]
            int k = endEffectors.Length; // only one end effector now
            int n = bones.Length;
            Vector3[,] J = new Vector3[k, n];
            Vector3[,] rotAxis = new Vector3[k, n];

            int iter = 0;
            while (!targetReached(ref endEffectors, ref targets) && iter < maxIterations)
            {
                Vector3[] positions = getPositions(ref bones, ref endEffectors); // TEST
                fillJacobian(out J, out rotAxis, ref root, ref endEffectors, ref targets);
                float[] dTheta;
                calculateDTheta(out dTheta, ref J, ref endEffectors, ref targets);

                bool allZero = true;
                for (int i = 0; i < n; i++) // go through all joints (not end effector)
                {
                    if (dTheta[i] > 0.000001) // good values? TODO
                        allZero = false;
                    Quaternion q = Quaternion.FromAxisAngle(rotAxis[0, i], dTheta[i]); // TODO rotationaxis?
                    bones[i].Rotate(q); // Rotate bone i dTheta[i] radians around previously calculated axis
                }

                // if we got (almost) no change in the angles we want to force change to not get stuck
                if (allZero)
                {
                    // rotate end effector 2 degrees
                    Quaternion q = Quaternion.FromAxisAngle(rotAxis[0, bones.Length - 1],
                        MathHelper.DegreesToRadians(2));
                    bones[bones.Length - 1].Rotate(q);
                }

                // set all positions
                IEnumerator<TreeNode<Bone>> it = root.GetEnumerator();
                int index = 0;
                while (it.MoveNext())
                {
                    foreach (var c in it.Current.Children)
                    {
                        c.Data.Pos = it.Current.Data.Pos + distances[index] * it.Current.Data.GetYAxis();
                        index++;
                    }
                }
                iter++;
            }
        }

        private void fillJacobian(out Vector3[,] jacobian, out Vector3[,] rotAxis,
            ref Bone[] bones, ref Bone target)
        {
            int k = 1; // only one end effector now
            jacobian = new Vector3[k, bones.Length - 1];
            rotAxis = new Vector3[k, bones.Length - 1];

            // Create Jacobian matrix J(theta) = (ds[i]/dtheta[j])[ij]
            // ds[i]/dtheta[j] = v[j] x (s[i]-p[j])
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < bones.Length - 1; j++)
                {
                    Vector3 a = Vector3.Cross(bones[bones.Length - 1].Pos - bones[j].Pos, target.Pos - bones[j].Pos);
                    // If a is the zero vector the end effector and the target are aligned
                    // we choose a to be the cross between the bone itself and the vector to the target 
                    if (a.X == 0 && a.Y == 0 && a.Z == 0)
                    {
                        a = Vector3.Cross(bones[j].GetYAxis(), target.Pos - bones[j].Pos);
                    }
                    a.Normalize();

                    rotAxis[i, j] = a;
                    jacobian[i, j] = Vector3.Cross(a, bones[bones.Length - 1].Pos - bones[j].Pos);
                }
            }
        }


        private void fillJacobian(out Vector3[,] jacobian, out Vector3[,] rotAxis,
            ref TreeNode<Bone> root, ref Bone[] endEffectors, ref Bone[] targets)
        {
            int k = targets.Length;
            int n = root.Count() - k;
            jacobian = new Vector3[k, n];
            rotAxis = new Vector3[k, n];

            Bone[] bones = getNodes(ref root);

            //IEnumerator<TreeNode<Bone>> it = root.GetEnumerator();
            // Create Jacobian matrix J(theta) = (ds[i]/dtheta[j])[ij]
            // ds[i]/dtheta[j] = v[j] x (s[i]-p[j])
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Vector3 a = Vector3.Cross(endEffectors[i].Pos - bones[j].Pos, targets[i].Pos - bones[j].Pos);
                    // If a is the zero vector the end effector and the target are aligned
                    // we choose a to be the cross between the bone itself and the vector to the target 
                    if (a.X == 0 && a.Y == 0 && a.Z == 0)
                    {
                        //a = Vector3.Cross(bones[j].GetDirection(), targets[i].Pos - bones[j].Pos);
                        a = bones[j].GetXAxis(); // a; TODO, fix correct rotation axis
                    }
                    a.Normalize();

                    rotAxis[i, j] = a;
                    jacobian[i, j] = Vector3.Cross(a, endEffectors[i].Pos - bones[j].Pos);
                }
            }

        }

        abstract protected void calculateDTheta(out float[,] dTheta, ref Vector3[,] J,
            ref Bone[] bones, ref Bone target);

        abstract protected void calculateDTheta(out float[] dTheta, ref Vector3[,] J,
            ref Bone[] endEffectors, ref Bone[] targets);

        private void getDistances(out float[] distances, ref TreeNode<Bone> root)
        {
            distances = new float[root.Count() - 1]; // TODO will this be correct?
            IEnumerator<TreeNode<Bone>> it = root.GetEnumerator();
            int i = 0;
            while (it.MoveNext())
            {
                foreach (var c in it.Current.Children)
                {
                    distances[i] = (c.Data.Pos - it.Current.Data.Pos).Length;
                    i++;
                }
            }
        }

        private Bone[] getEndEffectors(ref TreeNode<Bone> root)
        {
            List<Bone> res = new List<Bone>();
            IEnumerator<TreeNode<Bone>> it = root.GetEnumerator();
            while (it.MoveNext())
            {
                if (it.Current.IsLeaf)
                    res.Add(it.Current.Data);
            }
            return res.ToArray(); // TODO send back the list and use that instead of an array
        }

        private Bone[] getNodes(ref TreeNode<Bone> root)
        {
            List<Bone> res = new List<Bone>();
            IEnumerator<TreeNode<Bone>> it = root.GetEnumerator();
            while (it.MoveNext())
            {
                if (!it.Current.IsLeaf)
                    res.Add(it.Current.Data);
            }
            return res.ToArray(); // TODO send back the list and use that instead of an array
        }

        private Vector3[] getPositions(ref Bone[] bones, ref Bone[] endEffectors)
        {
            Vector3[] res = new Vector3[bones.Length + endEffectors.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                res[i] = bones[i].Pos;
            }
            for (int i = 0; i < endEffectors.Length; i++)
            {
                res[bones.Length + i] = endEffectors[i].Pos;
            }
            return res;
        }

        private bool equalPositions(ref Vector3[] v1, ref Vector3[] v2, float precision)
        {
            if (v1.Length != v2.Length)
                return false;

            for (int i = 0; i < v1.Length; i++)
            {
                if (!(Math.Abs(v1[i].X - v2[i].X) < precision && Math.Abs(v1[i].Y - v2[i].Y) < precision
                    && Math.Abs(v1[i].Z - v2[i].Z) < precision))
                    return false;
            }

            return true;
        }

        private bool targetReached(ref Bone[] endEffectors, ref Bone[] targets)
        {
            if (endEffectors.Length != targets.Length)
                return false; // TODO exception?

            for (int i = 0; i < endEffectors.Length; i++)
            {
                if ((endEffectors[i].Pos - targets[i].Pos).Length > threshold)
                    return false;
            }
            return true;
        }

        // Helper functions (TODO move these)
        public float[,] add(float[,] m1, float[,] m2)
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

        public Vector3[,] add(Vector3[,] m1, Vector3[,] m2)
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

        public Vector3[] mult(float[,] m, Vector3[] v)
        {
            // #columns in matrix must equal #rows in vector
            if (v.Length != m.GetLength(1))
                return null; // TODO exception

            Vector3[] res = new Vector3[m.GetLength(0)];
            for (int i = 0; i < m.GetLength(0); i++)
            {
                Vector3 entry = Vector3.Zero;
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    entry = Vector3.Add(entry, m[i, j] * v[j]);
                }
                res[i] = entry;
            }
            return res;
        }

        public float[] mult(Vector3[,] m, Vector3[] v)
        {
            // #columns in matrix must equal #rows in vector
            if (v.Length != m.GetLength(1))
                return null; // TODO exception

            float[] res = new float[m.GetLength(0)];
            for (int i = 0; i < m.GetLength(0); i++)
            {
                float entry = 0;
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    entry = entry + Vector3.Dot(m[i, j], v[j]);
                }
                res[i] = entry;
            }
            return res;
        }

        public float mult(Vector3[] v1, Vector3[] v2)
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

        public float[,] mult(Vector3[,] m1, Vector3[,] m2)
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

        public float[,] mult(float scalar, float[,] m)
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

        public float[] mult(float scalar, float[] v)
        {
            float[] res = new float[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                res[i] = scalar * v[i];
            }
            return res;
        }

        public Vector3[] mult(float scalar, Vector3[] v)
        {
            Vector3[] res = new Vector3[v.Length];
            for (int i = 0; i < v.Length - 1; i++)
            {
                res[i] = scalar * v[i];
            }
            return res;
        }

        public Vector3[] mult(Vector3[] v, float scalar)
        {
            return mult(scalar, v);
        }

        public float[] mult(Vector3[] vArray, Vector3 v)
        {
            float[] res = new float[vArray.Length];
            for (int i = 0; i < vArray.Length; i++)
            {
                res[i] = Vector3.Dot(vArray[i], v);
            }
            return res;
        }

        public Vector3[,] mult(float[,] fs, Vector3 v)
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

        public Vector3[,] mult(float scalar, Vector3[,] m)
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

        public Vector3[,] mult(Vector3[,] m, float scalar)
        {
            return mult(scalar, m);
        }

        public float[,] mult(Vector3[,] m, Vector3 v)
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

        public Vector3[,] transpose(Vector3[,] m)
        {
            Vector3[,] transpose = new Vector3[m.GetLength(1), m.GetLength(0)];
            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    transpose[j, i] = m[i, j];
                }
            }
            return transpose;
        }

        public float[,] transpose(float[,] m)
        {
            float[,] transpose = new float[m.GetLength(1), m.GetLength(0)];
            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    transpose[j, i] = m[i, j];
                }
            }
            return transpose;
        }
    }
}
