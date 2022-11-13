using System;
using System.Threading.Tasks;

namespace HttpTransport.Transports
{
    public class PipelineBreak : Exception
    {
        public static readonly PipelineBreak Break = new PipelineBreak();

        public static Task BreakTask()
        {
            throw new PipelineBreak();
        }
    }
}
