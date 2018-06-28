using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Console = Colorful.Console;

namespace ConsoleLogCapture
{

    public class Parameter
    {
        /// <summary>
        /// Gets or sets the verb.
        /// </summary>
        /// <value>
        /// The verb.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.Name}: {this.Description}";
        }
    }

    public class ParameterBuilder : Parameter, IParameterBuilder
    {
        /// <summary>
        /// The console handler builder
        /// </summary>
        private IConsoleHandlerBuilder consoleHandlerBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBuilder"/> class.
        /// </summary>
        /// <param name="consoleHandler">The console handler.</param>
        public ParameterBuilder(IConsoleHandlerBuilder consoleHandler)
        {
            this.consoleHandlerBuilder = consoleHandler;
        }

        /// <summary>
        /// Bases this instance.
        /// </summary>
        /// <returns></returns>
        public IConsoleHandlerBuilder Base()
        {
            return this.consoleHandlerBuilder;
        }

        /// <summary>
        /// Matches the specified parameter name.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns></returns>
        public IParameterBuilder Match(string paramName)
        {
            this.Name = paramName;
            return this;
        }

        /// <summary>
        /// Descriptions the specified description.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        IParameterBuilder IParameterBuilder.Description(string description)
        {
            this.Description = description;
            return this;
        }

        /// <summary>
        /// Writes the help.
        /// </summary>
        /// <returns></returns>
        public string GetHelp()
        {
            return base.ToString();
        }
    }

    public class ConsoleHandler
    {
        /// <summary>
        /// Gets or sets the verb.
        /// </summary>
        /// <value>
        /// The verb.
        /// </value>
        public string Verb { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();

        /// <summary>
        /// Gets or sets the handler.
        /// </summary>
        /// <value>
        /// The handler.
        /// </value>
        public Func<List<Parameter>, bool> Handler { get; set; }
    }

    public class ConsoleHandlerBuilder : ConsoleHandler, IConsoleHandlerBuilder
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string HandlerExample { get; set; }

        /// <summary>
        /// Gets or sets the conditions.
        /// </summary>
        /// <value>
        /// The conditions.
        /// </value>
        public List<Tuple<Func<List<Parameter>, bool>, Func<string>>> Conditions { get; set; } = new List<Tuple<Func<List<Parameter>, bool>, Func<string>>>();

        /// <summary>
        /// Conditions the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        public IConsoleHandlerBuilder Condition(Func<List<Parameter>, bool> condition, string message)
        {
            if (condition != null)
            {
                this.Conditions.Add(new Tuple<Func<List<Parameter>, bool>, Func<string>>(condition, () => message));
            }

            return this;
        }

        /// <summary>
        /// Matches the specified verb.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns></returns>
        public IConsoleHandlerBuilder Match(string verb, Func<List<Parameter>, bool> callBack)
        {
            if (verb != null)
            {
                this.Verb = verb;
                this.Handler = callBack;
            }

            return this;
        }

        /// <summary>
        /// Withes the parameters.
        /// </summary>
        /// <returns></returns>
        public IParameterBuilder WithParam()
        {
            var parameter = new ParameterBuilder(this);
            this.Parameters.Add(parameter);

            return parameter;
        }

        /// <summary>
        /// Descriptions the specified description.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        IConsoleHandlerBuilder IConsoleHandlerBuilder.Description(string description)
        {
            this.Description = description;
            return this;
        }

        /// <summary>
        /// Writes the help.
        /// </summary>
        /// <returns></returns>
        public void WriteHelp()
        {
            const string childSpace = "     ";
            const string groupChar = "|";

            if (!string.IsNullOrWhiteSpace(this.Description))
            {
                // Write handler summary
                var handlerVerb = $"{groupChar} -{this.Verb}";
                Console.Write(handlerVerb, Color.Yellow);
                var handlerDrpt = $" : {this.Description}";
                Console.WriteLine(handlerDrpt, Color.White);

                // Write pattern
                Console.Write($"{groupChar}", Color.Yellow);
                Console.Write($"{childSpace}Pattern: ", Color.Green);
                Console.Write($"-{this.Verb}", Color.Yellow);

                if (this.Parameters.Any())
                {
                    foreach (var param in this.Parameters)
                    {
                        Console.Write($" [{param.Name}]", Color.White);
                    }
                }
                Console.WriteLine();

                // Write parameters

                foreach (var param in this.Parameters)
                {
                    var description = param.Description;
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        description = "N/A";
                    }

                    Console.Write($"{groupChar}", Color.Yellow);
                    Console.Write($"{childSpace}[{param.Name}]", Color.Green);
                    Console.WriteLine($" : {description}", Color.White);
                }

                if (!string.IsNullOrWhiteSpace(this.HandlerExample))
                {
                    Console.Write($"{groupChar}", Color.Yellow);
                    Console.Write($"{childSpace}Ex: ", Color.Green);
                    Console.WriteLine(this.HandlerExample, Color.Gray);
                }
            }
        }

        /// <summary>
        /// Examples the specified example.
        /// </summary>
        /// <param name="example">The example.</param>
        /// <returns></returns>
        public IConsoleHandlerBuilder Example(string example)
        {
            this.HandlerExample = example;
            return this;
        }
    }

    public interface IParameterBuilder
    {
        /// <summary>
        /// Matches the specified parameter name.
        /// </summary>
        /// <param name="paramName">Name of the parameter.</param>
        /// <returns></returns>
        IParameterBuilder Match(string paramName);

        /// <summary>
        /// Descriptions the specified description.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        IParameterBuilder Description(string description);

        /// <summary>
        /// Bases this instance.
        /// </summary>
        /// <returns></returns>
        IConsoleHandlerBuilder Base();

        /// <summary>
        /// Writes the help.
        /// </summary>
        /// <returns></returns>
        string GetHelp();
    }

    public interface IConsoleHandlerBuilder
    {
        /// <summary>
        /// Matches the specified verb.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns></returns>
        IConsoleHandlerBuilder Match(string verb, Func<List<Parameter>, bool> callBack);

        /// <summary>
        /// Descriptions the specified description.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        IConsoleHandlerBuilder Description(string description);

        /// <summary>
        /// Examples the specified description.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        IConsoleHandlerBuilder Example(string description);

        /// <summary>
        /// Conditions the specified condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        IConsoleHandlerBuilder Condition(Func<List<Parameter>, bool> condition, string message);

        /// <summary>
        /// Withes the parameters.
        /// </summary>
        /// <returns></returns>
        IParameterBuilder WithParam();

        /// <summary>
        /// Writes the help.
        /// </summary>
        void WriteHelp();
    }


    public class ConsoleArgManager
    {
        /// <summary>
        /// The instance
        /// </summary>
        public static ConsoleArgManager Instance;

        /// <summary>
        /// Initializes the <see cref="ConsoleArgManager"/> class.
        /// </summary>
        static ConsoleArgManager()
        {
            Instance = new ConsoleArgManager();
        }

        /// <summary>
        /// The handlers
        /// </summary>
        private List<ConsoleHandlerBuilder> handlers = new List<ConsoleHandlerBuilder>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleArgManager"/> class.
        /// </summary>
        public ConsoleArgManager()
        {
            this.GetHandlerBuilder()
                .Match("h", (args) => { this.Help(); return true; })
                .Description("Show help.");
        }

        /// <summary>
        /// Gets the handler builder.
        /// </summary>
        /// <returns></returns>
        public IConsoleHandlerBuilder GetHandlerBuilder()
        {
            var handlerBuilder = new ConsoleHandlerBuilder();
            this.handlers.Add(handlerBuilder);
            return handlerBuilder;
        }

        /// <summary>
        /// Verifies this instance.
        /// </summary>
        /// <returns></returns>
        public ConsoleArgManager Build(string[] args)
        {
            return this;
        }

        /// <summary>
        /// Does this instance.
        /// </summary>
        /// <returns></returns>
        public void Do(string[] args)
        {
            if (args.Length > 0)
            {
                var verb = args[0].Trim('-');
                foreach (var handler in this.handlers)
                {
                    if (string.Equals(handler.Verb, verb))
                    {
                        ParseParams(handler.Parameters, args);
                        foreach (var condition in handler.Conditions)
                        {
                            if (!condition.Item1.Invoke(handler.Parameters))
                            {
                                Console.WriteLine(condition.Item2(), Color.Red);
                                handler.WriteHelp();
                                return;
                            }
                        }

                        handler.Handler.Invoke(handler.Parameters);
                        return;
                    }
                }
            }

            this.Help();
        }

        /// <summary>
        /// Helps this instance.
        /// </summary>
        private void Help()
        {
            Wellcome();
            Console.WriteLine("HELP ----------------------------------------------------", Color.Yellow);
            foreach (var handler in this.handlers)
            {
                handler.WriteHelp();
            }
            Console.Write($"|__", Color.Yellow);
        }

        /// <summary>
        /// Wellcomes this instance.
        /// </summary>
        private static void Wellcome()
        {
            Console.WriteLine("Wellcome to ............................................/>", Color.Yellow);
            Console.WriteAscii("Console Log Capture", Color.FromArgb(244, 212, 255));
            Console.WriteLine();
        }

        /// <summary>
        /// Parses the parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="args">The arguments.</param>
        private void ParseParams(List<Parameter> parameters, string[] args)
        {
            var parametersIn = args.ToList();
            parametersIn.RemoveAt(0);
            for (int i = 0; i < parametersIn.Count; i++)
            {
                ParseParam(parameters[i], parametersIn[i]);
            }
        }

        /// <summary>
        /// Parses the parameter.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        private void ParseParam(Parameter param, string arg)
        {
            param.Value = arg;
        }
    }
}
