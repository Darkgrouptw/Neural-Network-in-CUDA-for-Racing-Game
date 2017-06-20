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
    /// 刪除 Neural Network 的指標
    /// </summary>
    /// <param name="net">Neural Network 的指標</param>
    [DllImport("Neural Network in Cuda")]
    public static extern void DeleteNeuralNetwork(IntPtr net);

    /// <summary>
    /// 經過每一層算出最後結果
    /// </summary>
    /// <param name="net">Neural Network 的指標</param>
    /// <param name="InputValues">輸入的值</param>
    /// <returns>傳出 Output 的資料</returns>
    [DllImport("Neural Network in Cuda")]
    public static extern IntPtr Compute(IntPtr net, IntPtr InputValues);

    /// <summary>
    /// 將輸出的值清空
    /// </summary>
    /// <param name="InputValues">輸入的值</param>
    [DllImport("Neural Network in Cuda")]
    public static extern void ReleaseCompute(IntPtr InputValues);
}