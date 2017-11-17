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
    class Cluster
    {
        static int requestedPrecision = 1; // requested equal fingerprints (10 / 50 - 20%)

        public List<FingerPrintedPage> cluster;
        public Cluster()
        {
            cluster = new List<FingerPrintedPage>();
        }

        public void Add(FingerPrintedPage page) {
            cluster.Add(page);
        }

        public static List<Cluster> allClusters;

        static int clusterMax = 25000;
        static Object lockClusters = new Object();
        static Object lockFor = new Object();

        public static List<Cluster> ClusterPages_2(List<FingerPrintedPage> toCluster)
        {
            allClusters = new List<Cluster>();

#if DEBUG
            Console.Clear();
            Console.WriteLine("Number Of Cores: {0}", Environment.ProcessorCount);
            Console.WriteLine("Clustering in " + (Program.permNumber) + " steps, " + Program.stopWatch.Elapsed.TotalSeconds + " seconds");
#endif

            for (int i = 0; i < toCluster.Count; i++)
            {
                    Cluster t = new Cluster();
                    toCluster[i].assignCluster(t);
                    allClusters.Add(t);
            }

            Parallel.For(0, Program.permNumber, (k) =>
            {
                KeyValuePair<FingerPrintedPage, ulong>[] klist = new KeyValuePair<FingerPrintedPage, ulong>[toCluster.Count];

#if DEBUG
                if (k == 1)
                {
                    Console.Clear();
                    Console.WriteLine("Number Of Cores: {0}", Environment.ProcessorCount);
                    Console.WriteLine("Clustering in " + (Program.permNumber) + " steps, " + Program.stopWatch.Elapsed.TotalSeconds + " seconds");
                }
#endif

                for (int i = 0; i < toCluster.Count; i++)
                {
                    klist[i] = new KeyValuePair<FingerPrintedPage, ulong>(toCluster[i], toCluster[i].mins[k]);
                }

                Merge.sort(klist);

                ulong lastmin = klist[0].Value;
                FingerPrintedPage lastFingered = klist[0].Key;

                for (int i = 1; i < klist.Length; i++)
                {

#if DEBUG
                    if ((k == 1) && (i % 100 == 0))
                    {
                        Console.Clear();
                        Console.WriteLine("Number Of Cores: {0}", Environment.ProcessorCount);
                        Console.WriteLine("Clustering in " + (Program.permNumber) + " steps, " + Program.stopWatch.Elapsed.TotalSeconds + " seconds");
                    }
#endif

                    if ((klist[i].Value == lastmin) && (klist[i].Value != 0))
                    {
                        if (lastFingered.itsCluster != klist[i].Key.itsCluster)
                        {
                            int precision = 0;

                            for (int m = 0; m < Program.permNumber; m++)
                            {
                                if (lastFingered.mins[m] == klist[i].Key.mins[m])
                                {
                                    precision++;
                                }

                            }

                            if ((precision > klist[i].Key.lastPrecision) && (precision > requestedPrecision))
                            {
                                lock (lockClusters)
                                {
                                    if (lastFingered.itsCluster != klist[i].Key.itsCluster)
                                    {
                                        if (lastFingered.itsCluster.cluster.Count < clusterMax)
                                        {
                                            klist[i].Key.lastPrecision = precision;

                                            klist[i].Key.itsCluster.cluster.Remove(klist[i].Key);
                                            if (klist[i].Key.itsCluster.cluster.Count == 0) allClusters.Remove(klist[i].Key.itsCluster);
                                            lastFingered.itsCluster.cluster.Insert(lastFingered.itsCluster.cluster.IndexOf(lastFingered) + 1, klist[i].Key);
                                            //lastFingered.itsCluster.Add(klist[i].Key);
                                            klist[i].Key.itsCluster = lastFingered.itsCluster;

                                        }
                                    }
                                    else
                                    {
                                        klist[i].Key.lastPrecision = precision;
                                    }
                                }
                            }
                        }

                        lastFingered = klist[i].Key;
                    }
                    else
                    {
                        lastmin = klist[i].Value;
                        lastFingered = klist[i].Key;
                    }
                }
            });

            

#if DEBUG
            StreamWriter outfile = new StreamWriter(@"z:/clusters.txt");
#endif

            for (int i = 0; i < allClusters.Count; i++ )
                //foreach (Cluster x in allClusters)
                {
#if DEBUG
                    outfile.WriteLine(allClusters[i].cluster.Count + " ");
#endif
                }


#if DEBUG
            outfile.Close();
#endif

            return allClusters;
        }
    }

}
