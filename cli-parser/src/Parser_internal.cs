using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CliParser;

enum ParsingState {
    Default,
    OptionParsing,
    AlwaysPlain
}

public sealed partial class Parser {
    // private Dictionary<string, Option> _options = new();
    // private Dictionary<char, Option> _shortAliases = new();
    // private Dictionary<string, Option> _longAliases = new();

    // public bool AllPlainAfterFirst { get;  set; } = true;

    private ParsedData _parsedData;
    private ParsingState _state = ParsingState.Default;
    private OptionResult _parsedOption;
    
    public ParseResult Parse(IEnumerable<string> args) {
        var enumerable = args as string[] ?? args.ToArray();
        
        if (!enumerable.Any() && ShowHelpOnEmptyArguments) {
            return new ParserInfo(GenerateHelpString(), GenerateVersionString());
        }

        if (enumerable.Any(x =>
                x == "-" + VersionShortAlias.ToString()
                || x == "-" + HelpShortAlias.ToString()
                || x == "--" + VersionLongAlias
                || x == "--" + HelpLongAlias)
            ) {
            return new ParserInfo(GenerateHelpString(), GenerateVersionString());
        }

        try {
            _parsedData = new ParsedData(_options, GenerateHelpString());
            StartParsing(enumerable);
            _parsedData.Validate(_conflicts, _dependencies);
            return _parsedData;
        }
        catch (IndexOutOfRangeException) {
            if (ErrorReturnsParseInfo) {
                return new ParserInfo(GenerateHelpString(), GenerateVersionString());
            }
            throw;
        }
    }
    
    private void StartParsing(IEnumerable<string> args) {
        foreach (var arg in args) {
            switch (_state) {
                case ParsingState.Default:
                    if (arg == PlainSeparator) {
                        _state = ParsingState.AlwaysPlain;
                        continue;
                    }

                    if (arg.StartsWith("--")) {
                        string alias = arg.Substring(2);
                        _parsedOption = _parsedData.StartOptionParsing(_longAliases[alias].Name);
                        if (_parsedOption._option.Parameters.Count != 0) {
                            _state = ParsingState.OptionParsing;
                        }
                        continue;
                    }

                    if (arg.StartsWith("-") && arg.Length == 2) {
                        char alias = arg[1];
                        _parsedOption = _parsedData.StartOptionParsing(_shortAliases[alias].Name);
                        if (_parsedOption._option.Parameters.Count != 0) {
                            _state = ParsingState.OptionParsing;
                        }
                        continue;
                    }
                    
                    // plain
                    _parsedData.AddPlain(arg);
                    if (AllPlainAfterFirst) {
                        _state = ParsingState.AlwaysPlain;
                    }
                    break;
                
                case ParsingState.OptionParsing:
                    if (_parsedOption.AddParameter(arg)) {
                        _state = ParsingState.Default;
                    }
                    break;
                
                case ParsingState.AlwaysPlain:
                    _parsedData.AddPlain(arg);
                    break;
            }
        }
    }
}