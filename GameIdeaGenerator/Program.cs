using System;

namespace GameIdeaGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string templateFile = "basic";
            if (args.Length > 0)
            {
                templateFile = args[0];
            }

            int count = 1;
            if (args.Length > 1)
            {
                if (!int.TryParse(args[1], out count))
                {
                    Console.WriteLine("Invalid Argument: " + args[1]);
                    return;
                } else
                {
                    if (count < 1)
                        Console.WriteLine("Invalid Argument: " + args[1] + ", count must be positive!");
                }
            }

            GameTemplate template;
            try
            {
                template = GameTemplate.LoadGameTemplate("Templates/" + templateFile + ".gtp");
            }
            catch (TemplateReadException ex)
            {
                Console.Error.WriteLine("Error: Failed to read template. " + ex.Message);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                string result = template.Generate(new Random());
                Console.WriteLine(result);
            }
        }
    }
}
