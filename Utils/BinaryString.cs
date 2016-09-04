using System;
using System.Collections.Generic;
using System.Text;

namespace O2.Utils
{
    public class BinaryString
    {
        private string _values = "";
        public string values
        {
            get { return _values; }
            set { _values = value; }
        }

        public void AddValue(int pos, int value)
        {
            if (pos > (values.Length - 1))
                values = values.PadRight((pos + 1), '0');

            if (pos > 0)
                values = values.Substring(0, pos) + value.ToString() + values.Substring(pos + 1);
            else if (values.Length > 1)
                values = value.ToString() + values.Substring(pos + 1);
            else
                values = value.ToString();

        }

        public static byte[] StringToArray(string str)
        {
            byte[] result = new byte[str.Length];

            for (int i = 0; i < str.Length; i++)
                result[i] = Convert.ToByte(str.Substring(i, 1));

            return result;
        }
    }
}
