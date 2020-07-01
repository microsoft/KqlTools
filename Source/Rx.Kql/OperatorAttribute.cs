// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System;

    public class OperatorAttribute : Attribute
    {
        public string[] Operators { get; set; }
        
        public OperatorAttribute(params string[] operators)
        {
            Operators = operators;
        }
    }
}
