using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CliParser;

public abstract partial class Parameter { }

public sealed partial class BoolParameter {
    internal bool DefaultValue { get; init; }

    internal BoolParameter(bool defaultValue = true) {
        DefaultValue = defaultValue;
    }
}

public sealed partial class EnumParameter {
    internal bool CaseSensitive { get; init; }
    internal List<string> Values { get; init; }

    internal EnumParameter(Type enumType, bool caseSensitive) {
        CaseSensitive = caseSensitive;

        if (!CaseSensitive)
            Values = Enum.GetNames(enumType).Select(x => x.ToLower()).ToList();
        else
            Values = Enum.GetNames(enumType).ToList();
    }

    internal EnumParameter(string[] values) {
        if (!CaseSensitive)
            Values = values.Select(x => x.ToLower()).ToList();
        else
            Values = values.ToList();
    }
}

public sealed partial class IntParameter {
    internal int DefaultValue { get; init; }
    internal int MinValue { get; init; }
    internal int MaxValue { get; init; }
    internal Func<int, bool> CustomValidation { get; init; }

    internal IntParameter(int defaultValue, int minValue, int maxValue, Func<int, bool>? customValidation) {
        DefaultValue = defaultValue;
        MinValue = minValue;
        MaxValue = maxValue;
        CustomValidation = customValidation is null ? x => true : customValidation;
    }
}

public sealed partial class StringParameter {
    private const string WILDCARD = ".*";
    internal string DefaultValue { get; init; }
    internal Func<string, bool> CustomValidation { get; init; }
    internal string RegexPattern { get; init; }

    internal StringParameter(string defaultValue, Func<string, bool>? customValidation, string? regexValidation) {
        DefaultValue = defaultValue;
        CustomValidation = customValidation is null ? x => true : customValidation;
        RegexPattern = regexValidation is null ? WILDCARD : regexValidation;
    }
}

public sealed partial class CustomParameter<T> {
    internal Func<string, T> ParsingFunction { get; init; }
    internal T? DefaultValue { get; init; }
    internal Func<T, bool> CustomValidation { get; init; }

    internal CustomParameter(Func<string, T> parsingFunction, T? defaultValue, Func<T, bool>? customValidation) {
        ParsingFunction = parsingFunction;
        DefaultValue = defaultValue;
        CustomValidation = customValidation is null ? x => true : customValidation;
    }
}
