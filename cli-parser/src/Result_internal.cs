namespace CliParser;


public sealed partial class ParsedData : ParseResult {
    internal void AddAllOptions(Dictionary<string, Option> options) {
        foreach (var (key, value) in options) {
            Options.Add(key, new OptionResult(value));
        }
    }

    internal void AddPlain(string plain) {
        PlainArguments.Add(plain);
    }

    internal void MakeSet(string name) {
        Options[name].IsSet = true;
    }
    
    // returns true if all parsed
    internal bool AddParameter(string name, string parameter) {
        return Options[name].AddParameter(parameter);
    }

    internal void Validate(List<HashSet<string>> conflicts, Dictionary<string, HashSet<string>> dependencies) {
        foreach (var (key, value) in Options) {
            value.CheckValidParsing();

            if (dependencies.TryGetValue(key, out var dependency)) {
                foreach (var name in dependency) {
                    if (!Options.TryGetValue(name, out OptionResult? val) || val.IsSet) {
                        throw new ParserException($"Dependency for {key} not found: {name}");
                    }
                }
            }
        }

        foreach (var conflictGroup in conflicts) {
            if (conflictGroup.Count(x => Options[x].IsSet) > 1) {
                throw new ParserException("Multiple options for the same conflict group");
            }
        }
    }
}

public sealed partial class OptionResult {
    // returns true if all parsed
    internal bool AddParameter(string parameter) {
        if (GetParameterCount >= _option.Parameters.Count) {
            throw new ParserException("All parameters already parsed");
        }
        ParameterResults.Add(_option.ParseParameter(parameter, GetParameterCount));

        return GetParameterCount >= _option.Parameters.Count;
    }
    
    internal void CheckValidParsing() {
        if (!IsSet && _option.Required) {
            throw new ParserException("All parameters must be set");
        }
        if (IsSet && GetParameterCount < _option.Parameters.Count) {
            throw new ParserException("More parameters required");
        }
    }
}