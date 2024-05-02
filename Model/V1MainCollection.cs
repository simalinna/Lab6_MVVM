using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;  //??
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class V1MainCollection : System.Collections.ObjectModel.ObservableCollection<V1Data>
    {

        public bool Contains(string key)
        {
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i].Key == key) return true;
            }
            return false;
        }

        public bool Add(V1Data v1Data)
        {
            if (!this.Contains(v1Data.Key))
            {
                base.Add(v1Data);
                return true;
            }
            return false;
        }

        public V1MainCollection(int nV1DataArray, int nV1DataList)
        {
            //double[] x = new double[] { 1, -4};
            //FValues FA = Delegates.NULL_Array;
            FValues FA = Delegates.FA;
            FDI FL = Delegates.FL;
            Random rand = new Random();
            for (int i = 0; i < nV1DataArray; i++)
            {
                double[] x = new double[] { rand.Next(-10, 10)};
                V1DataArray DA = new V1DataArray($"Array_{i}", DateTime.Now, x, FA);
                this.Add(DA);
            }
            for (int i = 0; i < nV1DataList; i++)
            {
                double[] x = new double[] { rand.Next(1, 3)};
                //double[] x = new double[] { 3, rand.Next(-2, 2) };
                V1DataList DL = new V1DataList($"List_{i}", DateTime.Now, x, FL);
                this.Add(DL);
            }
        }

        public string ToLongString(string format)
        {
            string str_res = "MY COLLECTION:\n\n\n";
            foreach (V1Data v in this)
            {
                str_res += $"{v.ToLongString(format)}\n";
            }
            str_res += "END OF MY COLLECTION\n\n";
            return str_res;
        }

        public override string ToString()
        {
            string str_res = "";
            foreach (V1Data v in this)
            {
                str_res += $"{v.ToString}\n\n";
            }
            return str_res;
        }

        public List<bool> Is_Null
        {
            get 
            {
                List<bool> res=new List<bool>();
                foreach (V1Data v in this)
                {
                    if (v.IsNull) res.Add(true);
                    else res.Add(false);
                }
                return res;
            }
        }
        
        public double Avg_Y_Distance
        {
            get
            {
                if (this.Count()==0)
                {
                    return double.NaN;
                }
                else
                {
                    var Distance = (from collection in this
                               from item in collection
                               select Math.Sqrt(item.Y1 * item.Y1 + item.Y2 * item.Y2));
                    return Distance.Average();
                }
            }
        }
        
        public DataItem? Max_Dev
        {
            get 
            {
                if (this.Count() == 0)
                {
                    return null;
                }
                else
                {
                    var Dev = (from collection in this
                               from item in collection
                               select Math.Abs(Math.Sqrt(item.Y1 * item.Y1 + item.Y2 * item.Y2))-this.Avg_Y_Distance);
                    double Max_value = Dev.Max();
                    DataItem Max_Dev = (from collection in this
                                   from item in collection
                                   where Math.Abs(Math.Sqrt(item.Y1 * item.Y1 + item.Y2 * item.Y2)) - this.Avg_Y_Distance==Max_value
                                   select item).First();
                    return Max_Dev;
                }
            }
        }

        public IEnumerable<double>? X_repeats
        {
            get
            {
                if (this.Count() == 0)
                {
                    return null;
                }
                else
                {
                    var groupRes = (from collection in this
                                from dataitem in collection
                                group dataitem by dataitem.X);
                    var Res = (from item in groupRes
                               where item.Count() > 1
                               orderby item.Key
                               select item.Key);
                    if (Res.Any()) return Res;
                    else return null;
                }
            }
        }

    }
}
