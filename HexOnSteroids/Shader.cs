using System;
using System.Collections.Generic;

namespace HexOnSteroids
{
    public class Shader
    {
        public string Name;
        private Endianness endianness;

        public Shader(TypeOfValues type = TypeOfValues.Double, Endianness endianness = Endianness.Little, string name = "")
        {
            Length = 0;
            Name = name;
            this.endianness = endianness;
            switch (type)
            {
                case TypeOfValues.Double:
                    valuesD = new List<double>();
                    typeOfValues = TypeOfValues.Double;
                    break;
                case TypeOfValues.Float:
                    valuesF = new List<float>();
                    typeOfValues = TypeOfValues.Float;
                    break;
                case TypeOfValues.Byte:
                    valuesB = new List<byte>();
                    typeOfValues = TypeOfValues.Byte;
                    break;
                case TypeOfValues.Int16:
                    valuesI16 = new List<short>();
                    typeOfValues = TypeOfValues.Int16;
                    break;
                case TypeOfValues.Int32:
                    valuesI32 = new List<int>();
                    typeOfValues = TypeOfValues.Int32;
                    break;
                case TypeOfValues.Int64:
                    valuesI64 = new List<long>();
                    typeOfValues = TypeOfValues.Int64;
                    break;
                case TypeOfValues.UInt16:
                    valuesUI16 = new List<ushort>();
                    typeOfValues = TypeOfValues.UInt16;
                    break;
                case TypeOfValues.UInt32:
                    valuesUI32 = new List<uint>();
                    typeOfValues = TypeOfValues.UInt32;
                    break;
                case TypeOfValues.UInt64:
                    valuesUI64 = new List<ulong>();
                    typeOfValues = TypeOfValues.UInt64;
                    break;
            }
        }

        public TypeOfValues typeOfValues { get; private set; }

        public int Length { get; private set; }
        public long Start { get; set; }

        private List<double> valuesD { get; set; }
        private List<float> valuesF { get; set; }
        private List<byte> valuesB { get; set; }
        private List<short> valuesI16 { get; set; }
        private List<int> valuesI32 { get; set; }
        private List<long> valuesI64 { get; set; }
        private List<ushort> valuesUI16 { get; set; }
        private List<uint> valuesUI32 { get; set; }
        private List<ulong> valuesUI64 { get; set; }

        public bool AddValue(object value)
        {
            try
            {
                switch (typeOfValues)
                {
                    case TypeOfValues.Double:
                        valuesD.Add(Convert.ToDouble(value));
                        break;
                    case TypeOfValues.Float:
                        valuesF.Add(Convert.ToSingle(value));
                        break;
                    case TypeOfValues.Byte:
                        valuesB.Add(Convert.ToByte(value));
                        break;
                    case TypeOfValues.Int16:
                        valuesI16.Add(Convert.ToInt16(value));
                        break;
                    case TypeOfValues.Int32:
                        valuesI32.Add(Convert.ToInt32(value));
                        break;
                    case TypeOfValues.Int64:
                        valuesI64.Add(Convert.ToInt64(value));
                        break;
                    case TypeOfValues.UInt16:
                        valuesUI16.Add(Convert.ToUInt16(value));
                        break;
                    case TypeOfValues.UInt32:
                        valuesUI32.Add(Convert.ToUInt32(value));
                        break;
                    case TypeOfValues.UInt64:
                        valuesUI64.Add(Convert.ToUInt64(value));
                        break;
                }
                Length++;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool InsertValue(int index, object value)
        {
            try
            {
                switch (typeOfValues)
                {
                    case TypeOfValues.Double:
                        valuesD.Insert(index, Convert.ToDouble(value));
                        break;
                    case TypeOfValues.Float:
                        valuesF.Insert(index, Convert.ToSingle(value));
                        break;
                    case TypeOfValues.Byte:
                        valuesB.Insert(index, Convert.ToByte(value));
                        break;
                    case TypeOfValues.Int16:
                        valuesI16.Insert(index, Convert.ToInt16(value));
                        break;
                    case TypeOfValues.Int32:
                        valuesI32.Insert(index, Convert.ToInt32(value));
                        break;
                    case TypeOfValues.Int64:
                        valuesI64.Insert(index, Convert.ToInt64(value));
                        break;
                    case TypeOfValues.UInt16:
                        valuesUI16.Insert(index, Convert.ToUInt16(value));
                        break;
                    case TypeOfValues.UInt32:
                        valuesUI32.Insert(index, Convert.ToUInt32(value));
                        break;
                    case TypeOfValues.UInt64:
                        valuesUI64.Insert(index, Convert.ToUInt64(value));
                        break;
                }
                Length++;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ChangeValue(int index, object value)
        {
            try
            {
                switch (typeOfValues)
                {
                    case TypeOfValues.Double:
                        double valD = Convert.ToDouble(value);
                        if (!Double.IsNaN(valD))
                            valuesD[index] = valD;
                        break;
                    case TypeOfValues.Float:
                        float valF = Convert.ToSingle(value);
                        if (!Single.IsNaN(valF))
                            valuesF[index] = valF;
                        break;
                    case TypeOfValues.Byte:
                        valuesB[index] = Convert.ToByte(value);
                        break;
                    case TypeOfValues.Int16:
                        valuesI16[index] = Convert.ToInt16(value);
                        break;
                    case TypeOfValues.Int32:
                        valuesI32[index] = Convert.ToInt32(value);
                        break;
                    case TypeOfValues.Int64:
                        valuesI64[index] = Convert.ToInt64(value);
                        break;
                    case TypeOfValues.UInt16:
                        valuesUI16[index] = Convert.ToUInt16(value);
                        break;
                    case TypeOfValues.UInt32:
                        valuesUI32[index] = Convert.ToUInt32(value);
                        break;
                    case TypeOfValues.UInt64:
                        valuesUI64[index] = Convert.ToUInt64(value);
                        break;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object GetValue(int index, bool useBounds = false, object lowerBound = null, object upperBound = null, bool useAbsolute = false, object absoluteValue = null)
        {
            if (useBounds)
            {
                if (lowerBound == null || upperBound == null)
                {
                    throw new Exception("Shader.GetValue was called with useBounds set to true, but one or both bounds were null.");
                }
            }
            if (useAbsolute)
            {
                if (absoluteValue == null)
                {
                    {
                        throw new Exception("Shader.GetValue was called with useAbsolute set to true, but the lower bound for the absolute value was null.");
                    }
                }
            }
            bool valid = true;
            switch (typeOfValues)
            {
                case TypeOfValues.Double:
                    var valD = valuesD[index];

                    if (useBounds && !((valD >= Convert.ToDouble(lowerBound)) && (valD <= Convert.ToDouble(upperBound))))
                        valid = false;
                    if (valid && useAbsolute && !(Math.Abs(valD) >= Convert.ToDouble(absoluteValue) || valD.Equals(0)))
                        valid = false;

                    if (valid)
                        return valD;

                    break;
                case TypeOfValues.Float:
                    var valF = valuesF[index];

                    if (useBounds && !((valF >= Convert.ToSingle(lowerBound)) && (valF <= Convert.ToSingle(upperBound))))
                        valid = false;
                    if (valid && useAbsolute && !(Math.Abs(valF) >= Convert.ToSingle(absoluteValue) || valF.Equals(0)))
                        valid = false;

                    if (valid)
                        return valF;

                    break;
                case TypeOfValues.Byte:
                    var valB = valuesB[index];

                    if (useBounds && !((valB >= Convert.ToByte(lowerBound)) && (valB <= Convert.ToByte(upperBound))))
                        valid = false;
                    if (valid && useAbsolute && !(Math.Abs(valB) >= Convert.ToByte(absoluteValue) || valB.Equals(0)))
                        valid = false;

                    if (valid)
                        return valB;

                    break;
                case TypeOfValues.Int16:
                    var valI16 = valuesI16[index];

                    if (useBounds && !((valI16 >= Convert.ToInt16(lowerBound)) && (valI16 <= Convert.ToInt16(upperBound))))
                        valid = false;
                    if (valid && useAbsolute && !(Math.Abs(valI16) >= Convert.ToInt16(absoluteValue) || valI16.Equals(0)))
                        valid = false;

                    if (valid)
                        return valI16;

                    break;
                case TypeOfValues.Int32:
                    var valI32 = valuesI32[index];

                    if (useBounds && !((valI32 >= Convert.ToInt32(lowerBound)) && (valI32 <= Convert.ToInt32(upperBound))))
                        valid = false;
                    if (valid && useAbsolute && !(Math.Abs(valI32) >= Convert.ToInt32(absoluteValue) || valI32.Equals(0)))
                        valid = false;

                    if (valid)
                        return valI32;

                    break;
                case TypeOfValues.Int64:
                    var valI64 = valuesI64[index];

                    if (useBounds && !((valI64 >= Convert.ToInt64(lowerBound)) && (valI64 <= Convert.ToInt64(upperBound))))
                        valid = false;
                    if (valid && useAbsolute && !(Math.Abs(valI64) >= Convert.ToInt64(absoluteValue) || valI64.Equals(0)))
                        valid = false;

                    if (valid)
                        return valI64;

                    break;
                case TypeOfValues.UInt16:
                    var valUI16 = valuesI16[index];

                    if (useBounds && !((valUI16 >= Convert.ToUInt16(lowerBound)) && (valUI16 <= Convert.ToUInt16(upperBound))))
                        valid = false;
                    if (valid && useAbsolute && !(valUI16 >= Convert.ToUInt16(absoluteValue) || valUI16.Equals(0)))
                        valid = false;

                    if (valid)
                        return valUI16;

                    break;
                case TypeOfValues.UInt32:
                    var valUI32 = valuesUI32[index];

                    if (useBounds && !((valUI32 >= Convert.ToUInt32(lowerBound)) && (valUI32 <= Convert.ToUInt32(upperBound))))
                        valid = false;
                    if (valid && useAbsolute && !(valUI32 >= Convert.ToUInt32(absoluteValue) || valUI32.Equals(0)))
                        valid = false;

                    if (valid)
                        return valUI32;

                    break;
                case TypeOfValues.UInt64:
                    var valUI64 = valuesUI64[index];

                    if (useBounds && !((valUI64 >= Convert.ToUInt64(lowerBound)) && (valUI64 <= Convert.ToUInt64(upperBound))))
                        valid = false;
                    if (valid && useAbsolute && !(valUI64 >= Convert.ToUInt64(absoluteValue) || valUI64.Equals(0)))
                        valid = false;

                    if (valid)
                        return valUI64;

                    break;
            }
            return null;
        }

        public int GetShaderEntrySize()
        {
            int size;
            switch (typeOfValues)
            {
                case TypeOfValues.Double:
                case TypeOfValues.Int64:
                case TypeOfValues.UInt64:
                    size = 8;
                    break;
                case TypeOfValues.Float:
                case TypeOfValues.Int32:
                case TypeOfValues.UInt32:
                    size = 4;
                    break;
                case TypeOfValues.Int16:
                case TypeOfValues.UInt16:
                    size = 2;
                    break;
                case TypeOfValues.Byte:
                    size = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return size;
        }

        public long End()
        {
            int size = GetShaderEntrySize();
            return Start + Length*size;
        }
    }

    public enum TypeOfValues
    {
        Double,
        Float,
        Byte,
        Int16,
        Int32,
        Int64,
        UInt16,
        UInt32,
        UInt64
    }

    public enum Endianness
    {
        Little,
        Big
    }
}