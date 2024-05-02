using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    abstract public class V1Data: IEnumerable<DataItem>
    {

        public string Key { get; set; }
        public DateTime DateTime { get; set; }

        public V1Data(string key, DateTime date)
        {
            Key = key;
            DateTime = date;
        }

        public abstract double MaxDistance { get; }

        public abstract string ToLongString(string format);

        public override string ToString()
        {
            return $"Key:       {Key}\nDateTime:  {DateTime}";
        }

        public abstract bool IsNull { get; }

        public abstract IEnumerator<DataItem> GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return GetEnumerator();
        }
        
    }
}

