A short summary of the Spreadsheet GUI features:
On running the application, you see 4 things above the grid. An enter filename entry,
a cell name label, a value label, and a contents entry.
Enter a filename in the name entry for saving to a file (which will be explained later on.
Next, select any cell and you find that you can enter contents in the cell that is selected.
Depending on what the contents are, (string, number, formula) the value will update accordingly.
If the cell is invalid, notice that a display popup error will occur. We have three extra buttons,
 a set button and sum row/column buttons. The set button just is a way to manually
click setting a button contents and value.
The sum column and row buttons are used by selecting a cell, and then clicking the buttons, to get
 the sum of the whole row or the whole column to be the contents and value of the cell that is selected
in the row and column it is in.
If you open the menu bar on the top left of the spreadsheet, we see that it allows us to open
a .sprd file in our directory to a spreadsheet in the GUI.
We can also just make a new spreadsheet by selecting new that will erase the current displayed spreadsheet.
 We can also save a spreadsheet in the application as a .sprd file in the documents folder in
the system directory.
If you need help managing the spreadsheet, there is a help page in the menu bar Help > spreadsheet help.
There is an explanation on how to mangage the spreadsheet.

Design decisions:
We chose to keep it very simple in the xaml file for the mainpage, with a few stack layouts containing labels
for value and name, entries for contents, and buttons
for our special features. The main logic happens in the xaml.cs file for
updating a cell in our spreadsheet.

Implementation:
We first start my making our validator and normalizer. The normalizer normalizes each string by making it uppercase,
and the validator will only accept 1 letter followed by 1 number and another number zero or more times.
Oct 20th passed in lamda expressions
Oct 23rd completed the correct validator and normalizer

In the displayselection method, we basically get the column and row of the selected cell, and get its contents to display
in the GUI by what the user inputs. We check if the cell is empty, in order to display an empty value and contents for each empty cell.
If the cell is not empty, we get its value using the formula class, check if its of type formula, and have the formula be the contents as well.
Oct 16th completed most functionality
Oct 17th completed if contents is a formula

In the NewClicked method, we first display an alert asking the user if they want to save their work first, and if they do, will clear the grid,
get a path to the user's Documents folder, and write the full folder path and call our save method from our previous assignment. If they do not
want to do that, they will just exit the popup and stay on the spreadsheet.
Oct 20th Spent most of the day figuring out save, open, new.
OnCompleted is a method that basically listens for the event that enter or return is pressed, and calls set contents when that happens
for a more practicle spreadsheet mechanic.
Oct 16th Completed this method and got used to event listeners

SetClicked is a method that listens for the set button to be clicked to call set contents.
Oct 18th Implemented the set button after seeing it in lecture

OpenClicked is the listener method for the open item in the menu bar. We first check if the
spreadsheet has changed, then make a safety popup like in newclicked, asking if they want to save the
spreadsheet first. We then use filepicker for the user to choose a file, than make a spreadsheet with
the constructor that takes in a filepath. We then get all nonempty cells in the spreadsheet, get their row's and column's,
then set them on the spreadsheetgrid. We also then update the name of the file in the gui entry with the file name. We
catch exceptions if there was an error reading a file.
Oct 20th- Along with save and new, spent most of the day struggling the filepaths. Figured it out.

FileNameCompleted is the method listening for our filename entry in the GUI. It sets the filepath name to the files name with
the appropriate ".sprd" extension.
Oct 20th implemented this listener for choosing filename.

SaveClicked is the listener for our save menu item. We get a folder path to the users documents, and save the spreadsheet to the filepath.
If there was an error with the file, catch the exception and display a message.
Oct 20th The first thing we figured out by using the premade save method (easier).

HelpClicked is our listener for the help menu item. It displays a new Help page with helpful information.
Oct 23rd One of the last things we implemented, figured it out pretty quickly how to write text with xaml file.

SumColumnClicked is a listener for one of our unique features, that gets the non empty cells in a column, and checks if they are all in the same column,
and sums all the values in the column together, displaying the contents and value to be the sum in whichever cell is selected.
Oct 23rd Last thing we implemented, took about an hour to figure out.

SumRowClicked is a listener for one of our unique features, that gets the non empty cells in a row, and checks if they are all in the same row,
and sums all the values in the row together, displaying the contents and value to be the sum in whichever cell is selected.
Oct 23rd Last thing we implemented, took about an hour to figure out.

getLetterName is a helper method we implemented to take a row and column, and return the corresponding cell name.
Oct 19th Zoomed and figured out how to convert cell names to columns using ascii values.

getColRow is a helper method we implemented to take a string letter name, and return the corresponding row and column.
Oct 19th Zoomed and figured out how to convert cell names to columns using ascii values.

SetContents is a method that was made to set the contents of each cell while also handeling formulaErrors to display alers, catching circular exceptions,
and properly setting values in the spreadsheet grid.
Oct 23rd Finished this method with main concern circular exceptions and formulaErrors.