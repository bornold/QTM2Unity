using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
namespace QTM2Unity
{
    class IKApplier
    {
        public IKApplier(IKSolver IKSolver) { this.IKSolver = IKSolver; }
        private BipedSkeleton lastSkel;
        public IKSolver IKSolver { private get; set; } 
        private IKSolver fabrik = new FABRIK();
        public bool thisOrThat = false;
        public void ApplyIK(ref BipedSkeleton skeleton)
        {
            if (lastSkel == null) lastSkel = skeleton;
            IEnumerator skelEnumer = skeleton.GetEnumerator();
            IEnumerator lastSkelEnumer = lastSkel.GetEnumerator();
            //Root and all of roots children MUST have set possition
            TreeNode<Bone> bone;
            if (lastSkel.Any(z => z.Data.Pos.IsNaN()))
            {
                UnityEngine.Debug.LogError(lastSkel.First(c => c.Data.Pos.IsNaN()).Data);
            }
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
            fix(skeleton.First());
            if (skeleton.Any(z => z.Data.Pos.IsNaN() || z.Data.Orientation.Xyz.IsNaN()))
            {
                UnityEngine.Debug.LogError(skeleton.First(c => c.Data.Pos.IsNaN() || c.Data.Orientation.Xyz.IsNaN()).Data);
            }
            UnityDebug.sanity(skeleton);
            lastSkel = skeleton;
        }

        private void MissingJoint(Bone referenceBone, ref IEnumerator skelEnum, ref IEnumerator lastSkelEnum)
        {
            List<Bone> missingChain = new List<Bone>(); // chain to be solved

            //root of chain 
            // missings joints parent from last frame is root in solution
            TreeNode<Bone> curr = ((TreeNode<Bone>)skelEnum.Current).Parent;
            TreeNode<Bone> first = curr;
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
                TreeNode<Bone> fga = (TreeNode<Bone>)lastSkelEnum.Current;
                if (curr.Data.Exists) // target found! it the last in list
                {
                    if (curr.Data.Orientation.Xyz.IsNaN())
                    {
                        
                    }
                    target = new Bone(
                        curr.Data.Name,
                        new Vector3(curr.Data.Pos),
                        new Quaternion(new Vector3(curr.Data.Orientation.Xyz), curr.Data.Orientation.W),
                        new Vector4(curr.Data.Constraints)
                        );
                    target.TwistLimit = curr.Data.TwistLimit;

                    CopyFromLast(ref curr, last);
                    curr.Data.Pos += offset;

                    missingChain.Add(curr.Data);

                    TestNewThing(missingChain.ToArray(), target, referenceBone, first); // solve with IK)

                    break;
                }

                CopyFromLast(ref curr, last);
                curr.Data.Pos += offset;
                missingChain.Add(curr.Data);
            }
            if (missingChain.Any(z => z.Pos.IsNaN()))
            {
                UnityEngine.Debug.LogError(missingChain.First(c => c.Pos.IsNaN()));
            }
            ConstraintsBeforeReturn(first, missingChain.Count());
            if (missingChain.Any(z => z.Pos.IsNaN()))
            {
                UnityEngine.Debug.LogError(missingChain.First(c => c.Pos.IsNaN()));
            }
        }
        private void CopyFromLast(ref TreeNode<Bone> curr, Bone last)
        {
            curr.Data.Pos = new Vector3(last.Pos);
            curr.Data.Orientation = new Quaternion(new Vector3(last.Orientation.Xyz), last.Orientation.W);
        }

        private void TestNewThing(Bone[] bones, Bone target, Bone referenceBone, TreeNode<Bone> first)
        {
            if (!thisOrThat)
            {

               bones = IKSolver.SolveBoneChain(bones, target, referenceBone); // solve with IK

               fix(first);
               if (bones.Any(z => z.Pos.IsNaN()))
               {
                   UnityEngine.Debug.LogError(bones.First(c => c.Pos.IsNaN()));
               }
               UnityDebug.sanity(bones, "VAFAN");
               UnityDebug.sanity(first, "after ccd target: " + target.Name);
            }

            else
            {
                fabrik.SolveBoneChain(bones, target, referenceBone); //first solve with fabrik

                UnityDebug.sanity(first, "fab + " + target.Name);
                if (ConstraintsBeforeReturn(first, bones.Count())) // then check if solution is valid
                {

                    //sanity(first,"tar");

                    //JerkingTest(first, "first tots const");
                    //UnityEngine.Debug.Log("Using Fabrik, and it gave an invalid solution, solving with " + IKSolver.ToString());
                    IKSolver.SolveBoneChain(bones, target, referenceBone); // //if not, solve with CCD 

                }

                if (ConstraintsBeforeReturn(first, bones.Count()))
                {
                }

                UnityDebug.sanity(first, "sist" + target.Name);
                //JerkingTest(first, "JERKIN'!"); // test for jerking movement
            }
        }



        private void fix(TreeNode<Bone> test)
        {
            foreach (TreeNode<Bone> b in test)
            {
                if (b.IsRoot || b.Parent.IsRoot || !b.Data.Exists || !b.Parent.Data.Exists || b.Data.ParentPointer != Quaternion.Identity) continue;
                Vector3 ray1 = b.Parent.Data.GetYAxis();
                Vector3 ray2 = (b.Data.Pos - b.Parent.Data.Pos);
                bool parallel = Vector3Helper.Parallel(ray1, ray2);
                if (!parallel)
                {
                    b.Parent.Data.RotateTowards(ray2);
                }
            }
        }
        private bool JerkingTest(TreeNode<Bone> fest, string message)
        {
            foreach (TreeNode<Bone> bvn in fest)
            {
                Bone a = bvn.Parent.Data;
                Bone b = lastSkel[a.Name];
                Quaternion qa = a.Orientation;
                Quaternion qb = b.Orientation;
                float test = QuaternionHelper.DiffrenceBetween(qa, qb);
                Quaternion c = Quaternion.Slerp(qa, qb, 0.03f);
                Vector3 newYax = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, c));
                Vector3 oldYax = a.GetYAxis();
                if (test > 0.03f)
                {
                    UnityDebug.DrawRay(a.Pos, oldYax, UnityEngine.Color.blue, 2f);
                    Quaternion rot =  QuaternionHelper.GetRotationBetween(oldYax, newYax);
                    UnityDebug.DrawRays2(a.Orientation, a.Pos, 1f);
                    //UnityDebug.DrawRay(a.Pos, newYax, UnityEngine.Color.red, 2f);
                    //UnityDebug.DrawRays2(rot,a.Pos,0.5f);
                    //ForwardKinematics(bvn.Parent, rot);
                    UnityEngine.Debug.Log(a);
                }
                if (bvn.IsLeaf) break;
            }
            return false;
        }
        private bool ConstraintsBeforeReturn(TreeNode<Bone> bone, int depth)
        {
            int count = 0;
            bool anychange = false;
            foreach (var tnb in bone)
            {
                if (tnb.IsLeaf || count++ >= depth || tnb.Data.Orientation.Xyz.IsNaN()) break;
                Quaternion rot;
                if (Constraint.CheckOrientationalConstraint(tnb.Data, tnb.Parent.Data, out rot))
                {
                    anychange = true;
                    tnb.Data.Rotate(rot);
                }
                Vector3 res;
                if (tnb.Children.First().Data.Pos.IsNaN()) break;
                if (Constraint.CheckRotationalConstraints(tnb.Data, tnb.Parent.Data.Orientation, tnb.Children.First().Data.Pos, out res, out rot ))
                {
                    anychange = true;
                    ForwardKinematics(tnb, rot);
                }
                if (Constraint.CheckOrientationalConstraint(tnb.Data, tnb.Parent.Data, out rot))
                {
                    anychange = true;
                    tnb.Data.Rotate(rot);
                }
            }
            return anychange;
        }
        protected void ForwardKinematics(TreeNode<Bone> bones,  Quaternion rotation)
        {
            Vector3 oripos = bones.Parent.Data.Pos;
            foreach (var tnb in bones)
            {
                tnb.Data.Pos = oripos + Vector3.Transform((tnb.Data.Pos - oripos), rotation);
                tnb.Parent.Data.RotateTowards(tnb.Data.Pos - tnb.Parent.Data.Pos);
                if (tnb.IsLeaf) break;
            }
        }
    }
}