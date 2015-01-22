using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Missile;
using System.ComponentModel;
using System.Windows;
using Microsoft.Win32;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ComputeParams computeParams = new ComputeParams();
            ulong Sum = 0, SumBlast = 0, SumRod = 0, SumFragments = 0, SumDirectHit = 0;
            int N;
            Aircraft aircraft;

            computeParams.ae = 0;
            computeParams.alphaH = 0;
            computeParams.d = 0.01;
            computeParams.D = 30;
            computeParams.gamma = 1;
            computeParams.H = 10000;
            computeParams.phi = 1;
            computeParams.q = 20;
            computeParams.Rmax = 10;
            computeParams.sigmaX = 5;
            computeParams.sigmaY = 10;
            computeParams.sigmaZ = 10;
            computeParams.tau = 0.04;
            computeParams.Vm = 900;
            computeParams.Vo = 1500;
            computeParams.Vt = 300;
            computeParams.y = 20;
            computeParams.z = 15;

            aircraft = new Aircraft(@"Data\Aircraft.xml");

            StreamWriter ofile = new StreamWriter("output.txt");

            for (int num = 0; num < 100; num++)
            {
                int cores = Environment.ProcessorCount;
                N = 10000;
                Sum = 0;
                SumBlast = 0;
                SumRod = 0;
                SumFragments = 0;
                SumDirectHit = 0;
                for (int p = 0; p < aircraft.parts.Count; p++)
                {
                    aircraft.parts[p].J = (4 * num);
                }
                Computes.Init(aircraft, computeParams);

                ulong[] DestroyedPartsCount = new ulong[aircraft.parts.Count];
                ulong[] ReasonsCount = new ulong[Enum.GetValues(typeof(DestroyType)).Length];
                ReasonsCount.Initialize();
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
                ThreadResult res1 = new ThreadResult(destroyed, DestroyedPartsCount, ReasonsCount);

                Sum += res1.Destroyed;
                SumBlast += res1.ReasonsCount[(int)DestroyType.Blast];
                SumRod += res1.ReasonsCount[(int)DestroyType.ContinuousRod];
                SumDirectHit += res1.ReasonsCount[(int)DestroyType.DirectHit];
                DestroyedPartsCount = DestroyedPartsCount.Zip(res1.DestroyedPartsCount, (outp, newp) => outp += newp).ToArray();

                ofile.WriteLine("{0}\t{1}\t{2}", num, aircraft.parts[0].J, Sum);
                Console.Write("\r{0,3} of 100", num + 1);
                //Console.WriteLine("Сумма {0}\nФугасом {1}\nСтержнями {2}\nПрямым {3}", Sum, SumBlast, SumRod, SumDirectHit);
            }
            ofile.Close();
            Console.Write("Press ENTER");
            Console.Read();
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
}
