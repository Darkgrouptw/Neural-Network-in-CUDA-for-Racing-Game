#include "Synapse.h"

Synapse::Synapse(Neuron *inputNeuron, Neuron *outputNeuron)
{
	this->InputNeuron	= inputNeuron;
	this->OuputNeron	= outputNeuron;

	this->Weight = 2.0f * rand() / RAND_MAX - 1;
}