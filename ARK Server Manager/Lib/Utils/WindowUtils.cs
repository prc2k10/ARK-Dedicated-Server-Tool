using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ARK_Server_Manager.Lib
{
    public static class WindowUtils
    {
        private static string DEFAULT_LANGUAGE_DICTIONARY = @"Globalization\en-US\en-US.xaml";
        private static string DEFAULT_STYLE_DICTIONARY = @"Styles\Default.xaml";

        public static void RemoveDefaultResourceDictionaries(Window window)
        {
            if (window == null)
                return;

            var dictToRemove = window.Resources.MergedDictionaries.FirstOrDefault(d => d.Source.OriginalString.Contains(DEFAULT_LANGUAGE_DICTIONARY));
            if (dictToRemove != null)
            {
                window.Resources.MergedDictionaries.Remove(dictToRemove);
            }

            dictToRemove = window.Resources.MergedDictionaries.FirstOrDefault(d => d.Source.OriginalString.Contains(DEFAULT_STYLE_DICTIONARY));
            if (dictToRemove != null)
            {
                window.Resources.MergedDictionaries.Remove(dictToRemove);
            }
        }

        public static void RemoveDefaultResourceDictionaries(UserControl control)
        {
            if (control == null)
                return;

            var dictToRemove = control.Resources.MergedDictionaries.FirstOrDefault(d => d.Source.OriginalString.Contains(DEFAULT_LANGUAGE_DICTIONARY));
            if (dictToRemove != null)
            {
                control.Resources.MergedDictionaries.Remove(dictToRemove);
            }

            dictToRemove = control.Resources.MergedDictionaries.FirstOrDefault(d => d.Source.OriginalString.Contains(DEFAULT_STYLE_DICTIONARY));
            if (dictToRemove != null)
            {
                control.Resources.MergedDictionaries.Remove(dictToRemove);
            }
        }
    }
}
