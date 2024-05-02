using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using Model;
using OxyPlot.Series;
using OxyPlot.Legends;

namespace ViewModel
{
    public class OxyPlotModel
    {

        public PlotModel plotModel { get; private set; }

        public SplineData splineData { get; private set; }

        public OxyPlotModel(SplineData splineData)
        {
            this.splineData = splineData;
            this.plotModel = new PlotModel { Title = "Результат сплайн-апроксимации" };
            this.SplinePlot();
        }

        public void SplinePlot()
        {

            LineSeries lineSeries = new LineSeries();
            for (int i = 0; i < splineData.DataArray.X.Length; i++)
            {
                lineSeries.Points.Add(new DataPoint(splineData.DataArray.X[i], splineData.DataArray.Y[0][i]));
            }

            lineSeries.Title = "Заданные значения функции";
            lineSeries.Color = OxyColors.Purple;
            lineSeries.LineStyle = LineStyle.None;
            lineSeries.MarkerType = MarkerType.Circle;
            lineSeries.MarkerSize = 4;

            Legend leg = new Legend();
            this.plotModel.Legends.Add(leg);

            this.plotModel.Series.Add(lineSeries);

            lineSeries = new LineSeries();
            for (int i = 0; i < splineData.NewNodesCount; i++)
            {
                lineSeries.Points.Add(new DataPoint(splineData.SplineCoordinates[i].X, splineData.SplineCoordinates[i].Y));
            }

            lineSeries.Title = "Значения сплайна";
            lineSeries.Color = OxyColors.Magenta;
            lineSeries.MarkerSize = 4;

            this.plotModel.Series.Add(lineSeries);

        }

    }
}
