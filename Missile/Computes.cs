using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Missile
{
    class Computes
    {
        Aircraft ac;
        ComputeParams par;
        double epsilon;
        double Vrel;
        double q;

        public void Init(Aircraft aircraft, ComputeParams computeParams)
        {
            this.ac = aircraft;
            this.par = computeParams;

            //
            // Part 1
            //

            epsilon = Math.Atan((par.Vt * Math.Sin(par.ae)) / (par.Vt * Math.Cos(par.ae) + par.Vm));
            Vrel = Math.Sqrt((par.Vm * par.Vm) + (par.Vt * par.Vt) + (2 * par.Vt * par.Vm * Math.Cos(par.ae)));
            q = par.ae - epsilon;
            double sinq = Math.Sin(q);
            double sinalphaH = Math.Sin(par.alphaH);
            double cosq = Math.Cos(q);
            double cosalphaH = Math.Cos(par.alphaH);
            Point3D vertex;
            // Model translate
            for (int partN = 0; partN < ac.parts.Count(); partN++)
            {
                Part curPart = ac.parts[partN];
                curPart.MeshContentB.Clear();
                for (int triN = 0; triN < curPart.MeshContent.Count; triN++)
                {
                    STLData stldata = new STLData();
                    for (int j = 0; j < 3; j++)
                    {
                        vertex = curPart.MeshContent[triN].Vertexes[j];
                        stldata.Vertexes[j].X = -(vertex.X * cosq) + (vertex.Y * sinalphaH) - (vertex.Z * sinq * cosalphaH);
                        stldata.Vertexes[j].Y = (vertex.Y * cosalphaH) + (vertex.Z * sinalphaH);
                        stldata.Vertexes[j].Z = (vertex.X * sinq) + (vertex.Y * cosq * sinalphaH) - (vertex.Z * cosq * cosalphaH);
                    }
                    vertex = curPart.MeshContent[triN].Normal;
                    stldata.Normal.X = -(vertex.X * cosq) + (vertex.Y * sinalphaH) - (vertex.Z * sinq * cosalphaH);
                    stldata.Normal.Y = (vertex.Y * cosalphaH) + (vertex.Z * sinalphaH);
                    stldata.Normal.Z = (vertex.X * sinq) + (vertex.Y * cosq * sinalphaH) - (vertex.Z * cosq * cosalphaH);
                    curPart.MeshContentB.Add(stldata);
                }
            }
            // Contour translate
            ac.contour0 = new Edge[ac.contour.Count()];
            for (int i = 0; i < ac.contour.Count(); i++)
            {
                vertex = ac.contour[i].p1;
                ac.contour0[i].p1.X = -(vertex.X * cosq) + (vertex.Y * sinalphaH) - (vertex.Z * sinq * cosalphaH);
                ac.contour0[i].p1.Y = (vertex.Y * cosalphaH) + (vertex.Z * sinalphaH);
                ac.contour0[i].p1.Z = (vertex.X * sinq) + (vertex.Y * cosq * sinalphaH) - (vertex.Z * cosq * cosalphaH);

                vertex = ac.contour[i].p2;
                ac.contour0[i].p2.X = -(vertex.X * cosq) + (vertex.Y * sinalphaH) - (vertex.Z * sinq * cosalphaH);
                ac.contour0[i].p2.Y = (vertex.Y * cosalphaH) + (vertex.Z * sinalphaH);
                ac.contour0[i].p2.Z = (vertex.X * sinq) + (vertex.Y * cosq * sinalphaH) - (vertex.Z * cosq * cosalphaH);
            }
        }

        public bool Iterate()
        {
            //
            // Part 2
            //

            double y, z;
            {
                double alpha, beta;
                RandVal(0.0, 1.0, out alpha, 0.0, 1.0, out beta);
                y = par.y + alpha * par.sigmaY;
                z = par.z + beta * par.sigmaZ;
            }

            //
            // Part 3
            //

            {
                Point3D p = new Point3D(0.0, y, z);
                foreach (Part part in ac.parts)
                {
                    foreach (STLData facet in part.MeshContentB)
                    {
                        if (IsPointInTriangle(p, facet.Vertexes[0], facet.Vertexes[1], facet.Vertexes[2]))
                        {
                            return true;
                        }
                    }
                }
            }

            //
            // Part 4
            //

            double x;
            {
                double[] xpi = new double[ac.contour.Count()];

            }

            return false;
        }

        void RandVal(double mean1, double sigma1, out double rand1, double mean2, double sigma2, out double rand2)
        {
            double u1, u2, v1, v2, s, z1, z2;

            Random rnd = new Random();
            do
            {
                u1 = rnd.NextDouble();  // a uniform random number from 0 to 1
                u2 = rnd.NextDouble();
                v1 = 2.0 * u1 - 1.0;
                v2 = 2.0 * u2 - 1.0;
                s = v1 * v1 + v2 * v2;
            } while (s >= 1.0);

            z1 = Math.Sqrt(-2.0 * Math.Log(s) / s) * v1;
            z2 = Math.Sqrt(-2.0 * Math.Log(s) / s) * v2;
            rand1 = (z1 * sigma1 + mean1);
            rand2 = (z2 * sigma2 + mean2);
            return;
        }

        static bool IsPointInTriangle(Point3D p, Point3D p0, Point3D p1, Point3D p2)
        {
            double s = (p0.Y * p2.Z - p0.Z * p2.Y + (p2.Y - p0.Y) * p.Z + (p0.Z - p2.Z) * p.Y);
            double t = (p0.Z * p1.Y - p0.Y * p1.Z + (p0.Y - p1.Y) * p.Z + (p1.Z - p0.Z) * p.Y);

            if (Math.Sign(s) != Math.Sign(t))
                return false;

            var A = (-p1.Y * p2.Z + p0.Y * (-p1.Z + p2.Z) + p0.Z * (p1.Y - p2.Y) + p1.Z * p2.Y);

            return (Math.Sign(t) == Math.Sign(A));
        }
    }
}
