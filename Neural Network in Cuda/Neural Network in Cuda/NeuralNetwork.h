#pragma once
/*
Neural Network 最大的架構
這個裡面只有三層
Input Layer
Hidden Layer
Output Layer
*/
#include "Neuron.h"
#include <iostream>
#include <Windows.h>
#include <vector>

#define  NeuralNetworkAPI _declspec(dllexport)
using namespace std;

#define IsDebugMode

class NeuralNetwork
{
public:
	NeuralNetwork(int, int, int);
	~NeuralNetwork();

	void								ForwardPropagate(float *);						// 拿到資料之後，要開始算最後的 Output Value
	void								BackwardPropagate(float *);						// 看跟 Target Value 差多少

	float*								Compute(float *);								// 經過 Weight 跑出的 Output
	float								ComputeError(float *);							// 計算出差多少

	int									MaxEpochsCount;									// 最多 Train 幾次
	float								LearningRate;									// 接近解的快慢
	float								Momentum;										// 前面值影響他的多少的比例
	float								MinimumError;									// 在這個範圍裡面，就不用在 Train 了

	// 裡面的 Neuron 數目
	int									InputSize;
	int									HiddenSize;
	int									OutputSize;

	// 三個層
	vector<Neuron *>					InputLayer;
	vector<Neuron *>					HiddenLayer;
	vector<Neuron *>					OutputLayer;
};

// 要接資料的型態
extern "C" NeuralNetworkAPI struct DataSet
{
	float *Values;
	float *Targets;

	DataSet(float *Values, float *Targets)
	{
		this->Values = Values;
		this->Targets = Targets;
	}
};

// 要輸出的 API
extern "C"
{
	NeuralNetworkAPI NeuralNetwork*		CreateNeuralNetwork(int, int, int);				// 產生一個 NN 的架構
	NeuralNetworkAPI void				ReleaseNeuralNetwork(NeuralNetwork*);			// 將 Memory 清空
	NeuralNetworkAPI void				Train(NeuralNetwork *, DataSet *, int);			// 要 Train 的資料

	NeuralNetworkAPI float*				Compute(NeuralNetwork*, float *);				// 經過多個 Layer 之後
	NeuralNetworkAPI void				ReleaseCompute(float *);						// 清除 Output Dark
};