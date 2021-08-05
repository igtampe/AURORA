using System;

namespace Igtampe.Aurora {

    /// <summary>Holds one Aura Outage</summary>
    public class Outage: IComparable {

        //-[Variables]-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Start time of this outage</summary>
        public DateTime Start { get; set; }

        /// <summary>End time of this outage</summary>
        public DateTime End { get; set; }

        /// <summary>Optional description of this outage</summary>
        public string Description { get; set; } = "";

        //-[Calculations]-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Calculated duration of this outage</summary>
        /// <value><see cref="End"/> - <see cref="Start"/></value>
        public TimeSpan Duration { get { return End - Start; } }

        /// <summary>Calculated uptime between this outage and what should be the last outage before this one</summary>
        /// <param name="O">Outage that occurred chronologically before this one</param>
        public TimeSpan UptimeBetween(Outage O) { return Start - O.End; }

        /// <summary>Compares this Outage to another object</summary>
        /// <param name="obj">Object to compare to (Should be another <see cref="Outage"/>!)</param>
        /// <returns>0 if the object is not an <see cref="Outage"/>, or this outage's <see cref="Duration"/> compared to the object's start if it is an <see cref="Outage"/> </returns>
        public int CompareTo(object obj) {
            if (obj is Outage O) { return Duration.CompareTo(O.Duration); }
            return 0;
        }

        /// <summary>Checks if this outage colides with another outage</summary>
        /// <param name="O">Another Outage</param>
        /// <returns>True if the start or end of the other outage is between this one's start and end</returns>
        public bool Colides(Outage O) { return (Start < O.Start && O.Start < End) || (Start < O.End && O.End < End); }

        //-[Overrides]-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Turns this Outage into a savable string</summary>
        /// <returns><see cref="Start"/>~<see cref="End"/>~<see cref="Description"/></returns>
        public override string ToString() { return string.Join('~', Start.ToString(), End.ToString(), Description); }

        /// <summary> Compares this outage to another object</summary>
        /// <param name="obj"></param>
        /// <returns>True if and only if the object is an <see cref="Outage"/> and <see cref="Start"/>, <see cref="End"/>, and <see cref="Description"/> match</returns>
        public override bool Equals(object obj) {
            if (obj is Outage O) { return O.Start.Equals(Start) && O.End.Equals(End) && O.Description.Equals(Description); } else return false;
        }

        /// <summary>Delegates hashcode to Start</summary>
        /// <returns><see cref="Start"/>'s hashcode (<see cref="DateTime.GetHashCode"/>)</returns>
        public override int GetHashCode() { return Start.GetHashCode(); }

        //-[Static Functions]-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Creates an outage from a string created by <see cref="ToString"/></summary>
        /// <param name="O"></param>
        /// <returns></returns>
        public static Outage StringToOutage(string O) {
            string[] OSplit = O.Split("~");
            return new Outage() {
                Start = DateTime.Parse(OSplit[0]),
                End = DateTime.Parse(OSplit[1]),
                Description = OSplit.Length > 2 ? OSplit[2] : ""
            };

        }

        //-[Operators]-------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool operator ==(Outage left, Outage right) {
            if (left is null) { return right is null; }
            return left.Equals(right);
        }

        public static bool operator !=(Outage left, Outage right) { return !(left == right); }

        public static bool operator <(Outage left, Outage right) { return left is null ? right is not null : left.CompareTo(right) < 0; }

        public static bool operator <=(Outage left, Outage right) {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(Outage left, Outage right) { return left is not null && left.CompareTo(right) > 0; }

        public static bool operator >=(Outage left, Outage right) { return left is null ? right is null : left.CompareTo(right) >= 0; }
    }
}
