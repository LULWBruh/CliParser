using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliParser;

public sealed partial class Option
{
    internal Option(List<Parameter> parameters, int requiredParametersCount, List<char> shortAliases,
        List<string> longAliases, string? description, string name)
    {
        Parameters = parameters;
        RequiredParametersCount = requiredParametersCount;
        ShortAliases = shortAliases.ToHashSet<char>();
        LongAliases = longAliases.ToHashSet<string>();
        Description = description;
        Name = name;
    }

    internal OptionParser GetOptionParser()
    {
        return new OptionParser(this);
    }
}

internal class OptionParser
{
    private int _numberOfParsedParameters = 0;
    private Option _option;
    internal bool IsSet { get; set; } = false;

    private List<ParameterParser> _paramParsers;

    internal OptionParser(Option option)
    {
        _option = option;
        _paramParsers = new List<ParameterParser>();
        foreach (var param in _option.Parameters) _paramParsers.Add(ParameterParser.GetParser(param));
    }

    internal bool TryParse(string token)
    {
        var canParse = _paramParsers[_numberOfParsedParameters].TryParse(token);
        if (canParse) _numberOfParsedParameters++;
        return canParse;
    }

    internal bool LastParamIsValid()
    {
        var valid = _paramParsers[_numberOfParsedParameters - 1].Validate();
        return valid;
    }

    internal bool CanParse()
    {
        return _numberOfParsedParameters < _paramParsers.Count;
    }

    internal bool AllRequiredParamsParsed()
    {
        return _numberOfParsedParameters >= _option.RequiredParametersCount;
    }


    internal OptionResult GetResult()
    {
        var results = (from param in _paramParsers
            select param.GetValue()).ToList<object>();
        return new OptionResult(results, IsSet);
    }
}