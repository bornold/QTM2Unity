using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
namespace QTM2Unity
{
    class IKApplier
    {
        private BipedSkeleton lastSkel;
        public IKSolver IKSolver { private get; set; } 
        private IKSolver fabrik = new FABRIK();
        public bool test = false;
        public IKApplier()
        {
                lastSkel = new BipedSkeleton();
                IKSolver = new CCD();
                fabrik.MaxIterations = 20;
                IKSolver.MaxIterations = 200;
        }
        public void ApplyIK(ref BipedSkeleton skeleton)
        {
            if (lastSkel.Any(z => z.Data.HasNaN))
            {
                UnityEngine.Debug.LogError(lastSkel.First(c => c.Data.HasNaN));
            }
            IEnumerator skelEnumer = skeleton.GetEnumerator();
            IEnumerator lastSkelEnumer = lastSkel.GetEnumerator();
            //Root and all of roots children MUST have set possition
            TreeNode<Bone> bone;
            while (skelEnumer.MoveNext() && lastSkelEnumer.MoveNext())
            {
                bone = (TreeNode<Bone>)skelEnumer.Current;
                if (!bone.Data.Exists) // Possition of joint no knowned, Solve with IK
                {
                    ///////////////////////////////////////////////TEMPORARY/////////////////////////
                    if (bone.IsRoot || bone.Parent.IsRoot)
                    {
                        CopyFromLast(ref bone, lastSkel[bone.Data.Name]);
                        continue;
                    }
                    else if (bone.Parent.Data.Name.Equals(BipedSkeleton.SPINE3))
                    {
                        bone.Data.Pos = new Vector3(bone.Parent.Data.Pos);
                        Vector3 forward = bone.Parent.Data.GetZAxis();
                        Vector3 target = bone.Children.First().Data.Pos;
                        bone.Data.Orientation = QuaternionHelper.LookAtUp(bone.Data.Pos, target, forward);
                        continue;
                    }
                    /////////////////////////////////////////////// TEMPORARY END//////////////////////////////
                    MissingJoint(bone.Parent.Parent, ref skelEnumer, ref lastSkelEnumer);
                }
            }

            //FixRotation(skeleton.First());
            if (skeleton.Any(z => z.Data.HasNaN))
            {
                UnityEngine.Debug.LogError(skeleton.First(c => c.Data.HasNaN)); 
            }
            //UnityDebug.sanity(skeleton);
            lastSkel = skeleton;
        }

        private void MissingJoint(TreeNode<Bone> referenceBone, ref IEnumerator skelEnum, ref IEnumerator lastSkelEnum)
        {
            List<Bone> missingChain = new List<Bone>(); // chain to be solved

            //root of chain 
            // missings joints parent from last frame is root in solution
            TreeNode<Bone> curr = ((TreeNode<Bone>)skelEnum.Current).Parent;
            TreeNode<Bone> first = curr;
            Bone target = new Bone("target");
            Bone last = ((TreeNode<Bone>)lastSkelEnum.Current).Parent.Data;
            Vector3 offset = curr.Data.Pos - last.Pos; // offset to move last frames chain to this frames' position
            //QTransition = QFinal * QInitial^{-1}
            //Quaternion roffset = referenceBone.Orientation * Quaternion.Conjugate(((TreeNode<Bone>)lastSkelEnum.Current).Parent.Parent.Data.Orientation);
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
                    target = new Bone(
                        curr.Data.Name,
                        new Vector3(curr.Data.Pos)
                        );
                    target.Orientation = curr.Data.Orientation.IsNaN() ? Quaternion.Identity : new Quaternion(curr.Data.Orientation.Xyz, curr.Data.Orientation.W);
                    CopyFromLast(ref curr, last);
                    curr.Data.Pos += offset;

                    missingChain.Add(curr.Data);
                    Bone[] bones = missingChain.ToArray();
                    //if (!thisOrThat  || (thisOrThat  && !fabrik.SolveBoneChain(bones, target, referenceBone)))
                    {
                        IKSolver.SolveBoneChain(bones, target, referenceBone.Data); // solve with IK
                        break;
                    }

                }
                CopyFromLast(ref curr, last);
                curr.Data.Pos += offset;
                missingChain.Add(curr.Data);
            }
            if (test)
            {

                ConstraintsBeforeReturn(first);
                if (missingChain.Any(b => b.HasNaN))
                {
                    UnityEngine.Debug.LogError(missingChain.First(b => b.HasNaN));
                }
                JerkingTest(first);
                if (missingChain.Any(b => b.HasNaN))
                {
                    UnityEngine.Debug.LogError(missingChain.First(b => b.HasNaN));
                }
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
                if (b.IsRoot || b.Parent.IsRoot || b.Parent.Children.First() != b) continue;
                Vector3 ray1 = b.Parent.Data.GetYAxis();
                Vector3 ray2 = (b.Data.Pos - b.Parent.Data.Pos);
                bool parallel = Vector3Helper.Parallel(ray1, ray2, 0.01f);
                if (!parallel)
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
        private bool JerkingTest(TreeNode<Bone> bones)
        {
            bool hasChanges = false;
            foreach (TreeNode<Bone> bone in bones)
            {
                if (bone.IsRoot) continue;
                Bone currBone = bone.Parent.Data;
                Bone lastFrameBone = lastSkel[currBone.Name];
                Quaternion oriInitial = currBone.Orientation;
                Quaternion oriFinal = lastFrameBone.Orientation;
                float quatDiff = QuaternionHelper.DiffrenceBetween(oriInitial, oriFinal);
                if (quatDiff > 0.1f)
                {
                    UnityEngine.Debug.Log(currBone.Name + " twisted " + quatDiff + " amount");
                    /*
                    Vector3 prepos = new Vector3(currBone.Pos);
                    Vector3 offset = currBone.Pos - lastFrameBone.Pos;
                    foreach (TreeNode<Bone> t in lastSkel.First(g => g.Data.Name == currBone.Name).Children.First())
                    {
                        UnityDebug.DrawLine(t.Parent.Data.Pos + offset, t.Data.Pos + offset, UnityEngine.Color.blue);
                    }
                    foreach (TreeNode<Bone> t in bone)
                    {
                        UnityDebug.DrawLine(t.Parent.Data.Pos, t.Data.Pos, UnityEngine.Color.green);
                    }
                    */
                    Quaternion y = Quaternion.Slerp(oriInitial, oriFinal, 0.1f);
                    Quaternion yTrans2 = y * Quaternion.Invert(oriFinal);
                    Quaternion svart = Quaternion.Invert(yTrans2);
                    UnityEngine.Debug.LogWarning(" FK " + bone.Parent.Data.Name + " with " + svart);
                    FK(bone.Parent, svart);
                    hasChanges = true;
                }
            }
            return hasChanges;
        }
        /// <summary>
        /// Rotate the first joint, and move the rest
        /// </summary>
        /// <param name="bvn">The first joint to be rotated</param>
        /// <param name="svart"> The quaternion to rotate by</param>
        private void FK(TreeNode<Bone> bvn, Quaternion svart)
        {
            if (bvn.IsRoot || bvn.IsLeaf)
            {
                UnityEngine.Debug.LogWarning("FK called on root or leaf: " + bvn);
                return;
            }
            
            Vector3 root = new Vector3(bvn.Data.Pos);
            Vector3 parent = new Vector3(root);
            foreach (TreeNode<Bone> t in bvn)
            {
                if (t != bvn)// dont move the first
                {
                    Vector3 child = new Vector3(t.Data.Pos);
                    Vector3 theTransform = Vector3.Transform((child - root), svart);
                    Vector3 transformedChild = theTransform + root;
                    //UnityEngine.Debug.LogWarning(t.Data.Name  + " from " + t.Data.Pos + "  to " +  transformedChild);
                    t.Data.Pos = transformedChild;
                    parent = new Vector3(transformedChild);
                }
                if (!t.IsLeaf)
                {
                    t.Data.Orientation = svart * t.Data.Orientation;
                }
            }
            FixRotation(bvn);
        }
        private bool ConstraintsBeforeReturn(TreeNode<Bone> bone)
        {
            bool anychange = false; //FixRotation(bone);

            foreach (var tnb in bone)
            {
                if (tnb.IsRoot || tnb.IsLeaf) continue;
                Quaternion rot;
                Vector3 res;
                if (!tnb.Data.HasNaN && 
                    !tnb.Children.First().Data.HasNaN && 
                    tnb.Data.HasConstraints)
                {
                    Vector3 child = tnb.Children.First().Data.Pos;
                    if (Constraint.CheckOrientationalConstraint(tnb.Data, tnb.Parent.Data, out rot))
                    {
                        tnb.Data.Rotate(rot);
                        anychange = true;
                    }
                    if (Constraint.CheckRotationalConstraints(tnb.Data, tnb.Parent.Data.Orientation, child, out res, out rot ))
                    {
                        //UnityEngine.Debug.Log("CONSTRAINTS FK " + tnb.Data.Name + " with " + rot);
                        FK(tnb, rot);
                        anychange = true;
                    }
                }
                return anychange;
            }
            //anychange = FixRotation(bone) || anychange;
            return anychange;
        }
    }
}