using System;
using System.Collections.Generic;
using System.Text;
using O2.Includes.DataBaseAccess;
using System.Xml.Serialization;
using System.IO;
using O2.Includes.Exceptions;
using System.Reflection;

namespace O2.Includes
{
    [Serializable]
    public class SysCollection<T> : IList<T> where T : SysObject
    {
        [NonSerialized, XmlIgnore, SoapIgnore]
        protected Query _query;
        [XmlIgnore, SoapIgnore]
        public Query query
        {
            get { return _query; }
            set { _query = value; }
        }

        public virtual string get_fk_column_name()
        {
            if (GetType().GetField("fk_column_name") == null)
                throw new CollectionWithoutFKColumnNameFieldException(GetType());

            return GetType().GetField("fk_column_name").GetValue(this).ToString();
        }

        protected int? _fk_id;
        public int? fk_id
        {
            get { return _fk_id; }
            set { _fk_id = value; }
        }

        private List<T> _innerList;
        protected List<T> innerList
        {
            get
            {
                if (_innerList == null)
                    _innerList = new List<T>();

                return _innerList;
            }

            set { _innerList = value; }
        }

        public SysCollection() : base() { }

        public SysCollection(int capacity)
        {
            innerList.Capacity = capacity;
        }

        public virtual void Bind_fk_id()
        {
            BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (SysObject obj in this)
                obj.GetType().GetField("_" + get_fk_column_name(), bf).SetValue(obj, Convert.ToInt32(GetType().GetField("_" + get_fk_column_name(), bf).GetValue(this)));
        }

        public virtual void InsertAll()
        {
            if (fk_id == 0)
                throw new Exception("fk_id = 0");

            Bind_fk_id();

            foreach (SysObject obj in this)
                if (obj.get_id_value() == 0)
                {
                    if (query != null)
                        obj.query = query;

                    obj.Insert();
                }

            DeleteNotIn();
        }

        public virtual void DeleteNotIn()
        {
            string ids = String.Empty;
            foreach (SysObject item in this)
                ids += "," + item.id;

            if (!String.IsNullOrEmpty(ids))
            {
                BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;
                query.AddParameter("@fk_id", Convert.ToInt32(GetType().GetField("_" + get_fk_column_name(), bf).GetValue(this)));
                query.ExecuteNonQuery("DELETE FROM " + this[0].get_table() + " WHERE " + get_fk_column_name() + " = @fk_id AND id NOT IN (" + ids.Substring(1) + ")");
            }
        }

        #region IList<T> Members
        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public virtual void Insert(int index, T item)
        {
            innerList.Insert(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            innerList.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return innerList[index]; }
            set { innerList[index] = value; }
        }
        #endregion

        #region ICollection<T> Members
        public virtual void Add(T item)
        {
            innerList.Add(item);
        }

        public void Clear()
        {
            innerList.Clear();
        }

        public virtual bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public virtual bool Remove(T item)
        {
            bool result = false;
            for (int i = 0; i < innerList.Count; i++)
                if (((SysObject)innerList[i]).get_id_value() == ((SysObject)item).get_id_value())
                {
                    RemoveAt(i);
                    result = true;
                    break;
                }

            return result;
        }
        #endregion

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
        #endregion

        public string SerializeXML()
        {
            return SerializeXML(this);
        }

        public static string SerializeXML(Object _obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(_obj.GetType());

            MemoryStream ms = new MemoryStream();
            xmlSerializer.Serialize(ms, _obj);

            ms.Seek(0, SeekOrigin.Begin);
            using (StreamReader sr = new StreamReader(ms))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
