#pragma once
/*
神經元~~
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
	float								Value;						// 計算完之後的值
	float								BiasDelta;					// Bias 的
	float								Bias;						// Train 的時候，在 Bias 區間的，都有可能是正確解
};

