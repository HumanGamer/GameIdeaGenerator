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

                        _sections[sectionName].Add(line.Trim());
                    } else
                    {
                        sectionName = line.Trim();
                        if (_sections.ContainsKey(sectionName))
                            throw new TemplateReadException("Duplicate Section '" + sectionName.Trim() + "'");
                        _sections.Add(sectionName, new List<string>());
                    }
                }
            }
        }
        #endregion
    }
}
