///
/// An implementation of AbstractSpreadsheet
///
/// Quinn Pritchett
/// September 2023
///
using System.Text.Json.Serialization;
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

        ///// <summary>
        ///// Returns true if the variable name is valid.
        ///// Otherwise returns false
        ///// </summary>
        ///// <param name="variable"></param>
        ///// <returns></returns>
        //private delegate bool Validator(string variable);

        ///// <summary>
        ///// Returns a normalized version of a variable
        ///// For example if all variable letters should be
        ///// capitialed Normalizer("a1") => "A1".
        ///// </summary>
        ///// <param name="variable"></param>
        ///// <returns></returns>
        //private delegate string Normalizer(string variable);

        private Func<string, string> Normalize;
        private Func<string, bool> IsValid;

        /// <summary>
        /// Creates and empty spreadsheet that imposes
        /// no extra validity conditions
        ///
        /// Normalizer: every cell name to itself
        /// 
        /// Version: default
        /// </summary>
        public Spreadsheet(): base("default") {
            Normalize = (string name) => name;
            IsValid = (string name) => true;
        }

        public Spreadsheet(Func<string, bool> validator, Func<string, string> normalizer, string version): base (version) {
            IsValid = validator;
            Normalize = normalizer;
        }

        public override void Save(string filename) {
            if(Changed) {

            }
            throw new NotImplementedException();
        }

        public override object GetCellContents(string name) {
            if (IsValidCellName(name)) {
                if (IsInNonEmptyCells(name)) {
                    return NonEmptyCells[name].Contents;
                } else {
                    return "";
                }
            } else {
                throw new InvalidNameException();
            }
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells() {
            return NonEmptyCells.Keys;
        }

        public override IList<string> SetContentsOfCell(string name, string content) {
            if (IsValidCellName(name)) {
                Changed = true;
                if (Double.TryParse(content, out double result)) {
                    return SetCellContents(name, result);
                } else if (content.Length > 0 && content.ElementAt(0) == '=') {
                    string formula = content.Substring(1);
                    return SetCellContents(name, new Formula(formula, Normalize, IsValid));
                } else {
                    return SetCellContents(name, content);
                }
            } else {
                throw new InvalidNameException();
            }

        }

        protected override IList<string> SetCellContents(string name, double number) {
            if (!IsValidCellName(name)) {
                throw new InvalidNameException();
            } else {
                NonEmptyCells.UpdateOrAdd(name, number);
                DG.ReplaceDependees(name, new List<string>()); // Reset dependees
                return GetCellsToRecalculate(name).ToList();
            }
        }

        protected override IList<string> SetCellContents(string name, string text) {
            if (!IsValidCellName(name)) {
                throw new InvalidNameException();
            }
            if (text.Equals("") && NonEmptyCells.ContainsKey(name)) {
                NonEmptyCells.Remove(name); // Cell is now empty
                DG.ReplaceDependees(name, new List<string>()); // Reset dependees
                // The cell is now empty, should we clear it's dependents?
                // No, the graph is a separate concern. Keep the relationship
                // even if the contents are empty 
                return GetCellsToRecalculate(name).ToList();
            } else if (text.Equals("")) {
                return GetCellsToRecalculate(name).ToList();

            } else {
                NonEmptyCells.UpdateOrAdd(name, text);
                DG.ReplaceDependees(name, new List<string>()); // Reset dependees
                return GetCellsToRecalculate(name).ToList();
            }
        }

        protected override IList<string> SetCellContents(string name, Formula formula) {
            if (!IsValidCellName(name)) {
                throw new InvalidNameException();
            } else { 
                try {
                    foreach(string s in DG.GetDependents(name)) {
                        if (formula.GetVariables().Contains(s)) {
                            throw new CircularException();
                        }
                    }
                    NonEmptyCells.UpdateOrAdd(name, formula);
                    DG.ReplaceDependees(name, formula.GetVariables());
                    return GetCellsToRecalculate(name).ToList();
                } catch (CircularException) {
                   
                    throw new CircularException();
                } 
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
        private bool IsValidCellName(string name) {
            name = Normalize(name);
            string varPattern = "^[_a-zA-Z][_a-zA-Z0-9]*";

            return Regex.IsMatch(name, varPattern) && IsValid(name) ;    
        }

        /// <summary>
        /// Returns true if the name represents a non-empty cell
        /// in the spreadsheet
        /// Otherwise returns false
        /// </summary>
        /// <param name="name"> The cell name </param>
        /// <returns> A boolean representing if the cell is non-empty</returns>
        private bool IsInNonEmptyCells(string name) {
            return NonEmptyCells.ContainsKey(name);
        }



        public override object GetCellValue(string name) {
            throw new NotImplementedException();
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

       public string Name { get; set; }
       public object Contents { get; set; }


        public Cell(string name, object contents) {
            Name = name;
            Contents = contents;
        }
    }

    /// <summary>
    /// An extension class for Dictionary
    /// </summary>
    internal static class DictionaryExtensions {

        /// <summary>
        /// Updates or adds a new pair to a Dictionary of strings to Cell objets 
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        public static void UpdateOrAdd(this Dictionary<string, Cell> dictionary, string name, object contents) {
            if (dictionary.ContainsKey(name)) {
                dictionary[name].Contents = contents;
            } else {
                dictionary.Add(name, new Cell(name, contents));
            }
        }
    }
}


