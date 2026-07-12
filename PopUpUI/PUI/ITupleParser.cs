using PopUpUI.PUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PopUpUI.PUI
{
    public interface ITupleParser
    {
        public string TypeName { get; }
        /// <summary>Parses a value represented as a tuple. TokenCursor will be pointing to first paranthesis of the tuple.</summary>
        /// <param name="cursor"></param>
        /// <returns>The parsed object. Also cursor will be advanced right after close paranthesis ')'.</returns>
        public object Parse(TokenCursor cursor);
    }
}
