// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Syslog.Model;

    public class IpAddressesExtractor: IValuesExtractor
    {
        const char Dot = '.';
        const char Colon = ':';

        public IList<NameValuePair> ExtractValues(ParserContext ctx)
        {
            var entry = ctx.Entry; 
            var ipList = new List<NameValuePair>(); 

            var ipv4list = FindIpv4Addresses(entry.Message).ToList();
            if (ipv4list != null && ipv4list.Count > 0)
            {
                var prmList = ipv4list.Select(ip => new NameValuePair() { Name = "IPv4", Value = ip }).ToList();
                ipList.AddRange(prmList);
            }

            var ipv6list = FindIpv6Addresses(entry.Message).ToList();
            if (ipv6list != null && ipv6list.Count > 0)
            {
                var prmList = ipv6list.Select(ip => new NameValuePair() { Name = "IPv6", Value = ip }).ToList();
                ipList.AddRange(prmList);
            }

            return ipList; 
        }

        private static IEnumerable<string> FindIpv4Addresses(string message)
        {
            var start = 1; 
            while(start < message.Length - 5)
            {
                // try to find the first dot in IP like 127.0.0.1
                var dotPos = message.IndexOf(Dot, start);
                // found, and at least single char before, and a few chars after
                if (dotPos <= 0 || dotPos >= message.Length - 5)
                    yield break; //we are done
                start = message.SkipUntil(dotPos + 1, ch => !ch.IsIpV4Char()); //for the next iteration, skip beyond the whole address
                // there should be digit before and after
                if (!char.IsDigit(message[dotPos - 1]) || !char.IsDigit(message[dotPos + 1]))
                    continue; // proceed to next occurrence
                // try getting IP; first go back and find first digit
                var ipStart = dotPos; 
                while (ipStart > 0 && char.IsDigit(message[ipStart - 1]))
                    ipStart--;
                var ipEnd = ipStart;
                while (ipEnd < message.Length - 1 && message[ipEnd + 1].IsIpV4Char())
                    ipEnd++;
                var ipStr = message.Substring(ipStart, ipEnd - ipStart + 1);
                start = ipEnd + 1; //for next iteration
                // check it has 4 segments
                if (IsIpv4Address(ipStr))
                    yield return ipStr; 
            } //while
        }

        private static IEnumerable<string> FindIpv6Addresses(string message)
        {
            var start = 1;
            while (start < message.Length - 10)
            {
                // try to find the first colon in IP
                var colPos = message.IndexOf(Colon, start);
                // found, and at least single char before, and a few chars after
                if (colPos <= 0 || colPos >= message.Length - 5)
                    yield break; //we are done
                start = message.SkipUntil(colPos + 1, ch => !ch.IsIpV6Char()); //for the next iteration
                // there should be hex digit before and hex digit or colon after
                if (!message[colPos - 1].IsHexDigit() || ! message[colPos + 1].IsIpV6Char())
                    continue; // proceed to next occurrence
                // try getting IP; first go back and find first digit
                var ipStart = colPos;
                while (ipStart > 0 && message[ipStart - 1].IsHexDigit())
                    ipStart--;
                var ipEnd = ipStart;
                while (ipEnd < message.Length - 1 && message[ipEnd + 1].IsIpV6Char())
                    ipEnd++;
                var ipStr = message.Substring(ipStart, ipEnd - ipStart + 1);
                start = ipEnd + 1; //for next iteration
                if (IsIpv6Address(ipStr))
                    yield return ipStr;
            } //while
        }

        /// <summary>Provides quick test for IPv4 address. </summary>
        /// <param name="value">String value to check.</param>
        /// <returns></returns>
        public static bool IsIpv4Address(string value)
        {
            // quick checks to get out quickly
            if (string.IsNullOrWhiteSpace(value) || value.Length < 7)
                return false;
            // first and last chars must be digits, and must contain dot
            if (!char.IsDigit(value[0]) || !char.IsDigit(value[value.Length - 1]) || !value.Contains(Dot))
                return false;
            var segms = value.Split(Dot);
            if (segms.Length != 4)
                return false;
            // check that every segment is integer
            // last segment may include :port, so remove it
            var colPos = segms[3].IndexOf(':');
            if (colPos > 0)
                segms[3] = segms[3].Substring(0, colPos); 
            foreach (var segm in segms)
                if (segm.Length > 3 || !int.TryParse(segm, out var _))
                    return false;
            return true; 
        }

        /// <summary>Provides quick test for IPv6 address. </summary>
        /// <param name="value">String value to check.</param>
        /// <returns></returns>
        public static bool IsIpv6Address(string value)
        {
            // quick checks to get out quickly
            if (string.IsNullOrWhiteSpace(value) || value.Length < 7 || !value.Contains(Colon))
                return false;
            // check it has 8 segments or contains :: - do not confuse it with time 15:20:46
            var segms = value.Split(':');
            if (segms.Length <= 4)
                return false;
            if (segms.Length < 8 && !value.Contains("::") || segms.Length > 9) //segm #9 might be port
                return false;
            foreach(var segm in segms)
            {
                if (string.IsNullOrEmpty(segm))
                    continue; //case for :: - skipped 0 segments
                if (segm.Length > 4)
                    return false; 
                // check hex digits only
                if (segm.ToCharArray().Any(c => !c.IsHexDigit())) 
                    return false; 
            }
            return true; 
        }

        public static IList<NameValuePair> ExtractIpAddresses(IList<NameValuePair> fromValuesOf)
        {
            var ipv4s = fromValuesOf.Where(p => IpAddressesExtractor.IsIpv4Address(p.Value))
                                .Select(p => new NameValuePair() { Name = "IPv4", Value = p.Value }).ToList();
            var ipv6s = fromValuesOf.Where(p => IpAddressesExtractor.IsIpv6Address(p.Value))
                                .Select(p => new NameValuePair() { Name = "IPv6", Value = p.Value }).ToList();
            var all = new List<NameValuePair>(ipv4s);
            all.AddRange(ipv6s);
            return all; 

        }
    }
}
