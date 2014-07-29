using System;

namespace RallyToFeature
{

    /// <summary>
    /// Class to handle the removal or formatting of HTML tags in a string.
    /// </summary>
    public static class HtmlHandler
    {

        /// <summary>
        /// Method to handle the removal or formatting of HTML tags in a string using char array.
        /// </summary>
        public static string FormatHtmlTags(string source)
        {

            var array = new char[source.Length];
            var arrayIndex = 0;
            var inside = false;
            var tagging = false;
            var tag = String.Empty;

            foreach (var @let in source)
            {
                if (@let == '<')
                {
                    inside = true;
                    tagging = true;
                    continue;
                }
                if (@let == '>')
                {
                    inside = false;
                    tagging = false;
                    switch (tag)
                    {
                        case "":
                            break;
                        case "B":
                            break;
                        case "BR":
                            //Console.WriteLine("Tag: {0}", tag);
                            //array[arrayIndex] = Convert.ToChar(10);
                            //arrayIndex++;
                            //array[arrayIndex] = Convert.ToChar(13);
                            //arrayIndex++;
                            break;
                        case "DIV":
                            //Console.WriteLine("Tag: {0}", tag);
                            array[arrayIndex] = Convert.ToChar(10);
                            arrayIndex++;
                            //array[arrayIndex] = Convert.ToChar(13);
                            //arrayIndex++;
                            break;
                        default:
                            Console.WriteLine("*** UNKNOWN TAG *** <{0}>", tag);
                            break;
                    }
                    tag = String.Empty;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = @let;
                    arrayIndex++;
                    continue;
                }
                if (tagging && Char.IsLetter(@let))
                {
                    tag += Char.ToUpper(@let);
                }
                else
                {
                    tagging = false;
                }
            }

            return new string(array, 0, arrayIndex);

        }

    }

}