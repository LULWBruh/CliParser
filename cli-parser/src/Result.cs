using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace CliParser;

public abstract class ParseResult {
    internal ParseResult(string helpString) {
        HelpString = helpString;
    }

    public string HelpString { get; private init; }
}

public sealed class ParserInfo : ParseResult {
    public string VersionString { get; private init; }
    internal ParserInfo(string helpString, string versionString) : base(helpString) {
        VersionString = versionString;
    }
}

public sealed class ParsedData : ParseResult {
    internal Dictionary<string, OptionResult> Options { get; private init; }

    internal ParsedData(Dictionary<string, OptionResult> options, string helpString,
        IReadOnlyList<string> plainArguments) : base(helpString) {
        Options = options;
        PlainArguments = plainArguments;
    }

    public IReadOnlyList<string> PlainArguments { get; private init; }

    public int GetPlainCount => PlainArguments.Count;

    public OptionResult GetOption(string name) {
        if (Options.TryGetValue(name, out OptionResult? optionResult)) {
            return optionResult;
        }
        throw new OptionNotFoundException(name);
    }

    public OptionResult GetOption(Option option) {
        return GetOption(option.Name);
    }

    public string GetPlainArgument(int index) {
        return PlainArguments[index];
    }
}

public sealed class OptionResult {
    private List<object> ParameterResults { get; init; }

    public OptionResult(List<object> parameterResults, bool isSet) {
        ParameterResults = parameterResults;
        IsSet = isSet;
    }

    public bool IsSet { get; private init; }

    public int GetParameterCount => ParameterResults.Count;

    public T GetParameter<T>(int index) {
        if (!IsSet) throw new InvalidOperationException();
        if (index < 0 || index >= ParameterResults.Count) throw new IndexOutOfRangeException();
        object result = ParameterResults[index];

        if (result is T resultT) return resultT; // including enum

        if (typeof(T).IsEnum && result is string enumName) {
            if (Enum.TryParse(typeof(T), enumName, true, out object? enumResult)) {
                return (T)enumResult;
            }
        }

        throw new InvalidCastException($"Cannot cast parameter to type {typeof(T).Name}");
    }

    public bool TryGetParameter<T>(int index, out T? result) {
        result = default;

        try {
            result = GetParameter<T>(index);
            return true;
        }
        catch (Exception e) {
            return false;
        }
    }
}