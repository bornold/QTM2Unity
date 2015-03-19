using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QTM2Unity;
using System.Linq;
public class IKChainTest : MonoBehaviour {

    public int chains = 10;
    private int _chains;
    public float boneLength = 0.5f;
    public float coneSize = 0.2f;
    private float _boneLength;
    public IKS IK = IKS.ccd;
    public int conResolution = 50;
    public bool showconstraints = true;
    public Vector4 constraints = new Vector4(20, 30, 20, 30);
    public Vector4 _constraints;
    private List<Bone> bones = new List<Bone>();
    private IKSolver solver = new FABRIK();
    public enum IKS
	    {
	        target,
            ccd,
            fabrik
	    }

	// Use this for initialization
	void Start () {
        solver = new CCD();
        bones = AddBones();
	}
	
	// Update is called once per frame
	void Update () {
        if (chains != _chains || boneLength != _boneLength || bones.Count == 0)
        {
            bones = AddBones();
        }
        if (constraints != _constraints)
        {
            newConstraints();
        }
        Vector3 last = bones[bones.Count-1].Pos.Convert();
        if ((last - this.gameObject.transform.position).magnitude > 0.01 )
        {
//            Debug.Log("SOLVING!");
            switch (IK)
            {
                case IKS.target:
                    solver = new TargetTriangleIK();
                    break;
                case IKS.ccd:
                    solver = new CCD();
                    break;
                case IKS.fabrik:
                    solver = new FABRIK();
                    break;
                default:
                    break;
            }
            Bone target = new Bone("target", this.gameObject.transform.position.Convert());
            bones = solver.solveBoneChain(bones.ToArray(), target,OpenTK.Vector3.UnitY).ToList();
        }
        float d = 0;
        foreach (Bone curr in bones)
        {
            Color c = Color.Lerp(Color.magenta, Color.cyan, d++ / (float)bones.Count);
            UnityDebug.DrawRays(curr.Orientation, curr.Pos, boneLength);
            if (showconstraints && curr.RotationalConstraint != null)
            {
                UnityDebug.CreateIrregularCone3(
                    curr.RotationalConstraint.Constraints,
                    curr.Pos,
                    curr.Orientation,
                    conResolution,
                    coneSize    
                    );
            } 
        } 
	}

    private List<Bone> AddBones()
    {
        _chains = chains;
        _boneLength = boneLength;
        _constraints = constraints;
        List<Bone> newBones = new List<Bone>();
        for (int i = 0; i < _chains; i++)
        {
            Bone b = new Bone("Bone " + i.ToString(), new OpenTK.Vector3(0f, (float)i * boneLength, 0f), OpenTK.Quaternion.Identity);
            b.SetRotationalConstraint(constraints.Convert());
            newBones.Add(b);
        }
        Bone c = new Bone("endeffector", new OpenTK.Vector3(0f, _chains * boneLength, 0f));
        newBones.Add(c);
        return newBones;
    }
    private void newConstraints()
    {
        _constraints = constraints;
        foreach (Bone b in bones)
        {
            b.SetRotationalConstraint(constraints.Convert());
        }
    }
}
