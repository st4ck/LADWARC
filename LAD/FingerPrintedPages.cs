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

namespace LADWARC
{
    class FingerPrintedPage
    {
        public ulong[] mins;
        public long pageOffset;
        public long pageSize;
        public bool isInCluster;
        public Cluster itsCluster;
        public int lastPrecision = 0;

        public FingerPrintedPage(ulong[] mins, long p1, long p2, byte[] url)
        {
            this.mins = mins;
            this.pageOffset = p1;
            this.pageSize = p2;
            isInCluster = false;
            this.itsCluster = null;
        }

        public void assignCluster(Cluster cluster)
        {
            cluster.Add(this);
            itsCluster = cluster;
            isInCluster = true;
        }
        
    }
}
