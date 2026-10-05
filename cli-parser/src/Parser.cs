namespace CliParser;

public sealed partial class Parser {
    private Dictionary<string, Option> _options = new();
    private List<HashSet<string>> _conflicts = new();
    private Dictionary<string, HashSet<string>> _dependencies = new();

    private Dictionary<char, Option> _shortAliases = new();
    private Dictionary<string, Option> _longAliases = new();

    public string Version { get; set; } = "1.0.0";
    public bool AllPlainAfterFirst { get;  set; } = true;

    public bool ShowHelpOnEmptyArguments { get; set; } = false;

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

    public ParseResult Parse(IReadOnlyList<string> args) {
        throw new NotImplementedException();
    }

    public ParseResult Parse(string args) {
        return Parse(args.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries));
    }
}