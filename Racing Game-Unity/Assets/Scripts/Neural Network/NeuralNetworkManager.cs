using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NeuralNetworkManager : MonoBehaviour
{
	void Start ()
    {
        #region 測試
        Debug.Log("Test Function =>" + NeuralNetworkAPI.TestFunction());
        #endregion
        #region 測試 Cuda
        IntPtr ArrayDataPointer = NeuralNetworkAPI.TestCudaFunction(25);
        int[] ArrayData = new int[25];
        Marshal.Copy(ArrayDataPointer, ArrayData, 0, 25);
        for (int i = 0; i < ArrayData.Length; i++)
            Debug.Log("Test Cuda =>" + ArrayData[i]);
        #endregion
        #region 清除測試
        NeuralNetworkAPI.TestFreeFunction(ArrayDataPointer);
        #endregion
    }
}
