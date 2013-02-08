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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace HexOnSteroids
{
    /// <summary>
    ///     Interaction logic for CategoriesWindow.xaml
    /// </summary>
    public partial class CategoriesWindow : Window
    {
        public CategoriesWindow()
        {
            InitializeComponent();

            RefreshCategoriesList();
        }

        private void btnCategoriesAdd_Click(object sender, RoutedEventArgs e)
        {
            var ibw = new InputBoxWindow("Enter the name of the new category");
            if (ibw.ShowDialog() == true)
            {
                string fn = MainWindow.input;
                if (!Helper.IsValidFilename(fn))
                {
                    MessageBox.Show(String.Format("'{0}' is not a valid filename.", fn));
                    return;
                }

                if (Directory.Exists(MainWindow.ProfilesPath + "\\" + fn))
                {
                    MessageBox.Show(String.Format("'{0}' already exists as a category.", fn));
                    return;
                }

                Directory.CreateDirectory(MainWindow.ProfilesPath + "\\" + fn);
                RefreshCategoriesList();

                lstCategories.SelectedItem = fn;
            }
        }

        private void RefreshCategoriesList()
        {
            lstCategories.Items.Clear();
            Directory.GetDirectories(MainWindow.ProfilesPath).ToList().ForEach(d => lstCategories.Items.Add(Helper.GetFolderName(d)));
        }

        private void btnCategoriesRename_Click(object sender, RoutedEventArgs e)
        {
            if (lstCategories.SelectedIndex == -1)
                return;

            var ibw = new InputBoxWindow("Enter the new name for the category", lstCategories.SelectedItem.ToString());
            if (ibw.ShowDialog() == true)
            {
                string fn = MainWindow.input;
                if (!Helper.IsValidFilename(fn))
                {
                    MessageBox.Show(String.Format("'{0}' is not a valid filename.", fn));
                    return;
                }

                if (Directory.Exists(MainWindow.ProfilesPath + "\\" + fn))
                {
                    MessageBox.Show(String.Format("'{0}' already exists as a category.", fn));
                    return;
                }

                Directory.Move(MainWindow.ProfilesPath + "\\" + lstCategories.SelectedItem, MainWindow.ProfilesPath + "\\" + fn);
                RefreshCategoriesList();

                lstCategories.SelectedItem = fn;
            }
        }

        private void btnCategoriesRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstCategories.SelectedIndex == -1)
                return;

            MessageBoxResult r =
                MessageBox.Show("Are you sure you want to delete this category? All profiles inside this category will be deleted!",
                                "Hex on Steroids", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (r == MessageBoxResult.Yes)
            {
                Directory.Delete(MainWindow.ProfilesPath + "\\" + lstCategories.SelectedItem);
                int index = lstCategories.SelectedIndex;
                RefreshCategoriesList();
                lstCategories.SelectedIndex = index - 1;
            }
        }

        private void btnProfilesAdd_Click(object sender, RoutedEventArgs e)
        {
            if (lstCategories.SelectedIndex == -1)
            {
                MessageBox.Show("You need to select a category first.");
                return;
            }

            var ibw = new InputBoxWindow("Enter the name of the new profile");
            if (ibw.ShowDialog() == true)
            {
                string fn = MainWindow.input;
                if (!Helper.IsValidFilename(fn))
                {
                    MessageBox.Show(String.Format("'{0}' is not a valid filename.", fn));
                    return;
                }

                if (Directory.Exists(MainWindow.ProfilesPath + "\\" + lstCategories.SelectedItem + "\\" + fn))
                {
                    MessageBox.Show(String.Format("'{0}' already exists as a profile.", fn));
                    return;
                }

                FileStream fs = File.Create(MainWindow.ProfilesPath + "\\" + lstCategories.SelectedItem + "\\" + fn);
                fs.Close();
                RefreshProfilesList();

                lstProfiles.SelectedItem = fn;
            }
        }

        private void lstCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCategories.SelectedIndex == -1)
                return;
            RefreshProfilesList();
        }

        private void RefreshProfilesList()
        {
            lstProfiles.Items.Clear();
            Directory.GetFiles(MainWindow.ProfilesPath + "\\" + lstCategories.SelectedItem)
                     .ToList()
                     .ForEach(f => lstProfiles.Items.Add(Path.GetFileName(f)));
        }

        private void btnProfilesRename_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedIndex == -1)
                return;

            var ibw = new InputBoxWindow("Enter the new name for the profile", lstCategories.SelectedItem.ToString());
            if (ibw.ShowDialog() == true)
            {
                string fn = MainWindow.input;
                if (!Helper.IsValidFilename(fn))
                {
                    MessageBox.Show(String.Format("'{0}' is not a valid filename.", fn));
                    return;
                }

                if (Directory.Exists(MainWindow.ProfilesPath + "\\" + lstCategories.SelectedItem + "\\" + fn))
                {
                    MessageBox.Show(String.Format("'{0}' already exists as a profile.", fn));
                    return;
                }

                File.Move(MainWindow.ProfilesPath + "\\" + lstCategories.SelectedItem + "\\" + lstProfiles.SelectedItem,
                          MainWindow.ProfilesPath + "\\" + lstCategories.SelectedItem + "\\" + fn);
                RefreshProfilesList();

                lstProfiles.SelectedItem = fn;
            }
        }

        private void btnProfilesRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedIndex == -1)
                return;

            MessageBoxResult r = MessageBox.Show("Are you sure you want to delete this profile?", "Hex on Steroids", MessageBoxButton.YesNo,
                                                 MessageBoxImage.Question);

            if (r == MessageBoxResult.Yes)
            {
                Directory.Delete(MainWindow.ProfilesPath + "\\" + lstCategories.SelectedItem + "\\" + lstProfiles.SelectedItem);
                int index = lstProfiles.SelectedIndex;
                RefreshProfilesList();
                lstProfiles.SelectedIndex = index - 1;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}