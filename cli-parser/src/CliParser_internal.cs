using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CliParser;

public sealed partial class CliParser
{
    private const string LONG_HELP_OPTION = "--help";
    private const string SHORT_HELP_OPTION = "-H";
    private const string LONG_VERSION_OPTION = "--version";
    private const string SHORT_VERSION_OPTION = "-V";

    private ParseResult Parsing(IReadOnlyList<string> args)
    {
        if ((args.Count == 0 && ShowHelpOnEmptyArguments) ||
            (args.Count == 1 && (args[0] == LONG_HELP_OPTION || args[0] == SHORT_HELP_OPTION)))
            return new HelpResult(GetHelpString());
        if (args.Count == 1 && (args[0] == SHORT_VERSION_OPTION || args[0] == LONG_VERSION_OPTION))
            return new VersionResult(GetHelpString(), Version);
        return OptionParsing(args);
    }


    private string GetHelpString()
    {
        var sb = new StringBuilder();
        foreach (var option in _options.Values)
        {
            var isfirst = true;
            foreach (var shortAlias in option.ShortAliases)
            {
                if (!isfirst) sb.Append(", ");
                else isfirst = false;
                sb.Append("-" + shortAlias);
            }

            foreach (var longAlias in option.LongAliases)
            {
                if (!isfirst) sb.Append(", ");
                else isfirst = false;
                sb.Append("-" + longAlias);
            }

            sb.Append("\n");
            sb.Append(option.Description);
            sb.Append("\n");
        }


        return sb.ToString();
    }


    private bool ArgIsAlias(string arg, out Option? option)
    {
        option = null;
        Debug.Assert(arg != null);
        Debug.Assert(arg.Length > 0);

        if (arg[0] == '-') //arg strats with "-"
        {
            if (arg.Length > 1) //arg is not singular "-"
            {
                if (arg[1] == '-') //arg starts with "--"
                {
                    var longBody = arg[2..];
                    if (_longAliases.ContainsKey(longBody)) //long alias found
                    {
                        option = _longAliases[longBody];
                        return true;
                    }
                    else
                    {
                        return false; //not long alias
                    }
                }
                else //arg starts with "-" followed by some characters
                {
                    var shortBody = arg[1..];
                    if (shortBody.Length > 1) return false; //arg stars with "-" but is not a short alias

                    var shortAlias = shortBody[shortBody.Length - 1];
                    if (_shortAliases.ContainsKey(shortAlias)) //short alias found
                    {
                        option = _shortAliases[shortAlias];
                        return true;
                    }
                    else
                    {
                        return false; // not short alias
                    }
                }
            }
            else
            {
                return false; //arg is just "-"
            }
        }
        else
        {
            return false; //arg is not an optionCallbackPair alias
        }
    }

    private ParseData OptionParsing(IReadOnlyList<string> args)
    {
        var usedOptions = new Dictionary<string, Option>();
        var optionResults = new Dictionary<string, OptionResult>();
        var plainArgs = new List<string>();
        for (var i = 0; i < args.Count; i++) // for each argument
        {
            var arg = args[i];

            if (ArgIsAlias(arg, out var option)) //is arg alias?
            {
                if (usedOptions.ContainsKey(option!.Name)) //is associated option already set?
                    throw new ConflictingOptionsSetException(usedOptions[option.Name], option);
                else
                    usedOptions.Add(option.Name, option);

                optionResults.Add(option.Name, ReadForOption(option, args, ref i)); //read parameters for the option
            }
            else if (arg == PlainSeparator) // is arg plain separator?
            {
                var newPlain = ReadPlains(args, ref i);
                plainArgs.AddRange(newPlain);
            }
            else // undefined
            {
                throw new ParserException($"Unknown token: {arg}");
            }
        }

        var optionNames = usedOptions.Keys.ToHashSet<string>();
        CheckConflicts(optionNames);
        CheckDependencies(optionNames);
        CheckRequiredOptions(optionNames);
        GetUnsetOptionResults(optionResults, _options.Keys.Except(optionNames));
        return new ParseData(optionResults, GetHelpString(), plainArgs);
    }


    private void GetUnsetOptionResults(Dictionary<string, OptionResult> destination,
        IEnumerable<string> unsetOptionNames)
    {
        foreach (var optionName in unsetOptionNames)
            destination.Add(optionName, _options[optionName].GetOptionParser().GetResult());
    }


    private OptionResult ReadForOption(Option option, IReadOnlyList<string> args, ref int argIndex)
    {
        var optionParser = option.GetOptionParser();
        optionParser.IsSet = true;
        ++argIndex; //shift to another token
        var firstParamIndex = argIndex;
        while
            (argIndex < args.Count &&
             optionParser
                 .CanParse()) //while there are available arguments and option has parameters which can be parsed
        {
            if (optionParser.TryParse(args[argIndex])) // if arg can be parsed as a parameter on the given index
            {
                if (!optionParser.LastParamIsValid())
                    throw new ParserException(
                        $"Parameter for {option.Name} on index {argIndex - firstParamIndex} is not valid.");
            }
            else //if arg is not a parameter
            {
                if (optionParser.AllRequiredParamsParsed()) // are all required parameters parsed?
                {
                    argIndex--; // index of args is shifted back so that the current arg can be read again (it may be an option alias).
                    // will NOT cause infinite loop, since argIndex was incremented at the begining of the method
                    return optionParser.GetResult();
                }
                else // not all required parameters were parsed
                {
                    throw new ParserException($"Not all required parameters were set for option {option.Name}.");
                }
            }

            argIndex++;
        }

        argIndex--;
        return optionParser.GetResult();
    }

    private IReadOnlyList<string> ReadPlains(IReadOnlyList<string> args, ref int argIndex)
    {
        var plainArgs = new List<string>();
        argIndex++; //shift to next token
        while (argIndex < args.Count)
        {
            plainArgs.Add(args[argIndex]);
            argIndex++;
            if (!AllPlainAfterFirst)
            {
                argIndex--;
                break;
            }
        }

        return plainArgs;
    }

    private void CheckConflicts(IReadOnlySet<string> optionNames)
    {
        foreach (var conflictSet in _conflicts)
            if (optionNames.Count(item => conflictSet.Contains(item)) > 1)
                throw new ConflictingOptionsSetException("Conflicting options were set.");
    }

    private void CheckDependencies(IReadOnlySet<string> optionNames)
    {
        foreach (var (dependent, dependencies) in _dependencies)
        {
            if (!optionNames.Contains(dependent)) continue;

            foreach (var dependency in dependencies)
                if (!optionNames.Contains(dependency))
                    throw new ParserException($"Missing dependency {dependency} required for {dependent}.");
        }
    }

    private void CheckRequiredOptions(IReadOnlySet<string> optionNames)
    {
        foreach (var optionCallbackPair in _requiredOptionsAndCallback)
            if (!optionNames.Contains(optionCallbackPair.Key))
            {
                if (optionCallbackPair.Value != null)
                    optionCallbackPair.Value();
                else
                    throw new RequiredOptionMissingException(optionCallbackPair.Key);
            }
    }
}