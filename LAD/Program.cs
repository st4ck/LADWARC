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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LADWARC
{
    partial class Program
    {

        public static int permNumber = 100;
        public static Stopwatch stopWatch;
        public static Random rnd;
        
        static void Main(string[] args)
        {
            Console.WriteLine("Number Of Cores: {0}", Environment.ProcessorCount);

            stopWatch = new Stopwatch();
            stopWatch.Start();

            // open WARC file
            String pathWarcFile = @"e:/LADWARC/_FinalOriginal_fixed_1gb_middle.warc";
            WARC warcFile = new WARC(pathWarcFile);
            long WARCSize = warcFile.getFileStream().Length;
            
            FileStream test = File.Open(@"z:/test.warc", FileMode.Create);

            rnd = new Random();
            RandomPermutations[] permutation = new RandomPermutations[permNumber];
            for (int i = 0; i < permNumber; i++) permutation[i] = new RandomPermutations(rnd);

            List<FingerPrintedPage> toCluster = new List<FingerPrintedPage>();
            int counter = 0;
            // while loop page-by-page

            Object lockWARC = new Object();

            Parallel.For(0, Environment.ProcessorCount, (cpu) =>
            {
                FilePage page;

                lock (lockWARC)
                {
                    page = warcFile.getNextPage();
                }

                while (page != null)
                {
                    // shingle each page
                    // body shingles " " separated
                    Shingle shingledpage = new Shingle(page);

                    // fingerprint each shingle
                    Fingerprint bodyFingers = null;

                    bodyFingers = new Fingerprint(shingledpage.getBodyShingles(), page);

                    ulong[] mins = new ulong[permNumber];

                    for (int i = 0; i < permNumber; i++)
                    {
                        ulong bodymin = 0;
                        bodymin = permutation[i].getMin(bodyFingers);
                        mins[i] = bodymin;
                    }


                    FingerPrintedPage toAddInCluster = new FingerPrintedPage(mins, page.getOffset(), page.getSize(), shingledpage.getUrl());
#if DEBUG
                    counter++;
                    if (counter % 30 == 0)
                    {
                        Console.Clear();
                        double duration = stopWatch.Elapsed.TotalSeconds;
                        Console.WriteLine("Number Of Cores: {0}", Environment.ProcessorCount);
                        Console.WriteLine("Page: " + counter + ", " + duration + " seconds");
                        Console.Write((double)page.getOffset() * 100 / (double)WARCSize);
                        Console.WriteLine("%");
                        Console.WriteLine("Estimated total for fingerprinting: " + (double)duration * 100 / ((double)page.getOffset() * 100 / (double)WARCSize));
                    }
#endif

                    lock (lockWARC)
                    {
                        toCluster.Add(toAddInCluster);
                        page = warcFile.getNextPage();
                    }

                    toAddInCluster = null;
                }
                
            });

            FileStream warc = warcFile.getFileStream();            

            List<Cluster> clusters = Cluster.ClusterPages_2(toCluster);

            long pi = 0;
            foreach (Cluster x in clusters)
            {
                foreach (FingerPrintedPage p in x.cluster)
                {
                    pi += p.pageSize;
                    byte[] buffer = new byte[p.pageSize];
                    warc.Seek((long)p.pageOffset, SeekOrigin.Begin);
                    int r = warc.Read(buffer, 0, (int)p.pageSize);
                    test.Write(buffer, 0, (int)p.pageSize);

#if DEBUG
                    Console.Clear();
                    Console.WriteLine("Number Of Cores: {0}", Environment.ProcessorCount);
                    Console.WriteLine("File writing...");
                    Console.Write((double)pi * 100 / (double)WARCSize);
                    Console.WriteLine("%");
                    Console.WriteLine("Elapsed total: " + Program.stopWatch.Elapsed.TotalSeconds + " seconds");
#endif
                }
                
            }
            
            test.Close();

            Console.WriteLine("Number Of Cores: {0}", Environment.ProcessorCount);
            Console.WriteLine("Total time: " + stopWatch.Elapsed.TotalSeconds + " seconds");
            Console.WriteLine("Completed.");
            Console.ReadLine();
            
            return;
        }

        private static void ClusterPages(List<FingerPrintedPage> toCluster)
        {
            throw new NotImplementedException();
        }
    }
}
