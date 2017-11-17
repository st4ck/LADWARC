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
    class FilePage : IDisposable
    {
        long offsetFromBeginning;
        long endFromBeginning;
        long length;
        List<byte[]> tokens;
        int tokensnumber;

        byte[] pageInBytes = null;

        public FilePage(List<byte[]> actualToken, long o, long l) {
            offsetFromBeginning = o;
            length = l-o;
            endFromBeginning = l;
            tokens = actualToken;
            tokensnumber = tokens.Count;
        }

        public byte[] getPageInBytes()
        {
            if (pageInBytes == null)
            {
                long bytesize = tokens.Sum(arr => arr.Length);
                if (bytesize > length) bytesize = length;
                var output = new byte[bytesize];
                int writeIdx = 0;
                for (int i = 0; i < tokens.Count - 1; i++)
                {
                    tokens.ElementAt(i).CopyTo(output, writeIdx);
                    writeIdx += tokens.ElementAt(i).Length;
                }

                Array.Copy(tokens.ElementAt(tokens.Count - 1), 0, output, writeIdx, length - writeIdx);
                pageInBytes = output;
            }

            return pageInBytes;
        }

        public long getOffset()
        {
            return offsetFromBeginning;
        }

        public long getSize()
        {
            return length;
        }

        public List<byte[]> getListOfTokens() {
            return tokens;
        }

        public void Dispose() {
            if (pageInBytes != null) pageInBytes = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }

    }
}
