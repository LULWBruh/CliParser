namespace CliParser;

/// <summary>
/// This class represents an option which can be added to the <see cref="Parser"/> instance.
/// </summary>
public sealed partial class Option {
    internal IReadOnlyList<Parameter> Parameters { get; private init; }
    internal int RequiredParametersCount { get; private init; }
    internal IReadOnlySet<char> ShortAliases { get; private init; }
    internal IReadOnlySet<string> LongAliases { get; private init; }
    internal string? Description { get; private init; }

    /// <summary>
    /// Name of the option. Two options in the same parser must NOT have the same Name.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    /// Creates a builder for the option object.
    /// </summary>
    /// <param name="name">Name of the option. Two options in the same parser must NOT have the same Name.</param>
    /// <returns>Returns <see cref="OptionBuilder"/> instance.</returns>
    /// <remarks>By returning <see cref="OptionBuilder"/> we ensure that the only method that can be called next is either <see cref="OptionBuilder.AddShortAlias(char)"/> or <see cref="OptionBuilder.AddLongAlias(string)"/>. This essentially makes specifying the alias compulsory.</remarks>
    public static OptionBuilder Builder(string name) {
        return new OptionBuilder(name);
    }
}

/// <summary>
/// A first builder class used to build <c>Option</c> object instance using fluent API.
/// </summary>
public class OptionBuilder {
    private List<char> _shortAliases = new();
    private List<string> _longAliases = new();
    private string _name { get; init; }

    public OptionBuilder(string name) {
        _name = name;
    }

    /// <summary>
    /// Allows the user to specify single character alias for the option.
    /// </summary>
    /// <param name="alias">Character for the alias.</param>
    /// <returns><see cref="RequiredAllowedBuilder"/> instance, which allows the user to continue in the building process. The instance stores the input alias.</returns>
    /// <example>When the programmer specifies alias to i.e. 'a', then the user can reference the option using "-a".</example>
    /// <remarks>First stage in building the <see cref="Option"/> instance. Ensures that at least one alias is specified for the option.</remarks>
    /// <exception  cref="BadAliasException">Thrown when invalid alias is given. Invalid aliases: 'V', 'H'.</exception>
    public RequiredAllowedBuilder AddShortAlias(char alias) {
        if (alias == 'V' || alias == 'H' || !char.IsLetter(alias) || !char.IsAscii(alias))
            throw new BadAliasException(alias.ToString());
        _shortAliases.Add(alias);
        return new RequiredAllowedBuilder(_shortAliases, _longAliases, _name);
    }

    /// <summary>
    /// Allows the user to specify multicharacter alias for the option.
    /// </summary>
    /// <param name="alias">String for the alias.</param>
    /// <returns><see cref="RequiredAllowedBuilder"/> instance, which allows the user to continue in the building process. The instance stores the input alias.</returns>
    /// <example>When the programmer specifies alias to i.e. "something", then the user can reference the option using "--something".</example>
    /// <remarks>First stage in building the <see cref="Option"/> instance. Ensures that at least one alias is specified for the option.</remarks>
    /// <exception  cref="BadAliasException">Thrown when invalid alias is given. Invalid aliases: "version", "help".</exception>
    public RequiredAllowedBuilder AddLongAlias(string alias) {
        if (alias == "version" || alias == "help" || alias == "" || !char.IsLetter(alias[0]))
            throw new BadAliasException(alias);
        foreach (var ch in alias)
            if ((!char.IsLetter(ch) && !char.IsNumber(ch)) || !char.IsAscii(ch))
                throw new BadAliasException(alias);

        _longAliases.Add(alias);
        return new RequiredAllowedBuilder(_shortAliases, _longAliases, _name);
    }
}

/// <summary>
/// This class is used to build <see cref="Option"/> instance using fluent API. Instance of this class can be acquired by calling <see cref="OptionBuilder.AddShortAlias(char)"/> or <see cref="OptionBuilder.AddLongAlias(string)"/> method.<br/>
/// Allows the programmer to specify more aliases for the option, add descriptions, add required parameters, add optional parameters and finish the build process.
/// </summary>
/// <remarks>This class inherits form <see cref="OptionalOnlyBuilder"/>. This means that the programmer can call both <see cref="AddRequiredParameter(Parameter)"/> and <see cref="OptionalOnlyBuilder.AddOptionalParameter(Parameter)"/>.<br/>
/// The order in which these methods are called specifies the order of parameters for the option.<br/>
/// Since <see cref="AddRequiredParameter(Parameter)"/> returns <see cref="RequiredAllowedBuilder"/> and <see cref="OptionalOnlyBuilder.AddOptionalParameter(Parameter)"/> returns <see cref="OptionalOnlyBuilder"/> and since no method in <see cref="OptionalOnlyBuilder"/> returns <see cref="RequiredAllowedBuilder"/>, required parameters MUST be specified before optional parameters.
/// </remarks>
public class RequiredAllowedBuilder : OptionalOnlyBuilder {
    public RequiredAllowedBuilder(List<char> shortAliases, List<string> longAliases, string name)
        : base(shortAliases, longAliases, name) { }

    /// <summary>
    /// Add required parameter for the option.
    /// </summary>
    /// <param name="parameter"><see cref="Parameter"/> instance.</param>
    /// <returns><code>this</code></returns>
    public RequiredAllowedBuilder AddRequiredParameter(Parameter parameter) {
        _parameters.Add(parameter);
        _requiredParametersCount++;
        return this;
    }

    /// <summary>
    /// Allows the user to specify single character alias for the option.
    /// </summary>
    /// <param name="alias">Character for the alias.</param>
    /// <returns><code>this</code></returns>
    /// <example>When the programmer specifies alias to i.e. 'a', then the user can reference the option using "-a".</example>
    /// <remarks>Can be called multiple times to specify alternative aliases.</remarks>
    /// <exception cref="BadAliasException">Thrown when invalid alias is given. Invalid aliases: 'V'.</exception>
    public override RequiredAllowedBuilder AddShortAlias(char alias) {
        base.AddShortAlias(alias);
        return this;
    }

    /// <summary>
    /// Allows the user to specify multicharacter alias for the option.
    /// </summary>
    /// <param name="alias">String for the alias.</param>
    /// <returns><code>this</code></returns>
    /// <example>When the programmer specifies alias to i.e. "something", then the user can reference the option using "--something".</example>
    /// <remarks>Can be called multiple times to specify alternative aliases.</remarks>
    /// <exception  cref="BadAliasException">Thrown when invalid alias is given. Invalid aliases: "version", "help".</exception>
    public override RequiredAllowedBuilder AddLongAlias(string alias) {
        base.AddLongAlias(alias);
        return this;
    }

    /// <summary>
    /// Allows user to add description for the option, which will be shown (and formated) when "--help" is called. Multiple calls overwrite the description. Default descriptions is an empty string. 
    /// </summary>
    /// <param name="description">Description of the option.</param>
    /// <returns><code>this</code></returns>
    public override RequiredAllowedBuilder AddDescription(string description) {
        base.AddDescription(description);
        return this;
    }
}

/// <summary>
/// This class is used to build <see cref="Option"/> instance using fluent API. Instance of this class can be acquired by calling <see cref="RequiredAllowedBuilder.AddOptionalParameter(Parameter)"/> method. This class does not permit to add required parameters to the option (see <see cref="RequiredAllowedBuilder"/> for more info). <br/>
/// Allows the programmer to specify more aliases for the option, add descriptions, add optional parameters and finish the build process.
/// </summary>
public class OptionalOnlyBuilder {
    internal List<Parameter> _parameters = new();
    internal int _requiredParametersCount = 0;

    protected List<char> _shortAliases { get; init; }
    protected List<string> _longAliases { get; init; }
    protected string? _description = null;
    protected string _name { get; init; }

    public OptionalOnlyBuilder(List<char> shortAliases, List<string> longAliases, string name) {
        _shortAliases = shortAliases;
        _longAliases = longAliases;
        _name = name;
    }

    /// <summary>
    /// Add optional parameter.
    /// </summary>
    /// <param name="parameter"><see cref="Parameter"/> instance.</param>
    /// <returns><c>this</c>.</returns>
    public OptionalOnlyBuilder AddOptionalParameter(Parameter parameter) {
        _parameters.Add(parameter);
        return this;
    }

    /// <summary>
    /// Allows the user to specify single character alias for the option.
    /// </summary>
    /// <param name="alias">Character for the alias.</param>
    /// <returns><code>this</code></returns>
    /// <example>When the programmer specifies alias to i.e. 'a', then the user can reference the option using "-a".</example>
    /// <remarks>Can be called multiple times to specify alternative aliases.</remarks>
    /// <exception cref="BadAliasException">Thrown when invalid alias is given. Invalid aliases: 'V'.</exception>
    public virtual OptionalOnlyBuilder AddShortAlias(char alias) {
        if (alias == 'V' || alias == 'H' || !char.IsLetter(alias) || !char.IsAscii(alias))
            throw new BadAliasException(alias.ToString());
        _shortAliases.Add(alias);
        return this;
    }

    /// <summary>
    /// Allows the user to specify multicharacter alias for the option.
    /// </summary>
    /// <param name="alias">String for the alias.</param>
    /// <returns><code>this</code></returns>
    /// <example>When the programmer specifies alias to i.e. "something", then the user can reference the option using "--something".</example>
    /// <remarks>Can be called multiple times to specify alternative aliases.</remarks>
    /// <exception cref="BadAliasException">Thrown when invalid alias is given. Invalid aliases: "version", "help".</exception>
    public virtual OptionalOnlyBuilder AddLongAlias(string alias) {
        if (alias == "version" || alias == "help" || alias == "" || !char.IsLetter(alias[0]))
            throw new BadAliasException(alias);
        foreach (var ch in alias)
            if ((!char.IsLetter(ch) && !char.IsNumber(ch)) || !char.IsAscii(ch))
                throw new BadAliasException(alias);

        _longAliases.Add(alias);
        return this;
    }

    /// <summary>
    /// Allows user to add description for the option, which will be shown (and formated) when "--help" is called. Multiple calls overwrite the description. Default descriptions is an empty string. 
    /// </summary>
    /// <param name="description">Description of the option.</param>
    /// <returns><code>this</code></returns>
    public virtual OptionalOnlyBuilder AddDescription(string description) {
        _description = description;
        return this;
    }

    /// <summary>
    /// Finishes the option building process.
    /// </summary>
    /// <returns>Finished <see cref="Option"/> instance.</returns>
    public Option Build() {
        return new Option(_parameters, _requiredParametersCount, _shortAliases, _longAliases, _description, _name);
    }

    /// <summary>
    /// Allows implicit conversion of an <see cref="OptionalOnlyBuilder"/> instance to an <see cref="Option"/> instance.
    /// </summary>
    /// <param name="builder">The <see cref="OptionalOnlyBuilder"/> instance to be converted.</param>
    /// <returns>The finished <see cref="Option"/> instance.</returns>
    public static implicit operator Option(OptionalOnlyBuilder builder) {
        return builder.Build();
    }
}