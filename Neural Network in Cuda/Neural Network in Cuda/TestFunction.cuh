/*
�o���ɮ׬O���� Unity �M Cuda �������\��
*/

#include <cuda_runtime.h>
#include <device_launch_parameters.h>

#define TestFunctionAPI _declspec(dllexport)

extern "C"
{
	TestFunctionAPI int TestFunction();
	TestFunctionAPI int* TestCudaFunction(int);
	TestFunctionAPI void TestFreeFunction(int *);
}