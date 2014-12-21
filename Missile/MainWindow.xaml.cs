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
using Microsoft.Win32;

namespace Missile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ComputeParams computeParams;

        Aircraft aircraft;
        bool LMBDown = false;
        Point basePos;
        Vector savedState = new Vector(0.0, 0.0), currentState;

        public MainWindow()
        {
            InitializeComponent();
            computeParams = new ComputeParams();
            computeParams.Vt = 10.43453;
            MainGrid.DataContext = computeParams;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                aircraft = new Aircraft(dlg.FileName);
                ModelVisual3D mv3d = new ModelVisual3D();
                mv3d.Content = aircraft.Model;
                Pic3d.Children.Add(mv3d);
            }
        }

        private void Pic3d_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                LMBDown = true;
                basePos = e.GetPosition(Pic3d);
                //savedState = DirLight.Transform;
                Pic3d.CaptureMouse();
            }
        }

        private void Pic3d_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                LMBDown = false;
                Pic3d.ReleaseMouseCapture();
                savedState = currentState;
            }
        }

        private void Pic3d_MouseMove(object sender, MouseEventArgs e)
        {
            if (LMBDown)
            {
                Vector p = basePos - e.GetPosition(Pic3d);
                currentState = savedState + p;
                Transform3DGroup gr = new Transform3DGroup();
                gr.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1.0, 0.0, 0.0), currentState.Y)));
                gr.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0.0, 1.0, 0.0), currentState.X)));
                ((PerspectiveCamera)Pic3d.Camera).Transform = gr;
                DirLight.Transform = gr;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            aircraft.parts[0].Damage = 1.0;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            int N = int.Parse(txtN.Text);
            ulong destroyed = 0;
            Computes c = new Computes();
            c.Init(aircraft, computeParams);
            for (int i = 0; i < N; i++)
            {
                if (c.Iterate())
                {
                    destroyed++;
                }
            }
            txtSum.Text = destroyed.ToString();
            txtChance.Text = ((float)destroyed / (float)N).ToString();
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }
    }
}
