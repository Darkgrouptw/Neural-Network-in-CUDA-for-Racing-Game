#include "NeuralNetwork.h"

NeuralNetwork::NeuralNetwork(int InputSize, int HiddenSize, int OutputSize)
{
	// ��l�ƾǲߪ��ƭ�
	LearningRate		= 0.4f;
	Momentum			= 0.9f;

	// �N�ȵ��i�ܼ�
	this->InputSize		= InputSize;
	this->HiddenSize	= HiddenSize;
	this->OutputSize	= OutputSize;

	// ��l�ƤT�h
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
// �n��X�� API
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
