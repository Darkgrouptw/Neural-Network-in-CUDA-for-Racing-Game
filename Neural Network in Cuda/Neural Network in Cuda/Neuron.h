#pragma once
/*
���g��~~
*/
#include "Synapse.h"
#include "Sigmoid.cuh"

#include <vector>
#include <cstdlib>
#include <ctime>

using namespace std;

class Neuron
{
public:
	Neuron();
	Neuron(vector<Neuron *>);
	~Neuron();

	void								CalculateValue();
	void								CalculateGradient();
	void								CalculateGradient(float);
	void								UpdateWeights(float, float);

	float								CalculateError(float);

	float								GetRandom();

	vector<Synapse *>					InputSynapse;
	vector<Synapse *>					OutputSynapse;

	float								Gradient;
	float								Value;						// �p�⧹���᪺��
	float								BiasDelta;					// Bias ��
	float								Bias;						// Train ���ɭԡA�b Bias �϶����A�����i��O���T��
};

