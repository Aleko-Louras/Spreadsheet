///
/// An implementation of AbstractSpreadsheet
///
/// Quinn Pritchett
/// September 2023
///


using System.Text.RegularExpressions;
using SpreadsheetUtilities;

namespace SS {
    public class Spreadsheet : AbstractSpreadsheet {

        private Dictionary<string, Cell> Cells = new();
        private DependencyGraph DG = new();

        public Spreadsheet() {

        }

        public override object GetCellContents(string name) {
            if (IsValid(name)) {
                return Cells[name].Contents;
            } else {
                throw new InvalidNameException();
            }
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells() {
            return Cells.Keys;
        }

        public override IList<string> SetCellContents(string name, double number) {
            if (!IsValid(name)) {
                throw new InvalidNameException();
            }
            else {
                Cells.Add(name, new Cell(name, number));
                return GetCellsToRecalculate(name).ToList();
            }
        }

        public override IList<string> SetCellContents(string name, string text) {
            if (!IsValid(name)) {
                throw new InvalidNameException();
            }
            else {
                Cells.Add(name, new Cell(name, text));
                return GetCellsToRecalculate(name).ToList();
            }
        }

        public override IList<string> SetCellContents(string name, Formula formula) {
            if (!IsValid(name)) {
                throw new InvalidNameException();
            }
            else {
                Cells.Add(name, new Cell(name, formula));

                foreach (string variable in formula.GetVariables()) {
                    DG.AddDependency(variable, name);
                }
                return GetCellsToRecalculate(name).ToList();
            }
        }

        protected override IEnumerable<string> GetDirectDependents(string name) {
            return DG.GetDependents(name);
        }

        private bool IsValid(string name) {
            string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            if (Regex.IsMatch(name, varPattern)) {
                return true;
            }
            else return false;
        }
    }

    internal class Cell {

        public string Name { get; set; }
        public object Contents { get; set; }

        public Cell(string name, double number) {
            Name = name;
            Contents = number;

        }

        public Cell(string name, string text) {
            Name = name;
            Contents = text;
        }

        public Cell(string name, Formula formula) {
            Name = name;
            Contents = formula;
        }

        public bool IsEmpty() {
            if (Contents.ToString() == "") {
                return true;
            }
            else {
                return false;
            }
        }
    }
}


