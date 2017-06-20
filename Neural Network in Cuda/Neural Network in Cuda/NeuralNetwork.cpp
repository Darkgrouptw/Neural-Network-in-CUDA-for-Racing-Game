#include "NeuralNetwork.h"

NeuralNetwork::NeuralNetwork(int InputSize, int HiddenSize, int OutputSize)
{
	// 初始化學習的數值
	LearningRate		= 0.4f;
	Momentum			= 0.9f;
	MinimumError		= 0.01f;

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

void NeuralNetwork::ForwardPropagate(float *inputValues)
{
	for (int i = 0; i < InputSize; i++)
		InputLayer[i]->Value = inputValues[i];

	for (int i = 0; i < HiddenSize; i++)
		HiddenLayer[i]->CalculateValue();

	for (int i = 0; i < OutputSize; i++)
		OutputLayer[i]->CalculateValue();
}
void NeuralNetwork::BackwardPropagate(float *targets)
{
	for (int i = 0; i < OutputSize; i++)
		OutputLayer[i]->CalculateGradient(targets[i]);

	for (int i = 0; i < HiddenSize; i++)
		HiddenLayer[i]->CalculateGradient();

	// 根據 Learning Rate & Momentum，來更新 Weight
	for (int i = 0; i < HiddenSize; i++)
		HiddenLayer[i]->UpdateWeights(LearningRate, Momentum);
}

float* NeuralNetwork::Compute(float* InputValues)
{
	// 先經過每個 Weight
	ForwardPropagate(InputValues);

	float *outputArray = new float[OutputSize];
	for (int i = 0; i < OutputSize; i++)
		outputArray[i] = OutputLayer[i]->Value;

	return outputArray;
}

//////////////////////////////////////////////////////////////////////////
// 要輸出的 API
//////////////////////////////////////////////////////////////////////////
NeuralNetworkAPI NeuralNetwork*		CreateNeuralNetwork(int InputSize, int HiddenSize, int OutputSize)
{
	NeuralNetwork *net = new NeuralNetwork(InputSize, HiddenSize, OutputSize);
	return net;
}
NeuralNetworkAPI void				DeleteNeuralNetwork(NeuralNetwork* net)
{
	delete net;
}
NeuralNetworkAPI void				Train(NeuralNetwork *net, DataSet *dataArray, int dataSize)
{
	float error = 1.0f;					// Error 值
	int EpochsCount = 0;				// 總共做了幾個週期

	while (error > net->MinimumError && EpochsCount < INT_MAX)
	{
		for (int i = 0; i < dataSize; i++)
		{
			net->ForwardPropagate(dataArray->Values);
			net->BackwardPropagate(dataArray->Targets);
		}
		EpochsCount++;
	}
}

NeuralNetworkAPI float*				Compute(NeuralNetwork* net, float *InputValues)
{
	return net->Compute(InputValues);
}
NeuralNetworkAPI void				ReleaseCompute(float *InputValues)
{
	delete[] InputValues;
}
