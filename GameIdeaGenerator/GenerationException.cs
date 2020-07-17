using System;
using System.Collections.Generic;
using System.Text;

namespace GameIdeaGenerator
{
    public class GenerationException : Exception
    {
        public GenerationException(string message) : base(message)
        {

        }
    }
}
