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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using LeftosCommonLibrary;

using Microsoft.Win32;

using MiscUtil.Conversion;
using MiscUtil.IO;

#endregion

namespace HexOnSteroids
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string input;

        public static string DocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Hex on Steroids";

        public static string ProfilesPath = DocsPath + @"\Profiles";

        public static string profileToLoad = "";

        private string profileSelected;

        private string title = "Hex on Steroids";

        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(DocsPath))
            {
                Directory.CreateDirectory(DocsPath);
            }

            if (!Directory.Exists(ProfilesPath))
            {
                Directory.CreateDirectory(ProfilesPath);
            }

            RefreshProfilesMenu();

            cp = new CustomProfile();
            profileSelected = "";

            Height = GetRegistrySetting("Height", (int)Height);
            Width = GetRegistrySetting("Width", (int)Width);
            Left = GetRegistrySetting("Left", 0);
            Top = GetRegistrySetting("Top", 0);
        }

        private List<Shader> shadersList { get; set; }

        public static CustomProfile cp { get; set; }

        private void RefreshProfilesMenu()
        {
            int i = 0;
            mnuProfilesList.Items.Clear();
            foreach (var dir in Directory.GetDirectories(ProfilesPath))
            {
                var mi = new MenuItem();
                mi.Header = Helper.GetFolderName(dir);
                foreach (var f in Directory.GetFiles(dir))
                {
                    var mi2 = new MenuItem();
                    mi2.Header = Path.GetFileName(f);
                    mi2.Click += AnyProfile_Click;
                    mi.Items.Add(mi2);
                }
                mnuProfilesList.Items.Insert(i++, mi);
            }
        }

        private void AnyProfile_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.Items.Count > 0)
            {
                if (MessageBox.Show(
                    "Any unsaved changes will be lost. Are you sure you want to continue?",
                    "Hex on Steroids",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }
            string category = ((sender as MenuItem).Parent as MenuItem).Header.ToString();
            string profile = (sender as MenuItem).Header.ToString();
            cp = ProfilesWindow.LoadProfile(ProfilesPath + "\\" + category + "\\" + profile, true);
            SetRegistrySetting("LastEditedProfile", ProfilesPath + "\\" + category + "\\" + profile);
            Title = string.Format("{0} - {1} ({2})", title, profile, category);
            profileSelected = category + "$;$" + profile;
            var dt = new DataTable();
            relinkDataGrid(dt);
        }

        private void mnuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            if (profileSelected == "")
            {
                MessageBox.Show("You must first create and/or select a profile.");
                return;
            }

            var ofd = new OpenFileDialog();
            ofd.ShowDialog();

            if (ofd.FileName == "")
            {
                return;
            }

            foreach (var dir in Directory.GetDirectories(ProfilesPath))
            {
                foreach (var f in Directory.GetFiles(dir))
                {
                    CustomProfile temp = ProfilesWindow.LoadProfile(f, true);
                    if (temp.Files.Any(f1 => f1 == ofd.FileName))
                    {
                        if (f != profileSelected)
                        {
                            MessageBoxResult r =
                                MessageBox.Show(
                                    "Profile '" + Path.GetFileName(f)
                                    + "' is not currently selected but is set as a preset profile for this file. Would you like to load it?",
                                    "Hex on Steroids",
                                    MessageBoxButton.YesNoCancel,
                                    MessageBoxImage.Question);

                            if (r == MessageBoxResult.Yes)
                            {
                                cp = ProfilesWindow.LoadProfile(f, true);
                            }
                            else if (r == MessageBoxResult.Cancel)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            shadersList = new List<Shader>();
            Shader s;
            int i = 0;
            byte[] file = File.ReadAllBytes(ofd.FileName);
            var memory = new MemoryStream(file);
            var br =
                new EndianBinaryReader(
                    (cp.EndiannessType == Endianness.Little
                         ? (EndianBitConverter)new LittleEndianBitConverter()
                         : new BigEndianBitConverter()),
                    memory);

            if (cp.TypeOfRange == RangeType.AutoDetectShaders || cp.TypeOfRange == RangeType.WholeFile)
            {
                if (cp.IgnoreCRC)
                {
                    br.BaseStream.Position = 4;
                }
            }

            switch (cp.TypeOfRange)
            {
                case RangeType.AutoDetectShaders:
                case RangeType.AutoDetectCustomHeader:
                    mainGrid.IsEnabled = false;
                    string oldTitle = Title;
                    var worker = new BackgroundWorker();

                    worker.WorkerReportsProgress = true;

                    worker.DoWork += delegate
                        {
                            int length = cp.TypeOfRange == RangeType.AutoDetectShaders ? 15 : cp.AutoDetectCustomHeader.Length / 2;
                            while (br.BaseStream.Length - br.BaseStream.Position >= length)
                            {
                                byte b = 0;
                                if (cp.TypeOfRange == RangeType.AutoDetectShaders)
                                {
                                    while (b != 83 && br.BaseStream.Position < br.BaseStream.Length)
                                    {
                                        b = br.ReadByte();
                                    }
                                }
                                else
                                {
                                    while (Tools.ByteArrayToHexString(new[] { b }) != cp.AutoDetectCustomHeader.Substring(0, 2)
                                           && br.BaseStream.Position < br.BaseStream.Length)
                                    {
                                        b = br.ReadByte();
                                    }
                                }
                                br.BaseStream.Position -= 1;
                                if (br.BaseStream.Length - br.BaseStream.Position >= length)
                                {
                                    //Console.WriteLine("Reading {0} bytes from {1}", length, br.BaseStream.Position);
                                    byte[] bufArray = br.ReadBytes(length);
                                    if ((cp.TypeOfRange == RangeType.AutoDetectShaders
                                         && Tools.ByteArrayToHexString(bufArray) == "53686164657220436F6D70696C6572")
                                        || (cp.TypeOfRange == RangeType.AutoDetectCustomHeader
                                            && Tools.ByteArrayToHexString(bufArray) == cp.AutoDetectCustomHeader)) // "Shader Compiler"
                                    {
                                        if (cp.TypeOfRange == RangeType.AutoDetectShaders)
                                        {
                                            br.BaseStream.Position += 19;
                                        }
                                        if (cp.AutoDetectJump == 0)
                                        {
                                            if (br.BaseStream.Position % 2 == 1)
                                            {
                                                br.BaseStream.Position++;
                                            }
                                        }
                                        else
                                        {
                                            br.BaseStream.Position += cp.AutoDetectJump;
                                        }
                                        s = new Shader(
                                            cp.AutoDetectValueType,
                                            cp.EndiannessType,
                                            string.Format("S{0} @ {1}", i++, br.BaseStream.Position));
                                        s.Start = br.BaseStream.Position;
                                        int j = 0;
                                        switch (cp.AutoDetectValueType)
                                        {
                                            case TypeOfValues.Double:
                                                while (j < cp.AutoDetectValueCount
                                                       && br.BaseStream.Length - br.BaseStream.Position >= 8)
                                                {
                                                    double buf = br.ReadDouble();
                                                    s.AddValue(buf);
                                                    j++;
                                                }
                                                break;
                                            case TypeOfValues.Float:
                                                while (j < cp.AutoDetectValueCount
                                                       && br.BaseStream.Length - br.BaseStream.Position >= 4)
                                                {
                                                    float buf = br.ReadSingle();
                                                    s.AddValue(buf);
                                                    j++;
                                                }
                                                break;
                                            case TypeOfValues.Byte:
                                                while (j < cp.AutoDetectValueCount
                                                       && br.BaseStream.Length - br.BaseStream.Position >= 1)
                                                {
                                                    byte buf = br.ReadByte();
                                                    s.AddValue(buf);
                                                    j++;
                                                }
                                                break;
                                            case TypeOfValues.Int16:
                                                while (j < cp.AutoDetectValueCount
                                                       && br.BaseStream.Length - br.BaseStream.Position >= 2)
                                                {
                                                    short buf = br.ReadInt16();
                                                    s.AddValue(buf);
                                                    j++;
                                                }
                                                break;
                                            case TypeOfValues.Int32:
                                                while (j < cp.AutoDetectValueCount
                                                       && br.BaseStream.Length - br.BaseStream.Position >= 4)
                                                {
                                                    int buf = br.ReadInt32();
                                                    s.AddValue(buf);
                                                    j++;
                                                }
                                                break;
                                            case TypeOfValues.Int64:
                                                while (j < cp.AutoDetectValueCount
                                                       && br.BaseStream.Length - br.BaseStream.Position >= 8)
                                                {
                                                    long buf = br.ReadInt64();
                                                    s.AddValue(buf);
                                                    j++;
                                                }
                                                break;
                                            case TypeOfValues.UInt16:
                                                while (j < cp.AutoDetectValueCount
                                                       && br.BaseStream.Length - br.BaseStream.Position >= 2)
                                                {
                                                    ushort buf = br.ReadUInt16();
                                                    s.AddValue(buf);
                                                    j++;
                                                }
                                                break;
                                            case TypeOfValues.UInt32:
                                                while (j < cp.AutoDetectValueCount
                                                       && br.BaseStream.Length - br.BaseStream.Position >= 4)
                                                {
                                                    uint buf = br.ReadUInt32();
                                                    s.AddValue(buf);
                                                    j++;
                                                }
                                                break;
                                            case TypeOfValues.UInt64:
                                                while (j < cp.AutoDetectValueCount
                                                       && br.BaseStream.Length - br.BaseStream.Position >= 8)
                                                {
                                                    ulong buf = br.ReadUInt64();
                                                    s.AddValue(buf);
                                                    j++;
                                                }
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException();
                                        }
                                        shadersList.Add(s);
                                    }
                                    else
                                    {
                                        br.BaseStream.Position -= (length - 1);
                                    }
                                }
                                worker.ReportProgress((int)((double)br.BaseStream.Position / br.BaseStream.Length * 100));
                            }
                        };

                    worker.ProgressChanged +=
                        delegate(object o, ProgressChangedEventArgs args)
                            { Title = string.Format("{0} - Auto-detecting shaders: {1}% complete", title, args.ProgressPercentage); };

                    worker.RunWorkerCompleted += delegate
                        {
                            Title = oldTitle;
                            mainGrid.IsEnabled = true;
                            fillDataGrid(shadersList);
                            br.Close();
                            memory.Close();
                        };
                    worker.RunWorkerAsync();
                    break;
                case RangeType.WholeFile:

                    #region Whole File

                    s = new Shader(cp.AutoDetectValueType, cp.EndiannessType, "@" + br.BaseStream.Position);
                    s.Start = br.BaseStream.Position;
                    switch (cp.AutoDetectValueType)
                    {
                        case TypeOfValues.Double:
                            while (br.BaseStream.Length - br.BaseStream.Position >= 8)
                            {
                                double buf = br.ReadDouble();
                                s = updateShader(s, buf, shadersList, ref i, br);
                            }
                            break;
                        case TypeOfValues.Float:
                            while (br.BaseStream.Length - br.BaseStream.Position >= 4)
                            {
                                float buf = br.ReadSingle();
                                s = updateShader(s, buf, shadersList, ref i, br);
                            }
                            break;
                        case TypeOfValues.Byte:
                            while (br.BaseStream.Length - br.BaseStream.Position >= 1)
                            {
                                byte buf = br.ReadByte();
                                s = updateShader(s, buf, shadersList, ref i, br);
                            }
                            break;
                        case TypeOfValues.Int16:
                            while (br.BaseStream.Length - br.BaseStream.Position >= 2)
                            {
                                short buf = br.ReadInt16();
                                s = updateShader(s, buf, shadersList, ref i, br);
                            }
                            break;
                        case TypeOfValues.Int32:
                            while (br.BaseStream.Length - br.BaseStream.Position >= 4)
                            {
                                int buf = br.ReadInt32();
                                s = updateShader(s, buf, shadersList, ref i, br);
                            }
                            break;
                        case TypeOfValues.Int64:
                            while (br.BaseStream.Length - br.BaseStream.Position >= 8)
                            {
                                long buf = br.ReadInt64();
                                s = updateShader(s, buf, shadersList, ref i, br);
                            }
                            break;
                        case TypeOfValues.UInt16:
                            while (br.BaseStream.Length - br.BaseStream.Position >= 2)
                            {
                                ushort buf = br.ReadUInt16();
                                s = updateShader(s, buf, shadersList, ref i, br);
                            }
                            break;
                        case TypeOfValues.UInt32:
                            while (br.BaseStream.Length - br.BaseStream.Position >= 4)
                            {
                                uint buf = br.ReadUInt32();
                                s = updateShader(s, buf, shadersList, ref i, br);
                            }
                            break;
                        case TypeOfValues.UInt64:
                            while (br.BaseStream.Length - br.BaseStream.Position >= 8)
                            {
                                ulong buf = br.ReadUInt64();
                                s = updateShader(s, buf, shadersList, ref i, br);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    shadersList.Add(s);
                    fillDataGrid(shadersList);
                    br.Close();
                    memory.Close();
                    break;

                    #endregion

                case RangeType.Custom:

                    #region Custom Profile

                    foreach (var cdr in cp.Ranges)
                    {
                        s = new Shader(cdr.Type, cp.EndiannessType, cdr.Name);
                        br.BaseStream.Position = cdr.Start;
                        s.Start = cdr.Start;
                        switch (cdr.Type)
                        {
                            case TypeOfValues.Double:
                                while (cdr.End + 1 - br.BaseStream.Position >= 8)
                                {
                                    double buf = br.ReadDouble();
                                    s.AddValue(buf);
                                }
                                break;
                            case TypeOfValues.Float:
                                while (cdr.End + 1 - br.BaseStream.Position >= 4)
                                {
                                    float buf = br.ReadSingle();
                                    if (buf.Equals(0))
                                    {
                                        buf *= -1;
                                    }
                                    s.AddValue(buf);
                                }
                                break;
                            case TypeOfValues.Byte:
                                while (cdr.End + 1 - br.BaseStream.Position >= 1)
                                {
                                    byte buf = br.ReadByte();
                                    s.AddValue(buf);
                                }
                                break;
                            case TypeOfValues.Int16:
                                while (cdr.End + 1 - br.BaseStream.Position >= 2)
                                {
                                    short buf = br.ReadInt16();
                                    s.AddValue(buf);
                                }
                                break;
                            case TypeOfValues.Int32:
                                while (cdr.End + 1 - br.BaseStream.Position >= 4)
                                {
                                    int buf = br.ReadInt32();
                                    s.AddValue(buf);
                                }
                                break;
                            case TypeOfValues.Int64:
                                while (cdr.End + 1 - br.BaseStream.Position >= 8)
                                {
                                    long buf = br.ReadInt64();
                                    s.AddValue(buf);
                                }
                                break;
                            case TypeOfValues.UInt16:
                                while (cdr.End + 1 - br.BaseStream.Position >= 2)
                                {
                                    ushort buf = br.ReadUInt16();
                                    s.AddValue(buf);
                                }
                                break;
                            case TypeOfValues.UInt32:
                                while (cdr.End + 1 - br.BaseStream.Position >= 4)
                                {
                                    uint buf = br.ReadUInt32();
                                    s.AddValue(buf);
                                }
                                break;
                            case TypeOfValues.UInt64:
                                while (cdr.End + 1 - br.BaseStream.Position >= 8)
                                {
                                    ulong buf = br.ReadUInt64();
                                    s.AddValue(buf);
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        shadersList.Add(s);
                    }
                    br.Close();
                    memory.Close();
                    fillDataGrid(shadersList);
                    break;

                    #endregion

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void fillDataGridNew(List<Shader> shadersList)
        {
            long max = 0;
            for (int i = 0; i < shadersList.Count; i++)
            {
                if (shadersList[i].Length > max)
                {
                    max = shadersList[i].Length;
                }
            }
        }

        private void fillDataGrid(List<Shader> shadersList)
        {
            var dt = new DataTable();
            int i;
            long max = 0;
            for (i = 0; i < shadersList.Count; i++)
            {
                if (shadersList[i].Length > max)
                {
                    max = shadersList[i].Length;
                }

                switch (shadersList[i].typeOfValues)
                {
                    case TypeOfValues.Double:
                        dt.Columns.Add(shadersList[i].Name, typeof(double));
                        break;
                    case TypeOfValues.Float:
                        dt.Columns.Add(shadersList[i].Name, typeof(float));
                        break;
                    case TypeOfValues.Byte:
                        dt.Columns.Add(shadersList[i].Name, typeof(byte));
                        break;
                    case TypeOfValues.Int16:
                        dt.Columns.Add(shadersList[i].Name, typeof(Int16));
                        break;
                    case TypeOfValues.Int32:
                        dt.Columns.Add(shadersList[i].Name, typeof(Int32));
                        break;
                    case TypeOfValues.Int64:
                        dt.Columns.Add(shadersList[i].Name, typeof(Int64));
                        break;
                    case TypeOfValues.UInt16:
                        dt.Columns.Add(shadersList[i].Name, typeof(UInt16));
                        break;
                    case TypeOfValues.UInt32:
                        dt.Columns.Add(shadersList[i].Name, typeof(UInt32));
                        break;
                    case TypeOfValues.UInt64:
                        dt.Columns.Add(shadersList[i].Name, typeof(UInt64));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            for (long j = 0; j < max; j++)
            {
                DataRow dr = dt.NewRow();
                dt.Rows.Add(dr);
            }

            string oldtitle = Title;
            mainGrid.IsEnabled = false;

            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += delegate
                {
                    for (int k = 0; k < shadersList.Count; k++)
                    {
                        for (int j = 0; j < shadersList[k].Length; j++)
                        {
                            object val = shadersList[k].GetValue(
                                j, cp.UseBounds, cp.LowerBound, cp.UpperBound, cp.UseAbsolute, cp.AbsoluteValue);
                            dt.Rows[j][k] = val ?? DBNull.Value;
                        }
                        worker.ReportProgress((int)((double)k / shadersList.Count * 100));
                    }
                };

            worker.ProgressChanged +=
                delegate(object sender, ProgressChangedEventArgs args)
                    { Title = string.Format("{0} - Loading Data: {1}% complete", title, args.ProgressPercentage.ToString()); };

            worker.RunWorkerCompleted += delegate
                {
                    relinkDataGrid(dt);
                    Title = oldtitle;
                    mainGrid.IsEnabled = true;
                };

            worker.RunWorkerAsync();
        }

        private void relinkDataGrid(DataTable dt)
        {
            DataView dv = dt.DefaultView;
            dv.AllowEdit = true;
            dv.AllowNew = false;
            dv.AllowDelete = false;

            dataGrid.DataContext = dv;
        }

        private object GetCellValue(DataGrid dataGrid, int row, int col)
        {
            var dataRowView = dataGrid.Items[row] as DataRowView;
            if (dataRowView != null)
            {
                return dataRowView.Row.ItemArray[col];
            }

            return null;
        }

        private object myCell(int row, int col)
        {
            return GetCellValue(dataGrid, row, col);
        }

        private static Shader updateShader(Shader s, object buf, List<Shader> shadersList, ref int i, EndianBinaryReader br)
        {
            s.AddValue(buf);
            if (s.Length == cp.AutoDetectValueCount)
            {
                shadersList.Add(s);
                s = new Shader(cp.AutoDetectValueType, cp.EndiannessType, "@" + br.BaseStream.Position.ToString());
                s.Start = br.BaseStream.Position;
            }
            return s;
        }

        private void mnuProfilesEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.Items.Count > 0)
            {
                if (MessageBox.Show(
                    "Any unsaved changes will be lost. Are you sure you want to continue?",
                    "Hex on Steroids",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }

            var dt = new DataTable();
            relinkDataGrid(dt);

            var pw = new ProfilesWindow();
            pw.ShowDialog();

            RefreshProfilesMenu();

            if (profileToLoad != "")
            {
                ProfilesWindow.LoadProfile(profileToLoad, true);
                string category = Helper.GetFolderName(Path.GetDirectoryName(profileToLoad));
                string profile = Path.GetFileName(profileToLoad);
                Title = string.Format("{0} - {1} ({2})", title, profile, category);
                profileSelected = category + "$;$" + profile;
            }
            else
            {
                Title = title;
                profileSelected = "";
            }
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()).ToString();
        }

        private void mnuFileSave_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.ShowDialog();

            if (sfd.FileName == "")
            {
                return;
            }

            var openmode = FileMode.Open;
            if (!File.Exists(sfd.FileName))
            {
                if (cp.TypeOfRange == RangeType.WholeFile)
                {
                    openmode = FileMode.Create;
                }
                else
                {
                    MessageBox.Show(
                        "You can't save to a new file using profiles with ranges other than 'Whole File'. You should save to the same file.");
                    return;
                }
            }

            string oldtitle = Title;
            mainGrid.IsEnabled = false;

            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += delegate
                {
                    for (int k = 0; k < shadersList.Count; k++)
                    {
                        for (int j = 0; j < shadersList[k].Length; j++)
                        {
                            object val = myCell(j, k);
                            if (val != DBNull.Value)
                            {
                                shadersList[k].ChangeValue(j, val);
                            }
                        }
                        worker.ReportProgress((int)((double)k / shadersList.Count * 100));
                    }
                };

            worker.ProgressChanged +=
                delegate(object s, ProgressChangedEventArgs args)
                    { Title = string.Format("{0} - Updating Data: {1}% complete", title, args.ProgressPercentage.ToString()); };

            worker.RunWorkerCompleted += delegate
                {
                    using (
                        var bw =
                            new EndianBinaryWriter(
                                cp.EndiannessType == Endianness.Little
                                    ? (EndianBitConverter)new LittleEndianBitConverter()
                                    : new BigEndianBitConverter(),
                                new FileStream(sfd.FileName, openmode)))
                    {
                        for (int k = 0; k < shadersList.Count; k++)
                        {
                            Shader shader = shadersList[k];
                            bw.BaseStream.Position = shader.Start;
                            switch (shader.typeOfValues)
                            {
                                case TypeOfValues.Double:
                                    for (int j = 0; j < shader.Length; j++)
                                    {
                                        bw.Write((double)shader.GetValue(j));
                                    }
                                    break;
                                case TypeOfValues.Float:
                                    for (int j = 0; j < shader.Length; j++)
                                    {
                                        bw.Write((float)shader.GetValue(j));
                                    }
                                    break;
                                case TypeOfValues.Byte:
                                    for (int j = 0; j < shader.Length; j++)
                                    {
                                        bw.Write((byte)shader.GetValue(j));
                                    }
                                    break;
                                case TypeOfValues.Int16:
                                    for (int j = 0; j < shader.Length; j++)
                                    {
                                        bw.Write((Int16)shader.GetValue(j));
                                    }
                                    break;
                                case TypeOfValues.Int32:
                                    for (int j = 0; j < shader.Length; j++)
                                    {
                                        bw.Write((Int32)shader.GetValue(j));
                                    }
                                    break;
                                case TypeOfValues.Int64:
                                    for (int j = 0; j < shader.Length; j++)
                                    {
                                        bw.Write((Int64)shader.GetValue(j));
                                    }
                                    break;
                                case TypeOfValues.UInt16:
                                    for (int j = 0; j < shader.Length; j++)
                                    {
                                        bw.Write((UInt16)shader.GetValue(j));
                                    }
                                    break;
                                case TypeOfValues.UInt32:
                                    for (int j = 0; j < shader.Length; j++)
                                    {
                                        bw.Write((UInt32)shader.GetValue(j));
                                    }
                                    break;
                                case TypeOfValues.UInt64:
                                    for (int j = 0; j < shader.Length; j++)
                                    {
                                        bw.Write((UInt64)shader.GetValue(j));
                                    }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                    mainGrid.IsEnabled = true;
                    Title = oldtitle;
                    MessageBox.Show("Saved successfully.");
                };
            worker.RunWorkerAsync();
        }

        private void dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (dataGrid.SelectedCells.Count == 0)
            {
                return;
            }

            int row = dataGrid.Items.IndexOf(dataGrid.CurrentCell.Item);
            int col = dataGrid.CurrentCell.Column.DisplayIndex;

            Shader current = shadersList[col];

            int size = 1;
            size = current.GetShaderEntrySize();

            long offset = current.Start + row * size;

            try
            {
                object value = current.GetValue(row, cp.UseBounds, cp.LowerBound, cp.UpperBound, cp.UseAbsolute, cp.AbsoluteValue);
                object realvalue = current.GetValue(row);
                txbStatus.Text = string.Format(
                    "Offset {0} (Original value: {1})", offset, value ?? string.Format("Out of Bounds ({0})", realvalue));
            }
            catch (ArgumentOutOfRangeException)
            {
                txbStatus.Text = string.Format("Outside of detected range.");
            }
        }

        private void mnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void mnuFindGoTo_Click(object sender, RoutedEventArgs e)
        {
            var ibw = new InputBoxWindow("Enter the absolute offset you want to go to");
            if (ibw.ShowDialog() == true)
            {
                try
                {
                    long offset = Convert.ToInt64(input);
                    for (int i = 0; i < shadersList.Count; i++)
                    {
                        Shader cur = shadersList[i];
                        if (offset >= cur.Start && offset <= cur.End())
                        {
                            int size = cur.GetShaderEntrySize();
                            for (int j = 0; j < cur.Length; j++)
                            {
                                if (offset >= cur.Start + j * size && offset <= cur.Start + (j + 1) * size)
                                {
                                    dataGrid.SelectedCells.Clear();
                                    dataGrid.CurrentCell = new DataGridCellInfo(dataGrid.Items[j], dataGrid.Columns[i]);
                                    dataGrid.SelectedCells.Add(dataGrid.CurrentCell);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                string[] lines = Tools.SplitLinesToArray(Clipboard.GetText());

                int row = dataGrid.Items.IndexOf(dataGrid.CurrentCell.Item);
                int col = dataGrid.CurrentCell.Column.DisplayIndex;

                if (row + lines.Length > dataGrid.Items.Count + 1)
                {
                    MessageBox.Show(
                        "You're trying to paste more rows than currently available. Make sure you're not selecting the "
                        + "shader/range names when copying data.");
                    e.Handled = true;
                    return;
                }

                DataTable dt = ((DataView)dataGrid.DataContext).Table;

                int length = lines[0].Split('\t').Length;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (String.IsNullOrWhiteSpace(line) && i == lines.Length - 1)
                    {
                        continue;
                    }
                    string[] parts = line.Split('\t');
                    if (parts.Length == length)
                    {
                        for (int j = 0; j < parts.Length; j++)
                        {
                            dt.Rows[row + i][col + j] = (!String.IsNullOrWhiteSpace(parts[j])) ? (object)parts[j] : DBNull.Value;
                        }
                    }
                }
                relinkDataGrid(dt);
                e.Handled = true;
                //GenericEventHandlers.OnExecutedPaste(sender, null);
            }
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Hex on Steroids", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.No)
            {
                e.Cancel = true;
            }

            SetRegistrySetting("Height", Height);
            SetRegistrySetting("Width", Width);
            SetRegistrySetting("Left", Left);
            SetRegistrySetting("Top", Top);
        }

        public static void SetRegistrySetting<T>(string setting, T value)
        {
            RegistryKey rk = Registry.CurrentUser;
            try
            {
                try
                {
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\Hex On Steroids", true);
                    if (rk == null)
                    {
                        throw new Exception();
                    }
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(@"SOFTWARE\Lefteris Aslanoglou\Hex On Steroids");
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\Hex On Steroids", true);
                    if (rk == null)
                    {
                        throw new Exception();
                    }
                }

                rk.SetValue(setting, value);
            }
            catch
            {
                MessageBox.Show("Couldn't save changed setting.");
            }
        }

        public static T GetRegistrySetting<T>(string setting, T defaultValue)
        {
            return (T)Convert.ChangeType(GetRegistrySetting(setting, defaultValue.ToString()), typeof(T));
        }

        public static string GetRegistrySetting(string setting, string defaultValue)
        {
            RegistryKey rk = Registry.CurrentUser;
            string settingValue = defaultValue;
            try
            {
                if (rk == null)
                {
                    throw new Exception();
                }

                rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\Hex On Steroids");
                if (rk != null)
                {
                    settingValue = rk.GetValue(setting, defaultValue).ToString();
                }
            }
            catch
            {
                settingValue = defaultValue;
            }

            return settingValue;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mnuOptionsIncludeHeaders.IsChecked = GetRegistrySetting("IncludeHeaders", false);

            dataGrid.ClipboardCopyMode = mnuOptionsIncludeHeaders.IsChecked
                                             ? DataGridClipboardCopyMode.IncludeHeader
                                             : DataGridClipboardCopyMode.ExcludeHeader;
        }

        private void mnuOptionsIncludeHeaders_Click(object sender, RoutedEventArgs e)
        {
            SetRegistrySetting("IncludeHeaders", mnuOptionsIncludeHeaders.IsChecked);

            dataGrid.ClipboardCopyMode = mnuOptionsIncludeHeaders.IsChecked
                                             ? DataGridClipboardCopyMode.IncludeHeader
                                             : DataGridClipboardCopyMode.ExcludeHeader;
        }
    }
}