#include <iostream>
#include <cuda_runtime.h>
#include <device_launch_parameters.h>

#define SigmoidAPI _declspec(dllexport)

extern "C"
{
	SigmoidAPI int TestFunction();
}