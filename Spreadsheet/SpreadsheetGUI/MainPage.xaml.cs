﻿using SS;
using SpreadsheetUtilities;
using System.Threading.Channels;

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{
    Spreadsheet s = new Spreadsheet();
   

    /// <summary>
    /// Constructor for the demo
    /// </summary>
	public MainPage()
    {
        InitializeComponent();
        

        // This an example of registering a method so that it is notified when
        // an event happens.  The SelectionChanged event is declared with a
        // delegate that specifies that all methods that register with it must
        // take a SpreadsheetGrid as its parameter and return nothing.  So we
        // register the displaySelection method below.
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(0, 0);
    }


    private void displaySelection(ISpreadsheetGrid grid)
    {
        Contents.Focus();
  
        spreadsheetGrid.GetSelection(out int col, out int row);
        spreadsheetGrid.GetValue(col, row, out string value);
        Contents.Text = value;
        
        if (value == "")
        {
            Contents.Text = "";
            spreadsheetGrid.GetValue(col, row, out value);
            CellName.Text = getLetterName(col, row);
            Value.Text = "";
        }
        else {

            CellName.Text = getLetterName(col, row);
            Value.Text = s.GetCellValue(CellName.Text).ToString();
            if ( s.GetCellContents(CellName.Text).GetType() == typeof(Formula)) {
                string withoutEquals = s.GetCellContents(CellName.Text).ToString();
                string withEquals = "=" + withoutEquals;
                Contents.Text = withEquals;
                Value.Text = s.GetCellValue(CellName.Text).ToString();

            }
            
        }
     
    }

    private async void NewClicked(Object sender, EventArgs e)
    {
        bool save = await DisplayAlert("Warning: Any unsaved data will be lost", "Save your work?", "Save", "Cancel");
        if (save)
        {
            spreadsheetGrid.Clear();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var subFolderPath = Path.Combine(path, filePath.Text);
            Console.WriteLine(subFolderPath);
            s.Save(subFolderPath);
        }
        else
        {
            //do nothing
        }
    }
    private void OnCompleted(Object sender, EventArgs e) {
        
        spreadsheetGrid.GetSelection(out int col, out int row);
        string cellName = ((char)('A' + col)).ToString();
        CellName.Text = cellName + (row + 1);
        try {
            List<string> cells = s.SetContentsOfCell(CellName.Text, Contents.Text).ToList();
            Value.Text = s.GetCellValue(CellName.Text).ToString();

            spreadsheetGrid.SetValue(col, row, s.GetCellValue(CellName.Text).ToString());

            foreach (string c in cells) {

                (int, int) colrow = getColRow(c);
                spreadsheetGrid.SetValue(colrow.Item1, colrow.Item2, s.GetCellValue(c).ToString());

            }
        }
        catch(FormulaFormatException) {

            DisplayAlert("There is a problem with this formula", "Please check your formula","OK" );
        }
        
        
            
    }
    /// <summary>
    /// Opens any file as text and prints its contents.
    /// Note the use of async and await, concepts we will learn more about
    /// later this semester.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        try
        {
            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
        Console.WriteLine( "Successfully chose file: " + fileResult.FileName );
        // for windows, replace Console.WriteLine statements with:
        //System.Diagnostics.Debug.WriteLine( ... );

        string fileContents = File.ReadAllText(fileResult.FullPath);
                Console.WriteLine("First 100 file chars:\n" + fileContents.Substring(0, 100));
            }
            else
            {
                Console.WriteLine("No file selected.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error opening file:");
            Console.WriteLine(ex);
        }
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
            Console.WriteLine(subFolderPath);
            s.Save(subFolderPath);
        } catch (SpreadsheetReadWriteException) {
            await DisplayAlert("There was a problem saving", "Please check your formula", "OK");
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
}
