// The MainPage for the SpreadsheetGUI application
// Links information between the SpreadsheetGrid and
// the backing Spreadsheet.
//
// Modified from code given in CS 3500
// Aleko Louras and Quinn Pritchett
// October 2023
//

using SS;
using SpreadsheetUtilities;
using System.Threading.Channels;
using Microsoft.Maui.Storage;
using System.Text.RegularExpressions;

namespace SpreadsheetGUI;

/// <summary>
/// The main Spreadsheet view with deails about each cell
/// </summary>
public partial class MainPage : ContentPage {

    // Cells can be upper or lower case but will be Normalized to
    // Upper case 
    private static string Normalizer(string name) {
        return name.ToUpper();
    }

    // Cell names must be valid cells in the grid. A-Z followed by a number 1-99
    private static bool Validator(string name) {
        string pattern = "^[A-Z][1-9][1-9]?$";
        return Regex.IsMatch(name, pattern);
    }
    Spreadsheet s = new Spreadsheet(Validator, Normalizer, "ps6");
    

    /// <summary>
    /// Constructor for the MainPage
    /// </summary>
    public MainPage() {
        InitializeComponent();


        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(0, 0);
    }

    /// <summary>
    /// When a cell is selected, the details of that cell are then shown
    /// in the text boxes above the grid. 
    /// </summary>
    /// <param name="grid"></param>
    private void displaySelection(ISpreadsheetGrid grid) {
        Contents.Focus();

        spreadsheetGrid.GetSelection(out int col, out int row);
        spreadsheetGrid.GetValue(col, row, out string value);
        Contents.Text = value;

        if (value == "") {
            Contents.Text = "";
            spreadsheetGrid.GetValue(col, row, out value);
            CellName.Text = getLetterName(col, row);
            Value.Text = "";
        } else {

            CellName.Text = getLetterName(col, row);
            Value.Text = s.GetCellValue(CellName.Text).ToString();
            if (s.GetCellContents(CellName.Text).GetType() == typeof(Formula)) {
                string withoutEquals = s.GetCellContents(CellName.Text).ToString();
                string withEquals = "=" + withoutEquals;
                Contents.Text = withEquals;
                Value.Text = s.GetCellValue(CellName.Text).ToString();

            }

        }

    }

    /// <summary>
    /// If the New button is clicked, Warn aboout unsaved data, allow user to save
    /// If the operation is cancelled, nothing changes. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void NewClicked(Object sender, EventArgs e) {
        bool save = await DisplayAlert("Warning: Any unsaved data will be lost", "Save your work?", "Save", "Cancel");
        if (save) {
            spreadsheetGrid.Clear();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var subFolderPath = Path.Combine(path, filePath.Text);
            Console.WriteLine(subFolderPath);
            s.Save(subFolderPath);
        } else {
            //do nothing
        }
    }

    /// <summary>
    /// Handles the return key press on cell contents
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCompleted(Object sender, EventArgs e) {
        SetContents();
    }

    /// <summary>
    /// Handles the set button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SetClicked(Object sender, EventArgs e) {
        SetContents();
    }

    /// <summary>
    /// Opens any file and displays its contents in the spreadsheet grid.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e) {

        // Prompty for saving
        if (s.Changed) {
            bool save = await DisplayAlert("Warning: Any unsaved data will be lost", "Save your work?", "Save", "Cancel");
            if (save) {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var subFolderPath = Path.Combine(path, filePath.Text);
                Console.WriteLine(subFolderPath);
                s.Save(subFolderPath);
            }
        }
        // Try and read the file, catch potential reading exceptions. 
        try {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null) {
                Console.WriteLine("Successfully chose file: " + fileResult.FileName);
                // for windows, replace Console.WriteLine statements with:
                //System.Diagnostics.Debug.WriteLine( ... );
                 s = new Spreadsheet(fileResult.FullPath, s=> true , s => s.ToUpper(), "ps6"); // TODO: fix validator

                spreadsheetGrid.Clear();

                List<string> newCellNames = s.GetNamesOfAllNonemptyCells().ToList();
                foreach (string cellName in newCellNames) {
                    (int, int) position = getColRow(cellName);
                    spreadsheetGrid.SetValue(position.Item1, position.Item2, s.GetCellValue(cellName).ToString());
                }
                filePath.Text = fileResult.FileName;

                string fileContents = File.ReadAllText(fileResult.FullPath);
                Console.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
            } else {
                Console.WriteLine("No file selected.");
            }
        } catch (Exception ex) {
            Console.WriteLine("Error opening file:");
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    /// Appends the .sprd to any file name given
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FileNameCompleted(Object sender, EventArgs e) {
        string filePathWithExtension = filePath.Text + ".sprd";
        filePath.Text = filePathWithExtension;
    }

    /// <summary>
    /// Handles the Save menu clicked. All Spreadsheets are saved to the users Documents folder
    /// </summary>
    private async void SaveClicked(Object sender, EventArgs e) {
        try {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var subFolderPath = Path.Combine(path, filePath.Text);
            s.Save(subFolderPath);
        } catch (SpreadsheetReadWriteException) {
            await DisplayAlert("There was a problem saving", "Please check your filename", "OK");
        }
    }

    /// <summary>
    /// Opens the Help Guide
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HelpClicked(Object sender, EventArgs e) {
        await Navigation.PushAsync(new HelpPage());
    }

    /// <summary>
    /// Sums the Column of the selected cell and puts the result in the selected
    /// cell's contents. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SumColumnClicked(Object sender, EventArgs e) {
        List<string> cells = s.GetNamesOfAllNonemptyCells().ToList();
        double sum = 0;
        spreadsheetGrid.GetSelection(out int col, out int row);
        foreach(string cell in cells) {
            if (cell[0] == col+'A' || cell[0] == 97 ) {
                Console.WriteLine("Found an A");
                sum += (double)s.GetCellValue(cell);
                Contents.Text = sum.ToString();
                SetContents();
            }
        }
        
    }

    /// <summary>
    /// Sums the Row of the selected cell and puts the result in the selected
    /// cell's contents. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SumRowClicked(Object sender, EventArgs e) {
        List<string> cells = s.GetNamesOfAllNonemptyCells().ToList();
        double sum = 0;
        spreadsheetGrid.GetSelection(out int col, out int row);
        foreach (string cell in cells) {
            if ((cell.Substring(1) == (row + 1).ToString())) {
                sum += (double)s.GetCellValue(cell);
                Contents.Text = sum.ToString();
                SetContents();
            }
        }

    }

    /// <summary>
    /// Gets the letter-number combination of the selected cell
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    private string getLetterName(int col, int row) {
        string letter = ((char)('A' + col)).ToString();
        string letterNumber = letter + (row + 1);
        return letterNumber;
    }

    /// <summary>
    /// Gets the Col-Row combination from a given letter-number combination
    /// </summary>
    /// <param name="letterNumber"></param>
    /// <returns></returns>
    private (int col, int row) getColRow(string letterNumber) {

        int col = char.ToUpper(letterNumber[0]) - 'A';
        int row = int.Parse(letterNumber.Substring(1)) - 1;

        return (col, row);
    }

    /// <summary>
    /// Sets the the contents of the spreadsheetGrid to whatever was entered into
    /// the cell contents entry box. Displays allerts for FormulaFormatExceptions and
    /// CircularExceptions. 
    /// </summary>
    private void SetContents() {
        spreadsheetGrid.GetSelection(out int col, out int row);
        string cellName = ((char)('A' + col)).ToString();
        CellName.Text = cellName + (row + 1);
        try {
            object returned = s.SetContentsOfCell(CellName.Text, Contents.Text);
            if (returned is FormulaError) {
                DisplayAlert("There is a problem with this formula", "Please check your formula", "OK");
            }
            List<string> cells = s.SetContentsOfCell(CellName.Text, Contents.Text).ToList();
            Value.Text = s.GetCellValue(CellName.Text).ToString();

            spreadsheetGrid.SetValue(col, row, s.GetCellValue(CellName.Text).ToString());

            foreach (string c in cells) {

                (int, int) colrow = getColRow(c);
                spreadsheetGrid.SetValue(colrow.Item1, colrow.Item2, s.GetCellValue(c).ToString());

            }
        } catch (FormulaFormatException) {

            DisplayAlert("There is a problem with this formula", "Please check your formula", "OK");
        } catch (CircularException) {
            DisplayAlert("There is a problem with this formula", "Please check your formula", "OK");
        }
    }

    
}
