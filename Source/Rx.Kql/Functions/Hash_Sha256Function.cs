// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Kql.ExceptionTypes;

    [Description("hash_sha256")]
    public class HashSha256Function : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public HashSha256Function()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            string hashTarget = Arguments[0].GetValue(evt).ToString();

            if (string.IsNullOrEmpty(hashTarget))
            {
                return string.Empty;
            }

            using (SHA256 hash = SHA256.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(hashTarget))
                    .Select(item => item.ToString("x2")));
            }
        }
    }
}