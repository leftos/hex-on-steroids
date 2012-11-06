using System.Collections.ObjectModel;

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