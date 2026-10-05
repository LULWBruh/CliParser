namespace CliParser;

/// <summary>
/// Represents a parameter of an option and how it is supposed to be parsed.
/// </summary>
public abstract partial class Parameter
{
    /// <summary>
    /// Creates a custom type parameter.
    /// </summary>
    /// <param name="parsingFunction">Predicate (of type <see cref="Func{string, T}"/>) for T parsing.</param>
    /// <param name="defaultValue">Default value (if the parameter is not specified by the user). Optional.</param>
    /// <param name="customValidation">Predicate (of type <see cref="Func{T, bool}"/>) for T validation. Optional.</param>
    /// <returns>New <see cref="T:CliParser.CustomParameter{T}"/> instance.</returns>
    public static CustomParameter<T> CustomParameter<T>(Func<string, T> parsingFunction, T? defaultValue = default,
        Func<T, bool>? customValidation = null)
    {
        return new CustomParameter<T>(parsingFunction, defaultValue, customValidation);
    }

    /// <summary>
    /// Creates a boolean parameter.
    /// </summary>
    /// <param name="defaultValue">Default value (if the parameter is not specified by the user). Optional.</param>
    /// <returns>New <see cref="T:CliParser.BoolParameter"/> instance.</returns>
    public static BoolParameter BoolParameter(bool defaultValue = true)
    {
        return new BoolParameter(defaultValue);
    }

    /// <summary>
    /// Creates an enum parameter. Enum is represented by a collection of strings (<see cref="string[]"/>).
    /// </summary>
    /// <param name="firstValue">First enum value. Ensures that at least one enum value is passed to the function.</param>
    /// <param name="values">Non empty collection of strings. These represent possible string values.</param>
    /// <returns>New <see cref="T:CliParser.EnumParameter"/> instance.</returns>
    public static EnumParameter EnumParameter(string firstValue, params string[] values)
    {
        return new EnumParameter([firstValue, ..values]);
    }

    /// <summary>
    /// Creates an enum parameter. Enum values are represented by enum type. Is supposed to be used for direct casting from string to enum.
    /// </summary>
    /// <param name="type">Type of the enum</param>
    /// <param name="caseSensitive">If true, the parameter value is case sensitive. Optional.</param>
    /// <returns>New <see cref="T:CliParser.EnumParameter"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if type.IsEnum (see <see cref="Type.IsEnum"/>) returns false.</exception>
    public static EnumParameter EnumParameter(Type type, bool caseSensitive = false)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (!type.IsEnum) throw new InvalidOperationException($"{nameof(type)} is not enumerable.");
        else return new EnumParameter(type, caseSensitive);
    }

    /// <summary>
    /// Creates an integer parameter.
    /// </summary>
    /// <param name="defaultValue">Default value (if the parameter is not specified by the user). Optional.</param>
    /// <param name="min">Minimum value. Optional.</param>
    /// <param name="max">Maximum value. Optional.</param>
    /// <param name="customValidation">Predicate (of type <see cref="Func{int, bool}"/>) for integer validation. Optional.</param>
    /// <returns>New <see cref="T:CliParser.IntParameter"/> instance.</returns>
    public static IntParameter IntParameter(int defaultValue = 0, int min = int.MinValue, int max = int.MaxValue,
        Func<int, bool>? customValidation = null)
    {
        return new IntParameter(defaultValue, min, max, customValidation);
    }

    /// <summary>
    /// Creates a string parameter.
    /// </summary>
    /// <param name="defaultValue">Default value (if the parameter is not specified by the user). Optional.</param>
    /// <param name="customValidation">Predicate (of type <see cref="Func{string, bool}"/>) for string validation. Optional.</param>
    /// <param name="regexValidation">Custom regex for string validation. Optional.</param>
    /// <returns>New <see cref="T:CliParser.StringParameter"/> instance.</returns>
    public static StringParameter StringParameter(string defaultValue = "", Func<string, bool>? customValidation = null,
        string? regexValidation = null)
    {
        return new StringParameter(defaultValue, customValidation, regexValidation);
    }
}

/// <summary>
/// Represents a bool parameter for an <see cref="Option"/> instance.
/// </summary>
public sealed partial class BoolParameter : Parameter { }

/// <summary>
/// Represents an enum parameter for an <see cref="Option"/> instance.
/// </summary>
public sealed partial class EnumParameter : Parameter { }

/// <summary>
/// Represents an integer parameter for an <see cref="Option"/> instance.
/// </summary>
public sealed partial class IntParameter : Parameter { }

/// <summary>
/// Represents a string parameter for an <see cref="Option"/> instance.
/// </summary>
public sealed partial class StringParameter : Parameter { }

/// <summary>
/// Represents a custom parameter for an <see cref="Option"/> instance.
/// </summary>
public sealed partial class CustomParameter<T> : Parameter { }