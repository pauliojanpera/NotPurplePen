using System;
using System.Collections.ObjectModel;

namespace PurplePen.MapModel
{
    using PurplePen.Graphics2D;

    // The halftone screen of an ink: its frequency in lines per inch, and its angle in degrees.
    public struct InkScreen : IEquatable<InkScreen>
    {
        public readonly float Frequency;
        public readonly float Angle;

        public InkScreen(float frequency, float angle)
        {
            Frequency = frequency;
            Angle = angle;
        }

        public bool Equals(InkScreen other)
        {
            return Frequency == other.Frequency && Angle == other.Angle;
        }

        public override bool Equals(object obj)
        {
            return obj is InkScreen && Equals((InkScreen) obj);
        }

        public override int GetHashCode()
        {
            return Frequency.GetHashCode() ^ (Angle.GetHashCode() * 31);
        }

        public override string ToString()
        {
            return string.Format("{0} lpi at {1} degrees", Frequency, Angle);
        }
    }

    // A printing ink: the colorant of one ink separation of a print. A palette color (SymColor) is made of inks.
    // The four process inks always exist, and a map adds its spot inks to them. An ink does not change after it
    // is made, so that maps can share it. Its identity is the object: two inks with the same name are two inks.
    public sealed class Ink
    {
        public static readonly Ink Cyan = new Ink("Cyan", CmykColor.FromCmyk(1, 0, 0, 0), null, true);
        public static readonly Ink Magenta = new Ink("Magenta", CmykColor.FromCmyk(0, 1, 0, 0), null, true);
        public static readonly Ink Yellow = new Ink("Yellow", CmykColor.FromCmyk(0, 0, 1, 0), null, true);
        public static readonly Ink Black = new Ink("Black", CmykColor.FromCmyk(0, 0, 0, 1), null, true);

        // The process inks, in the order cyan, magenta, yellow, black.
        public static readonly ReadOnlyCollection<Ink> ProcessInks = Array.AsReadOnly(new Ink[] { Cyan, Magenta, Yellow, Black });

        private readonly string name;
        private readonly CmykColor appearance;
        private readonly InkScreen? screen;
        private readonly bool isProcessInk;

        // Make a spot ink.
        // name: the name of the ink, which a format can use to refer to it.
        // appearance: the CMYK value that shows the ink at full tone.
        // screen: the halftone screen of the ink, or null where none is defined.
        public Ink(string name, CmykColor appearance, InkScreen? screen)
            : this(name, appearance, screen, false)
        {
        }

        private Ink(string name, CmykColor appearance, InkScreen? screen, bool isProcessInk)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (appearance == null)
                throw new ArgumentNullException("appearance");

            this.name = name;
            this.appearance = appearance;
            this.screen = screen;
            this.isProcessInk = isProcessInk;
        }

        public string Name
        {
            get { return name; }
        }

        // The CMYK value that shows the ink at full tone.
        public CmykColor Appearance
        {
            get { return appearance; }
        }

        // The halftone screen of the ink, or null where none is defined.
        public InkScreen? Screen
        {
            get { return screen; }
        }

        // Is this one of the four process inks?
        public bool IsProcessInk
        {
            get { return isProcessInk; }
        }

        public override string ToString()
        {
            return name;
        }
    }

    // The share of one ink in a palette color: the ink, and the factor of its full tone, from 0 to 1. A factor of 0 is
    // a component: it erases the ink where the palette color is drawn. An ink that is not a component of a palette
    // color leaves the ink as it is.
    public struct InkComponent : IEquatable<InkComponent>
    {
        public readonly Ink Ink;
        public readonly float Factor;

        public InkComponent(Ink ink, float factor)
        {
            if (ink == null)
                throw new ArgumentNullException("ink");
            if (!(factor >= 0 && factor <= 1))
                throw new ArgumentOutOfRangeException("factor", factor, "The factor of an ink component is from 0 to 1.");

            Ink = ink;
            Factor = factor;
        }

        // Bound a factor that a file gives to the range from 0 to 1. A value that is not a number is 0.
        public static float BoundFactor(float factor)
        {
            if (float.IsNaN(factor))
                return 0;
            else
                return Math.Min(Math.Max(factor, 0), 1);
        }

        public bool Equals(InkComponent other)
        {
            return Ink == other.Ink && Factor == other.Factor;
        }

        public override bool Equals(object obj)
        {
            return obj is InkComponent && Equals((InkComponent) obj);
        }

        public override int GetHashCode()
        {
            return (Ink == null ? 0 : Ink.GetHashCode()) ^ Factor.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Ink, Factor);
        }
    }
}
