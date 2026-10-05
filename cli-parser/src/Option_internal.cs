using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliParser;

public sealed partial class Option {
    internal IReadOnlyList<Parameter> Parameters { get; private init; }
    internal int RequiredParametersCount { get; private init; }
    internal bool Required { get; private init; }
    internal IReadOnlySet<char> ShortAliases { get; private init; }
    internal IReadOnlySet<string> LongAliases { get; private init; }
    internal string? Description { get; private init; }
    
    internal Option(List<Parameter> parameters, int requiredParametersCount, List<char> shortAliases,
        List<string> longAliases, string? description, string name, bool required) {
        Parameters = parameters;
        RequiredParametersCount = requiredParametersCount;
        ShortAliases = shortAliases.ToHashSet<char>();
        LongAliases = longAliases.ToHashSet<string>();
        Description = description;
        Name = name;
        Required = required;
    }
}
