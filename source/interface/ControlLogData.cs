using System;
using System.Drawing;
using System.Windows.Forms;

public class ControlLogData : Panel
{
    private int m_margin = 15;

    // data - likely redundant
    private string m_logPath = "";
    private string m_logName = "";
    private string m_projectName = "";
    private string m_projectDescription = "";
    private string m_googleSheetsId = "";

    // handles for contained controls
    //private TextBox m_textBoxLogPath;
    private TextBox m_textBoxLogName;
    private TextBox m_textBoxProjectName;
    private TextBox m_textBoxProjectDescription;
    private TextBox m_textBoxGoogleSheetsId;

    public readonly int m_height = 15;
    
    public ControlLogData()
    {
        this.AutoSize = true;
        this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.BorderStyle = BorderStyle.FixedSingle;
        
        // TEXTBOXES
        //m_textBoxLogPath = new TextBox();
        m_textBoxLogName = new TextBox();
        m_textBoxProjectName = new TextBox();
        m_textBoxProjectDescription = new TextBox();
        m_textBoxGoogleSheetsId = new TextBox();

        //m_textBoxLogPath.Size            = new Size(30, m_height);
        m_textBoxLogName.Size            = new Size(100, m_height);
        m_textBoxProjectName.Size        = new Size(100, m_height);
        m_textBoxProjectDescription.Size = new Size(250, m_height);
        m_textBoxGoogleSheetsId.Size     = new Size(30, m_height);
        
        //m_textBoxLogPath.Location            = new Point(0, 0);
        m_textBoxLogName.Location            = new Point(30  + m_margin, 0);
        m_textBoxProjectName.Location        = new Point(m_textBoxLogName.Location.X + m_textBoxLogName.Size.Width + m_margin, 0);
        m_textBoxProjectDescription.Location = new Point(m_textBoxProjectName.Location.X   + m_textBoxProjectName.Size.Width   + m_margin, 0);
        m_textBoxGoogleSheetsId.Location     = new Point(m_textBoxProjectDescription.Location.X + m_textBoxProjectDescription.Size.Width + m_margin, 0);
        
        //m_textBoxLogPath.Text            = m_logPath;
        m_textBoxLogName.Text            = m_logName;
        m_textBoxProjectName.Text        = m_projectName;
        m_textBoxProjectDescription.Text = m_projectDescription;
        m_textBoxGoogleSheetsId.Text     = m_googleSheetsId;

        //m_textBoxLogPath.TextAlign = HorizontalAlignment.Left;
        m_textBoxLogName.TextAlign = HorizontalAlignment.Left;
        m_textBoxProjectName.TextAlign = HorizontalAlignment.Left;
        m_textBoxProjectDescription.TextAlign = HorizontalAlignment.Left;
        m_textBoxGoogleSheetsId.TextAlign = HorizontalAlignment.Left;
        
        // LAMBDAS
        //m_textBoxLogPath.TextChanged += (s, e)            => { m_logPath = m_textBoxLogPath.Text; };
        m_textBoxLogName.TextChanged += (s, e)            => { m_logName = m_textBoxLogName.Text; };
        m_textBoxProjectName.TextChanged += (s, e)        => { m_projectName = m_textBoxProjectName.Text; };
        m_textBoxProjectDescription.TextChanged += (s, e) => { m_projectDescription = m_textBoxProjectDescription.Text; };
        m_textBoxGoogleSheetsId.TextChanged += (s, e)     => {  m_googleSheetsId = m_textBoxGoogleSheetsId.Text; };
        
        // ADD CONTROLS
        //this.Controls.Add(m_textBoxLogPath);
        this.Controls.Add(m_textBoxLogName);
        this.Controls.Add(m_textBoxProjectName);
        this.Controls.Add(m_textBoxProjectDescription);
        this.Controls.Add(m_textBoxGoogleSheetsId);
    }
    
    // GET/SET FUNCTIONS
    public void setLogPath(string path) // NOT IMPLEMENTED IN MAIN PROGRAM OR USER PREFERENCES
    {
        //m_logPath = path;
        //m_textBoxLogPath.Text = path;
    }
    public void setLogName(string name)
    {
        m_logName = name;
        m_textBoxLogName.Text = name;
    }
    public void setProjectName(string name)
    {
        m_projectName = name;
        m_textBoxProjectName.Text = name;
    }
    public void setProjectDescription(string description)
    {
        m_projectDescription = description;
        m_textBoxProjectDescription.Text = description;
    }
    public void setGoogleSheetsId(string googleSheetId)
    {
        m_googleSheetsId = googleSheetId;
        m_textBoxGoogleSheetsId.Text = googleSheetId;
    }

    public string getLogPath() { return  m_logPath; }
    public string getLogName() { return m_logName; }
    public string getProjectName() { return m_projectName; }
    public string getProjectDescription() {  return m_projectDescription; }
    public string getGoogleSheetsId() { return m_googleSheetsId; }

    public void setAllLogsData(string logPath, string logName, string projectName, string sheetDescription, string googleSheetsId)
    {
        setLogPath(logPath);
        setLogName(logName);
        setProjectName(projectName);
        setProjectDescription(sheetDescription);
        setGoogleSheetsId(googleSheetsId);
    }
}
