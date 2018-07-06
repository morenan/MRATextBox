using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media;
using System.Windows.Controls;

namespace Morenan.MRATextBox.View
{
    internal class MRATextItemDrawingVisual : DrawingVisual
    {
        public MRATextItemDrawingVisual(MRATextItemView _parent)
        {
            this.parent = _parent;
        }

        #region Number

        private MRATextItemView parent;
        public MRATextItemView ViewParent { get { return this.parent; } }
        public IMRATextItemInfo Core { get { return parent.Core; } }

        #endregion

        #region Method

        public virtual void InvalidateVisual()
        {
            using (DrawingContext ctx = RenderOpen())
            {
                Render(ctx);
            }
        }

        protected virtual void Render(DrawingContext ctx)
        {
        }

        #endregion


    }
}
