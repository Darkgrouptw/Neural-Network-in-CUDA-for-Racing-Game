#include "NeuralNetwork.h"

NeuralNetwork::NeuralNetwork(int InputSize, int HiddenSize, int OutputSize)
{
	#pragma region ��l��
	LearningRate = 0.4f;
	Momentum = 0.9f;
	#pragma endregion
}

float NeuralNetwork::Compute()
{
	return LearningRate;
}

//////////////////////////////////////////////////////////////////////////
// �n��X�� API
//////////////////////////////////////////////////////////////////////////
NeuralNetworkAPI NeuralNetwork* CreateNeuralNetwork(int InputSize, int HiddenSize, int OutputSize)
{
	NeuralNetwork *net = new NeuralNetwork(InputSize, HiddenSize, OutputSize);
	return net;
}
NeuralNetworkAPI float Compute(NeuralNetwork* net)
{
	return net->Compute();
}
