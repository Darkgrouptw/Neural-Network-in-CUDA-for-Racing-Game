#pragma once
/*
Neural Network �̤j���[�c
�o�Ӹ̭��u���T�h
Input Layer
Hidden Layer
Output Layer
*/
#define  NeuralNetworkAPI _declspec(dllexport)

class NeuralNetwork
{
public:
	NeuralNetwork(int, int, int);

	float Compute();
private:
	float LearningRate;																	// ����Ѫ��ֺC
	float Momentum;																		// �e���ȼv�T�L���h�֪����
};

// �n��X�� API
extern "C"
{
	NeuralNetworkAPI NeuralNetwork*		CreateNeuralNetwork(int, int, int);				// ���ͤ@�� NN ���[�c
	NeuralNetworkAPI float				Compute(NeuralNetwork*);						// �g�L�h�� Layer ����
};