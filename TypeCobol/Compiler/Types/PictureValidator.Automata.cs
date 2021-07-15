using System;
using System.Collections.Generic;

namespace TypeCobol.Compiler.Types
{
    public partial class PictureValidator
    {
        private class Automata
        {
            /// <summary>
            /// Total count of special characters. Size of the automata alphabet.
            /// </summary>
            private static readonly int _SpecialCharactersCount = Enum.GetValues(typeof(SC)).Length;

            /// <summary>
            /// Non Floating Insertion Symbol Column
            /// </summary>
            private const int NFIS_COLUMN = 0;
            private const int NFIS_CR_COLUMN = 9;
            private const int NFIS_DB_COLUMN = 10;
            private const int NFIS_CS_COLUMN = 11;
            private const int NFIS_E_COLUMN = 12;
            /// <summary>
            /// Floating Insertion Symbol Column
            /// </summary>
            private const int FIS_COLUMN = 13;
            private const int FIS_PLUS_MINUS_COLUMN = FIS_COLUMN + 4;
            private const int FIS_CS_COLUMN = FIS_COLUMN + 8;
            /// <summary>
            /// Other Symbol Column
            /// </summary>
            private const int OS_COLUMN = 23;
            private const int OS_G_COLUMN = 30;
            private const int OS_N_COLUMN = 31;

            /// <summary>
            /// Special Symbols Row in the States
            /// </summary>
            private static readonly SC[] SCRow ={ SC.B, SC.ZERO, SC.SLASH, SC.COMMA, SC.DOT, SC.PLUS, SC.MINUS, SC.PLUS, SC.MINUS, SC.CR, SC.DB, SC.CS,SC.E,
                       SC.Z, SC.STAR, SC.Z, SC.STAR, SC.PLUS, SC.MINUS, SC.PLUS, SC.MINUS, SC.CS, SC.CS,
                       SC.NINE, SC.A, SC.X, SC.S, SC.V, SC.P, SC.P, SC.G, SC.N};

            /// <summary>
            /// States of the automata. Each state lists all allowed transitions from one SC to another.
            /// </summary>
            private static readonly bool[][] _States =
            {
                //State 0: Start Symbols
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, true/*.*/, true/*+*/, true/*-*/, true/*+*/, true/*-*/, true/*CR*/, true/*DB*/, true/*CS*/,true/*E*/,
                       true/*Z*/, true/* * */, true/*Z*/, true/* * */, true/*+*/, true/*-*/, true/*+*/, true/*-*/, true/*CS*/, true/*CS*/,
                       true/*9*/, true/*A*/, true/*X*/, true/*S*/, true/*V*/, true/*P*/, true/*P*/, true/*G*/, true/*N)*/
                },

                //------------------------------------------------------
                // NON FLOATING INSERTION SYMBOLS
                //------------------------------------------------------
                //State 1: --B-->(1)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, true/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/, false/*E*/,
                       true/*Z*/, true/* * */, true/*Z*/, true/* * */, true/*+*/, true/*-*/, true/*+*/, true/*-*/, true/*CS*/, true/*CS*/,
                       true/*9*/, true/*A*/, true/*X*/, false/*S*/, true/*V*/, false/*P*/, true/*P*/, true/*G*/, true/*N)*/
                },
                //State 2: --[0|/]-->(2)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, true/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/, false/*E*/,
                       true/*Z*/, true/* * */, true/*Z*/, true/* * */, true/*+*/, true/*-*/, true/*+*/, true/*-*/, true/*CS*/, true/*CS*/,
                       true/*9*/, true/*A*/, true/*X*/, false/*S*/, true/*V*/, false/*P*/, true/*P*/, false/*G*/, true/*N)*/
                },
                //State 3: --,-->(3)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, true/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/, false/*E*/,
                       true/*Z*/, true/* * */, true/*Z*/, true/* * */, true/*+*/, true/*-*/, true/*+*/, true/*-*/, true/*CS*/, true/*CS*/,
                       true/*9*/, false/*A*/, false/*X*/, false/*S*/, true/*V*/, false/*P*/, true/*P*/, false/*G*/, false/*N)*/
                },
                //State 4: --.-->(4)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, false/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/, false/*E*/,
                       true/*Z*/, true/* * */, false/*Z*/, false/* * */, true/*+*/, true/*-*/, false/*+*/, false/*-*/, true/*CS*/, false/*CS*/,
                       true/*9*/, false/*A*/, false/*X*/, false/*S*/, false/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },
                //State 5: --[+|-]-->(5)
                new bool[]{ false/*B*/, false/*0*/, false/* / */, false/*,*/, false/*.*/, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, false/*CS*/, false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, false/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },
                //State 6: --[+|-]-->(6)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, true/*.*/, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/,true/*E*/,
                       true/*Z*/, true/* * */, true/*Z*/, true/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, true/*CS*/, true/*CS*/,
                       true/*9*/, false/*A*/, false/*X*/, false/*S*/, true/*V*/, true/*P*/, true/*P*/, false/*G*/, false/*N)*/
                },
                //State : --[CR|DB]-->()
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, true/*.*/, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/,false/*E*/,
                       true/*Z*/, true/* * */, true/*Z*/, true/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, true/*CS*/, true/*CS*/,
                       true/*9*/, false/*A*/, false/*X*/, false/*S*/, true/*V*/, true/*P*/, true/*P*/, false/*G*/, false/*N)*/
                },
                //State : --CS-->(8)
                new bool[]{ false/*B*/, false/*0*/, false/* / */, false/*,*/, false/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, false/*CS*/, false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, false/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },

                //State : --E-->(9)
                new bool[]{ false/*B*/, false/*0*/, false/* / */, true/*,*/, true/*.*/, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, false/*CS*/, false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       true/*9*/, false/*A*/, false/*X*/, false/*S*/, true/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },

                //------------------------------------------------------
                //FLOATING INSERTION SYMBOLS
                //------------------------------------------------------
                //State 10: --[Z|*]-->(10)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, false/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/,false/*E*/,
                       true/*Z*/, true/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, false/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },
                //State 11: --[Z|*]-->(11)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, true/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/,false/*E*/,
                       true/*Z*/, true/* * */, true/*Z*/, true/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, true/*V*/, false/*P*/, true/*P*/, false/*G*/, false/*N)*/
                },
                //State 12: --[+|-]-->(12)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, false/*.*/, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/,false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, true/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },
                //State 13: --[+|-]-->(13)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, true/*.*/, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/,false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, true/*+*/, true/*-*/, true/*+*/, true/*-*/, false/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, true/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },
                //State 14: --CS-->(14)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, false/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, false/*CS*/,false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, true/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, false/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },
                //State 15: --CS-->(15)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, true/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, false/*CS*/,false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, true/*CS*/, true/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, true/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },

                //------------------------------------------------------
                //OTHER SYMBOLS
                //------------------------------------------------------
                //State 16: --9-->(16)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, true/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/,false/*E*/,
                       true/*Z*/, true/* * */, false/*Z*/, false/* * */, true/*+*/, true/*-*/, false/*+*/, false/*-*/, true/*CS*/, false/*CS*/,
                       true/*9*/, true/*A*/, true/*X*/, true/*S*/, true/*V*/, false/*P*/, true/*P*/, false/*G*/, false/*N)*/
                },
                //State 17: --[A|X]-->(17)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, false/*,*/, false/*.*/, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, false/*CS*/,false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       true/*9*/, true/*A*/, true/*X*/, false/*S*/, false/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },

                //State 18: --S-->(18)
                new bool[]{ false/*B*/, false/*0*/, false/* / */, false/*,*/, false/*.*/, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, false/*CS*/,false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, false/*V*/, false/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },
                //State 19: --V-->(19)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, false/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/,false/*E*/,
                       true/*Z*/, true/* * */, false/*Z*/, false/* * */, true/*+*/, true/*-*/, false/*+*/, false/*-*/, true/*CS*/, false/*CS*/,
                       true/*9*/, false/*A*/, false/*X*/, true/*S*/, false/*V*/, true/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },
                //State 20: --P-->(20)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, true/*,*/, false/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/,false/*E*/,
                       true/*Z*/, true/* * */, false/*Z*/, false/* * */, true/*+*/, true/*-*/, false/*+*/, false/*-*/, true/*CS*/, false/*CS*/,
                       true/*9*/, false/*A*/, false/*X*/, true/*S*/, false/*V*/, true/*P*/, false/*P*/, false/*G*/, false/*N)*/
                },
                //State 21: --P-->(21)
                new bool[]{ false/*B*/, false/*0*/, false/* / */, false/*,*/, false/*.*/, true/*+*/, true/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, true/*CS*/,false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, true/*S*/, true/*V*/, false/*P*/, true/*P*/, false/*G*/, false/*N)*/
                },
                //State 22: --G-->(22)
                new bool[]{ true/*B*/, false/*0*/, false/* / */, false/*,*/, false/*.*/, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, false/*CS*/,false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, false/*V*/, false/*P*/, false/*P*/, true/*G*/, false/*N)*/
                },
                //State 23: --N-->(23)
                new bool[]{ true/*B*/, true/*0*/, true/* / */, false/*,*/, false/*.*/, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CR*/, false/*DB*/, false/*CS*/,false/*E*/,
                       false/*Z*/, false/* * */, false/*Z*/, false/* * */, false/*+*/, false/*-*/, false/*+*/, false/*-*/, false/*CS*/, false/*CS*/,
                       false/*9*/, false/*A*/, false/*X*/, false/*S*/, false/*V*/, false/*P*/, false/*P*/, false/*G*/, true/*N)*/
                },
            };

            private readonly PictureValidator _validator;

            //run private data
            private Character[] _sequence;
            private int _firstFloatingIndex;
            private int _lastFloatingIndex;
            private int _sequenceIndex;
            private bool _digitsSeenBeforeP;
            private bool _isBeforeDecimalPoint;

            //run output
            public PictureCategory Category { get; private set; }
            public int Digits { get; private set; }
            public int RealDigits { get; private set; }
            public bool IsSigned { get; private set; }
            public int Scale { get; private set; }
            public int Size { get; private set; }

            public Automata(PictureValidator validator)
            {
                _validator = validator;
            }

            private bool CheckTransition(int state, SC c, int index, bool[] PState, bool Z_seen_Star_seen, bool vSeen)
            {
                int column = -1;
                switch(c)
                {
                    case SC.B:
                    case SC.ZERO:
                    case SC.SLASH:
                    case SC.COMMA:
                    case SC.DOT:
                        column = (int)c;
                            break;
                    case SC.PLUS:
                    case SC.MINUS:
                        {
                            if (!IsCurrentIndexInsideFloatingInsertion)
                            {

                                if (IsAtFirst(index))
                                    column = NFIS_COLUMN + (int)c;
                                else if (Z_seen_Star_seen)
                                    column = NFIS_COLUMN + (int)c + 2;
                                else
                                    column = NFIS_COLUMN + (int)c;
                            }
                            else
                            {
                                column = FIS_PLUS_MINUS_COLUMN + (int)c - (int)SC.PLUS + (_isBeforeDecimalPoint ? 0 : 2);
                            }                            
                        }
                        break;
                    case SC.CR:
                        column = NFIS_CR_COLUMN;
                        break;
                    case SC.DB:
                        column = NFIS_DB_COLUMN;
                        break;
                    case SC.CS:
                        {
                            if (!IsCurrentIndexInsideFloatingInsertion)
                            {
                                column = NFIS_CS_COLUMN + (!vSeen ? 0 : 1);
                            }
                            else
                            {
                                column = FIS_CS_COLUMN + (_isBeforeDecimalPoint ? 0 : 1);
                            }
                        }
                        break;
                    case SC.E:
                        column = NFIS_E_COLUMN;
                        break;
                    case SC.Z:
                    case SC.STAR:
                        column = FIS_COLUMN + (int)c - (int)SC.Z + (_isBeforeDecimalPoint ? 0 : 2);
                        break;
                    case SC.NINE:
                    case SC.A:
                    case SC.X:
                    case SC.S:
                    case SC.V:
                        column = OS_COLUMN + (int)c - (int)SC.NINE;
                        break;
                    case SC.P:
                        column = OS_COLUMN + (int)c - (int)SC.NINE + (PState[index] ? 0 : 1);
                        break;
                    case SC.G:
                        column = OS_G_COLUMN;
                        break;
                    case SC.N:
                        column = OS_N_COLUMN;
                        break;
                    default:
                        column = (int)c;//Should never happend
                        break;
                }
                return _States[state][column];
            }

            /// <summary>
            /// Determines if the current Sequence Index is inside the Floating Insertion Symbols range.
            /// </summary>
            private bool IsCurrentIndexInsideFloatingInsertion
                => _firstFloatingIndex >= 0
                   && _lastFloatingIndex >= 0
                   && _firstFloatingIndex <= _sequenceIndex
                   && _sequenceIndex <= _lastFloatingIndex;

            /// <summary>
            /// Return true if the current sequence index is the first symbol of the sequence.
            /// </summary>
            private bool IsFirstSymbol => _sequenceIndex == 0;
            private bool IsAtFirst(int index)
            {
                return index == 0;
            }

            /// <summary>
            /// Return true if the current sequence index is the last symbol of the sequence.
            /// </summary>
            private bool IsLastSymbol => _sequenceIndex == _sequence.Length - 1;
            private bool IsAtLast(int index)
            {
                return index == _sequence.Length - 1;
            }

            public bool Run(Character[] sequence, List<string> validationMessages)
            {
                //Initialize run variables
                Initialize(sequence);

                //Some character counts/flags used during validation
                int S_count = 0;               //Count of either CR, DB, + or - symbols
                int V_count = 0;               //Count of V symbol
                bool Z_seen = false;           //Have we seen Z ?
                bool V_seen = false;
                bool Star_seen = false;        //Have we seen '*' ?
                bool CS_signSizeAdded = false; //Flag to detect the first time we encounter a CS symbol during size computation
                //Remeber P state for assumed V
                bool[] PState = new bool[_sequence.Length];

                //Run automata
                int state = 0;
                while (_sequenceIndex < _sequence.Length)
                {
                    Character c = _sequence[_sequenceIndex];
                    if (state == 0 && !CheckTransition(state, c.SpecialChar, _sequenceIndex, PState, Z_seen || Star_seen, V_seen))
                    {   // Check that the first character can start le PICTURE
                        //No transition allowed
                        validationMessages.Add(string.Format(INVALID_SYMBOL_POSITION, _validator.SC2String(c.SpecialChar)));
                        return false;
                    }

                    int nextState = GetState(c, Z_seen || Star_seen);
                    V_seen = V_seen || c.SpecialChar == SC.V;

                    if (state != 0 && !CheckTransition(nextState, sequence[_sequenceIndex - 1].SpecialChar, _sequenceIndex - 1, PState, Z_seen || Star_seen, V_seen))
                    {
                        //No transition allowed
                        validationMessages.Add(string.Format(INVALID_SYMBOL_POSITION, _validator.SC2String(sequence[_sequenceIndex - 1].SpecialChar)));
                        return false;
                    }

                    if (!ValidateState(c))
                    {
                        return false;
                    }
                    PState[_sequenceIndex] = _digitsSeenBeforeP && _isBeforeDecimalPoint;

                    state = nextState;
                    _sequenceIndex++;
                }

                //Successful validation, adjust category then return CheckCategory
                AdjustCategory();
                return CheckCategory();

                //Called when changing transition on the given character, so specific validation actions can be performed here.
                bool ValidateState(Character c)
                {
                    switch (c.SpecialChar)
                    {
                        case SC.B:      //'B'
                        case SC.ZERO:   //'0'
                        case SC.SLASH:  //'/'
                            Category |= PictureCategory.Edited;
                            break;
                        case SC.PLUS:   //'+'
                        case SC.MINUS:  //'-'
                            Category |= PictureCategory.NumericEdited;
                            Digits += c.Count - 1;
                            S_count++;
                            break;
                        case SC.Z:
                        case SC.STAR:
                            if (c.SpecialChar == SC.Z)
                                Z_seen = true;
                            else
                                Star_seen = true;
                            if (Z_seen && Star_seen)
                            {
                                validationMessages.Add(Z_STAR_MUTUALLY_EXCLUSIVE);
                                return false;
                            }
                            if (IsLastSymbol)
                            {// Check that sequence contans only DOT, COMMA or Z or * or CS
                                foreach (var s in _sequence)
                                {
                                    if (s.SpecialChar != SC.B &&
                                        s.SpecialChar != SC.ZERO &&
                                        s.SpecialChar != SC.SLASH &&
                                        s.SpecialChar != SC.COMMA &&
                                        s.SpecialChar != SC.DOT &&
                                        s.SpecialChar != SC.CS &&
                                        s.SpecialChar != SC.Z &&
                                        s.SpecialChar != SC.STAR &&
                                        s.SpecialChar != SC.PLUS &&
                                        s.SpecialChar != SC.MINUS)
                                        return false;
                                }
                            }
                            Category |= PictureCategory.NumericEdited;
                            Digits += c.Count;
                            if (V_count > 0)
                            {
                                Scale += c.Count;
                            }
                            break;
                        case SC.CR:
                        case SC.DB:
                            Category |= PictureCategory.NumericEdited;
                            S_count++;
                            break;
                        case SC.CS:
                            Category |= PictureCategory.NumericEdited;
                            Digits += c.Count - 1;
                            break;
                        case SC.E:
                            Category = PictureCategory.ExternalFloat;
                            break;
                        case SC.A:
                            Category |= PictureCategory.Alphabetic;
                            break;
                        case SC.X:
                            Category |= PictureCategory.AlphaNumeric;
                            break;
                        case SC.NINE://9
                            Category |= PictureCategory.Numeric;
                            Digits += c.Count;
                            RealDigits += c.Count;
                            if (V_count > 0)
                            {
                                //We have seen the decimal point --> digits are in the decimal part
                                Scale += c.Count;
                            }
                            break;
                        case SC.G:
                            Category |= PictureCategory.Dbcs;
                            break;
                        case SC.N:
                            Category |= PictureCategory.National;
                            break;
                        case SC.S:
                            Category |= PictureCategory.Numeric;
                            if (c.Count > 1)
                            {
                                validationMessages.Add(SYMBOL_S_MUST_OCCUR_ONLY_ONCE);
                                return false;
                            }
                            if (state != 0 || _sequenceIndex != 0)
                            {
                                validationMessages.Add(SYMBOL_S_MUST_BE_THE_FIRST);
                                return false;
                            }
                            S_count += c.Count;
                            break;
                        case SC.DOT:
                        case SC.COMMA:
                            if ((c.SpecialChar == SC.DOT && this._validator.DecimalPoint == '.' ||
                                    c.SpecialChar == SC.COMMA && this._validator.DecimalPoint == ',') &&
                                    IsLastSymbol)
                                return false;//The decimal point cannot be the last character                        

                            Category |= PictureCategory.NumericEdited;
                            if (!_validator.IsDecimalPoint(c.SpecialChar))
                                break;
                            goto case SC.V;
                        case SC.V:
                            Category |= PictureCategory.Numeric;
                            V_count += c.Count;
                            if (V_count > 1)
                            {
                                validationMessages.Add(MULTIPLE_V);
                                return false;
                            }
                            _isBeforeDecimalPoint = false;
                            break;
                        case SC.P:
                            Category |= PictureCategory.Numeric;
                            if (!ValidateP())
                                return false;
                            Digits += c.Count;
                            Scale += (V_count > 0 ? c.Count : (-c.Count));
                            break;
                    }

                    if (S_count > 0)
                        IsSigned = true;

                    //Update total size
                    switch (c.SpecialChar)
                    {
                        case SC.S:
                            Size += _validator.IsSeparateSign ? 1 : 0;
                            break;
                        case SC.V:
                        case SC.P:
                            break;
                        case SC.DB:
                        case SC.CR:
                            Size += c.Count * 2;
                            break;
                        case SC.CS:
                            System.Diagnostics.Debug.Assert(_validator._currencyDescriptor != null);
                            if (!CS_signSizeAdded)
                            {
                                //First CS adds the sign length
                                Size += _validator._currencyDescriptor.Sign.Length;
                                CS_signSizeAdded = true;
                                Size += c.Count - 1; //Each subsequent CS counts for 1 character
                            }
                            else
                            {
                                Size += c.Count; //Each subsequent CS counts for 1 character
                            }
                            break;
                        case SC.N:
                        case SC.G:
                            Size += c.Count * 2;
                            break;
                        default:
                            Size += c.Count;
                            break;
                    }

                    return true;
                }

                /*
                 * Validate the position of the P character. 
                   The Symbol P specified a scaling position and implies an assumed decimal point (to the left of the Ps if the Ps are leftmost PICTURE characters,
                   to right of the Ps if the Ps are rightmost PICTURE characters).
                   If we say that the character ^ means the beginning of the PICTURE sequence
                   and $ means the end of the PICTURE sequence sequence, only the following situations are valid for P.
                   ^P | ^VP | ^SP | ^SVP | P$ | PV$
                 */
                bool ValidateP()
                {
                    if (IsFirstSymbol || IsLastSymbol)
                    {
                        V_count += _sequenceIndex == 0 ? 1 : 0; //Assume decimal point symbol V at the beginning
                        return true;//^P | P$;
                    }
                    if (_sequenceIndex == 1 && (_sequence[0].SpecialChar == SC.S || _sequence[0].SpecialChar == SC.V))
                    {
                        V_count += 1;//Assume decimal point symbol V at the beginning
                        return true;//^SP | ^VP
                    }
                    if (_sequenceIndex == 2 && _sequence[0].SpecialChar == SC.S && _sequence[1].SpecialChar == SC.V)
                    {
                        V_count += 1;//Assume decimal point symbol V at the beginning
                        return true;//^SVP
                    }
                    if (_sequenceIndex == _sequence.Length - 2 && _sequence[_sequence.Length - 1].SpecialChar == SC.V)
                    {
                        return true;//$PV
                    }

                    //validation failed
                    validationMessages.Add(WRONG_P_POSITION);
                    return false;
                }
            }

            private void Initialize(Character[] sequence)
            {
                _sequence = sequence;
                _validator.ComputeFloatingStringIndexes(_sequence, out _firstFloatingIndex, out _lastFloatingIndex);
                _sequenceIndex = 0;
                _digitsSeenBeforeP = false;
                _isBeforeDecimalPoint = true;
            }

            /// <summary>
            /// Get the state that is used to handle the given character in the Automata
            /// </summary>
            /// <param name="c">The character to get the handling state</param>
            /// <returns>The state number if one exist, -1 otherwise</returns>
            private int GetState(Character c, bool Z_seen_Star_seen)
            {
                switch (c.SpecialChar)
                {
                    case SC.B:
                        return 1;
                    case SC.ZERO:
                    case SC.SLASH:
                        return 2;
                    case SC.COMMA:
                        return 3;
                    case SC.DOT:
                        return 4;
                    case SC.PLUS:
                    case SC.MINUS:
                        {
                            if (!IsCurrentIndexInsideFloatingInsertion)
                            {
                                if (IsFirstSymbol)
                                    return 5;
                                else if (!_isBeforeDecimalPoint || (IsLastSymbol /*&& Z_seen_Star_seen*/))
                                    return 6;
                                else
                                    return 5;
                            }

                            _digitsSeenBeforeP = true;
                            return _isBeforeDecimalPoint ? 12 : 13;
                        }
                    case SC.CR:
                    case SC.DB:
                        return 7;
                    case SC.CS:
                        {
                            if (!IsCurrentIndexInsideFloatingInsertion)
                            {
                                return 8;
                            }

                            _digitsSeenBeforeP = true;
                            return _isBeforeDecimalPoint ? 14 : 15;
                        }
                    case SC.E:
                        return 9;
                    case SC.Z:
                    case SC.STAR:
                        _digitsSeenBeforeP = true;
                        return _isBeforeDecimalPoint ? 10 : 11;
                    case SC.NINE:
                        _digitsSeenBeforeP = true;
                        return 16;
                    case SC.A:
                    case SC.X:
                        return 17;
                    case SC.S:
                        return 18;
                    case SC.V:
                        return 19;
                    case SC.P:
                        return _digitsSeenBeforeP && _isBeforeDecimalPoint ? 20 : 21;
                    case SC.G:
                        return 22;
                    case SC.N:
                        return 23;
                    default:
                        return -1;//Should never arrive.
                }
            }

            /// <summary>
            /// Perform post-validation adjustment of the computed Category
            /// for special categories ExternalFloat and Dbcs.
            /// </summary>
            private void AdjustCategory()
            {
                if (IsExternalFloatSequence())
                {
                    Category = PictureCategory.ExternalFloat;
                }
                else if (IsDbcsSequence())
                {
                    Category = PictureCategory.Dbcs;
                }

                bool IsExternalFloatSequence()
                {
                    if (_sequence.Length <= 4)
                        return false; //Should contain at least (+|-)*2,(.|V),E
                    if ((Category & PictureCategory.NumericEdited) == 0)
                        return false; //By Default we should end on a NumericEdited category.
                    int i = 0;
                    if (_sequence[i].SpecialChar != SC.PLUS && _sequence[i].SpecialChar != SC.MINUS)
                        return false;
                    int len = _sequence.Length;
                    i++;
                    if (_sequence[i].SpecialChar == SC.DOT || _sequence[i].SpecialChar == SC.V)
                    {
                        if (_sequence[i + 1].SpecialChar != SC.NINE)
                            return false;
                    }
                    else if (_sequence[i].SpecialChar == SC.NINE)
                    {
                        i++;
                        if (_sequence[i].SpecialChar != SC.DOT & _sequence[i].SpecialChar != SC.V)
                            return false;
                        i++;
                    }
                    else
                        return false;
                    if (i >= len || _sequence[i].SpecialChar == SC.NINE)
                        i++;
                    if (i >= len || _sequence[i].SpecialChar != SC.E)
                        return false;
                    if (++i >= len || (_sequence[i].SpecialChar != SC.PLUS && _sequence[i].SpecialChar != SC.MINUS))
                        return false;
                    if (++i >= len || !(_sequence[i].SpecialChar == SC.NINE && _sequence[i].Count == 2))
                        return false;
                    return i == (len - 1);
                }

                bool IsDbcsSequence() => _sequence.Length > 0 && Array.TrueForAll(_sequence, c => c.SpecialChar == SC.G || c.SpecialChar == SC.B);
            }

            /// <summary>
            /// Check that the sequence metches the associated categoty
            /// </summary>
            /// <returns></returns>
            private bool CheckCategory()
            {
                if (this.Category == PictureCategory.ExternalFloat)
                {
                    return true;//Already checked by AdjustCategory
                }
                if (this.Category == PictureCategory.AlphaNumericEdited)
                {
                    return IsAlphaNumericEditedSequence();
                }
                if (this.Category == PictureCategory.NumericEdited)
                {
                    return IsNumericEditedSequence();
                }
                if (this.Category == PictureCategory.AlphaNumeric)
                {
                    return IsAlphanumericSequence();
                }
                if (this.Category == PictureCategory.Alphabetic)
                {
                    return IsAplphabeticSequence();
                }
                if (this.Category == PictureCategory.Numeric)
                {
                    return IsNumericSequence();
                }
                if (this.Category == PictureCategory.NationalEdited)
                {
                    return IsNationalEditedSequence();
                }
                if (this.Category == PictureCategory.National)
                {
                    return IsNationalSequence();
                }
                return true;
            }

            /// <summary>
            /// Determines if the current sequence after a call to Isvalid method, is in fact an ExternalFloat picture string
            /// category.
            /// </summary>
            /// <returns></returns>
            public bool IsExternalFloatSequence()
            {
                if (this._sequence == null)
                    return false;
                if (this._sequence.Length <= 4)
                    return false;// should contained with at leas (+|-)*2,(.|V),E
                if (this.Category != PictureCategory.NumericEdited)
                    return false;//By Defualt is a NumericEdited category.
                int i = 0;
                if (_sequence[i].SpecialChar != SC.PLUS && _sequence[i].SpecialChar != SC.MINUS)
                    return false;
                int len = _sequence.Length;
                i++;
                if (_sequence[i].SpecialChar == SC.DOT || _sequence[i].SpecialChar == SC.V)
                {
                    if (_sequence[i + 1].SpecialChar != SC.NINE)
                        return false;
                }
                else if (_sequence[i].SpecialChar == SC.NINE)
                {
                    i++;
                    if (_sequence[i].SpecialChar != SC.DOT & _sequence[i].SpecialChar != SC.V)
                        return false;
                    i++;
                }
                else
                    return false;
                if (i >= len || _sequence[i].SpecialChar == SC.NINE)
                    i++;
                if (i >= len || _sequence[i].SpecialChar != SC.E)
                    return false;
                if (++i >= len || (_sequence[i].SpecialChar != SC.PLUS && _sequence[i].SpecialChar != SC.MINUS))
                    return false;
                if (++i >= len || !(_sequence[i].SpecialChar == SC.NINE && _sequence[i].Count == 2))
                    return false;
                return i == (len - 1);
            }

            /// <summary>
            /// Determine whether or not the sequence is alphabetic.
            /// </summary>
            /// <returns>true if the sequence is alphabetic, false otherwise</returns>
            public bool IsAplphabeticSequence()
            {
                if (this._sequence == null)
                    return false;
                if (this._sequence.Length == 0)
                    return false;
                return Array.TrueForAll(this._sequence, c => c.SpecialChar == SC.A);
            }

            /// <summary>
            /// Determines if the current sequence is a Dbcs seqeunce
            /// </summary>
            /// <returns>true if yes, false otherwise</returns>
            public bool IsDbcsSequence()
            {
                if (this._sequence == null)
                    return false;
                if (this._sequence.Length == 0)
                    return false;
                return Array.TrueForAll(this._sequence,c => c.SpecialChar == SC.G || c.SpecialChar == SC.B);
            }

            /// <summary>
            /// Determines if the current sequence is a numeric sequence
            /// </summary>
            /// <returns>true if yes, false otherwise</returns>
            public bool IsNumericSequence()
            {
                if (this._sequence == null)
                    return false;
                if (this._sequence.Length == 0)
                    return false;
                return Array.TrueForAll(this._sequence,c => c.SpecialChar == SC.NINE || c.SpecialChar == SC.S || c.SpecialChar == SC.V || c.SpecialChar == SC.P);
            }

            /// <summary>
            /// Determine whether or not the sequence is alphanumeric.
            /// </summary>
            /// <returns>true if the sequence is alphanumeric, false otherwise</returns>
            public bool IsAlphanumericSequence()
            {
                if (this._sequence == null)
                    return false;
                if (this._sequence.Length == 0)
                    return false;
                return Array.TrueForAll(this._sequence,c => c.SpecialChar == SC.NINE || c.SpecialChar == SC.X || c.SpecialChar == SC.A);
            }

            /// <summary>
            /// Determine whether or not the sequence is a numeric-edited item
            /// </summary>
            /// <returns>true if the sequence is numeric-edited, false otherwise</returns>
            public bool IsNumericEditedSequence()
            {
                if (this._sequence == null)
                    return false;
                if (this._sequence.Length == 0)
                    return false;
                return Array.TrueForAll(this._sequence,c =>
                    c.SpecialChar == SC.B ||
                    c.SpecialChar == SC.P ||
                    c.SpecialChar == SC.V ||
                    c.SpecialChar == SC.Z ||
                    c.SpecialChar == SC.NINE ||
                    c.SpecialChar == SC.ZERO ||
                    c.SpecialChar == SC.SLASH ||
                    c.SpecialChar == SC.COMMA ||
                    c.SpecialChar == SC.DOT ||
                    c.SpecialChar == SC.PLUS ||
                    c.SpecialChar == SC.MINUS ||
                    c.SpecialChar == SC.STAR ||
                    c.SpecialChar == SC.CS ||
                    c.SpecialChar == SC.CR ||
                    c.SpecialChar == SC.DB
                );
            }

            /// <summary>
            /// Determine whether or not the sequence is a alphanumeric-edited item
            /// </summary>
            /// <returns>true if the sequence is alphanumeric-edited, false otherwise</returns>
            public bool IsAlphaNumericEditedSequence()
            {
                if (this._sequence == null)
                    return false;
                if (this._sequence.Length == 0)
                    return false;
                return Array.TrueForAll(this._sequence,c =>
                    c.SpecialChar == SC.A ||
                    c.SpecialChar == SC.X ||
                    c.SpecialChar == SC.NINE ||
                    c.SpecialChar == SC.B ||
                    c.SpecialChar == SC.ZERO ||
                    c.SpecialChar == SC.SLASH
                );
            }

            /// <summary>
            /// Is a sequence only formed with national characters that is to say only N.
            /// </summary>
            /// <returns>true if yes, false otherwise</returns>
            public bool IsNationalSequence()
            {
                if (this._sequence == null)
                    return false;
                if (this._sequence.Length == 0)
                    return false;
                return Array.TrueForAll(this._sequence,c => c.SpecialChar == SC.N);
            }

            /// <summary>
            /// Is a sequence only formed with national edited characters, that is to say with
            /// symbols N, B, 0 or /, with at least one N and one other symbol in the picture chracter_string.
            /// </summary>
            /// <returns>true if yes, false otherwise</returns>
            public bool IsNationalEditedSequence()
            {
                if (this._sequence == null)
                    return false;
                if (this._sequence.Length == 0)
                    return false;
                bool hasN = false;
                bool bAll = Array.TrueForAll(this._sequence,c => (hasN |= c.SpecialChar == SC.N) ||
                                c.SpecialChar == SC.B ||
                                c.SpecialChar == SC.ZERO ||
                                c.SpecialChar == SC.SLASH);
                return hasN && bAll;
            }

            /// <summary>
            /// All validation messages if any.
            /// </summary>
            public List<String> ValidationMesssage
            {
                get;
                internal set;
            }

        }
    }
}
