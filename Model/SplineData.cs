using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Model
{
    public class SplineData
    {

        public V1DataArray DataArray { get; set; } // изначальный массив, для которого строится сплайн

        public int NodesCount { get; set; } // количество узлов равномерной сетки, на которых строится сплайн

        public int NewNodesCount { get; set; } // количество узлов более мелкой равномерной сетки, на которых
                                               // строится сплайн: NewNodesCount > NodesCount

        public double[] SplineY { get; set; } // массив, который содержит значения Y сглаживающего сплайна (на изначальной сетке)

        public int MaxIter { get; set; }

        public int StopReason { get; set; }

        public double MinRes { get; set; }

        public List<SplineDataItem> SplineDataList { get; set; } // структура, которая содержит значения X, Y1 изначального массива
                                                                 // и значение Y сглаживающего сплайна в этих узлах X (на изначальной сетке)

        public List<DataItemS> SplineCoordinates { get; set; } // структура, которая содержит значения узлов X сетки NewNodesCount, в которой
                                                              // строится сплайн и значения Y в этих узлах

        public SplineData(V1DataArray dataArray, int nodesCount, int newNodesCount, int maxIter, double minRes)
        {
            DataArray = dataArray;
            NodesCount = nodesCount;
            NewNodesCount = newNodesCount;
            MaxIter = maxIter;
            MinRes = minRes;
            SplineDataList = new List<SplineDataItem>();
            SplineY = new double[DataArray.X.Length];
            SplineCoordinates = new List<DataItemS>();
        }

        public bool GetSmoothingSpline()
        {

            double[] GridY = new double[NodesCount];
            GridY[0] = DataArray.Y[0][0];
            GridY[NodesCount - 1] = DataArray.Y[0][DataArray.X.Length - 1];

            //double[] NewGridY = new double[NewNodesCount];
            //NewGridY[0] = DataArray.Y[0][0];
            //NewGridY[NewNodesCount - 1] = DataArray.Y[0][DataArray.X.Length - 1];

            int error = 24;
            int status = 0;

            try
            {

                SplineSmoothing(DataArray.X.Length, NodesCount, DataArray.X, DataArray.Y[0], GridY, SplineY, MaxIter,
                                MinRes, ref status, ref error);

                StopReason = status;

                for (int i = 0; i < DataArray.X.Length; i++)
                {
                    SplineDataItem Node = new SplineDataItem(DataArray.X[i], DataArray.Y[0][i], SplineY[i]);
                    SplineDataList.Add(Node);
                }

                double[] GridX = new double[2] { DataArray.X[0], DataArray.X[DataArray.X.Length - 1] };
                double[] bc = new double[2] { 0, 0 };
                double[] scoeff = new double[1 * 4 * (NodesCount - 1)];
                int ndorder = 3;
                int[] dorder = new int[3] { 1, 0, 0 };
                double[] NewGridY = new double[NewNodesCount];
                int ret = 0;

                CubicSplineInterpolation(NodesCount, GridX, 1, GridY, bc, scoeff, NewNodesCount, GridX, ndorder, dorder, NewGridY, ref ret, true);

                double NewGridX = DataArray.X[0];
                double step = (DataArray.X[DataArray.X.Length - 1] - NewGridX) / (NewNodesCount - 1);

                for (int i = 0; i < NewNodesCount; i++)
                {
                    DataItemS Node = new DataItemS(NewGridX, NewGridY[i]);
                    SplineCoordinates.Add(Node);                                
                    NewGridX += step;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in function GetSmoothingSpline: {ex.Message}");
                return false;
            }
        }

        [DllImport("..\\..\\..\\..\\x64\\Debug\\DLL_MKL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SplineSmoothing(int LenX, int NodesCount, double[] X, double[] Y, double[] GridY, 
                                                  double[] SplineY, int MaxIter, double MinRes, ref int StopCriteria, ref int error);

        [DllImport("..\\..\\..\\..\\x64\\Debug\\DLL_MKL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CubicSplineInterpolation(int nx, double[] x, int ny, double[] y, double[] bc,
                                                         double[] scoeff, int nsite, double[] site, int ndorder,
                                                         int[] dorder, double[] resultDeriv, ref int ret, bool UniformGrid);

        public string ToLongString(string format)
        {
            string str_res = $"V1_DATA_ARRAY:\n\n{DataArray.ToLongString(format)}";

            str_res += "SPLINE_DATA:\n\n";

            foreach (SplineDataItem item in SplineDataList)
            {
                str_res += item.ToLongString(format);
            }

            str_res += $"MIN_RESIDUAL:    {MinRes}\n\n";

            str_res += $"STOP_REASON:     {GetStopReason(StopReason)}\n\n";

            str_res += $"ITERATIONS_NUM:  {MaxIter}\n\n";

            return str_res;
        }

        public bool Save(string filename, string format)
        {
            StreamWriter? writer = null;
            try
            {
                writer = new StreamWriter(filename, false);
                writer.WriteLine(this.ToLongString(format));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in function Save: {ex.Message}");
                return false;
            }
            finally
            {
                if (writer != null) writer.Dispose();
            }
        }

        private string GetStopReason(int StopReason)
        {
            switch (StopReason)
            {
                case 1: return "Превышено заданное число итераций";
                case 2: return "Размер доверительной области < 1.0E-12";
                case 3: return "Норма невязки < 1.0E-12";
                case 4: return "Норма строк матрицы Якоби < 1.0E-12";
                case 5: return "Пробный шаг < 1.0E-12";
                case 6: return "Разность нормы функции и погрешности < 1.0E-12";
                default: return "";
            }
        }

    }
}
