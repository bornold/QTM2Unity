using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using QTM2Unity.Unity;
using System.Collections;
namespace QTM2Unity
{
    class IKTest : RT {
        private JointLocalization joints;
        private BipedSkeleton skeleton;
        private Vector3 thisPos;
        public bool showRotationTrace;
        public float markerScale;
        Skel s;
        // Use this for initialization
        public override void StartNext()
        {
            joints = new JointLocalization();
        }

        // Update is called once per frame
        public override void UpdateNext()
        {
            thisPos = this.transform.position;
            if (joints == null) joints = new JointLocalization();
            BipedSkeleton lastSkel = skeleton;
            skeleton = joints.GetJointLocation(markerData);
            s = new Skel();
            Skel ls = new Skel();
            foreach (Bone b in skeleton.Bones)
            {
                s[b.Name] = new Bon(b.Name, b.Pos, b.Orientation);
            }
            foreach (Bone b in lastSkel.Bones)
            {
                ls[b.Name] = new Bon(b.Name, b.Pos, b.Orientation);
            }

            //IEnumerator i = s.GetEn
            IEnumerator it = s.GetEnumerator();
            IEnumerator itLast = ls.GetEnumerator();
            while (it.MoveNext() && itLast.MoveNext())
            {
                TreeNode<Bon> b = (TreeNode<Bon>)it.Current;
                TreeNode<Bon> last = (TreeNode<Bon>)itLast.Current;
                if (b.Data.Pos.IsNaN())
                {
                    OpenTK.Vector3 target = OpenTK.Vector3.One ;
                    bool set = false;
                    Bon parent = b.Parent.Data;
                    Bon root = last.Parent.Data; // förra framens parent är root i lösningen
                    OpenTK.Vector3 offset = root.Pos - parent.Pos; // offset för att föra förra kedjan mot root

                    root.Pos += offset; // rör hela kedjan till nuvarande root position
                    last.Data.Pos += offset; // samtliga måste flyttas för att kedjan rotationer skall stämma
                    List<Bon> chain = new List<Bon>() { root, last.Data }; // de som skall lösas
                    while (it.MoveNext() && itLast.MoveNext() && !b.IsLeaf) // endeffector måste markeras
                    {
                        b = (TreeNode<Bon>)it.Current;
                        last = (TreeNode<Bon>)itLast.Current;
                        last.Data.Pos += offset; // denna joint 
                        chain.Add(last.Data);
                        if (!b.Data.Pos.IsNaN())
                        {
                            target = b.Data.Pos;
                            set = true;
                            break;
                        }
                    }
                    Bone[] solvedBones;
                    if (set)
		            {
                        Debug.Log(string.Format("Target accuired"));
                        List<Bone> test = new List<Bone>();
                        foreach (Bon vv in chain)
                        {
                            test.Add(new Bone(vv.Name,vv.Pos,vv.Rot));
                        }
			            solvedBones = CCD.solveBoneChain(test.ToArray(), target); // solve with IK
                        set = false;
                    }
                    else
                    {
                        Debug.Log(string.Format("Could not find target for IK"));
                        List<Bone> haha = new List<Bone>();
                        foreach (Bon ha in chain) {
                            haha.Add(new Bone(ha.Name,ha.Pos,ha.Rot));
                        }
                        solvedBones = haha.ToArray();

                    }
                    foreach (Bone a in solvedBones)
                    {
                        s[a.Name] = new Bon(a.Name, a.Pos, a.Orientation); // Add all in solved except first and last element in skeleton
                    }
                }
            }
        }
        void OnDrawGizmos()
        {
            if (s != null)
            {
                foreach (TreeNode<Bon> b in s)
                {
                    Vector3 v = cv(b.Data.Pos) + thisPos;
                
                    Gizmos.DrawSphere(v, markerScale);
                    if (showRotationTrace )
                        drawRays(b.Data.Rot,v);
                    if (!b.IsLeaf)
                    {
                        foreach (TreeNode<Bon> b2 in b.Children)
                        {
                            drawLine(b.Data.Pos, b2.Data.Pos);
                        }
                    }
                }
            }
        }
    }
}


