#include "Neuron.h"

Neuron::Neuron()
{
	srand(time(0));
	Bias = GetRandom();
}

Neuron::Neuron(vector<Neuron *> lastLayer)
{
	srand(time(0));
	Bias = GetRandom();

	// �[�W�s�������Y
	for (int i = 0; i < lastLayer.size(); i++)
	{
		Synapse *tempSynapse = new Synapse(lastLayer[i], this);
		lastLayer[i]->OutputSynapse.push_back(tempSynapse);						// �e�@�Ӫ� Output ���W synapse
		InputSynapse.push_back(tempSynapse);									// �o�@�h�� Input ���W synapse
	}
}

Neuron::~Neuron()
{
}

float Neuron::GetRandom()
{
	// ��X -1 ~ 1 
	return 2.0f * rand() / RAND_MAX - 1;
}
