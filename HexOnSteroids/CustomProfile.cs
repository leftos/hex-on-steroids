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

#endregion

namespace HexOnSteroids
{
    public class CustomProfile
    {
        public CustomProfile()
        {
            AutoDetectValueCount = 1;
            Files = new ObservableCollection<string>();
            Ranges = new ObservableCollection<CustomDataRange>();
        }

        public string Name { get; set; }
        public RangeType RangeType { get; set; }
        public int AutoDetectValueCount { get; set; }
        public TypeOfValues AutoDetectValueType { get; set; }
        public Endianness Endianness { get; set; }
        public ObservableCollection<string> Files { get; set; }
        public ObservableCollection<CustomDataRange> Ranges { get; set; }
        public bool UseBounds { get; set; }
        public object LowerBound { get; set; }
        public object UpperBound { get; set; }
        public bool UseAbsolute { get; set; }
        public object AbsoluteValue { get; set; }
        public bool IgnoreCRC { get; set; }
        public string AutoDetectCustomHeader { get; set; }
    }

    public class CustomDataRange
    {
        public CustomDataRange()
        {
        }

        public CustomDataRange(long start, long end, TypeOfValues typeOfValues, string name = "")
        {
            Name = name;
            Start = start;
            End = end;
            Type = typeOfValues;
        }

        public string Name { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public TypeOfValues Type { get; set; }
    }

    public enum RangeType
    {
        AutoDetectCustomHeader,
        AutoDetectShaders,
        WholeFile,
        Custom
    }
}