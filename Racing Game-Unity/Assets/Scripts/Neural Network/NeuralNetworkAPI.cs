using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class NeuralNetworkAPI : MonoBehaviour
{
    /// <summary>
    /// Data Set 的資料型態
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DataSet
    {
        public float[] Values;
        public float[] Targets;
        
        public DataSet(float[] Values, float[] Targets)
        {
            this.Values = Values;
            this.Targets = Targets;
        }
    }

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
    public static extern void ReleaseNeuralNetwork(IntPtr net);

    /// <summary>
    /// 要 Train Weight
    /// </summary>
    /// <param name="net">Neural Network 的指標</param>
    /// <param name="dataArray">測試資料的 Array</param>
    /// <param name="dataSize">測試資料的大小</param>
    [DllImport("Neural Network in Cuda")]
    public static extern void Train(IntPtr net, DataSet[] dataArray, int dataSize);

    /// <summary>
    /// 經過每一層算出最後結果
    /// </summary>
    /// <param name="net">Neural Network 的指標</param>
    /// <param name="InputValues">輸入的值</param>
    /// <returns>傳出 Output 的資料</returns>
    [DllImport("Neural Network in Cuda")]
    public static extern IntPtr Compute(IntPtr net, float[] InputValues);
}