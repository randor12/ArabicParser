using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using MySql.Data.MySqlClient;
using System.IO;
using HtmlAgilityPack;
using AngleSharp;
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

            string connectionString = "Server = 127.0.0.1; User Id = root; Password = Mftiarc14!; Database = dict";

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

            return Inserted;
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
        /*public static string[] GetHtmlWords(string input)
        {
            MatchCollection match = Regex.Matches(input, @"\b[\w']*\b");
        }*/

        static void Main(string[] args)
        {
            HtmlParser parser = new HtmlParser();
            //Collects all files within the articles section of wikipedia
            string[] directory = Directory.GetFiles("C:\\Users\\Ryan's Computer\\OneDrive\\Documents\\Arabic\\wikipedia-ar-html.tar\\wikipedia-ar-html\\ar\\articles\\ط\\س\\م", "*.html", SearchOption.AllDirectories);

            //Console.WriteLine("The directory is: C:\\Users\\Ryan's Computer\\OneDrive\\Documents\\Arabic\\wikipedia-ar-html.tar\\wikipedia-ar-html\\ar\\articles\\ط\\س\\م \n");

            //Add data to the database
            string connectionString = "Server = 127.0.0.1; User Id = root; Password = Password; Database = dict";

            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(connectionString);
            
            using (MySqlConnection connection = new MySqlConnection(builder.ConnectionString))
            {

                MySqlCommand command;
                string commandLine = "USE dict;\n";
                string action = "";

                string text = "";

                int count = GetId() + 1;
                
                foreach (var file in directory)
                {
                    var documents = new HtmlWeb();

                    documents.Load(file);

                    var htmlDoc = documents.Load(file);

                    var node = htmlDoc.DocumentNode;

                    text = node.OuterHtml + "\n\n";

                    text = Regex.Replace(text, "<[^>]*>", string.Empty);

                    text = Regex.Replace(text, @"^\s*$\n", string.Empty, RegexOptions.Multiline);
                    
                    foreach (var word in text)
                    {

                        if (HasArabicGlyphs(word.ToString()) && !ContainsWord(word.ToString()))
                        {
                            action += "INSERT INTO dict_ar VALUES (" +
                                count + ", '" + word + "', 'N'" +
                                ");\n";

                            count += 1;
                        }

                        Console.WriteLine(text);
                    }
                }


                commandLine += action;

                using (command = new MySqlCommand(commandLine, connection))
                {
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            Console.ReadKey();
        }
    }
}
