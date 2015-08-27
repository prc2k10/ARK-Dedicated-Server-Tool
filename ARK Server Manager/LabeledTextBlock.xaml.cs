using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ARK_Server_Manager
{
    /// <summary>
    /// Interaction logic for LabeledTextBlock.xaml
    /// </summary>
    public partial class LabeledTextBlock : UserControl
    {    
        // Added Dependency Properties
        public static readonly DependencyProperty InputTextProperty = DependencyProperty.Register(nameof(InputText), typeof(String), typeof(LabeledTextBlock), new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });
        public static readonly DependencyProperty HasLockBoxProperty = DependencyProperty.Register(nameof(HasLockBox), typeof(Boolean), typeof(LabeledTextBlock));
        public static readonly DependencyProperty HasCheckBoxProperty = DependencyProperty.Register(nameof(HasCheckBox), typeof(Boolean), typeof(LabeledTextBlock));



        #region Public Dependency Properties
        [Category("Custom Properties")]
        public Boolean HasCheckBox
        {
            get { return (Boolean)GetValue(HasCheckBoxProperty); }
            set { SetValue(HasCheckBoxProperty, value); }
        }

        [Category("Custom Properties")]
        public String InputText
        {
            get { return (String)GetValue(InputTextProperty); }
            set { SetValue(InputTextProperty, value); }
        }

        [Category("Custom Properties")]
        public Boolean HasLockBox
        {
            get { return (Boolean)GetValue(HasLockBoxProperty); }
            set { SetValue(HasLockBoxProperty, value); }
                    
        }
        #endregion

        public LabeledTextBlock()
        {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }


    }
}
