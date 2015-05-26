using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QTM2Unity;
using System.Linq;
public class KneeErrorTEst : MonoBehaviour
{
    public IKS IK = IKS.fabrik;
    public float coneScale = 0.5f;
    public bool showConstraints = true;
    public bool showOrientation = true;

    public bool reset = false;
    public int conResolution = 50;
    public float targetScale = 0.5f;

    public Vector4 femurRotConst = new Vector4(20, 85, 50, 30);
    public Vector2 femurTwistConst = new Vector2(315, 45);
    public Vector4 kneeRotConst = new Vector4(10, 10, 10, 175);
    public Vector2 kneeTwistConst = new Vector2(359, 1);
    public Vector4 ankleRotConst = new Vector4(20, 85, 20, 50);
    public Vector2 ankleTwistConst = new Vector2(315, 45);

    private BipedSkeleton bones;
    private BipedSkeleton bones2;

    private IKSolver solver;
    private IKApplier applier;
    private Vector3 _pos;
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
        applier = new IKApplier(solver);
        bones = GetBones(this.gameObject.transform.position.Convert());
        _pos = this.gameObject.transform.position;
	}


    private BipedSkeleton GetBones(OpenTK.Vector3 femurPos)
    {

        OpenTK.Vector3 anklePos = new OpenTK.Vector3(0f, 0.1f, 0.1f);
        OpenTK.Quaternion ankleOri = QuaternionHelper.LookAtRight(anklePos, OpenTK.Vector3.Zero, OpenTK.Vector3.UnitX);
        Bone ankle = new Bone(BipedSkeleton.FOOT_L, anklePos, ankleOri);

        OpenTK.Vector3 kneePos = new OpenTK.Vector3(0f, 1f, 0.2f);
        OpenTK.Quaternion kneeOri = QuaternionHelper.LookAtRight(kneePos, anklePos, OpenTK.Vector3.UnitX);
        Bone knee = new Bone(BipedSkeleton.LOWERLEG_L, kneePos, kneeOri);

        OpenTK.Quaternion femurOri = QuaternionHelper.LookAtRight(femurPos, kneePos, -OpenTK.Vector3.UnitX);
        Bone femur = new Bone(BipedSkeleton.UPPERLEG_L, femurPos, femurOri);

        OpenTK.Vector3 hipPos = new OpenTK.Vector3(0, 2f, 1f);

        Bone hip = new Bone(BipedSkeleton.PELVIS,
            hipPos,
            QuaternionHelper.RotationY(OpenTK.MathHelper.Pi));
        //QuaternionHelper.LookAtRight(OpenTK.Vector3.UnitY, OpenTK.Vector3.Zero, OpenTK.Vector3.UnitX));

        Bone target = new Bone(BipedSkeleton.TOE_L, OpenTK.Vector3.Zero, OpenTK.Quaternion.Identity);
        TreeNode<Bone> root = new TreeNode<Bone>(hip);
        {
            TreeNode<Bone> upperlegleft = root.AddChild(femur);
            {
                TreeNode<Bone> lowerlegleft = upperlegleft.AddChild(knee);
                {
                    TreeNode<Bone> footleft = lowerlegleft.AddChild(ankle);
                    {
                        footleft.AddChild(target);
                    }
                }
            }
        }
        return new BipedSkeleton(root);
    }
	// Update is called once per frame
	void Update () {
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

        if (bones == null || reset)
        {
            bones = GetBones(this.gameObject.transform.position.Convert());
            reset = false;
        }

        SetConstraints(ref bones);

        this.transform.localScale = Vector3.one * targetScale;
        Vector3 pos = this.gameObject.transform.position;

        if (!pos.Equals(_pos))
        {
            bones2 = GetBones(_pos.Convert());
            bones2[BipedSkeleton.LOWERLEG_L].Pos = new OpenTK.Vector3(float.NaN, float.NaN, float.NaN);
            bones2[BipedSkeleton.LOWERLEG_L].Orientation = new OpenTK.Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
            bones2[BipedSkeleton.FOOT_L].Pos = new OpenTK.Vector3(float.NaN, float.NaN, float.NaN);
            bones2[BipedSkeleton.FOOT_L].Orientation = new OpenTK.Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);

            bones2[BipedSkeleton.UPPERLEG_L].Orientation = new OpenTK.Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
            bones2[BipedSkeleton.UPPERLEG_L].Pos = pos.Convert();
            _pos = pos;
            SetConstraints(ref bones2);
            bones = bones2;
        }
        //Debug.Log("______Before______");
        //foreach (TreeNode<Bone> b in bones) Debug.Log(b.Data.ToString());
        applier.IKSolver = solver;
        applier.ApplyIK(ref bones);
        //Debug.Log("______After______");
        //foreach (TreeNode<Bone> b in bones) Debug.Log(b.Data.ToString());

        IEnumerator it = bones.GetEnumerator();
        it.MoveNext();
        Bone prev = ((TreeNode<Bone>)it.Current).Data;
        if (showOrientation) UnityDebug.DrawRays(prev.Orientation * QuaternionHelper.RotationZ(OpenTK.MathHelper.Pi), prev.Pos, 0.5f);
        while (it.MoveNext())
        {
            Bone curr = ((TreeNode<Bone>)it.Current).Data;
            UnityDebug.DrawLine(prev.Pos, curr.Pos);
            if (!curr.Name.Equals(BipedSkeleton.TOE_L))
            {
                if (showOrientation) UnityDebug.DrawRays(curr.Orientation, curr.Pos, 0.5f);
                if (showConstraints)
                {
                    var L1 = (prev.Name.Equals(BipedSkeleton.PELVIS)) ? -prev.GetYAxis() : prev.GetYAxis();
                    UnityDebug.DrawLine(curr.Pos, curr.Pos + L1, UnityEngine.Color.black);
                    UnityDebug.CreateIrregularCone3(
                        curr.Constraints,
                        curr.Pos,
                        L1,
                        prev.Orientation,
                        conResolution,
                        coneScale    
                    );
                }
            }
            prev = curr;
        }


	}

    private void SetConstraints(ref BipedSkeleton bones)
    {
        Bone ankle = bones[BipedSkeleton.FOOT_L];
        ankle.SetOrientationalConstraints(ankleTwistConst.Convert());
        ankle.SetRotationalConstraints(ankleRotConst.Convert());

        Bone knee = bones[BipedSkeleton.LOWERLEG_L];
        knee.SetOrientationalConstraints(kneeTwistConst.Convert());
        knee.SetRotationalConstraints(kneeRotConst.Convert());

        Bone femur = bones[BipedSkeleton.UPPERLEG_L];
        femur.SetRotationalConstraints(femurRotConst.Convert());
        femur.SetOrientationalConstraints(femurTwistConst.Convert());
    }
    void OnDrawGizmos()
    {
        if (bones != null)
            foreach (TreeNode<Bone> tn in bones)
            {
                Bone b = tn.Data;
                Gizmos.DrawSphere(b.Pos.Convert(), 0.01f);
            }
        Gizmos.DrawSphere(Vector3.zero, 0.02f);
    }
}
