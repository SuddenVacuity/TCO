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
    public class SheetsLogEntry
    {
        RowData rowData = new RowData();
        CellFormat m_formatTitle;
        CellFormat m_formatMonth;
        CellFormat m_formatDay;
        CellFormat m_formatText;
        Google.Apis.Sheets.v4.Data.Color m_backColor;
        Google.Apis.Sheets.v4.Data.Color m_backColorTitle;
        Google.Apis.Sheets.v4.Data.Color m_backColorHighlight;

        public SheetsLogEntry()
        {
            rowData.Values = new List<CellData>();

            // Colors
            m_backColor = new Google.Apis.Sheets.v4.Data.Color();
            m_backColor.Red = 0.5764705882352941f;
            m_backColor.Green = 0.7686274509803922f;
            m_backColor.Blue = 0.4901960784313725f;

            m_backColorTitle = new Google.Apis.Sheets.v4.Data.Color();
            m_backColorTitle.Red = 0.4156862745098039f;
            m_backColorTitle.Green = 0.6588235294117647f;
            m_backColorTitle.Blue = 0.3098039215686275f;

            m_backColorHighlight = new Google.Apis.Sheets.v4.Data.Color();
            m_backColorHighlight.Red = 0.7137254901960784f;
            m_backColorHighlight.Green = 0.8431372549019608f;
            m_backColorHighlight.Blue = 0.6588235294117647f;

            // CellFormat
            m_formatTitle = new CellFormat();
            m_formatTitle.TextFormat = new TextFormat();
            m_formatTitle.TextFormat.Bold = true;
            m_formatTitle.TextFormat.Underline = true;
            m_formatTitle.BackgroundColor = m_backColorTitle;

            m_formatMonth = new CellFormat();
            m_formatMonth.NumberFormat = new NumberFormat();
            m_formatMonth.NumberFormat.Pattern = "MMM dd";
            m_formatMonth.NumberFormat.Type = "DATE";
            m_formatMonth.BackgroundColor = m_backColor;

            m_formatDay = new CellFormat();
            m_formatDay.NumberFormat = new NumberFormat();
            m_formatDay.NumberFormat.Pattern = "DDD";
            m_formatDay.NumberFormat.Type = "DATE";
            m_formatDay.BackgroundColor = m_backColor;

            m_formatText = new CellFormat();
            m_formatText.TextFormat = new TextFormat();
            m_formatText.TextFormat.Bold = false;
            m_formatText.TextFormat.Underline = false;
            m_formatText.BackgroundColor = m_backColor;
            
        }

        public List<RowData> createTitleBar()
        {
            string monthDay = "Date";
            string dayName = "Day";
            string timeStr = "Time";
            string descriptionStr = "Description";

            ExtendedValue valEmpty = new ExtendedValue();
            ExtendedValue valMonthDay = new ExtendedValue();
            ExtendedValue valDayName = new ExtendedValue();
            ExtendedValue valTime = new ExtendedValue();
            ExtendedValue valDescription = new ExtendedValue();

            valEmpty.StringValue = "";
            valMonthDay.StringValue = monthDay;
            valDayName.StringValue = dayName;
            valTime.StringValue = timeStr;
            valDescription.StringValue = descriptionStr;

            CellData cellEmpty = new CellData();
            CellData cellMonthDay = new CellData();
            CellData cellDayName = new CellData();
            CellData cellTime = new CellData();
            CellData cellDescription = new CellData();

            CellFormat formatTitle = m_formatTitle;

            cellEmpty.UserEnteredValue = valEmpty;
            cellEmpty.UserEnteredFormat = formatTitle;

            cellMonthDay.UserEnteredValue = valMonthDay;
            cellMonthDay.UserEnteredFormat = formatTitle;
            cellDayName.UserEnteredValue = valDayName;
            cellDayName.UserEnteredFormat = formatTitle;
            cellTime.UserEnteredValue = valTime;
            cellTime.UserEnteredFormat = formatTitle;
            cellDescription.UserEnteredValue = valDescription;
            cellDescription.UserEnteredFormat = formatTitle;

            RowData rowData1 = new RowData();
            rowData1.Values = new List<CellData>();
            rowData1.Values.Add(cellEmpty);
            rowData1.Values.Add(cellMonthDay);
            rowData1.Values.Add(cellDayName);
            rowData1.Values.Add(cellTime);
            rowData1.Values.Add(cellDescription);
            
            RowData rowData2 = new RowData();
            rowData2.Values = new List<CellData>();
            rowData2.Values.Add(cellEmpty);
            rowData2.Values.Add(cellEmpty);
            rowData2.Values.Add(cellEmpty);
            rowData2.Values.Add(cellEmpty);
            rowData2.Values.Add(cellEmpty);

            List<RowData> rowsData = new List<RowData>();
            rowsData.Add(rowData1);
            rowsData.Add(rowData2);

            return rowsData;
        }

        /// <summary>
        /// Return RowData with 5 cells. [empty] [MM dd] [DDD] [hh:mm:ss] [description]
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public RowData addEntry(string timeStr, string description)
        {
            ExtendedValue valEmpty = new ExtendedValue();
            ExtendedValue valMonthDay = new ExtendedValue();
            ExtendedValue valDayName = new ExtendedValue();
            ExtendedValue valTime = new ExtendedValue();
            ExtendedValue valDescription = new ExtendedValue();
            
            valEmpty.StringValue = "";
            //valMonthDay.FormulaValue = "=TEXT("+ DateTime.Now.Month + ",\"MMM\")&\" \"&TEXT("+ DateTime.Now.Day + ",\"dd\")";
            //valDayName.FormulaValue = "=TEXT(\"" + DateTime.Now.Month + "\"/\"" + DateTime.Now.Day + "\"/\"" + DateTime.Now.Year + "\", \"DDD\")";
            //valDayName.FormulaValue = "=TEXT("+DateTime.Now.DayOfWeek+", \"DDD\"";
            valMonthDay.FormulaValue = "=DATE(" + DateTime.Now.Year + "," + DateTime.Now.Month + "," + DateTime.Now.Day + ")";
            valDayName.FormulaValue = "=DATE(" + DateTime.Now.Year + "," + DateTime.Now.Month + "," + DateTime.Now.Day + ")";
            valTime.StringValue = timeStr;
            valDescription.StringValue = description;
            
            CellData cellEmpty = new CellData();
            CellData cellMonthDay = new CellData();
            CellData cellDayName = new CellData();
            CellData cellTime = new CellData();
            CellData cellDescription = new CellData();
            
            CellFormat formatText = m_formatText;
            CellFormat formatMonth = m_formatMonth;
            CellFormat formatDay = m_formatDay;

            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday ||
                DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            {
                formatText.BackgroundColor = m_backColorHighlight;
                formatMonth.BackgroundColor = m_backColorHighlight;
                formatDay.BackgroundColor = m_backColorHighlight;
            }

            cellEmpty.UserEnteredValue = valEmpty;
            cellEmpty.UserEnteredFormat = formatText;
            cellMonthDay.UserEnteredValue = valMonthDay;
            cellMonthDay.UserEnteredFormat = formatMonth;
            cellDayName.UserEnteredValue = valDayName;
            cellDayName.UserEnteredFormat = formatDay;
            cellTime.UserEnteredValue = valTime;
            cellTime.UserEnteredFormat = formatText;
            cellDescription.UserEnteredValue = valDescription;
            cellDescription.UserEnteredFormat = formatText;
            
            RowData rowData = new RowData();
            rowData.Values = new List<CellData>();
            rowData.Values.Add(cellEmpty);
            rowData.Values.Add(cellMonthDay);
            rowData.Values.Add(cellDayName);
            rowData.Values.Add(cellTime);
            rowData.Values.Add(cellDescription);
            
            return rowData;
        }



    }
}

