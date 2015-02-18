using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QTM2Unity
{
    class IKMover : StandardUnityModel
    {
        public override void UpdateNext()
        {
            base.UpdateNext();
            Dictionary<string, Bone> lastSkelDic = skeleton.ToDictionary(k => k.Data.Name, v => v.Data);
            joints.GetJointLocation(ref skeleton, markerData);
            OpenTK.Vector3 before = skeleton[BipedSkeleton.LOWERARM_L].Pos;

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
                }
            }
            SetAll();
        }
    }
}
