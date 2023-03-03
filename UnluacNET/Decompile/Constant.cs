using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnluacNET
{
    public class Constant
    {
        public const int CONST_NIL    = 0;
        public const int CONST_BOOL   = 1;
        public const int CONST_NUMBER = 2;
        public const int CONST_STRING = 3;

        private static readonly HashSet<String> m_reservedWords =
            new HashSet<string>() {
                "and",
                "and",
                "break",
                "do",
                "else",
                "elseif",
                "end",
                "false",
                "for",
                "function",
                "if",
                "in",
                "local",
                "nil",
                "not",
                "or",
                "repeat",
                "return",
                "then",
                "true",
                "until",
                "while",
            };

        private readonly int m_type;

        private readonly bool m_bool;
        private readonly LNumber m_number;
        private readonly string m_string;

        public bool IsBoolean
        {
            get { return (m_type == CONST_BOOL); }
        }

        public bool IsIdentifier
        {
            get
            {
                if (!IsString || m_string.Length == 0 || m_reservedWords.Contains(m_string))
                    return false;
                
                var start = (char)m_string[0];

                if (start != '_' && !Char.IsLetter(start))
                    return false;

                for (int i = 1; i < m_string.Length; i++)
                {
                    var next = (char)m_string[i];

                    if (Char.IsLetterOrDigit(next) || next == '_')
                        continue;

                    return false;
                }

                return true;
            }
        }

        public bool IsInteger
        {
            get
            {
                var value = m_number.Value;

                return value == Math.Round(value);
            }
        }

        public bool IsNil
        {
            get { return (m_type == CONST_NIL); }
        }

        public bool IsNumber
        {
            get { return (m_type == CONST_NUMBER); }
        }

        public bool IsString
        {
            get { return (m_type == CONST_STRING); }
        }

        public int AsInteger()
        {
            if (!IsInteger)
                throw new InvalidOperationException();

            return (int)m_number.Value;
        }

        public string AsName()
        {
            if (!IsString)
                throw new InvalidOperationException();

            return m_string;
        }

        public void Print(Output output)
        {
            switch (m_type)
            {
            case CONST_NIL:
                output.Print("nil");
                break;
            case CONST_BOOL:
                output.Print(m_bool ? "true" : "false");
                break;
            case CONST_NUMBER:
                output.Print(m_number.ToString());
                break;
            case CONST_STRING:
                {
                    var newLines = 0;
                    var unprinttable = 0;

                    foreach (char c in m_string)
                    {
                        if (c == '\n')
                            newLines++;
                        else if ((c <= 31 && c != '\t' || c >= 127))
                            unprinttable++;
                    }

                    if (unprinttable == 0 && !m_string.Contains("[[") &&
                        (newLines > 1 || newLines == 1 && m_string.IndexOf('\n') != m_string.Length - 1))
                    {
                        var pipe = 0;
                        var pipeString = "]]";

                        while (m_string.IndexOf(pipeString) >= 0)
                        {
                            pipe++;
                            pipeString = "]";

                            var i = pipe;

                            while (i-- > 0)
                                pipeString += "=";

                            pipeString += "]";
                        }

                        output.Print("[");

                        while (pipe-- > 0)
                            output.Print("=");

                        output.Print("[");

                        var indent = output.IndentationLevel;

                        output.IndentationLevel = 0;

                        output.PrintLine();
                        output.Print(m_string);
                        output.Print(pipeString);

                        output.IndentationLevel = indent;
                    }
                    else
                    {
                        output.Print("\"");

                        var chars = new[] {
                                    "\\a",
                                    "\\b",
                                    "\\t",
                                    "\\n",
                                    "\\v",
                                    "\\f",
                                    "\\r",
                                };

                        foreach (char c in m_string)
                        {
                            if (c <= 31 || c >= 127)
                            {
                                //if (c == 7)
                                //    output.Print("\\a");
                                //else if (c == 8)
                                //    output.Print("\\b");
                                //else if (c == 12)
                                //    output.Print("\\f");
                                //else if (c == 10)
                                //    output.Print("\\n");
                                //else if (c == 13)
                                //    output.Print("\\r");
                                //else if (c == 9)
                                //    output.Print("\\t");
                                //else if (c == 11)
                                //    output.Print("\\v");

                                var cx = ((int)c);

                                if (cx >= 7 && cx <= 13)
                                {
                                    output.Print(chars[cx - 7]);
                                }
                                else
                                {
                                    var dec = cx.ToString();
                                    var len = dec.Length;

                                    output.Print("\\");

                                    while (len++ < 3)
                                        output.Print("0");

                                    output.Print(dec);
                                }
                            }
                            else if (c == 34)
                                output.Print("\\\"");
                            else if (c == 92)
                                output.Print("\\\\");
                            else
                                output.Print(c.ToString());
                        }

                        output.Print("\"");
                    }
                } break;
            default:
                throw new InvalidOperationException();
            }
        }

        public Constant(int constant)
        {
            m_type   = 2;
            m_bool   = false;
            m_number = LNumber.MakeInteger(constant);
            m_string = null;
        }

        public Constant(LObject constant)
        {
            if (constant is LNil)
            {
                m_type   = 0;
                m_bool   = false;
                m_number = null;
                m_string = null;
            }
            else if (constant is LBoolean)
            {
                m_type   = 1;
                m_bool   = (constant == LBoolean.LTRUE);
                m_number = null;
                m_string = null;
            }
            else if (constant is LNumber)
            {
                m_type   = 2;
                m_bool   = false;
                m_number = (LNumber)constant;
                m_string = null;
            }
            else if (constant is LString)
            {
                m_type   = 3;
                m_bool   = false;
                m_number = null;
                m_string = ((LString)constant).DeRef();
            }
            else
            {
                throw new ArgumentException("Illegal constant type: " + constant.ToString());
            }
        }
    }
}
