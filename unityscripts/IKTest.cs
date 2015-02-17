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
            BipedSkeleton lastSkel = skeleton;
            joints.GetJointLocation(ref skeleton, markerData);
            if (debug) Debug.Log(object.ReferenceEquals(skeleton,lastSkel));

            //IEnumerator i = s.GetEn
            IEnumerator it = skeleton.GetEnumerator();
            IEnumerator itLast = lastSkel.GetEnumerator();
            while (it.MoveNext() && itLast.MoveNext())
            {
                TreeNode<Bone> b = (TreeNode<Bone>)it.Current;
                TreeNode<Bone> last = (TreeNode<Bone>)itLast.Current;
                if (b.Data.Pos.IsNaN())
                {
                    Bone parent = b.Parent.Data;
                    Bone root = last.Parent.Data; // förra framens parent är root i lösningen
                    OpenTK.Vector3 offset = root.Pos - parent.Pos; // offset för att föra förra kedjan mot root

                    root.Pos += offset; // rör hela kedjan till nuvarande root position
                    last.Data.Pos += offset; // samtliga måste flyttas för att kedjan rotationer skall stämma
                    List<Bone> chain = new List<Bone>() { root, last.Data }; // de som skall lösas
                    while (it.MoveNext() && itLast.MoveNext() && !b.IsLeaf)
                    {
                        b = (TreeNode<Bone>)it.Current;
                        last = (TreeNode<Bone>)itLast.Current;
                        last.Data.Pos += offset; // denna joint 
                        chain.Add(last.Data);
                        if (b.Data.Exists)
                        {
                            OpenTK.Vector3 target = b.Data.Pos;
                            chain = CCD.solveBoneChain(chain.ToArray(), target).ToList(); // solve with IK
                            break;
                        }
                    }
                    foreach (Bone a in chain)
                    {
                        if (debug && a.Name == BipedSkeleton.LOWERARM_L) Debug.Log(a.Name + a.Pos);
                        skeleton[a.Name] = new Bone(a.Name, a.Pos, a.Orientation); // Add all in solved except first and last element in skeleton
                    }
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


