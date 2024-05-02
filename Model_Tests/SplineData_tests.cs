using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Model_Tests
{
    public class SplineData_tests
    {

        [Fact]
        public void CreateSpline()
        {
            double[] x = new double[] { -1, 0.5, 4, 16, 17, 18.3 };
            FValues F_Array = Delegates.Square_Function;

            V1DataArray array = new V1DataArray("Array", DateTime.Now, x, F_Array);
            SplineData spline = new SplineData(array, 4, 50, 100, 0.1);

            bool res = spline.GetSmoothingSpline();

            Assert.True(res);
            Assert.Equal(6, spline.SplineDataList.Count);
            Assert.Equal(50, spline.SplineCoordinates.Count);

        }
    }
}
