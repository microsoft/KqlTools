// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
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
