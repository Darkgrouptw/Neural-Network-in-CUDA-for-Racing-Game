#pragma once
/*
���g���M���g�������A�s��������
*/

#include "Neuron.h"
class Synapse
{
public:
	Synapse(Neuron inputNeuron, Neuron outputNeuron);

	Neuron	InputNeuron;					// ��J�����g��
	Neuron	OuputNeron;						// ��X�����g��
	float	Weight;							// �������v��
};

