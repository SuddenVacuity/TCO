using System;
using System.Windows.Forms;
using System.Drawing;

namespace TCO
{
    public class DialogGoogleSheetsWindow : Form
    {
        string m_stringTitle = "Google Sheets Information";
        string m_stringLabelText = "Please enter:" +
            "\n\t(Required) A Google Sheets doc id" +
            "\n\t(Required) The name of a  tab to add/write to" +
            "\n\t(Optional)  A description to go along with each entry" +
            "\n" +
            "\nA Google Sheets doc id can be found by opening a sheets doc" +
            "\nand copying the id from the address bar." +
            "\n\thttps://docs.google.com/spreadsheets/d/[Spreadsheet ID is Here]/edit#gid=#########" +
            "\n";
        string m_stringLabelSpreadsheetId = "Spreadsheet Id:";
        string m_stringLabelTabName = "Tab Name:";
        string m_stringLabelDescription = "Description:";
        string m_stringButtonConfirm = "Confirm";
        string m_stringButtonCancel = "Cancel";

        public string resultSpreadSheetId;
        public string resultTabName;
        public string resultDescription;

        Label m_labelText;
        Label m_labelSpreadsheetId;
        Label m_labelTabName;
        Label m_labelDescription;

        TextBox m_textBoxSpreadsheetId;
        TextBox m_textBoxTabName;
        TextBox m_textBoxDescription;

        Button m_buttonConfirm;
        Button m_buttonCancel;

        public DialogGoogleSheetsWindow(string spreadsheetId, string tabName, string description)
        {
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
            m_labelSpreadsheetId = new Label();
            m_labelTabName = new Label();
            m_labelDescription = new Label();
            m_textBoxSpreadsheetId = new TextBox();
            m_textBoxTabName = new TextBox();
            m_textBoxDescription = new TextBox();
            m_buttonConfirm = new Button();
            m_buttonCancel = new Button();

            // Size
            m_labelText.AutoSize          = true;
            m_labelSpreadsheetId.AutoSize = true;
            m_labelTabName.AutoSize       = true;
            m_labelDescription.AutoSize   = true;
            m_textBoxSpreadsheetId.Size = new Size(100, 30);
            m_textBoxTabName.Size       = new Size(80, 30);
            m_textBoxDescription.Size   = new Size(200, 30);
            m_buttonConfirm.Size = new Size(80, 40);
            m_buttonCancel.Size  = new Size(80, 40);

            // Location
            m_labelText.Location          = new Point(0, 0);
            m_labelSpreadsheetId.Location = new Point(90, 110);
            m_labelTabName.Location       = new Point(90, 140);
            m_labelDescription.Location   = new Point(90, 170);
            m_textBoxSpreadsheetId.Location = new Point(180, 110);
            m_textBoxTabName.Location       = new Point(180, 140);
            m_textBoxDescription.Location   = new Point(180, 170);
            m_buttonConfirm.Location = new Point(230, 200);
            m_buttonCancel.Location  = new Point(130, 200);

            // Text
            m_labelText.Text          = m_stringLabelText;
            m_labelSpreadsheetId.Text = m_stringLabelSpreadsheetId;
            m_labelTabName.Text       = m_stringLabelTabName;
            m_labelDescription.Text   = m_stringLabelDescription;
            m_textBoxSpreadsheetId.Text = spreadsheetId;
            m_textBoxTabName.Text       = tabName;
            m_textBoxDescription.Text   = description;
            m_buttonConfirm.Text = m_stringButtonConfirm;
            m_buttonCancel.Text  = m_stringButtonCancel;
            
            m_labelSpreadsheetId.TextAlign = ContentAlignment.BottomRight;
            m_labelTabName.TextAlign       = ContentAlignment.BottomRight;
            m_labelDescription.TextAlign   = ContentAlignment.BottomRight;

            // lambdas
            m_buttonConfirm.MouseUp += (s, e) =>
            {
                if (TCOSheetsInterface.getIsServiceActive() == false)
                {
                    DialogMessage message = new DialogMessage("Google Sheets Service Inactive", "The Google Sheets service has not been set up correctly.");
                    message.ShowDialog();
                }

                bool idSet = true;
                bool tabSet = true;
                if (m_textBoxSpreadsheetId.Text == "" ||
                    m_textBoxSpreadsheetId.Text == null)
                    idSet = false;
                if (m_textBoxTabName.Text == "" ||
                    m_textBoxTabName.Text == null)
                    tabSet = false;

                if (idSet == false && tabSet == false)
                {
                    DialogMessage message = new DialogMessage("Error", "Spreadsheet Id and Tab Name must both be set.");
                    message.ShowDialog();
                    return;
                }
                if (idSet == false)
                {
                    DialogMessage message = new DialogMessage("Error", "Spreadsheet Id must be set.");
                    message.ShowDialog();
                    return;
                }
                if(tabSet == false)
                {
                    DialogMessage message = new DialogMessage("Error", "Tab Name must be set.");
                    message.ShowDialog();
                    return;
                }

                resultSpreadSheetId = m_textBoxSpreadsheetId.Text;
                resultTabName = m_textBoxTabName.Text;
                resultDescription = m_textBoxDescription.Text;

                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            m_buttonCancel.MouseUp += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            m_labelText.BackColor = Color.White;
            m_labelSpreadsheetId.BackColor = Color.White;
            m_labelTabName.BackColor = Color.White;
            m_labelDescription.BackColor = Color.White;

            resultSpreadSheetId = spreadsheetId;
            resultTabName = tabName;
            resultDescription = description;

            this.Controls.Add(m_labelText);
            this.Controls.Add(m_labelSpreadsheetId);
            this.Controls.Add(m_labelTabName);
            this.Controls.Add(m_labelDescription);
            this.Controls.Add(m_textBoxSpreadsheetId);
            this.Controls.Add(m_textBoxTabName);
            this.Controls.Add(m_textBoxDescription);
            this.Controls.Add(m_buttonConfirm);
            this.Controls.Add(m_buttonCancel);

        }


    }
}
