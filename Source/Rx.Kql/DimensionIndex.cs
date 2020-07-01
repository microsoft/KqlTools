// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;

    /// <summary>
    ///     Class that manages memory for multi-dimensional aggregation
    ///     Example is 2D matrics in which every cell has 2 coordinates
    /// </summary>
    /// <typeparam name="T">Cell value type </typeparam>
    public class DimensionIndex<T>
    {
        private readonly object thisLock = new object();

        private readonly Dictionary<int, List<Cell>> _index = new Dictionary<int, List<Cell>>();

        /// <summary>
        ///     Find a cell by vector of coordinates
        /// </summary>
        /// <param name="dimensionValues">coordinates</param>
        /// <returns>The Cell wrapper object for the cell. The property Value is the actual value</returns>
        public Cell FindCell(string[] dimensionValues)
        {
            int hash = Cell.GetHash(dimensionValues);

            lock (thisLock)
            {
                //Try finding existing cell
                List<Cell> sameHash = null;
                if (_index.TryGetValue(hash, out sameHash))
                {
                    if (sameHash.Count == 1)
                    {
                        return sameHash[0];
                    }

                    foreach (var cell in sameHash)
                    {
                        if (cell.Equals(dimensionValues))
                        {
                            return cell;
                        }
                    }
                }

                // Add new cell
                if (sameHash == null)
                {
                    sameHash = new List<Cell>();
                    _index.Add(hash, sameHash);
                }

                var vector = new Cell(dimensionValues);
                sameHash.Add(vector);
                return vector;
            }
        }

        public bool ValuesArePresent()
        {
            return _index.Values.Count > 0;
        }

        public IEnumerable<Cell> GetAllCells()
        {
            foreach (var list in _index.Values)
            {
                foreach (var cell in list)
                {
                    yield return cell;
                }
            }
        }

        /// <summary>
        ///     Wrapper class to represent a cell in the multi-dimensional space
        /// </summary>
        public class Cell
        {
            /// <summary>
            ///     Actual value at that location
            /// </summary>
            public T Value { get; set; }

            public static int GetHash(string[] vector)
            {
                int hash = 0;
                for (int i = 0; i < vector.Length; i++)
                {
                    hash += vector[i].GetHashCode();
                }
                return hash;
            }

            private readonly string[] _vector;

            private readonly int _hash;

            public Cell(string[] vector)
            {
                _vector = vector;
                _hash = GetHash(vector);
            }

            public override int GetHashCode()
            {
                return _hash;
            }

            public override bool Equals(object obj)
            {
                Cell other = obj as Cell;
                if (other != null)
                {
                    return EqualsStringVector(other);
                }

                string[] array = obj as string[];
                if (array == null)
                {
                    return false;
                }

                return EqualsArray(array);
            }

            private bool EqualsStringVector(Cell other)
            {
                if (other._hash != this._hash)
                {
                    return false;
                }

                if (other._vector.Length != _vector.Length)
                {
                    return false;
                }

                for (int i = 0; i < _vector.Length; i++)
                {
                    if (_vector[i] != other._vector[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool EqualsArray(string[] array)
            {
                if (array.Length != _vector.Length)
                {
                    return false;
                }

                for (int i = 0; i < _vector.Length; i++)
                {
                    if (_vector[i] != array[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}