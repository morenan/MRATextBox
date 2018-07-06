using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Morenan.MRATextBox
{
    internal class ResourceManager
    {
        private static readonly ResourceDictionary Resources;

        static ResourceManager()
        {
            Resources = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Morenan.MRATextBox;component/Resources/Theme_Default.xaml") }; 
        }

        public static object Get(string name)
        {
            return Resources.Contains(name) ? Resources[name] : null;
        }
    }
}
