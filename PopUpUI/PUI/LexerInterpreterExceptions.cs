using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    public class LexerException : Exception
    {
        public LexerException(string message) : base(message)
        {

        }
    }

    public class InterpreterException : Exception
    {
        public InterpreterException(string message, int index) : base($"{message}. At index: {index}.")
        {

        }
    }
}
