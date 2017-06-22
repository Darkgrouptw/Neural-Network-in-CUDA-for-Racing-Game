#include "Neuron.h"

Neuron::Neuron()
{
	srand(time(0));
	Bias = GetRandom();
	BiasDelta = 0;
}
Neuron::Neuron(vector<Neuron *> lastLayer)
{
	srand(time(0));
	Bias = GetRandom();
	BiasDelta = 0;

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
	for (int i = OutputSynapse.size() - 1; i > 0; i--)
		delete OutputSynapse[i];
}

void Neuron::CalculateValue()
{
	float Total = 0;
	
	// 將 Weight 乘上值加總起來
	for (int i = 0; i < InputSynapse.size(); i++)
		Total += InputSynapse[i]->Weight * InputSynapse[i]->InputNeuron->Value;

	// Sigmoid Function
	Value = Sigmoid::Calc(Total + Bias);
}
void Neuron::CalculateGradient()
{
	float Total = 0;

	// 根據 Output 的 Gradient 反算回來， Input 的 Gradient 應該是多少
	for (int i = 0; i < OutputSynapse.size(); i++)
		Total += OutputSynapse[i]->OuputNeron->Gradient * OutputSynapse[i]->Weight;
	Gradient = Total * Sigmoid::Derivative(Value);
}
void Neuron::CalculateGradient(float target)
{
	Gradient = CalculateError(target) * Sigmoid::Derivative(Value);
}
void Neuron::UpdateWeights(float LearnRate, float Momentum)
{
	float preDelta = BiasDelta;
	BiasDelta = LearnRate * Gradient;
	Bias += BiasDelta + Momentum * preDelta;

	for (int i = 0; i < InputSynapse.size(); i++)
	{
		preDelta = InputSynapse[i]->WeightDelta;
		InputSynapse[i]->WeightDelta = LearnRate * Gradient * InputSynapse[i]->InputNeuron->Value;
		InputSynapse[i]->Weight += InputSynapse[i]->WeightDelta + Momentum * preDelta;
	}
}

float Neuron::CalculateError(float target)
{
	return target - Value;
}

float Neuron::GetRandom()
{
	// 輸出 -1 ~ 1 
	//return 2.0f * rand() / RAND_MAX - 1;
	return 0;
}
