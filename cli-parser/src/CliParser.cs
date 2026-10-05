namespace CliParser;

/// <summary>
/// Class <c>CliParser</c> is used to verify and extract information from CLI.
/// </summary>
public sealed partial class CliParser
{
    private Dictionary<string, Option> _options = new();
    private Dictionary<string, Action?> _requiredOptionsAndCallback = new();
    private List<HashSet<string>> _conflicts = new();
    private Dictionary<string, HashSet<string>> _dependencies = new();

    private Dictionary<char, Option> _shortAliases = new();
    private Dictionary<string, Option> _longAliases = new();


    /// <summary>
    /// Sets (or gets) the version of the CLI parser.
    /// </summary>
    public string Version { get; set; } = "v 1.0.0";

    /// <summary>
    /// Sets (or gets) the plain separator string used by the parser to distinguish between options and plain-style arguments.
    /// </summary>
    public string PlainSeparator { get; set; } = "--";

    /// <summary>
    /// Gets or sets a value indicating whether all arguments after the first plain argument should also be treated as plain arguments.
    /// </summary>
    [Obsolete]
    public bool AllPlainAfterFirst
    {
        get => true;
        set {} 
    }

    /// <summary>
    /// Determines whether help information is displayed when no arguments are provided to the CLI parser.
    /// </summary
    public bool ShowHelpOnEmptyArguments { get; set; } = false;

    /// <summary>
    /// Allows to add an optional option to the parser. User is not required to set this option.
    /// </summary>
    /// <param name="option">Option object describing the optional option.</param>
    /// <exception cref="NotUniqueOptionNameException">Thrown when at least two options in the same parser have the same name.</exception>
    /// <exception cref="ConflictingOptionAliasesException">Thrown if some alias of added option is the same as any alias from of any other option.</exception>
    public void AddOptionalOption(Option option)
    {
        if (_options.ContainsKey(option.Name)) throw new NotUniqueOptionNameException(option);
        foreach (var alias in option.ShortAliases)
            if (_shortAliases.TryGetValue(alias, out var other))
                throw new ConflictingOptionAliasesException(option, other, alias.ToString());

        foreach (var alias in option.LongAliases)
            if (_longAliases.TryGetValue(alias, out var other))
                throw new ConflictingOptionAliasesException(option, other, alias.ToString());


        foreach (var alias in option.ShortAliases) _shortAliases.Add(alias, option);
        foreach (var alias in option.LongAliases) _longAliases.Add(alias, option);
        _options.Add(option.Name, option);

        /*
        foreach (var (_, addedOption) in _options) {
            if (addedOption.Name == option.Name) throw new NotUniqueOptionNameException(option);
            foreach (var alias in option.ShortAliases) {
                if (addedOption.ShortAliases.Contains(alias)) throw new ConflictingOptionAliasesException(option, addedOption, alias.ToString());
            }
            foreach (var alias in option.LongAliases) {
                if (addedOption.LongAliases.Contains(alias)) throw new ConflictingOptionAliasesException(option, addedOption, alias);
            }
        }
        _options.Add(option.Name, option);
        _allOptions.Add(option.Name);
        */
    }

    /// <summary>
    /// Allows to add a required option to the parser. User must set this option, if they did not already set a conflicting option.
    /// </summary>
    /// <param name="option">Option object describing the required option.</param>
    /// <param name="onMissing">Function to be called if the user does not set the option.</param>
    /// <exception cref="NotUniqueOptionNameException">Thrown when at least two options in the same parser have the same name.</exception>
    /// <exception cref="ConflictingOptionAliasesException">Thrown if some alias of added option is the same as any alias from of any other option.</exception>
    public void AddRequiredOption(Option option, Action? onMissing = null)
    {
        AddOptionalOption(option);
        _requiredOptionsAndCallback.Add(option.Name, onMissing);
    }

    /// <summary>
    /// Specify which options are in conflict with each other. Only one of them can be present. Options are specified using their names. See also <see cref="AddConflict(Option[])"/>.
    /// </summary>
    /// <param name="names">Names of conflicting options</param>
    /// <exception cref="OptionNotFoundException">Thrown if any name passed as a parameter does not match any option in the current <see cref="CliParser"/> instance.</exception>>
    public void AddConflict(string a, string b, params string[] names)
    {
        HashSet<string> conflict = new(names);
        conflict.Add(a);
        conflict.Add(b);

        foreach (var s in conflict)
            if (!_options.ContainsKey(s))
                throw new OptionNotFoundException(s);
        _conflicts.Add(conflict);
    }

    /// <summary>
    /// Specify which options are in conflict with each other. Only one of them can be present. See also <see cref="AddConflict(string[])"/>.
    /// </summary>
    /// <param name="options">Option objects representing conflictiong options.</param>
    /// <exception cref="OptionNotFoundException">Thrown if any name passed as a parameter does not match any option in the current <see cref="CliParser"/> instance.</exception>>
    public void AddConflict(Option a, Option b, params Option[] options)
    {
        AddConflict(a.Name, b.Name, options.Select(o => o.Name).ToArray());
    }

    /// <summary>
    /// Makes one option dependent on another option. 
    /// </summary>
    /// <param name="dependentName">Name of the dependent.</param>
    /// <param name="dependencyName">Name of the dependency.</param>
    /// <exception cref="OptionNotFoundException">If dependentName or dependencyName do not correspond to any option specified in the parser instance.</exception>
    public void AddDependency(string dependentName, string dependencyName)
    {
        if (!_options.ContainsKey(dependentName)) throw new OptionNotFoundException(dependentName);
        if (!_options.ContainsKey(dependencyName)) throw new OptionNotFoundException(dependencyName);

        if (_dependencies.ContainsKey(dependentName))
        {
            _dependencies[dependentName].Add(dependencyName);
        }
        else
        {
            var newDependencies = new HashSet<string>();
            newDependencies.Add(dependencyName);
            _dependencies[dependentName] = newDependencies;
        }

        //_dependencies.GetValueOrDefault(dependentName, new HashSet<string>()).Add(dependencyName);
    }

    /// <summary>
    /// Makes one option dependent on another option. 
    /// </summary>
    /// <param name="dependent">Dependent option.</param>
    /// <param name="dependencyName">Name of the dependency</param>
    /// <exception cref="OptionNotFoundException">If dependentName or dependencyName do not correspond to any option specified in the parser instance.</exception>
    public void AddDependency(Option dependent, string dependencyName)
    {
        AddDependency(dependent.Name, dependencyName);
    }

    /// <summary>
    /// Makes one option dependent on another option. 
    /// </summary>
    /// <param name="dependentName">Name of the dependent</param>
    /// <param name="dependency">Dependency instance.</param>
    /// <exception cref="OptionNotFoundException">If dependentName or dependencyName do not correspond to any option specified in the parser instance.</exception>
    public void AddDependency(string dependentName, Option dependency)
    {
        AddDependency(dependentName, dependency.Name);
    }

    /// <summary>
    /// Makes one option dependent on another option.
    /// </summary>
    /// <param name="dependent">Dependent option.</param>
    /// <param name="dependency">Dependency instance.</param>
    /// <exception cref="OptionNotFoundException">If dependentName or dependencyName do not correspond to any option specified in the parser instance.</exception>
    public void AddDependency(Option dependent, Option dependency)
    {
        AddDependency(dependent.Name, dependency.Name);
    }

    /// <summary>
    /// Parses <c>IReadOnlyList</c> of strings (list of tokens) using specified information. See also <see cref="Parse(string)"/>
    /// </summary>
    /// <param name="args">List of tokens to be parsed.</param>
    /// <returns>IParseResult instance containing the result of parsing.</returns>
    /// <exception cref="RequiredOptionMissingException">Thrown when for some option onMissingAction (specified in <see cref="AddRequiredOption(Option, Action?)"/>) is null and the user does not set the required option.</exception>
    /// <exception cref="ConflictingOptionsSetException">Thrown when user sets two conflictiong options.</exception>
    public ParseResult Parse(IReadOnlyList<string> args)
    {
        return Parsing(args);
    }

    /// <summary>
    /// Parses a string using specified information.
    /// </summary>
    /// <param name="args">String to be parsed.</param>
    /// <returns>IParseResult instance containing the result of parsing.</returns>
    /// <exception cref="RequiredOptionMissingException">Thrown when for some option onMissingAction (specified in <see cref="AddRequiredOption(Option, Action?)"/>) is null and the user does not set the required option.</exception>
    /// <exception cref="ConflictingOptionsSetException">Thrown when user sets two conflicting options.</exception>
    public ParseResult Parse(string args)
    {
        return Parse(args.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
    }
}