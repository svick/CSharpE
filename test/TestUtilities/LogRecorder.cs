using System.Collections.Generic;

namespace CSharpE.TestUtilities
{
    public class LogRecorder<T>
    {
        private List<T> messages = new List<T>();

        public void Record(T message) => messages.Add(message);

        public IReadOnlyList<T> Read()
        {
            var result = messages;

            messages = new List<T>();

            return result;
        }
    }
}