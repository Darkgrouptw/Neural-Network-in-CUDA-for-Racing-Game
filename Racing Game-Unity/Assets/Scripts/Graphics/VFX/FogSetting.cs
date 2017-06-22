using UnityEngine;
using System.Collections;

public class FogSetting : MonoBehaviour {

	public bool enable = true;
	public Color color = new Color(0.8f, 0.8f, 1f);
	public float density = 0.5f;
	public FogMode mode = FogMode.Linear;
	public float startDistance = 700f;
	public float endDistance = 1500f;


	// Use this for initialization
	void Start () {
		RenderSettings.fog = enable;
		RenderSettings.fogDensity = density;
		RenderSettings.fogColor = color;
		RenderSettings.fogMode = mode;
		RenderSettings.fogStartDistance = startDistance;
		RenderSettings.fogEndDistance = endDistance;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnValidate() {
		RenderSettings.fog = enable;
		RenderSettings.fogDensity = density;
		RenderSettings.fogColor = color;
		RenderSettings.fogMode = mode;
		RenderSettings.fogStartDistance = startDistance;
		RenderSettings.fogEndDistance = endDistance;
	}


}
