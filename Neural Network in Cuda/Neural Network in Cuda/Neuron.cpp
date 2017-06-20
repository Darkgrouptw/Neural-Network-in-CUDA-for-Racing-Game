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

	// 加上連接的關係
	for (int i = 0; i < lastLayer.size(); i++)
	{
		Synapse *tempSynapse = new Synapse(lastLayer[i], this);
		lastLayer[i]->OutputSynapse.push_back(tempSynapse);						// 前一個的 Output 接上 synapse
		InputSynapse.push_back(tempSynapse);									// 這一層的 Input 接上 synapse
	}
}

Neuron::~Neuron()
{
}

float Neuron::GetRandom()
{
	// 輸出 -1 ~ 1 
	return 2.0f * rand() / RAND_MAX - 1;
}
