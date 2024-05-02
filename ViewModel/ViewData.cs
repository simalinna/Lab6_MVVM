using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Model;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using OxyPlot;
using System.Windows.Input;
using System.Windows.Controls;

namespace ViewModel
{
    public class ViewData: INotifyPropertyChanged, IDataErrorInfo
    {

        private readonly IErrorSender errorSender;

        private readonly IFileDialog fileDialog;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double LeftBorder { get; set; } = 0;

        public double RightBorder { get; set; } = 10;

        public int NodesX { get; set; } = 10;

        public bool UniformGrid { get; set; } = true;

        public FValuesEnum FValuesEnum { get; set; } = FValuesEnum.Square_Function;

        public double MinRes { get; set; } = 0.5;

        public int NodesSpline { get; set; } = 4;

        public int NewNodesSpline { get; set; } = 10;

        public int MaxIter { get; set; } = 100;

        public V1DataArray? DataArray { get; set; }

        public SplineData? DataSpline { get; set; }

        public PlotModel? DataPlot { get; set; }

        public string Error { get; }

        public string this[string property]
        {
            get
            {
                string message = string.Empty;
                switch (property)
                {
                    case "NodesX":
                        if (NodesX < 3)
                        {
                            message = "Число узлов сетки, на которой заданы дискретные значения функции, должно быть " +
                                      "больше или равно 3!";
                        }
                        break;
                    case "NodesSpline":
                        if (NodesSpline < 3 || NodesSpline > NodesX)
                        {
                            message = "Число узлов сглаживающего сплайна должно быть больше или равно 2 и меньше или " +
                                      "равно числу заданных дискретных значений функции!";
                        }
                        break;
                    case "NewNodesSpline":
                        if (NewNodesSpline <= 3)
                        {
                            message = "Число узлов равномерной сетки, на которой вычисляются значения сплайна, должно " +
                                      "быть больше 3!";
                        }
                        break;
                    case "LeftBorder":
                        if (LeftBorder > RightBorder)
                        {
                            message = "Левый конец отрезка, на котором заданы дискретные значения функции, должен быть " +
                                      "меньше, чем правый конец отрезка!";
                        }
                        break;
                }
                return message;
            }
        }

        public ICommand DataFromControls_Command { get; private set; }

        public ICommand DataFromFile_Command { get; private set; }

        public ICommand Save_Command { get; private set; }

        public ViewData(IErrorSender errorSender, IFileDialog fileDialog)
        {
            DataArray = new V1DataArray("array", DateTime.Now);
            this.errorSender = errorSender;
            this.fileDialog = fileDialog;
            DataFromControls_Command = new Commands(o => { DataFromControls_Execute(); }, o => DataFromControls_CanExecute());
            DataFromFile_Command = new Commands(o => { DataFromFile_Execute(); }, o => DataFromFile_CanExecute());
            Save_Command = new Commands(o => { Save_Execute(); }, o => Save_CanExecute());
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DataFromControls_Execute()
        {
            try
            {
                DataFromControls();
                RaisePropertyChanged("NodesSpline");
                bool result = GetSpline();
                Spline_Plot();
                RaisePropertyChanged("DataArray");
                RaisePropertyChanged("DataSpline");
                RaisePropertyChanged("DataPlot");
            }
            catch (Exception ex)
            {
                errorSender.SendError(ex.Message);
            }
        }

        private bool DataFromControls_CanExecute()
        {
            return string.IsNullOrEmpty(this[nameof(LeftBorder)]) && string.IsNullOrEmpty(this[nameof(RightBorder)]) &&
                   string.IsNullOrEmpty(this[nameof(NodesX)]) && string.IsNullOrEmpty(this[nameof(NodesSpline)]) &&
                   string.IsNullOrEmpty(this[nameof(NewNodesSpline)]);
        }

        private void DataFromFile_Execute()
        {
            try 
            {
                string filename = fileDialog.OpenFileDialog();
                if (!string.IsNullOrEmpty(filename))
                {
                    Load(filename);
                    RaisePropertyChanged("LeftBorder");
                    RaisePropertyChanged("RightBorder");
                    RaisePropertyChanged("NodesX");
                    RaisePropertyChanged("NodesSpline");
                    RaisePropertyChanged("DataArray");
                }
                if (Save_CanExecute() && string.IsNullOrEmpty(this[nameof(NodesSpline)]))
                {
                    bool result = GetSpline();
                    Spline_Plot();
                    RaisePropertyChanged("DataSpline");
                    RaisePropertyChanged("DataPlot");
                }
            }
            catch (Exception ex)
            {
                errorSender.SendError(ex.Message);
            }
        }

        private bool DataFromFile_CanExecute()
        {
            return string.IsNullOrEmpty(this[nameof(NodesSpline)]) && string.IsNullOrEmpty(this[nameof(NewNodesSpline)]);
        } 

        private void Save_Execute()
        {
            try
            {
                string filename = fileDialog.SaveFileDialog();
                if (!string.IsNullOrEmpty(filename))
                    Save(filename);
            }
            catch (Exception ex)
            {
                errorSender.SendError(ex.Message);
            }
        }

        private bool Save_CanExecute()
        {
            return string.IsNullOrEmpty(this[nameof(LeftBorder)]) && string.IsNullOrEmpty(this[nameof(RightBorder)]) && 
                   string.IsNullOrEmpty(this[nameof(NodesX)]);
        }

        public void Save(string filename)
        {
            try
            {
                DataArray.Save(filename);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Load(string filename)
        {
            try
            {
                V1DataArray dataArray = new V1DataArray("Array", DateTime.Now);
                V1DataArray.Load(filename, ref dataArray);
                DataArray = dataArray;

                LeftBorder = dataArray.X[0];
                RightBorder = dataArray.X[dataArray.X.Length - 1];
                NodesX = dataArray.X.Length;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DataFromControls()
        {
            try
            {
                FValues fValues = enumToFunc(FValuesEnum);
                if (UniformGrid)
                    DataArray = new V1DataArray("array", DateTime.Now, NodesX, LeftBorder, RightBorder, fValues);
                else
                {

                    double[] x = new double[NodesX];
                    x[0] = LeftBorder; x[NodesX-1] = RightBorder;
                    double segment = (RightBorder - LeftBorder) / (NodesX-2);
                    double pointer = LeftBorder;
                    Random rnd = new Random();
                    for (int i=1;i<NodesX-1;i++)
                    {
                        x[i] = pointer + rnd.NextDouble() * segment;
                        pointer += segment;
                    }
                    DataArray = new V1DataArray("array", DateTime.Now, x, fValues);
                }
            }
            catch (Exception)
            {
                throw new Exception("Неправильный формат ввода!"); ;
            }
        }

        public FValues enumToFunc(FValuesEnum fValuesEnum)
        {
            FValues fValues = fValuesEnum switch
            {
                FValuesEnum.Linear_Function => Delegates.Linear_Function,
                FValuesEnum.Square_Function => Delegates.Square_Function,
                FValuesEnum.Cubic_Function => Delegates.Cubic_Function,
                FValuesEnum.FA => Delegates.FA,
                _ => Delegates.Linear_Function
            };
            return fValues;
        }

        public bool GetSpline()
        {
            try
            {
                DataSpline = new SplineData(DataArray, NodesSpline, NewNodesSpline, MaxIter, MinRes);
                return DataSpline.GetSmoothingSpline();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Spline_Plot()
        {
            try
            {
                var oxyPlotModel = new OxyPlotModel(DataSpline);
                this.DataPlot = oxyPlotModel.plotModel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в построении графика:\n" + ex.Message);
            }
        }

    }
}
