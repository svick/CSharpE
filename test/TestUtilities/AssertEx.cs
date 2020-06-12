using Xunit;

namespace CSharpE.TestUtilities
{
    public static class AssertEx
    {
        public static void LinesEqual(string expected, string actual)
        {
            string NormalizeNewlines(string s) => s.Replace("\r\n", "\n");
        
            Assert.Equal(NormalizeNewlines(expected), NormalizeNewlines(actual));
        }
    }
}
