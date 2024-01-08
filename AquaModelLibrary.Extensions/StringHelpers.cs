namespace AquaModelLibrary.Helpers
{
    public class StringHelpers
    {
        public static Dictionary<char, char> illegalChars = new Dictionary<char, char>() {
            { '<', '[' },
            { '>', ']'},
            {':', '-'},
            {'"', '\''},
            {'/', '_'},
            {'\\', '_'},
            {'|', '_'},
            {'?', '_'},
            {'*', '_'},
        };
        public static Dictionary<char, char> illegalCharsPath = new Dictionary<char, char>() {
            { '<', '[' },
            { '>', ']'},
            {'"', '\''},
            {'|', '_'},
            {'?', '_'},
            {'*', '_'},
        };

        public static string GetLastPipeString(string str)
        {
            var split = str.Split('|');
            return split[split.Length - 1];
        }

        public static string NixIllegalCharacters(string str)
        {
            foreach (var ch in illegalChars.Keys)
            {
                if (str.Contains(ch))
                {
                    str = str.Replace(ch, illegalChars[ch]);
                }
            }

            return str;
        }

        public static string NixIllegalCharactersPath(string str)
        {
            foreach (var ch in illegalCharsPath.Keys)
            {
                if (str.Contains(ch))
                {
                    str = str.Replace(ch, illegalCharsPath[ch]);
                }
            }

            return str;
        }

        //https://stackoverflow.com/questions/8809354/replace-first-occurrence-of-pattern-in-a-string
        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string ExtToIceEnvExt(string ext)
        {
            string newExt = ext;
            if(ext?.Length > 0 && ext[0] == '.')
            {
                newExt = ext.Substring(1, ext.Length - 1);
            }
            while(newExt.Length < 4)
            {
                newExt += "\0";
            }
            if(newExt.Length > 4)
            {
                newExt = newExt.Substring(0, 4);
            }

            return newExt;
        }
    }
}
