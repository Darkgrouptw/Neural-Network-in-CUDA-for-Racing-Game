using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NeuralNetworkManager : MonoBehaviour
{
    private IntPtr NeuralNetwork;

	void Start ()
    {
        NeuralNetwork = NeuralNetworkAPI.CreateNeuralNetwork(3, 4, 2);
        //Debug.Log(NeuralNetworkAPI.Compute(NeuralNetwork));
    }
}
