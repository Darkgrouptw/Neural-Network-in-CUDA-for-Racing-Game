using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class NeuralNetworkAPI : MonoBehaviour
{
    [DllImport("Neural Network in Cuda")]
    public static extern int TestFunction();
}
