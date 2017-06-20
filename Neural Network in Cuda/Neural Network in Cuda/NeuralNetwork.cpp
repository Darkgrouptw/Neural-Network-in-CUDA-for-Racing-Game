#include "NeuralNetwork.h"

NeuralNetwork::NeuralNetwork(int InputSize, int HiddenSize, int OutputSize)
{
	// 初始化學習的數值
	LearningRate		= 0.4f;
	Momentum			= 0.9f;

	// 將值給進變數
	this->InputSize		= InputSize;
	this->HiddenSize	= HiddenSize;
	this->OutputSize	= OutputSize;

	// 初始化三層
	for (int i = 0; i < InputSize; i++)
		InputLayer.push_back(new Neuron());
	for (int i = 0; i < HiddenSize; i++)
		HiddenLayer.push_back(new Neuron(InputLayer));
	for (int i = 0; i < OutputSize; i++)
		OutputLayer.push_back(new Neuron(HiddenLayer));
}
NeuralNetwork::~NeuralNetwork()
{
	for (int i = InputSize - 1; i >=0; i--)
		delete InputLayer[i];
	for (int i = HiddenSize - 1; i >= 0; i--)
		delete HiddenLayer[i];
	for (int i = OutputSize - 1; i >= 0; i--)
		delete OutputLayer[i];
}

float NeuralNetwork::Compute()
{
	return Momentum;
}

//////////////////////////////////////////////////////////////////////////
// 要輸出的 API
//////////////////////////////////////////////////////////////////////////
NeuralNetworkAPI NeuralNetwork* CreateNeuralNetwork(int InputSize, int HiddenSize, int OutputSize)
{
	NeuralNetwork *net = new NeuralNetwork(InputSize, HiddenSize, OutputSize);
	return net;
}
NeuralNetworkAPI void DeleteNeuralNetwork(NeuralNetwork* net)
{
	delete net;
}

NeuralNetworkAPI float Compute(NeuralNetwork* net)
{
	return net->Compute();
}
