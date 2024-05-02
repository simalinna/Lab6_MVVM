using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class V1DataList : V1Data
    {

        public List<DataItem> ListDataItem { get; set; }

        public string ThisString
        {
            get
            {
                return this.ToLongString("{0:f4}");
            }
        }

        public V1DataList(string key, DateTime date) : base(key, date)
        {
            ListDataItem = new List<DataItem>();
        }

        public V1DataList(string key, DateTime date, double[] x, FDI F) : base(key, date)
        {
            ListDataItem = new List<DataItem>();
            foreach (double item in x)
            {
                if (!search(ListDataItem, item))
                {
                    ListDataItem.Add(F(item));
                }
            }
        }

        private bool search(List<DataItem> list, double x)
        {
            foreach (DataItem item in list)
            {
                if (x == item.X) return true;
            }
            return false;
        }

        public override double MaxDistance
        {
            get
            {
                double min = ListDataItem[0].X, max = min;
                foreach (DataItem item in ListDataItem)
                {
                    if (item.X < min) min = item.X;
                    if (item.X > max) max = item.X;
                }
                return max - min;
            }
        }

        public static explicit operator V1DataArray(V1DataList source)
        {
            double[] X = new double[source.ListDataItem.Count];
            double[][] Y = new double[2][];
            Y[0] = new double[X.Length];
            Y[1] = new double[X.Length];
            for (int i = 0; i < X.Length; i++)
            {
                X[i] = source.ListDataItem[i].X;
                Y[0][i] = source.ListDataItem[i].Y1;
                Y[1][i] = source.ListDataItem[i].Y2;
            }
            V1DataArray res = new V1DataArray("Array_" + source.Key[5], source.DateTime);
            res.X = new double[source.ListDataItem.Count];
            res.Y = new double[2][];
            res.Y[0] = new double[X.Length];
            res.Y[1] = new double[X.Length];
            X.CopyTo(res.X, 0);
            Y.CopyTo(res.Y, 0);
            return res;
        }

        public override string ToString()
        {
            return $"Type:      V1DataList\n{base.ToString()}\nListCount: {ListDataItem.Count}\n\n";
        }

        public override string ToLongString(string format)
        {
            string str_res = this.ToString() + "List:\n\n";
            if (this.Count() == 0) return $"{str_res}NULL\n\n";
            foreach (DataItem item in ListDataItem)
            {
                str_res += item.ToLongString(format);
            }
            return str_res;
        }

        public override bool IsNull
        {
            get
            {
                foreach (DataItem item in ListDataItem)
                {
                    if (item.Y1 == item.Y2 && item.Y1 == 0) return true;
                }
                return false;
            }
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            foreach(DataItem item in ListDataItem)
            {
                yield return item;
            }
        }

    }
}
