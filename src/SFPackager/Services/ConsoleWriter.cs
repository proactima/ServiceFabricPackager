using System;
using SFPackager.Models;

namespace SFPackager.Services
{
    public class ConsoleWriter
    {
        private readonly AppConfig _baseConfig;

        public ConsoleWriter(AppConfig baseConfig)
        {
            _baseConfig = baseConfig;
        }

        public void WriteLine(string message, LogLevel logLevel = LogLevel.Debug)
        {
            if (_baseConfig.VerboseOutput)
            {
                Console.WriteLine(message);
            }
            else
            {
                if(logLevel < LogLevel.Debug)
                    Console.WriteLine(message);
            }
        }
    }
}