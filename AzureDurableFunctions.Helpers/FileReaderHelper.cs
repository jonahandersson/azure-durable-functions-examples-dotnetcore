using System;
using System.Collections.Generic;
using System.IO;

namespace AzureDurableFunctions.Helpers
{
    public static class FileReaderHelper
    {
       public static List<string> ReadInputStringsFromFile(string inputNamesTextFilePath)
        {
            List<string> inputStrings = new List<string>();
            using (var streamReader = new StreamReader(inputNamesTextFilePath))
            {
                while (streamReader.Peek() >= 0)
                    inputStrings.Add(streamReader.ReadLine());
            }
            return inputStrings;
        }
    }
}
