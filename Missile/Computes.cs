using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            System.Windows.Media.Media3D.Point3D vertex;
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
            ac.contour0 = new System.Windows.Media.Media3D.Point3D[ac.contour.Count()];
            for(int i =0 ;i<ac.contour.Count();i++)
            {
                vertex = ac.contour[i];
                ac.contour0[i].X = -(vertex.X * cosq) + (vertex.Y * sinalphaH) - (vertex.Z * sinq * cosalphaH);
                ac.contour0[i].Y = (vertex.Y * cosalphaH) + (vertex.Z * sinalphaH);
                ac.contour0[i].Z = (vertex.X * sinq) + (vertex.Y * cosq * sinalphaH) - (vertex.Z * cosq * cosalphaH);
            }
        }

        public bool Iterate()
        {
            return false;
        }
    }
}
