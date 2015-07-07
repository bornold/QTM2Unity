﻿using System.Collections;
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
        /// <summary>
        /// Given an incomplete skeleton, returns a complete skeleton
        /// Using IK to fill in the gaps between the joints, or the last skeleton if whole limps are missing
        /// </summary>
        /// <param name="skeleton">The skeleton with joints</param>
        public void ApplyIK(ref BipedSkeleton skeleton)
        {
            //c++;
            //Stopwatch stopwatch2 = //Stopwatch.StartNew();
            IEnumerator skelEnumer = skeleton.GetEnumerator();
            IEnumerator lastSkelEnumer = lastSkel.GetEnumerator();
            //Root and all of roots children MUST have set possition
            TreeNode<Bone> bone;
            while (skelEnumer.MoveNext() && lastSkelEnumer.MoveNext())
            {
                bone = (TreeNode<Bone>)skelEnumer.Current;
                if (!bone.Data.Exists) // Possition of joint no knowned, Solve with IK
                {
                    ///////////////////////////////////////////////Special cases/////////////////////////
                    if (bone.IsRoot || bone.Parent.IsRoot)
                    {
                        CopyFromLast(ref bone, lastSkel[bone.Data.Name]);
                        UnityEngine.Debug.LogWarning("Root is undefined!");
                        continue;
                    }
                    else 
                        if (bone.Parent.Data.Name.Equals(BipedSkeleton.SPINE3))
                    {
                        bone.Data.Pos = new Vector3(bone.Parent.Data.Pos);
                        //Vector3 forward = bone.Parent.Data.GetZAxis();
                        //Vector3 target = bone.Children.First().Data.Pos;
                        bone.Data.Orientation = QuaternionHelper.LookAtUp(
                            bone.Data.Pos, 
                            bone.Children.First().Data.Pos,
                            bone.Parent.Data.GetZAxis());
                        continue;
                    }
                    /////////////////////////////////////////////// Special END//////////////////////////////
                    //Stopwatch stopwatch = //Stopwatch.StartNew();
                    //GC 13.1kB
                    MissingJoint(ref skelEnumer, ref lastSkelEnumer);
                    //stopwatch.Stop();
                    //if (stopwatch.Elapsed.TotalMilliseconds > 3.0)
                    //{
                    //    UnityEngine.Debug.LogWarningFormat("{1}\tTime missing Joint taken: \n\t\t\t{0}ms", stopwatch.Elapsed.TotalMilliseconds, c);
                    //}
                }
            }

            //GC 11.4kB
            //FixRotation(skeleton.First());
            //GC 4kB
            //if (skeleton.Any(z => z.Data.HasNaN))
            //{
            //    UnityEngine.Debug.LogError(skeleton.First(r => r.Data.HasNaN));
            //}
            //GC END
            lastSkel = skeleton;
            //stopwatch2.Stop();
            //if (stopwatch2.Elapsed.TotalMilliseconds > 3.0)
            //{
            //    UnityEngine.Debug.LogWarningFormat("{1} Time taken: \n\t{0}ms", stopwatch2.Elapsed.TotalMilliseconds,c);
            //}
        }

        /// <summary>
        /// If a joints is missing from the skeletontree, fill the joints with the previus frames joints and solve with ik if a joint is found, or return the previus frames joint pos offseted the new position
        /// </summary>
        /// <param name="skelEnum">The enumurator to the missing bone position</param>
        /// <param name="lastSkelEnum">The enumurator to the missing bone position from the last skeleton</param>
        private void MissingJoint(ref IEnumerator skelEnum, ref IEnumerator lastSkelEnum)
        {
            List<Bone> missingChain = new List<Bone>(); // chain to be solved

            //root of chain 
            // missings joints parent from last frame is root in solution
            TreeNode<Bone> curr = ((TreeNode<Bone>)skelEnum.Current).Parent;
            TreeNode<Bone> first = curr;
            TreeNode<Bone> referenceBone = curr.Parent;
            // The root if the chain
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
                    Bone target = new Bone(
                        curr.Data.Name,
                        new Vector3(curr.Data.Pos)
                        );
                    if (!curr.Data.Orientation.IsNaN())
                    {
                        target.Orientation = 
                            new Quaternion(curr.Data.Orientation.Xyz, curr.Data.Orientation.W);
                    }
                    CopyFromLast(ref curr, last);
                    curr.Data.Pos += offset;
                    missingChain.Add(curr.Data);

                    //Stopwatch stopwatch = //Stopwatch.StartNew();
                    IKSolver.SolveBoneChain(missingChain.ToArray(), target, referenceBone.Data); // solve with IK
                    //stopwatch.Stop();
                    //if (stopwatch.Elapsed.TotalMilliseconds > 3.0)
                    //{
                    //    UnityEngine.Debug.LogWarningFormat("{1}\t\tTime IK taken: \n\t\t\t\t{0}ms", stopwatch.Elapsed.TotalMilliseconds,c);
                    //}
                    //if (missingChain.Any(z => z.HasNaN))
                    //{
                    //    UnityEngine.Debug.LogError(missingChain.First(c => c.HasNaN));
                    //}
                    break;
                }
                CopyFromLast(ref curr, last);
                curr.Data.Pos += offset;
                missingChain.Add(curr.Data);
            }

            //if (missingChain.Any(z => z.HasNaN))
            //{
            //    UnityEngine.Debug.LogError(missingChain.First(c => c.HasNaN));
            //}
            if (test)
            {
                //Stopwatch stopwatch = //Stopwatch.StartNew();
                JerkingTest(first);
                //if (missingChain.Any(b => b.HasNaN))
                //{
                //    UnityEngine.Debug.LogError(missingChain.First(b => b.HasNaN));
                //}
                //ConstraintsBeforeReturn(first);
                //if (missingChain.Any(b => b.HasNaN))
                //{
                //    UnityEngine.Debug.LogError(missingChain.First(b => b.HasNaN));
                //}
                //stopwatch.Stop();
                //if (stopwatch.Elapsed.TotalMilliseconds > 1.0)
                //{
                //    UnityEngine.Debug.LogWarningFormat("{1}\t\tTime Jerk and CBR taken: \n\t\t\t\t{0}ms", stopwatch.Elapsed.TotalMilliseconds, c);
                //}
            }
        }
        private void CopyFromLast(ref TreeNode<Bone> curr, Bone last)
        {
            curr.Data.Pos = new Vector3(last.Pos);
            curr.Data.Orientation = new Quaternion(new Vector3(last.Orientation.Xyz), last.Orientation.W);
        }
        private bool FixRotation(TreeNode<Bone> boneTree, bool endWithEndEffector = false)
        {
            bool hasChanged = false;
            foreach (TreeNode<Bone> b in boneTree)
            {
                if (!b.Data.Exists) break;
                if (b.IsRoot || b.Parent.IsRoot || b.Parent.Children.First() != b) continue;
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