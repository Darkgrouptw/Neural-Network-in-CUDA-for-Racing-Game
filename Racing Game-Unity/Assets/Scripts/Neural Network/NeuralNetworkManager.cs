using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;

public class NeuralNetworkManager : MonoBehaviour
{
    [Header("========== Neural Network 相關 ==========")]
    public bool                                     IsTraining              = true;
    public string                                   FileName                = "../Test Data/";
    public DataRecorder                             recorder;
    
    private IntPtr                                  NeuralNetwork;                                  // NeuralNetwork 的指標
    private List<NeuralNetworkAPI.DataSet>          DataSetArray;                                   // Data Set 的集合

	private void Awake ()
    {
        if(!IsTraining)
        {
            // 關閉 Recorder
            recorder.enabled = false;

            // 產生 Neural Network
            NeuralNetwork = NeuralNetworkAPI.CreateNeuralNetwork(8, 11, 3);
            //NeuralNetworkAPI.Train(NeuralNetwork, DataSetArray.ToArray(), DataSetArray.Count);
        }
    }
    private void OnDisable()
    {
        NeuralNetworkAPI.ReleaseNeuralNetwork(NeuralNetwork);
    }


    private void ReadSaveData()
    {

    }

}
