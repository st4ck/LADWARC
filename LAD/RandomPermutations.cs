/*
 * Copyright © 2015 Roberto Tacconelli <roberto.tacconelli@libero.it>
 * 
 * This file is part of LADWARC.
 * 
 * LADWARC is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * LADWARC is distributed in the hope that it will be useful,
 * 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with LADWARC.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LADWARC
{
    class RandomPermutations
    {
        ulong a = 0, b = 0, p = 30016801;
        Random rnd;
        Murmur3 rndM;

        public RandomPermutations(Random rnd)
        {
            this.rnd = rnd;
            rndM = new Murmur3((uint)rnd.Next(), (uint)rnd.Next());

            while (a < 10000000000) a = ((ulong)rnd.Next() << 32) + (ulong)rnd.Next();
            while (b < 10000000000) b = ((ulong)rnd.Next() << 32) + (ulong)rnd.Next();
            while (p < 10000000000) p = ((ulong)rnd.Next() << 32) + (ulong)rnd.Next();

#if DEBUG
            Console.Write(a);
            Console.Write(" ");
            Console.Write(b);
            Console.Write(" ");
            Console.Write(p);
            Console.WriteLine();
#endif
        }

        public ulong getMin(Fingerprint Fingerprints)
        {
            List<ulong> f = Fingerprints.getFingerprints();
            if ((f == null) || (f.Count == 0)) {
                return 0;
            }
            ulong min = (f[0] * a + b) % p;
            //ulong min = rndM.ComputeHash(f[0]);            

            foreach (ulong x in f)
            {
                ulong t = (x * a + b) % p;
                //ulong t = rndM.ComputeHash(x); ;
                if ((t != 0) && (t < min)) min = t;
            }

            return min;
        }



    }
}
