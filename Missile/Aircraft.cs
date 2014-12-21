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

            foreach (XElement part in doc.Root.Elements("Part"))
            {
                tmpPart = new Part(Path.Combine(fi.DirectoryName, part.Attribute("ModelFileName").Value));
                tmpPart.Name = part.Attribute("Name").Value;
                tmpPart.A = Double.Parse(part.Attribute("A").Value, CultureInfo.InvariantCulture);
                tmpPart.Delta = Double.Parse(part.Attribute("Delta").Value, CultureInfo.InvariantCulture);
                tmpPart.ID = Int32.Parse(part.Attribute("ID").Value, CultureInfo.InvariantCulture);
                tmpPart.J = Double.Parse(part.Attribute("J").Value, CultureInfo.InvariantCulture);
                tmpPart.L = Double.Parse(part.Attribute("L").Value, CultureInfo.InvariantCulture);
                tmpPart.SigmaS = Double.Parse(part.Attribute("SigmaS").Value, CultureInfo.InvariantCulture);
                if (part.Attribute("GroupID") != null)
                {
                    tmpPart.GroupID = Int32.Parse(part.Attribute("GroupID").Value, CultureInfo.InvariantCulture);
                }
                if (part.Attribute("Required") != null)
                {
                    switch (part.Attribute("Required").Value)
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
            foreach (Part part in parts)
            {
                model.Children.Add(part.Model);
            }
            XElement c = doc.Root.Element("Contour");
            var e = c.Elements("Edge");
            contour = new Edge[e.Count()];
            for (int i = 0; i < e.Count(); i++)
            {
                Edge edge = new Edge();
                edge.p1.X = Double.Parse(e.ElementAt(i).Element("StartPoint").Attribute("X").Value, CultureInfo.InvariantCulture);
                edge.p1.Z = Double.Parse(e.ElementAt(i).Element("StartPoint").Attribute("Z").Value, CultureInfo.InvariantCulture);
                edge.p2.X = Double.Parse(e.ElementAt(i).Element("EndPoint").Attribute("X").Value, CultureInfo.InvariantCulture);
                edge.p2.Z = Double.Parse(e.ElementAt(i).Element("EndPoint").Attribute("Z").Value, CultureInfo.InvariantCulture);
                contour[i] = edge;
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
