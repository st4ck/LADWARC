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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LADWARC
{
    class Shingle : IDisposable
    {
        FilePage page;
        byte[] sections;
        long httpRequestStart;
        long headerStart;
        long bodyStart;
        byte[] url;
        List<int> bodyShingles = null;

        public void Dispose()
        {
            if (sections != null) sections = null;
            if (bodyShingles != null) bodyShingles = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        public Shingle(FilePage page)
        {
            this.page = page;
            sections = page.getPageInBytes();
            long urlStart = WARC.IndexOf(sections, GetBytes("response ")) + 9;
            long urlEnd = WARC.IndexOf(sections, GetBytes(" "), (int)urlStart + 1);
            url = new byte[urlEnd - urlStart];
            Array.Copy(sections, urlStart, url, 0, urlEnd - urlStart);
            httpRequestStart = WARC.IndexOf(sections, GetBytes("\r\n\r\n"));
            headerStart = WARC.IndexOf(sections, GetBytes("\r\n\r\n"), (int)httpRequestStart + 4);
            bodyStart = WARC.IndexOf(sections, GetBytes("<body"));
            if (bodyStart == -1) bodyStart = WARC.IndexOf(sections, GetBytes("<BODY"));

            if (httpRequestStart == -1) httpRequestStart = 0;
            if (headerStart == -1) headerStart = 0;
            if (bodyStart == -1) bodyStart = headerStart;

            bodyShingles = shingleBySeparator((int)bodyStart, (int)(sections.Length - bodyStart),".");
            bodyShingles.Add((int)(sections.Length));
        }

        public List<int> getBodyShingles()
        {
            return bodyShingles;
        }

        public byte[] getUrl()
        {
            return url;
        }


        public List<int> shingleBySeparator(int offset, int size, string separator)
        {
            int pos = offset;
            List<int> shingles = new List<int>();
            byte[] openTag = GetBytes(separator);
            int lastpos = offset;
            while (((pos = WARC.IndexOf(sections, openTag, pos + 1)) != -1) && pos < (offset + size))
            {
                if (pos != (lastpos + 1)) shingles.Add(pos);
                lastpos = pos;
            }

            return shingles;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }
            return bytes;
        }
    }
}
