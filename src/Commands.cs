using System;
using System.Collections.Generic;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Celin
{
    public enum Commands
    {
        HELP,
        HELLO,
        CLEAR,
        SERVER,
        LOGIN,
        OPEN
    }
    public class CommandParser
    {
        static readonly Parser<char, string> Arg =
            SkipWhitespaces
            .Then(Try(Token(c => !char.IsWhiteSpace(c))).ManyString());
        static readonly Parser<char, IEnumerable<string>> Args =
            Arg.Separated(Whitespace);
        static readonly Parser<char, Commands> help =
            Try(String("help")).ThenReturn(Commands.HELP);
        static readonly Parser<char, Commands> hello =
            Try(String("clear")).ThenReturn(Commands.CLEAR);
        static readonly Parser<char, Commands> server =
            Try(String("server")).ThenReturn(Commands.SERVER);
        static readonly Parser<char, Commands> login =
            Try(String("login")).ThenReturn(Commands.LOGIN);
        static readonly Parser<char, Commands> open =
            Try(String("open")).ThenReturn(Commands.OPEN);
        public static Parser<char, ValueTuple<Commands, Maybe<IEnumerable<string>>>> Parser
            => Map((c, a) => new ValueTuple<Commands, Maybe<IEnumerable<string>>>(c, a),
            OneOf(
                help,
                hello,
                server,
                login,
                open),
                Args.Optional());
        public static Parser<char, IEnumerable<string>> Test
            => Args;
    }
}
