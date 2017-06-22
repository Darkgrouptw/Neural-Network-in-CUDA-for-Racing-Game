using UnityEngine;
using System.Collections;

public class UvOffsetByTime : MonoBehaviour {

	public enum AnimTextureType{
		DIFFUSE,
		BUMP,
	}

	public AnimTextureType type = AnimTextureType.DIFFUSE;
	public Vector2 speed = new Vector2(1f, 1f);
	Vector2 curOffset = new Vector2(0,0);
	Material mat = null;
	string texchanel = "_BumpMap";

	// Use this for initialization
	void Start () {
		MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
		mat = mr.sharedMaterial;
		switch (type)
		{
		case AnimTextureType.DIFFUSE:
			texchanel = "_MainTex";
			break;
		case AnimTextureType.BUMP:
			texchanel = "_BumpMap";
			break;
		default:
			break;
		}
	}
	
	// Update is called once per frame
	void Update () {
		curOffset += new Vector2(Time.deltaTime*speed.x, Time.deltaTime*speed.y);
		mat.SetTextureOffset(texchanel, curOffset);
	}
}
