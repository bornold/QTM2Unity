using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class qrtFABRIK : MonoBehaviour
{
	public Transform[] chain;
	public Transform target;
	public Transform root;

	private float totChainDistance;
	private float[] jointDistances;
	private Vector3[] chainPositions;
	private Transform endOfchain;

	// Use this for initialization
	void Start ()
	{
		if(chain.Length > 0)
		{
			totChainDistance = 0;
			endOfchain = chain[chain.Length-1];
			jointDistances = new float[chain.Length-1];
			chainPositions = new Vector3[chain.Length];

            //calculate initial distances of chain
			for(int i = 0; i < chain.Length-1; i++)
			{
				float distance = Vector3.Distance(chain[i].position,chain[i+1].position);
				jointDistances[i] = distance;
				totChainDistance += distance;
				chainPositions[i] = chain[i].position;
			}

			chainPositions[chain.Length-1] = chain[chain.Length-1].position;

		}
	}

    /// <summary>
    /// First stage of algorithm
    /// </summary>
	private void stageOne()
	{
		for(int i = chain.Length-2; i > 0; i--)
		{
			float r = Vector3.Distance(chainPositions[i], chainPositions[i+1]);
			float delta = jointDistances[i]/r;

			chainPositions[i] = (1-delta)*chainPositions[i+1] + delta*chainPositions[i];
		}
	}

    /// <summary>
    /// Second stage of algorithm
    /// </summary>
	private void stageTwo()
	{
		for(int i = 0;  i < chain.Length-2; i++)
		{
			float r = Vector3.Distance(chainPositions[i], chainPositions[i+1]);
			float delta = jointDistances[i]/r;
			chainPositions[i+1] = (1-delta)*chainPositions[i] + delta*chainPositions[i+1];
		}
	}

	// Update is called once per frame
	void Update ()
	{
		float rootToTargetDistance = Vector3.Distance(root.position, target.position);

		if(totChainDistance < rootToTargetDistance)
		{
			//Unreachable target
			chain[0].position = root.position;
			chainPositions[0] = root.position;

			for(int i = 0;  i < chain.Length-1; i++)
			{
				Vector3 pPos = chain[i].position;
				float r = Vector3.Distance(target.position,pPos);

				float delta = jointDistances[i]/r;

				chainPositions[i+1] = (1.0f-delta)*chainPositions[i]+ delta*target.position;

				Debug.DrawLine(chainPositions[i] ,chainPositions[i+1],Color.red);

				chain[i].transform.position = chainPositions[i];
			}
		}
		else
		{
			//Reachable target
			chain[0].position = root.position;

			float diffA = Vector3.Distance(endOfchain.position,target.position);

			float tolorance = 0.01f;
			while(diffA > tolorance)
			{
				endOfchain.position = target.position;
				stageOne();

				chain[0].position = root.position;
				stageTwo();
				diffA = Vector3.Distance(target.position,endOfchain.position);
			}

			for(int i = 0;  i < chain.Length-1; i++)
			{
				Debug.DrawLine(chain[i].position ,chain[i+1].position,Color.blue);
			}

		}
	}
}