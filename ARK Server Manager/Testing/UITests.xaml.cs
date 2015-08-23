using ARK_Server_Manager.Lib;
using System;
using System.Collections.Generic;
using System.IO;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //
            // Kick off the initialization.
            //
            TaskUtils.RunOnUIThreadAsync(() =>
            {
                // We need to load the set of existing servers, or create a blank one if we don't have any...
                foreach (var profile in Directory.EnumerateFiles(Config.Default.ConfigDirectory, "*" + Config.Default.ProfileExtension))
                {
                    try
                    {
                        ServerManager.Instance.AddFromPath(profile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(String.Format("The profile at {0} failed to load.  The error was: {1}\r\n{2}", profile, ex.Message, ex.StackTrace), "Profile failed to load", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }

            }).DoNotWait();
        }
    }
}