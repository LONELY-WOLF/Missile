using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Missile
{
    class Computes
    {
        Aircraft aircraft;
        ComputeParams computeParams;

        public void Init(Aircraft aircraft, ComputeParams computeParams)
        {
            this.aircraft = aircraft;
            this.computeParams = computeParams;
        }

        public bool Iterate()
        {
            return false;
        }
    }
}
