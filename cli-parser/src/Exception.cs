using CliParser;

namespace CliParser;

/// <summary>
/// Exception thrown during parsing.
/// </summary>
public class ParserException : Exception {
    /// <summary>
    /// Base constructor. Calls <see cref="Exception"> constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ParserException(string message) : base("ParserException:\t" + message) { }
}

/// <summary>
/// Required option was not set by the user.
/// </summary>
public class RequiredOptionMissingException : ParserException {
    /// <summary>
    /// Base constructor. Calls <see cref="ParserException"> constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RequiredOptionMissingException(string message) : base(message) { }

    /// <summary>
    /// Stores the unset option and creates default message.
    /// </summary>
    /// <param name="option">Unset option.</param>
    public RequiredOptionMissingException(Option option) : base($"Option {option.Name} not set.") { }

    /// <summary>
    /// Unset option.
    /// </summary>
    public Option? option { get; private set; }
}

/// <summary>
/// Two conflicting options were set by the user.
/// </summary>
public class ConflictingOptionsSetException : ParserException {
    /// <summary>
    /// Base constructor. Calls <see cref="ParserException"> constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConflictingOptionsSetException(string message) : base(message) { }

    /// <summary>
    /// Sets ConflictingOptions field and creates default message.
    /// </summary>
    /// <param name="option1">Conflicting option.</param>
    /// <param name="option2">Conflicting option.</param>
    public ConflictingOptionsSetException(Option option1, Option option2) : base(
        $"Options {option1.Name} and {option2.Name} set even though they are conflicting.") { }

    /// <summary>
    /// Conflicting options.
    /// </summary>
    public IEnumerable<Option>? ConflictingOptions { get; private set; }
}

/// <summary>
/// Exception thrown during parser building.
/// </summary>
public class ParserBuildException : Exception {
    /// <summary>
    /// Base constructor. Calls <see cref="Exception"> constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ParserBuildException(string? message) : base(message) { }
}

/// <summary>
/// Two options in the same parser have the same name.
/// </summary>
public class NotUniqueOptionNameException : ParserBuildException {
    /// <summary>
    /// Base constructor. Calls <see cref="ParserBuildException"> constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotUniqueOptionNameException(string message) : base(message) { }

    /// <summary>
    /// Creates default message containing the repeated name.
    /// </summary>
    /// <param name="option">Repeated name.</param>
    public NotUniqueOptionNameException(Option option) : base($"Option name {option.Name} repeated.") { }
}

/// <summary>
/// Option of a given name not found in the given parser.
/// </summary>
public class OptionNotFoundException : ParserBuildException {
    /// <summary>
    /// Sets base message containing the name the invalid name of an option.
    /// </summary>
    /// <param name="name">Invalid name.</param>
    public OptionNotFoundException(string name) : base($"Option of name {name} not found.") { }
}

/// <summary>
/// Two options have conflicting aliases.
/// </summary>
public class ConflictingOptionAliasesException : ParserBuildException {
    /// <summary>
    /// Base constructor. Calls <see cref="ParserBuildException"> constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConflictingOptionAliasesException(string message) : base(message) { }

    /// <summary>
    /// Creates default message containing names of options with conflicting aliases and the alias itself.
    /// </summary>
    /// <param name="option1">Option with conflicting alias.</param>
    /// <param name="option2">Option with conflicting alias.</param>
    /// <param name="alias">Conflicting alias.</param>
    public ConflictingOptionAliasesException(Option option1, Option option2, string alias) : base(
        $"Option {option1.Name} and {option2.Name} have conflicting alias \"{alias}.\"") { }
}

/// <summary>
/// Given alias is invalid.
/// </summary>
public class BadAliasException : ParserBuildException {
    /// <summary>
    /// Base constructor. Calls <see cref="ParserBuildException"> constructor.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BadAliasException(string message) : base(message) { }

    /// <summary>
    /// Creates default message containing name of the option with invalid alias and the alias itself.
    /// </summary>
    /// <param name="option">Option with bad alias.</param>
    /// <param name="alias">Used alias.</param>
    public BadAliasException(Option option, string alias) : base(
        $"Option {option.Name} has {alias} which is not allowed.") { }
}