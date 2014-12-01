using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
        public ObservableCollection<Part> parts = new ObservableCollection<Part>();
        public Edge[] contour;
        public Edge[] contour0;

        public Aircraft(string xmlFileName)
        {
            XDocument doc = XDocument.Load(xmlFileName);
            model = new Model3DGroup();
            FileInfo fi = new FileInfo(xmlFileName);
            Part tmpPart;

            foreach(XElement part in doc.Root.Elements(XName.Get("Part")))
            {
                tmpPart = new Part(Path.Combine(fi.DirectoryName, part.Attribute(XName.Get("ModelFileName")).Value));
                tmpPart.A = Double.Parse(part.Attribute(XName.Get("A")).Value, CultureInfo.InvariantCulture);
                tmpPart.Delta = Double.Parse(part.Attribute(XName.Get("Delta")).Value, CultureInfo.InvariantCulture);
                tmpPart.ID = Int32.Parse(part.Attribute(XName.Get("ID")).Value, CultureInfo.InvariantCulture);
                tmpPart.J = Double.Parse(part.Attribute(XName.Get("J")).Value, CultureInfo.InvariantCulture);
                tmpPart.L = Double.Parse(part.Attribute(XName.Get("L")).Value, CultureInfo.InvariantCulture);
                tmpPart.SigmaS = Double.Parse(part.Attribute(XName.Get("SigmaS")).Value, CultureInfo.InvariantCulture);
                if(part.Attribute(XName.Get("GroupID")) != null)
                {
                    tmpPart.GroupID = Int32.Parse(part.Attribute(XName.Get("GroupID")).Value, CultureInfo.InvariantCulture);
                }
                if (part.Attribute(XName.Get("Required")) != null)
                {
                    switch (part.Attribute(XName.Get("Required")).Value)
                    {
                        case "1":
                        case "true":
                        case "True":
                            {
                                tmpPart.Requried = true;
                                break;
                            }
                        case "0":
                        case "false":
                        case "False":
                            {
                                tmpPart.Requried = false;
                                break;
                            }
                        default:
                            {
                                throw new ArgumentOutOfRangeException("Required");
                            }
                    }
                }
                else
                {
                    tmpPart.Requried = false;
                }
                parts.Add(tmpPart);
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

    public class Edge
    {
        public Point3D p1, p2;
    }
}
