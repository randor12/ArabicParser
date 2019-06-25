using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace ArabicParserApp
{
    class Program
    {
        /*
         * Returns the next id number in the list
         */
        public static int GetId()
        {
            int num = 0;

            string connectionString = "Server = 127.0.0.1; User Id = root; Password = Password; Database = dict";

            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(connectionString);

            using (MySqlConnection connection = new MySqlConnection(builder.ConnectionString))
            {
                MySqlCommand command;
                string commandLine = "USE dict;\n" +
                    "SELECT ID FROM dict_ar order by ID desc limit 1;\n";

                using (command = new MySqlCommand(commandLine, connection))
                {
                    command.Connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            num = reader.GetInt32("ID");
                        }
                    }

                }
            }

            return num;
        }

        /*
         * Checks the lexicon to see if the word already was inputted
         * @param word              word being checked
         * @return Inserted         Returns if the word was already inserted or not
         */
        public static bool ContainsWord(string word)
        {
            bool Inserted = false;

            string check = "";

            string connectionString = "Server = 127.0.0.1; User Id = root; Password = Password; Database = dict";

            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(connectionString);

            using (MySqlConnection connection = new MySqlConnection(builder.ConnectionString))
            {
                MySqlCommand command;
                string commandLine = "USE dict;\n" +
                    "SELECT ar FROM dict_ar where ar = '" + word + "';\n";

                using (command = new MySqlCommand(commandLine, connection))
                {
                    command.Connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            check = reader.GetString("ar");

                            //Console.WriteLine("\n\n" + word.Equals(check));
                            //Console.WriteLine(word + " is being checked");
                            //Console.WriteLine(check + " was found in the reader");
                        }
                    }

                    if (check.Equals(word))
                    {
                        Inserted = true;
                    }

                }
            }


            return Inserted;
        }

        /*
         * Checks if the letter is [a - z]
         */
        public static bool CheckEnglishLetter(string word)
        {
            bool isEnglish = false;
            
            foreach (char c in word)
            {
                string input = c.ToString();
                bool isLetter = Regex.IsMatch(input, "[a-z]", RegexOptions.IgnoreCase);

                if (isLetter)
                {
                    isEnglish = true;
                }
            }

            return isEnglish;
        }

        /*
         * Check for numbers
         */
        public static bool CheckNumbers(string word)
        {
            bool hasNumbers = false;

            foreach (var c in word)
            {
                string input = c.ToString();
                bool isNumber = Regex.IsMatch(input, "[0-9]", RegexOptions.Compiled);

                if (isNumber)
                {
                    hasNumbers = true;
                }
            }

            return hasNumbers;
        }

        /**
         * Determines if a character is arabic
         * @param text          Text
         * @return isArabic         Returns true if the character is an arabic glyph
         */
        public static bool HasArabicGlyphs(string text)
        {
            bool isArabic = false;

            char[] glyphs = text.ToCharArray();

            foreach (char glyph in glyphs)
            {
                if (glyph >= 0x600 && glyph <= 0x6ff)
                {
                    isArabic = true;
                }
                if (glyph >= 0x750 && glyph <= 0x77f)
                {
                    isArabic = true;
                }
                if (glyph >= 0xfb50 && glyph <= 0xfc3f)
                {
                    isArabic = true;
                }
                if (glyph >= 0xfe70 && glyph <= 0xfefc)
                {
                    isArabic = true;
                }
            }

            return isArabic;
        }

        /**
         * The purpose of this function is to extract only the words that are arabic
         */
        public static List<string> GetArabicWords(string input)
        {
            string[] delimiters = new string[] { " " };

            string[] words = input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            List<string> ArabicWords = new List<string>();

            foreach (var word in words)
            {
                if (HasArabicGlyphs(word))
                {
                    if (!CheckEnglishLetter(word))
                    {
                        if (!CheckNumbers(word))
                        {
                            if (word.Length <= 4000)
                            {
                                ArabicWords.Add(word);
                            }
                        }
                    }
                }
            }

            return ArabicWords;
        }

        /*
         * Reverse text
         */
        public static string Reverse(string text)
        {
            if (text == null) return null;
            char[] array = text.ToCharArray();
            Array.Reverse(array);
            return new String(array);
        }

        /*
         * Process files
         * @param sDir          directory
         */
        public static void ProcessInputFiles(string sDir)
        {
            try
            {
                numFiles = Directory.GetFiles(sDir, "*.html").Length;

                /*************************************************************
                 * SELECT BELOW FOR ALL FILES WITHIN THE DIRECTORY (INCLUDING SUB FILES)
                 * 
                 * WILL NEED TO ADD "SearchOption.AllDirectories" below
                 * 
                 *numFiles = Directory.GetFiles(sDir, "*.html", SearchOption.AllDirectories).Length;
                 *************************************************************/


                foreach (string f in Directory.GetFiles(sDir, "*.html"))
                {
                    ProcessFile(f);
                }
                

            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt);
            }
        }

        private static int numFiles;

        /*
         * Add words to the text file
         * @param f         File
         */
        private static void ProcessFile(string f)
        {
            FileStream fileStream = new FileStream("ArArticle.txt", FileMode.Append);

            StreamWriter writer = new StreamWriter(fileStream);
            

            string text = "";

            int count = 0;
                
            var encoding = Encoding.UTF8;

            Console.OutputEncoding = System.Text.Encoding.Unicode;

            var HtmlToString = File.ReadAllText(f, encoding);

            var documents = new HtmlDocument();

            documents.LoadHtml(HtmlToString);

            text = documents.ParsedText;

            text = Regex.Replace(text, "<[^>]*>", string.Empty);

            text = Regex.Replace(text, @"^\s*$\n", " ", RegexOptions.Multiline);

            Console.WriteLine("Number of articles: " + passes + " out of " + numFiles);

            passes++;

            //Console.WriteLine(Reverse(text) + "\n");

            Console.WriteLine("File Name: " + f.ToString());

            List<string> ArabicWords = GetArabicWords(text);

            foreach (var word in ArabicWords)
            {

                if (!AddedWords.Contains(word))
                {
                    AddedWords.Add(word);
                    //action += "INSERT INTO dict_ar VALUES (" + count +
                    //   ", N'" + word + "', 'N');";
                    writer.WriteLine(word);
                    count += 1;
                }
            }

            Console.WriteLine("Words downloaded: " + count);
            //Console.WriteLine(text);

                
            writer.Close();
            
            
        }
        
        private static List<string> AddedWords;

        private static int passes;

        static void Main(string[] args)
        {
            //Test
            Console.WriteLine(HasArabicGlyphs("المقال ")); //Should be true

            Console.WriteLine(HasArabicGlyphs("hello world")); //Should be false

            Console.WriteLine(HasArabicGlyphs("xل") && CheckEnglishLetter("xل")); //should be true

            Console.WriteLine(CheckNumbers("1") && CheckNumbers("09a")); //Should be true

            Console.WriteLine(CheckNumbers("a")); //should be false

            Console.ReadKey();

            AddedWords = new List<string>();

            passes = 1;

            /****************************************************************************
             *Process directory
             ****************************************************************************/
            ProcessInputFiles("C:\\Users\\rnicholas\\Documents\\ArabicArticles");

            Console.WriteLine("Completed");

            Console.ReadKey();
        }
    }
}
