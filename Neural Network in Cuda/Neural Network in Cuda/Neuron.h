#pragma once
/*
神經元~~
*/
#include "Synapse.h"
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

private:
	float								GetRandom();

	vector<Synapse *>					InputSynapse;
	vector<Synapse *>					OutputSynapse;

	float								Bias;						// Train 的時候，在 Bias 區間的，都有可能是正確解
};

