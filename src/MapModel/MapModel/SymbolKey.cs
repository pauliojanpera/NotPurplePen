using System;
using System.Text;

namespace PurplePen.MapModel
{
    // A symbol key: the identity of a symbol in its map (Symbol.Key). The maker of the symbol gives it, or the map gives
    // it from its counter. A key has a text form, which a self-supporting vector file can carry, and two keys are the
    // same when their text forms are the same, character by character. A key is not the number of a symbol definition
    // (SymDef.SymbolId).
    public sealed class SymbolKey : IEquatable<SymbolKey>
    {
        // The digits of the numbers of the keys, in base 62: 0 to 9, A to Z and a to z, in this order.
        private const string Base62Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        private readonly string text;

        // Make a symbol key from its text form.
        // text: the text form, of at least one character.
        public SymbolKey(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (text.Length == 0)
                throw new ArgumentException("A symbol key has at least one character.", nameof(text));

            this.text = text;
        }

        // The text form of the key.
        public string Text
        {
            get { return text; }
        }

        // The key that a value of the counter of a map gives: the value in base 62.
        // value: the value of the counter.
        public static SymbolKey FromCounter(long value)
        {
            return new SymbolKey(ToBase62(value));
        }

        // The text form of a number in base 62, with the digits 0 to 9, A to Z and a to z, the most significant first.
        // value: the number, 0 or more.
        public static string ToBase62(long value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "A number of a key is 0 or more.");
            if (value == 0)
                return "0";

            StringBuilder digits = new StringBuilder();
            while (value > 0) {
                digits.Insert(0, Base62Digits[(int)(value % 62)]);
                value /= 62;
            }

            return digits.ToString();
        }

        public bool Equals(SymbolKey other)
        {
            return !ReferenceEquals(other, null) && string.Equals(text, other.text, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SymbolKey);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(text);
        }

        public static bool operator ==(SymbolKey key1, SymbolKey key2)
        {
            return ReferenceEquals(key1, null) ? ReferenceEquals(key2, null) : key1.Equals(key2);
        }

        public static bool operator !=(SymbolKey key1, SymbolKey key2)
        {
            return !(key1 == key2);
        }

        public override string ToString()
        {
            return text;
        }
    }
}
