using System;
using System.Collections.Generic;

namespace PurplePen.MapModel
{
    // Names the classes, or the ids, of a document: each in turn gets the shortest name not yet given, so the first
    // fifty-two are single letters (S-4, L-5). A name starts with a letter, which ids and class selectors require.
    public class SvgNames
    {
        // The characters that may start a name.
        private const string FirstCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // The characters that may follow the first.
        private const string FollowingCharacters = FirstCharacters + "0123456789";

        // How many names have been tried.
        private int count;

        // The names given otherwise, which are not given again, such as the ids of the pages' views.
        private readonly HashSet<string> reserved = new HashSet<string>(StringComparer.Ordinal);

        // Keep a name from being given: it is given otherwise.
        // name: the name.
        public void Reserve(string name)
        {
            reserved.Add(name);
        }

        // Give the next name: the shortest not given yet and not reserved.
        public string Next()
        {
            string name;
            do {
                name = NameOf(count++);
            } while (reserved.Contains(name));
            return name;
        }

        // The name given in a turn: all names of one character first, then those of two, and so on.
        // turn: the turn, counting from zero.
        public static string NameOf(int turn)
        {
            int length = 1;
            long namesOfLength = FirstCharacters.Length;
            long remaining = turn;
            while (remaining >= namesOfLength) {
                remaining -= namesOfLength;
                ++length;
                namesOfLength *= FollowingCharacters.Length;
            }

            char[] name = new char[length];
            for (int position = length - 1; position > 0; --position) {
                name[position] = FollowingCharacters[(int)(remaining % FollowingCharacters.Length)];
                remaining /= FollowingCharacters.Length;
            }
            name[0] = FirstCharacters[(int)remaining];
            return new string(name);
        }
    }
}
