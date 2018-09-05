using Xunit;

namespace CSharpE.TestUtilities
{
    public static class AssertEx
    {
        public static void LinesEqual(string expected, string actual)
        {
            // PERF: unnecessary allocation
            string[] Split(string s) => s.Split(new[] {"\r\n", "\n"}, options: default);
        
            Assert.Equal(Split(expected), Split(actual));
        }
    }
}
