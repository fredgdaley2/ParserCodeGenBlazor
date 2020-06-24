namespace ParserCodeGenBlazor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DelimitedLineParserResult
    {
        public string[] Values { get; set; }
        public bool Errored { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class Delimiters
    {
        public const char Comma = ',';
        public const char Tab = '\t';
        public const char Pipe = '|';
    }

    public class DelimitedLineParser
    {

        public DelimitedLineParserResult Parse(string lineToParse, char delimiter, int maxFields = -1, bool trimWhiteSpace = true, bool fieldsEnclosedInQuotes = true)
        {
            char[] characters = lineToParse.ToCharArray();
            List<string> returnValueList = new List<string>();
            string tempString = string.Empty;
            bool blockUntilEndQuote = false;
            bool parseErrored = false;
            string errorMsg = string.Empty;
            for (int cIdx = 0; cIdx < characters.Length; cIdx++)
            {
                char character = characters[cIdx];
                if (character == '"' && fieldsEnclosedInQuotes)
                {
                    if (blockUntilEndQuote == false)
                    {
                        blockUntilEndQuote = true;
                    }
                    else if (blockUntilEndQuote == true)
                    {
                        blockUntilEndQuote = false;
                    }
                }

                if (character != delimiter)
                {
                    tempString = tempString + character;
                }
                else if (character == delimiter && (blockUntilEndQuote == true))
                {
                    tempString = tempString + character;
                }
                else
                {
                    if (fieldsEnclosedInQuotes)
                    {
                        errorMsg = ParseFieldEnclosedInQuotes(tempString, trimWhiteSpace, returnValueList);
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            errorMsg += string.Format(" Field ({0})", returnValueList.Count);
                            parseErrored = true;
                            break;
                        }
                    }
                    else
                    {
                        returnValueList.Add(TrimAllWhiteSpace(tempString, trimWhiteSpace));
                    }

                    if (ReachedMaxFields(maxFields, returnValueList))
                    {
                        break;
                    }
                    tempString = "";
                }

                if (cIdx + 1 == lineToParse.Length)
                {
                    if (fieldsEnclosedInQuotes)
                    {
                        errorMsg = ParseFieldEnclosedInQuotes(tempString, trimWhiteSpace, returnValueList);
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            errorMsg += string.Format(" Field ({0})", returnValueList.Count);
                            parseErrored = true;
                            break;
                        }
                    }
                    else
                    {
                        returnValueList.Add(TrimAllWhiteSpace(tempString, trimWhiteSpace));
                    }

                    if (ReachedMaxFields(maxFields, returnValueList))
                    {
                        break;
                    }
                    tempString = "";
                }
            }

            string[] returnValue = returnValueList.ToArray();
            returnValueList.Clear();
            return new DelimitedLineParserResult() { Values = returnValue, Errored = parseErrored, ErrorMessage = errorMsg };
        }

        private string ParseFieldEnclosedInQuotes(string tempString, bool trimWhiteSpace, List<string> returnValueList)
        {
            if (!DblQuoteCompliant(tempString))
            {
                return "Malformed Line: " + tempString;
            }
            returnValueList.Add(TrimAllWhiteSpace(RemoveFirstOccurance(RemoveLastOccurance(tempString, "\""), "\"").Replace("\"\"", "\""), trimWhiteSpace));
            return string.Empty;
        }

        private bool ReachedMaxFields(int maxFields, List<string> returnValueList)
        {
            return maxFields > -1 && returnValueList.Count == maxFields;
        }

        private bool DblQuoteCompliant(string checkValue)
        {
            if (EnclosedInQuotes(checkValue))
            {
                string firstLastRemoved = RemoveFirstOccurance(RemoveLastOccurance(checkValue, "\""), "\"");
                string escapedDblQuotesRemoved = firstLastRemoved.Replace("\"\"", "");
                return escapedDblQuotesRemoved.IndexOf("\"") == -1;
            }
            return true;
        }

        private bool EnclosedInQuotes(string checkValue)
        {
            if (!string.IsNullOrWhiteSpace(checkValue) && checkValue.Length > 1)
            {
                return Convert.ToString(checkValue[0]) == "\"" && Convert.ToString(checkValue[checkValue.Length - 1]) == "\"";
            }
            return false;

        }

        private string TrimAllWhiteSpace(string source, bool trimWhiteSpace)
        {
            return trimWhiteSpace ? source.Trim() : source;
        }

        private string RemoveFirstOccurance(string source, string toRemove)
        {
            return string.IsNullOrWhiteSpace(source) ? source
                : Convert.ToString(source[0]) == toRemove ? source.Substring(1) : source;
        }

        private string RemoveLastOccurance(string source, string toRemove)
        {
            return string.IsNullOrWhiteSpace(source) ? source
                : Convert.ToString(source[source.Length - 1]) == toRemove ? source.Substring(0, source.Length - 1) : source;
        }
    }
}
