﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Diagnostics;

// Basic Thai IME word list reader & browser

// Thai word list from:
// https://github.com/nv23/thai-wordlist

namespace ThaiIMEBasic
{
    class Manager
    {
        const string
            wordlistFilename = "thai_wordlist.txt",
            frequencyFilename = "frequency_list.txt";
        private static Dictionary<string, string> transcriptionDict;
        private static List<(string, List<string>)> wordList;
        private static Dictionary<string, int> frequencyDict;

        // must be called once during lifetime
        public static void init() {
            transcriptionDict = new Dictionary<string, string>() {
                { "ก", "k" },
                { "ข", "kh" },
                { "ฃ", "kh" },
                { "ค", "kh" },
                { "ฅ", "kh" },
                { "ฆ", "kh" },
                { "ง", "ng" },
                { "จ", "c" },
                { "ฉ", "ch" },
                { "ช", "ch" },
                { "ซ", "s" },
                { "ฌ", "ch" },
                { "ญ", "y" },
                { "ฎ", "d" },
                { "ฏ", "t" },

                { "ฐ", "th" },
                { "ฑ", "th" },
                { "ฒ", "th" },
                { "ณ", "n" },
                { "ด", "d" },
                { "ต", "t" },
                { "ถ", "th" },
                { "ท", "th" },
                { "ธ", "th" },
                { "น", "n" },
                { "บ", "b" },
                { "ป", "p" },
                { "ผ", "ph" },
                { "ฝ", "f" },
                { "พ", "ph" },
                { "ฟ", "f" },

                { "ภ", "ph" },
                { "ม", "m" },
                { "ย", "y" },
                { "ร", "r" },
                { "ฤ", "r" },
                { "ล", "l" },
                { "ฦ", "l" },
                { "ว", "w" },
                { "ศ", "s" },
                { "ษ", "s" },
                { "ส", "s" },
                { "ห", "h" },
                { "ฬ", "l" },
                { "อ", "o" },
                { "ฮ", "h" },

                { "ะ", "a" },
                { "า", "a" },
                { "ำ", "am" },

                { "เ", "e" },
                { "แ", "ae" },
                { "โ", "o" },
                { "ใ", "ai" },
                { "ไ", "ai" },
                { "ๅ", "a" },
                { "ๆ", "q" },

                { "๐", "0" },
                { "๑", "1" },
                { "๒", "2" },
                { "๓", "3" },
                { "๔", "4" },
                { "๕", "5" },
                { "๖", "6" },
                { "๗", "7" },
                { "๘", "8" },
                { "๙", "9" },

                { " ", " " }
            };
            loadWordList();
            loadFrequencyDict();
        }

        public static string getTranscription(string thai) =>
            string.Join("",
                thai.ToCharArray()
                    .Where(c => transcriptionDict.ContainsKey("" + c))
                    .Select(c => transcriptionDict["" + c]));

        public async static void loadWordList()
        {
            Debug.WriteLine("load word list");

            wordList = new List<(string, List<string>)>();

            StreamReader sr = null;

            try
            {
                sr = new StreamReader(wordlistFilename);
                string word, transcription;

                while (!sr.EndOfStream)
                {
                    word = await sr.ReadLineAsync();
                    transcription = getTranscription(word);

                    var result = wordList.Where(pair => pair.Item1 == transcription);
                    if (result.Count() == 0)
                        wordList.Add((
                            transcription,
                            new List<string>() { word }));
                    else
                        result.First().Item2.Add(word);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }

        public async static void loadFrequencyDict() {
            Debug.WriteLine("load frequency dict");

            frequencyDict = new Dictionary<string, int>();
            StreamReader sr = null;

            try
            {
                sr = new StreamReader(frequencyFilename);

                string line;
                string[] temp;

                // Todo: read the dictionary
                while (!sr.EndOfStream) {
                    line = await sr.ReadLineAsync();
                    temp = line.Split('\t');
                    frequencyDict.Add(temp[0], Convert.ToInt32(temp[1]));
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            finally {
                if (sr != null)
                    sr.Close();
            }
        }

        public static string[] search(string term) {
            var exact = wordList.Where(pair => pair.Item1 == term).SelectMany(pair => pair.Item2).ToArray();

            // flatten list of lists
            var startsWith = wordList.Where(pair => pair.Item1.StartsWith(term)) // pair.Item1.Contains(term)
                .SelectMany(pair => pair.Item2)
                .ToArray();

            return exact.Concat(startsWith)
                .OrderByDescending(word => getFrequency(word))
                .Distinct()
                .ToArray();
        }

        public static int getFrequency(string thaiWord) =>
            frequencyDict.ContainsKey(thaiWord) ?
                frequencyDict[thaiWord] : 0;
    }
}
