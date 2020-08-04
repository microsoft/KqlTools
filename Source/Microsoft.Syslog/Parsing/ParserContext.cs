// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using Microsoft.Syslog.Model;
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("P:{Position}, ch: {Current}")]
    public class ParserContext
    {
        public string Text;
        public int Position;
        public string Prefix; // standard <n> prefix identifying 'syslog' message
        public SyslogEntry Entry;
        public readonly List<string> ErrorMessages = new List<string>();

        public char Current => Position < Text.Length ? Text[Position] : '\0';
        public char CharAt(int position) => this.Text[position];
        public bool Eof() => this.Position >= this.Text.Length;

        public ParserContext(string text)
        {
            Text = text.CutOffBOM(); 

        }

        public void AddError(string message)
        {
            ErrorMessages.Add($"{message} (near {this.Position})");
        }


    }
}
