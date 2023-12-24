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
    }
}
