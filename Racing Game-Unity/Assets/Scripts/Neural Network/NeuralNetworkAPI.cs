using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class NeuralNetworkAPI : MonoBehaviour
{
    /// <summary>
    /// 產生一個 Neural Network 的架構
    /// </summary>
    /// <param name="InputSize">Input Layer 的 Neural 數目</param>
    /// <param name="HiddenSize">Hidden Layer 的 Neural 數目</param>
    /// <param name="OutputSize">Output Layer 的 Neural 數目</param>
    /// <returns>回傳 Neural Network 的指標</returns>
    [DllImport("Neural Network in Cuda")]
    public static extern IntPtr CreateNeuralNetwork(int InputSize, int HiddenSize, int OutputSize);

    /// <summary>
    /// 經過每一層的 Weight，算出最後的結果
    /// </summary>
    /// <param name="NeuralNetwork">Neural Network 的指標</param>
    [DllImport("Neural Network in Cuda")]
    public static extern float Compute(IntPtr NeuralNetwork);

    #region 測試部分
    // 測試東西傳得過來
    [DllImport("Neural Network in Cuda")]
    public static extern int TestFunction();

    // 測試能不能運行 GPU 程式
    [DllImport("Neural Network in Cuda")]
    public static extern IntPtr TestCudaFunction(int length);

    // 測試 Free Memory
    [DllImport("Neural Network in Cuda")]
    public static extern void TestFreeFunction(IntPtr dataArray);
    #endregion
}