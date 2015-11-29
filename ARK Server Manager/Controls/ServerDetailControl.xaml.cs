using ARK_Server_Manager.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using ARK_Server_Manager.Lib.ViewModel;
using ARK_Server_Manager.Views.ServerProfilePanels;

namespace ARK_Server_Manager
{
    /// <summary>
    /// Interaction logic for ServerDetailControl.xaml
    /// </summary>
    public partial class ServerDetailControl : UserControl
    {


        public ObservableCollection<ProfilePanelBase> ProfilePanels
        {
            get { return (ObservableCollection<ProfilePanelBase>)GetValue(ProfilePanelsProperty); }
            private set { SetValue(ProfilePanelsProperty, value); }
        }

        public static readonly DependencyProperty ProfilePanelsProperty = DependencyProperty.Register(nameof(ProfilePanels), typeof(ObservableCollection<ProfilePanelBase>), typeof(ServerDetailControl), new PropertyMetadata(null));


        public ProfilePanelBase CurrentPanel
        {
            get { return (ProfilePanelBase)GetValue(CurrentPanelProperty); }
            set { SetValue(CurrentPanelProperty, value); }
        }

        public static readonly DependencyProperty CurrentPanelProperty = DependencyProperty.Register(nameof(CurrentPanel), typeof(ProfilePanelBase), typeof(ServerDetailControl), new PropertyMetadata(null));        

        Server defaultSrv = Server.FromDefaults();

        public ServerDetailControl()
        {
            // Create a default server for the control to avoid binding errors on initialization

            this.DataContext = defaultSrv;
            InitializeComponent();

            this.ProfilePanels = new ObservableCollection<ProfilePanelBase>();
            this.ProfilePanels.Add(new Installation());
            this.ProfilePanels.Add(new Administration());
        }

        private void listBox_Loaded(object sender, RoutedEventArgs e)
        {
            // If no item is selected, select the first item.
            if (listBox.SelectedIndex < 0)
            {
                listBox.SelectedIndex = 0;
            }
        }
    }
}
