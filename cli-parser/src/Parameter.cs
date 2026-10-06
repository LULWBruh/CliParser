namespace CliParser;

public abstract partial class Parameter {
    public static CustomParameter<T> CustomParameter<T>(Func<string, T> parsingFunction,
        Func<T, bool>? customValidation = null) {
        return new CustomParameter<T>(parsingFunction, customValidation);
    }

    public static BoolParameter BoolParameter() {
        return new BoolParameter();
    }

    public static EnumParameter EnumParameter(string firstValue, params string[] values) {
        return new EnumParameter([firstValue, ..values]);
    }

    public static EnumParameter EnumParameter(Type type, bool caseSensitive = false) {
        if (type == null) {
            throw new ArgumentNullException(nameof(type));
        }
        if (!type.IsEnum) {
            throw new InvalidOperationException($"{nameof(type)} is not enumerable.");
        }
        return new EnumParameter(type, caseSensitive);
    }

    public static IntParameter IntParameter(int min = int.MinValue, int max = int.MaxValue,
        Func<int, bool>? customValidation = null) {
        return new IntParameter(min, max, customValidation);
    }

    public static StringParameter StringParameter(Func<string, bool>? customValidation = null,
        string? regexValidation = null) {
        return new StringParameter(customValidation, regexValidation);
    }
}

public sealed partial class BoolParameter : Parameter { }

public sealed partial class EnumParameter : Parameter { }

public sealed partial class IntParameter : Parameter { }

public sealed partial class StringParameter : Parameter { }

public sealed partial class CustomParameter<T> : Parameter { }