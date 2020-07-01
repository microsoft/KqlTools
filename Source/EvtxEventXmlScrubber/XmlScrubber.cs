// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Microsoft.EvtxEventXmlScrubber
{
    using System;
    using System.Text;

    public static class XmlScrubber
    {
        private static XmlCharType xmlCharType = XmlCharType.Instance;

        /// <summary>
        ///     Verifies XML characters as defined in XML specification, repairs invlaid XML and removes or replaces
        ///     special characters with text respresentations.
        /// </summary>
        /// <param name="xmlData">The XML data.</param>
        /// <returns>Verified and repaired XML.</returns>
        /// <remarks>
        ///     Based on VerifyCharData method from https://referencesource.microsoft.com/#System.Xml/System/Xml/XmlConvert.cs.
        /// </remarks>
        public static string VerifyAndRepairXml(string xmlData)
        {
            if (xmlData == null || xmlData.Length == 0)
            {
                return string.Empty;
            }

            // Set initial StringBuilder capacity to 2X XML data length to minimize reallocation during character replacement.
            StringBuilder sb = new StringBuilder(xmlData.Length * 2);

            int i = 0;
            int len = xmlData.Length;
            while (i < len)
            {
                var ch = xmlData[i];
                if ((xmlCharType.charProperties[ch] & XmlCharType.fCharData) != 0)
                {
                    switch (ch)
                    {
                        case '\r':
                            // Skip CR alone or CR/LF pair.
                            if (i < len)
                            {
                                if (xmlData[i + 1] == '\n')
                                {
                                    // Skip CR/LF.
                                    i++;
                                }
                            }

                            break;
                        case '\n':
                            sb.Append(string.Empty);
                            break;
                        case '\t':
                            sb.Append("[TAB]");
                            break;
                        default:
                            sb.Append(ch);
                            break;
                    }

                    i++;
                }
                else
                {
                    sb.Append(string.Format("0x{0:X}", Convert.ToInt32(ch)));
                    i++;
                }
            }

            sb.Append("\r\n");

            return sb.ToString();
        }
    }
}