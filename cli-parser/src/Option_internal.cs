using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliParser;

public sealed partial class Option {
    internal IList<Parameter> Parameters { get; private init; }
    internal bool Required { get; private init; }
    internal ISet<char> ShortAliases { get; private init; }
    internal ISet<string> LongAliases { get; private init; }
    internal string? Description { get; private init; }
    
    internal Option(List<Parameter> parameters, List<char> shortAliases,
        List<string> longAliases, string? description, string name, bool required) {
        Parameters = parameters;
        ShortAliases = shortAliases.ToHashSet<char>();
        LongAliases = longAliases.ToHashSet<string>();
        Description = description;
        Name = name;
        Required = required;
    }

    internal object ParseParameter(string parameterString, int index) {
        return Parameters[index].ParseParameter(parameterString);
    }
}
