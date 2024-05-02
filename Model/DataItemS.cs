using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public struct DataItemS
    {
        public double X { get; set; }
        public double Y { get; set; }

        public DataItemS(double x, double y)
        {
            X = x; Y = y; 
        }

        public override string ToString()
        {
            return $"X:             {string.Format("{0:f10}", X)}\nY:             {string.Format("{0:f10}", Y)}\n";
        }
    }
}
