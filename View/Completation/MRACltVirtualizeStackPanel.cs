using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Morenan.MRATextBox.View.Completation
{
    internal class MRACltVirtualizeStackPanel : VirtualizingStackPanel
    {
        public MRACltVirtualizeStackPanel()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                FrameworkElement fele = (FrameworkElement)sender;
                while (fele != null)
                {
                    if (fele is MRACltBox)
                    {
                        MRACltBox cltbox = (MRACltBox)fele;
                        cltbox.ItemPanel = this;
                        if (cltbox.SelectedIndex >= 0)
                            cltbox.ScrollIntoMiddle(cltbox.SelectedIndex);
                        break;
                    }
                    if (fele.Parent is FrameworkElement)
                        fele = (FrameworkElement)(fele.Parent);
                    else if (fele.TemplatedParent is FrameworkElement)
                        fele = (FrameworkElement)(fele.TemplatedParent);
                    else
                        fele = null;
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
