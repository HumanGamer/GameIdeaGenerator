using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameIdeaGenerator
{
    public class GameTemplate
    {
        #region Initialization
        protected Dictionary<string, List<string>> Sections;

        private GameTemplate()
        {
            Sections = new Dictionary<string, List<string>>();
        }
        #endregion

        public string Generate(Random random)
        {
            string template = PickFrom(random, "Template");
            template = DecideOptionals(random, template);
            template = FillBlanks(random, template);
            template = PickOptions(random, template);
            template = Finalize(random, template);

            template = char.ToUpper(template[0]) + template.Substring(1);

            return template;
        }

        protected string Finalize(Random random, string text)
        {
            char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'y', 'r' };

            while (text.Contains('['))
            {
                int index = text.IndexOf('[');
                int lastIndex = text.IndexOf(']', index);
                string start = text.Substring(0, index);
                string middle = text.Substring(index + 1, (lastIndex - 1) - index);
                string end = text.Substring(lastIndex + 1);

                string[] options = middle.Split(',');
                if (options.Length != 2)
                    throw new GenerationException("Missing comma '" + text + "'");

                char letter = end.Trim().ToLower()[0];
                if (vowels.Contains(letter))
                    text = start + options[1] + end;
                else
                    text = start + options[0] + end;
            }

            return text;
        }

        protected string PickOptions(Random random, string text)
        {
            while (text.Contains('('))
            {
                int index = text.IndexOf('(');
                int lastIndex = text.IndexOf(')', index);
                string start = text.Substring(0, index);
                string middle = text.Substring(index + 1, (lastIndex - 1) - index);
                string end = text.Substring(lastIndex + 1);

                string[] options = middle.Split(',');
                if (options.Length != 2)
                    throw new GenerationException("Missing comma '" + text + "'");

                text = start + options[random.Next(0, options.Length)] + end;
            }

            return text;
        }

        protected string FillBlanks(Random random, string text)
        {
            string lastSection = null;
            string lastValue = null;

            while (text.Contains('@'))
            {
                int index = text.IndexOf('@');
                int lastIndex = text.IndexOf('@', index + 1);
                if (lastIndex == -1)
                    throw new GenerationException("Mismatched '@' symbols");
                string start = text.Substring(0, index);
                string middle = text.Substring(index + 1, (lastIndex - 1) - index);
                bool plural = false;
                if (middle.EndsWith('*'))
                {
                    middle = middle.Substring(0, middle.Length - 1);
                    plural = true;
                }
                string end = text.Substring(lastIndex + 1);
                string picked;
                do
                {
                    picked = PickFrom(random, middle, plural);
                } while (picked == lastValue && middle == lastSection);
                lastValue = picked;
                lastSection = middle;

                text = start + picked + end;
            }

            return text;
        }

        protected string DecideOptionals(Random random, string text)
        {
            while (text.Contains('{'))
            {
                int index = text.IndexOf('{');
                int lastIndex = text.IndexOf('}', index);
                string start = text.Substring(0, index);
                string middle = text.Substring(index + 1, (lastIndex - 1) - index);
                string end = text.Substring(lastIndex + 1);
                if (random.Next(2) == 0)
                    text = start + middle + end;
                else
                    text = start + end;
            }

            return text;
        }

        protected string CheckPlural(Random random, string text, bool plural)
        {
            text = text.Trim();
            string baseText = text;
            if (text.StartsWith('[') && text.EndsWith(']'))
            {
                text = text.Substring(1, text.Length - 2);
                string[] options = text.Split(',');
                if (options.Length != 2)
                    throw new GenerationException("Invalid Entry '" + baseText + "'");
                text = options[plural ? 1 : 0];
            } else if (plural)
                text += 's';
            return text;
        }

        protected string PickFrom(Random random, string section, bool plural = false)
        {
            if (!Sections.ContainsKey(section))
                return null;

            List<string> options = Sections[section];
            return CheckPlural(random, options[random.Next(0, options.Count)], plural);
        }

        #region Load
        public static GameTemplate LoadGameTemplate(string path)
        {
            GameTemplate template = new GameTemplate();
            template.Read(path);
            return template;
        }

        protected void Read(string path)
        {
            Dictionary<string, List<string>> sections = new Dictionary<string, List<string>>();

            using (Stream s = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(s))
            {
                string sectionName = null;

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line.Trim().Length == 0)
                        continue;

                    if (line.StartsWith(' ') || line.StartsWith('\t'))
                    {
                        if (sectionName == null)
                            throw new TemplateReadException("Data without Section '" + line.Trim() + "'");

                        sections[sectionName].Add(line.Trim());
                    } else
                    {
                        sectionName = line.Trim();
                        if (Sections.ContainsKey(sectionName))
                            throw new TemplateReadException("Duplicate Section '" + sectionName.Trim() + "'");
                        sections.Add(sectionName, new List<string>());
                    }
                }
            }

            Dictionary<string,string> toCheck = new Dictionary<string, string>();
            foreach (var pair in sections)
            {
                string sectionHeader = pair.Key.Trim();
                List<string> data = pair.Value;

                if (!sectionHeader.Contains(':'))
                {
                    Sections.Add(sectionHeader, data);
                    continue;
                }

                string[] names = sectionHeader.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (names.Length != 2)
                    throw new TemplateReadException("Invalid Section '" + sectionHeader + "'");
                string sectionName = names[0].Trim();
                if (Sections.ContainsKey(sectionName))
                    throw new TemplateReadException("Duplicate Section '" + sectionName.Trim() + "'");

                string sectionExtension = names[1].Trim();
                toCheck.Add(sectionExtension, sectionName);

                Sections.Add(sectionName, data);
            }

            foreach (var pair in toCheck)
            {
                string from = pair.Key;
                string target = pair.Value;

                List<string> data = Sections[from];

                Sections[target].AddRange(data);
            }
        }
        #endregion
    }
}
