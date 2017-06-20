#pragma once
/*
神經元和神經元之間，連接的部分
*/

#include "Neuron.h"
class Synapse
{
public:
	Synapse(Neuron inputNeuron, Neuron outputNeuron);

	Neuron	InputNeuron;					// 輸入的神經元
	Neuron	OuputNeron;						// 輸出的神經元
	float	Weight;							// 佔有的權重
};

