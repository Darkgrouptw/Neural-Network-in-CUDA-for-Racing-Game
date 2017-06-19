using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkManager : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Debug.Log(NeuralNetworkAPI.TestFunction());	
	}
}
