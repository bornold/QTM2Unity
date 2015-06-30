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
        public bool thisOrThat = false;
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
                    MissingJoint(bone.Parent.Parent.Data, ref skelEnumer, ref lastSkelEnumer);
                }
            }

            FixRotation(skeleton.First());
            if (skeleton.Any(z => z.Data.HasNaN))
            {
                UnityEngine.Debug.LogError(skeleton.First(c => c.Data.HasNaN)); 
            }
            //UnityDebug.sanity(skeleton);
            lastSkel = skeleton;
        }

        private void MissingJoint(Bone referenceBone, ref IEnumerator skelEnum, ref IEnumerator lastSkelEnum)
        {
            List<Bone> missingChain = new List<Bone>(); // chain to be solved

            //root of chain 
            // missings joints parent from last frame is root in solution
            TreeNode<Bone> curr = ((TreeNode<Bone>)skelEnum.Current).Parent;
            Bone target = new Bone("target");
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
                    target = new Bone(
                        curr.Data.Name,
                        new Vector3(curr.Data.Pos)
                        );
                    target.Orientation = curr.Data.Orientation.IsNaN() ? Quaternion.Identity : new Quaternion(curr.Data.Orientation.Xyz, curr.Data.Orientation.W);
                    CopyFromLast(ref curr, last);
                    curr.Data.Pos += offset;
                    missingChain.Add(curr.Data);
                    Bone[] bones = missingChain.ToArray();
                    if (thisOrThat && fabrik.SolveBoneChain(bones, target, referenceBone))
                    {
                        break;
                    }
                    IKSolver.SolveBoneChain(bones, target, referenceBone); // solve with IK
                    break;
                }
                CopyFromLast(ref curr, last);
                curr.Data.Pos += offset;
                missingChain.Add(curr.Data);
            }
            //ConstraintsBeforeReturn(first, missingChain.Count());
        }
        private void CopyFromLast(ref TreeNode<Bone> curr, Bone last)
        {
            curr.Data.Pos = new Vector3(last.Pos);
            curr.Data.Orientation = new Quaternion(new Vector3(last.Orientation.Xyz), last.Orientation.W);
        }

        private bool FixRotation(TreeNode<Bone> test, bool endWithEndEffector = false)
        {
            bool change = false;
            
            foreach (TreeNode<Bone> b in test)
            {
                if (b.IsRoot || b.Parent.IsRoot || b.Parent.Children.First() != b) continue;
                Vector3 ray1 = b.Parent.Data.GetYAxis();
                Vector3 ray2 = (b.Data.Pos - b.Parent.Data.Pos);
                bool parallel = Vector3Helper.Parallel(ray1, ray2, 0.01f);
                if (!parallel)
                {
                    ray2.NormalizeFast();
                    b.Parent.Data.RotateTowards(ray2);
                    change = true;
                }
                if (endWithEndEffector && b.IsLeaf) break;
            }
            return change;
        }
        private bool JerkingTest(TreeNode<Bone> fest, string message)
        {
            foreach (TreeNode<Bone> bvn in fest)
            {
                Bone a = bvn.Parent.Data;
                Bone b = lastSkel[a.Name];
                Quaternion QInitial = a.Orientation;
                Quaternion QFinal = b.Orientation;
                float test = QuaternionHelper.DiffrenceBetween(QInitial, QFinal);
                //Quaternion c = Quaternion.Slerp(qa, qb, 0.03f);
                //Vector3 newYax = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, c));
                //Vector3 oldYax = a.GetYAxis();
                if (test > 0.1f)
                {
                    UnityEngine.Debug.LogError(a.Name + test);
                    Quaternion y = Quaternion.Slerp(QInitial, QFinal, 0.01f);
                    Quaternion Trans = y * Quaternion.Conjugate(QInitial);
                    UnityDebug.DrawRays(Trans, a.Pos, 0.1f);
                    UnityDebug.DrawRays2(QInitial, a.Pos, 0.1f);

                    ForwardKinematics(bvn, Trans);
                    //UnityDebug.DrawRays(QFinal, Vector3.UnitX, 1f);
                    //UnityDebug.DrawRays(y, Vector3.UnitX*2, 1f);
                    //UnityDebug.DrawRays(Trans, Vector3.UnitX * 3, 1f);

                }
            }
            return false;
        }
        private bool ConstraintsBeforeReturn(TreeNode<Bone> bone, int depth)
        {
            int count = 0;
            bool anychange = FixRotation(bone, endWithEndEffector: true);
            
            foreach (var tnb in bone)
            {
                if (tnb.IsLeaf || count++ >= depth) break;
                if (tnb.Children.First().Data.HasNaN) break;
                if (tnb.Parent.IsRoot) continue;
                Quaternion rot;
                Vector3 res;
                Vector3 child = tnb.Children.First().Data.Pos;
                if (Constraint.CheckRotationalConstraints(tnb.Data, tnb.Parent.Data.Orientation, child, out res, out rot ))
                {
                    //UnityEngine.Debug.Log(tnb.Data);
                    return true;
                }
            }
            return anychange;
        }
        protected void ForwardKinematics(TreeNode<Bone> bones,  Quaternion rotation)
        {
            Vector3 oripos = bones.Parent.Data.Pos;
            foreach (var tnb in bones)
            {
                if (tnb.Data.HasNaN) break;
                tnb.Data.Pos = oripos + Vector3.Transform((tnb.Data.Pos - oripos), rotation);
                tnb.Parent.Data.RotateTowards(tnb.Data.Pos - tnb.Parent.Data.Pos);
                if (tnb.IsLeaf) break;
            }
        }
    }
}