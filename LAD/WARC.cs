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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LADWARC
{
    class WARC
    {
        FileStream warc;
        long seeking = 0;
        byte[] b;
        byte[] pattern = new byte[] { (byte)'\n', (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n', (byte)'w', (byte)'a', (byte)'r', (byte)'c', (byte)'/', (byte)'0', (byte)'.', (byte)'9' };
        int patternln;
        FileStream openWarc(String warcFile)
        {
            seeking = 0;
            return File.Open(warcFile, FileMode.Open);
        }

        public long getFileSeek() {
            return seeking;
        }

        public FileStream getFileStream()
        {
            return warc;
        }

        public List<byte[]> getNextToken()
        {
            int posstr = -1;
            int bread = -1;
            List<byte[]> tokens = new List<byte[]>();
            do {
                b = new byte[1024];
                bread = warc.Read(b, 0, b.Length);
                if (bread == 0) { // EOF
                    return null;
                }

                seeking += bread;
                tokens.Add(b);

                if (bread < 1024)
                { //LAST TOKEN
                    return tokens;
                }
            } while ((posstr = IndexOf(b,pattern)) == -1);
            int seekback = 0 - bread + posstr + 5;
            warc.Seek(seekback, SeekOrigin.Current); // I can seek back because this part of file it's already in memory (it's not a real disk seek)
            seeking += seekback;
            return tokens;
        }

        public FilePage getNextPage()
        {
            long from = getFileSeek();
            List<byte[]> actualToken = getNextToken();
            if (actualToken == null) return null;
            long to = getFileSeek();
            FilePage currentPage = new FilePage(actualToken, from, to);

            return currentPage;
        }

        public WARC(String warcFile)
        {
            patternln = pattern.Length;
            warc = openWarc(warcFile);
        }

        public static int IndexOf(byte[] arrayToSearchThrough, byte[] patternToFind, int startOffset = 0)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return -1;
            for (int i = startOffset; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

    }
}