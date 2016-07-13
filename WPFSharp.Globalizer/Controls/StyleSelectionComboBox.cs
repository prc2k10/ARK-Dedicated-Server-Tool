using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WPFSharp.Globalizer.Controls
{
    public partial class StyleSelectionComboBox : ComboBox
    {
        public StyleSelectionComboBox()
        {
            var itemSourceBinding = new Binding
            {
                Source = AvailableStyles.Instance,
                BindsDirectlyToSource = true
            };
            SetBinding(ItemsSourceProperty, itemSourceBinding);

            ItemTemplate = new DataTemplate();
            var binding = new Binding();
            var textElement = new FrameworkElementFactory(typeof(TextBlock));
            textElement.SetBinding(TextBlock.TextProperty, binding);
            ItemTemplate.VisualTree = textElement;

            SelectionChanged += StyleSelectionComboBox_SelectionChanged;

            var selectedItemBinding = new Binding(nameof(AvailableStyles.SelectedStyle))
            {
                Source = AvailableStyles.Instance,
                Mode = BindingMode.OneWay
            };
            SetBinding(SelectedItemProperty, selectedItemBinding);
        }

        void StyleSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var style = e.AddedItems[0].ToString();
            if (!string.IsNullOrWhiteSpace(style))
                GlobalizedApplication.Instance.StyleManager.SwitchStyle(style);
        }
    }
}
