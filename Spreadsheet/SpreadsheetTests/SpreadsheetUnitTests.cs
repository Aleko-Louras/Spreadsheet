// A test class for an implementation of Abstract Spreadsheet
//
// Quinn Pritchett
// September 2023
//

namespace SpreadsheetTests;

using SS;
using SpreadsheetUtilities;

[TestClass]
public class SpreadsheetUnitTests {
    //################################# PS5 Related Tests ####################################
    public string TestNormalizer(string name) {
        return name.ToUpper();
    }

    public bool TestValidator(string name) {
        return name.ElementAt(0) == 'A';
    }

    [TestMethod]
    public void EmtpyConstructorTest() {
        AbstractSpreadsheet s = new Spreadsheet();
    }

    [TestMethod]
    public void ThreeArgumentConstructorTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
    }

    //[TestMethod]
    //public void FourArgumentConstructorTest() {
    //    AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
    //}

    [TestMethod]
    public void GetVersionTest() {
        AbstractSpreadsheet s = new Spreadsheet();
        Assert.AreEqual("default", s.Version);
    }

    [TestMethod]
    public void SetContentsOfCellDoubleTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "5.0");
    }

    [TestMethod]
    public void SetContentsOfCellInvalidNameTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        Assert.ThrowsException<InvalidNameException> (() => s.SetContentsOfCell("B1", "5.0"));
    }

    [TestMethod]
    public void SetContentsOfCellInvalidFormulaVariableTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        Assert.ThrowsException<FormulaFormatException>(() => s.SetContentsOfCell("A1", "=A1 + B1"));
    }

    [TestMethod]
    public void GetCellValueDoubleTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "4.5");
        Assert.AreEqual(4.5, (double)s.GetCellValue("A1"), 1e-9);
    }

    [TestMethod]
    public void GetCellValueStringTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "a good day");
        Assert.AreEqual("a good day", s.GetCellValue("A1"));
    }

    [TestMethod]
    public void GetCellValueFormulaErrorTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "= 5 / 0");
        Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));
    }

    [TestMethod]
    public void SetContentsOfCellEmptyTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "");
        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsFalse(e.MoveNext());
    }

    [TestMethod]
    public void GetCellValueTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "15");
        s.SetContentsOfCell("A2", "10");
        s.SetContentsOfCell("A3", "=A1+A2");
        Assert.AreEqual(25.0, (double)s.GetCellValue("A3"),1e-9 );
    }

    // A formula entered before its variables have values should
    // contain a FormulaError. After the values are added it should
    // contain the correct updated value
    [TestMethod]
    public void PresetFormulaTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "=A2+A3");
        Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));
        s.SetContentsOfCell("A2", "10");
        s.SetContentsOfCell("A3", "15");
        Assert.AreEqual(25.0, (double)s.GetCellValue("A1"), 1e-9);
    }

    [TestMethod]
    public void NestedUpdatesTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "5");
        Assert.AreEqual(5, (double)s.GetCellValue("A1"), 1e-9);
        s.SetContentsOfCell("A2", "= A1 + 1");
        s.SetContentsOfCell("A3", "= A2 + 1");
        s.SetContentsOfCell("A4", "= A3 + 1");
        Assert.AreEqual(8, (double)s.GetCellValue("A4"), 1e-9);
        s.SetContentsOfCell("A1", "6");
        Assert.AreEqual(9, (double)s.GetCellValue("A4"), 1e-9);
    }

    // Testing Json serilization of a double, formula and string cell
    [TestMethod]
    public void JsonTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "5");
        s.SetContentsOfCell("A2", "=A1+2");
        s.SetContentsOfCell("A3", "My great string cell");
        s.Save("threecells.json");
    }
    [TestMethod]
    public void EmptySpreadsheetJsonTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.Save("empty.json");
    }

    [TestMethod]
    public void FourArgumentConstructorTest() {
        AbstractSpreadsheet s = new Spreadsheet("mycells.json", TestValidator, TestNormalizer, "default");
        s.Save("test.json");
    }

    [TestMethod]
    public void MismatchedVersionsThrowsTest() {
        Assert.ThrowsException<SpreadsheetReadWriteException>(() => new Spreadsheet("mycells.json", TestValidator, TestNormalizer, "wrong_version"));
    }

    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void InvalidVariableNamesThrowsTest() {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "5");
        s.SetContentsOfCell("B2", "=A1+2");
        s.Save("bad_names.json");
        s =  new Spreadsheet("bad_names.json", TestValidator, TestNormalizer, "default");
    }

    [TestMethod]
    public void SheetHasCircularDependencyThrowsTest() {
        
        Assert.ThrowsException<SpreadsheetReadWriteException>(() => new Spreadsheet("circular_sheet.json", TestValidator, TestNormalizer, "default"));
    }


    ///// <summary>
    ///// If a circular dependency is created by SetContentsOfCell
    ///// it should throw a Circular Exception
    ///// </summary>
    //[TestMethod]
    //public void CreateCircularDependency() {
    //    Spreadsheet s = new Spreadsheet();
    //    s.SetContentsOfCell("A1", "=B1");
    //    Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("B1", "=A1"));

    //}

}
