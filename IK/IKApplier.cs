using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
namespace QTM2Unity
{
    class IKApplier
    {
        CCD ccd = new CCD(); // CCD for now

        BipedSkeleton lastSkel;
        public BipedSkeleton ApplyIK(BipedSkeleton skeleton, IKSolver iks)
        {
            if (lastSkel == null) lastSkel = skeleton;

            IEnumerator it = skeleton.GetEnumerator();
            //TODO ATTENTION! 
            //Root and all of roots children MUST have set possition
            TreeNode<Bone> b;
            while (it.MoveNext())
            {
                b = (TreeNode<Bone>)it.Current;
                if (!b.Data.Exists) // Possition of joint no knowned, Solve with IK
                {
                    Vector3 L1 = b.Parent.Data.GetDirection();
                    foreach (Bone a in MissingJoint(b, iks, L1, ref it)) skeleton[a.Name] = a;
                }
            }
            lastSkel = skeleton;
            return skeleton;
        }


        private List<Bone> MissingJoint(TreeNode<Bone> b, IKSolver iks, Vector3 L1, ref IEnumerator it)
        {
            Bone root = lastSkel[b.Parent.Data.Name]; // last frames' parent is root in solution
            Bone missing = lastSkel[b.Data.Name]; // this node that is missing
            Vector3 offset = b.Parent.Data.Pos - root.Pos; // offset to move last frames chain to this frames' position
            root.Pos += offset; // move chain to this place
            missing.Pos += offset;
            List<Bone> missingChain = new List<Bone>() { root, missing }; // chain to be solved
            while (!b.IsLeaf && it.MoveNext()) //while not leaf
            {
                b = (TreeNode<Bone>)it.Current;
                missing = lastSkel[b.Data.Name];
                missing.Pos += offset;
                missingChain.Add(missing);
                if (b.Data.Exists) // target found! it the last in list
                {
                    Bone target = b.Data;
                    missingChain = iks.solveBoneChain(missingChain.ToArray(), target, L1).ToList(); // solve with IK
                    break;
                }
            }
            return missingChain;
        }
    }
}