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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

#endregion

namespace HexOnSteroids
{
    /// <summary>
    ///     Interaction logic for ProfilesWindow.xaml
    /// </summary>
    public partial class ProfilesWindow : Window
    {
        private static ProfilesWindow pw;

        public ProfilesWindow()
        {
            InitializeComponent();

            MainWindow.cp = new CustomProfile();

            pw = this;

            RefreshCategoriesCombo();

            if (cmbCategories.Items.Count > 0)
                cmbCategories.SelectedIndex = 0;
            if (cmbProfiles.Items.Count > 0)
                cmbProfiles.SelectedIndex = 0;
        }

        private void cmbDatatype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show(MainWindow.cp.AutoDetectValueType.ToString());
        }

        private void rbAutoDetect_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.cp.RangeType = RangeType.AutoDetectShaders;
                dgRanges.IsEnabled = false;
                txtAutoDetectValueCount.IsEnabled = true;
                //cmbDatatype.IsEnabled = true;
            }
            catch
            {
            }
        }

        private void rbWholeFile_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.cp.RangeType = RangeType.WholeFile;
            dgRanges.IsEnabled = false;
            txtAutoDetectValueCount.IsEnabled = true;
            //cmbDatatype.IsEnabled = true;
        }

        private void rbCustom_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.cp.RangeType = RangeType.Custom;
            dgRanges.IsEnabled = true;
            txtAutoDetectValueCount.IsEnabled = false;
            //cmbDatatype.IsEnabled = false;
        }

        private void btnEditCategories_Click(object sender, RoutedEventArgs e)
        {
            var cw = new CategoriesWindow();
            cw.ShowDialog();

            RefreshCategoriesCombo();
            cmbCategories.SelectedIndex = 0;
        }

        private void RefreshProfilesCombo()
        {
            if (cmbCategories.SelectedIndex == -1)
            {
                cmbProfiles.Items.Clear();
                return;
            }

            cmbProfiles.Items.Clear();
            Directory.GetFiles(MainWindow.ProfilesPath + "\\" + cmbCategories.SelectedItem)
                     .ToList()
                     .ForEach(d => cmbProfiles.Items.Add(Path.GetFileName(d)));
        }

        private void RefreshCategoriesCombo()
        {
            cmbCategories.Items.Clear();
            Directory.GetDirectories(MainWindow.ProfilesPath).ToList().ForEach(d => cmbCategories.Items.Add(Helper.GetFolderName(d)));
        }

        private void cmbCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshProfilesCombo();
        }

        private void cmbProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategories.SelectedIndex == -1 || cmbProfiles.SelectedIndex == -1)
                return;

            if (e.RemovedItems.Count == 1)
                SaveProfile(MainWindow.ProfilesPath + "\\" + cmbCategories.SelectedItem + "\\" + e.RemovedItems[0]);
            MainWindow.cp = LoadProfile(MainWindow.ProfilesPath + "\\" + cmbCategories.SelectedItem + "\\" + cmbProfiles.SelectedItem);
        }

        public static void SaveProfile(string p)
        {
            using (var sw = new StreamWriter(p, false))
            {
                sw.WriteLine("Name$;$" + MainWindow.cp.Name);
                sw.WriteLine("RangeType$;$" + MainWindow.cp.RangeType);
                sw.WriteLine("AutoDetectCustomHeader$;$" + MainWindow.cp.AutoDetectCustomHeader);
                sw.WriteLine("AutoDetectValueCount$;$" + MainWindow.cp.AutoDetectValueCount);
                sw.WriteLine("AutoDetectValueType$;$" + MainWindow.cp.AutoDetectValueType);
                sw.WriteLine("Endianness$;$" + MainWindow.cp.Endianness);
                foreach (var file in MainWindow.cp.Files)
                    sw.WriteLine("File$;$" + file);
                foreach (var cdr in MainWindow.cp.Ranges)
                    sw.WriteLine("Range$;$" + cdr.Start + "$;$" + cdr.End + "$;$" + cdr.Type + "$;$" + cdr.Name);
                switch (MainWindow.cp.AutoDetectValueType)
                {
                    case TypeOfValues.Double:
                    case TypeOfValues.Float:
                        sw.WriteLine("UseBounds$;${0}$;${1:R}$;${2:R}", MainWindow.cp.UseBounds.ToString(), MainWindow.cp.LowerBound,
                                     MainWindow.cp.UpperBound);
                        sw.WriteLine("UseAbsolute$;${0}$;${1:R}", MainWindow.cp.UseAbsolute.ToString(), MainWindow.cp.AbsoluteValue);
                        break;
                    case TypeOfValues.Byte:
                    case TypeOfValues.Int16:
                    case TypeOfValues.Int32:
                    case TypeOfValues.Int64:
                    case TypeOfValues.UInt16:
                    case TypeOfValues.UInt32:
                    case TypeOfValues.UInt64:
                        sw.WriteLine("UseBounds$;${0}$;${1}$;${2}", MainWindow.cp.UseBounds.ToString(), MainWindow.cp.LowerBound,
                                     MainWindow.cp.UpperBound);
                        sw.WriteLine("UseAbsolute$;${0}$;${1}", MainWindow.cp.UseAbsolute.ToString(), MainWindow.cp.AbsoluteValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                sw.WriteLine("IgnoreCRC$;$" + MainWindow.cp.IgnoreCRC);
            }
        }

        public static CustomProfile LoadProfile(string p, bool fromMain = false)
        {
            var cp = new CustomProfile();

            using (var sr = new StreamReader(p))
            {
                string s;
                while (!sr.EndOfStream)
                {
                    s = sr.ReadLine();
                    string[] parts = s.Split(new[] {"$;$"}, StringSplitOptions.None);
                    switch (parts[0])
                    {
                        case "Name":
                            cp.Name = parts[1];
                            break;
                        case "RangeType":
                            cp.RangeType = (RangeType) Enum.Parse(typeof (RangeType), parts[1]);
                            if (!fromMain)
                            {
                                switch (cp.RangeType)
                                {
                                    case RangeType.AutoDetectCustomHeader:
                                        pw.rbAutoDetectCustom.IsChecked = true;
                                        break;
                                    case RangeType.AutoDetectShaders:
                                        pw.rbAutoDetect.IsChecked = true;
                                        break;
                                    case RangeType.WholeFile:
                                        pw.rbWholeFile.IsChecked = true;
                                        break;
                                    case RangeType.Custom:
                                        pw.rbCustom.IsChecked = true;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                            break;
                        case "AutoDetectCustomHeader":
                            cp.AutoDetectCustomHeader = parts[1];
                            break;
                        case "AutoDetectValueCount":
                            cp.AutoDetectValueCount = Convert.ToInt32(parts[1]);
                            break;
                        case "AutoDetectValueType":
                            cp.AutoDetectValueType = (TypeOfValues) Enum.Parse(typeof (TypeOfValues), parts[1]);
                            break;
                        case "Endianness":
                            cp.Endianness = (Endianness) Enum.Parse(typeof (Endianness), parts[1]);
                            break;
                        case "File":
                            cp.Files.Add(parts[1]);
                            break;
                        case "Range":
                            cp.Ranges.Add(new CustomDataRange(Convert.ToInt64(parts[1]), Convert.ToInt64(parts[2]),
                                                              (TypeOfValues) Enum.Parse(typeof (TypeOfValues), parts[3]), parts[4]));
                            break;
                        case "UseBounds":
                            cp.UseBounds = Convert.ToBoolean(parts[1]);
                            if (cp.UseBounds)
                            {
                                switch (cp.AutoDetectValueType)
                                {
                                    case TypeOfValues.Double:
                                        cp.LowerBound = Convert.ToDouble(parts[2]);
                                        cp.UpperBound = Convert.ToDouble(parts[3]);
                                        break;
                                    case TypeOfValues.Float:
                                        cp.LowerBound = Convert.ToSingle(parts[2]);
                                        cp.UpperBound = Convert.ToSingle(parts[3]);
                                        break;
                                    case TypeOfValues.Byte:
                                        cp.LowerBound = Convert.ToByte(parts[2]);
                                        cp.UpperBound = Convert.ToByte(parts[3]);
                                        break;
                                    case TypeOfValues.Int16:
                                        cp.LowerBound = Convert.ToInt16(parts[2]);
                                        cp.UpperBound = Convert.ToInt16(parts[3]);
                                        break;
                                    case TypeOfValues.Int32:
                                        cp.LowerBound = Convert.ToInt32(parts[2]);
                                        cp.UpperBound = Convert.ToInt32(parts[3]);
                                        break;
                                    case TypeOfValues.Int64:
                                        cp.LowerBound = Convert.ToInt64(parts[2]);
                                        cp.UpperBound = Convert.ToInt64(parts[3]);
                                        break;
                                    case TypeOfValues.UInt16:
                                        cp.LowerBound = Convert.ToUInt16(parts[2]);
                                        cp.UpperBound = Convert.ToUInt16(parts[3]);
                                        break;
                                    case TypeOfValues.UInt32:
                                        cp.LowerBound = Convert.ToUInt32(parts[2]);
                                        cp.UpperBound = Convert.ToUInt32(parts[3]);
                                        break;
                                    case TypeOfValues.UInt64:
                                        cp.LowerBound = Convert.ToUInt64(parts[2]);
                                        cp.UpperBound = Convert.ToUInt64(parts[3]);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                            break;
                        case "UseAbsolute":
                            cp.UseAbsolute = Convert.ToBoolean(parts[1]);
                            if (cp.UseAbsolute)
                            {
                                switch (cp.AutoDetectValueType)
                                {
                                    case TypeOfValues.Double:
                                        cp.AbsoluteValue = Convert.ToDouble(parts[2]);
                                        break;
                                    case TypeOfValues.Float:
                                        cp.AbsoluteValue = Convert.ToSingle(parts[2]);
                                        break;
                                    case TypeOfValues.Byte:
                                        cp.AbsoluteValue = Convert.ToByte(parts[2]);
                                        break;
                                    case TypeOfValues.Int16:
                                        cp.AbsoluteValue = Convert.ToInt16(parts[2]);
                                        break;
                                    case TypeOfValues.Int32:
                                        cp.AbsoluteValue = Convert.ToInt32(parts[2]);
                                        break;
                                    case TypeOfValues.Int64:
                                        cp.AbsoluteValue = Convert.ToInt64(parts[2]);
                                        break;
                                    case TypeOfValues.UInt16:
                                        cp.AbsoluteValue = Convert.ToUInt16(parts[2]);
                                        break;
                                    case TypeOfValues.UInt32:
                                        cp.AbsoluteValue = Convert.ToUInt32(parts[2]);
                                        break;
                                    case TypeOfValues.UInt64:
                                        cp.AbsoluteValue = Convert.ToUInt64(parts[2]);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                            break;
                        case "IgnoreCRC":
                            cp.IgnoreCRC = Convert.ToBoolean(parts[1]);
                            break;
                    }
                }
            }

            if (!fromMain)
            {
                pw.grdProfile.DataContext = cp;
                pw.grdOptions.DataContext = cp;
                pw.dgRanges.DataContext = cp.Ranges;
                pw.dgRanges.ItemsSource = cp.Ranges;
            }

            return cp;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnFilesAdd_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.ShowDialog();

            if (ofd.FileName == "")
                return;

            MainWindow.cp.Files.Add(ofd.FileName);
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            e.Cancel = !(CheckIfRangesAreValid());
            if (!e.Cancel && cmbCategories.SelectedIndex != -1 && cmbProfiles.SelectedIndex != -1)
            {
                SaveProfile(MainWindow.ProfilesPath + "\\" + cmbCategories.SelectedItem + "\\" + cmbProfiles.SelectedItem);
                MainWindow.profileToLoad = MainWindow.ProfilesPath + "\\" + cmbCategories.SelectedItem + "\\" + cmbProfiles.SelectedItem;
            }
            else
            {
                MainWindow.profileToLoad = "";
            }
        }

        private bool CheckIfRangesAreValid()
        {
            List<CustomDataRange> list = MainWindow.cp.Ranges.ToList();
            list.Sort((r1, r2) => r1.Start.CompareTo(r2.Start));
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].End < list[i].Start)
                {
                    MessageBox.Show(String.Format("Ranges can't have their end-point before their start-point: {0}-{1}", list[i].Start,
                                                  list[i].End));
                    return false;
                }
                long len = list[i].End - list[i].Start + 1;
                switch (list[i].Type)
                {
                    case TypeOfValues.Double:
                        if (len%8 != 0)
                        {
                            MessageBox.Show(String.Format("A Double type range's length must be a multiple of 8: {0}-{1}", list[i].Start,
                                                          list[i].End));
                            return false;
                        }
                        break;
                    case TypeOfValues.Float:
                        if (len%4 != 0)
                        {
                            MessageBox.Show(String.Format("A Float type range's length must be a multiple of 4: {0}-{1}", list[i].Start,
                                                          list[i].End));
                            return false;
                        }
                        break;
                    case TypeOfValues.Byte:
                        break;
                    case TypeOfValues.Int16:
                        if (len%2 != 0)
                        {
                            MessageBox.Show(String.Format("A Int16 type range's length must be a multiple of 2: {0}-{1}", list[i].Start,
                                                          list[i].End));
                            return false;
                        }
                        break;
                    case TypeOfValues.Int32:
                        if (len%4 != 0)
                        {
                            MessageBox.Show(String.Format("A Int32 type range's length must be a multiple of 4: {0}-{1}", list[i].Start,
                                                          list[i].End));
                            return false;
                        }
                        break;
                    case TypeOfValues.Int64:
                        if (len%8 != 0)
                        {
                            MessageBox.Show(String.Format("A Int64 type range's length must be a multiple of 8: {0}-{1}", list[i].Start,
                                                          list[i].End));
                            return false;
                        }
                        break;
                    case TypeOfValues.UInt16:
                        if (len%2 != 0)
                        {
                            MessageBox.Show(String.Format("A UInt16 type range's length must be a multiple of 2: {0}-{1}", list[i].Start,
                                                          list[i].End));
                            return false;
                        }
                        break;
                    case TypeOfValues.UInt32:
                        if (len%4 != 0)
                        {
                            MessageBox.Show(String.Format("A UInt32 type range's length must be a multiple of 4: {0}-{1}", list[i].Start,
                                                          list[i].End));
                            return false;
                        }
                        break;
                    case TypeOfValues.UInt64:
                        if (len%8 != 0)
                        {
                            MessageBox.Show(String.Format("A UInt64 type range's length must be a multiple of 8: {0}-{1}", list[i].Start,
                                                          list[i].End));
                            return false;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i].End > list[i + 1].Start)
                {
                    MessageBox.Show(String.Format("Ranges are overlapping: {0}-{1} and {2}-{3}", list[i].Start, list[i].End,
                                                  list[i + 1].Start, list[i + 1].End));
                    return false;
                }
            }
            return true;
        }

        private void btnFilesRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedIndex == -1)
                return;

            MainWindow.cp.Files.Remove(lstFiles.SelectedItem.ToString());
        }

        private void rbAutoDetectCustom_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.cp.RangeType = RangeType.AutoDetectCustomHeader;
                dgRanges.IsEnabled = false;
                txtAutoDetectValueCount.IsEnabled = true;
                //cmbDatatype.IsEnabled = true;
            }
            catch
            {
            }
        }
    }
}