using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace Missile
{
    class Aircraft : INotifyPropertyChanged
    {
        Model3DGroup model;
        ObservableCollection<Part> parts = new ObservableCollection<Part>();

        public Aircraft(string xmlFileName)
        {
            XDocument doc = XDocument.Load(xmlFileName);
            model = new Model3DGroup();
            FileInfo fi = new FileInfo(xmlFileName);

            foreach(XElement part in doc.Root.Elements(XName.Get("Part")))
            {
                parts.Add(new Part(Path.Combine(fi.DirectoryName, part.Attribute(XName.Get("ModelFileName")).Value)));
            }
            foreach(Part part in parts)
            {
                model.Children.Add(part.Model);
            }
        }

        public Model3DGroup Model
        {
            set
            {
                if (model != value)
                {
                    model = value;
                    NotifyPropertyChanged();
                }
            }
            get
            {
                return model;
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                //lock (PropertyChanged) //null exception!
                {
                    PropertyChanged += value;
                }
            }
            remove
            {
                lock (PropertyChanged)
                {
                    PropertyChanged -= value;
                }
            }
        }
        #endregion
    }
}
