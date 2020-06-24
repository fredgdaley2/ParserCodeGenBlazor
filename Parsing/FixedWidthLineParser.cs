namespace ParserCodeGenBlazor
{
    using System.Collections.Generic;

    public class FixedWidthLineParser
    {

        public string[] Parse(string lineToParse, int[] widths, bool trimWhiteSpace = false, int maxFields = -1)
        {
            char[] characters = lineToParse.ToCharArray();
            List<string> returnValueList = new List<string>();
            int startPosition = 0;
            string value = string.Empty;

            for (int widthIdx = 0; widthIdx < widths.Length; widthIdx++)
            {
                value = string.Empty;

                for (int positionIdx = 0; positionIdx < widths[widthIdx]; positionIdx++)
                {
                    value += characters[startPosition + positionIdx];
                }

                returnValueList.Add(trimWhiteSpace ? value.Trim() : value);

                if (maxFields > -1 && returnValueList.Count == maxFields)
                {
                    break;
                }
                startPosition += widths[widthIdx];

            }

            return returnValueList.ToArray();
        }
    }
}