using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTM2Unity
{
    class IKApplier
    {
        Dictionary<string, Bone> lastSkelDic;
        public BipedSkeleton ApplyIK(BipedSkeleton skeleton)
        {
            if (lastSkelDic == null) lastSkelDic = skeleton.ToDictionary(k => k.Data.Name, v => v.Data);
            IEnumerator it = skeleton.GetEnumerator();
            //TODO ATTENTION! 
            //This has oprediction that Root and all of roots children MUST have set possition
            while (it.MoveNext())
            {
                TreeNode<Bone> b = (TreeNode<Bone>)it.Current;
                if (!b.Data.Exists) // Possition of joint no knowned, Solve with IK
                {
                    TreeNode<Bone> parent = b.Parent; // save parent
                    Bone root = lastSkelDic[parent.Data.Name]; // last frames' parent is root in solution
                    Bone missing = lastSkelDic[b.Data.Name]; // this node that is missing
                    OpenTK.Vector3 offset = parent.Data.Pos - root.Pos; // offset to move last frames chain to this frames' position
                    root.Pos += offset; // move chain to this place
                    missing.Pos += offset;
                    List<Bone> missingChain = new List<Bone>() { root, missing }; // chain to be solved
                    while (!b.IsLeaf && it.MoveNext())
                    {
                        b = (TreeNode<Bone>)it.Current;
                        missing = lastSkelDic[b.Data.Name];
                        missing.Pos += offset;
                        missingChain.Add(missing);
                        if (b.Data.Exists)
                        {
                            OpenTK.Vector3 target = b.Data.Pos;
                            missingChain = CCD.solveBoneChain(missingChain.ToArray(), target).ToList(); // solve with IK
                            break;
                        }
                    }
                    foreach (Bone a in missingChain) skeleton[a.Name] = a;
                    //TODO change this so we add a chain instead of changing value
                   /*
                    TreeNode<Bone> solvedChain = null;
                    TreeNode<Bone> last = null;
                    TreeNode<Bone> grandpa = parent.Parent;
                    grandpa.RemoveChild(parent);
                    foreach (Bone a in missingChain)
                    {
                        if (solvedChain == null || last == null)
                        {
                            last = solvedChain = new TreeNode<Bone>(a);
                            continue;
                        }
                        last = last.AddChild(a);
                    }
                    // Add child to last of it existslast.AddChild
                    */
                }
            }
            
            lastSkelDic = skeleton.ToDictionary(k => k.Data.Name, v => v.Data);
            return skeleton;
        }
    }
}
