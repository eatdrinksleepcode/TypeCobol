﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeCobol.Compiler.Types;

using Type = TypeCobol.Compiler.Types.Type;

namespace TypeCobol.Test.Types
{
    [TestClass]
    public class PictureStringUnitTest
    {
        private static IDictionary<char, PictureValidator.CurrencyDescriptor> CurrencyDescriptor(char symbol) => CurrencyDescriptor(symbol, symbol.ToString());

        private static IDictionary<char, PictureValidator.CurrencyDescriptor> CurrencyDescriptor(char symbol, string sign)
        {
            return new Dictionary<char, PictureValidator.CurrencyDescriptor>()
                   {
                       { symbol, new PictureValidator.CurrencyDescriptor(symbol, sign) }
                   };
        }

        [TestMethod]
        public void PictureStringRegExpValidationTest()
        {
            PictureValidator.Result result = (new PictureValidator("$99(45)99.99CRV")).Validate(out _);
            Assert.IsTrue(!result.IsValid);

            //Numeric
            string[] numerics = { "9999",
                "S99",
                "S999V9",
                "PPP999",
                "S999PPP",
            };
            for (int i = 0; i < numerics.Length; i++)
            {
                result = (new PictureValidator(numerics[i])).Validate(out _);
                Assert.IsTrue(result.IsValid);
            }


            string[] valids = { "ZZZZ.99",
                        "ZZZZ.99",
                        "++++.99",
                        "Z,ZZZ.99",
                        "$,$$$.99",
                        "$,$$9.99",
                        "$$,$$$.99",
                        "$$9,999.99",
                        "$$$$,$$$.99",
                        "$$$,$$$.$$",
                        "++++.99",
                        "$***,***.99+",
                        "Z,ZZZ.99+",
                        "$,$$$,$$$.99DB"};
            for (int i = 0; i < valids.Length; i++)
            {
                result = (new PictureValidator(valids[i])).Validate(out _);
                Assert.IsTrue(result.IsValid);
            }

            Tuple<string, int, int, bool>[] pics = {
                new Tuple<string, int, int, bool>("9(6)V99", 6, /*whole numbers and*/ 2  /*decimal places*/, false),
            new Tuple<string, int, int, bool>("V999", 0, 3 /*decimal places*/, false),
            new Tuple<string, int, int, bool>("9(4)V9(4)", 4, 4, false),
            new Tuple<string, int, int, bool>("9(5)", 5,  0, false),
            new Tuple<string, int, int, bool>("S9(5)", 5, 0, true /*the sign is remembered*/),
            new Tuple<string, int, int, bool>("S9(4)V9", 4, 1, true),
            new Tuple<string, int, int, bool>("S99V999", 2, 3, true),
            new Tuple<string, int, int, bool>("S999", 3, 0, true)
            };

            for (int i = 0; i < pics.Length; i++)
            {
                result = (new PictureValidator(pics[i].Item1)).Validate(out _);
                Assert.IsTrue(result.IsValid);
                Assert.AreEqual(pics[i].Item2, result.RealDigits - result.Scale);
                Assert.AreEqual(pics[i].Item3, result.Scale);
                Assert.AreEqual(pics[i].Item4, result.IsSigned);
            }

            string[] ifp_pics =
            {
                "9(5)-",
                "9(5)+",
                "-ZZ9.99",
                "-ZZ9.99",
                "Z,ZZ9-",
                "Z,ZZ9-",
                "Z,ZZ9-",
                "Z,ZZ9-",
                "+ZZ,ZZ9",
                "+ZZ,ZZ9",
                "+Z,ZZ9.99",
                "+Z,ZZZ.99",
                "--,--9.99",
                "--,--9.99",
                "--,---.99",
                "--,---.99",
                "+++,+++.99",
                "+++,+++.99",
                "Z,ZZ9CR",
                "Z,ZZ9.99CR",
                "Z,ZZ9.99DB",
                "Z,ZZ9.99DB",
                "+,+++,999.99",
                "++,+++,+++.+++"
            };

            for (int i = 0; i < ifp_pics.Length; i++)
            {
                result = (new PictureValidator(ifp_pics[i])).Validate(out _);
                Assert.IsTrue(result.IsValid);
            }

            string ex_float_pic = "-9v9(9)E-99";
            result = (new PictureValidator(ex_float_pic)).Validate(out _);
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.Category == PictureCategory.ExternalFloatingPoint);

            ex_float_pic = "-99(9).E-99";
            result = (new PictureValidator(ex_float_pic)).Validate(out _);
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.Category == PictureCategory.ExternalFloatingPoint);

            ex_float_pic = "+VE-99";
            result = (new PictureValidator(ex_float_pic)).Validate(out _);
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.Category == PictureCategory.ExternalFloatingPoint);
        }

        /// <summary>
        /// Tests from
        /// https://www.ibm.com/support/knowledgecenter/en/SSAE4W_9.1.0/com.ibm.etools.iseries.langref.doc/c0925395267.htm
        /// </summary>
        [TestMethod]
        public void PictureSimpleInsertionTest()
        {
            Tuple<string,int>[] pics =
            {
                new Tuple<string,int>("X(10)/XX", 13),
                new Tuple<string,int>("X(5)BX(7)", 13),
                new Tuple<string,int>("99,B999,B000", 12),
                new Tuple<string,int>("99,999", 6),
                new Tuple<string,int>("GGBBGG", 12)
            };
            for (int i = 0; i < pics.Length; i++)
            {
                PictureValidator psv = new PictureValidator(pics[i].Item1);
                PictureValidator.Result result = psv.Validate(out _);
                Assert.IsTrue(result.IsValid);
                PictureType type = new PictureType(result, psv.IsSeparateSign);
                int len = type.Length;
                Assert.AreEqual(pics[i].Item2, len);
            }
        }

        /// <summary>
        /// Tests from
        /// https://www.ibm.com/support/knowledgecenter/en/SSAE4W_9.1.0/com.ibm.etools.iseries.langref.doc/c0925395267.htm
        /// </summary>
        [TestMethod]
        public void PictureSpecialInsertionEditingTest()
        {
            Tuple<string, int>[] pics =
            {
                new Tuple<string,int>("999.99", 6),
                new Tuple<string,int>("+999.99E+99", 11),
            };
            for (int i = 0; i < pics.Length; i++)
            {
                PictureValidator psv = new PictureValidator(pics[i].Item1);
                PictureValidator.Result result = psv.Validate(out _);
                Assert.IsTrue(result.IsValid);
                PictureType type = new PictureType(result, psv.IsSeparateSign);
                int len = type.Length;
                Assert.AreEqual(pics[i].Item2, len);
            }
        }

        /// <summary>
        /// Tests from
        /// https://www.ibm.com/support/knowledgecenter/en/SSAE4W_9.1.0/com.ibm.etools.iseries.langref.doc/c0925395267.htm
        /// </summary>
        [TestMethod]
        public void PictureZeroSuppressionAndReplacementEditingTest()
        {
            Tuple<string, int>[] pics =
            {
                new Tuple<string,int>("****.**", 7),
                new Tuple<string,int>("*,***.**+", 9),
                new Tuple<string,int>("$Z,ZZZ,ZZZ.ZZCR", 15),
                new Tuple<string,int>("$B*,***,***.**BBDB", 18),
            };
            for (int i = 0; i < pics.Length; i++)
            {
                PictureValidator psv = new PictureValidator(pics[i].Item1);
                PictureValidator.Result result = psv.Validate(out _);
                Assert.IsTrue(result.IsValid);
                PictureType type = new PictureType(result, psv.IsSeparateSign);
                int len = type.Length;
                Assert.AreEqual(pics[i].Item2, len);
            }
        }

        /// <summary>
        /// Test from
        /// http://www.3480-3590-data-conversion.com/article-packed-fields.html
        /// </summary>
        [TestMethod]
        public void PictureWithUsageTest()
        {
            Tuple<string, Type.UsageFormat, int>[] pics =
            {
                new Tuple<string,Type.UsageFormat,int>("S9(6)V99", Type.UsageFormat.None, 8),
                new Tuple<string,Type.UsageFormat,int>("S9(6)V99", Type.UsageFormat.Comp3, 5),
                new Tuple<string,Type.UsageFormat,int>("S9(7)", Type.UsageFormat.Comp3, 4),
                new Tuple<string,Type.UsageFormat,int>("S9(5)V99", Type.UsageFormat.Comp3, 4),
                new Tuple<string,Type.UsageFormat,int>("S9(6)", Type.UsageFormat.Comp3, 4),
                new Tuple<string,Type.UsageFormat,int>("9(7)", Type.UsageFormat.Comp3, 4),
                new Tuple<string,Type.UsageFormat,int>("9(6)", Type.UsageFormat.Comp3, 4),
            };
            for (int i = 0; i < pics.Length; i++)
            {
                PictureValidator psv = new PictureValidator(pics[i].Item1);
                PictureValidator.Result result = psv.Validate(out _);
                Assert.IsTrue(result.IsValid);
                PictureType type = new PictureType(result, psv.IsSeparateSign);
                type.Usage = pics[i].Item2;
                int len = type.Length;
                Assert.AreEqual(pics[i].Item3, len);
            }
        }

        /// <summary>
        /// Test invalid syntax of picture strings
        /// </summary>
        [TestMethod]
        public void InvalidSyntaxPictureStringTest()
        {
            string[] invalids = {
                "",
                "()",
                "9(",
                "9()",
                "9)(2)",
                "9(2)(4)",
                "()9(2)",
                "9(((2)))",
                "X(0)9(44B",
                "X(0B)9",
                "X(0102)9()X",
                "X(0102",
                "X)0102",
                "(0102)",
                "X(7)B(8)(1)()(0)",
                "b(1)(2)X",
                "9(1)V9(02)()()()()",
                //The following one is invalid because if any symbols after the decimal points are Z, then all the symbols after the decimal point must be Z.
                "ZZZ.Z9",
                "Z,ZZ9CRR",
                "Z,ZZ9.99DBBB",
                "Z,ZZ9.99DBCR",
                "Z,ZZ9.99CRDB",
                "---X(2)"
            };
            for (int i = 0; i < invalids.Length; i++)
            {
                PictureValidator.Result result = (new PictureValidator(invalids[i])).Validate(out _);
                Assert.IsFalse(result.IsValid);
            }

            //Change other currency symbol than $
            PictureValidator.Result result1 = (new PictureValidator("$,$$$.99", currencyDescriptors: CurrencyDescriptor('€'))).Validate(out _);
            Assert.IsFalse(result1.IsValid);

            invalids = new[]
                       {
                           "$v",
                           ",,.",
                           ",/.",
                           "zzzbb+zz-",
                           "aa9b+",
                           "$-***9b",
                           "b9b$",
                           "*9v-99",
                           "9/0*",
                           "$+"
                       };
            for (int i = 0; i < invalids.Length; i++)
            {
                PictureValidator.Result result = (new PictureValidator(invalids[i])).Validate(out _);
                Assert.IsFalse(result.IsValid);
            }
        }

        /// <summary>
        /// Test Picture String Syntax with various Currency Symbols
        /// </summary>
        [TestMethod]
        public void CurrenciesPictureStringTest()
        {
            //EURO
            PictureValidator.Result result = (new PictureValidator("€Z,ZZZ,ZZZ.ZZCR", currencyDescriptors: CurrencyDescriptor('€'))).Validate(out _);
            Assert.IsTrue(result.IsValid);

            //Swiss franc
            result = (new PictureValidator("HZ,ZZZ,ZZZ.ZZCR", currencyDescriptors: CurrencyDescriptor('H', "CHF"))).Validate(out _);
            Assert.IsTrue(result.IsValid);

            //Hong Kong Dollar
            result = (new PictureValidator("hZ,ZZZ,ZZZ.ZZCR", currencyDescriptors: CurrencyDescriptor('h', "HK$"))).Validate(out _);
            Assert.IsTrue(result.IsValid);

            //Mixed
            var dollar = PictureValidator.CurrencyDescriptor.Default;
            var currencyDescriptors = CurrencyDescriptor('€', "EUR");
            currencyDescriptors.Add(dollar.Symbol, dollar);
            result = (new PictureValidator("$€99", currencyDescriptors: currencyDescriptors)).Validate(out var messages);
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(messages.Count == 1);
            Assert.AreEqual("Cannot mix currency symbols in a PICTURE string: '€' symbol was not expected", messages[0]);
        }

        /// <summary>
        /// Test interpretation of DecimalPoint and NumericSeparator chars, based on decimalPointIsComma parameter
        /// </summary>
        [TestMethod]
        public void DecimalPointIsComma()
        {
            PictureValidator.Result result = (new PictureValidator("99.999")).Validate(out _);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(3, result.Scale);

            result = (new PictureValidator("99,999")).Validate(out _);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Scale);

            result = (new PictureValidator("99.999", decimalPointIsComma: true)).Validate(out _);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Scale);

            result = (new PictureValidator("99,999", decimalPointIsComma: true)).Validate(out _);
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(3, result.Scale);
        }

        /// <summary>
        /// Basic type length tests
        /// </summary>
        [TestMethod]
        public void DataItemLength()
        {
            var dollar = PictureValidator.CurrencyDescriptor.Default;
            var currencyDescriptors = CurrencyDescriptor('€', "EUR");
            currencyDescriptors.Add(dollar.Symbol, dollar);
            List<(string, bool, Type.UsageFormat, int)> data = new List<(string, bool, Type.UsageFormat, int)>()
                                                               {
                                                                   ("S99", false, Type.UsageFormat.Display, 2),
                                                                   ("S99", true, Type.UsageFormat.Display, 3),
                                                                   ("99CR", false, Type.UsageFormat.Display, 4),
                                                                   ("99DB", false, Type.UsageFormat.Display, 4),
                                                                   ("+99", false, Type.UsageFormat.Display, 3),
                                                                   ("-99", false, Type.UsageFormat.Display, 3),
                                                                   ("$99", false, Type.UsageFormat.Display, 3),
                                                                   ("$$$$99", false, Type.UsageFormat.Display, 6),
                                                                   ("€99", false, Type.UsageFormat.Display, 5),
                                                                   ("€€€€99", false, Type.UsageFormat.Display, 8),
                                                                   ("GG", false, Type.UsageFormat.Display1, 4),
                                                                   ("NN", false, Type.UsageFormat.National, 4),
                                                                   ("XX", false, Type.UsageFormat.Display, 2),
                                                                   ("X(20)", false, Type.UsageFormat.Display, 20)
                                                               };
            foreach (var item in data)
            {
                string picture = item.Item1;
                bool signIsSeparate = item.Item2;
                Type.UsageFormat usage = item.Item3;
                int expectedLength = item.Item4;

                PictureValidator validator = new PictureValidator(picture, signIsSeparate, false, currencyDescriptors);
                var result = validator.Validate(out _);
                Assert.IsTrue(result.IsValid);

                PictureType type = new PictureType(result, validator.IsSeparateSign) { Usage = usage };
                int actualLength = type.Length;
                Assert.AreEqual(expectedLength, actualLength);
            }
        }

        /// <summary>
        /// Test about Character-string representation
        /// </summary>
        [TestMethod]
        public void PictureRepresentation()
        {
            PictureValidator.Result result = (new PictureValidator("$+")).Validate(out _);
            Assert.IsFalse(result.IsValid);
            result = (new PictureValidator("$$")).Validate(out _);
            Assert.IsTrue(result.IsValid);
            result = (new PictureValidator("$++")).Validate(out _);
            Assert.IsTrue(result.IsValid);
            result = (new PictureValidator("$v")).Validate(out _);
            Assert.IsFalse(result.IsValid);
        }
    }
}
