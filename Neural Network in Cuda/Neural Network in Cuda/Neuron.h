#pragma once
/*
���g��~~
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

	float								Bias;						// Train ���ɭԡA�b Bias �϶����A�����i��O���T��
};

