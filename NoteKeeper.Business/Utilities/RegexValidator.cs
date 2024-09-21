using System.Text.RegularExpressions;
using NoteKeeper.Business.Constants;

namespace NoteKeeper.Business.Utilities;

public partial class RegexValidator
{
    [GeneratedRegex(RegexPatternConstants.UsernameRegexPattern)]
    public static partial Regex UsernameRegex();

    [GeneratedRegex(RegexPatternConstants.PasswordRegexPattern)]
    public static partial Regex PasswordRegex();
}