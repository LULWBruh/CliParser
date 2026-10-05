using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CliParser;

public abstract partial class Parameter { }

public sealed partial class BoolParameter
{
    internal bool DefaultValue { get; init; }

    internal BoolParameter(bool defaultValue = true)
    {
        DefaultValue = defaultValue;
    }
}

public sealed partial class EnumParameter
{
    internal bool CaseSensitive { get; init; }
    internal List<string> Values { get; init; }

    internal EnumParameter(Type enumType, bool caseSensitive)
    {
        CaseSensitive = caseSensitive;

        if (!CaseSensitive)
            Values = Enum.GetNames(enumType).Select(x => x.ToLower()).ToList();
        else
            Values = Enum.GetNames(enumType).ToList();
    }

    internal EnumParameter(string[] values)
    {
        if (!CaseSensitive)
            Values = values.Select(x => x.ToLower()).ToList();
        else
            Values = values.ToList();
    }
}

public sealed partial class IntParameter
{
    internal int DefaultValue { get; init; }
    internal int MinValue { get; init; }
    internal int MaxValue { get; init; }
    internal Func<int, bool> CustomValidation { get; init; }

    internal IntParameter(int defaultValue, int minValue, int maxValue, Func<int, bool>? customValidation)
    {
        DefaultValue = defaultValue;
        MinValue = minValue;
        MaxValue = maxValue;
        CustomValidation = customValidation is null ? x => true : customValidation;
    }
}

public sealed partial class StringParameter
{
    private const string WILDCARD = ".*";
    internal string DefaultValue { get; init; }
    internal Func<string, bool> CustomValidation { get; init; }
    internal string RegexPattern { get; init; }

    internal StringParameter(string defaultValue, Func<string, bool>? customValidation, string? regexValidation)
    {
        DefaultValue = defaultValue;
        CustomValidation = customValidation is null ? x => true : customValidation;
        RegexPattern = regexValidation is null ? WILDCARD : regexValidation;
    }
}

public sealed partial class CustomParameter<T>
{
    internal Func<string, T> ParsingFunction { get; init; }
    internal T? DefaultValue { get; init; }
    internal Func<T, bool> CustomValidation { get; init; }

    internal CustomParameter(Func<string, T> parsingFunction, T? defaultValue, Func<T, bool>? customValidation)
    {
        ParsingFunction = parsingFunction;
        DefaultValue = defaultValue;
        CustomValidation = customValidation is null ? x => true : customValidation;
    }
}

internal abstract class ParameterParser
{
    internal abstract bool TryParse(string token);
    internal abstract bool Validate();
    internal abstract object GetValue();

    internal static ParameterParser GetParser(Parameter parameter)
    {
        switch (parameter)
        {
            case StringParameter sp:
                return new StringParamParser(sp);
            case EnumParameter ep:
                return new EnumParamParser(ep);
            case IntParameter ip:
                return new IntParamParser(ip);
            case BoolParameter bp:
                return new BoolParamParser(bp);
            case object obj when obj.GetType().IsGenericType &&
                                 obj.GetType().GetGenericTypeDefinition() == typeof(CustomParameter<>):
                var genericArgument = obj.GetType().GetGenericArguments()[0];
                var parserType = typeof(CustomParamParser<>).MakeGenericType(genericArgument);
                return (ParameterParser)Activator.CreateInstance(parserType, obj);
            default:
                throw new UnreachableException("Unreachable reached in ParameterParser.GetParser");
        }
    }

    private class StringParamParser : ParameterParser
    {
        private StringParameter _parameter;
        private string? _value = null;

        public StringParamParser(StringParameter stringParameter)
        {
            _parameter = stringParameter;
        }

        internal override object GetValue()
        {
            return _value is null ? _parameter.DefaultValue : _value;
        }

        internal override bool TryParse(string token)
        {
            _value = token;
            return true;
        }

        internal override bool Validate()
        {
            if (_value is null) return true;
            if (!Regex.IsMatch(_value, _parameter.RegexPattern)) return false;
            return _parameter.CustomValidation(_value);
        }
    }

    private class EnumParamParser : ParameterParser
    {
        private EnumParameter _parameter;
        private string? _value = null;

        public EnumParamParser(EnumParameter enumParameter)
        {
            _parameter = enumParameter;
        }

        internal override object GetValue()
        {
            return _value is null ? _parameter.Values.First() : _value;
        }

        internal override bool TryParse(string token)
        {
            if (_parameter.Values.Contains(token))
            {
                _value = token;
                return true;
            }
            else if (!_parameter.CaseSensitive && _parameter.Values.Contains(token.ToLower()))
            {
                _value = token;
                return true;
            }

            return false;
        }

        internal override bool Validate()
        {
            return true;
        }
    }

    private class IntParamParser : ParameterParser
    {
        private IntParameter _parameter;
        private int? _value = null;

        public IntParamParser(IntParameter parameter)
        {
            _parameter = parameter;
        }

        internal override object GetValue()
        {
            return _value is null ? _parameter.DefaultValue : _value;
        }

        internal override bool TryParse(string token)
        {
            var canParse = int.TryParse(token, out var result);
            if (canParse) _value = result;
            return canParse;
        }

        internal override bool Validate()
        {
            if (_value is null) return true;
            if (!(_value <= _parameter.MaxValue && _value >= _parameter.MinValue)) return false;
            return _parameter.CustomValidation((int)_value);
        }
    }

    private class BoolParamParser : ParameterParser
    {
        private BoolParameter _parameter;
        private bool? _value = null;

        public BoolParamParser(BoolParameter parameter)
        {
            _parameter = parameter;
        }

        internal override object GetValue()
        {
            return _value is null ? _parameter.DefaultValue : _value;
        }

        internal override bool TryParse(string token)
        {
            var canParse = bool.TryParse(token, out var result);
            if (canParse) _value = result;
            return canParse;
        }

        internal override bool Validate()
        {
            return true;
        }
    }

    private class CustomParamParser<T> : ParameterParser
    {
        private CustomParameter<T> _parameter;
        private T? _value = default;

        public CustomParamParser(CustomParameter<T> parameter)
        {
            _parameter = parameter;
        }

        internal override object GetValue()
        {
            return _value is null ? _parameter.DefaultValue : _value;
        }

        internal override bool TryParse(string token)
        {
            try
            {
                _value = _parameter.ParsingFunction(token);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal override bool Validate()
        {
            if (_value is null) return true;
            return _parameter.CustomValidation(_value);
        }
    }
}