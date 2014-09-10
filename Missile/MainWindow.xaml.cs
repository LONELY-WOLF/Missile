using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Missile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Aircraft a = new Aircraft();
            ModelVisual3D mv3d = new ModelVisual3D();
            GeometryModel3D model;
            Pic3d.Children.RemoveAt(1);
            MeshGeometry3D geom = new MeshGeometry3D();
            //Positions="0 0 0  1 0 0  0 1 0  1 1 0  0 0 1  1 0 1  0 1 1  1 1 1"
            //TriangleIndices="2 3 1  2 1 0  7 1 3  7 5 1  6 5 7  6 4 5  6 2 0  2 0 4  2 7 3  2 6 7  0 1 5  0 5 4"
            geom.Positions.Add(new Point3D(0.0, 0.0, 0.0));
            geom.Positions.Add(new Point3D(1.0, 0.0, 0.0));
            geom.Positions.Add(new Point3D(0.0, 1.0, 0.0));
            geom.Positions.Add(new Point3D(1.0, 1.0, 0.0));
            geom.Positions.Add(new Point3D(0.0, 0.0, 1.0));
            geom.Positions.Add(new Point3D(1.0, 0.0, 1.0));
            geom.Positions.Add(new Point3D(0.0, 1.0, 1.0));
            geom.Positions.Add(new Point3D(1.0, 1.0, 1.0));
            foreach (string item in "2 3 1  2 1 0  7 1 3  7 5 1  6 5 7  6 4 5  6 2 0  2 0 4  2 7 3  2 6 7  0 1 5  0 5 4".Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries))
            {
                geom.TriangleIndices.Add(int.Parse(item));
            }
            DiffuseMaterial materia = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(128, 128, 128)));
            model = new GeometryModel3D(geom, materia);
            mv3d.Content = model;
            Pic3d.Children.Add(mv3d);
        }
    }
}
