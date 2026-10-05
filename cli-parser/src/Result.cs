namespace CliParser;

/// <summary>
/// Class for result of parsing. Has to be casted to implementations to be useable.
/// </summary>
public abstract class ParseResult {
    internal ParseResult(string helpString) {
        HelpString = helpString;
    }
    
    /// <summary>
    /// Represents the help message string, typically displayed to the user
    /// with information about command usage, available options, and their descriptions.
    /// </summary>
    public string HelpString { get; private init; }
}

/// <summary>
/// Result of "--help".
/// </summary>
public sealed class HelpResult : ParseResult {
    internal HelpResult(string helpString) : base(helpString) {
    }
}

/// <summary>
/// Result of "--version" or "-V".
/// </summary>
public sealed class VersionResult : ParseResult {
    internal VersionResult(string helpString, string versionString) : base(helpString) {
        VersionString = versionString;
    }

    /// <summary>
    /// Represents the version information string, typically used to display
    /// the current version of the application or command-line tool.
    /// </summary>
    public string VersionString { get; private init; }
}

/// <summary>
/// Contains results of successful parsing.
/// </summary>
public sealed class ParseData : ParseResult {
    internal Dictionary<string, OptionResult> _options { get; private init; }

    internal ParseData(Dictionary<string, OptionResult> options, string helpString,
        IReadOnlyList<string> plainArguments) : base(helpString) {
        _options = options;
        PlainArguments = plainArguments;
    }

    /// <summary>
    /// Returns IReadOnlyList of strings with plain arguments passed to the parser in their order.
    /// </summary>
    public IReadOnlyList<string> PlainArguments { get; private init; }

    /// <summary>
    /// Returns how many plain arguments were passed to the parser.
    /// </summary>
    public int GetPlainCount => PlainArguments.Count;

    /// <summary>
    /// Returns a result of parsing for an option of a given name.
    /// </summary>
    /// <param name="name">Name of the option.</param>
    /// <returns>New <see cref="OptionResult"/> instance.</returns>
    /// <exception cref="OptionNotFoundException">If option of a given name can not be found in the parser.</exception>
    public OptionResult GetOption(string name) {
        if (_options.TryGetValue(name, out OptionResult? optionResult)) return optionResult;
        throw new OptionNotFoundException(name);
    }

    /// <summary>
    /// Returns a result of parsing for an option of a given name.
    /// </summary>
    /// <param name="option">Option instance.</param>
    /// <returns>New <see cref="OptionResult"/> instance.</returns>
    /// <exception cref="OptionNotFoundException">If option of a given name cannot be found in the parser.</exception>
    public OptionResult GetOption(Option option) {
        return GetOption(option.Name);
    }

    /// <summary>
    /// Get string of a plain argument at given index.
    /// </summary>
    /// <param name="index">Index of the plain argument.</param>
    /// <returns>Plain argument at given index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if index is out of range.</exception>
    public string GetPlainArgument(int index) {
        return PlainArguments[index];
    }
}

/// <summary>
/// Represents a result of parsing parameters for a given option.
/// </summary>
public sealed partial class OptionResult {
    private List<object> _parameterResults { get; init; }

    public OptionResult(List<object> parameterResults, bool isSet) {
        _parameterResults = parameterResults;
        IsSet = isSet;
    }


    /// <summary>
    /// Is true if the user set the parameter.
    /// </summary>
    public bool IsSet { get; private init; }

    /// <summary>
    /// Returns the count of set parameters.
    /// </summary>
    public int GetParameterCount => _parameterResults.Count;

    /// <summary>
    /// Gets the value of the parameter. Can cast to int, string and enum types.
    /// </summary>
    /// <typeparam name="T">Type of the parameter.</typeparam>
    /// <param name="index">Index of the parameter.</param>
    /// <returns>Parameter casted to the given type.</returns>
    /// <exception cref="InvalidCastException">Thrown when given parameter can not be casted into T.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if index is out of range.</exception>
    public T GetParameter<T>(int index) {
        //if (!IsSet) throw new InvalidOperationException();
        if (index < 0 || index >= _parameterResults.Count) throw new IndexOutOfRangeException();
        object result = _parameterResults[index];

        if (result is T resultT) return resultT; // including enum

        if (typeof(T).IsEnum && result is string enumName) {
            if (Enum.TryParse(typeof(T), enumName, true, out object enumResult)) {
                return (T)enumResult;
            }
        }

        throw new InvalidCastException($"Cannot cast parameter to type {typeof(T).Name}");

        // old version
        // if (typeof(T).IsEnum && result is string enumName) {
        //     var names = Enum.GetNames(typeof(T));
        //     var values = Enum.GetValues(typeof(T));
        //
        //     for (int i = 0; i < names.Length; i++) {
        //         if (names[i].ToLower() == enumName.ToLower()) {
        //             return (T)values.GetValue(i);
        //         }
        //     }
        // }
    }

    /// <summary>
    /// Silent version of <see cref="GetParameter{T}(int)"/>. Gets the value of the parameter. Can cast to int, string, enum types and custom types. Returns bool (true = cast successful).
    /// </summary>
    /// <typeparam name="T">Type of the parameter.</typeparam>
    /// <param name="index">Index of the parameter.</param>
    /// <param name="result">Out reference for the result</param>
    /// <returns>True if cast was successful. False if cast was unsuccessful or IsSet is false.</returns>
    public bool TryGetParameter<T>(int index, out T? result) {
        // exceptions are slow
        result = default;
        //if (!IsSet) return false;
        if (index < 0 || index >= _parameterResults.Count) return false;

        try {
            result = GetParameter<T>(index);
            return true;
        }
        catch (Exception e) {
            return false;
        }
    }

}
