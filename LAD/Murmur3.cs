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
using System.Text;
using System.Threading.Tasks;

namespace LADWARC
{
    class Murmur3
    {
        public static ulong READ_SIZE = 8;
        private static ulong C1 = 0x87c37b91114253d5L;
        private static ulong C2 = 0x4cf5ad432745937fL;

        private ulong length;
        private uint seed;
        private uint seed2;
        ulong h1;
        ulong h2;

        public Murmur3(uint seed, uint seed2)
        {
            this.seed = seed;
            this.seed2 = seed2;
        }

        private void MixBody(ulong k1, ulong k2)
        {
            h1 ^= MixKey1(k1);

            h1 = h1.RotateLeft(27);
            h1 += h2;
            h1 = h1 * 5 + 0x52dce729;

            h2 ^= MixKey2(k2);

            h2 = h2.RotateLeft(31);
            h2 += h1;
            h2 = h2 * 5 + 0x38495ab5;
        }

        private static ulong MixKey1(ulong k1)
        {
            k1 *= C1;
            k1 = k1.RotateLeft(31);
            k1 *= C2;
            return k1;
        }

        private static ulong MixKey2(ulong k2)
        {
            k2 *= C2;
            k2 = k2.RotateLeft(33);
            k2 *= C1;
            return k2;
        }

        private static ulong MixFinal(ulong k)
        {
            // avalanche bits

            k ^= k >> 33;
            k *= 0xff51afd7ed558ccdL;
            k ^= k >> 33;
            k *= 0xc4ceb9fe1a85ec53L;
            k ^= k >> 33;
            return k;
        }

        public ulong ComputeHash(ulong bb)
        {
            ProcessUlong(bb);
            return UHash;
        }

        private void ProcessUlong(ulong bb)
        {
            h1 = seed;
            h2 = seed2;
            this.length = 0L;

            ulong k1 = (bb >> 32);
            ulong k2 = ((bb << 32) >> 32);
            length += READ_SIZE;
            MixBody(k1, k2);
        }

        public ulong UHash
        {
            get
            {
                h1 ^= length;
                h2 ^= length;

                h1 += h2;
                h2 += h1;

                h1 = Murmur3.MixFinal(h1);
                h2 = Murmur3.MixFinal(h2);

                h1 += h2;
                h2 += h1;

                ulong x = 0;
                x += h1;
                //x <<= 32;
                x += h2;

                return x;
            }
        }
    }

    public static class IntHelpers
    {
        public static ulong RotateLeft(this ulong original, int bits)
        {
            return (original << bits) | (original >> (64 - bits));
        }

        public static ulong RotateRight(this ulong original, int bits)
        {
            return (original >> bits) | (original << (64 - bits));
        }
    }
}
