using CliParser;

namespace CliParser;

public class ParserException : Exception {
    public ParserException(string message) : base("ParserException:\t" + message) { }
}

public class RequiredOptionMissingException : ParserException {
    public RequiredOptionMissingException(string message) : base(message) { }

    public RequiredOptionMissingException(Option option) : base($"Option {option.Name} not set.") { }

    public Option? option { get; private set; }
}

public class ConflictingOptionsSetException : ParserException {
    public ConflictingOptionsSetException(string message) : base(message) { }

    public ConflictingOptionsSetException(Option option1, Option option2) : base(
        $"Options {option1.Name} and {option2.Name} set even though they are conflicting.") { }

    public IEnumerable<Option>? ConflictingOptions { get; private set; }
}

public class ParserBuildException : Exception {
    public ParserBuildException(string? message) : base(message) { }
}

public class NotUniqueOptionNameException : ParserBuildException {
    public NotUniqueOptionNameException(string message) : base(message) { }

    public NotUniqueOptionNameException(Option option) : base($"Option name {option.Name} repeated.") { }
}

public class OptionNotFoundException : ParserBuildException {
    public OptionNotFoundException(string name) : base($"Option of name {name} not found.") { }
}

public class ConflictingOptionAliasesException : ParserBuildException {
    public ConflictingOptionAliasesException(string message) : base(message) { }

    public ConflictingOptionAliasesException(Option option1, Option option2, string alias) : base(
        $"Option {option1.Name} and {option2.Name} have conflicting alias \"{alias}.\"") { }
}

public class BadAliasException : ParserBuildException {
    public BadAliasException(string message) : base(message) { }

    public BadAliasException(Option option, string alias) : base(
        $"Option {option.Name} has {alias} which is not allowed.") { }
}