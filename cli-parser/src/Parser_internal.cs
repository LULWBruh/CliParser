using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CliParser;

public sealed partial class Parser {
    internal const string PLAIN_SEPARATOR = "--";
    internal const string LONG_HELP_OPTION = "help";
    internal const string LONG_VERSION_OPTION = "version";
    internal const char SHORT_HELP_OPTION = 'H';
    internal const char SHORT_VERSION_OPTION = 'V';
}