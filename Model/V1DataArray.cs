using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Formats.Asn1;

namespace Model
{
    public class V1DataArray : V1Data
    {

        public double[] X { get; set; }

        public double[][] Y { get; set; }

        public string ThisString
        {
            get
            {
                return this.ToLongString("{0:f4}");
            }
        }

        public V1DataArray(string key, DateTime date): base(key, date)
        {
            X = new double[0];
            Y = new double[2][];
        }

        public V1DataArray(string key, DateTime date, double[] x, FValues F) : base(key, date)
        {
            X = new double[x.Length];
            x.CopyTo(X, 0);
            Y = new double[2][];
            Y[0] = new double[x.Length];
            Y[1] = new double[x.Length];
            for (int i = 0; i < X.Length; i++)
            {
                F(X[i], ref Y[0][i], ref Y[1][i]);
            }
        }

        public V1DataArray(string key, DateTime date, int nX, double xL, double xR, FValues F): base(key, date)
        {
            X = new double[nX];
            Y = new double[2][];
            Y[0] = new double[X.Length];
            Y[1] = new double[X.Length];
            double step = (xR - xL) / (nX - 1);
            double value = xL;
            for (int i = 0; i < nX; i++)
            {
                X[i] = value;
                F(X[i], ref Y[0][i], ref Y[1][i]);
                value += step;
            }
        }

        public double[] this[int index]
        {
            get => Y[index];
        }

        public V1DataList V1DataList
        {
            get
            {
                V1DataList res = new V1DataList(Key, DateTime);
                for (int i=0;i<X.Length;i++)
                {
                    res.ListDataItem[i] = new DataItem(X[i], Y[0][i], Y[1][i]);
                }
                return res;
            }
        }

        public override double MaxDistance
        {
            get 
            { 
                return X.Max()-X.Min();
            }
        }

        public override string ToString()
        {
            return $"Type:      V1DataArray\n{base.ToString()}\n\n";
        }

        public override string ToLongString(string format)
        {
            string str_res = this.ToString() + "Arrays:\n\n";
            if (X.Length==0) return $"{str_res}NULL\n\n";
            for (int i=0;i<X.Length;i++)
            {
                str_res += $"X:  {string.Format(format, X[i])}\n";
                str_res += $"Y1: {string.Format(format, Y[0][i])}\n";
                str_res += $"Y2: {string.Format(format, Y[1][i])}\n\n";
            }
            return str_res;
        }

        public override bool IsNull
        {
            get
            {
                for (int i=0;i<X.Length;i++)
                {
                    if (Y[0][i] == Y[1][i] && Y[0][i] == 0) return true;
                }
                return false;
            }
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            for (int i=0;i<X.Length;i++)
            {
                yield return new DataItem(X[i], Y[0][i], Y[1][i]);
            }
        }
        
        public bool Save(string filename)
        {
            StreamWriter? writer = null;
            try
            {
                writer = new StreamWriter(filename, false);
                writer.WriteLine(Key);
                writer.WriteLine(DateTime.ToString());
                for (int i = 0; i < X.Length; i++)
                {
                    writer.WriteLine(X[i].ToString() + ' ' + Y[0][i].ToString() + ' ' + Y[1][i].ToString());
                }
                Console.WriteLine($"{Key} saved to file!\n\n");
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception in function Save: {ex.Message}");
                return false;
            }
            finally
            {
                if (writer != null) writer.Dispose();
            }
        }

        public static bool Load(string filename, ref V1DataArray V1)
        {
            StreamReader? reader = null;
            try
            {
                reader = new StreamReader(filename);
                string? str;
                int arr_len = 0;
                while ((str = reader.ReadLine()) != null)
                {
                    arr_len++;
                }
                arr_len -= 2;
                //reader.DiscardBufferedData();
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                if ((str = reader.ReadLine()) != null)
                {
                    V1.Key = str;
                }
                if ((str = reader.ReadLine()) != null)
                {
                    V1.DateTime = DateTime.Parse(str);
                }
                V1.X = new double[arr_len];
                V1.Y = new double[2][];
                V1.Y[0] = new double[arr_len];
                V1.Y[1] = new double[arr_len];
                int ind = 0;
                while ((str = reader.ReadLine()) != null)
                {
                    string[] items = str.Split(' ');
                    V1.X[ind] = Convert.ToDouble(items[0]);
                    V1.Y[0][ind] = Convert.ToDouble(items[1]);
                    V1.Y[1][ind] = Convert.ToDouble(items[2]);
                    ind++;
                }
                Console.WriteLine($"{V1.Key} loaded from file in {V1.Key}_load!\n\n");
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception in function Load: {ex.Message}");
                return false;
            }
            finally
            {
                if (reader != null) reader.Dispose();
            }
        }

    }
}
