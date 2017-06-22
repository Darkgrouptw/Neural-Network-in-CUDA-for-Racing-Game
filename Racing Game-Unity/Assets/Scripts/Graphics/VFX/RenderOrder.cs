using UnityEngine;
using System.Collections;

public class RenderOrder : MonoBehaviour
{

	public int order = 0;

	// Use this for initialization
	void Start ()
	{
		GetComponent<Renderer>().sortingOrder = order;
		if(Application.isEditor)
		{
			enabled = false;
		}
		else
		{
			Destroy(this);
		}
	}
	
	void OnValidate()
	{
		GetComponent<Renderer>().sortingOrder = order;
	}
}
