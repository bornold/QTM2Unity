﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
namespace QTM2Unity
{
    class IKApplier
    {
        private BipedSkeleton lastSkel;
        public IKSolver IKSolver { private get; set; } 
        public void ApplyIK(ref BipedSkeleton skeleton)
        {
            if (lastSkel == null) lastSkel = skeleton;

            IEnumerator skelEnumer = skeleton.GetEnumerator();
            IEnumerator lastSkelEnumer = lastSkel.GetEnumerator();

            //Root and all of roots children MUST have set possition
            TreeNode<Bone> bone;

            while (skelEnumer.MoveNext() && lastSkelEnumer.MoveNext())
            {
                bone = (TreeNode<Bone>)skelEnumer.Current;
                if (!bone.Data.Exists ) // Possition of joint no knowned, Solve with IK
                {
                    ///////////////////////////////////////////////TEMPORARY/////////////////////////
                    if (bone.Parent.Data.Name.Equals(BipedSkeleton.SPINE3))
                    {
                        bone.Data.Pos = new Vector3(bone.Parent.Data.Pos);
                        Vector3 forward = bone.Parent.Data.GetZAxis();
                        Vector3 target = bone.Children.First().Data.Pos;
                        bone.Data.Orientation = QuaternionHelper.LookAtUp(bone.Data.Pos, target, forward);
                        continue;
                    }
                    /////////////////////////////////////////////// TEMPORARY END//////////////////////////////
                    MissingJoint(bone.Parent.Parent.Data, ref skelEnumer, ref lastSkelEnumer);
                }
            }
            lastSkel = skeleton;
            //return skeleton;
        }

        private void MissingJoint(Bone referenceBone, ref IEnumerator skelEnum, ref IEnumerator lastSkelEnum)
        {
            List<Bone> missingChain = new List<Bone>(); // chain to be solved

            //root of chain 
            // missings joints parent from last frame is root in solution
            TreeNode<Bone> curr = ((TreeNode<Bone>)skelEnum.Current).Parent;
            Bone last = ((TreeNode<Bone>)lastSkelEnum.Current).Parent.Data;
            Vector3 offset = curr.Data.Pos - last.Pos; // offset to move last frames chain to this frames' position
            CopyFromLast(ref curr, last); 
            curr.Data.Pos += offset;
            missingChain.Add(curr.Data); 

            // first missing, copy data from last frame
            curr = ((TreeNode<Bone>)skelEnum.Current);
            last = ((TreeNode<Bone>)lastSkelEnum.Current).Data;
            CopyFromLast(ref curr, last);
            curr.Data.Pos += offset;
            missingChain.Add(curr.Data);
            while (!curr.IsLeaf && skelEnum.MoveNext() && lastSkelEnum.MoveNext()) //while not leaf
            {
                curr = ((TreeNode<Bone>)skelEnum.Current);
                last = ((TreeNode<Bone>)lastSkelEnum.Current).Data;
                if (curr.Data.Exists) // target found! it the last in list
                {
                    Bone target = new Bone(curr.Data.Name,new Vector3(curr.Data.Pos),new Quaternion(new Vector3(curr.Data.Orientation.Xyz), curr.Data.Orientation.W));
                    CopyFromLast(ref curr, last);
                    curr.Data.Pos += offset;
                    missingChain.Add(curr.Data);
                    IKSolver.SolveBoneChain(missingChain.ToArray(), target, referenceBone); // solve with IK
                    break;
                }
                CopyFromLast(ref curr, last);
                curr.Data.Pos += offset;
                missingChain.Add(curr.Data);
            }
        }
        private void CopyFromLast(ref TreeNode<Bone> curr, Bone last)
        {
            curr.Data.Pos = new Vector3(last.Pos);
            curr.Data.Orientation = new Quaternion(new Vector3(last.Orientation.Xyz), last.Orientation.W);
        }
    }
}