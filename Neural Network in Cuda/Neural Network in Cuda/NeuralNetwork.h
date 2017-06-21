#pragma once
/*
Neural Network �̤j���[�c
�o�Ӹ̭��u���T�h
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

	void								ForwardPropagate(float *);						// �����Ƥ���A�n�}�l��̫᪺ Output Value
	void								BackwardPropagate(float *);						// �ݸ� Target Value �t�h��

	float*								Compute(float *);								// �g�L Weight �]�X�� Output
	float								ComputeError(float *);							// �p��X�t�h��

	int									MaxEpochsCount;									// �̦h Train �X��
	float								LearningRate;									// ����Ѫ��ֺC
	float								Momentum;										// �e���ȼv�T�L���h�֪����
	float								MinimumError;									// �b�o�ӽd��̭��A�N���Φb Train �F

	// �̭��� Neuron �ƥ�
	int									InputSize;
	int									HiddenSize;
	int									OutputSize;

	// �T�Ӽh
	vector<Neuron *>					InputLayer;
	vector<Neuron *>					HiddenLayer;
	vector<Neuron *>					OutputLayer;
};

// �n����ƪ����A
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

// �n��X�� API
extern "C"
{
	NeuralNetworkAPI NeuralNetwork*		CreateNeuralNetwork(int, int, int);				// ���ͤ@�� NN ���[�c
	NeuralNetworkAPI void				ReleaseNeuralNetwork(NeuralNetwork*);			// �N Memory �M��
	NeuralNetworkAPI void				Train(NeuralNetwork *, DataSet *, int);			// �n Train �����

	NeuralNetworkAPI float*				Compute(NeuralNetwork*, float *);				// �g�L�h�� Layer ����
	NeuralNetworkAPI void				ReleaseCompute(float *);						// �M�� Output Dark
};