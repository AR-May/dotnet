// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//
//
// This file was generated, please do not edit it directly.
//
// Please see MilCodeGen.html for more information.
//

using System.Collections;
using System.Globalization;

namespace System.Windows.Media.Animation
{
    /// <summary>
    /// This collection is used in conjunction with a KeyFrameInt32Animation
    /// to animate a Int32 property value along a set of key frames.
    /// </summary>
    public class Int32KeyFrameCollection : Freezable, IList
    {
        #region Data

        private List<Int32KeyFrame> _keyFrames;
        private static Int32KeyFrameCollection s_emptyCollection;

        #endregion

        #region Constructors

        /// <Summary>
        /// Creates a new Int32KeyFrameCollection.
        /// </Summary>
        public Int32KeyFrameCollection()
            : base()
        {
            _keyFrames = new List< Int32KeyFrame>(2);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// An empty Int32KeyFrameCollection.
        /// </summary>
        public static Int32KeyFrameCollection Empty
        {
            get
            {
                if (s_emptyCollection == null)
                {
                    Int32KeyFrameCollection emptyCollection = new Int32KeyFrameCollection
                    {
                        _keyFrames = new List<Int32KeyFrame>(0)
                    };
                    emptyCollection.Freeze();

                    s_emptyCollection = emptyCollection;
                }

                return s_emptyCollection;
            }
        }

        #endregion

        #region Freezable

        /// <summary>
        /// Creates a freezable copy of this Int32KeyFrameCollection.
        /// </summary>
        /// <returns>The copy</returns>
        public new Int32KeyFrameCollection Clone()
        {
            return (Int32KeyFrameCollection)base.Clone();
        }

        /// <summary>
        /// Implementation of <see cref="System.Windows.Freezable.CreateInstanceCore">Freezable.CreateInstanceCore</see>.
        /// </summary>
        /// <returns>The new Freezable.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new Int32KeyFrameCollection();
        }

        /// <summary>
        /// Implementation of <see cref="System.Windows.Freezable.CloneCore(System.Windows.Freezable)">Freezable.CloneCore</see>.
        /// </summary>
        protected override void CloneCore(Freezable sourceFreezable)
        {
            Int32KeyFrameCollection sourceCollection = (Int32KeyFrameCollection) sourceFreezable;
            base.CloneCore(sourceFreezable);

            int count = sourceCollection._keyFrames.Count;

            _keyFrames = new List< Int32KeyFrame>(count);

            for (int i = 0; i < count; i++)
            {
                Int32KeyFrame keyFrame = (Int32KeyFrame)sourceCollection._keyFrames[i].Clone();
                _keyFrames.Add(keyFrame);
                OnFreezablePropertyChanged(null, keyFrame);
            }
        }


        /// <summary>
        /// Implementation of <see cref="System.Windows.Freezable.CloneCurrentValueCore(System.Windows.Freezable)">Freezable.CloneCurrentValueCore</see>.
        /// </summary>
        protected override void CloneCurrentValueCore(Freezable sourceFreezable)
        {
            Int32KeyFrameCollection sourceCollection = (Int32KeyFrameCollection) sourceFreezable;
            base.CloneCurrentValueCore(sourceFreezable);

            int count = sourceCollection._keyFrames.Count;

            _keyFrames = new List< Int32KeyFrame>(count);

            for (int i = 0; i < count; i++)
            {
                Int32KeyFrame keyFrame = (Int32KeyFrame)sourceCollection._keyFrames[i].CloneCurrentValue();
                _keyFrames.Add(keyFrame);
                OnFreezablePropertyChanged(null, keyFrame);
            }
        }


        /// <summary>
        /// Implementation of <see cref="System.Windows.Freezable.GetAsFrozenCore(System.Windows.Freezable)">Freezable.GetAsFrozenCore</see>.
        /// </summary>
        protected override void GetAsFrozenCore(Freezable sourceFreezable)
        {
            Int32KeyFrameCollection sourceCollection = (Int32KeyFrameCollection) sourceFreezable;
            base.GetAsFrozenCore(sourceFreezable);

            int count = sourceCollection._keyFrames.Count;

            _keyFrames = new List< Int32KeyFrame>(count);

            for (int i = 0; i < count; i++)
            {
                Int32KeyFrame keyFrame = (Int32KeyFrame)sourceCollection._keyFrames[i].GetAsFrozen();
                _keyFrames.Add(keyFrame);
                OnFreezablePropertyChanged(null, keyFrame);
            }
        }


        /// <summary>
        /// Implementation of <see cref="System.Windows.Freezable.GetCurrentValueAsFrozenCore(System.Windows.Freezable)">Freezable.GetCurrentValueAsFrozenCore</see>.
        /// </summary>
        protected override void GetCurrentValueAsFrozenCore(Freezable sourceFreezable)
        {
            Int32KeyFrameCollection sourceCollection = (Int32KeyFrameCollection) sourceFreezable;
            base.GetCurrentValueAsFrozenCore(sourceFreezable);

            int count = sourceCollection._keyFrames.Count;

            _keyFrames = new List< Int32KeyFrame>(count);

            for (int i = 0; i < count; i++)
            {
                Int32KeyFrame keyFrame = (Int32KeyFrame)sourceCollection._keyFrames[i].GetCurrentValueAsFrozen();
                _keyFrames.Add(keyFrame);
                OnFreezablePropertyChanged(null, keyFrame);
            }
        }

        /// <summary>
        ///
        /// </summary>
        protected override bool FreezeCore(bool isChecking)
        {
            bool canFreeze = base.FreezeCore(isChecking);

            for (int i = 0; i < _keyFrames.Count && canFreeze; i++)
            {
                canFreeze &= Freezable.Freeze(_keyFrames[i], isChecking);
            }

            return canFreeze;
        }

        #endregion

        #region IEnumerable

        /// <summary>
        /// Returns an enumerator of the Int32KeyFrames in the collection.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            ReadPreamble();

            return _keyFrames.GetEnumerator();
        }

        #endregion

        #region ICollection

        /// <summary>
        /// Returns the number of Int32KeyFrames in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                ReadPreamble();

                return _keyFrames.Count;
            }
        }

        /// <summary>
        /// See <see cref="System.Collections.ICollection.IsSynchronized">ICollection.IsSynchronized</see>.
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                ReadPreamble();

                return (IsFrozen || Dispatcher != null);
            }
        }

        /// <summary>
        /// See <see cref="System.Collections.ICollection.SyncRoot">ICollection.SyncRoot</see>.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                ReadPreamble();

                return ((ICollection)_keyFrames).SyncRoot;
            }
        }

        /// <summary>
        /// Copies all of the Int32KeyFrames in the collection to an
        /// array.
        /// </summary>
        void ICollection.CopyTo(Array array, int index)
        {
            ReadPreamble();

            ((ICollection)_keyFrames).CopyTo(array, index);
        }

        /// <summary>
        /// Copies all of the Int32KeyFrames in the collection to an
        /// array of Int32KeyFrames.
        /// </summary>
        public void CopyTo(Int32KeyFrame[] array, int index)
        {
            ReadPreamble();

            _keyFrames.CopyTo(array, index);
        }

        #endregion

        #region IList

        /// <summary>
        /// Adds a Int32KeyFrame to the collection.
        /// </summary>
        int IList.Add(object keyFrame)
        {
            return Add((Int32KeyFrame)keyFrame);
        }

        /// <summary>
        /// Adds a Int32KeyFrame to the collection.
        /// </summary>
        public int Add(Int32KeyFrame keyFrame)
        {
            ArgumentNullException.ThrowIfNull(keyFrame);

            WritePreamble();

            OnFreezablePropertyChanged(null, keyFrame);
            _keyFrames.Add(keyFrame);

            WritePostscript();

            return _keyFrames.Count - 1;
        }

        /// <summary>
        /// Removes all Int32KeyFrames from the collection.
        /// </summary>
        public void Clear()
        {
            WritePreamble();

            if (_keyFrames.Count > 0)
            {            
                for (int i = 0; i < _keyFrames.Count; i++)
                {
                    OnFreezablePropertyChanged(_keyFrames[i], null);
                }

                _keyFrames.Clear();

                WritePostscript();
            }
        }

        /// <summary>
        /// Returns true of the collection contains the given Int32KeyFrame.
        /// </summary>
        bool IList.Contains(object keyFrame)
        {
            return Contains((Int32KeyFrame)keyFrame);
        }

        /// <summary>
        /// Returns true of the collection contains the given Int32KeyFrame.
        /// </summary>
        public bool Contains(Int32KeyFrame keyFrame)
        {
            ReadPreamble();

            return _keyFrames.Contains(keyFrame);
        }

        /// <summary>
        /// Returns the index of a given Int32KeyFrame in the collection. 
        /// </summary>
        int IList.IndexOf(object keyFrame)
        {
            return IndexOf((Int32KeyFrame)keyFrame);
        }

        /// <summary>
        /// Returns the index of a given Int32KeyFrame in the collection. 
        /// </summary>
        public int IndexOf(Int32KeyFrame keyFrame)
        {
            ReadPreamble();

            return _keyFrames.IndexOf(keyFrame);
        }

        /// <summary>
        /// Inserts a Int32KeyFrame into a specific location in the collection. 
        /// </summary>
        void IList.Insert(int index, object keyFrame)
        {
            Insert(index, (Int32KeyFrame)keyFrame);
        }

        /// <summary>
        /// Inserts a Int32KeyFrame into a specific location in the collection. 
        /// </summary>
        public void Insert(int index, Int32KeyFrame keyFrame)
        {
            ArgumentNullException.ThrowIfNull(keyFrame);

            WritePreamble();

            OnFreezablePropertyChanged(null, keyFrame);
            _keyFrames.Insert(index, keyFrame);

            WritePostscript();
        }

        /// <summary>
        /// Returns true if the collection is frozen.
        /// </summary>
        public bool IsFixedSize
        {
            get
            {
                ReadPreamble();

                return IsFrozen;
            }
        }

        /// <summary>
        /// Returns true if the collection is frozen.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                ReadPreamble();

                return IsFrozen;
            }
        }

        /// <summary>
        /// Removes a Int32KeyFrame from the collection.
        /// </summary>
        void IList.Remove(object keyFrame)
        {
            Remove((Int32KeyFrame)keyFrame);
        }

        /// <summary>
        /// Removes a Int32KeyFrame from the collection.
        /// </summary>
        public void Remove(Int32KeyFrame keyFrame)
        {
            WritePreamble();

            if (_keyFrames.Contains(keyFrame))
            {
                OnFreezablePropertyChanged(keyFrame, null);
                _keyFrames.Remove(keyFrame);

                WritePostscript();
            }
        }

        /// <summary>
        /// Removes the Int32KeyFrame at the specified index from the collection.
        /// </summary>
        public void RemoveAt(int index)
        {
            WritePreamble();

            OnFreezablePropertyChanged(_keyFrames[index], null);
            _keyFrames.RemoveAt(index);

            WritePostscript();
        }

        /// <summary>
        /// Gets or sets the Int32KeyFrame at a given index.
        /// </summary>
        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (Int32KeyFrame)value;
            }
        }

        /// <summary>
        /// Gets or sets the Int32KeyFrame at a given index.
        /// </summary>
        public Int32KeyFrame this[int index]
        {
            get
            {
                ReadPreamble();

                return _keyFrames[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(String.Format(CultureInfo.InvariantCulture, "Int32KeyFrameCollection[{0}]", index));
                }

                WritePreamble();

                if (value != _keyFrames[index])
                {
                    OnFreezablePropertyChanged(_keyFrames[index], value);
                    _keyFrames[index] = value;

                    Debug.Assert(_keyFrames[index] != null);

                    WritePostscript();
                }
            }
        }

        #endregion
    }
}
