#pragma once
/*
Neural Network 最大的架構
這個裡面只有三層
Input Layer
Hidden Layer
Output Layer
*/
#include "Neuron.h"
#include <vector>

#define  NeuralNetworkAPI _declspec(dllexport)
using namespace std;

class NeuralNetwork
{
public:
	NeuralNetwork(int, int, int);
	~NeuralNetwork();

	float Compute();
private:
	float								LearningRate;									// 接近解的快慢
	float								Momentum;										// 前面值影響他的多少的比例

	// 裡面的 Neuron 數目
	int									InputSize;
	int									HiddenSize;
	int									OutputSize;

	// 三個層
	vector<Neuron *>					InputLayer;
	vector<Neuron *>					HiddenLayer;
	vector<Neuron *>					OutputLayer;
};

// 要輸出的 API
extern "C"
{
	NeuralNetworkAPI NeuralNetwork*		CreateNeuralNetwork(int, int, int);				// 產生一個 NN 的架構
	NeuralNetworkAPI void				DeleteNeuralNetwork(NeuralNetwork*);			// 將 Memory 清空

	NeuralNetworkAPI float				Compute(NeuralNetwork*);						// 經過多個 Layer 之後
};