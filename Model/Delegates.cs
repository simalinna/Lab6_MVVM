using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public delegate void FValues(double x, ref double y1, ref double y2);

	public delegate DataItem FDI(double x);

    public static class Delegates
    {
        public static DataItem FL(double x)
        {
            return new DataItem(x, x+2, x*3);
        }

        public static void Linear_Function(double x, ref double y1, ref double y2)
        {
            y1 = x;
            y2 = x;
        }

        public static void Square_Function(double x, ref double y1, ref double y2)
        {
            y1 = x * x;
            y2 = x * x;
        }

        public static void Cubic_Function(double x, ref double y1, ref double y2)
        {
            y1 = x * x * x;
            y2 = x * x * x;
        }

        public static void FA(double x, ref double y1, ref double y2)
        {
            y1 = x + 3;
            y2 = x * 4;
        }

        public static DataItem NULL_List(double x)
        {
            return new DataItem(x, 0, 0);
        }

        public static void NULL_Array (double x, ref double y1, ref double y2)
        {
            y1 = 0;
            y2 = 0;
        }

    }

}
