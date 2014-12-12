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
        bool[] partsDestroyed;
        double A1, B1, C1, E1;

        public void Init(Aircraft aircraft, ComputeParams computeParams)
        {
            this.ac = aircraft;
            this.par = computeParams;

            partsDestroyed = new bool[ac.parts.Count()];

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

            double f0 = Vrel / par.Vo;
            double cose = Math.Cos(epsilon);
            double sine = Math.Sin(epsilon);
            double cosphi = Math.Cos(par.phi);
            double sinphi = Math.Sin(par.phi);
            A1 = Math.Pow(sine, 2.0) + Math.Pow(sinphi, 2.0);
            B1 = Math.Pow(cosphi + (f0 * cose), 2.0);
            C1 = Math.Pow(f0 + cosphi * cose, 2.0) - Math.Pow(sinphi, 2.0) * Math.Pow(sine, 2.0);
            E1 = -sine * (f0 * cosphi + cose);
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
                double[] xpi = new double[ac.contour0.Count()];
                double[] D = new double[2];
                double[] xr = new double[2];
                double xr0, yr0, zr0;
                for (int i = 0; i < xpi.Count(); i++)
                {
                    yr0 = ac.contour0[i].p1.Y - y;
                    zr0 = ac.contour0[i].p1.Z - z;
                    D[0] = (yr0 * yr0) * (Math.Pow(Math.Sin(par.gamma), 2.0) - Math.Pow(Math.Sin(epsilon), 2.0)) + (zr0 * zr0) * Math.Pow(Math.Sin(par.gamma), 2.0);
                    xr0 = ((-zr0 * Math.Sin(epsilon) * Math.Cos(epsilon)) + (Math.Cos(par.gamma) * Math.Sqrt(D[0])));
                    xr[0] = ac.contour0[i].p1.X - xr0;
                    D[0] = Math.Sqrt((xr0 * xr0) + (yr0 * yr0) + (zr0 * zr0));

                    yr0 = ac.contour0[i].p2.Y - y;
                    zr0 = ac.contour0[i].p2.Z - z;
                    D[1] = (yr0 * yr0) * (Math.Pow(Math.Sin(par.gamma), 2.0) - Math.Pow(Math.Sin(epsilon), 2.0)) + (zr0 * zr0) * Math.Pow(Math.Sin(par.gamma), 2.0);
                    xr0 = ((-zr0 * Math.Sin(epsilon) * Math.Cos(epsilon)) + (Math.Cos(par.gamma) * Math.Sqrt(D[1])));
                    xr[1] = ac.contour0[i].p2.X - xr0;
                    D[1] = Math.Sqrt((xr0 * xr0) + (yr0 * yr0) + (zr0 * zr0));

                    if ((D[0] <= par.D) && (D[1] <= par.D))
                    {
                        xpi[i] = (xr[0] < xr[1]) ? xr[0] : xr[1];
                    }
                    else if ((D[0] > par.D) && (D[1] > par.D))
                    {
                        xpi[i] = 100000.0;
                    }
                    else
                    {
                        double xp = xr[0] + ((xr[1] - xr[0]) / (D[1] - D[0])) * (par.D - D[0]);
                        if (D[0] <= par.D)
                        {
                            xpi[i] = (xp < xr[0]) ? xp : xr[0];
                        }
                        else
                        {
                            xpi[i] = (xp < xr[1]) ? xp : xr[1];
                        }
                    }
                }

                //
                // Part 5
                //

                double gamma, ret;

                RandVal(0.0, 1.0, out gamma, 0.0, 1.0, out ret);

                x = xpi.Min() + gamma * par.sigmaX + par.tau * Vrel;
            }

            //
            // Part 6
            //

            {
                double Rf;
                for (int i = 0; i < ac.parts.Count(); i++)
                {
                    Part part = ac.parts[i];
                    if (par.q >= 10.0)
                    {
                        //REM: A = 20 - 25
                        Rf = 22.5 * (Math.Pow(par.q, 2.0 / 3.0) / part.J);
                    }
                    else
                    {
                        //REM: Wpc0 = 0.1 for a0 = 0.6 - 0.8
                        double Wpc = 0.1 * (Math.Pow(part.A, 2.0) / (0.7 * 0.7));
                        Rf = (1000.0 * par.q) / (part.Delta * part.SigmaS * Wpc);
                    }

                    //REM!! LINQ madness!!
                    double DN = part.MeshContentB.Min(f => 0.25 * Math.Sqrt(Math.Pow(f.Vertexes.Sum(v => v.X) - x, 2.0) + Math.Pow(f.Vertexes.Sum(v => v.Y), 2.0) + Math.Pow(f.Vertexes.Sum(v => v.Z), 2.0)));
                    partsDestroyed[i] = DN < Rf;
                }
            }

            //
            // Part 7
            //

            if (CheckAircraftDestroyed())
            {
                return true;
            }

            //
            // Part 8
            //

            {
                double V01 = Math.Sqrt(Math.Pow(par.Vo, 2.0) + Math.Pow(par.Vm, 2.0) + 2.0 * par.Vo * par.Vm * Math.Cos(par.phi));

                Point3D k = new Point3D(), t = new Point3D();
                double l, A2, B2, C2, p;
                List<Point3D> points = new List<Point3D>();
                foreach (Part part in ac.parts)
                {
                    for (int i = 0; i < part.MeshContentB.Count; i++)
                    {

                        //
                        // Part 9
                        //

                        k = part.MeshContentB[i].Vertexes[0];
                        t = part.MeshContentB[i].Vertexes[1];
                        l = Math.Sqrt(Math.Pow(k.X - t.X, 2.0) + Math.Pow(k.Y - t.Y, 2.0) + Math.Pow(k.Z - t.Z, 2.0));

                        A2 = A1 * Math.Pow(k.X - t.X, 2.0);
                        A2 += B1 * Math.Pow(k.Y - t.Y, 2.0);
                        A2 += C1 * Math.Pow(k.Z - t.Z, 2.0);
                        A2 += 2 * E1 * (k.X - t.X) * (k.Z - t.Z);
                        A2 /= l * l;

                        B2 = A1 * t.X * (k.X - t.X);
                        B2 += B1 * t.Y * (k.Y - t.Y);
                        B2 += C1 * t.Z * (k.Z - t.Z);
                        B2 += E1 * t.X * (k.Z - t.Z);
                        B2 += E1 * t.Z * (k.X - t.X);
                        B2 *= 2 / l;

                        C2 = A1 * t.X * t.X;
                        C2 += B1 * t.Y * t.Y;
                        C2 += C1 * t.Z * t.Z;
                        C2 += 2 * E1 * t.X * t.Z;

                        double dis = (B2 * B2) - (4 * A2 * C2);
                        if (dis <= 0.0)
                        {
                            continue;
                        }
                        else
                        {
                            Point3D r = new Point3D();
                            p = -B2 + Math.Sqrt(dis) / (2 * A2);
                            r = t + (k - t) * p / l;
                            if (Math.Min(k.X, t.X) <= r.X &&
                                Math.Max(k.X, t.X) >= r.X &&
                                Math.Min(k.Y, t.Y) <= r.Y &&
                                Math.Max(k.Y, t.Y) >= r.Y &&
                                Math.Min(k.Z, t.Z) <= r.Z &&
                                Math.Max(k.Z, t.Z) >= r.Z)
                            {
                                points.Add(r);
                            }
                        }
                    }
                }
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

        bool CheckAircraftDestroyed()
        {
            Dictionary<int, int> groupsMax = new Dictionary<int, int>();
            Dictionary<int, int> groups = new Dictionary<int, int>();
            foreach (Part item in ac.parts)
            {
                if (item.GroupID >= 0)
                {
                    if (groupsMax.ContainsKey(item.GroupID))
                    {
                        groupsMax[item.GroupID]++;
                    }
                    else
                    {
                        groupsMax.Add(item.GroupID, 1);
                        groups.Add(item.GroupID, 0);
                    }
                }
            }

            for (int i = 0; i < ac.parts.Count(); i++)
            {
                if (partsDestroyed[i])
                {
                    Part part = ac.parts[i];
                    if (part.Requried)
                    {
                        return true;
                    }
                    else
                    {
                        if (part.GroupID >= 0)
                        {
                            groups[part.GroupID]++;
                            if (groups[part.GroupID] == groupsMax[part.GroupID])
                            {
                                return true;
                            }
                        }
                        else
                        {
                            // Useless part?
                        }
                    }
                }
            }

            return false;
        }
    }
}
