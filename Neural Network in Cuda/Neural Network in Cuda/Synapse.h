#pragma once
/*
���g���M���g�������A�s��������
*/
#include <cstdlib>
#include <ctime>

class Neuron;
class Synapse
{
public:
	Synapse(Neuron*, Neuron*);

	Neuron									*InputNeuron;					// ��J�����g��
	Neuron									*OuputNeron;					// ��X�����g��
	float									Weight;							// �������v��
};

