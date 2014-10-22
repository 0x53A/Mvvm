using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mvvm.Parsing
{
    public abstract class Token
    {
        public abstract bool TryTake(string s, out int nTaken);
        public abstract Token Clone();
    }

    public class FixedStringToken : Token
    {
        string _s;
        public FixedStringToken(string s)
        {
            _s = s;
        }

        public override bool TryTake(string s, out int nTaken)
        {
            if (s.StartsWith(_s))
            {
                nTaken = _s.Length;
                return true;
            }
            nTaken = -1;
            return false;
        }

        public override Token Clone()
        {
            return this;
        }
    }

    public class RegexToken : Token
    {
        string _match;
        public string Result { get; private set; }

        public RegexToken(string match)
        {
            _match = match;
        }

        public override bool TryTake(string s, out int nTaken)
        {
            var match = Regex.Match(s, _match);
            if (match.Success && match.Index == 0)
            {
                Result = match.Value;
                nTaken = match.Length;
                return true;
            }
            nTaken = -1;
            return false;
        }

        public override Token Clone()
        {
            return new RegexToken(_match) { Result = this.Result };
        }
    }

    public class FixedLengthOutToken : Token
    {
        public string Result { get; private set; }
        int _n;
        public FixedLengthOutToken(int n)
        {
            _n = n;
        }

        public override bool TryTake(string s, out int nTaken)
        {
            Result = s.Substring(0, _n);
            nTaken = _n;
            return true;
        }

        public override Token Clone()
        {
            return new FixedLengthOutToken(_n) { Result = this.Result };
        }
    }

    public class GuidOutToken : Token
    {
        public Guid Result { get; private set; }

        public override bool TryTake(string s, out int nTaken)
        {
            Result = Guid.Parse(s.Substring(0, 36));
            nTaken = 36;
            return true;
        }

        public override Token Clone()
        {
            return new GuidOutToken() { Result = this.Result };
        }
    }

    public class TokensExpression
    {
        List<Token> _tokens = new List<Token>();

        public TokensExpression Add(Token t)
        {
            _tokens.Add(t);
            return this;
        }

        public IList<Token[]> FindAll(string s)
        {
            List<Token[]> result = new List<Token[]>();
            string current = s;
            int nTaken = -1;
            while (current != "")
            {
                var tokens = _tokens.Select(t => t.Clone()).ToArray();
                var head = tokens.First();
                var tail = tokens.Skip(1);
                while (current != "" && !head.TryTake(current, out nTaken))
                    current = current.Substring(1);
                if (current == "")
                    break;
                current = current.Substring(nTaken);
                foreach (var t in tail)
                {
                    if (!t.TryTake(current, out nTaken))
                        throw new Exception();
                    current = current.Substring(nTaken);
                }
                result.Add(tokens);
            }
            return result;
        }
    }

    public static class FixedTokensParser
    {
        public static TokensExpression Start()
        {
            return new TokensExpression();
        }
    }
}
