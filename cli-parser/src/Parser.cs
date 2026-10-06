using System.Text;

namespace CliParser;

public sealed partial class Parser {
    private Dictionary<string, Option> _options = new();
    private List<HashSet<string>> _conflicts = new();
    private Dictionary<string, HashSet<string>> _dependencies = new();

    private Dictionary<char, Option> _shortAliases = new();
    private Dictionary<string, Option> _longAliases = new();

    public string Version { get; set; } = "1.0.0";
    public string? AppName { get; set; } = null;
    public bool ErrorReturnsParseInfo { get;  set; } = true;
    public bool AllPlainAfterFirst { get;  set; } = true;
    public bool ShowHelpOnEmptyArguments { get; set; } = false;
    
    public static char VersionShortAlias { get; set; } = 'V';
    public static char HelpShortAlias { get; set; } = 'H';
    public static string VersionLongAlias { get; set; } = "version";
    public static string HelpLongAlias { get; set; } = "help";
    public static string PlainSeparator { get; set; } = "--";

    public void AddOption(Option option) {
        if (_options.ContainsKey(option.Name)) {
            throw new NotUniqueOptionNameException(option);
        }

        foreach (var alias in option.ShortAliases) {
            if (_shortAliases.TryGetValue(alias, out var other)) {
                throw new ConflictingOptionAliasesException(option, other, alias.ToString());
            }
                
        }

        foreach (var alias in option.LongAliases) {
            if (_longAliases.TryGetValue(alias, out var other)) {
                throw new ConflictingOptionAliasesException(option, other, alias.ToString());
            }
        }

        foreach (var alias in option.ShortAliases) {
            _shortAliases.Add(alias, option);
        }
        foreach (var alias in option.LongAliases) {
            _longAliases.Add(alias, option);
        }
        _options.Add(option.Name, option);
    }
    
    public void AddConflict(string a, string b, params string[] names) {
        HashSet<string> conflict = [
            ..names,
            a,
            b
        ];

        foreach (var s in conflict) {
            if (!_options.ContainsKey(s)) {
                throw new OptionNotFoundException(s);
            }
        }
            
        _conflicts.Add(conflict);
    }

    public void AddConflict(Option a, Option b, params Option[] options) {
        AddConflict(a.Name, b.Name, options.Select(o => o.Name).ToArray());
    }

    public void AddDependency(string dependentName, string dependencyName) {
        if (!_options.ContainsKey(dependentName)) {
            throw new OptionNotFoundException(dependentName);
        }

        if (!_options.ContainsKey(dependencyName)) {
            throw new OptionNotFoundException(dependencyName);
        }

        if (_dependencies.ContainsKey(dependentName)) {
            _dependencies[dependentName].Add(dependencyName);
        }
        else {
            var newDependencies = new HashSet<string>();
            newDependencies.Add(dependencyName);
            _dependencies[dependentName] = newDependencies;
        }
    }

    public void AddDependency(Option dependent, Option dependency) {
        AddDependency(dependent.Name, dependency.Name);
    }

    public ParseResult Parse(string args) {
        return Parse(args.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries));
    }

    public string GenerateVersionString() {
        return (AppName is not null ? AppName + " " : "") + Version;
    }

    public string GenerateHelpString() {
        StringBuilder sb = new();
        sb.AppendLine(AppName is not null ? AppName + " options:" : "Options:");
        
        foreach (var (key, value) in _options) {
            sb.AppendLine("  " + key + (value.Required ? " (required):" : ":"));
            if (value.Description is not null) {
                sb.AppendLine("    Description: " + value.Description);
            }
            
            sb.Append("    Aliases:    ");
            foreach (var alias in value.ShortAliases) {
                sb.Append(" -" + alias);
            }
            foreach (var alias in value.LongAliases) {
                sb.Append(" --" + alias);
            }
            sb.AppendLine();

            if (value.Parameters.Count > 0) {
                sb.Append("    Parameters: ");
                foreach (var parameter in value.Parameters) {
                    sb.Append(" " + parameter.Name());
                }
                sb.AppendLine();
            }

            sb.Append("    Dependencies: ");
            if (_dependencies.TryGetValue(key, out var dependencies)) {
                foreach (var dependency in dependencies) {
                    sb.Append(" " + dependency);
                }
                sb.AppendLine();
            }
            sb.AppendLine();
        }

        sb.Append("\nConflicting option groups: ");
        foreach (var conflicts in _conflicts) {
            sb.Append("\n  ");
            foreach (var conflict in conflicts) {
                sb.Append(conflict + " ");
            }
        }
        sb.AppendLine();

        return sb.ToString();
    }
}