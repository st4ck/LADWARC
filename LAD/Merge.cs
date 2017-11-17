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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LADWARC
{

    class Merge
    {
        public static void sort(KeyValuePair<FingerPrintedPage, ulong>[] a)
        {
            sort(a, 0, a.Length);
        }

        public static void sort(List<FingerPrintedPage> a)
        {
            sort(a, 0, a.Count);
        }

        public static void sort(KeyValuePair<FingerPrintedPage, ulong>[] a, int low, int high)
        {
            int N = high - low;
            if (N <= 1)
                return;

            int mid = low + N / 2;

            Parallel.Invoke(() =>
            {
                sort(a, low, mid);
            }, () =>
            {
                sort(a, mid, high);
            });


            KeyValuePair<FingerPrintedPage, ulong>[] aux = new KeyValuePair<FingerPrintedPage, ulong>[N];
            int i = low, j = mid;
            for (int k = 0; k < N; k++)
            {
                if (i == mid) aux[k] = a[j++];
                else if (j == high) aux[k] = a[i++];
                else if (a[j].Value < a[i].Value) aux[k] = a[j++];
                else aux[k] = a[i++];
            }

            lock (lockA)
            {
                for (int k = 0; k < N; k++)
                {
                    a[low + k] = aux[k];
                }
            }
        }

        private static Boolean isSorted(IComparable[] a)
        {
            for (int i = 1; i < a.Length; i++)
                if (a[i].CompareTo(a[i - 1]) < 0) return false;
            return true;
        }

        static Object lockA = new Object();

        internal static void sort(List<FingerPrintedPage> a, int low, int high)
        {
            int N = high - low;
            if (N <= 1)
                return;

            int mid = low + N / 2;

            Parallel.Invoke(() =>
            {
                sort(a, low, mid);
            }, () =>
            {
                sort(a, mid, high);
            });

            FingerPrintedPage[] aux = new FingerPrintedPage[N];
            int i = low, j = mid;
            for (int k = 0; k < N; k++)
            {
                if (i == mid) aux[k] = a[j++];
                else if (j == high) aux[k] = a[i++];
                else if (a[j].mins[0] < a[i].mins[0]) aux[k] = a[j++];
                else aux[k] = a[i++];
            }

            lock (lockA)
            {
                for (int k = 0; k < N; k++)
                {
                    a[low + k] = aux[k];
                }
            }
        }

    }
}
