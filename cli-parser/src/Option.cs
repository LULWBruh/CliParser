namespace CliParser;

public sealed partial class Option {
    public string Name { get; private init; }

    public static OptionBuilder Builder(string name) {
        return new OptionBuilder(name);
    }
}

public sealed class OptionBuilder {
    private List<char> _shortAliases = new();
    private List<string> _longAliases = new();
    private List<Parameter> _parameters = new();
    private string? _description = null;
    private string Name { get; init; }
    private bool _required = false;

    internal OptionBuilder(string name) {
        Name = name;
    }

    public OptionBuilder Required() {
        _required = true;
        return this;
    }

    public OptionBuilder AddShortAlias(char alias) {
        if (alias == Parser.VersionShortAlias || alias == Parser.HelpShortAlias || !char.IsLetter(alias) || !char.IsAscii(alias)) {
            throw new BadAliasException(alias.ToString());
        }

        if (_shortAliases.Contains(alias)) {
            throw new ConflictingOptionAliasesException($"Alias {alias} already exists in {Name}");
        }
        
        _shortAliases.Add(alias);
        return this;
    }

    public OptionBuilder AddLongAlias(string alias) {
        if (alias == Parser.VersionLongAlias || alias == Parser.HelpLongAlias || alias == "" || !char.IsLetter(alias[0]))
            throw new BadAliasException(alias);
        foreach (var ch in alias) {
            // if ((!char.IsLetter(ch) && !char.IsNumber(ch)) || !char.IsAscii(ch)) {
            if (!char.IsLetterOrDigit(ch) && ch != '-') { 
                throw new BadAliasException(alias);
            }
        }

        if (_longAliases.Contains(alias)) {
            throw new ConflictingOptionAliasesException($"Alias {alias} already exists in {Name}");
        }

        _longAliases.Add(alias);
        return this;
    }

    public OptionBuilder AddParameter(Parameter parameter) {
        _parameters.Add(parameter);
        return this;
    }

    public OptionBuilder AddDescription(string description) {
        _description = description;
        return this;
    }
    
    public Option Build() {
        return new Option(_parameters, _shortAliases, _longAliases, _description, Name, _required);
    }

    public static implicit operator Option(OptionBuilder builder) {
        return builder.Build();
    }
}