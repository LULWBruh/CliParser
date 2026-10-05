namespace CliParser;

public sealed partial class Option {
    public string Name { get; private init; }

    public static OptionBuilder Builder(string name) {
        return new OptionBuilder(name);
    }
}

public class OptionBuilder {
    private List<char> _shortAliases = new();
    private List<string> _longAliases = new();
    private string Name { get; init; }
    private bool _required = false;

    internal OptionBuilder(string name) {
        Name = name;
    }

    public OptionBuilder Required() {
        _required = true;
        return this;
    }

    public RequiredAllowedBuilder AddShortAlias(char alias) {
        if (alias == Parser.SHORT_VERSION_OPTION || alias == Parser.SHORT_HELP_OPTION || !char.IsLetter(alias) || !char.IsAscii(alias)) {
            throw new BadAliasException(alias.ToString());
        }

        if (_shortAliases.Contains(alias)) {
            throw new ConflictingOptionAliasesException($"Alias {alias} already exists in {Name}");
        }
        
        _shortAliases.Add(alias);
        return new RequiredAllowedBuilder(_shortAliases, _longAliases, Name, _required);
    }

    public RequiredAllowedBuilder AddLongAlias(string alias) {
        if (alias == "version" || alias == "help" || alias == "" || !char.IsLetter(alias[0]))
            throw new BadAliasException(alias);
        foreach (var ch in alias) {
            if ((!char.IsLetter(ch) && !char.IsNumber(ch)) || !char.IsAscii(ch)) {
                throw new BadAliasException(alias);
            }
        }

        if (_longAliases.Contains(alias)) {
            throw new ConflictingOptionAliasesException($"Alias {alias} already exists in {Name}");
        }

        _longAliases.Add(alias);
        return new RequiredAllowedBuilder(_shortAliases, _longAliases, Name, _required);
    }
}

public class RequiredAllowedBuilder : OptionalOnlyBuilder {
    internal RequiredAllowedBuilder(List<char> shortAliases, List<string> longAliases, string name, bool required)
        : base(shortAliases, longAliases, name, required) { }

    public RequiredAllowedBuilder AddRequiredParameter(Parameter parameter) {
        _parameters.Add(parameter);
        _requiredParametersCount++;
        return this;
    }

    public override RequiredAllowedBuilder AddShortAlias(char alias) {
        base.AddShortAlias(alias);
        return this;
    }

    public override RequiredAllowedBuilder AddLongAlias(string alias) {
        base.AddLongAlias(alias);
        return this;
    }

    public override RequiredAllowedBuilder AddDescription(string description) {
        base.AddDescription(description);
        return this;
    }
}

public class OptionalOnlyBuilder {
    internal List<Parameter> _parameters = new();
    internal int _requiredParametersCount = 0;
    internal bool _required { get; init; }

    protected List<char> _shortAliases { get; init; }
    protected List<string> _longAliases { get; init; }
    protected string? _description = null;
    protected string _name { get; init; }

    public OptionalOnlyBuilder(List<char> shortAliases, List<string> longAliases, string name, bool required) {
        _shortAliases = shortAliases;
        _longAliases = longAliases;
        _name = name;
        _required = required;
    }
    
    public OptionalOnlyBuilder AddOptionalParameter(Parameter parameter) {
        _parameters.Add(parameter);
        return this;
    }

    public virtual OptionalOnlyBuilder AddShortAlias(char alias) {
        if (alias == 'V' || alias == 'H' || !char.IsLetter(alias) || !char.IsAscii(alias))
            throw new BadAliasException(alias.ToString());
        _shortAliases.Add(alias);
        return this;
    }

    public virtual OptionalOnlyBuilder AddLongAlias(string alias) {
        if (alias == "version" || alias == "help" || alias == "" || !char.IsLetter(alias[0]))
            throw new BadAliasException(alias);
        foreach (var ch in alias)
            if ((!char.IsLetter(ch) && !char.IsNumber(ch)) || !char.IsAscii(ch))
                throw new BadAliasException(alias);

        _longAliases.Add(alias);
        return this;
    }

    public virtual OptionalOnlyBuilder AddDescription(string description) {
        _description = description;
        return this;
    }
    
    public Option Build() {
        return new Option(_parameters, _requiredParametersCount, _shortAliases, _longAliases, _description, _name, _required);
    }

    public static implicit operator Option(OptionalOnlyBuilder builder) {
        return builder.Build();
    }
}