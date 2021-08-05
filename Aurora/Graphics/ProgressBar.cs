using System;
using Igtampe.BasicRender;

namespace Igtampe.Aurora.Graphics {
    public class ProgressBar {

        private double percent;

        /// <summary>Percent of this progress bar.</summary>
        public double Percent {
            get { return percent; }
            //Make sure any attempt to make this below 0 or above 1 are caught
            set { percent = Math.Max(0, Math.Min(value, 1)); }
        }

        private readonly int LeftPos;
        private readonly int TopPos;

        /// <summary>Background of this progressbar</summary>
        protected ConsoleColor BG;

        /// <summary>Color of the bar that will be drawn on top of this progressbar to indicate progress.</summary>
        protected ConsoleColor BarColor;
        private readonly int Length;

        /// <summary>Creates a ProgressBar with the default colors (Black BG, Green Bar Color)</summary>
        /// <param name="Parent"></param>
        /// <param name="Length"></param>
        /// <param name="LeftPos"></param>
        /// <param name="TopPos"></param>
        public ProgressBar(int Length, int LeftPos, int TopPos) : this(Length, LeftPos, TopPos, ConsoleColor.Black, ConsoleColor.Green) { }

        /// <summary>Creates a ProgressBar</summary>
        /// <param name="Parent"></param>
        /// <param name="Length"></param>
        /// <param name="LeftPos"></param>
        /// <param name="TopPos"></param>
        /// <param name="BG"></param>
        /// <param name="BarColor"></param>
        public ProgressBar(int Length, int LeftPos, int TopPos, ConsoleColor BG, ConsoleColor BarColor) {
            this.Length = Length;
            this.LeftPos = LeftPos;
            this.TopPos = TopPos;
            this.BG = BG;
            this.BarColor = BarColor;
        }

        /// <summary>Draws this progress bar</summary>
        public void DrawBar() {
            Draw.Sprite(SpecialChars.VERTICAL+"", BG, BarColor, LeftPos, TopPos);
            Draw.Row(BarColor, Convert.ToInt32(Length * Percent), LeftPos+1, TopPos);
            if ((Length * Percent) % 1 > .5) {Draw.Sprite(SpecialChars.LEFT_HALF_BLOCK + "", BG, BarColor);}
            Draw.Sprite(SpecialChars.VERTICAL + "", BG, BarColor, LeftPos + Length + 1, TopPos);
        }


    }
}
