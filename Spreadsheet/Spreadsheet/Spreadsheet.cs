///
/// An implementation of AbstractSpreadsheet
///
/// Quinn Pritchett
/// September 2023
///


using System.Text.RegularExpressions;
using SpreadsheetUtilities;

namespace SS {

    /// <summary>
    /// An implementation of the AbstractSpreadsheet
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet {

        /// <summary>
        /// A mapping from names to Cell objects 
        /// </summary>
        private readonly Dictionary<string, Cell> NonEmptyCells = new();

        /// <summary>
        /// A dependency graph for keeping track of dependencies
        /// within the spreadsheet
        /// </summary>
        private readonly DependencyGraph DG = new();

        public Spreadsheet() {

        }

        public override object GetCellContents(string name) {
            if (IsValid(name)) {
                if (IsNonEmptyCell(name)) {
                    return NonEmptyCells[name].Contents;
                }
                else {
                    return "";
                }
            } else {
                throw new InvalidNameException();
            }
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells() {
            return NonEmptyCells.Keys;
        }

        public override IList<string> SetCellContents(string name, double number) {
            if (!IsValid(name)) {
                throw new InvalidNameException();
            }
            else {
                NonEmptyCells.Add(name, new Cell(name, number));
                return GetCellsToRecalculate(name).ToList();
            }
        }

        public override IList<string> SetCellContents(string name, string text) {
            if (!IsValid(name)) {
                throw new InvalidNameException();
            }
            else {
                NonEmptyCells.Add(name, new Cell(name, text));
                return GetCellsToRecalculate(name).ToList();
            }
        }

        public override IList<string> SetCellContents(string name, Formula formula) {
            if (!IsValid(name)) {
                throw new InvalidNameException();
            }
            else {
                NonEmptyCells.Add(name, new Cell(name, formula));

                foreach (string variable in formula.GetVariables()) {
                    DG.AddDependency(variable, name);
                }
                return GetCellsToRecalculate(name).ToList();
            }
        }

        protected override IEnumerable<string> GetDirectDependents(string name) {
            return DG.GetDependents(name);
        }

        /// <summary>
        /// Returns true if a string is a valid Cell name
        /// Otherwise returns false
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool IsValid(string name) {
            string varPattern = "^[_a-zA-Z][_a-zA-Z0-9]*";
            if (Regex.IsMatch(name, varPattern)) {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Returns true if the name represents a non-empty cell
        /// in the spreadsheet
        /// Otherwise returns false
        /// </summary>
        /// <param name="name"> The cell name </param>
        /// <returns> A boolean representing if the cell is non-empty</returns>
        private bool IsNonEmptyCell(string name) {
            return NonEmptyCells.ContainsKey(name);
        }
    }

    /// <summary>
    /// A Cell represents a non-empty cell in a Spreadsheet
    /// Cells have Names and Contents.
    /// A string is a valid cell name if and only if:
    ///   (1) its first character is an underscore or a letter
    ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
    ///
    /// Contents can be a number, text or a Formula 
    /// </summary>
    internal class Cell {

        private string cellName;
        private object cellContents;

        public object Contents {
            get {
                return cellContents;
            }
        }
        public string Name {
            get {
                return cellName;
            }
        }

        public Cell(string name, double number) {
            cellName = name;
            cellContents = number;
        }

        public Cell(string name, string text) {
            cellName = name;
            cellContents = text;
        }

        public Cell(string name, Formula formula) {
            cellName = name;
            cellContents = formula;
        }
    }
}


