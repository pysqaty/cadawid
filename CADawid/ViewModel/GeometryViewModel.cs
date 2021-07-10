using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CADawid.Model;
using CADawid.Utils;
using SharpDX;

namespace CADawid.ViewModel
{
    public abstract class GeometryViewModel : INotifyPropertyChanged
    {
        public IGeometryObject SelectedObject { get; set; }
        public abstract void Visit(IGeometryPanel visitor);
        public abstract void VisitToAdd(IGeometryAddPanel visitor);

        #region Property changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
