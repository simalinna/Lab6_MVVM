#include "pch.h"
#include "mkl.h"
#include <iostream>

extern "C"  _declspec(dllexport)
void CubicSplineInterpolation(MKL_INT nx, double* x, MKL_INT ny, double* y, double* bc, double* scoeff, MKL_INT nsite,
	double* site, MKL_INT ndorder, MKL_INT * dorder, double* r, int& ret, bool UniformGrid)
{
	DFTaskPtr task;
	int dfStatus;

	try
	{

		dfStatus = dfdNewTask1D(&task, nx, x, DF_UNIFORM_PARTITION, ny, y, DF_MATRIX_STORAGE_ROWS);
		if (dfStatus != DF_STATUS_OK)
		{
			ret = 1;
			return;
		}

		dfStatus = dfdEditPPSpline1D(task, DF_PP_CUBIC, DF_PP_NATURAL, DF_BC_FREE_END, bc, DF_NO_IC, NULL,
			scoeff, DF_NO_HINT);
		if (dfStatus != DF_STATUS_OK)
		{
			ret = 2;
			return;
		}

		dfStatus = dfdConstruct1D(task, DF_PP_SPLINE, DF_METHOD_STD);
		if (dfStatus != DF_STATUS_OK)
		{
			ret = 3;
			return;
		}


		MKL_INT GridType = UniformGrid ? DF_UNIFORM_PARTITION : DF_NON_UNIFORM_PARTITION;
		dfStatus = dfdInterpolate1D(task, DF_INTERP, DF_METHOD_PP, nsite, site, GridType, ndorder,
			dorder, NULL, r, DF_MATRIX_STORAGE_ROWS, NULL);
		if (dfStatus != DF_STATUS_OK)
		{
			ret = 4;
			return;
		}


		dfStatus = dfDeleteTask(&task);
		if (dfStatus != DF_STATUS_OK)
		{
			ret = 6;
			return;
		}

		ret = 0;

	}
	catch (...)
	{
		ret = 7;
	}
}


struct NodesValue
{
	double grid[2];
	double* X = NULL;
	double* Y = NULL;
};


void YError(MKL_INT* LenX, MKL_INT* NodesCount, double* Y, double* YError, void* nodesValue);


extern "C"  _declspec(dllexport)
void SplineSmoothing(MKL_INT LenX, MKL_INT NodesCount, double* X, double* Y, double* GridY,
	double* SplineY, MKL_INT MaxIter, double MinRes, MKL_INT & stopCriteria, int& error)
{
	_TRNSP_HANDLE_t handle = NULL;
	double* fvec = NULL;
	double* fjac = NULL;

	NodesValue* nodesValue = new NodesValue();
	nodesValue->grid[0] = X[0];
	nodesValue->grid[1] = X[LenX - 1];
	nodesValue->X = X;
	nodesValue->Y = Y;

	try
	{

		fvec = new double[LenX]; // массив значений векторной функции
		fjac = new double[LenX * NodesCount]; // массив с элементами матрицы Якоби

		MKL_INT niter1 = MaxIter;
		MKL_INT niter2 = 10;

		MKL_INT ndoneIter = 0; // число выполненных итераций
		double rs = 10; // начальное значение для доверительного интервала
		const double eps[6] = { 1.0E-12, MinRes, 1.0E-12 , 1.0E-12 , 1.0E-12 , 1.0E-12 };
		double jac_eps = 1.0E-8;

		double resInitial = 0; // начальное значение невязки
		double resFinal = 0; // конечное значение невязки

		MKL_INT checkInfo[4];

		// Инициализация задачи
		MKL_INT ret = dtrnlsp_init(&handle, &NodesCount, &LenX, GridY, eps, &niter1, &niter2, &rs);
		if (ret != TR_SUCCESS) throw 1;

		// Проверка корректности входных данных
		ret = dtrnlsp_check(&handle, &NodesCount, &LenX, fjac, fvec, eps, checkInfo);
		if (ret != TR_SUCCESS) throw 2;

		MKL_INT RCI_Request = 0;

		// Итерационный процесс
		while (true)
		{
			ret = dtrnlsp_solve(&handle, fvec, fjac, &RCI_Request);
			if (ret != TR_SUCCESS) throw 3;

			if (RCI_Request == 0) continue;
			else if (RCI_Request == 1) YError(&LenX, &NodesCount, GridY, fvec, nodesValue);
			else if (RCI_Request == 2)
			{
				ret = djacobix(YError, &NodesCount, &LenX, fjac, X, &jac_eps, nodesValue);
				if (ret != TR_SUCCESS) throw 4;
			}
			else if (RCI_Request >= -6 && RCI_Request <= -1) break;
			else throw 5;
		}

		// Завершение итерационного процесса
		ret = dtrnlsp_get(&handle, &ndoneIter, &stopCriteria, &resInitial, &resFinal);

		if (ret != TR_SUCCESS) throw 6;

		// Освобождение ресурсов
		ret = dtrnlsp_delete(&handle);
		if (ret != TR_SUCCESS) throw 7;
	}
	catch (int err)
	{
		error = err;
	}

	// Освобождение памяти
	if (fvec != NULL) delete[] fvec;
	if (fjac != NULL) delete[] fjac;

	YError(&LenX, &NodesCount, GridY, SplineY, nodesValue);
	for (int i = 0; i < LenX; ++i)
		SplineY[i] += Y[i];
}


void YError(MKL_INT* LenX, MKL_INT* NodesCount, double* GridY, double* YError, void* nodesValue)
{
	NodesValue* NV = (NodesValue*)nodesValue;

	int ny = 1;

	double* bc = new double[2] {0, 0};

	double* scoeff = new double[ny * 4 * (*LenX - 1)];

	int ndorder = 3;
	int* dorder = new int[3] { 1, 0, 0 };

	int ret = 0;

	bool UniformGrid = false;

	CubicSplineInterpolation(*NodesCount, NV->grid, ny, GridY, bc, scoeff, *LenX,
		NV->X, ndorder, dorder, YError, ret, UniformGrid);

	for (int i = 0; i < *LenX; i++)
	{
		YError[i] -= NV->Y[i];
	}
}