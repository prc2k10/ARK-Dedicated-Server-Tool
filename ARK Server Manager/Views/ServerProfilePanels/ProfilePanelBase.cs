using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ARK_Server_Manager.Views.ServerProfilePanels
{
    public partial class ProfilePanelBase : UserControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            private set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(ProfilePanelBase), new PropertyMetadata("SET ME"));

        protected ProfilePanelBase(string title)
        {
            this.Title = title;            
        }
    }
}
