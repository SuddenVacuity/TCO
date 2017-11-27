using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;

namespace TCO
{
    public static class TCOSheetsInterface
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        private static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private static string ApplicationName = "TCO";
        private static SheetsService service;

        // Error messages
        private static string m_errorInvalidCell = "Invalid cell reference";
        private static string m_errorTooManyColumns = "Number of columns exceeds the maximum allowed";
        private static string m_errorArgumentNullReference = "Argument is null";
        private static string m_errorArgumentOutOfRange = "Argument is out of range";
        private static string m_errorTabAlreadyExists = "Tab with this name already exists";
        private static string m_errorInvalidTabDimension = "Invalid tab dimension";
        private static string m_errorInvalidPasteType = "Invalid Paste Type";
        private static string m_errorInvalidRange = "Invalid Range";
        private static string m_errorTabDoesNotExist = "Tab does not exist";

        /// <summary>
        /// array of plain text to specify tab dimenstion
        /// <para></para>
        /// Use enum Dimension to access the correct index
        /// </summary>
        private static string[] m_dimensions = {
            "ROWS",
            "COLUMNS"
        };

        /// <summary>
        /// Used as the index to access tab dimension text
        /// </summary>
        public enum Dimension
        {
            Rows,
            Columns,
            
            Count
        }

        /// <summary>
        /// array of plain text to specify paste type
        /// <para></para>
        /// Use enum PasteType to access the correct index
        /// </summary>
        private static string[] m_pasteTypes = {
            "PASTE_NORMAL",
            "PASTE_VALUES",
            "PASTE_FORMAT",
            "PASTE_NO_BORDERS",
            "PASTE_FORMULA",
            "PASTE_DATA_VALIDATION",
            "PASTE_CONDITIONAL_FORMATTING"
        };

        /// <summary>
        /// Used as the index to access paste type text
        /// </summary>
        public enum PasteType
        {
            /// <summary>
            /// Paste values, formulas, formats, and merges.
            /// </summary>
            Normal,
            /// <summary>
            /// Paste the values ONLY without formats, formulas, or merges.
            /// </summary>
            Values,
            /// <summary>
            /// Paste the format and data validation only.
            /// </summary>
            Format,
            /// <summary>
            /// Like PASTE_NORMAL but without borders.
            /// </summary>
            NoBorders,
            /// <summary>
            /// Paste the formulas only.
            /// </summary>
            Formula,
            /// <summary>
            /// Paste the data validation only.
            /// </summary>
            DataValidation,
            /// <summary>
            /// Paste the conditional formatting rules only.
            /// </summary>
            ConditionalFormatting,

            Count
        };

        // limits set by google
        private readonly static int MAXIMUM_COLUMNS = 256;
        private readonly static int MAXIMUM_SHEETS = 200;                            // can't realistically validate these               
        private readonly static int MAXIMUM_CELL_COUNT = 400000;                     // can't realistically validate these
        private readonly static int MAXIMUM_CELL_FORMULAS = 40000;                   // can't realistically validate these
        private readonly static int MAXIMUM_GOOGLE_FINANCE_FORMULAS = 1000;          // can't realistically validate these
        private readonly static int MAXIMUM_GOOGLE_LOOKUP_FORMULAS = 1000;           // can't realistically validate these
        private readonly static int MAXIMUM_CROSS_WORKBOOK_REFERENCE_FORMULAS = 50;  // can't realistically validate these
        private readonly static int MAXIMUM_EXTERNAL_DATA_FORMULAS = 50;             // can't realistically validate these

        private static bool m_isInitialized = false;
        private static bool m_shouldRun = false;

        public static bool Init()
        {
            if (m_isInitialized == true)
                return true;

            UserCredential credential;
            FileStream stream = null;
            //using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read)) ;

            try
            {
                stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read);
                m_shouldRun = true;
            }
            catch
            {
                Console.WriteLine("ERROR: unable to open client_secret.json");
                m_shouldRun = false;
                return false;
            }
            
            string credPath = System.Environment.CurrentDirectory;

            //string credPath = System.Environment.GetFolderPath(
            //    System.Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                TCOSheetsInterface.Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            Console.WriteLine("Credential file saved to: " + credPath);
            
            // Create Google Sheets API service.
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = TCOSheetsInterface.ApplicationName,
            });

            m_isInitialized = true;
            return m_shouldRun;
        }

        /// <summary>
        /// Reads data from the specified sheet within the specified range.
        /// </summary>
        /// <param name="spreadsheetId">The id of the spreadsheet to be read. docs.google.com/spreadsheets/d/[SpreadsheetIdIsHere]/edit#gid=0</param>
        /// <param name="rangeGet">The range to read data from. format: "sheetname!A1:E32"</param>
        /// <param name="readByColumn">If true data is read by column, if false data is read by row</param>
        /// <returns></returns>
        public static IList<IList<object>> ReadDataFromSheet(string spreadsheetId, string rangeGet, bool readByColumn = true)
        {
            if (m_shouldRun == false)
                return null;

            // get request
            SpreadsheetsResource.ValuesResource.GetRequest requestGet =
                    TCOSheetsInterface.service.Spreadsheets.Values.Get(spreadsheetId, rangeGet);

            if (readByColumn == true) requestGet.MajorDimension = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.COLUMNS;
            else requestGet.MajorDimension = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.ROWS;
            
            ValueRange response = requestGet.Execute();
            IList<IList<Object>> valuesRead = response.Values;

            return valuesRead;
        }

        /// <summary>
        /// Returns true if the google sheets service is set up porperly
        /// </summary>
        /// <returns></returns>
        public static bool getIsServiceActive()
        {
            return (m_shouldRun == true && m_isInitialized == true);
        }
        
        /// <summary>
        /// Writes a single dimensional array of data to the sheet.
        /// </summary>
        /// <param name="spreadsheetId">The id of the spreadsheet to be read. docs.google.com/spreadsheets/d/[SpreadsheetIdIsHere]/edit#gid=0</param>
        /// <param name="rangeGet">The range to read data from. format: "sheetname!A1:A32"</param>
        /// <param name="readByColumn">If true data is written by column, if false data is written by row</param>
        /// <returns></returns>
        public static bool WriteDataToSheet(string spreadsheetId, string rangeWrite, List<object> dataToWrite, Dimension dimension = Dimension.Columns)
        {
            if (m_shouldRun == false)
                return false;

            if (dimension >= Dimension.Count)
                throw new ArgumentOutOfRangeException(m_errorArgumentOutOfRange);

            // create a values object to contain the data
            ValueRange values = new ValueRange();
            // apply the range to the values object
            values.Range = rangeWrite;
            // values are always a 2 dimensional array
            // so initialize a new IList<IList<>>()
            values.Values = new List<IList<Object>>();
            // add the single dimensional array to the 2 dimnesional array
            values.Values.Add(dataToWrite);
            // set wether each array represents a row or column
            values.MajorDimension = m_dimensions[(int)dimension];

            // create an update request
            SpreadsheetsResource.ValuesResource.UpdateRequest requestUpdate =
                TCOSheetsInterface.service.Spreadsheets.Values.Update(values, spreadsheetId, values.Range);
            requestUpdate.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            // Documentation on Google API - requestUpdate.ValueInputOption =
            // SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED
            //     Set the input option to USERENTERED means object "=NOW()" is received as =NOW()
            // SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW
            //     Set the input option to RAW means object "=NOW()" is received as '=NOW()
            
            // send the request and recieve a response
            UpdateValuesResponse response = requestUpdate.Execute();
            return true;
        }
        /// <summary>
        /// Writes a two dimensional array of data to the sheet.
        /// </summary>
        /// <param name="spreadsheetId">The id of the spreadsheet to be read. docs.google.com/spreadsheets/d/[SpreadsheetIdIsHere]/edit#gid=0</param>
        /// <param name="rangeGet">The range to read data from. format: "sheetname!A1:E32"</param>
        /// <param name="readByColumn">If true data is written by column, if false data is written by row</param>
        /// <returns></returns>
        public static bool WriteDataToSheet(string spreadsheetId, string rangeWrite, IList<IList<object>> dataToWrite, Dimension dimension = Dimension.Columns)
        {
            if (m_shouldRun == false)
                return false;

            if (dimension >= Dimension.Count)
                throw new ArgumentOutOfRangeException(m_errorArgumentOutOfRange);

            ValueRange values = new ValueRange();

            values.MajorDimension = m_dimensions[(int)dimension];

            values.Range = rangeWrite;
            values.Values = dataToWrite;
            
            SpreadsheetsResource.ValuesResource.UpdateRequest requestUpdate = TCOSheetsInterface.service.Spreadsheets.Values.Update(values, spreadsheetId, values.Range);

            requestUpdate.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            UpdateValuesResponse result = requestUpdate.Execute();
            return true;
        }

        /// <summary>
        /// Creates a row of cells containing log information and appends under the lowest cells containing data. Expands the tab and cells to fit the data.
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc</param>
        /// <param name="tabName">The name of the tab to write to</param>
        /// <param name="timeStr">String containing information to write</param>
        /// <param name="description">String containing information to write</param>
        /// <returns></returns>
        public static bool WriteTimeNowToCell(string spreadsheetId, string tabName, string timeStr, string description)
        {
            if (m_shouldRun == false)
                return false;

            // TODO: properly validate spreadsheet id
            if (spreadsheetId.Length == 0)
                return false;

            SheetsLogEntry entrymaker = new SheetsLogEntry();

            List<RowData> rowDataList = new List<RowData>();
            rowDataList.Add(entrymaker.addEntry(timeStr, description));
            
            Request appendCells = requestAppendCells(spreadsheetId, tabName, rowDataList);
            Request autoSize = requestDimensionAutoSize(spreadsheetId, tabName, 0);

            BatchUpdateSpreadsheetRequest batchRequest = new BatchUpdateSpreadsheetRequest();
            batchRequest.Requests = new List<Request>();
            batchRequest.Requests.Add(autoSize);
            batchRequest.Requests.Add(appendCells);

            return ExecuteBatchRequest(batchRequest, spreadsheetId);
        }

        /// <summary>
        /// Formats a range of cells
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc</param>
        /// <param name="tabName">The name of the tab to format</param>
        /// <param name="startCell">The top-left cell in the range to be formatted</param>
        /// <param name="endCell">The bottom right cell in the range to be formatted</param>
        /// <param name="format">The cell format to apply to the range</param>
        /// <returns></returns>
        public static bool FormatCellRange(string spreadsheetId, string tabName, string startCell, string endCell, CellFormat format)
        {
            if (m_shouldRun == false)
                return false;

            Point startPoint = convertCellToPoint(startCell);
            Point endPoint = convertCellToPoint(endCell);

            GridRange gridRange = new GridRange();
            gridRange.SheetId = getTabIndex(spreadsheetId, tabName);
            gridRange.StartColumnIndex = startPoint.X;
            gridRange.StartRowIndex = startPoint.Y - 1;
            gridRange.EndColumnIndex = endPoint.X + 1;
            gridRange.EndRowIndex = endPoint.Y + 1;
            
            Request formatCells = requestFormatCells(spreadsheetId, tabName, format, gridRange);

            BatchUpdateSpreadsheetRequest batchRequest = new BatchUpdateSpreadsheetRequest();
            batchRequest.Requests = new List<Request>();
            batchRequest.Requests.Add(formatCells);

            return ExecuteBatchRequest(batchRequest, spreadsheetId);
        }

        /// <summary>
        /// Returns true if the tab exists
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc</param>
        /// <param name="tabName">The name of the tab to search for</param>
        /// <returns></returns>
        public static bool tabExists(string spreadsheetId, string tabName)
        {
            if (m_shouldRun == false)
                return false;

            if (getTabIndex(spreadsheetId, tabName) == null)
                return false;

            return true;
        }

        /// <summary>
        /// Creates a new tab
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc</param>
        /// <param name="tabName">The name of the tab that will be added</param>
        /// <param name="size">The dimensions of the cells the tab will contain</param>
        /// <param name="targetIndex">The location in the tab bar the tab will appear</param>
        /// <returns></returns>
        public static bool createTab(string spreadsheetId, string tabName, Size size, int? targetIndex = null)
        {
            if (m_shouldRun == false)
                return false;

            //if (tabExists(spreadsheetId, tabName))
            //    return false;
            //
            Request requestCreate = requestCreateNewTab(spreadsheetId, tabName, size, targetIndex);

            BatchUpdateSpreadsheetRequest batchRequest = new BatchUpdateSpreadsheetRequest();
            batchRequest.Requests = new List<Request>();
            batchRequest.Requests.Add(requestCreate);

            return ExecuteBatchRequest(batchRequest, spreadsheetId);
        }

        /// <summary>
        /// Applies the data and format of a list of rows to a range.
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc</param>
        /// <param name="tabName">The name of the tab to write to</param>
        /// <param name="startCell">The top-left cell in the range to be written to</param>
        /// <param name="endCell">The bottom-right cell in the range to be written to</param>
        /// <param name="rows">The list of rows to be written</param>
        /// <returns></returns>
        public static bool updateCells(string spreadsheetId, string tabName, string startCell, string endCell, List<RowData> rows)
        {
            if (m_shouldRun == false)
                return false;

            Point startPoint = convertCellToPoint(startCell);
            Point endPoint = convertCellToPoint(endCell);

            GridRange range = new GridRange();
            range.SheetId = getTabIndex(spreadsheetId, tabName);
            range.StartColumnIndex = startPoint.X;
            range.StartRowIndex = startPoint.Y;
            range.EndColumnIndex = endPoint.X + 1;
            range.EndRowIndex = endPoint.Y + 1;
            
            Request updateCells = requestUpdateCells(spreadsheetId, tabName, rows, range, "*");
            Request autoSize = requestDimensionAutoSize(spreadsheetId, tabName, 0);

            BatchUpdateSpreadsheetRequest batchRequest = new BatchUpdateSpreadsheetRequest();
            batchRequest.Requests = new List<Request>();
            batchRequest.Requests.Add(autoSize);
            batchRequest.Requests.Add(updateCells);

            return ExecuteBatchRequest(batchRequest, spreadsheetId);
        }

        /// <summary>
        /// Shifts the input cell reference by the amount in the direction of the input offset.
        /// <para></para>
        /// Returns a string containing the resulting cell reference.
        /// <para></para>
        /// THROWS: ArgumentException; ArgumentOutOfRangeException
        /// </summary>
        /// <param name="cell">The position of a single cell within a sheet. ex AR233</param>
        /// <param name="offset">The amount and direction to shift the cells position (row, column)</param>
        /// <returns></returns>
        public static string shiftCellReference(string rowColumn, Point offset)
        {
            string reference = rowColumn;

            Point cellIndex = convertCellToPoint(reference);

            cellIndex.Y += offset.Y;
            cellIndex.X += offset.X;
            
            return convertPointToCell(cellIndex);
        }

        /// <summary>
        /// Converts a cell reference to a point.
        /// <para> </para> 
        /// RETURNS a Point(colIndex, rowIndex) on success. 
        /// <para> </para> 
        /// THROWS: ArgumentException; ArgumentOutOfRangeException
        /// <para> </para> 
        /// NOTE: output values are inclusive.
        /// </summary>
        /// <param name="rowColumn"></param>
        /// <returns></returns>
        public static Point convertCellToPoint(string rowColumn)
        {
            string reference = rowColumn;

            // initialize empty strings to add numbers to as strings
            string row = "";
            string col = "";

            // used to make should letters and numbers aren't mixed
            bool foundFirstNumber = false;

            // go through each character and add letters to col and numbers to row
            for (int i = 0; i < reference.Length; i++)
            {
                if (char.IsLetter(reference[i]))
                {
                    // invalid cell
                    if (foundFirstNumber == true)
                        throw new ArgumentException(m_errorInvalidCell);

                    col += reference[i];
                }
                else if (char.IsDigit(reference[i]))
                {
                    foundFirstNumber = true;
                    row += reference[i];
                }
                else
                    throw new ArgumentException(m_errorInvalidCell);
            }

            // invalid cell
            if (row.Length == 0 || // row is empty
                col.Length == 0 || // col is empty
                col.Length > 2)    // col has too many characters
                throw new ArgumentException(m_errorInvalidCell);

            // change to lower for predictable interaction with char
            row = row.ToLower();
            col = col.ToLower();

            // initialize row and col indexs as a fail value
            int colIndex = -1;
            int rowIndex = -1;

            // the last character is the ones digit
            if (col.Length > 0)
                colIndex = (int)(col[col.Length - 1] - 'a');
            // if there are 2 characters the first
            // charater is the hexadodec digit
            if (col.Length == 2)
                colIndex += (int)(col[0] - 'a') * 26;
            
            // invalid cell - too many columns
            if (colIndex > MAXIMUM_COLUMNS)
                throw new ArgumentOutOfRangeException("colIndex", m_errorTooManyColumns);

            // try to convert row string to rowIndex
            int.TryParse(row, out rowIndex);

            // invalid cell - probably redundant do to string length check
            if (rowIndex == -1)
                throw new ArgumentException(m_errorInvalidCell);

            // make row index inclusive
            rowIndex -= 1;

            return new Point(colIndex, rowIndex);
        }

        /// <summary>
        /// Converts a point to a cell reference. 
        /// <para> </para> 
        /// THROWS: ArgumentOutOfRangeException
        /// <para> </para> 
        /// NOTE: input values are inclusive.
        /// </summary>
        /// <param name="point">The point to be converted into a cell</param>
        /// <returns></returns>
        public static string convertPointToCell(Point point)
        {
            int colIndex = point.X;
            int rowIndex = point.Y + 1; // add one to make it inclusive

            if (colIndex > MAXIMUM_COLUMNS)
                throw new ArgumentOutOfRangeException("colIndex", m_errorTooManyColumns);

            // initialize an empty string to ad numbers to as strings
            string cell = "";

            // if col index is greater than z
            if (colIndex > 26)
            {
                // add the value greater than 26 of colIndex to cell
                cell = cell + (char)('A' + (int)(colIndex / 26));
                // remove the value that's accounted for from colIndex
                colIndex %= 26;
            }

            // add the value to cell
            cell = cell + (char)('A' + colIndex) + (int)rowIndex;

            return cell;
        }


        ///////////////////////////////////////////////////////////////////
        /////                                                         /////
        /////                     PRIVATE METHODS                     /////
        /////                                                         /////
        ///////////////////////////////////////////////////////////////////


        /// <summary>
        /// Rearches for the tab with the specified name and returns it's index.
        /// Returns null on fail.
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc to search</param>
        /// <param name="tabName">The name of the tab to search for</param>
        /// <returns></returns>
        private static int? getTabIndex(string spreadsheetId, string tabName)
        {
            // initialed a value to hold the tab id
            int? sheetIndex = null;

            // get a list of all tabs in the doc
            IList<Sheet> sheets = service.Spreadsheets.Get(spreadsheetId).Execute().Sheets;
            
            // go through the list until a sheet with a matching 
            // name is found or the end of the list is reached
            foreach (Sheet s in sheets)
            {
                if (s.Properties.Title != tabName)
                    continue;
                sheetIndex = s.Properties.SheetId;
                break;
            }

            return sheetIndex;
        }

        /// <summary>
        /// Sends a batch request to google.
        /// </summary>
        /// <param name="request">The batchrequest to send</param>
        /// <param name="spreadsheetId">The id of the sheet doc to send the request for</param>
        /// <returns></returns>
        private static bool ExecuteBatchRequest(BatchUpdateSpreadsheetRequest request, string spreadsheetId)
        {
            SpreadsheetsResource.BatchUpdateRequest finalRequest = service.Spreadsheets.BatchUpdate(request, spreadsheetId);
            BatchUpdateSpreadsheetResponse response = finalRequest.Execute();

            return true;
        }

        /// <summary>
        /// Creates a request to create a new tab within a sheets doc
        /// <para></para>
        /// THROWS: InvalidOperationException
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc to create a tab in</param>
        /// <param name="tabName">The name of the tab to be created</param>
        /// <param name="size">The size in cells the created tab will be</param>
        /// <param name="targetIndex">The location in the tabs list the created tab will be placed</param>
        /// <returns></returns>
        private static Request requestCreateNewTab(string spreadsheetId, string tabName, Size size, int? targetIndex)
        {
            if (tabExists(spreadsheetId, tabName) == true)
                throw new InvalidOperationException(m_errorTabAlreadyExists);
            
            // create a request of the type you want
            AddSheetRequest addSheet = new AddSheetRequest();
            // create a new properties object for it
            addSheet.Properties = new SheetProperties();
            // set the values of the properties for this request
            addSheet.Properties.Title = tabName;
            addSheet.Properties.Index = targetIndex;
            addSheet.Properties.GridProperties = new GridProperties();
            addSheet.Properties.GridProperties.ColumnCount = size.Width;
            addSheet.Properties.GridProperties.RowCount = size.Height;

            // create a new base request object
            Request request = new Request();
            // apply the request to it's type in the base request
            request.AddSheet = addSheet;

            return request;
        }

        /// <summary>
        /// Creates a request to overlay the input cells onto the specified cells.
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc</param>
        /// <param name="tabName">The name of the tab containing the cells to update</param>
        /// <param name="rows">List of rows to apply to cells</param>
        /// <param name="range">The range of cells to update</param>
        /// <param name="fieldMask">Set this to a field of the request to only override that field</param>
        /// <returns></returns>
        private static Request requestUpdateCells(string spreadsheetId, string tabName, List<RowData> rows, GridRange range, string fieldMask = "*")
        {
            UpdateCellsRequest updateCellsRequest = new UpdateCellsRequest();
            updateCellsRequest.Range = range;
            updateCellsRequest.Fields = fieldMask;
            updateCellsRequest.Rows = rows;
            
            Request request = new Request();
            request.UpdateCells = updateCellsRequest;
            
            return request;
        }

        /// <summary>
        /// Creates a request to copy the cells in source range and paste them into the destination range.
        /// <para></para>
        /// THROWS: ArgumentOutOfRangeException
        /// </summary>
        /// <param name="source">The cells to be copied</param>
        /// <param name="destination">The cell to be pasted over</param>
        /// <param name="pasteType">Specifies how the paste should be handled</param>
        /// <returns></returns>
        private static Request requestCopyPaste(GridRange source, GridRange destination, PasteType pasteType = PasteType.Normal)
        {
            if (pasteType >= PasteType.Count)
                throw new ArgumentOutOfRangeException("pasteType", m_errorInvalidPasteType); 

            // change =NOW() to a literal value represeting the current time
            CopyPasteRequest copyPasteRequest = new CopyPasteRequest();
            copyPasteRequest.Source = source;
            copyPasteRequest.Destination = destination;
            copyPasteRequest.PasteType = m_pasteTypes[(int)pasteType];
            
            Request request = new Request();
            request.CopyPaste = copyPasteRequest;

            return request;
        }

        /// <summary>
        /// Creates a request to autosize a range of rows/columns to fit the data they contain.
        /// <para></para>
        /// THROWS: ArgumentOutOfRangeException; InvalidOperationException
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc</param>
        /// <param name="tabName">The name of the tab to autosize</param>
        /// <param name="dimension">Specifies wether rows or columns are autosized</param>
        /// <param name="startIndex">The point the autosize range begins</param>
        /// <param name="endIndex">The Point the autosize range ends</param>
        /// <returns></returns>
        private static Request requestDimensionAutoSize(string spreadsheetId, string tabName, int? startIndex = null, int? endIndex = null, Dimension dimension = Dimension.Columns)
        {
            if (dimension >= Dimension.Count)
                throw new ArgumentOutOfRangeException("dimension", m_errorInvalidTabDimension);

            if (startIndex > endIndex)
                throw new InvalidOperationException(m_errorInvalidRange);

            AutoResizeDimensionsRequest autoSizeDimensionProperties = new AutoResizeDimensionsRequest();
            autoSizeDimensionProperties.Dimensions = new DimensionRange();
            autoSizeDimensionProperties.Dimensions.SheetId = getTabIndex(spreadsheetId, tabName);
            autoSizeDimensionProperties.Dimensions.Dimension = m_dimensions[(int)dimension];
            autoSizeDimensionProperties.Dimensions.StartIndex = startIndex;
            autoSizeDimensionProperties.Dimensions.EndIndex = endIndex;
            
            Request request = new Request();
            request.AutoResizeDimensions = autoSizeDimensionProperties;

            return request;
        }

        /// <summary>
        /// Creates a request to delete the tab with the specified name
        /// <para></para>
        /// THROWS: InvalidOperationException
        /// </summary>
        /// <param name="tabName">The name of the tab to be deleted within the sheets doc.</param>
        /// <param name="spreadsheetId">The id of the spreadsheet to be read. docs.google.com/spreadsheets/d/[SpreadsheetIdIsHere]/edit#gid=0</param>
        /// <returns></returns>
        private static Request requestDeleteSheet(string spreadsheetId, string tabName)
        {
            int? sheetIndex = getTabIndex(spreadsheetId, tabName);

            // if the name was not found return
            if (sheetIndex == null)
                throw new InvalidOperationException(m_errorTabDoesNotExist);
            
            // create a deletesheet request
            DeleteSheetRequest deleteSheet = new DeleteSheetRequest();
            deleteSheet.SheetId = sheetIndex;

            // create base request and apply the delete request
            Request request = new Request();
            request.DeleteSheet = deleteSheet;
            
            return request;
        }

        /// <summary>
        /// Creates a request to format cells
        /// <para></para>
        /// THROWS: InvalidOperationException
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc</param>
        /// <param name="tabName">The name of the tab to be formatted</param>
        /// <param name="format">The cell format to apply to the cells</param>
        /// <param name="range">The range to apply the format to</param>
        /// <returns></returns>
        private static Request requestFormatCells(string spreadsheetId, string tabName, CellFormat format, GridRange range)
        {
            int? sheetIndex = getTabIndex(spreadsheetId, tabName);

            // if the name was not found return
            if (sheetIndex == null)
                throw new InvalidOperationException(m_errorTabDoesNotExist);

            CellData cellData = new CellData();
            cellData.UserEnteredFormat = format;

            GridRange grid = range;

            RepeatCellRequest repeatRequest = new RepeatCellRequest();
            repeatRequest.Cell = cellData;
            repeatRequest.Fields = "UserEnteredFormat";
            repeatRequest.Range = grid;

            Request request = new Request();
            request.RepeatCell = repeatRequest;

            return request;
        }

        /// <summary>
        /// Creates a reques to append cells at the bottom of existing data
        /// </summary>
        /// <param name="spreadsheetId">The id of the sheets doc</param>
        /// <param name="tabName">The name of the tab to append cells to</param>
        /// <param name="rowData">List of rows to append</param>
        /// <param name="fieldMask">Set this to a field of the request to only override that field</param>
        /// <returns></returns>
        private static Request requestAppendCells(string spreadsheetId, string tabName, List<RowData> rowData, string fieldMask = "*")
        {
            AppendCellsRequest appendCells = new AppendCellsRequest();
            appendCells.SheetId = getTabIndex(spreadsheetId, tabName);
            appendCells.Rows = rowData;
            appendCells.Fields = fieldMask;

            Request request = new Request();
            request.AppendCells = appendCells;

            return request;
        }
        
    } // END TCOSheetsInterface
} // END namespace TCO
