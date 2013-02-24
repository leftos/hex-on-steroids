#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

#region Using Directives

using System.Collections.ObjectModel;
using System.ComponentModel;
using HexOnSteroids.Annotations;

#endregion

namespace HexOnSteroids
{
    public class CustomProfile : INotifyPropertyChanged
    {
        public CustomProfile()
        {
            AutoDetectValueCount = 1;
            Files = new ObservableCollection<string>();
            Ranges = new BindingList<CustomDataRange>();
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private string _name;

        public RangeType TypeOfRange
        {
            get { return _typeOfRange; }
            set
            {
                _typeOfRange = value;
                OnPropertyChanged("TypeOfRange");
            }
        }

        private RangeType _typeOfRange;

        public int AutoDetectValueCount
        {
            get { return _autoDetectValueCount; }
            set
            {
                _autoDetectValueCount = value;
                OnPropertyChanged("AutoDetectValueCount");
            }
        }

        private int _autoDetectValueCount;

        public TypeOfValues AutoDetectValueType
        {
            get { return _autoDetectValueType; }
            set
            {
                _autoDetectValueType = value;
                OnPropertyChanged("AutoDetectValueType");
            }
        }

        private TypeOfValues _autoDetectValueType;

        public Endianness EndiannessType
        {
            get { return _endiannessType; }
            set
            {
                _endiannessType = value;
                OnPropertyChanged("EndiannessType");
            }
        }

        private Endianness _endiannessType;

        public ObservableCollection<string> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                OnPropertyChanged("Files");
            }
        }

        private ObservableCollection<string> _files;

        public BindingList<CustomDataRange> Ranges
        {
            get { return _ranges; }
            set
            {
                _ranges = value;
                OnPropertyChanged("Ranges");
            }
        }

        private BindingList<CustomDataRange> _ranges;

        public bool UseBounds
        {
            get { return _useBounds; }
            set
            {
                _useBounds = value;
                OnPropertyChanged("UseBounds");
            }
        }

        private bool _useBounds;

        public object LowerBound
        {
            get { return _lowerBound; }
            set
            {
                _lowerBound = value;
                OnPropertyChanged("LowerBound");
            }
        }

        private object _lowerBound;

        public object UpperBound
        {
            get { return _upperBound; }
            set
            {
                _upperBound = value;
                OnPropertyChanged("UpperBound");
            }
        }

        private object _upperBound;

        public bool UseAbsolute
        {
            get { return _useAbsolute; }
            set
            {
                _useAbsolute = value;
                OnPropertyChanged("UseAbsolute");
            }
        }

        private bool _useAbsolute;

        public object AbsoluteValue
        {
            get { return _absoluteValue; }
            set
            {
                _absoluteValue = value;
                OnPropertyChanged("AbsoluteValue");
            }
        }

        private object _absoluteValue;

        public bool IgnoreCRC
        {
            get { return _ignoreCRC; }
            set
            {
                _ignoreCRC = value;
                OnPropertyChanged("IgnoreCRC");
            }
        }

        private bool _ignoreCRC;

        public string AutoDetectCustomHeader
        {
            get { return _autoDetectCustomHeader; }
            set
            {
                _autoDetectCustomHeader = value;
                OnPropertyChanged("AutoDetectCustomHeader");
            }
        }

        private string _autoDetectCustomHeader;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CustomDataRange : INotifyPropertyChanged
    {
        public CustomDataRange()
        {
        }

        public CustomDataRange(long start, long count, bool autoEnd, long end, TypeOfValues typeOfValues, string name = "")
        {
            Name = name;
            Start = start;
            Count = count;
            AutoEnd = autoEnd;
            End = end;
            Type = typeOfValues;
        }


        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private string _name;

        public long Start
        {
            get { return _start; }
            set
            {
                _start = value;
                OnPropertyChanged("Start");
                calculateEnd();
            }
        }

        private long _start;

        public long Count
        {
            get { return _count; }
            set
            {
                _count = value;
                OnPropertyChanged("Count");
                calculateEnd();
            }
        }

        private long _count;

        public bool AutoEnd
        {
            get { return _autoEnd; }
            set
            {
                _autoEnd = value;
                OnPropertyChanged("AutoEnd");
                calculateEnd();
            }
        }

        private void calculateEnd()
        {
            if (_autoEnd)
            {
                _end = _start + (_count * Shader.SizeOf(_type)) - 1;
                OnPropertyChanged("End");
            }
            else
            {
                _count = (_end - _start + 1)/Shader.SizeOf(_type);
                OnPropertyChanged("Count");
            }
        }

        private bool _autoEnd;

        public long End
        {
            get { return _end; }
            set
            {
                _end = value;
                OnPropertyChanged("End");
                _autoEnd = false;
                OnPropertyChanged("AutoEnd");
            }
        }

        private long _end;

        public TypeOfValues Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        private TypeOfValues _type;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum RangeType
    {
        AutoDetectCustomHeader,
        AutoDetectShaders,
        WholeFile,
        Custom
    }
}