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
                if (!b.Data.Exists ) // Possition of joint no knowned, Solve with IK
                {
                    //UnityEngine.Debug.Log(b.Data.Name + "is missing");
///////////////////////////////////////////////FUCK UGLY AND TEMPORARY//////////////////////////////////////////////////////////////
                    Bone referenceBone;
                    if (b.Parent.Data.Name.Equals(BipedSkeleton.UPPERLEG_L) 
                        || b.Parent.Data.Name.Equals(BipedSkeleton.UPPERLEG_R))
                    {
                        referenceBone = new Bone(
                            "",
                            b.Parent.Parent.Data.Pos,
                            b.Parent.Parent.Data.Orientation * QuaternionHelper.RotationZ(MathHelper.Pi));
                    }
                    else if (b.Parent.Data.Name.Equals(BipedSkeleton.SHOULDER_R))
                    {
                        referenceBone = new Bone(
                            "",
                            b.Parent.Data.Pos,
                            b.Parent.Data.Orientation * QuaternionHelper.RotationZ(-OpenTK.MathHelper.PiOver2));
                    }
                    else if (b.Parent.Data.Name.Equals(BipedSkeleton.SHOULDER_L))
                    {
                        referenceBone = new Bone(
                            "",
                            b.Parent.Parent.Data.
                            b.Parent.Parent.Data.Orientation * QuaternionHelper.RotationZ(OpenTK.MathHelper.PiOver2));
                    }
                    else
                    {
                        referenceBone = b.Parent.Parent.Data;
                    }
///////////////////////////////////////////////FUCK UGLY AND TEMPORARY END//////////////////////////////////////////////////////////////
                    foreach (Bone a in MissingJoint(b, iks, referenceBone, ref it)) skeleton[a.Name] = a;
                }
            }
            lastSkel = skeleton;
            return skeleton;
        }


        private List<Bone> MissingJoint(TreeNode<Bone> b, IKSolver iks, Bone parent, ref IEnumerator it)
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
                    missingChain = iks.SolveBoneChain(missingChain.ToArray(), target, parent).ToList(); // solve with IK
                    break;
                }
            }
            return missingChain;
        }
    }
}