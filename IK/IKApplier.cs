using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using System.Diagnostics;
namespace QTM2Unity
{
    class IKApplier
    {
        //private uint c = 0;
        private BipedSkeleton lastSkel;
        public IKSolver IKSolver { private get; set; } 
        //private IKSolver fabrik = new FABRIK();
        public bool test = false;
        public IKApplier()
        {
                lastSkel = new BipedSkeleton();
                IKSolver = new CCD();
                //fabrik.MaxIterations = 20;
                //IKSolver.MaxIterations = 200;
        }

        public void ApplyIK(ref BipedSkeleton skeleton)
        {
            //Root and all of roots children MUST have set possition
            foreach (var bone in skeleton.Root)
            {
                if (!bone.Data.Exists) // Possition of joint no knowned, Solve with IK
                {
                    ///////////////////////////////////////////////Special cases/////////////////////////
                    //if (bone.IsRoot || bone.Parent.IsRoot)
                    //{
                    //    CopyFromLast(bone, lastSkel[bone.Data.Name]);
                    //    continue;
                    //}
                    
                    if (bone.Parent.Data.Name.Equals(BipedSkeleton.SPINE3)
                                || bone.Data.Name.StartsWith("trap"))
                    {
                        bone.Data.Pos = new Vector3(bone.Parent.Data.Pos);
                        bone.Data.Orientation = QuaternionHelper.LookAtUp(
                            bone.Data.Pos,
                            bone.Children.First().Data.Pos,
                            bone.Parent.Data.GetZAxis());
                        continue;
                    }
                    /////////////////////////////////////////////// Special END//////////////////////////////
                    //7.3kb garbage
                    MissingJoint(bone);
                    //12.2kb
                }
            }
            lastSkel = skeleton;
        }

        /// <summary>
        /// If a joints is missing from the skeletontree, fill the joints with the previus frames joints and solve with ik if a joint is found, or return the previus frames joint pos offseted the new position
        /// </summary>
        /// <param name="skelEnum">The enumurator to the missing bone position</param>
        /// <param name="lastSkelEnum">The enumurator to the missing bone position from the last skeleton</param>
        private void MissingJoint(TreeNode<Bone> missingJoint)
        { 
            // missings joints parent from last frame is root in solution
            //root of chain 
            TreeNode<Bone> lastSkelBone = lastSkel.Root.FindTreeNode(a => a.Data.Name == missingJoint.Data.Name);
            List<Bone> missingChain = new List<Bone>(); // chain to be solved
            // The root if the chain
            Vector3 offset = missingJoint.Parent.Data.Pos - lastSkelBone.Parent.Data.Pos; // offset to move last frames chain to this frames' position
            CopyFromLast(missingJoint.Parent.Data, lastSkelBone.Parent.Data);
            missingJoint.Parent.Data.Pos += offset;
            missingChain.Add(missingJoint.Parent.Data);
            bool iksolved = false;
            IEnumerator lastSkelEnum = lastSkelBone.GetEnumerator();
            Bone last;
            foreach(var curr in missingJoint)
            {
                lastSkelEnum.MoveNext();
                last = ((TreeNode<Bone>)lastSkelEnum.Current).Data;
                if (curr.Data.Exists) // target found! it the last in list
                {
                    Bone target = new Bone(
                        curr.Data.Name,
                        new Vector3(curr.Data.Pos)
                        );
                    if (!curr.Data.Orientation.IsNaN())
                    {
                        target.Orientation =
                            new Quaternion(curr.Data.Orientation.Xyz, curr.Data.Orientation.W);
                    }
                    CopyFromLast(curr.Data, last);
                    curr.Data.Pos += offset;
                    missingChain.Add(curr.Data);
                    IKSolver.SolveBoneChain(missingChain.ToArray(), target, missingJoint.Parent.Parent.Data); // solve with IK
                    iksolved = true;
                    break;
                }
                CopyFromLast(curr.Data, last);
                curr.Data.Pos += offset;
                missingChain.Add(curr.Data);
            }
            if (!iksolved)
            {
                var q2 = missingJoint.Parent.Parent.Data.Orientation;
                var q1 = lastSkelBone.Parent.Parent.Data.Orientation;
                FK(missingJoint.Parent, (q2 * Quaternion.Invert(q1)));
            } else if (test)
            {
                JerkingTest(missingJoint.Parent);
                ConstraintsBeforeReturn(missingJoint.Parent);
            }
        }
        private void CopyFromLast(Bone curr, Bone last)
        {
            curr.Pos = new Vector3(last.Pos);
            curr.Orientation = new Quaternion(new Vector3(last.Orientation.Xyz), last.Orientation.W);
        }
        private bool FixRotation(TreeNode<Bone> boneTree, bool endWithEndEffector = false)
        {
            bool hasChanged = false;
            foreach (TreeNode<Bone> b in boneTree)
            {
                if (!b.Data.Exists) break;
                if (b.IsRoot || b.Parent.IsRoot || b.Data.ParentPointer != Quaternion.Identity) continue;
                Vector3 ray2 = (b.Data.Pos - b.Parent.Data.Pos);
                if (!Vector3Helper.Parallel(b.Parent.Data.GetYAxis(), ray2, 0.01f))
                {
                    //UnityEngine.Debug.Log("fixing rot for " + b.Parent.Data.Name);
                    ray2.NormalizeFast();
                    b.Parent.Data.RotateTowards(ray2);
                    hasChanged = true;
                }
                if (endWithEndEffector && b.IsLeaf) break;
            }
            return hasChanged;
        }

        private bool ConstraintsBeforeReturn(TreeNode<Bone> bone)
        {
            bool anychange = false;
            foreach (var tnb in bone)
            {
                if (tnb.IsRoot || tnb.IsLeaf) continue;
                if (!tnb.Data.HasNaN && tnb.Data.HasConstraints)
                {
                    Quaternion rot;
                    if (IKSolver.CheckOrientationalConstraint(tnb.Data, tnb.Parent.Data, out rot))
                    {
                        tnb.Data.Rotate(rot);
                        anychange = true;
                    }
                    Vector3 res;
                    Vector3 child = tnb.Children.First().Data.Pos;
                    if (!child.IsNaN() &&
                        IKSolver.CheckRotationalConstraints(
                                        tnb.Data, tnb.Parent.Data.Orientation,
                                        child, out res, out rot))
                    {
                        FK(tnb, rot);
                        anychange = true;
                    }
                    if (IKSolver.CheckOrientationalConstraint(tnb.Data, tnb.Parent.Data, out rot))
                    {
                        tnb.Data.Rotate(rot);
                        anychange = true;
                    }
                }
            }
            return anychange;
        }
        private bool JerkingTest(TreeNode<Bone> bones)
        {
            bool hasChanges = false;
            foreach (TreeNode<Bone> bone in bones)
            {
                if (bone.IsRoot || bone.Data.HasNaN) continue;
                Bone lastFrameBone = lastSkel[bone.Data.Name];
                #region Poss

                //Vector3 posFinal = currBone.Pos;
                Vector3 posInitial = lastFrameBone.Pos;
                Vector3 diffInitToFinalVec = (bone.Data.Pos - posInitial);
                if (diffInitToFinalVec.Length > 0.1f)
                {
                    diffInitToFinalVec.NormalizeFast();
                    diffInitToFinalVec *= 0.05f;
                    //Vector3 newFinalPos = (posInitial + diffInitToFinalVec);
                    //Vector3 parentPos = bone.Parent.Data.Pos;
                    Quaternion rotToNewPos = 
                        QuaternionHelper.RotationBetween(
                                bone.Parent.Data.GetYAxis(),
                                ((posInitial + diffInitToFinalVec) - bone.Parent.Data.Pos));
                    /////////////////////////////
                    //var tnb = lastSkel.First(g => g.Data.Name == bone.Data.Name);
                    //Vector3 offset = bone.Parent.Data.Pos - tnb.Parent.Data.Pos;
                    //UnityEngine.Debug.LogError(bone.Data.Name + " diffed with " + diffInitToFinalVec.Length + "\nDiffvec:" + diffInitToFinalVec + " length " + diffInitToFinalVec.Length);
                    //UnityEngine.Color c = UnityEngine.Color.magenta;
                    //foreach (TreeNode<Bone> t in tnb)
                    //{
                    //    UnityDebug.DrawLine(t.Parent.Data.Pos + offset, t.Data.Pos + offset, UnityEngine.Color.blue);
                    //    c = UnityEngine.Color.blue;
                    //}
                    //foreach (TreeNode<Bone> t in bone)
                    //{
                    //    UnityDebug.DrawLine(t.Parent.Data.Pos, t.Data.Pos, UnityEngine.Color.green);
                    //}
                    /////////////////////////////
                    FK(bone.Parent, rotToNewPos);
                    hasChanges = true;
                }
                #endregion
                #region Rots
                Quaternion oriFinal = bone.Data.Orientation;
                Quaternion oriInitial = lastFrameBone.Orientation;
                if (!bone.IsLeaf)
                {
                    float quatDiff = QuaternionHelper.DiffrenceBetween(oriFinal, oriInitial);
                    if (quatDiff > 0.06f)
                    {
                        //UnityEngine.Debug.Log(currBone.Name + " jerked with " + quatDiff + " amount\n" + "Twisting back " + calc * 100 + "%");
                        //Vector3 prepos = new Vector3(currBone.Pos);
                        //Vector3 offset = currBone.Pos - lastFrameBone.Pos;
                        //foreach (TreeNode<Bone> t in lastSkel.First(g => g.Data.Name == currBone.Name))
                        //{
                        //    UnityDebug.DrawLine(t.Parent.Data.Pos + offset, t.Data.Pos + offset, UnityEngine.Color.blue);
                        //}
                        //foreach (TreeNode<Bone> t in bone)
                        //{
                        //    UnityDebug.DrawLine(t.Parent.Data.Pos, t.Data.Pos, UnityEngine.Color.green);
                        //}

                        float slerp = (1 - quatDiff) - (Mathf.Cos((MathHelper.Pi * quatDiff) / 2) - (1 - quatDiff * 0.8f));
                        Quaternion qTrans = Quaternion.Invert(
                            Quaternion.Slerp(oriInitial, oriFinal, slerp) 
                            * Quaternion.Invert(oriInitial));
                        FK(bone, qTrans);
                        hasChanges = true;
                    }
                }
                #endregion
            }
            return hasChanges;
        }
        /// <summary>
        /// Rotate the first joint, and move the rest according to a Quaternion
        /// </summary>
        /// <param name="bvn">The first joint to be rotated</param>
        /// <param name="rotation"> The quaternion to rotate by</param>
        private void FK(TreeNode<Bone> bvn, Quaternion rotation)
        {
            if (bvn.IsLeaf || bvn.IsRoot) return;
            Vector3 root = new Vector3(bvn.Data.Pos);
            foreach (TreeNode<Bone> t in bvn)
            {
                if (!t.Data.Exists) break;
                if (t != bvn)
                {
                    t.Data.Pos = Vector3.Transform((t.Data.Pos - root), rotation) + root;
                }
                if (!t.IsLeaf)
                {
                    t.Data.Orientation = rotation * t.Data.Orientation;
                }
            }
        }
    }
}