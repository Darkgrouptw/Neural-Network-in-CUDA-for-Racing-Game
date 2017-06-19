/*
這個檔案是測試 Unity 和 Cuda 之間的功能
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