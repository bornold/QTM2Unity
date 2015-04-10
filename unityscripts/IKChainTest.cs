using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QTM2Unity;
using System.Linq;
public class IKChainTest : MonoBehaviour {

    public int chains = 10;
    private int _chains;
    public float boneLength = 0.5f;
    private float _boneLength;
    public IKS IK = IKS.fabrik;
    public float coneScale = 0.5f;
    public bool showConstraints = true;
    public bool showOrientation = true;

    public int conResolution = 50;
    public float targetScale = 0.5f;
    public Vector4 constraints = new Vector4(20, 30, 20, 30);
    private Vector4 _constraints;
    public Vector2 twistConstraints = new Vector2(90, 90);
    private Vector2 _twistConstraints;
    public float jointTwistDiffrence = 20f;
    private float _jointTwistDiffrence;
    private List<Bone> bones = new List<Bone>();
    private IKSolver solver = new FABRIK();

    private Bone _target = new Bone("target");
    public enum IKS
	    {
	        target,
            ccd,
            fabrik,
            dsl,
            transpose
	    }

	// Use this for initialization
	void Start () {
        solver = new CCD();
        bones = AddBones();
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.localScale = Vector3.one * targetScale; 
        if (chains != _chains || boneLength != _boneLength || bones.Count == 0 || _jointTwistDiffrence != jointTwistDiffrence)
        {
            bones = AddBones();
        }
        if (constraints != _constraints || twistConstraints != _twistConstraints)
        {
            newConstraints();
        }
        Bone grandpa = new Bone("grandpa", new OpenTK.Vector3(0, -1, 0), QuaternionHelper.RotationY(0));

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
                case IKS.dsl:
                    solver = new DampedLeastSquares();
                    break;
                case IKS.transpose:
                    solver = new JacobianTranspose();
                    break;
                default:
                    break;
            }

            Bone target = new Bone("target", this.gameObject.transform.position.Convert(), this.gameObject.transform.rotation.Convert());
            if (!_target.Equals(target))
            {
                _target = target;
                bones = solver.SolveBoneChain(bones.ToArray(), target, grandpa).ToList();
            }
        }

        UnityDebug.DrawRays(grandpa.Orientation, grandpa.Pos, boneLength);

        Bone prev = bones[0];
        foreach (Bone curr in bones)
        {
            UnityDebug.DrawLine(prev.Pos, curr.Pos);

            if (curr != bones[bones.Count -1])
            {
                if (showOrientation) UnityDebug.DrawRays(curr.Orientation, curr.Pos, boneLength);
                if (showConstraints)
                {
                    OpenTK.Quaternion rot = (prev == curr) ? OpenTK.Quaternion.Identity : curr.Orientation;
                    OpenTK.Quaternion rotaten = (curr == prev) ? grandpa.Orientation : prev.Orientation;
                    var L1 = (prev == curr) ? OpenTK.Vector3.UnitY : (curr.Pos - prev.Pos);
                    UnityDebug.DrawLine(curr.Pos, curr.Pos + L1, UnityEngine.Color.black);
                    UnityDebug.CreateIrregularCone3(
                        curr.Constraints,
                        curr.Pos,
                        L1,
                        rotaten,
                        conResolution,
                        boneLength * coneScale    
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
        float twist = jointTwistDiffrence;
        var qwist = OpenTK.Quaternion.Identity;
        for (int i = 0; i < chains; i++)
        {
            qwist = QuaternionHelper.RotationY(OpenTK.MathHelper.DegreesToRadians(twist));
            Bone b = new Bone("Bone " + i.ToString(), new OpenTK.Vector3(0f, (float)i * boneLength, 0f), qwist);
            b.SetRotationalConstraints(constraints.Convert());
            b.SetOrientationalConstraints(twistConstraints.x, twistConstraints.y);

            twist += jointTwistDiffrence;
            newBones.Add(b);
        }
        qwist = new OpenTK.Quaternion(OpenTK.Vector3.UnitY, OpenTK.MathHelper.DegreesToRadians(twist));
        qwist.Normalize();
        Bone c = new Bone("endeffector", new OpenTK.Vector3(0f, _chains * boneLength, 0f),qwist);
        c.SetRotationalConstraints(constraints.Convert());
        c.SetOrientationalConstraints(twistConstraints.x, twistConstraints.y);

        newBones.Add(c);

        return newBones;
    }
    private void newConstraints()
    {
        _constraints = constraints;
        _twistConstraints = twistConstraints;
        Debug.Log("constraints updated");
        foreach (Bone b in bones)
        {
            b.SetRotationalConstraints(constraints.Convert());
            b.SetOrientationalConstraints(twistConstraints.x, twistConstraints.y);
        }
    }
}
