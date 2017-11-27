using System;
using System.Windows.Forms;
using System.Drawing;

namespace TCO
{
    public class DialogSheetsWindow : Form
    {
        string m_stringTitle = "Log Profiles";
        string m_stringLabelText = 
            "\n Enter log entry identifiers here and select which profile you would like to use." +
            "\n     Log Name     = A primary name to identify a log entry by" +
            "\n     Project Name = A secondary name to identify a log entry by" +
            "\n     Description  = A description of the action being logged" +
            "\n     Gid = A Google Sheets workbook id" +
            "\n" +
            "\n A Google Sheets workbook id can be found by opening a sheets document and copying the id from the address bar." +
            "\n     https://docs.google.com/spreadsheets/d/[Spreadsheet ID is Here]/edit#gid=#########" +
            "\n";
        string m_stringLabelFilepath = "Path";
        string m_stringLabelLogName = "Log Name";
        string m_stringLabelProjectName = "Project Name";
        string m_stringLabelProjectDescription = "Description";
        string m_stringLabelGoogleSheetId = "Gid";
        string m_stringButtonConfirm = "Confirm";
        string m_stringButtonCancel = "Cancel";

        public PrefsData.PrefLogIndex m_selectedLog;

        Label m_labelText;
        Label m_labelFilepath;
        Label m_labelLogName;
        Label m_labelProjectName;
        Label m_labelProjectDescription;
        Label m_labelGoogleSheetId;

        ControlLogData[] m_logData;
        PrefsData m_userPrefs;

        Button m_buttonConfirm;
        Button m_buttonCancel;
        
        public DialogSheetsWindow(PrefsData userPrefs)
        {
            m_userPrefs = userPrefs;
            m_selectedLog = m_userPrefs.getActiveSheet();

            this.ShowInTaskbar = false;
            this.BackColor = Color.LightGray;
            this.Text = m_stringTitle;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.AutoSize = true;
            
            m_labelText = new Label();
            m_labelFilepath = new Label();
            m_labelLogName = new Label();
            m_labelProjectName = new Label();
            m_labelProjectDescription = new Label();
            m_labelGoogleSheetId = new Label();

            m_buttonConfirm = new Button();
            m_buttonCancel = new Button();

            // Size
            m_labelText.Size = new Size(600, 130);
            m_labelFilepath.AutoSize = true;
            m_labelLogName.AutoSize = true;
            m_labelProjectName.AutoSize = true;
            m_labelProjectDescription.AutoSize = true;
            m_labelGoogleSheetId.AutoSize = true;
            m_buttonConfirm.Size = new Size(80, 40);
            m_buttonCancel.Size  = new Size(80, 40);

            // Location
            m_labelText.Location     = new Point(0, 0);
            m_labelFilepath.Location           = new Point(20,  5 + m_labelText.Size.Height);
            m_labelLogName.Location            = new Point(63,  5 + m_labelText.Size.Height);
            m_labelProjectName.Location        = new Point(178, 5 + m_labelText.Size.Height);
            m_labelProjectDescription.Location = new Point(293, 5 + m_labelText.Size.Height);
            m_labelGoogleSheetId.Location      = new Point(565, 5 + m_labelText.Size.Height);
            m_buttonConfirm.Location = new Point(290, 380);
            m_buttonCancel.Location  = new Point(190, 380);

            // Text
            m_labelText.Text     = m_stringLabelText;
            m_labelFilepath.Text           = m_stringLabelFilepath;
            m_labelLogName.Text            = m_stringLabelLogName;
            m_labelProjectName.Text        = m_stringLabelProjectName;
            m_labelProjectDescription.Text = m_stringLabelProjectDescription;
            m_labelGoogleSheetId.Text      = m_stringLabelGoogleSheetId;
            m_buttonConfirm.Text = m_stringButtonConfirm;
            m_buttonCancel.Text  = m_stringButtonCancel;

            // lambdas
            m_buttonConfirm.MouseUp += (s, e) =>
            {
                if (TCOSheetsInterface.getIsServiceActive() == false)
                {
                    DialogMessage message = new DialogMessage("Google Sheets Service Inactive", "The Google Sheets service has not been set up correctly.");
                    message.ShowDialog();
                }

                m_userPrefs.setActiveLog(m_selectedLog);

                for (PrefsData.PrefLogIndex i = 0; i < PrefsData.PrefLogIndex.Count; i++)
                {
                    m_userPrefs.setLogName(m_logData[(int)i].getLogName(), i);
                    m_userPrefs.setProjectName(m_logData[(int)i].getProjectName(), i);
                    m_userPrefs.setProjectDescription(m_logData[(int)i].getProjectDescription(), i);
                    m_userPrefs.setLogGoogleId(m_logData[(int)i].getGoogleSheetsId(), i);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            m_buttonCancel.MouseUp += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            m_labelText.BackColor = Color.White;
            
            // create log data rows
            PrefsData.PrefLogIndex entryCount = UserPrefs.getMaxEntries();
            m_logData = new ControlLogData[(int)entryCount];
            for (PrefsData.PrefLogIndex i = 0; i < entryCount; i++)
            {
                m_logData[(int)i] = new ControlLogData();

                int Ypos = m_labelFilepath.Location.Y + m_labelFilepath.Size.Height + (int)i * (m_logData[(int)i].m_height + 5);

                // get the log profile data from user prefs
                m_logData[(int)i].setLogPath(""); // NOT IMPLEMENTED IN MAIN PROGRAM OR USER PREFERENCES
                m_logData[(int)i].setLogName(m_userPrefs.getLogName(i));
                m_logData[(int)i].setProjectName(m_userPrefs.getProjectName(i));
                m_logData[(int)i].setProjectDescription(m_userPrefs.getProjectDescription(i));
                m_logData[(int)i].setGoogleSheetsId(m_userPrefs.getLogGoogleId(i));

                // create a new instance of the iteration in memory to be used as an index in on CheckedChanged lambda
                PrefsData.PrefLogIndex radioIndex = i; // NOTE: if this isn't done the value used in the lambda will always be PrefLogIndex.Count

                // create radio buttun used to select the log
                RadioButton selectLog = new RadioButton();
                selectLog.Size = new Size(15, 15);
                selectLog.Location = new Point(0, Ypos);

                if (i == m_selectedLog)
                    selectLog.Checked = true;

                selectLog.CheckedChanged += (s, e) => {
                    RadioButton rb = (RadioButton)s;
                    if (rb.Checked == true)
                        m_selectedLog = radioIndex;
                };

                // set position in dialog
                m_logData[(int)i].Location = new Point(selectLog.Location.X + selectLog.Size.Width + 5, Ypos);

                this.Controls.Add(selectLog);
                this.Controls.Add(m_logData[(int)i]);
            } // END create log data rows
            
            this.Controls.Add(m_labelText);
            this.Controls.Add(m_labelFilepath);
            this.Controls.Add(m_labelLogName);
            this.Controls.Add(m_labelProjectName);
            this.Controls.Add(m_labelProjectDescription);
            this.Controls.Add(m_labelGoogleSheetId);
            this.Controls.Add(m_buttonConfirm);
            this.Controls.Add(m_buttonCancel);
        }
    }
}
