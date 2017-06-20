#include "NeuralNetwork.h"

NeuralNetwork::NeuralNetwork(int InputSize, int HiddenSize, int OutputSize)
{
	// ��l�ƾǲߪ��ƭ�
	LearningRate		= 0.4f;
	Momentum			= 0.9f;
	MinimumError		= 0.01f;

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

	// �ھ� Learning Rate & Momentum�A�ӧ�s Weight
	for (int i = 0; i < HiddenSize; i++)
		HiddenLayer[i]->UpdateWeights(LearningRate, Momentum);
}

float* NeuralNetwork::Compute(float* InputValues)
{
	// ���g�L�C�� Weight
	ForwardPropagate(InputValues);

	float *outputArray = new float[OutputSize];
	for (int i = 0; i < OutputSize; i++)
		outputArray[i] = OutputLayer[i]->Value;

	return outputArray;
}

//////////////////////////////////////////////////////////////////////////
// �n��X�� API
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
	float error = 1.0f;					// Error ��
	int EpochsCount = 0;				// �`�@���F�X�Ӷg��

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
