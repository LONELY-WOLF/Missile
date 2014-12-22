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
using System.ComponentModel;

namespace Missile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ComputeParams computeParams;
        ulong Sum = 0, SumBlast = 0, SumRod = 0, SumFragments = 0, SumDirectHit = 0;
        BackgroundWorker[] threads;
        int N;
        ulong[] DestroyedPartsCount;
        ulong[] ReasonsCount = new ulong[Enum.GetValues(typeof(DestroyType)).Length];

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
            GridInput.IsEnabled = false;
            int cores = Environment.ProcessorCount;
            N = int.Parse(txtN.Text);
            Sum = 0;
            SumBlast = 0;
            SumRod = 0;
            SumFragments = 0;
            SumDirectHit = 0;
            DestroyedPartsCount = new ulong[aircraft.parts.Count];
            Computes.Init(aircraft, computeParams);
            threads = new BackgroundWorker[cores];
            for (int i = 0; i < cores; i++)
            {
                threads[i] = new BackgroundWorker();
                threads[i].WorkerReportsProgress = true;
                threads[i].DoWork += MainWindow_DoWork;
                threads[i].ProgressChanged += MainWindow_ProgressChanged;
                threads[i].RunWorkerCompleted += MainWindow_RunWorkerCompleted;
                threads[i].RunWorkerAsync(N / cores);
            }
        }

        void MainWindow_DoWork(object sender, DoWorkEventArgs e)
        {
            ulong[] DestroyedPartsCount = new ulong[aircraft.parts.Count];
            ulong[] ReasonsCount = new ulong[Enum.GetValues(typeof(DestroyType)).Length];
            ReasonsCount.Initialize();
            int N = (int)e.Argument;
            ulong destroyed = 0;
            Computes c = new Computes(aircraft, computeParams);
            for (int i = 0; i < N; i++)
            {
                ComputeResult res = c.Iterate();
                switch (res.ExitReason)
                {
                    case DestroyType.Blast:
                        {
                            destroyed++;
                            AddDestroyedParts(DestroyedPartsCount, res.PartsDestroyed);
                            ReasonsCount[(int)res.ExitReason]++;
                            break;
                        }
                    case DestroyType.ContinuousRod:
                        {
                            destroyed++;
                            AddDestroyedParts(DestroyedPartsCount, res.PartsDestroyed);
                            ReasonsCount[(int)res.ExitReason]++;
                            break;
                        }
                    case DestroyType.DirectHit:
                        {
                            destroyed++;
                            AddDestroyedParts(DestroyedPartsCount, res.PartsDestroyed);
                            ReasonsCount[(int)res.ExitReason]++;
                            break;
                        }
                    case DestroyType.Fragments:
                        {
                            destroyed++;
                            AddDestroyedParts(DestroyedPartsCount, res.PartsDestroyed);
                            ReasonsCount[(int)res.ExitReason]++;
                            break;
                        }
                    default:
                        {
                            AddDestroyedParts(DestroyedPartsCount, res.PartsDestroyed);
                            ReasonsCount[(int)res.ExitReason]++;
                            break;
                        }
                }
            }
            e.Result = new ThreadResult(destroyed, DestroyedPartsCount, ReasonsCount);
        }

        static void AddDestroyedParts(ulong[] destroyedPartsCount, bool[] destroyedParts)
        {
            for (int i = 0; i < destroyedPartsCount.Count(); i++)
            {
                if (destroyedParts[i])
                {
                    destroyedPartsCount[i]++;
                }
            }
        }

        void MainWindow_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ThreadResult res = (ThreadResult)e.Result;

            Sum += res.Destroyed;
            SumBlast += res.ReasonsCount[(int)DestroyType.Blast];
            SumRod += res.ReasonsCount[(int)DestroyType.ContinuousRod];
            SumDirectHit += res.ReasonsCount[(int)DestroyType.DirectHit];
            DestroyedPartsCount = DestroyedPartsCount.Zip(res.DestroyedPartsCount, (outp, newp) => outp += newp).ToArray();

            if (threads.All(t => !t.IsBusy))
            {
                txtSum.Text = Sum.ToString();
                txtBlast.Text = SumBlast.ToString();
                txtRod.Text = SumRod.ToString();
                txtDirectHit.Text = SumDirectHit.ToString();
                txtChance.Text = ((float)Sum / (float)N).ToString();
                for (int i = 0; i < aircraft.parts.Count; i++)
                {
                    aircraft.parts[i].Damage = DestroyedPartsCount[i] / (float)N;
                }
                GridInput.IsEnabled = true;
            }
        }

        void MainWindow_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }
    }

    struct ThreadResult
    {
        public ulong Destroyed;
        public ulong[] DestroyedPartsCount;
        public ulong[] ReasonsCount;

        public ThreadResult(ulong destroyed, ulong[] destroyedPartsCount, ulong[] reasonsCount)
        {
            Destroyed = destroyed;
            DestroyedPartsCount = destroyedPartsCount;
            ReasonsCount = reasonsCount;
        }
    }
}
