using System;
using System.Globalization;
using System.IO;
using Igtampe.BasicRender;
using Igtampe.BasicGraphics;
using Igtampe.Aurora.Graphics;


namespace Igtampe.Aurora {
    public static class Program {

        //-[Constant]-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Date Format of all dates</summary>
        public const string DATE_FORMAT = "MM-dd-yyyy HH:mm:ss";

        /// <summary>Default pulse file</summary>
        private const string DEFAULT_PULSE = "Pulse.aurlog";

        /// <summary>Default log location</summary>
        private const string DEFAULT_LOG = "Default.aurlog";

        /// <summary>Start</summary>
        /// <param name="args"></param>
        public static void Main(string[] args) {
            string Filename = DEFAULT_LOG;
            if (args.Length > 0) {

                switch (args[0].ToLower()) {
                    case "/logoutage":
                        LogOutage(args.Length > 1 ? args[1] : DEFAULT_PULSE, args.Length > 2 ? args[2] : DEFAULT_LOG);
                        return;
                    case "/pulse":
                        Pulse(args.Length > 1 ? args[1] : DEFAULT_PULSE);
                        return;
                    case "/generatedata":
                        Generate(args.Length > 1 ? args[1] : DEFAULT_LOG);
                        return;
                    case "/?":
                    case "-?":
                    case "--?":
                    case "-help":
                    case "--help":
                        ShowHelp();
                        return;
                    default:
                        if (File.Exists(args[0])) { Filename = args[0]; }
                        break;
                }
            }

            TitleScreen(Filename);
        }

        //-[Pages]-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Setup for the display</summary>
        private static void DisplaySetup() {
            //Window Size
            RenderUtils.SetSize(80, 24);

            //Window Clear
            RenderUtils.Color(ConsoleColor.Black, ConsoleColor.White);
            Console.Clear();
        }

        /// <summary>Draws a horizontal line from left to right at given toppos and with given color FG.</summary>
        private static void DrawHorizontalLine(ConsoleColor FG, int TopPos) { Draw.Sprite(RenderUtils.Repeater(SpecialChars.DOUBLE_HORIZONTAL, Console.WindowWidth), ConsoleColor.Black, FG, 0, TopPos); }

        /// <summary>Draw the IGTNET Header</summary>
        private static void DrawIGTNETHeader() {
            Draw.Box(ConsoleColor.DarkMagenta, 3, 1, 0, 0);
            Draw.Box(ConsoleColor.DarkMagenta, 3, 1, 79 - 3, 0);

            Draw.Sprite("[                              I G T N E T                              ]", ConsoleColor.Black, ConsoleColor.White, 3, 0);
            DrawHorizontalLine(ConsoleColor.Cyan, 1);
        }

        /// <summary>Start the app's title screen and loading the whole thing</summary>
        /// <param name="Filename"></param>
        private static void TitleScreen(string Filename) {
            DisplaySetup();
            DrawIGTNETHeader();

            //Get the Artemis Logo and draw it
            BasicGraphic ArtemisLogo = BasicGraphic.LoadFromResource(Properties.Resources.ArtemisLogo);

            //Get Leftpos
            int LeftPos = (Console.WindowWidth - ArtemisLogo.GetWidth() - 28) / 2;

            ArtemisLogo.Draw(LeftPos, 3);

            Draw.Sprite("AURORA: Artemis Relaunch Monitor", ConsoleColor.Black, ConsoleColor.White, LeftPos + ArtemisLogo.GetWidth() + 2, 3 + (ArtemisLogo.GetHeight() / 2) - 1);
            Draw.Sprite("Version 1.0 | (C)2021 Igtampe", ConsoleColor.Black, ConsoleColor.White, LeftPos + ArtemisLogo.GetWidth() + 2, 3 + (ArtemisLogo.GetHeight() / 2));

            Draw.CenterText("Loading " + Filename, 3 + ArtemisLogo.GetHeight() + 1, ConsoleColor.Black, ConsoleColor.White);
            ProgressBar P = new(38, 20, 3 + ArtemisLogo.GetHeight() + 3);

            P.DrawBar();

            if (!File.Exists(Filename)) {
                Draw.CenterText($"File {Filename} does not exist!", 3 + ArtemisLogo.GetHeight() + 4, ConsoleColor.Black, ConsoleColor.Red);
                Draw.CenterText($"No outages available to display", 3 + ArtemisLogo.GetHeight() + 5, ConsoleColor.Black, ConsoleColor.Red);
                RenderUtils.Pause();
                return;
            }

            OutageCollection L = new() { MaxAge = new TimeSpan(365, 0, 0, 0) };
            string[] AllOutages = File.ReadAllLines(Filename);
            double PerOutageIncrement = 1.0 / AllOutages.Length;
            foreach (string outage in AllOutages) {
                Outage O = Outage.StringToOutage(outage);
                L.AddOutage(O);
                Draw.CenterText($"Loaded outage from {O.Start.ToString(DATE_FORMAT)} to {O.End.ToString(DATE_FORMAT)}", 4 + ArtemisLogo.GetHeight() + 4, ConsoleColor.Black, ConsoleColor.White);
                Draw.CenterText($"Loaded {L.Count} outages in the past {L.MaxAge.TotalDays} days", 4 + ArtemisLogo.GetHeight() + 5, ConsoleColor.Black, ConsoleColor.White);
                P.Percent += PerOutageIncrement;
                P.DrawBar();
            }

            Draw.ClearLine(4 + ArtemisLogo.GetHeight() + 4);

            Draw.CenterText("Press a key to continue", 4 + ArtemisLogo.GetHeight() + 4, ConsoleColor.Black, ConsoleColor.White);
            RenderUtils.Pause();

            Home(L);
        }


        /// <summary>Home screen of this cosa which shows todo (everything)</summary>
        /// <param name="O"></param>
        private static void Home(OutageCollection O, bool Redraw = false) {

            Console.Clear();
            DrawIGTNETHeader();
            BasicGraphic SmallArtemis = BasicGraphic.LoadFromResource(Properties.Resources.ArtemisLogoSmall);
            SmallArtemis.Draw(3, 2);
            Draw.Sprite($"Artemis Uptime Relaunch and Outage Registry Administrator", ConsoleColor.Black, ConsoleColor.White, 16, 3);
            Draw.Sprite($"{O.Count} outage(s) in the last {Convert.ToInt32(O.OldestOutageAgo.TotalDays)} day(s)", ConsoleColor.Black, ConsoleColor.White, 16, 4);
            Draw.Sprite($"{O.Count24} outage(s) in the last 24 hours", ConsoleColor.Black, ConsoleColor.White, 16, 5);

            DrawHorizontalLine(ConsoleColor.White, 7);

            TimeSpan Uptime = DateTime.Now - O.LastOutage.End;

            //Get the uptime downtime strings:
            string CurrUptime = $"Current Uptime: {Uptime.Days} Day(s), {Uptime.Hours} Hour(s), {Uptime.Minutes} Minute(s), {Uptime.Seconds} Second(s)";
            string LastDowntime = $"Last Downtime : {O.LastOutage.Duration.Days} Day(s), {O.LastOutage.Duration.Hours} Hour(s), {O.LastOutage.Duration.Minutes} Minute(s), {O.LastOutage.Duration.Seconds} Second(s)";
            string MaxUptime = $"Max Uptime    : {O.MaxUptime.Days} Day(s), {O.MaxUptime.Hours} Hour(s), {O.MaxUptime.Minutes} Minute(s), {O.MaxUptime.Seconds} Second(s)";
            string MaxDowntime = $"Max Downtime  : {O.MaxOutage.Days} Day(s), {O.MaxOutage.Hours} Hour(s), {O.MaxOutage.Minutes} Minute(s), {O.MaxOutage.Seconds} Second(s)";

            //Find Maxlength
            int MaxLength = 0;
            MaxLength = Math.Max(CurrUptime.Length, MaxLength);
            MaxLength = Math.Max(LastDowntime.Length, MaxLength);
            MaxLength = Math.Max(MaxUptime.Length, MaxLength);
            MaxLength = Math.Max(MaxDowntime.Length, MaxLength);

            //Find where to place the thing:
            int leftpos = (Console.WindowWidth - MaxLength) / 2;

            //Draw all four:
            Draw.Sprite(CurrUptime, ConsoleColor.Black, ConsoleColor.White, leftpos, 9);
            Draw.Sprite(LastDowntime, ConsoleColor.Black, ConsoleColor.White, leftpos, 10);
            Draw.Sprite(MaxUptime, ConsoleColor.Black, ConsoleColor.White, leftpos, 11);
            Draw.Sprite(MaxDowntime, ConsoleColor.Black, ConsoleColor.White, leftpos, 12);

            DrawHorizontalLine(ConsoleColor.White, 14);

            Draw.CenterText("The last outage was from", 16, ConsoleColor.Black, ConsoleColor.White);
            Draw.CenterText(O.LastOutage.Start.ToString(), 18,ConsoleColor.Black, ConsoleColor.Red);
            Draw.CenterText("to", 19, ConsoleColor.Black, ConsoleColor.White);
            Draw.CenterText(O.LastOutage.End.ToString(), 20, ConsoleColor.Black, ConsoleColor.Green);

            /**00000000001111111111222222222233333333334444444444555555555566666666667777777777
             * 01234567890123456789012345678901234567890123456789012345678901234567890123456789
             0*:::                              [Igtnet]                                   :::
             1*═══════════════════════════════════════════════════════════════════════════════
             2*        FF
             3*       FF F       Artemis Uptime Relaunch and Outage Registry Administrator
             4*      F  F F      30 Outage(s) in the last 365 Days
             5*     F    F F     1 Outage(s) in the last 24 hours
             6*    F      F F
             7*═══════════════════════════════════════════════════════════════════════════════
             8*
             9*    Current Uptime: {X} Day(s), {X} Hour(s), {X} Minute(s), {X} Second(s)
            10*    Last Downtime : {X} Day(s), {X} Hour(s), {X} Minute(s), {X} Second(s)
            11*    Max Uptime    : {X} Day(s), {X} Hour(s), {X} Minute(s), {X} Second(s)
            12*    Max Downtime  : {X} Day(s), {X} Hour(s), {X} Minute(s), {X} Second(s)
            13*
            14*═══════════════════════════════════════════════════════════════════════════════
            15*
            16*                         The last outage was from
            17*
            18*                            MM-dd-yyyy HH:mm:ss
            19*                                    to
            20*                            MM-dd-yyyy HH:mm:ss
            21*
            22*═══════════════════════════════════════════════════════════════════════════════
            23*[S] See All Data                                                 [D] Disconnect
             */

            DrawHorizontalLine(ConsoleColor.White, 22);
            Draw.Sprite("[S] See All Data                                                 [D] Disconnect", ConsoleColor.Black, ConsoleColor.White, 0, 23);

            if (Redraw) { return; }

            while (true) {
                switch (Console.ReadKey().Key) {
                    case ConsoleKey.S:
                        StatisticsPage(O);
                        Console.Clear();
                        Home(O, true);
                        break;
                    case ConsoleKey.Escape:
                    case ConsoleKey.D:
                        return;
                    default:
                        break;
                }
            }
        }

        private static void StatisticsPage(OutageCollection O) {

            Console.Clear();
            DrawIGTNETHeader();

            //Draw the headers
            Draw.Sprite($"{SpecialChars.VERTICAL}####{SpecialChars.VERTICAL}        START        {SpecialChars.VERTICAL}          END        {SpecialChars.VERTICAL}  DURATION   {SpecialChars.VERTICAL}", ConsoleColor.White, ConsoleColor.Black, 0, 2);
            BasicGraphic SmallArtemis = BasicGraphic.LoadFromResource(Properties.Resources.ArtemisLogoSmall);
            SmallArtemis.Draw(66, 9);

            Draw.Sprite("AURORA", ConsoleColor.Black, ConsoleColor.White, 68, 15);

            int TopIndex = Math.Max(0, O.Count - 19);

            DrawRows(O, TopIndex);

            DrawHorizontalLine(ConsoleColor.White, 22);
            Draw.Sprite($"[{SpecialChars.ARROW_UP}] Scroll up  [{SpecialChars.ARROW_DOWN}] Scroll down  [PGUP] Page up  [PGDOWN] Page Down  [ESC] Back", ConsoleColor.Black, ConsoleColor.White, 0, 23);


            while (true) {
                switch (Console.ReadKey(true).Key) {
                    case ConsoleKey.UpArrow:
                        TopIndex = Math.Max(TopIndex - 1, 0);
                        break;
                    case ConsoleKey.DownArrow:
                        TopIndex = Math.Max(Math.Min(TopIndex + 1, O.Count - 19),0);
                        break;
                    case ConsoleKey.PageUp:
                        TopIndex = Math.Max(TopIndex - 19, 0);
                        break;
                    case ConsoleKey.PageDown:
                        TopIndex = Math.Max(Math.Min(TopIndex + 19, O.Count - 19),0);
                        break;
                    case ConsoleKey.Escape:
                    case ConsoleKey.D:
                        return;
                    default:
                        break;
                }
                DrawRows(O, TopIndex);
            }


            /**00000000001111111111222222222233333333334444444444555555555566666666667777777777
             * 01234567890123456789012345678901234567890123456789012345678901234567890123456789
             0*:::                              [Igtnet]                                   :::
             1*═══════════════════════════════════════════════════════════════════════════════
             2*|####|        START        |          END        |  DURATION   |          
             3*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
             4*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
             5*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
             6*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
             7*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
             8*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |       FF
             9*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |      FF F   
            10*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |     F  F F  
            11*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |    F    F F 
            12*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |   F      F F
            13*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |  F        F F
            14*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
            15*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |    AURORA
            16*|   1| MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
            17*|    | MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
            18*|    | MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
            19*|    | MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
            20*|    | MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
            21*|    | MM-dd-yyyy HH:mm:ss | MM-dd-yyyy HH:mm:ss | DD:HH:MM:SS |
            22*═══════════════════════════════════════════════════════════════════════════════
            23*[^] Scroll up  [v] Scroll down  [PGUP] Page up  [PGDOWN] Page Down  [ESC] Back
             */

        }

        /// <summary>Draw rows of the outage collection</summary>
        /// <param name="O"></param>
        /// <param name="StartIndex"></param>
        private static void DrawRows(OutageCollection O, int StartIndex) {
            int Offset = 0;

            for (int i = StartIndex; i < StartIndex + 19; i++) {
                if (i == O.Count) { return; }
                Outage o = O.GetOutageAt(i);
                bool AlternateColor = i % 2 > 0;
                Draw.Sprite($"{SpecialChars.VERTICAL}{SpaceInt(i)}{SpecialChars.VERTICAL} {o.Start.ToString(DATE_FORMAT)} {SpecialChars.VERTICAL} {o.End.ToString(DATE_FORMAT)} {SpecialChars.VERTICAL} {FormatTimeSpan(o.Duration)} {SpecialChars.VERTICAL}", ConsoleColor.Black, AlternateColor ? ConsoleColor.White : ConsoleColor.Gray, 0, 3 + Offset);
                Offset++;
            }
        }

        private static string SpaceInt(int Value) {
            if (Value > 9999) { return "9999"; }
            if (Value >= 1000) { return "" + Value; }
            if (Value >= 100) { return " " + Value; }
            if (Value >= 10) { return "  " + Value; }
            return "   " + Value;
        }

        /// <summary>Formats a timespan</summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static string FormatTimeSpan(TimeSpan T) {
            string Days = T.Days < 10 ? "0" + T.Days : T.Days + "";
            string Hours = T.Hours < 10 ? "0" + T.Hours : T.Hours + "";
            string Mins = T.Minutes < 10 ? "0" + T.Minutes : T.Minutes + "";
            string Secs = T.Seconds < 10 ? "0" + T.Seconds : T.Seconds + "";
            return string.Join(":", Days, Hours, Mins, Secs);
        }

        /// <summary>Shows the help text</summary>
        private static void ShowHelp() {
            Console.WriteLine("Artemis Uptime Relaunch & Outage Registry Administrator (Aurora)");
            Console.WriteLine("Version 1.0 | (C)2021 Igtampe, No Rights Reserved");
            Console.WriteLine();
            Console.WriteLine("Usage : /LogOutage [PulseFile] [LogFile]");
            Console.WriteLine();
            Console.WriteLine("LogOutage : Option to log an outage");
            Console.WriteLine("PulseFile : File that has a pulse (last known uptime date) in the");
            Console.WriteLine("            First 19 characters at " + DATE_FORMAT);
            Console.WriteLine("LogFile   : Aurlog file that contains an OutageCollection");
        }


        //-[Methods]-------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>Pulses to a file to be used later to detect the last downtime</summary>
        /// <param name="PulseFile"></param>
        private static void Pulse(string PulseFile) { File.WriteAllText(PulseFile, DateTime.Now.ToString(DATE_FORMAT)); }

        /// <summary>Logs an outage that started at the last pulse to now</summary>
        /// <param name="PulseFile"></param>
        /// <param name="Filename"></param>
        private static void LogOutage(string PulseFile, string Filename) {
            //Get the end time now as soon as possible
            DateTime End = DateTime.Now;

            //Find the pulse file:
            string[] PFile = File.ReadAllLines(PulseFile);

            //Get the first 19 characters of the line
            string StartDateAsString = PFile[^1].Substring(0, 19);

            //Parse the start   
            DateTime Start = DateTime.ParseExact(StartDateAsString, DATE_FORMAT, CultureInfo.CurrentCulture);

            //Add new outage to the outage log
            OutageCollection O = File.Exists(Filename) ? OutageCollection.LoadOutageCollection(Filename, new TimeSpan(365, 0, 0, 0)) : new OutageCollection();
            Outage Out = new() { Start = Start, End = End };
            O.AddOutage(Out);
            OutageCollection.SaveOutageCollection(O, Filename);

            //And that's it
            Console.WriteLine($"Succesfully logged outage from {Start.ToString(DATE_FORMAT)} to {End.ToString(DATE_FORMAT)}, lasted {Out.Duration:g} ");
        }

        /// <summary></summary>
        /// <param name="Filename"></param>
        /// <param name="Amount"></param>
        private static void Generate(string Filename, int Amount = 500) {
            OutageCollection O = new() { MaxAge = new TimeSpan(365, 0, 0, 0, 0) };
            DateTime BaseDate = DateTime.Now.Subtract(O.MaxAge).AddHours(5);
            Random R = new();

            for (int i = 0; i < Amount; i++) {
                DateTime End = BaseDate.AddHours(R.Next(11) + 1);
                O.AddOutage(new Outage() { Start = BaseDate, End = End });
                Console.WriteLine($"ENTRY {O.Count} = {O.LastOutage.Start.ToString(DATE_FORMAT)} - {O.LastOutage.End.ToString(DATE_FORMAT)} ({O.LastOutage.Duration.TotalHours})");
                BaseDate = End.AddHours(R.Next(11) + 1);
            }

            OutageCollection.SaveOutageCollection(O, Filename);
            Console.WriteLine("Done");
        }

    }
}
