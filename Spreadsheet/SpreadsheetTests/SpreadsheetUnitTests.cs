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

    // A Normalizer for testing
    // All variables will be uppercased a1 -> A1
    public string TestNormalizer(string name) {
        return name.ToUpper();
    }

    // A Validator for testing
    // A variable is valid if it begins with an A
    public bool TestValidator(string name) {
        return name.ElementAt(0) == 'A';
    }

    // Create a spreadsheet with the zero-argument constructor
    [TestMethod]
    public void EmptyConstructorTest() {
        AbstractSpreadsheet s = new Spreadsheet();
    }

    // Create a spreadsheet with the three-argument constructor
    [TestMethod]
    public void ThreeArgumentConstructorTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
    }

    // Create a spreadsheet with the four-argument constructor
    [TestMethod]
    public void FourArgumentConstructorTest() {
        AbstractSpreadsheet ss = new Spreadsheet();
        ss.SetContentsOfCell("A1", "5");
        ss.Save("ss.json");
        AbstractSpreadsheet s = new Spreadsheet("ss.json", TestValidator, TestNormalizer, "default");
    }

    // A spreadsheet created with the zero-argument constructor
    // should have a version of "default" 
    [TestMethod]
    public void GetVersionTest() {
        AbstractSpreadsheet s = new Spreadsheet();
        Assert.AreEqual("default", s.Version);
    }

    // A spreadsheet created with a custom version should
    // return that version
    [TestMethod]
    public void GetCustomVersionTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "custom");

        Assert.AreEqual("custom", s.Version);
    }

    // Setting contents of a cell to a double
    [TestMethod]
    public void SetContentsOfCellDoubleTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "5.0");
        Assert.AreEqual(5.0, (double)s.GetCellContents("A1"), 1e-9);
    }

    // If the variables are invalid, a FormulaFormatException is thrown
    [TestMethod]
    [ExpectedException(typeof(FormulaFormatException))]
    public void SetContentsOfCellInvalidFormulaVariableTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "=A1 + B1");
    }

    // GetCellValue with a double
    [TestMethod]
    public void GetCellValueDoubleTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "4.5");
        Assert.AreEqual(4.5, (double)s.GetCellValue("A1"), 1e-9);
    }

    // Get cell value with a string
    [TestMethod]
    public void GetCellValueStringTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "a good day");
        Assert.AreEqual("a good day", s.GetCellValue("A1"));
    }

    /// <summary>
    /// Get CellValue with a formula that returns a Formula Error
    /// </summary>
    [TestMethod]
    public void GetCellValueFormulaErrorTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "= 5 / 0");
        Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));
    }

    /// <summary>
    /// Set Cell contents to empty should return an empty list
    /// </summary>
    [TestMethod]
    public void SetContentsOfCellEmptyTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "");
        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsFalse(e.MoveNext());
    }

    /// <summary>
    /// Get cell value ofa Cell with a Formula
    /// </summary>
    [TestMethod]
    public void GetCellValueTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "15");
        s.SetContentsOfCell("A2", "10");
        s.SetContentsOfCell("A3", "=A1+A2");
        Assert.AreEqual(25.0, (double)s.GetCellValue("A3"),1e-9 );
    }

    /// <summary>
    /// A formula entered before its variables have values should
    /// contain a FormulaError. After the values are added it should
    /// contain the correct updated value
    /// </summary>

    [TestMethod]
    public void PresetFormulaTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "=A2+A3");
        Assert.IsInstanceOfType(s.GetCellValue("A1"), typeof(FormulaError));
        s.SetContentsOfCell("A2", "10");
        s.SetContentsOfCell("A3", "15");
        Assert.AreEqual(25.0, (double)s.GetCellValue("A1"), 1e-9);
    }

    /// <summary>
    /// Values should update when dependent cell updates
    /// </summary>
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

    /// <summary>
    /// Testing Json serilization of a double, formula and string cell
    /// </summary>
    [TestMethod]
    public void JsonTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.SetContentsOfCell("A1", "5");
        s.SetContentsOfCell("A2", "=A1+2");
        s.SetContentsOfCell("A3", "My great string cell");
        s.Save("threecells.json");
    }

    /// <summary>
    /// If the save path is invalid, a SpreadsheetReadWriteException shouldbe thrown
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void EmptySpreadsheetJsonTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        s.Save("/missing/save.json");
    }

    // Setting the contents of a cell should cause the Changed property to be true
    [TestMethod]
    public void ChangedPropertyTest() {
        AbstractSpreadsheet s = new Spreadsheet(TestValidator, TestNormalizer, "default");
        Assert.IsFalse(s.Changed);
        s.SetContentsOfCell("A1", "1");
        Assert.IsTrue(s.Changed);
    }


    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void MismatchedVersionsThrowsTest() {
        AbstractSpreadsheet s = new Spreadsheet("mycells.json", TestValidator, TestNormalizer, "wrong_version");
    }

    [TestMethod]
    [ExpectedException(typeof(SpreadsheetReadWriteException))]
    public void InvalidVariableNamesThrowsTest() {
        AbstractSpreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "5");
        s.SetContentsOfCell("B2", "=A1+2");
        s.Save("bad_names.json");
        s = new Spreadsheet("bad_names.json", TestValidator, TestNormalizer, "default");
    }


    /// <summary>
    /// If a circular dependency is created by SetContentsOfCell
    /// it should throw a Circular Exception
    /// </summary>
    [TestMethod]
    public void CreateCircularDependency() {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "=B1");
        Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("B1", "=A1"));

    }

    /// <summary>
    /// Creating a new spreadsheet
    /// </summary>
    [TestMethod]
    public void AbstractSpreadsheetTest() {
        AbstractSpreadsheet sheet = new Spreadsheet();
    }

    /// <summary>
    /// A new spreadsheet should have no named cells 
    /// </summary>
    [TestMethod]
    public void EmptySpreadsheetHasNoNamedCellsTest() {
        Spreadsheet s = new Spreadsheet();

        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsFalse(e.MoveNext());
    }

    /// <summary>
    /// If cells are given names, there should be two cells in the
    /// list of nonempty cells 
    /// </summary>
    [TestMethod]
    public void GetNamesOfAllNonemptyCellsTest() {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "1.0");
        s.SetContentsOfCell("A2", "=1+1");

        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        string name1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        string name2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((name1 == "A1") && (name2 == "A2")) ||
                      ((name1 == "A2") && (name2 == "A1")));
    }

    /// <summary>
    /// A test to get doubles out of the spreadsheet
    /// </summary>
    [TestMethod]
    public void GetCellContentsAsNumberTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetContentsOfCell("A2", "2.0");
        double d = 2.0;
        Assert.IsTrue(d.Equals(s.GetCellContents("A2")));

    }

    /// <summary>
    /// A test to get text out of the spreadsheet
    /// </summary>
    [TestMethod]
    public void GetCellContentsAsTextTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetContentsOfCell("A3", "Hello");
        string hello = "Hello";
        Assert.IsTrue(hello.Equals(s.GetCellContents("A3")));

    }

    /// <summary>
    /// A test to get a Formula out of the spreadsheet
    /// </summary>
    [TestMethod]
    public void GetCellContentsAsFormulaTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetContentsOfCell("A1", "= 1 + 1");
        Formula f = new Formula("1+1");
        Assert.IsTrue(f.Equals(s.GetCellContents("A1")));
    }

    /// <summary>
    /// If a cell is empty, it should return an empty string
    /// </summary>
    [TestMethod]
    public void GetCellContentsOfEmptyCellTest() {
        Spreadsheet s = new Spreadsheet();
        string empty = "";
        Assert.IsTrue(empty.Equals(s.GetCellContents("A1")));
    }

    /// <summary>
    /// If a spreadsheet has one cell has a name but is later emptied there
    /// should then be no named cells in the spreadsheet
    /// </summary>
    [TestMethod]
    public void GetCellContentsAfterEmptyingTest() {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "hello");
        Assert.AreEqual("hello", s.GetCellContents("A1"));
        s.SetContentsOfCell("A1", "");
        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsFalse(e.MoveNext());
    }

    /// <summary>
    /// If the cell name is invalid, a InvalidNameException should be thrown
    /// </summary>
    [TestMethod]
    public void GetCellContentsInvalidName() {
        Spreadsheet s = new Spreadsheet();
        Assert.ThrowsException<InvalidNameException>(() => {
            s.GetCellContents("!^A1");
        });
    }

    /// <summary>
    /// If the cell name is invalid, a InvalidNameException should be thrown
    /// This tests the number overload 
    /// </summary>
    [TestMethod]
    public void SetContentsOfCellToNumberWithInvalidNameTest() {
        Spreadsheet s = new Spreadsheet();

        Assert.ThrowsException<InvalidNameException>(() => {
            s.SetContentsOfCell("!^A1", "5.0");
        });
    }

    /// <summary>
    /// If the cell name is invalid, a InvalidNameException should be thrown
    /// This tests the string overload 
    /// </summary>
    [TestMethod]
    public void SetContentsOfCellToStringWithInvalidNameTest() {
        Spreadsheet s = new Spreadsheet();

        Assert.ThrowsException<InvalidNameException>(() => {
            s.SetContentsOfCell("$^A1", "hello");
        });
    }

    /// <summary>
    /// If the cell name is invalid, a InvalidNameException should be thrown
    /// This tests the Formula overload 
    /// </summary>
    [TestMethod]
    public void SetContentsOfCellToFormulaWithInvalidNameTest() {
        Spreadsheet s = new Spreadsheet();

        Assert.ThrowsException<InvalidNameException>(() => {
            s.SetContentsOfCell("#^A1", "= 1+1");
        });
    }

    /// <summary>
    /// Tests setting a cell with a number
    /// Should return just the name of the cell
    /// </summary>
    [TestMethod]
    public void SetContentsOfCellWithNumberTest() {
        Spreadsheet s = new Spreadsheet();

        List<string> result = s.SetContentsOfCell("A1", "1.0").ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    /// <summary>
    /// Tests setting a cell with text
    /// Should return just the name of the cell 
    /// </summary>
    [TestMethod]
    public void SetContentsOfCellWithTextTest() {
        Spreadsheet s = new Spreadsheet();

        List<string> result = s.SetContentsOfCell("A1", "My Cell Name").ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    /// <summary>
    /// Tests setting a cell with a Formula
    /// Should return just the name of the cell
    /// </summary>
    [TestMethod]
    public void SetContentsOfCellWithFormulaTest() {
        Spreadsheet s = new Spreadsheet();

        List<string> result = s.SetContentsOfCell("A1", "= 1+1").ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    /// <summary>
    /// If the cell contents are set with a dependency
    /// Set cell contents should return name, plus all dependencies 
    /// </summary>
    [TestMethod]
    public void SetContentsOfCellAfterSetupTest() {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("b1", "= 8-a1");
        s.SetContentsOfCell("c1", "=a1");

        List<string> expected = new List<string> { "a1", "b1", "c1" };
        List<string> result = s.SetContentsOfCell("a1", "10.9").ToList();
        CollectionAssert.AreEquivalent(expected, result);

    }

    /// <summary>
    /// Setting cell contents with nested dependencies 
    /// </summary>
    [TestMethod]
    public void SetContentsOfCellNestedDependencyTest() {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("a1", "=15.0");
        s.SetContentsOfCell("b1", "=5.0");
        s.SetContentsOfCell("c1", "=a1 + b1");
        s.SetContentsOfCell("d1", "=c1 + 4");
        s.SetContentsOfCell("e1", "=d1+ c1");

        List<string> expected = new List<string> { "a1", "c1", "d1", "e1" };
        List<string> result = s.SetContentsOfCell("a1", "20").ToList();
        CollectionAssert.AreEquivalent(expected, result);

    }

    /// <summary>
    /// If a Formula cell in the middle of a dependency chain
    /// is set to a number, it no longer has any dependees. 
    /// </summary>
    [TestMethod]
    public void ReplacingTest() {
        Spreadsheet s = new Spreadsheet();

        s.SetContentsOfCell("A1", "3");
        s.SetContentsOfCell("B1", "=A1 + 3");
        s.SetContentsOfCell("C1", "=B1 + 1");

        // Replace a middle node with a number
        // It now is no longer dependent on any other cells
        // It may still have dependents
        s.SetContentsOfCell("B1", "4");

        // Cell A1 no longer has any direct dependents
        // So if we set its value, the returned list should
        // only contain A1
        List<string> result = s.SetContentsOfCell("A1", "1").ToList();
        Assert.IsTrue(result.Count == 1);
        Assert.IsTrue(result[0] == "A1");

    }

    /// <summary>
    /// If a Formula cell in the middle of a dependency chain
    /// is set to a number, it no longer has any dependees. 
    /// </summary>
    [TestMethod]
    public void ReplacingTestB() {
        Spreadsheet s = new Spreadsheet();

        s.SetContentsOfCell("A1", "3");
        s.SetContentsOfCell("A2", "2");
        s.SetContentsOfCell("B1", "=A1 + 3");
        s.SetContentsOfCell("C1", "=B1 + 1");

        // Replace a middle node with a formula 
        List<string> cellsToRecalculate = s.SetContentsOfCell("B1", "=A2+A2").ToList();
        Assert.AreEqual(2, cellsToRecalculate.Count);
        Assert.AreEqual("B1", cellsToRecalculate[0]);
        Assert.AreEqual("C1", cellsToRecalculate[1]);

    }


    /// <summary>
    /// /// If a circular dependency is created by SetContentsOfCell
    /// it should throw a Circular Exception
    ///
    /// This tests a larger circular dependency
    /// </summary>
    [TestMethod]
    public void CreateLargeCircularDependency() {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "=B1");
        s.SetContentsOfCell("B1", "=C1");
        s.SetContentsOfCell("C1", "=D1");

        Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("D1", "=A1"));

    }

    /// <summary>
    /// If the contents of several cells are set to formulas
    /// then reset back to empty, there should be no names returned
    /// by GetNamesOfAllNonemptyCells.
    /// </summary>
    [TestMethod]
    public void SetAllCellsToEmptyCell() {
        Spreadsheet s = new Spreadsheet();
        s.SetContentsOfCell("A1", "=6");
        s.SetContentsOfCell("B1", "=A1+1");
        s.SetContentsOfCell("C1", "=B1+1");
        s.SetContentsOfCell("D1", "=C1+1");
        IEnumerator<string> e = s.GetNamesOfAllNonemptyCells().GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        s.SetContentsOfCell("A1", "");
        s.SetContentsOfCell("B1", "");
        s.SetContentsOfCell("C1", "");
        s.SetContentsOfCell("D1", "");
        IEnumerator<string> f = s.GetNamesOfAllNonemptyCells().GetEnumerator();

        Assert.IsFalse(f.MoveNext());

    }
}
