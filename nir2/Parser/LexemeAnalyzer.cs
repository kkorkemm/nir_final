using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Parser
{
    public enum LexemeType
    {
        Id,
        Public,
        Protected,
        Private,
        Class,
        Interface,
        Abstract,
        Static,
        ReadOnly,
        Virtual,
        Required,
        Const,
        Override,
        String,
        Int,
        Real,
        Bool,
        Void,
        LeftRoundBracket, // (
        RightRoundBracket, // )
        LeftSquareBracket, // [
        RightSquareBracket, // ]
        LeftBrace, // {
        RightBrace, // }
        Semicolon, // ;
        Colon, // :
        Comma, // ,
        Slash, // /
        Comment,
        Finish,
        Error
    }

    public class Lexeme
    {
        public LexemeType lexemeType;
        public int Line;
        public int Position;
        public string State;
        public int Program;
        public string Value;
    }

    public class LexemeAnalyzer
    {
        private LexemeType GetKeywordType(string ident)
        {
            switch (ident)
            {
                case "int": return LexemeType.Int;
                case "float":
                case "double":
                    return LexemeType.Real;
                case "bool": return LexemeType.Bool;
                case "string": return LexemeType.String;
                case "void": return LexemeType.Void;
                case "public": return LexemeType.Public;
                case "protected": return LexemeType.Protected;
                case "private": return LexemeType.Private;
                case "class": return LexemeType.Class;
                case "interface": return LexemeType.Interface;
                case "static": return LexemeType.Static;
                case "abstract": return LexemeType.Abstract;
                case "required": return LexemeType.Required;
                case "readonly": return LexemeType.ReadOnly;
                case "override": return LexemeType.Override;
                case "virtual": return LexemeType.Virtual;
                case "const": return LexemeType.Const;
                default: return LexemeType.Id;
            }
        }

        private enum CharType
        {
            Letter,
            Digit,
            Semicolon,    // ;
            Colon,        // :
            NewLine,      // \n
            LeftRoundBracket, // (
            RightRoundBracket, // )
            LeftSquareBracket, // [
            RightSquareBracket, // ]
            LeftBrace, // {
            RightBrace, // }
            Comma, // ,
            Slash, // /
            Comment,
            Other
        }

        private CharType GetCharType(char c)
        {
            if (char.IsLetter(c)) return CharType.Letter;
            if (char.IsDigit(c)) return CharType.Digit;

            switch (c)
            {
                case ';': return CharType.Semicolon;
                case ':': return CharType.Colon;
                case '\n': return CharType.NewLine;
                case '(': return CharType.LeftRoundBracket;
                case ')': return CharType.RightRoundBracket;
                case '[': return CharType.LeftSquareBracket;
                case ']': return CharType.RightSquareBracket;
                case '{': return CharType.LeftBrace;
                case '}': return CharType.RightBrace;
                case ',': return CharType.Comma;
                case '/': return CharType.Slash;
                default: return CharType.Other;
            }
        }

        private enum State
        {
            S,     // S - начальное состояние
            A,     // A - идентификаторы/ключевые слова
            B,     // B - комментарии
            C,     // C - разпознан комментарий
            F,     // F - успешное завершение
            O      // O - ошибка
        }

        private string programText;
        private int currentIndex;
        public  int currentLine = 1;
        private int currentPos = 1;
        private State currentState;

        private bool isComment = false;

        // список разпознанных лексем
        List<Lexeme> data = new List<Lexeme>();
        public List<Lexeme> GetData()
        {
            return data;
        }

        public LexemeAnalyzer(string prText)
        {
            programText = prText;
            currentIndex = 0;
            currentState = State.S;
        }

        public void Run()
        {
            Lexeme currentLexeme = new Lexeme();

            while (currentIndex < programText.Length)
            {
                // Пропускаем пробелы, табы, переходы на следующую строку
                while (currentIndex < programText.Length && char.IsWhiteSpace(programText[currentIndex]))
                {
                    if (programText[currentIndex] == ' ')
                        currentPos++;
                    else if (programText[currentIndex] == '\n')
                    {
                        currentLine++;
                        currentPos = 1;
                    }
                    //else
                    //    currentPos += 4;

                    currentIndex++;
                }

                // конец чтения
                if (currentIndex >= programText.Length)
                    break;

                currentLexeme = NextLexeme();
                currentLexeme.Line = currentLine;
                currentLexeme.Position = currentPos;

                if (currentLexeme.lexemeType == LexemeType.Error)
                {
                    string msg = $"Ошибка анализатора: строка = {currentLine}, позиция = {currentPos}";
                    throw new Exception(msg);
                }

                currentLexeme.State = currentState.ToString();
                data.Add(currentLexeme);
                currentIndex++;
                currentPos++;

                //currentLexeme.Position = currentIndex - 1;
            }

            // Добавляем маркер конца
            data.Add(new Lexeme
            {
                lexemeType = LexemeType.Finish,
                Line = currentLine,
                Position = currentPos
            });
        }

        private Lexeme NextLexeme()
        {
            // по умолчанию ошибка
            Lexeme result = new Lexeme();
            result.lexemeType = LexemeType.Error;
            StringBuilder value = new StringBuilder();
            currentState = State.S;

            char current_char = programText[currentIndex];
           
            // Добавляем первый символ
            if (currentIndex < programText.Length)
            {
                if (data.Count == 0)
                {
                    value.Append(current_char);
                    currentIndex++;
                }
            }

            while (currentIndex < programText.Length && currentState != State.F && currentState != State.O)
            {
                char currentChar = programText[currentIndex];

                //if (currentChar == ' ') 
                //    if (!isComment)
                //        break;

                CharType charType = GetCharType(currentChar);

                // Если встретили пробел и мы не в комментарии - завершаем лексему
                if (charType == CharType.Other && char.IsWhiteSpace(currentChar) && !isComment)
                {
                    //currentState = State.F; // Принудительно завершаем
                    break;
                }

                // Получаем переход из таблицы
                var transition = GetTransition(currentState, charType, currentChar);
                currentState = transition.NewState;

                // Выполняем действие
                if (transition.Action != -1)
                {
                    ExecuteAction(transition.Action, result, currentChar);
                }

                if (currentState != State.F && currentState != State.O)
                {
                    value.Append(currentChar);
                    currentIndex++;
                    result.Position++;
                }

                result.Program = transition.Action;
            }

            // После выхода из цикла проверяем конечное состояние
            try
            {
                switch (currentState)
                {
                    case State.A:
                        result.lexemeType = GetKeywordType(value.ToString());
                        break;
                    case State.O:
                        result.lexemeType = LexemeType.Error;
                        result.Value = "Нераспознанная лексема: " + value.ToString();
                        break;
                }
            }
            catch { }

            return result;
        }

        private (State NewState, int Action) GetTransition(State currentState, CharType charType, char currentChar)
        {
            // Таблица переходов
            switch (currentState)
            {
                case State.S:
                    switch (charType)
                    {
                        case CharType.Letter: return (State.A, 0);
                        case CharType.Digit: return (State.O, 4);
                        case CharType.LeftBrace:
                        case CharType.LeftSquareBracket:
                        case CharType.LeftRoundBracket:
                            return (State.F, 7);
                        case CharType.RightBrace:
                        case CharType.RightSquareBracket:
                        case CharType.RightRoundBracket:
                            return (State.F, 8);
                        case CharType.Colon:
                            return (State.F, 9);
                        case CharType.Semicolon:
                            return (State.F, 10);
                        case CharType.NewLine:
                            return (State.F, 5);
                        case CharType.Comma:
                            return (State.F, 11);
                        case CharType.Slash:
                            return (State.B, 12);
                        default: return (State.O, 3);
                    }

                case State.A:
                    switch (charType)
                    {
                        case CharType.Letter:
                        case CharType.Digit: 
                            return (State.A, 1);
                        case CharType.NewLine: return (State.F, 6);
                        case CharType.Slash: return (State.O, 4);
                        case CharType.Other: return (0, 3);

                        default: return (State.F, 2);
                    }

                case State.B:
                    switch (charType)
                    {
                        case CharType.Slash:
                            isComment = true;
                            return (State.C, 13);
                        case CharType.Other: return (0, 3);
                        default: return (State.O, 4);
                    }

                case State.C:
                    switch (charType)
                    {
                        case CharType.NewLine: isComment = false; return (State.F, 14);
                        default: return (State.C, 13);
                    }

                default:
                    return (State.O, 3);
            }
        }

        private void ExecuteAction(int action, Lexeme lexeme, char currentChar)
        {
            // Семантические программы
            switch (action)
            {
                case 0: // Начало идентификатора
                    lexeme.lexemeType = LexemeType.Id;
                    lexeme.Value += currentChar;
                    break;
                case 1: // Продолжение идентификатора
                    lexeme.Value += currentChar;
                    break;
                case 2: // Поиск идентификатора в списке служебных слов
                    lexeme.lexemeType = GetKeywordType(lexeme.Value);
                    currentIndex--;
                    currentPos--;
                    break;
                case 3: // Распознан символ, не относящийся к языку
                    lexeme.Value = "Недопустимый символ: " + currentChar;
                    lexeme.lexemeType = LexemeType.Error;
                    break;
                case 4: // Ошибка в лексеме
                    lexeme.Value = "Некорректная лексема: " + lexeme.Value;
                    lexeme.lexemeType = LexemeType.Error;
                    break;
                case 5: // Переход на новую строку
                    lexeme.Line++;
                    lexeme.Position = 1;
                    break;
                case 6: // Идентификатор + новая строка
                    lexeme.Value += currentChar;
                    lexeme.lexemeType = GetKeywordType(lexeme.Value);
                    lexeme.Line++;
                    lexeme.Position = 1;
                    break;
                case 7: // Открывающая скобка
                    switch (currentChar)
                    {
                        case '(': lexeme.lexemeType = LexemeType.LeftRoundBracket; break;
                        case '[': lexeme.lexemeType = LexemeType.LeftSquareBracket; break;
                        case '{': lexeme.lexemeType = LexemeType.LeftBrace; break;
                    }
                    break;
                case 8: // Закрывающая скобка
                    switch (currentChar)
                    {
                        case ')': lexeme.lexemeType = LexemeType.RightRoundBracket; break;
                        case ']': lexeme.lexemeType = LexemeType.RightSquareBracket; break;
                        case '}': lexeme.lexemeType = LexemeType.RightBrace; break;
                    }
                    break;
                case 9: // :
                    lexeme.lexemeType = LexemeType.Colon;
                    break;

                case 10: // ;
                    lexeme.lexemeType = LexemeType.Semicolon;
                    break;

                case 11: // ,
                    lexeme.lexemeType = LexemeType.Comma;
                    break;

                case 12:
                case 13:
                    lexeme.Value += currentChar;
                    lexeme.lexemeType = LexemeType.Comment;
                    break;

                case 14:
                    lexeme.lexemeType = LexemeType.Comment;
                    lexeme.Line++;
                    lexeme.Position = 1;
                    break;

                default:
                    lexeme.lexemeType = LexemeType.Error;
                    break;
            }
        }
    }
}
