#include <cuda_runtime.h>
#include <device_launch_parameters.h>

namespace Sigmoid
{
	__device__ float Calc(float value)
	{
		if (value < -45)
			return 0;
		else if (value > 45)
			return 1;
		return 1.0f / (1.0f + __expf(-value));
	}
	__device__ float Derivative(float value)
	{
		return value * (1 - value);
	}
}