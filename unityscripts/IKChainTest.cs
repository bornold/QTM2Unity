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
    public IKS IK = IKS.fabrik;
    public int conResolution = 50;
    public bool showconstraints = true;
    public float scale = 0.5f;
    public Vector4 constraints = new Vector4(20, 30, 20, 30);
    private Vector4 _constraints;
    public Vector2 twistConstraints = new Vector2(90, 90);
    private Vector2 _twistConstraints;
    public float jointTwistDiffrence = 20f;
    private float _jointTwistDiffrence;
    public bool twistBack = false;
    private List<Bone> bones = new List<Bone>();
    private IKSolver solver = new FABRIK();

    private Bone _target = new Bone("target");
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
        this.transform.localScale *= scale; 
	}
	
	// Update is called once per frame
	void Update () {
        if (chains != _chains || boneLength != _boneLength || bones.Count == 0 || _jointTwistDiffrence != jointTwistDiffrence)
        {
            bones = AddBones();
        }
        if (constraints != _constraints || twistConstraints != _twistConstraints)
        {
            newConstraints();
        }
        Vector3 last = bones[bones.Count-1].Pos.Convert();
        if ((last - this.gameObject.transform.position).magnitude > 0.01 )
        {
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
            Bone target = new Bone("target", this.gameObject.transform.position.Convert(), this.gameObject.transform.rotation.Convert());
            if (!_target.Equals(target))
            {
                _target = target;
                bones = solver.solveBoneChain(bones.ToArray(), target, OpenTK.Vector3.UnitY).ToList();
            }
        }
        else if (twistBack)
        {
            for (int i = 0; i < bones.Count-2; i++)
            {
               Bone b = bones[i+1];
               bones[i].EnsureConstraints(ref b, OpenTK.Vector3.UnitY, true);
            }
        }
        Debug.DrawLine(Vector3.zero, -Vector3.up * boneLength, UnityEngine.Color.white);
        Debug.DrawLine(Vector3.zero, Vector3.up * boneLength, UnityEngine.Color.black);

        Bone prev = bones[0];
        foreach (Bone curr in bones)
        {
            //Color c = Color.Lerp(Color.magenta, Color.cyan, d++ / (float)bones.Count);
            UnityDebug.DrawLine(prev.Pos, curr.Pos);
            UnityDebug.DrawRays(curr.Orientation, curr.Pos, boneLength);

            if (curr != bones[bones.Count -1])
            {
                UnityDebug.DrawLine(curr.Pos, curr.Pos + (curr.Pos - prev.Pos), UnityEngine.Color.black);
                if (showconstraints && curr.RotationalConstraint != null    )
                {
                    var rot = (curr == prev) ? OpenTK.Quaternion.Identity : prev.Orientation;
                    UnityDebug.CreateIrregularCone3(
                        curr.RotationalConstraint.Constraints,
                        curr.Pos,
                        rot,
                        conResolution,
                        boneLength * coneSize    
                        );
                }

            }
            prev = curr;
        }
	}

    private List<Bone> AddBones()
    {
        _chains = chains;
        _boneLength = boneLength;
        _constraints = constraints;
        _jointTwistDiffrence= jointTwistDiffrence;
        List<Bone> newBones = new List<Bone>();
        float twist = 0.0f;
        var qwist = OpenTK.Quaternion.Identity;
        for (int i = 0; i < chains; i++)
        {
                qwist = new OpenTK.Quaternion(OpenTK.Vector3.UnitY, OpenTK.MathHelper.DegreesToRadians(twist));
            qwist.Normalize();
            Bone b = new Bone("Bone " + i.ToString(), new OpenTK.Vector3(0f, (float)i * boneLength, 0f), qwist);
            b.SetRotationalConstraint(constraints.Convert());
            twist += jointTwistDiffrence;
            newBones.Add(b);
        }
        qwist = new OpenTK.Quaternion(OpenTK.Vector3.UnitY, OpenTK.MathHelper.DegreesToRadians(twist));
        qwist.Normalize();
        Bone c = new Bone("endeffector", new OpenTK.Vector3(0f, _chains * boneLength, 0f),qwist);
        c.SetRotationalConstraint(constraints.Convert());
        newBones.Add(c);

        return newBones;
    }
    private void newConstraints()
    {
        _constraints = constraints;
        foreach (Bone b in bones)
        {
            b.SetRotationalConstraint(constraints.Convert());
            b.setOrientationalConstraint(twistConstraints.x,twistConstraints.y);
        }
    }
    void OnDrawGizmos()
    {
       // Gizmos.DrawCube(bones[bones.Count-1].Pos.Convert(),Vector3.one/4f);
    }
}
