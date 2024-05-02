using Model;
using Xunit;

namespace Model_Tests
{
    public class DataArray_Tests
    {
        [Fact]
        public void LoadFromFile()
        {
            string filename = @"..\\..\\..\\..\\TestFiles\\DataArray_file_test.txt";

            V1DataArray array = new V1DataArray("Array", DateTime.Now);
            V1DataArray.Load(filename, ref array);

            DateTime date = new DateTime(2023, 11, 16, 9, 33, 01);
            double[] x = new double[] { 0.5, 16, -1 };
            double[][] y = new double[2][]; 
            y[0] = new double[] { 3.5, 19, 2 };
            y[1] = new double[] { 2, 64, -4 };

            Assert.Equal("Array_1", array.Key);
            Assert.Equal(date, array.DateTime);
            Assert.Equal(x, array.X);
            Assert.Equal(y, array.Y);
        }
     
    }
}