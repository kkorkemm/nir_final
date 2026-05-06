using nir2.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace nir2.Parser
{
    public class SyntaxAnalyzer
    {
        private enum State
        {
            S,
            M,
            B,
            N,
            Q,
            A,
            T,
            K,
            C,
            R,
            H,
            E,
            V,
            Error
        }

        private enum GeneratorTask
        {
            Empty,
            Name,
            ClassName,
            InterfaceName,
            SetString,
            SetInt,
            SetBoolean,
            SetReal,
            SetVoid,
            SetPublic,
            SetPrivate,
            SetProtected,
            SetStatic,
            SetAbstract,
            SetInterface,
            SetReadOnly,
            SetVirtual,
            SetRequired,
            StartClass,
            EndClass,
            AddAttribute,
            AddMethod,
            AddParameter,
            Inheritance, // наследование
            Agregation,
            Assosiation
        }

        private struct MagazineItem
        {
            public bool IsTerminal;
            public LexemeType Lexeme;
            public State S;

            public MagazineItem(LexemeType lexeme)
            {
                IsTerminal = true;
                Lexeme = lexeme;
                S = State.Error;
            }

            public MagazineItem(State state)
            {
                IsTerminal = false;
                Lexeme = LexemeType.Error;
                S = state;
            }
        }

        private GeneratorTask currentTask;
        private Lexeme currentLexeme;
        private State currentState;
        private Stack<MagazineItem> Magazine = new Stack<MagazineItem>();
        private Stack<GeneratorTask> Generator = new Stack<GeneratorTask>();

        private List<Lexeme> inputLexemes;
        private List<UMLElement> result;
        private Class currentClass;
        private ClassAttribute currentAttribute;
        private ClassMethod currentMethod;
        private MethodArgument currentArgument;
        private AccessType currentAccess = AccessType.Public;
        private DataType currentDataType = DataType.String;
        private ConstraintType currentConstraintType = ConstraintType.Null;
        private string currentName;
        private string currentRole;
        private bool isCollection = false;

        public List<Lexeme> input_data;

        public SyntaxAnalyzer(List<Lexeme> input_data)
        {
            inputLexemes = input_data;
            result = new List<UMLElement>();
        }

        public List<UMLElement> GetResult()
        {
            return result;
        }

        public void Run()
        {
            Magazine.Push(new MagazineItem(State.S));
            Generator.Push(GeneratorTask.Empty);

            int lexemeIndex = 0;
            currentLexeme = inputLexemes[lexemeIndex];

            while (Magazine.Count > 0 && Generator.Count > 0)
            {
                MagazineItem currentStackItem = Magazine.Pop();
                currentState = currentStackItem.S;
                currentTask = Generator.Pop();

                ApplyGrammarRule();

                if (currentStackItem.IsTerminal)
                {
                    if (currentLexeme.lexemeType == LexemeType.Finish)
                    {
                        throw new Exception($"Лексемы закончились, но стек не пуст. Строка: {currentLexeme.Line}, позиция: {currentLexeme.Position}");
                    }

                    if (currentStackItem.Lexeme == currentLexeme.lexemeType)
                    {
                        lexemeIndex++;
                        if (lexemeIndex < inputLexemes.Count)
                            currentLexeme = inputLexemes[lexemeIndex];
                    }
                    else
                    {
                        throw new Exception($"Неожиданная лексема. Ожидалось: {currentStackItem.Lexeme}, получено: {currentLexeme.lexemeType}. Строка: {currentLexeme.Line}, позиция: {currentLexeme.Position}");
                    }
                }
                else
                {
                    ExecuteTask();
                }
            }

            if (currentLexeme.lexemeType != LexemeType.Finish)
            {
                throw new Exception($"Остались необработанные лексемы. Строка: {currentLexeme.Line}, позиция: {currentLexeme.Position}");
            }
        }

        public void ExecuteTask()
        {
            string msg;
            switch (currentState)
            {
                case State.S:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.Public:
                        {
                            Magazine.Push(new MagazineItem(State.E));
                            Magazine.Push(new MagazineItem(State.Q));
                            Magazine.Push(new MagazineItem(LexemeType.LeftBrace));
                            Magazine.Push(new MagazineItem(State.N));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.B));
                            Magazine.Push(new MagazineItem(State.M));
                            Magazine.Push(new MagazineItem(LexemeType.Public));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.ClassName);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetPublic);
                            break;
                        }
                        case LexemeType.Protected:
                        {
                            Magazine.Push(new MagazineItem(State.E));
                            Magazine.Push(new MagazineItem(State.Q));
                            Magazine.Push(new MagazineItem(LexemeType.LeftBrace));
                            Magazine.Push(new MagazineItem(State.N));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.B));
                            Magazine.Push(new MagazineItem(State.M));
                            Magazine.Push(new MagazineItem(LexemeType.Protected));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.ClassName);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetProtected);
                            break;
                        }
                        case LexemeType.Private:
                        {
                            Magazine.Push(new MagazineItem(State.E));
                            Magazine.Push(new MagazineItem(State.Q));
                            Magazine.Push(new MagazineItem(LexemeType.LeftBrace));
                            Magazine.Push(new MagazineItem(State.N));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.B));
                            Magazine.Push(new MagazineItem(State.M));
                            Magazine.Push(new MagazineItem(LexemeType.Private));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.ClassName);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetPrivate);
                            break;
                        }
                        case LexemeType.Class:
                        {
                            Magazine.Push(new MagazineItem(State.E));
                            Magazine.Push(new MagazineItem(State.Q));
                            Magazine.Push(new MagazineItem(LexemeType.LeftBrace));
                            Magazine.Push(new MagazineItem(State.N));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(LexemeType.Class));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.ClassName);
                            Generator.Push(GeneratorTask.StartClass);
                            break;
                        }
                        case LexemeType.Interface:
                        {
                            Magazine.Push(new MagazineItem(State.E));
                            Magazine.Push(new MagazineItem(State.Q));
                            Magazine.Push(new MagazineItem(LexemeType.LeftBrace));
                            Magazine.Push(new MagazineItem(State.N));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(LexemeType.Interface));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.ClassName);
                            Generator.Push(GeneratorTask.SetInterface);
                            break;
                        }
                        case LexemeType.Comment:
                        {
                            Magazine.Push(new MagazineItem(State.S));
                            Magazine.Push(new MagazineItem(LexemeType.Comment));
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            break;
                        }

                        case LexemeType.Finish:
                            break;
                        default:
                        {
                            msg = $"Ошибка генератора; Строка: {Convert.ToString(currentLexeme.Line)},позиция: {Convert.ToString(currentLexeme.Position)}";
                            throw new Exception(msg);
                        }
                    }
                    break;
                }
                case State.M:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.Abstract:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Abstract));
                            Generator.Push(GeneratorTask.SetAbstract);
                            break;
                        }
                        case LexemeType.Static:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Static));
                            Generator.Push(GeneratorTask.SetStatic);
                            break;
                        }
                        case LexemeType.ReadOnly:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.ReadOnly));
                            Generator.Push(GeneratorTask.SetReadOnly);
                            break;
                        }
                        case LexemeType.Virtual:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Virtual));
                            Generator.Push(GeneratorTask.SetVirtual);
                            break;
                        }
                        case LexemeType.Required:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Required));
                            Generator.Push(GeneratorTask.SetRequired);
                            break;
                        }
                        case LexemeType.Const:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Const));
                            Generator.Push(GeneratorTask.Empty);
                            break;
                        }
                        case LexemeType.Override:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Override));
                            Generator.Push(GeneratorTask.Empty);
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                    break;
                }
                case State.B:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.Class:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Class));
                            Generator.Push(GeneratorTask.StartClass);
                            break;
                        }
                        case LexemeType.Interface:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Interface));
                            Generator.Push(GeneratorTask.SetInterface);
                            break;
                        }
                        default:
                        {
                            msg = $"Ошибка генератора; Строка: {Convert.ToString(currentLexeme.Line)},позиция: {Convert.ToString(currentLexeme.Position)}";
                            throw new Exception(msg);
                        }
                    }
                    break;
                }
                case State.N:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.Colon:
                        {
                            Magazine.Push(new MagazineItem(State.N));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(LexemeType.Colon));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Inheritance);
                            Generator.Push(GeneratorTask.Empty);
                            break;
                        }
                        case LexemeType.Comma:
                        {
                            Magazine.Push(new MagazineItem(State.N));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(LexemeType.Comma));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Inheritance);
                            Generator.Push(GeneratorTask.Empty);
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                    break;
                }
                case State.Q:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.Public:
                        {
                            Magazine.Push(new MagazineItem(State.Q));
                            Magazine.Push(new MagazineItem(State.R));
                            Magazine.Push(new MagazineItem(State.C));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(State.T));
                            Magazine.Push(new MagazineItem(State.M));
                            Magazine.Push(new MagazineItem(LexemeType.Public));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Name);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetPublic);
                            break;
                        }
                        case LexemeType.Private:
                        {
                            Magazine.Push(new MagazineItem(State.Q));
                            Magazine.Push(new MagazineItem(State.R));
                            Magazine.Push(new MagazineItem(State.C));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(State.T));
                            Magazine.Push(new MagazineItem(State.M));
                            Magazine.Push(new MagazineItem(LexemeType.Private));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Name);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetPrivate);
                            break;
                        }
                        case LexemeType.Protected:
                        {
                            Magazine.Push(new MagazineItem(State.Q));
                            Magazine.Push(new MagazineItem(State.R));
                            Magazine.Push(new MagazineItem(State.C));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(State.T));
                            Magazine.Push(new MagazineItem(State.M));
                            Magazine.Push(new MagazineItem(LexemeType.Protected));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Name);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetProtected);
                            break;
                        }
                        case LexemeType.Comment:
                        {
                            Magazine.Push(new MagazineItem(State.Q));
                            Magazine.Push(new MagazineItem(LexemeType.Comment));
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                    break;
                }
                case State.A:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.Public:
                        {
                            Magazine.Push(new MagazineItem(State.R));
                            Magazine.Push(new MagazineItem(State.C));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(State.T));
                            Magazine.Push(new MagazineItem(State.M));
                            Magazine.Push(new MagazineItem(LexemeType.Public));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Name);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetPublic);
                            break;
                        }
                        case LexemeType.Private:
                        {
                            Magazine.Push(new MagazineItem(State.R));
                            Magazine.Push(new MagazineItem(State.C));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(State.T));
                            Magazine.Push(new MagazineItem(State.M));
                            Magazine.Push(new MagazineItem(LexemeType.Private));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Name);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetPrivate);
                            break;
                        }
                        case LexemeType.Protected:
                        {
                            Magazine.Push(new MagazineItem(State.R));
                            Magazine.Push(new MagazineItem(State.C));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(State.T));
                            Magazine.Push(new MagazineItem(State.M));
                            Magazine.Push(new MagazineItem(LexemeType.Protected));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Name);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetProtected);
                            break;
                        }
                        case LexemeType.Comment:
                        {
                            Magazine.Push(new MagazineItem(State.S));
                            Magazine.Push(new MagazineItem(LexemeType.Comment));
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            break;
                        }

                        default:
                        {
                            break;
                        }
                    }
                    break;
                }
                case State.T:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.String:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.String));
                            Generator.Push(GeneratorTask.SetString);
                            break;
                        }
                        case LexemeType.Int:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Int));
                            Generator.Push(GeneratorTask.SetInt);
                            break;
                        }
                        case LexemeType.Bool:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Bool));
                            Generator.Push(GeneratorTask.SetBoolean);
                            break;
                        }
                        case LexemeType.Real:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Real));
                            Generator.Push(GeneratorTask.SetReal);
                            break;
                        }
                        case LexemeType.Void:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Void));
                            Generator.Push(GeneratorTask.SetVoid);
                            break;
                        }
                        case LexemeType.Id:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Generator.Push(GeneratorTask.Assosiation);
                            break;
                        }
                        default:
                        {
                            msg = $"Ошибка генератора; Строка: {Convert.ToString(currentLexeme.Line)},позиция: {Convert.ToString(currentLexeme.Position)}";
                            throw new Exception(msg);
                        }
                    }
                    break;
                }
                case State.K:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.LeftSquareBracket:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.RightSquareBracket));
                            Magazine.Push(new MagazineItem(LexemeType.LeftSquareBracket));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Agregation);
                            break;
                        }
                        default:
                        {
                             break;
                        }
                    }
                    break;
                }
                case State.C:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.Semicolon:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.Semicolon));
                            Generator.Push(GeneratorTask.AddAttribute);
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                    break;
                }
                case State.R:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.LeftRoundBracket:
                        {
                            Magazine.Push(new MagazineItem(LexemeType.RightBrace));
                            Magazine.Push(new MagazineItem(LexemeType.LeftBrace));
                            Magazine.Push(new MagazineItem(LexemeType.RightRoundBracket));
                            Magazine.Push(new MagazineItem(State.H));
                            Magazine.Push(new MagazineItem(LexemeType.LeftRoundBracket));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.AddMethod);
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                    break;
                }
                case State.H:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.String:
                        {
                            Magazine.Push(new MagazineItem(State.H));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(LexemeType.String));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.AddParameter);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetString);
                            break;
                        }
                        case LexemeType.Int:
                        {
                            Magazine.Push(new MagazineItem(State.H));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(LexemeType.Int));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.AddParameter);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetInt);
                            break;
                        }
                        case LexemeType.Bool:
                        {
                            Magazine.Push(new MagazineItem(State.H));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(LexemeType.Bool));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.AddParameter);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetBoolean);
                            break;
                        }
                        case LexemeType.Real:
                        {
                            Magazine.Push(new MagazineItem(State.H));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(LexemeType.Real));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.AddParameter);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetReal);
                            break;
                        }
                        case LexemeType.Void:
                        {
                            Magazine.Push(new MagazineItem(State.H));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(LexemeType.Void));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.AddParameter);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.SetVoid);
                            break;
                        }
                        case LexemeType.Id:
                        {
                            Magazine.Push(new MagazineItem(State.H));
                            Magazine.Push(new MagazineItem(LexemeType.Id));
                            Magazine.Push(new MagazineItem(State.K));
                            Magazine.Push(new MagazineItem(LexemeType.Id));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.AddParameter);
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Name);
                            break;
                        }
                        case LexemeType.Comma:
                        {
                            Magazine.Push(new MagazineItem(State.H));
                            Magazine.Push(new MagazineItem(LexemeType.Comma));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                    break;
                }
                case State.E:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.RightBrace:
                        {
                            Magazine.Push(new MagazineItem(State.S));
                            Magazine.Push(new MagazineItem(LexemeType.RightBrace));

                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.EndClass);
                            break;
                        }
                        default:
                        {
                            msg = $"Ошибка генератора; Строка: {Convert.ToString(currentLexeme.Line)},позиция: {Convert.ToString(currentLexeme.Position)}";
                            throw new Exception(msg);
                        }
                    }
                    break;
                }
                case State.V:
                {
                    switch (currentLexeme.lexemeType)
                    {
                        case LexemeType.Comment:
                        {
                            Magazine.Push(new MagazineItem(State.S));
                            Magazine.Push(new MagazineItem(LexemeType.Comment));
                            Generator.Push(GeneratorTask.Empty);
                            Generator.Push(GeneratorTask.Empty);
                            break;
                        }
                        default:
                            break;
                    }

                    break;
                }
                case State.Error:
                default:
                    msg = $"Ошибка генератора; Строка: {Convert.ToString(currentLexeme.Line)},позиция: {Convert.ToString(currentLexeme.Position)}";
                    throw new Exception(msg);
            }
        }

        public void ApplyGrammarRule()
        {
            var classes = result.Where(p => p is Class).ToList();
            
            switch (currentTask)
            {
                case GeneratorTask.Empty:
                    break;

                case GeneratorTask.StartClass:
                    currentClass = new Class() { AccessType = currentAccess, Constraint = currentConstraintType };
                    currentConstraintType = ConstraintType.Null;
                    break;

                case GeneratorTask.SetInterface:
                    currentClass = new Class() { AccessType = currentAccess, Constraint = ConstraintType.Interface };
                    break;

                case GeneratorTask.EndClass:
                    Class isNew = classes.Where(p => (p as Class).Name  == currentClass.Name).FirstOrDefault() as Class;
                    if (isNew == null)
                    {
                        result.Add(currentClass);
                    }
                    else
                    {
                        isNew.AccessType = currentClass.AccessType;
                        isNew.Attributes.AddRange(currentClass.Attributes);
                        isNew.Methods.AddRange(currentClass.Methods);
                        isNew.Constraint = currentClass.Constraint;
                    }

                    currentClass = null;
                    currentAttribute = null;
                    currentArgument = null;
                    currentName = "";
                    currentMethod = null;
                    currentConstraintType = ConstraintType.Null;
                    break;

                case GeneratorTask.Name:
                    currentName = new string(currentLexeme.Value.Where(c => !char.IsWhiteSpace(c)).ToArray());
                    break;

                case GeneratorTask.ClassName:
                    if (currentClass != null)
                        currentClass.Name = new string(currentLexeme.Value.Where(c => !char.IsWhiteSpace(c)).ToArray());
                    break;

                case GeneratorTask.InterfaceName:
                    if (currentClass != null)
                        currentClass.Name = new string(currentLexeme.Value.Where(c => !char.IsWhiteSpace(c)).ToArray());
                    break;

                case GeneratorTask.SetPublic:
                    currentAccess = AccessType.Public;
                    break;

                case GeneratorTask.SetPrivate:
                    currentAccess = AccessType.Private;
                    break;

                case GeneratorTask.SetProtected:
                    currentAccess = AccessType.Protected;
                    break;

                case GeneratorTask.SetStatic:
                    currentConstraintType = ConstraintType.Static;
                    break;

                case GeneratorTask.SetAbstract:
                    currentConstraintType = ConstraintType.Abstract;
                    break;
                case GeneratorTask.SetRequired:
                    currentConstraintType = ConstraintType.required;
                    break;
                case GeneratorTask.SetReadOnly:
                    currentConstraintType = ConstraintType.readOnly;
                    break;
                case GeneratorTask.SetVirtual:
                    currentConstraintType = ConstraintType.Virtual;
                    break;
                case GeneratorTask.SetString:
                    currentDataType = DataType.String;
                    break;
                case GeneratorTask.SetInt:
                    currentDataType = DataType.Integer;
                    break;
                case GeneratorTask.SetVoid:
                    currentDataType = DataType.Void;
                    break;
                case GeneratorTask.SetBoolean:
                    currentDataType = DataType.Boolean;
                    break;
                case GeneratorTask.SetReal:
                    currentDataType = DataType.Real;
                    break;

                case GeneratorTask.AddAttribute:
                    if (!string.IsNullOrWhiteSpace(currentRole))
                    {
                        Class class2 = classes.FirstOrDefault(p => (p as Class).Name == currentRole) as Class;
                        if (class2 == null)
                        {
                            class2 = new Class() { Name = currentRole };
                            result.Add(class2);
                        }

                        Connection connection = new Connection()
                        {
                            Class1 = currentClass,
                            Class2 = class2,
                            Role2 = new ConnectionRole() { Name = currentName, AccessType = currentAccess }
                        };

                        if (isCollection)
                            connection.ConnectionType = new ConnectionType(3, "Агрегация");
                        else
                            connection.ConnectionType = new ConnectionType(1, "Ассоциация");

                        result.Add(connection);
                    }
                    else
                    {
                        currentAttribute = new ClassAttribute()
                        {
                            Name = currentName,
                            accessType = currentAccess,
                            Constraint = currentConstraintType,
                            dataType = currentDataType.ToString(),
                            IsCollection = isCollection
                        };
                        if (currentClass != null)
                            currentClass.Attributes.Add(currentAttribute);
                    }
                    isCollection = false;
                    currentRole = null;
                    currentAccess = AccessType.Public;
                    currentConstraintType = ConstraintType.Null;
                    break;

                case GeneratorTask.AddMethod:
                    currentMethod = new ClassMethod()
                    {
                        Name = currentName,
                        accessType = currentAccess,
                        Constraint = currentConstraintType,
                        Arguments = new List<MethodArgument>(),
                        IsCollection = isCollection
                    };
                    if (!string.IsNullOrWhiteSpace(currentRole))
                        currentMethod.dataType = currentRole;
                    else
                        currentMethod.dataType = currentDataType.ToString();

                    if (currentClass != null)
                        currentClass.Methods.Add(currentMethod);

                    currentRole = null;
                    currentName = null;
                    isCollection = false;
                    break;

                case GeneratorTask.AddParameter:
                    currentArgument = new MethodArgument()
                    {
                        Name = currentLexeme.Value,
                        IsCollection = isCollection
                    };
                    if (!string.IsNullOrWhiteSpace(currentName))
                    {
                        currentArgument.dataType = currentName;
                        Class isClass = result.OfType<Class>().FirstOrDefault(c => c.Name == currentName);
                        if (isClass == null)
                        {
                            isClass = new Class() { Name = currentName };
                            result.Add(isClass);
                        }

                        Connection dependency = new Connection()
                        {
                            Class1 = currentClass,
                            Class2 = isClass,
                            ConnectionType = new ConnectionType(2, "Зависимость")
                        };
                        result.Add(dependency);
                    }
                    else
                        currentArgument.dataType = currentDataType.ToString();

                    if (currentMethod != null)
                        currentMethod.Arguments.Add(currentArgument);

                    isCollection = false;
                    currentName = null;
                    currentRole = null;
                    currentAccess = AccessType.Public;
                    currentConstraintType = ConstraintType.Null;
                    break;

                case GeneratorTask.Assosiation:
                    currentRole = currentLexeme.Value;
                    break;

                case GeneratorTask.Inheritance:
                    string baseName = new string(currentLexeme.Value.Where(c => !char.IsWhiteSpace(c)).ToArray());

                    Class baseClass = result.OfType<Class>().FirstOrDefault(c => c.Name == baseName);
                    if (baseClass == null)
                    {
                        baseClass = new Class() { Name = baseName };
                        result.Add(baseClass);
                    }

                    bool connectionExists = result.OfType<Connection>().Any(c =>
                        (c.Class1?.Name == currentClass.Name && c.Class2?.Name == baseName) ||
                        (c.Class2?.Name == currentClass.Name && c.Class1?.Name == baseName));

                    if (!connectionExists)
                    {
                        Connection inheritance = new Connection()
                        {
                            Class1 = baseClass,
                            Class2 = currentClass
                        };

                        if (baseClass.Constraint == ConstraintType.Interface)
                            inheritance.ConnectionType = new ConnectionType(7, "Реализация");
                        else
                            inheritance.ConnectionType = new ConnectionType(4, "Наследование");

                        result.Add(inheritance);
                    }
                    break;

                case GeneratorTask.Agregation:
                    isCollection = true;
                    break;

                default:
                    break;
            }
        }
    }
}
