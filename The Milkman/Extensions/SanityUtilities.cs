namespace The_Milkman.Extensions
{
    public class SanityUtilities
    {
        public static string ExtractCode(string code)
        {
            int nlIndex = code.IndexOf('\n') + 1;
            int length = code.Length - 4 - nlIndex;
            return code.Substring(nlIndex, length);
        }
    }
}