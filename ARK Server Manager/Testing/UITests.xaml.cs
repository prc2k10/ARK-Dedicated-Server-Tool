using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ARK_Server_Manager
{
	/// <summary>
	/// Interaction logic for UITests.xaml
	/// </summary>
	public partial class UITests : Window
	{
		public UITests()
		{
			this.InitializeComponent();
			
			// Insert code required on object creation below this point.
		}

        // Due to a shortcoming in Blend, we can't have a selected item for the listbox prior to runtime.
        // To prevent having NO item selected at runtime, select the first item on load.
        private void listBox_Loaded(object sender, RoutedEventArgs e)
        {
            ListBox lb = (ListBox)(e.Source);
            if (lb.Items.Count > 0)
            {
                lb.SelectedIndex = 0;
            }

        }

        private void ShowDetailView(object sender, RoutedEventArgs e)
        {
            serverDetailCtrl.BeginStoryboard(serverDetailCtrl.FindResource("show") as System.Windows.Media.Animation.Storyboard);
        }
    }
}