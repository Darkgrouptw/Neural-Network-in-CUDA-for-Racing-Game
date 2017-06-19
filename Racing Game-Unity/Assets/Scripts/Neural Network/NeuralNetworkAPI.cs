using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class NeuralNetworkAPI : MonoBehaviour
{
    #region 測試部分
    // 測試東西傳得過來
    [DllImport("Neural Network in Cuda")]
    public static extern int TestFunction();

    // 測試能不能運行 GPU 程式
    [DllImport("Neural Network in Cuda")]
    public static extern IntPtr TestCudaFunction(int length);
    #endregion
}