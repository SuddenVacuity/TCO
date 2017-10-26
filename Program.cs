

/*
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCO
{
class Program
{

    static void Main(string[] args)
    {
        TCOSheetsInterface.Init();

        // Define request parameters.


        // read data from sheet
        String rangeGet = "Sheet3!B7:B49";
        IList<IList<Object>> valuesRead = TCOSheetsInterface.ReadDataFromSheet(spreadsheetId, rangeGet);

        // use read data
        if (valuesRead != null && valuesRead.Count > 0)
        {
            Console.WriteLine("Name");
            foreach (IList<object> row in valuesRead)
            {
                // Print columns A and E, which correspond to indices 0 and 4.
                Console.WriteLine("{0}", row[0]);
            }
        }
        else
        {
            Console.WriteLine("No data found.");
        }

        // update a 

        // update range of cells
        String rangeWrite = "Sheet3!A1:B2";
        List<object> valuesWrite1 = new List<object>() { "1", "2" };
        List<object> valuesWrite2 = new List<object>() { "3", "4" };
        IList<IList<Object>> rangeData = new List<IList<Object>>();
        rangeData.Add(valuesWrite1);
        rangeData.Add(valuesWrite2);
        TCOSheetsInterface.WriteDataToSheet(spreadsheetId, rangeWrite, rangeData);

        // add new sheet
        //TCOSheetsInterface.addNewSheet("newlyAddedSheet", spreadsheetId);


        // delete a sheet
        TCOSheetsInterface.deleteSheet("abcd", spreadsheetId);


        Console.Read();
    }
}
} //*/






/*
namespace SheetsQuickstart
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "TCO";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define request parameters.
            String spreadsheetId = "1qF5J0u_eyPQHTu5LX3tOgg5_ESAzMMDBWbh7rRAnBCo";
            String rangeGet = "plant data!B7:B49";
            SpreadsheetsResource.ValuesResource.GetRequest requestGet =
                    service.Spreadsheets.Values.Get(spreadsheetId, rangeGet);


            ValueRange response = requestGet.Execute();
            IList<IList<Object>> valuesRead = response.Values;
            if (valuesRead != null && valuesRead.Count > 0)
            {
                Console.WriteLine("Name, Major");
                foreach (var row in valuesRead)
                {
                    // Print columns A and E, which correspond to indices 0 and 4.
                    Console.WriteLine("{0}", row[0]);
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
            Console.Read();

            // update a cell
            IList<Object> valuesUpdate = new List<Object>();
            valuesUpdate.Add("test");

            String rangeUpdate = "plant data!A1";
            ValueRange message = new ValueRange();
            message.MajorDimension = "COLUMNS";
            message.Range = "plant data!A1";

            List<object> oblist = new List<object>() { "Write to cell" };
            message.Values = new List<IList<Object>>();
            message.Values.Add(oblist);
            SpreadsheetsResource.ValuesResource.UpdateRequest requestUpdate = service.Spreadsheets.Values.Update(message, spreadsheetId, rangeUpdate);
            requestUpdate.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            UpdateValuesResponse result = requestUpdate.Execute();
        }
    }
} //*/















using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace TCO
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool isGoogleSetup = TCOSheetsInterface.Init();

            InputHook.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (isGoogleSetup == false)
            {
                DialogMessage message = new DialogMessage("Google Sheets Error", "An error occured initializing Google Sheets service.\n\nLogs will not be written to Google Sheets.");
                message.ShowDialog();
            }

            Application.Run(new Form1());

            InputHook.End();
        }
    }
}

