// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace EtwKql
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// This class is a view, that makes IEnumerable collection of dynamic objects look as data reader
    /// It is by no means complete or correct implementation of IDataReader as only the 
    /// methods called by Kusto are implemented
    /// </summary>
    public class DictionaryDataReader : IDataReader
    {
        IEnumerator<IDictionary<string, object>> _enumerator;
        bool _isclosed = false;

        public DictionaryDataReader(IEnumerable<IDictionary<string, object>> data)
        {
            _enumerator = data.GetEnumerator();
        }
        public object this[int i] => throw new NotImplementedException();

        public object this[string name] => throw new NotImplementedException();

        public int Depth => throw new NotImplementedException();

        public bool IsClosed => _isclosed;

        public int RecordsAffected => throw new NotImplementedException();

        public int FieldCount => _enumerator.Current.Keys.Count;

        public void Close()
        {
            ;
        }

        public void Dispose()
        {
            ;
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            throw new NotImplementedException();
        }

        public int GetValues(object[] values)
        {
            var row = _enumerator.Current.Values;
            int i = 0;
            foreach (var v in row)
            {
                values[i] = v;
                i++;
            }
            return i;
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            bool result = _enumerator.MoveNext();
            _isclosed = !result;

            return result;
        }
    }
}
