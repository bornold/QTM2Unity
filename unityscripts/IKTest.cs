using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
namespace QTM2Unity
{
    class IKTest : RT {
        private JointLocalization joints;
        private BipedSkeleton skeleton;
        private Vector3 thisPos;
        public bool debug;
        public bool showRotationTrace;
        public float markerScale;
        // Use this for initialization
        public override void StartNext()
        {

            joints = new JointLocalization();
            skeleton = new BipedSkeleton();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            thisPos = this.transform.position;
            if (joints == null) joints = new JointLocalization();
            if (skeleton == null) skeleton = new BipedSkeleton();
            
            Dictionary<string, Bone> lastSkelDic =
                skeleton.ToDictionary(k => k.Data.Name,v => v.Data);
            joints.GetJointLocation(ref skeleton, markerData);
                
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
                    
                    /*
                    //TODO change this so we add a chain instead of changing value
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
        }
        void OnDrawGizmos()
        {
            if (skeleton != null)
            {
                foreach (TreeNode<Bone> b in skeleton)
                {
                    Vector3 v = cv(b.Data.Pos) + thisPos;
                
                    Gizmos.DrawSphere(v, markerScale);
                    if (showRotationTrace )
                        drawRays(b.Data.Orientation, cv(b.Data.Pos));
                    if (!b.IsLeaf)
                    {
                        foreach (TreeNode<Bone> b2 in b.Children)
                        {
                            drawLine(b.Data.Pos, b2.Data.Pos);
                        }
                    }
                }
            }
        }
    }
}


