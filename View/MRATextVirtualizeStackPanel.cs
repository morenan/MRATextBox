using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Morenan.MRATextBox.View
{
    internal class MRATextVirtualizeStackPanel : VirtualizingStackPanel
    {
        public MRATextVirtualizeStackPanel()
        {
            Orientation = Orientation.Vertical;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private MRATextControl parent;
        public MRATextControl ViewParent { get { return this.parent; } }
        
        #region Event Handler

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement)
            {
                FrameworkElement fele = (FrameworkElement)sender;
                while (fele != null)
                {
                    if (fele is MRATextControl)
                    {
                        this.parent = (MRATextControl)(fele);
                        parent.UI_Stack = this;
                    }
                    if (fele is ScrollViewer)
                        ScrollOwner = (ScrollViewer)(fele);
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
            //parent.UI_Stack = null;
            //this.parent = null;
        }

        #endregion
    }
}
