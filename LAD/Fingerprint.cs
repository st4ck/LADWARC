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
    class Fingerprint : IDisposable
    {
        List<ulong> Fingerprints = null;

        public void Dispose()
        {
            if (Fingerprints != null) Fingerprints = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        public List<ulong> getFingerprints()
        {
            return Fingerprints;
        }

        public Fingerprint(List<int> shingles, FilePage page)
        {
            byte[] pageBytes = page.getPageInBytes();
            //if (shingles.Count < 2) return;
            if (shingles.Count < Program.permNumber) return;

            Fingerprints = new List<ulong>();

            Object lockMe = new Object();
            Parallel.For(0, shingles.Count - 1, (i) =>
            {
                ulong x = calculateFingerprint(pageBytes, shingles[i], shingles[i + 1]);
                if (x > 0) {
                    lock (lockMe)
                    {
                        Fingerprints.Add(x);
                    }
                }
            });

            if (Fingerprints.Count < Program.permNumber) Fingerprints.Clear();
        }

        public int getSize()
        {
            return Fingerprints.Count();
        }

        private ulong calculateFingerprint(byte[] pageBytes, int p1, int p2)
        {
            ulong siga = 0;
            ulong Q = 994534132561;
            //ulong D = 256;

            bool openedtag = false;
            byte lastbyte = 0;
            int sizefingered = 0;
            int wordsfingered = 0;
            for (int i = p1; i < p2; i++)
            {
                if (pageBytes[i] == (byte)'<') { openedtag = true; continue; }
                else if (pageBytes[i] == (byte)'>') { if (!openedtag) { siga = 0; } openedtag = false; continue; }
                else if ((pageBytes[i] == (byte)'\t') || (pageBytes[i] == (byte)'\r') || (pageBytes[i] == (byte)'\n')) {
                    continue;
                }
                else if ((pageBytes[i] == (byte)' ') && (lastbyte == (byte)' ')) { continue; }

                if (!openedtag) {
                    sizefingered++;
                    if (pageBytes[i] == ((byte)' ')) { wordsfingered++; }

                    //Karp-Rabin hashing
                    siga = ((siga << 8) + (ulong)pageBytes[i]);
                    if (siga > Q) { siga %= Q; }
                }

                lastbyte = pageBytes[i];
            }

            //if ((sizefingered < 75) || (wordsfingered < 14)) {
            if ((sizefingered < 40) || (wordsfingered < 7))
            {
                siga = 0;
            }
            
            return siga;
    
        }
    }
}
