#pragma once
#include "mkl.h"

extern "C"  _declspec(dllexport)
void CubicSplineInterpolation(MKL_INT nx, double* x, MKL_INT ny, double* y, double* bc, double* scoeff, MKL_INT nsite,
	double* site, MKL_INT ndorder, MKL_INT * dorder, double* r, int& ret, bool UniformGrid);


extern "C"  _declspec(dllexport)
void SplineSmoothing(MKL_INT LenX, MKL_INT NodesCount, double* X, double* Y, double* GridY,
	double* SplineY, MKL_INT MaxIter, double MinRes, MKL_INT & stopCriteria, int& error);
