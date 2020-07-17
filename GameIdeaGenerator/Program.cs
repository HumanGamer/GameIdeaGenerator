using System;

namespace GameIdeaGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GameTemplate template;
            try
            {
                template = GameTemplate.LoadGameTemplate("Templates/basic.gtp");
            }
            catch (TemplateReadException ex)
            {
                Console.Error.WriteLine("Error: Failed to read template. " + ex.Message);
                return;
            }

            string result = template.Generate();
            Console.WriteLine(result);
        }
    }
}
