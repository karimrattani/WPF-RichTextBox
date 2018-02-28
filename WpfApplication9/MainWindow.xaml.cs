//@Author: Karim Rattani
//@Description: First APP on WPF for learning purpose. Using RichTextBox to highlight usernames and hashtags
//Date: 02/27/2018


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RichTextBoxSocialMedia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        //get the text, separate each word, check for first char if its @ or #, if it's, match against rules, then format else do nothing
        private void TextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            if (Text_Input.Document == null)
                return;

            TextRange documentRange = new TextRange(Text_Input.Document.ContentStart, Text_Input.Document.ContentEnd);
            documentRange.ClearAllProperties();


            TextPointer navigator = Text_Input.Document.ContentStart; //get the start of the input

            //inplace to remove invisible tags and match char position with text position, stackoverflow.com/questions/2565783/wpf-flowdocument-absolute-character-position
            while (navigator.CompareTo(Text_Input.Document.ContentEnd) < 0)
            {
                TextPointerContext context = navigator.GetPointerContext(LogicalDirection.Backward);
                if (context == TextPointerContext.ElementStart && navigator.Parent is Run)
                {
                    CheckWordsInRun((Run)navigator.Parent);

                }
                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
            }
            Format(words);
        }

        //gets the substring from the text
        private String getSubString(String str, int index)
        {
            int end = index;
            while (end < str.Length && !isWhiteSpace(str[end]))
            {
                end++;
            }
            if (end != index) //inplace incase of any possibility for crash
                return str.Substring(index, end-index);
            else
                return "";
        }

        //checks if the character is whitespace
        private Boolean isWhiteSpace(char a)
        {
            if (a == ' ' || a =='\r' || a=='\n')
                return true;
            return false;
        }

        List<wordList> words = new List<wordList>(); //store all the valid username and hashtags

        private void CheckWordsInRun(Run run)
        {
            
            string inp = run.Text; // converts the run into text

            for (int i = 0; i < inp.Length; i++) {


                i = stripWhiteSpaces(inp, i);//strip white spaces

                string val = getSubString(inp, i); //get substring from the text=

                if (val.Length > 0 && (val[0] == '@' || val[0] == '#')) //check if first character of the substring is @ or # and length of my substring is greater than 0(incase of any unusual input)
                {
                    Boolean rulesMatched = false; //true if all rules will be matched
                    string newVal=""; //to store the value without punctuation
                    if (val[0] == '@' && (i == 0 || (isAllowedPriorCharacterForUsername(inp[i - 1])))) //if it's @ and it's either starting of the input text or the character before is allowed
                    {
                        newVal = getWordWithoutPunctuation(val);//trim the word till punctuation 
                        rulesMatched = matchUsernameRules(newVal);//match rules for the word
                    }
                    else if (val[0] == '#' && (i == 0 || isAllowedPriorCharacterForHashTag(inp[i - 1])))//if it's # and it's either starting of the input text or the character before is allowed
                    {
                        newVal = getWordWithoutPunctuation(val);//trim the word till punctuation 
                        rulesMatched = matchHashTagRules(newVal);//match rules for the word
                    }

                    if(rulesMatched){//if rules matched for the substring
                    TextPointer start = run.ContentStart.GetPositionAtOffset(i, LogicalDirection.Forward);//get the pointer offset to index i
                    TextPointer end = run.ContentStart.GetPositionAtOffset(i + newVal.Length, LogicalDirection.Backward); //get the pointer offset to i+length

                    // creates the word range, add it to the wordList in order to format it later
                    TextRange wordRange = new TextRange(start, end); 
                    wordList wL = new wordList();
                    wL.start = start;
                    wL.end = end;
                    wL.word = newVal;
                    words.Add(wL);

                    i += newVal.Length; //increment i to new value length, since we don't need to check again
                      }

                }
                
            }
        }

        //removes all extra whitespaces
        private int stripWhiteSpaces(string inp, int index)
        {
            while (index < inp.Length && isWhiteSpace(inp[index]))
            {
                index++;
            }
            return index;
        }

        //trim the punctuation as long as it's not @ or #
        private string getWordWithoutPunctuation(String inputWord)
        {
            int count=1;
            for (int i = 1; i < inputWord.Length;i++){
                if (((inputWord[i] != '@' && inputWord[i] != '#') && (inputWord[i]=='|' || char.IsPunctuation(inputWord[i]))))
                {
                    break;
                }
                count++;
            }
            String val = inputWord.Substring(0,count);
            //Console.WriteLine("my new val is "+val);
            return val;
        }

        //Returns true if its whitespace or bar or punctuation
        private Boolean isAllowedPriorCharacterForHashTag(Char c)
        {

            if (isWhiteSpace(c) || c=='|' || Char.IsPunctuation(c)) {
                return true;
            }

            return false;
        }

        //Returns true if its whitespace or bar or punctuation(not @ and #)
        private Boolean isAllowedPriorCharacterForUsername(Char c)
        {

            if(c == '@' || c == '#' ){
                return false;
            }
            if (isWhiteSpace(c) || c == '|' || Char.IsPunctuation(c))
            {
                return true;
            }

            return false;
        }



        //Formats the output list
        private void Format(List<wordList> m_tags)
        {
            Text_Input.TextChanged -= this.TextChangedEventHandler;

            for (int i = 0; i < m_tags.Count; i++)
            {
                TextRange range = new TextRange(m_tags[i].start, m_tags[i].end);
                range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
            }
            m_tags.Clear();

            Text_Input.TextChanged += this.TextChangedEventHandler;
        }

        //Match the rules for username, using twitter rules
        private Boolean matchUsernameRules(String user)
        {
            //Applied Twitter Rules
            if(user.Length>16 || user.Length <= 1){ //15 length username + 1 @
                return false;
            }

            for (int i = 1; i < user.Length; i++)//starts at 1 since 0 will always be @
            {
                if (!((user[i] >= 'a' && user[i] <= 'z') || user[i] == '_'))
                {
                    return false;
                }
            }
                return true;
        }

        //Match the rules for HashTag, using HashTag Rules
        private Boolean matchHashTagRules(String hashTag)
        {
            if (hashTag.Length <= 1)
            {
                return false;
            }
            for (int i = 1; i < hashTag.Length; i++)//starts at 1 since 0 will always be #
            {
                if ((hashTag[i] >= '!' && hashTag[i] <= '/') || (hashTag[i] >= '{' && hashTag[i] <= '~') || (hashTag[i] >= ':' && hashTag[i] <= '@'))//using regular ascii table
                {
                    return false;
                }
            }


                return true;
        }

        //create object to store start pointer, end pointer, and word, word is not required but can be use later on
        private struct wordList
        {
            public TextPointer start;
            public TextPointer end;
            public string word;
        }
    }
}
