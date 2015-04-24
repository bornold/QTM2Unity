using OpenTK;

namespace QTM2Unity
{
    abstract class IKSolver
    {
        abstract public Bone[] SolveBoneChain(Bone[] bones, Bone target, Bone parent);
        protected float threshold = 0.005f; 
        protected int maxIterations = 500;
        protected float pushValue = 1;

        // TODO probably better if we just keep length in bones... oor is it...
        protected void GetDistances(out float[] distances, ref Bone[] bones)
        {
            distances = new float[bones.Length - 1];
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = (bones[i].Pos - bones[i + 1].Pos).Length;
            }
        }
        protected void ForwardKinematics(ref Bone[] bones, Quaternion rotation)
        {
            ForwardKinematics(ref bones, rotation, 0, bones.Length - 1);
        }
        protected void ForwardKinematics(ref Bone[] bones, Quaternion rotation, int i)
        {
            ForwardKinematics(ref bones, rotation, i, bones.Length-1);
        }
        protected void ForwardKinematics(ref Bone[] bones, Quaternion rotation, int i, int length)
        {
            for (int j = length; j >= i; j--)
            {
                if (j > i)
                {
                    bones[j].Pos = bones[i].Pos +
                        Vector3.Transform((bones[j].Pos - bones[i].Pos), rotation);
                }

                // rotate orientation
                bones[j].Rotate(rotation);
            }
        }

        // Returns true if target is located on the chain
        // Assumes target is reachable
        protected bool IsTargetOnChain(ref Bone[] bones, ref Bone target)
        {
            // If every joint in the chain (except end effector) has the same direction vector
            // the chain is straight
            for (int i = 0; i < bones.Length - 2; i++)
            {
                Vector3 y1 = bones[i].GetYAxis();
                Vector3 y2 = bones[i + 1].GetYAxis();
                if (y1.X - y2.X > 0.001 && y1.Y - y2.Y > 0.001 && y1.Z - y2.Z > 0.001)
                {
                    return false;
                }
            }
            Vector3 a = bones[bones.Length - 1].Pos; // end effector
            Vector3 b = 2 * bones[0].Pos - a; // end effector reflected in root (ref = 2*root - endef)

            if (Vector3Helper.Parallel(a - target.Pos, b - target.Pos))
            {
                // Since target is reachable it is on the line
                return true;
            }

            return false;
        }
    }
}
