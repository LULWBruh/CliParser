using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CliParser;

public sealed partial class Parser {
    // private Dictionary<string, Option> _options = new();
    // private Dictionary<char, Option> _shortAliases = new();
    // private Dictionary<string, Option> _longAliases = new();
    
    // private Dictionary<string, HashSet<string>> _dependencies = new();
    // private List<HashSet<string>> _conflicts = new();

    // public bool ErrorReturnsParseInfo { get;  set; } = true;
    // public bool AllPlainAfterFirst { get;  set; } = true;
    // public bool ShowHelpOnEmptyArguments { get; set; } = false;
    
    // public static char VersionShortAlias { get; set; } = 'V';
    // public static char HelpShortAlias { get; set; } = 'H';
    // public static string VersionLongAlias { get; set; } = "version";
    // public static string HelpLongAlias { get; set; } = "help";
    // public static string PlainSeparator { get; set; } = "--";

    internal ParsedData _parsedData;
    
    public ParseResult Parse(IEnumerable<string> args) {
        
        _parsedData = new ParsedData(_options, GenerateHelpString());

        throw new NotImplementedException();
    }
}