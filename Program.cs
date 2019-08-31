using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Diff
{
    public static class Program
    {
        private static List<ulong> ExceptedLines = new List<ulong>();

        private static void Main(string[] args)
        {
            ExceptedLines = File.ReadAllLines("exceptions.txt").Select(ulong.Parse).ToList();
            Console.WriteLine(GetPercentageDiff(args[0], args[1], out var notes));
            File.WriteAllLines("notes.txt", notes);
            File.WriteAllLines("exceptions.txt", ExceptedLines.OrderBy(x => x).Select(x =>
            {
                var num = x;
                return num.ToString();
            }));
        }

        private static double GetPercentageDiff(string file1Path, string file2Path, out string[] notes)
        {
            return GetPercentageDiff(File.ReadAllLines(file1Path), File.ReadAllLines(file2Path), out notes);
        }

        // Token: 0x06000003 RID: 3 RVA: 0x0000229C File Offset: 0x0000049C
        private static double GetPercentageDiff(string[] file1Content, string[] file2Content, out string[] notes)
        {
            var list = new List<string>();
            var totalLength = 0.0;
            var changedLength = 0.0;
            for (var i = 0; i < file2Content.Length; i++)
            {
                var split = file2Content[i].Split(new[] {'='}, 2);
                if (split.Length <= 1)
                {
                    continue;
                }

                split[0] = split[0].RemoveTrailingWhitespace();

                if (ExceptedLines.Any(x => split[0].ToLower().Contains($"#autoloc_{x}")))
                {
                    var text = $"{split[0]}is excepted manually. Line is:{split[1]}";
                    if (!file1Content[i].Equals(file2Content[i]))
                    {
                        text += "; BUT TEXTS ARE NOT SAME!";
                        ExceptedLines.Remove(ulong.Parse(split[0].Split('_')[1]));
                    }
                    list.Add(text);
                }
                else
                {
                    var arg = split[0];
                    split[0] = string.Empty;
                    if (!file1Content[i].Equals(file2Content[i]))
                    {
                        changedLength += ConvertToString(split).Length - 1;
                    }
                    else
                    {
                        list.Add($"{arg}is not translated yet. Line is:{split[1]}");
                    }
                    totalLength += ConvertToString(split).Length - 1;
                }
            }
            notes = list.ToArray();
            return changedLength / totalLength;
        }

        private static string ConvertToString(params string[] strs)
        {
            return strs.Aggregate("", (current, str) => current + str);
        }

        public static string RemoveTrailingWhitespace(this string s)
        {
            while (s.StartsWith("\t") || s.StartsWith(" "))
            {
                s = s.Substring(1);
            }
            return s;
        }
    }
}
