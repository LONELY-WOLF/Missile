using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Missile
{
    class Part : INotifyPropertyChanged
    {
        GeometryModel3D model;
        double damage = 0.0;

        List<STLData> MeshContent = new List<STLData>();

        #region Compute params
        public int ID = -1;
        public int GroupID = -1;
        public bool Requried = false;
        /// <summary>
        /// Значение удельного импульса ударной волны,
        /// при котором возникает разрушение отсека, кгс*с/м^2
        /// </summary>
        public double J;
        /// <summary>
        /// Толщина оболочки отсека, мм
        /// </summary>
        public double Delta;
        /// <summary>
        /// Предел текучести материала оболочки отсека
        /// </summary>
        public double SigmaS;
        /// <summary>
        /// Расстояние между силовыми элементами подкрепляющего набора отсека, м
        /// </summary>
        public double A;
        /// <summary>
        /// Критическое значение глубины внедрения стержневого кольца в конструкцию, мм
        /// </summary>
        public double L;
        #endregion

        public Part(string stlPath)
        {
            STLData stldata = new STLData();
            char[] separator = new char[] {'\t', ' '};
            MeshGeometry3D mesh = new MeshGeometry3D();
            DiffuseMaterial materia = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)));
            string[] lines = File.ReadAllLines(stlPath);
            for (int i = 0; i < lines.Count(); i++ )
            {
                string[] nums = lines[i].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (nums[0] == "facet")
                {
                    stldata.Normal = new Point3D(Double.Parse(nums[2], CultureInfo.InvariantCulture), Double.Parse(nums[3], CultureInfo.InvariantCulture), Double.Parse(nums[4], CultureInfo.InvariantCulture));
                    i += 2;
                    for (int j = 0; j < 3; j++)
                    {
                        nums = lines[i].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        if (nums[0] == "vertex")
                        {
                            stldata.Vertexes[j] = new Point3D(Double.Parse(nums[1], CultureInfo.InvariantCulture), Double.Parse(nums[2], CultureInfo.InvariantCulture), Double.Parse(nums[3], CultureInfo.InvariantCulture));
                            mesh.Positions.Add(stldata.Vertexes[j]);
                        }
                        i++;
                    }
                    MeshContent.Add(stldata);
                }
                i += 2;
            }
            model = new GeometryModel3D(mesh, materia);
        }

        public GeometryModel3D Model
        {
            set
            {
                if(model != value)
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

        public double Damage
        {
            set
            {
                if (damage != value)
                {
                    damage = value;
                    model.Material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, (byte)(damage * 255.0), (byte)((1.0 - damage) * 255.0), 0)));
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Model");
                }
            }
            get
            {
                return damage;
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

    public struct STLData
    {
        public Point3D[] Vertexes = new Point3D[3];
        public Point3D Normal;
    }
}
