using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CliParser;

public abstract partial class Parameter {
    internal abstract object ParseParameter(string parameter);
    internal abstract string Name();
}

public sealed partial class BoolParameter {
    internal BoolParameter() {
    }

    internal override object ParseParameter(string parameter) {
        return bool.Parse(parameter);
    }

    internal override string Name() {
        return "<bool>";
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

    internal override object ParseParameter(string parameter) {
        string output = CaseSensitive ?  parameter : parameter.ToLower();
        if (!Values.Contains(output)) {
            throw  new ParserException("Parameter " + parameter + " is invalid");
        }
        return output;
    }

    internal override string Name() {
        StringBuilder output = new();
        string separator = "[";

        foreach (var val in Values) {
            output.Append(separator + val);
            separator = " ";
        }
        
        return output.Append("]").ToString();
    }
}

public sealed partial class IntParameter {
    internal int MinValue { get; init; }
    internal int MaxValue { get; init; }
    internal Func<int, bool> CustomValidation { get; init; }

    internal IntParameter(int minValue, int maxValue, Func<int, bool>? customValidation) {
        MinValue = minValue;
        MaxValue = maxValue;
        CustomValidation = customValidation is null ? x => true : customValidation;
    }

    internal override object ParseParameter(string parameter) {
        int output = int.Parse(parameter);
        if (output < MinValue || output > MaxValue) {
            throw new ParserException("Parameter " + parameter + " is out of range " + MinValue + "-" + MaxValue);
        }
        if (!CustomValidation(output)) {
            throw new ParserException("Parameter " + parameter + " is invalid");
        }
        return output;
    }

    internal override string Name() {
        return "<int>";
    }
}

public sealed partial class StringParameter {
    private const string WILDCARD = ".*";
    internal Func<string, bool> CustomValidation { get; init; }
    internal string RegexPattern { get; init; }

    internal StringParameter(Func<string, bool>? customValidation, string? regexValidation) {
        CustomValidation = customValidation is null ? x => true : customValidation;
        RegexPattern = regexValidation is null ? WILDCARD : regexValidation;
    }

    internal override object ParseParameter(string parameter) {
        if (!Regex.IsMatch(parameter, RegexPattern)) {
            throw  new ParserException("Parameter " + parameter + " is invalid");
        }
        if  (!CustomValidation(parameter)) {
            throw new ParserException("Parameter " + parameter + " is invalid");
        }
        return parameter;
    }

    internal override string Name() {
        return "<string>";
    }
}

public sealed partial class CustomParameter<T> {
    internal Func<string, T> ParsingFunction { get; init; }
    internal Func<T, bool> CustomValidation { get; init; }

    internal CustomParameter(Func<string, T> parsingFunction, Func<T, bool>? customValidation) {
        ParsingFunction = parsingFunction;
        CustomValidation = customValidation is null ? x => true : customValidation;
    }

    internal override object ParseParameter(string parameter) {
        T output = ParsingFunction(parameter);
        if (!CustomValidation(output)) {
            throw new ParserException("Parameter " + parameter + " is invalid");
        }
        return output;
    }

    internal override string Name() {
        return $"<{typeof(T).ToString()}>";
    }
}
