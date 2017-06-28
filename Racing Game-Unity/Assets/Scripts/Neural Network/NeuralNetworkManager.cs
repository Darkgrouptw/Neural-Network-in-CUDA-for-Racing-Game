using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(NeuralNetworkCarController))]
public class NeuralNetworkManager : MonoBehaviour
{
    [Header("========== Neural Network 相關 ==========")]
    public bool                                     IsTraining              = true;
    public string                                   FileName                = "../Test Data/";
    public DataRecorder                             recorder;
    
    private IntPtr                                  NeuralNetwork;                                  // NeuralNetwork 的指標
    private List<float[]>                           FileValuesSet;                                  // File Data Set
    private List<float[]>                           FileTargetSet;                                  // File Target Set
    private List<NeuralNetworkAPI.DataSet>          DataSetArray;                                   // Data Set 的集合

	private void Awake ()
    {
        if(!IsTraining)
        {
            // 關閉 Recorder
            recorder.enabled = false;

            // 產生 Neural Network
            if (!ReadTrainData())
                return;

            //NeuralNetwork = NeuralNetworkAPI.CreateNeuralNetwork(5, 11, 3);
            NeuralNetwork = NeuralNetworkAPI.CreateNeuralNetwork(25, 53, 3, 1000, 0.4f, 0.9f);
            NeuralNetworkAPI.Train(NeuralNetwork, DataSetArray.ToArray(), DataSetArray.Count);
        }
    }

    private void OnDisable()
    {
        if (!IsTraining)
            NeuralNetworkAPI.ReleaseNeuralNetwork(NeuralNetwork);
    }


    private bool ReadTrainData()
    {
        // 判斷檔案存不存在
        if(!File.Exists(FileName))
        {
            Debug.LogError("檔案位置錯誤");
            return false;
        }

        if (DataSetArray == null)
        {
            DataSetArray = new List<NeuralNetworkAPI.DataSet>();
            FileValuesSet = new List<float[]>();
            FileTargetSet = new List<float[]>();
        }
        else
        {
            DataSetArray.Clear();
            FileValuesSet.Clear();
            FileTargetSet.Clear();
        }

        string[] FileTextLine = File.ReadAllLines(FileName);
        for(int i = 1; i < FileTextLine.Length; i++)
        {
            string[] EachPart = FileTextLine[i].Split(',');

            if (EachPart.Length == 0)
                break;

            float[] Values = new float[5];
            float[] Targets = new float[3];

            // 讀資料
            for (int j = 0; j < Values.Length; j++)
                Values[j] = float.Parse(EachPart[j + 1]);
            for (int j = 0; j < Targets.Length; j++)
                Targets[j] = float.Parse(EachPart[j + 1 + Values.Length]);
            FileValuesSet.Add(Values);
            FileTargetSet.Add(Targets);
        }

        // 把資料加進來
        for(int i = 4; i < FileValuesSet.Count; i++)
        {
            float[] Values = new float[25];
            float[] Targets = new float[3];
            #region 把資料加進去
            for (int j = 0; j < FileValuesSet[i - 4].Length; j++)
                Values[j] = FileValuesSet[i][j];

            for (int j = 0; j < FileValuesSet[i - 3].Length; j++)
                Values[j + 5] = FileValuesSet[i][j];
            
            for (int j = 0; j < FileValuesSet[i - 2].Length; j++)
                Values[j + 10] = FileValuesSet[i][j];

            for (int j = 0; j < FileValuesSet[i - 1].Length; j++)
                Values[j + 15] = FileValuesSet[i][j];

            for (int j = 0; j < FileValuesSet[i].Length; j++)
                Values[j + 20] = FileValuesSet[i][j];
            
            for (int j = 0; j < Targets.Length; j++)
                Targets[j] = FileTargetSet[i][j];
            #endregion
            NeuralNetworkAPI.DataSet dataSet = new NeuralNetworkAPI.DataSet(Values, Targets);
            DataSetArray.Add(dataSet);
        }
        return true;
    }


    public float[] Compute(float[] Values)
    {
        IntPtr DataPointer = NeuralNetworkAPI.Compute(NeuralNetwork, Values);
        float[] ReturnFloatArray = new float[3];
        Marshal.Copy(DataPointer, ReturnFloatArray, 0, 3);
        return ReturnFloatArray;
    }
}
