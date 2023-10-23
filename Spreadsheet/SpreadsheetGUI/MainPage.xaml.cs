using SS;
using SpreadsheetUtilities;
using System.Threading.Channels;
using Microsoft.Maui.Storage;
using System.Text.RegularExpressions;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage {
    private static string Normalizer(string name) {
        return name.ToUpper();
    }

    private static bool Validator(string name) {
        string pattern = "^[A-Z][1-9][1-9]?$";
        return Regex.IsMatch(name, pattern);
    }
    Spreadsheet s = new Spreadsheet(Validator, Normalizer, "ps6");
    

    /// <summary>
    /// Constructor for the demo
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
    private void OnCompleted(Object sender, EventArgs e) {
        SetContents();
    }

    private void SetClicked(Object sender, EventArgs e) {
        SetContents();
    }

    /// <summary>
    /// Opens any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e) {
        if (s.Changed) {
            bool save = await DisplayAlert("Warning: Any unsaved data will be lost", "Save your work?", "Save", "Cancel");
            if (save) {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var subFolderPath = Path.Combine(path, filePath.Text);
                Console.WriteLine(subFolderPath);
                s.Save(subFolderPath);
            }
        }
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

    private void FileNameCompleted(Object sender, EventArgs e) {
        string filePathWithExtension = filePath.Text + ".sprd";
        filePath.Text = filePathWithExtension;
    }

    /// <summary>
    /// Opens any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
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

    private async void HelpClicked(Object sender, EventArgs e) {
        //await DisplayAlert("Spreadsheet Help", "Press return to enter a value. " +
        //                   "You can enter formula with lower or upercase letters" +
        //                   "You may also hightlight a cell using the highlight button", "OK");
        await Navigation.PushAsync(new HelpPage());
    }
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

    private string getLetterName(int col, int row) {
        string letter = ((char)('A' + col)).ToString();
        string letterNumber = letter + (row + 1);
        return letterNumber;
    }

    private (int col, int row) getColRow(string letterNumber) {

        int col = char.ToUpper(letterNumber[0]) - 'A';
        int row = int.Parse(letterNumber.Substring(1)) - 1;

        return (col, row);
    }

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
