using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimatedTexture : MonoBehaviour {
	public enum AnimTextureType{
		DIFFUSE,
		EMISSIVE,
	}
	public AnimTextureType type = AnimTextureType.DIFFUSE;
	public float speed = 0.3f;
	public bool isAnimated = false;
	public List<Texture> textureList = new List<Texture>();

	float time = 0;
	int curIndex = 0;
	MeshRenderer mr = null;
	int textureNumber = -1;
	string texchanel = "_MainTex";

	// Use this for initialization
	void Start () {
		mr = GetComponent<MeshRenderer>();
		textureNumber = textureList.Count;
		switch (type)
		{
		case AnimTextureType.DIFFUSE:
			texchanel = "_MainTex";
			break;
		case AnimTextureType.EMISSIVE:
			texchanel = "_EmissiveTex";
			break;
		default:
			break;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(!isAnimated)
			return;
		if(time < speed)
		{
			time += Time.deltaTime;
			
		}else{

			time = 0;
			curIndex++;
			if(curIndex >= textureNumber)
			{
				curIndex = 0;
			}

			if(mr != null)
			{
				//mr.sharedMaterial.mainTexture = textureList[curIndex];
				mr.sharedMaterial.SetTexture(texchanel, textureList[curIndex]);
			}

		}
	}
}
