namespace FormulaTests;
using Newtonsoft.Json.Linq;

using System.Runtime.InteropServices;
using SpreadsheetUtilities;
using System.ComponentModel.DataAnnotations;

[TestClass]
public class FormulaTests {

    // Validator
    static bool V (string v) {
        return true;
    }

    // Normalizer
    static string N(string v) {
        return v.ToUpper();
    }

    // ############### FormulaFormatException Syntax Checking #########################

    // Even if the Validator says a variable is valid
    // the constructor should stil fail if variable is illegal 
    [TestMethod]
    public void FormatExeptionBadValidator() {

        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("^$", N, V); });

        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("@"); });


    }

    // Only valid tokens should be in the formula
    [TestMethod]
    public void ParsingRule() {

        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("(16 + 17) + %%"); });
        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("$A1 + $A2");
        });

    }

    // There must be at least one token.
    [TestMethod]
    public void OneTokenRule() {

        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula(""); });

    }

    // There must be at least one token, even if Normalizer returns
    // an empty string.
    [TestMethod]
    public void NormalizerReturnsEmptyStringOneTokenRule() {

        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("A1", s => "", s => true); });

    }

    // When reading tokens from left to right, at no point should the number of
    // closing parentheses seen so far be greater than the number of opening
    // parentheses seen so far.
    [TestMethod]
    public void RightParenthesesRule() {

        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("())"); });
        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula(")"); });
        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("(()))");
        });

    }

    // The total number of opening parentheses must equal the total number of
    // closing parentheses.
    [TestMethod]
    public void BalancedParenthesesRule() {

        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("(((5))"); });
        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("(5)))");
        });

    }

    // The first token of an expression must be a number, a variable,
    // or an opening parenthesis.
    [TestMethod]
    public void StartingTokenRule() {
        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula(")"); });
        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("$A!");
        });
    }

    // The last token of an expression must be a number, a variable, or a closing parenthesis.
    [TestMethod]
    public void EndingTokenRule() {

        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("(+"); });

    }

    // Any token that immediately follows an opening parenthesis or an operator
    // must be either a number, a variable, or an opening parenthesis.
    [TestMethod]
    public void ParenFollowingRule() {

        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("(+");
        });
        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("(+11");
        });
        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("( 1 * $)");
        });

    }

    // Any token that immediately follows a number, a variable, or a closing
    // parenthesis must be either an operator or a closing parenthesis.
    [TestMethod]
    public void ExtraFollowingRule() {

        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("4("); });
        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("A1 E"); });
        Assert.ThrowsException<FormulaFormatException>(() => {
            Formula f = new Formula("(1+1) E");
        });

    }

    // ################### Floating Point Math Tests ##############################
    [TestMethod]
    public void SimpleAddition() {
        Formula f = new Formula("1.0 + 1.0");
        Assert.AreEqual(2D, f.Evaluate( s => 0));
    }

    [TestMethod]
    public void Subtraction() {
        Formula f = new Formula("5.6-3.6");
        Assert.AreEqual(2.0, (double)f.Evaluate(v => 0), 1e-9);
    }

    [TestMethod]
    public void DivideByZero() {
        Formula f = new Formula("5 / (3 - 2 - 1)");
        Assert.IsInstanceOfType(f.Evaluate(v => 0), typeof(FormulaError));
    }


    // ################### Formula Method Tests ####################################

    // A Formula with no Variables should return no variables
    [TestMethod]
    public void GetNoVariablesTest() {
        Formula f = new Formula("1");
        IEnumerator<string> e = f.GetVariables().GetEnumerator();
        Assert.IsFalse(e.MoveNext());
    }

    [TestMethod]
    public void GetOneVariableTest() {
        Formula f = new Formula("A1");
        IEnumerator<string> e = f.GetVariables().GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        string s1 = e.Current;
        Assert.AreEqual("A1", s1);
    }

    [TestMethod]
    public void GetVariablesNoNormalizationTest() {
        Formula f = new Formula("x+X*z");
        IEnumerator<string> e = f.GetVariables().GetEnumerator();

        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s3 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "x") && (s2 == "X")) && (s3 == "z"));
    }

    // Should enumerate X,Y,Z
    [TestMethod]
    public void GetVariablesWithNormalizationTest() {
        Formula f = new Formula("x+y*z", N, s => true);
        IEnumerator<string> e = f.GetVariables().GetEnumerator();

        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s3 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "X") && (s2 == "Y")) && (s3 == "Z"));
    }

    // GetVariables should contain no duplicate variables 
    [TestMethod]
    public void GetVariablesWithDuplicationNormalizationTest() {
        Formula f = new Formula("x+X*z", N, s => true);
        IEnumerator<string> e = f.GetVariables().GetEnumerator();

        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "X") && (s2 == "Z")));
    }

    [TestMethod]
    public void BadVariableTest() {

        Assert.ThrowsException<FormulaFormatException>(() => { Formula f = new Formula("1+$^+1"); });
    }

    [TestMethod]
    public void ToStringTest() {
        Formula f = new Formula("1+1");
        string result = "1+1";

        Assert.IsTrue(result.Equals(f.ToString()));
    }

    [TestMethod]
    public void ToStringWithVariableTest() {
        Formula f = new Formula("1+A1", N, s => true);
        string result = "1+A1";

        Assert.IsTrue(result.Equals(f.ToString()));
    }

    [TestMethod]
    public void ToStringWithNonNormalizedVariableTest() {
        Formula f = new Formula("1+a1", N, s => true);
        string result = "1+A1";

        Assert.IsTrue(result.Equals(f.ToString()));
    }

    [TestMethod]
    public void EqualsTest() {

        Assert.IsTrue(new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")));
        Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("X1+Y2")));
        Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("y2+x1")));
        Formula f = new Formula("2.0 + x7");
        Formula g = new Formula("2.000 + x7");
        Assert.IsTrue(new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")));
 
    }

    [TestMethod]
    public void NotFormulaObjectNotEqual() {

        Formula f = new Formula("1");
        string s = "1";

        Assert.IsFalse(f.Equals(s));

    }

    [TestMethod]
    public void OperatorEqualsTest() {

        Assert.IsTrue(new Formula("x1+y2", N, s => true) == (new Formula("X1  +  Y2")));
        Assert.IsFalse(new Formula("x1+y2") == (new Formula("X1+Y2")));
        Assert.IsFalse(new Formula("x1+y2") == (new Formula("y2+x1")));
        Formula f = new Formula("2.0 + x7");
        Formula g = new Formula("2.000 + x7");
        Assert.IsTrue(new Formula("2.0 + x7") == (new Formula("2.000 + x7")));
    }

    [TestMethod]
    public void OperatorNotEqulasTest() {
        Assert.IsTrue(new Formula("x1+y2") != (new Formula("X1+Y2")));
        Assert.IsTrue(new Formula("x1+y2") != (new Formula("y2+x1")));
        Formula f = new Formula("2.0 + x7");
        Formula g = new Formula("2.000 + x7");
        Assert.IsFalse(new Formula("2.0 + x7") != (new Formula("2.000 + x7")));
    }

    [TestMethod]
    public void IdenticalFormulasHaveSameHashCodeTest() {
        Formula f = new Formula("2.0 + x7");
        Formula g = new Formula("2.0 + x7");
        Assert.IsTrue(f == g);

        int fHashCode = f.GetHashCode();
        int gHashCode = g.GetHashCode();


        Assert.IsTrue(fHashCode == gHashCode);
    }

    [TestMethod]
    public void EquivalentFormulasHaveSameHashCodeTest() {
        Formula f = new Formula("2.0 + x7");
        Formula g = new Formula("2.000 + x7");
        Assert.IsTrue(f == g);

        int fHashCode = f.GetHashCode();
        int gHashCode = g.GetHashCode();


        Assert.IsTrue(fHashCode == gHashCode);
    }

    [TestMethod]
    public void DifferentFormulasHaveDifferntHashCodeTest() {
        Formula f = new Formula("x + y + z");
        Formula g = new Formula("2*(x+y)");
        Assert.IsFalse(f == g);

        int fHashCode = f.GetHashCode();
        int gHashCode = g.GetHashCode();

        Assert.IsFalse(fHashCode == gHashCode);
    }


    [TestMethod]
    public void EvaluateTest() {

        Formula f = new Formula("1+A1", N, s => true);

        double result = 2;
        object d = f.Evaluate(s => 1);

        Assert.AreEqual(result, d);
    }

    [TestMethod]
    public void ScientificNotation() {
        Formula f = new Formula("1 * 1.23E2");
        Assert.AreEqual(123D, f.Evaluate(v => 0));
    }

    [TestMethod]
    public void VariableReturnsScientificNotation() {

        Formula f = new Formula("1 * A1");
        
        Assert.AreEqual(123D, f.Evaluate(s => 1.23E2));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("1")]
    public void TestSingleNumber() {
        Formula f = new Formula("5");
        Assert.AreEqual( 5D, f.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("2")]
    public void TestSingleVariable() {
        Formula f = new Formula("X5");
        Assert.AreEqual(13D, f.Evaluate(s => 13));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("3")]
    public void TestAddition() {
        Formula f = new Formula("5+3");
        Assert.AreEqual(8D, f.Evaluate( s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("4")]
    public void TestSubtraction() {
        Formula f = new Formula("18-10");
        Assert.AreEqual(8D, f.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("5")]
    public void TestMultiplication() {
        Formula f = new Formula("2*4");
        Assert.AreEqual(8D, f.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("6")]
    public void TestDivision() {
        Formula f = new Formula("16/2");
        Assert.AreEqual(8D, f.Evaluate( s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("7")]
    public void TestArithmeticWithVariable() {
        Formula f = new Formula("2+X1");
        Assert.AreEqual(6D, f.Evaluate( s => 4));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("8")]
    [ExpectedException(typeof(ArgumentException))]
    public void TestUnknownVariable() {
        Formula f = new Formula("2+X1");
        f.Evaluate(s => { throw new ArgumentException("Unknown variable"); });
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("9")]
    public void TestLeftToRight() {
        Formula f = new Formula("2*6+3");
        Assert.AreEqual(15D, f.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("10")]
    public void TestOrderOperations() {
        Formula f = new Formula("2+6*3");
        Assert.AreEqual(20D, f.Evaluate( s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("11")]
    public void TestParenthesesTimes() {
        Formula f = new Formula("(2+6)*3");
        Assert.AreEqual(24D, f.Evaluate( s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("12")]
    public void TestTimesParentheses() {
        Formula f = new Formula("2*(3+5)");
        Assert.AreEqual(16D, f.Evaluate( s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("13")]
    public void TestPlusParentheses() {
        Formula f = new Formula("2+(3+5)");
        Assert.AreEqual(10D, f.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("14")]
    public void TestPlusComplex() {
        Formula f = new Formula("2+(3+5*9)");
        Assert.AreEqual(50D, f.Evaluate( s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("15")]
    public void TestOperatorAfterParens() {
        Formula f = new Formula("(1*1)-2/2");
        Assert.AreEqual(0D, f.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("16")]
    public void TestComplexTimesParentheses() {
        Formula f = new Formula("2+3*(3+5)");
        Assert.AreEqual(26D, f.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("17")]
    public void TestComplexAndParentheses() {
        Formula f = new Formula("2+3*5+(3+4*8)*5+2");
        Assert.AreEqual(194D, f.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("18")]
    public void TestDivideByZero() {
        Formula f = new Formula("5/0");
        Assert.AreEqual(new FormulaError(), f.Evaluate(s => 0));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("19")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestSingleOperator() {
        Formula f = new Formula("+");
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("20")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestExtraOperator() {
        Formula f = new Formula("2+5+");
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("21")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestExtraParentheses() {
        Formula f = new Formula("2+5*7)");
        f.Evaluate(s => 0);
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("22")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestInvalidVariable() {
        Formula f = new Formula("$x$");
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("23")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestPlusInvalidVariable() {
        Formula f = new Formula("5 + $x$");
        f.Evaluate(s => 0); ;
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("24")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestParensNoOperator() {
        Formula f = new Formula("5+7+(5)8");
        f.Evaluate(s => 0);
    }


    [TestMethod(), Timeout(5000)]
    [TestCategory("25")]
    [ExpectedException(typeof(FormulaFormatException))]
    public void TestEmpty() {
        Formula f = new Formula("");
        f.Evaluate(s => 0);
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("26")]
    public void TestComplexMultiVar() {
        Formula f = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
        Assert.AreEqual(36/7D, (double)f.Evaluate(s => (s == "x7") ? 1 : 4), 1e-9);
    }
    [TestMethod(), Timeout(5000)]
    public void TestComplexMultiVaGetVariables() {
        Formula f = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
        IEnumerator<string> e = f.GetVariables().GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        string s1 = e.Current;
        Assert.AreEqual("y1", s1);
        Assert.IsTrue ( e.MoveNext());
        string s2 = e.Current;
        Assert.AreEqual("x7", s2);
    }


    [TestMethod(), Timeout(5000)]
    [TestCategory("27")]
    public void TestComplexNestedParensRight() {
        Formula f = new Formula("x1+(x2+(x3+(x4+(x5+x6))))");
        Assert.AreEqual(6D, f.Evaluate( s => 1));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("28")]
    public void TestComplexNestedParensLeft() {
        Formula f = new Formula("((((x1+x2)+x3)+x4)+x5)+x6");
        Assert.AreEqual(12D, f.Evaluate(s => 2));
    }

    [TestMethod(), Timeout(5000)]
    [TestCategory("29")]
    public void TestRepeatedVar() {
        Formula f = new Formula("a4-a4*a4/a4");
        Assert.AreEqual(0D, f.Evaluate(s => 3));
    }

    [TestMethod(), Timeout(5000)]
    public void FormulaFormatEceptions() {

        static bool Validator1(string s) {
            return Char.IsLower(s[0]);
        }
        static string Normalizer1(string s) {
            return s.ToLower();
        }
        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("1 / $^"); });
        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("1 / 1_a"); });
        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("1 / _1", Normalizer1, Validator1); });
   
        Assert.ThrowsException<FormulaFormatException>(delegate { Formula f = new Formula("5 + 5 + A1", s => s, s => false); });
    }



}
