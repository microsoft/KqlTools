// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Concurrent;

    public class GlobalFunctions
    {
        /// <summary>
        ///     Global list of currently loaded functions.
        /// </summary>
        public static ConcurrentDictionary<string, CslFunction> KqlFunctions =
            new ConcurrentDictionary<string, CslFunction>();

        /// <summary>
        ///     Parse and load any functions from the current file contents
        /// </summary>
        /// <param name="cslFileContent">the content from the file</param>
        public static void ReadFunctionsFromFile(string cslFileContent)
        {
            var functions = CslParser.ReadFunctionsFromFile(cslFileContent);
            foreach (var f in functions) Add(f.Name, f);
        }

        /// <summary>
        ///     Reads the function from a particular query
        /// </summary>
        /// <param name="queryContent">the content from the query</param>
        public static void ReadFunctionsFromQuery(string queryContent)
        {
            var functions = CslParser.ReadFunctionsFromQuery(queryContent);
            foreach (var f in functions) Add(f.Name, f);
        }

        /// <summary>
        ///     Gets the currently seeked function from memory by name
        /// </summary>
        /// <param name="name">the name of the function</param>
        /// <returns></returns>
        public static CslFunction GetFunction(string kqlFunctionName)
        {
            KqlFunctions.TryGetValue(kqlFunctionName, out var requestedCslFunction);
            return requestedCslFunction;
        }

        /// <summary>
        ///     Remove all Functions from the current Global object.
        /// </summary>
        public static void Clear()
        {
            KqlFunctions.Clear();
        }

        /// <summary>
        ///     Add Concurrent Dictionary function, and remove it prior to insertion
        /// </summary>
        /// <param name="kqlFunctionName"></param>
        /// <param name="cslFunction"></param>
        public static bool Add(string kqlFunctionName, CslFunction cslFunction)
        {
            // Update the function if it exists
            if (KqlFunctions.TryGetValue(kqlFunctionName, out var existingCslFunction))
            {
                return KqlFunctions.TryUpdate(kqlFunctionName, cslFunction, existingCslFunction);
            }

            return KqlFunctions.TryAdd(kqlFunctionName, cslFunction);
        }

        /// <summary>
        ///     Remove Concurrent Dictionary function
        /// </summary>
        /// <param name="kqlFunctionName"></param>
        public static bool Remove(string kqlFunctionName)
        {
            return KqlFunctions.TryRemove(kqlFunctionName, out var removedCslFunction);
        }
    }
}