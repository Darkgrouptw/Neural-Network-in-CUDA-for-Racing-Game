#pragma once
/*
Neural Network �̤j���[�c
�o�Ӹ̭��u���T�h
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
	float								LearningRate;									// ����Ѫ��ֺC
	float								Momentum;										// �e���ȼv�T�L���h�֪����

	// �̭��� Neuron �ƥ�
	int									InputSize;
	int									HiddenSize;
	int									OutputSize;

	// �T�Ӽh
	vector<Neuron *>					InputLayer;
	vector<Neuron *>					HiddenLayer;
	vector<Neuron *>					OutputLayer;
};

// �n��X�� API
extern "C"
{
	NeuralNetworkAPI NeuralNetwork*		CreateNeuralNetwork(int, int, int);				// ���ͤ@�� NN ���[�c
	NeuralNetworkAPI void				DeleteNeuralNetwork(NeuralNetwork*);			// �N Memory �M��

	NeuralNetworkAPI float				Compute(NeuralNetwork*);						// �g�L�h�� Layer ����
};