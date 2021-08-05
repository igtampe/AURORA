using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Igtampe.Aurora {
    public class OutageCollection: ICollection {

        //-[Variables]-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Maximum uptime in this outage collection</summary>
        public TimeSpan MaxUptime { get; private set; }

        /// <summary>Max Outage time in this outage collection</summary>
        public TimeSpan MaxOutage { get; private set; }

        /// <summary>Max age of this outage collection. Items older than this will not be added to this outage collection</summary>
        /// <value>Default: 30 days</value>
        public TimeSpan MaxAge { get; set; }

        /// <summary>Oldest outage in this collection</summary>
        public TimeSpan OldestOutageAgo  => DateTime.Now - InternalList.Values.First().Start;

        /// <summary>Count of outages in the last 24 hours</summary>
        public int Count24 { get; set; } = 0;

        /// <summary>Last outage in this collection</summary>
        public Outage LastOutage => InternalList.Values.LastOrDefault();

        public int Count => ((ICollection)InternalList).Count;

        public bool IsSynchronized => ((ICollection)InternalList).IsSynchronized;

        public object SyncRoot => ((ICollection)InternalList).SyncRoot;

        /// <summary>Finds the index of an outage</summary>
        /// <param name="O"></param>
        /// <returns></returns>
        public int IndexOf(Outage O) => InternalList.IndexOfValue(O);

        /// <summary>internal list to store all outages</summary>
        private readonly SortedList<DateTime, Outage> InternalList = new();

        //-[Methods]-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Gets the enumerator of the internal list</summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator() { return InternalList.Values.GetEnumerator(); }

        /// <summary>Adds an outage to this outage collection</summary>
        /// <param name="O"></param>
        public void AddOutage(Outage O) {

            //If this is too far back, don't add it.
            if (O.End < DateTime.Now.Subtract(MaxAge)) { return; }
            if (O.End > DateTime.Now.Subtract(new TimeSpan(24, 0, 0))) { Count24++; }

            //Add it
            InternalList.Add(O.Start, O); //add it

            //If this outage's duration is longer, than the previous maximum, mark it as the maximum
            if (O.Duration > MaxOutage) { MaxOutage = O.Duration; }

            int Index = InternalList.IndexOfValue(O);
            if (Index < 1) { return; } //we're done if this is the first one or if it wasn't found somehow I guess

            TimeSpan Uptime = O.UptimeBetween(InternalList.Values[Index - 1]);
            if (Uptime > MaxUptime) { MaxUptime = Uptime; } //This *WILL* break if an outage occurs before two other ones.
                                                            //However, this doesn't matter
        }

        public Outage GetOutageAt(int Index) { return InternalList.Values[Index]; }

        public void CopyTo(Array array, int index) { ((ICollection)InternalList).CopyTo(array, index); }

        //-[IO]-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Loads an outage collection with the default max age of 30 days</summary>
        /// <param name="Filename"></param>
        /// <returns></returns>
        public static OutageCollection LoadOutageCollection(string Filename) { return LoadOutageCollection(Filename, new TimeSpan(30, 0, 0, 0)); }

        /// <summary>Loads an outage collection with the specified max age</summary>
        /// <param name="Filename"></param>
        /// <param name="MaxAge"></param>
        /// <returns></returns>
        public static OutageCollection LoadOutageCollection(string Filename, TimeSpan MaxAge) {
            OutageCollection L = new() { MaxAge = MaxAge };
            string[] AllOutages = File.ReadAllLines(Filename);
            foreach (string outage in AllOutages) { L.AddOutage(Outage.StringToOutage(outage)); }
            return L;
        }

        /// <summary>Saves a given outage collection to given filename</summary>
        /// <param name="O"></param>
        /// <param name="Filename"></param>
        public static void SaveOutageCollection(OutageCollection O, string Filename) {
            StreamWriter Pen = new(Filename);
            foreach (Outage outage in O) { Pen.WriteLine(outage.ToString()); }
            Pen.Flush();
            Pen.Close();
            Pen.Dispose();
        }

    }
}
