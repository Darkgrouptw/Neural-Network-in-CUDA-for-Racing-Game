#pragma once
/*
神經元和神經元之間，連接的部分
*/
#include <cstdlib>
#include <ctime>

class Neuron;
class Synapse
{
public:
	Synapse(Neuron*, Neuron*);

	Neuron									*InputNeuron;					// 輸入的神經元
	Neuron									*OuputNeron;					// 輸出的神經元
	float									Weight;							// 佔有的權重
};

