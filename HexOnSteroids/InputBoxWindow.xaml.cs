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

using System.Windows;

#endregion

namespace HexOnSteroids
{
    /// <summary>
    ///     Interaction logic for Window1.xaml
    /// </summary>
    public partial class InputBoxWindow
    {
        public InputBoxWindow(string message)
        {
            InitializeComponent();

            lblMessage.Content = message;

            txtInput.Focus();
        }

        public InputBoxWindow(string message, string defaultValue) : this(message)
        {
            txtInput.Text = defaultValue;
            txtInput.SelectAll();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.input = txtInput.Text;
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.input = "";
            DialogResult = false;
            Close();
        }
    }
}