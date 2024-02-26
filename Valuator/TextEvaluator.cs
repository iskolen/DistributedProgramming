using StackExchange.Redis;

namespace Valuator
{
    public class TextEvaluator
    {
        public static double CalculateRank(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0.0;

            int totalCharacters = text.Length;
            int nonAlphabeticCharacters = text.Count(c => !char.IsLetter(c));

            return (double)nonAlphabeticCharacters / totalCharacters;
        }

        public static int CalculateSimilarity(string text, IDatabase db, string currentKey)
        {
            if (IsDuplicate(text, db, currentKey))
                return 1;
            else
                return 0;
        }

        private static bool IsDuplicate(string text, IDatabase db, string currentKey)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            const string textKeyPrefix = "TEXT-";

            var textKeys = db.Multiplexer.GetServer("127.0.0.1:6379").Keys(pattern: textKeyPrefix + "*");

            foreach (var key in textKeys)
            {
                if (key != currentKey)
                {
                    string storedText = db.StringGet(key);

                    if (string.Equals(storedText, text, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
