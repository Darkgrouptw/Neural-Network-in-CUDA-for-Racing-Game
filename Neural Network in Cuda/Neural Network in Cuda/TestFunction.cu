#include "TestFunction.cuh"

__global__ void SetIntArray(int* data, int size)
{
	int index = blockIdx.x * blockDim.x + threadIdx.x;
	if (index < size)
	{
		data[index] = index;
	}
}

TestFunctionAPI int TestFunction()
{
	return 6666;
}

TestFunctionAPI int* TestCudaFunction(int size)
{
	int ThreadSize = 256;
	int BlockSize = size / ThreadSize + 1;

	int *HostDataArray = new int[size];
	int *DeviceDataArray;

	size_t DataSize = sizeof(int) * size;
	cudaMalloc(&DeviceDataArray, DataSize);
	SetIntArray << <BlockSize, ThreadSize >> > (DeviceDataArray, size);

	cudaMemcpy(HostDataArray, DeviceDataArray, DataSize, cudaMemcpyDeviceToHost);
	cudaFree(DeviceDataArray);

	return HostDataArray;
}

TestFunctionAPI void TestFreeFunction(int *data)
{
	delete[] data;
}

