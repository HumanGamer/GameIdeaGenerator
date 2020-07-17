using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameIdeaGenerator
{
    public class GameTemplate
    {
        #region Initialization
        private Dictionary<string, List<string>> _sections;

        private GameTemplate()
        {
            _sections = new Dictionary<string, List<string>>();
        }
        #endregion

        public string Generate()
        {
            return "N/A";
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
                        if (_sections.ContainsKey(sectionName))
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
                    _sections.Add(sectionHeader, data);
                    continue;
                }

                string[] names = sectionHeader.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (names.Length != 2)
                    throw new TemplateReadException("Invalid Section '" + sectionHeader + "'");
                string sectionName = names[0].Trim();
                if (_sections.ContainsKey(sectionName))
                    throw new TemplateReadException("Duplicate Section '" + sectionName.Trim() + "'");

                string sectionExtension = names[1].Trim();
                toCheck.Add(sectionExtension, sectionName);

                _sections.Add(sectionName, data);
            }

            foreach (var pair in toCheck)
            {
                string from = pair.Key;
                string target = pair.Value;

                List<string> data = _sections[from];

                _sections[target].AddRange(data);
            }
        }
        #endregion
    }
}
