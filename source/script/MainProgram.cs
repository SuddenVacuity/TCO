using System;
using System.IO;
using System.Collections.Generic;

namespace TCO
{
    public enum States
    {
        None = 0,
        /// <summary>
        /// Count and log time as normal.
        /// </summary>
        Active = 1,
        /// <summary>
        /// Warning state before timeout.
        /// </summary>
        Idle = 2,
        /// <summary>
        /// Stop counting time and remove all time since the last user action.
        /// </summary>
        TimeOut = 4,

        /// <summary>
        /// Do not add/clear/log/reset time.
        /// </summary>
        Pause = 8,

        /// <summary>
        /// Not used to manage internal program state. Used by the interface to store a state value to represent the interface state.
        /// </summary>
        NeverTimeOut = 16,
    }

    public class MainProgram
    {
        private int m_logSession = 0;
        private bool m_foundSheetAndTab = false;

        /// <summary>
        /// The sheet id used for google sheets logging
        /// </summary>
        static String m_spreadsheetId = "";
        //static String m_spreadsheetId = "1FIvf2k_LAlry76FEPReGCcsnHNQ8x_5NED0KJkWrF58";
        /// <summary>
        /// The name id of the current tab to log data to
        /// </summary>
        static string m_spreadsheetTab = "";
        //static string m_spreadsheetTab = "October";
        /// <summary>
        /// Additional information included in the log
        /// </summary>
        private string m_description = "";
        //private string m_description = "Write stuff here";
        /// <summary>
        /// The formatted sting used to display the total time.
        /// </summary>
        private string m_timeStr = "00:00:00";
        /// <summary>
        /// Set this to true to never enter timeout mode. 
        /// Continue to count and log time regardless of user interaction.
        /// </summary>
        private bool m_neverTimeout = false;
        /// <summary>
        /// The current state of the program.
        /// </summary>
        private int m_state = (int)States.Pause;
        /// <summary>
        /// The timer used to control when and how often updates occur.
        /// </summary>
        private System.Windows.Forms.Timer m_timer;

        /// <summary>
        /// Counter that accumulates time to be used to trigger logging.
        /// </summary>
        private int m_logTimeAccumulator = 0;
        /// <summary>
        /// The amount of time inbetween automatic log entries.
        /// </summary>
        private int m_logInterval = 1800000;
        /// <summary>
        /// The amount of time the user must be inactive to recieve a warning.
        /// </summary>
        private int m_idleTime = 120000;
        /// <summary>
        /// The amount of time a user must be inactive for the 
        /// time accumulator to stop and have the time since last activity removed.
        /// </summary>
        private int m_timeoutTime = 300000;
        /// <summary>
        /// The interval at which the program checks if there was activity, 
        /// adds to time, updates the time string and checks if it should log.
        /// </summary>
        private int m_pollInteval = 1000;
        /// <summary>
        /// This is time that's been logged and can't be removed
        /// </summary>
        private int m_timeConfirmed = 0;
        /// <summary>
        /// This is time time thats been accumulated and has not been logged.
        /// </summary>
        private int m_timePending = 0;
        /// <summary>
        /// This is the time since the last detected user activity.
        /// </summary>
        private int m_timeBuffer = 0;
        
        /// <summary>
        /// Event that occurs when the program updates its information.
        /// </summary>
        public event EventHandler TimerTick;

        public MainProgram()
        {
            m_timer = new System.Windows.Forms.Timer();
            m_timer.Interval = m_pollInteval;
            m_timer.Enabled = true;
            m_timer.Tick += (s, e) =>
            {
            // remove sustain from flags
            if (m_state != (int)States.Pause)
                {
                    bool isUserActive = TCO.InputHook.getActivityDetected();

                    if (isUserActive == true)
                        m_state = (int)States.Active;
                    else
                        Console.WriteLine("user is inactive " + m_timeBuffer);

                // do this if not timed out
                if (m_state != (int)States.TimeOut)
                        m_timeBuffer += m_pollInteval;

                // do this is user was active or program is in sustain mode
                if (isUserActive == true ||
                        m_neverTimeout == true)
                    {
                        m_timePending += m_timeBuffer;
                        m_logTimeAccumulator += m_timeBuffer;
                        m_timeBuffer = 0;
                    }

                    timeToString();
                    updateState();

                    if (m_timePending >= m_logInterval)
                        logTime();
                }
                EventHandler handler = TimerTick;
                if (handler != null)
                    handler(this, e);
            };
            importFileData();
            sheetsInterfaceSetup();
        }

        private void importFileData()
        {
            string filePath = System.Environment.CurrentDirectory + "\\config.prefs";
            if (File.Exists(filePath))
            {
                //FileStream file = File.Open(filePath, FileMode.Open);

                bool sheetIdFound = false;
                foreach (string line in File.ReadLines(filePath))
                {
                    if (line.Length == 0)
                        continue;
                    if (line[0] == '/')
                        continue;
                    if (line.Contains("ssid"))
                    {
                        m_spreadsheetId = line.Remove(0, line.LastIndexOf(":") + 1);
                        if (m_spreadsheetId != "")
                            sheetIdFound = true;
                    }
                    if (line.Contains("stid"))
                    {
                        m_spreadsheetTab = line.Remove(0, line.LastIndexOf(":") + 1);
                        if (sheetIdFound == true &&
                            m_spreadsheetTab != "")
                            m_foundSheetAndTab = true;
                    }
                    if (line.Contains("desc"))
                        m_description = line.Remove(0, line.LastIndexOf(":") + 1);

                    // get intervals and times
                    if (line.Contains("logi"))
                        int.TryParse(line.Remove(0, line.LastIndexOf(":") + 1), out m_logInterval);
                    if (line.Contains("idlt"))
                        int.TryParse(line.Remove(0, line.LastIndexOf(":") + 1), out m_idleTime);
                    if (line.Contains("timt"))
                        int.TryParse(line.Remove(0, line.LastIndexOf(":") + 1), out m_timeoutTime);
                    if (line.Contains("poli"))
                        int.TryParse(line.Remove(0, line.LastIndexOf(":") + 1), out m_pollInteval);

                    checkTimeLimits();
                }

            }
        }

        private void checkTimeLimits()
        {
            // create minimum values intervals and times can be
            if (m_pollInteval < 1000) // 1 second min
                m_pollInteval = 1000;
            if (m_pollInteval > 300000) // 5 minutes max
                m_pollInteval = 300000;
            if (m_logInterval < 300000) // 5 minutes min
                m_logInterval = 300000;
            if (m_idleTime < m_pollInteval) // can not be shorter than poll interval
                m_idleTime = m_pollInteval;
            if (m_timeoutTime < m_idleTime) // can not be shorter than idle time
                m_timeoutTime = m_idleTime;
        }

        private void exportFileData()
        {
            string filePath = System.Environment.CurrentDirectory + "\\config.prefs";
            //FileStream file = File.Open(filePath, FileMode.Create);

            string data =
                "// Log interval, poll interval, time until idle and time until timeout" +
                "\n//" +
                "\n// logi - the time interval between logs in milliseconds" +
                "\n//          -300000 minimum 1800000 recommended" +
                "\n// idlt - the time in milliseconds for the program to enter idle state" +
                "\n//          -Must not be less than poll interval" +
                "\n// timt - the time in milliseconds for the program to enter timeout state" +
                "\n//          -Must not be less than idle time" +
                "\n// poli - the tick rate in milliseconds of the internal timer" +
                "\n//          -1000~300000 range 1000 recommended" +
                "\n//" +
                "\n#logi:" + m_logInterval +
                "\n#idlt:" + m_idleTime +
                "\n#timt:" + m_timeoutTime +
                "\n#poli:" + m_pollInteval +
                "\n// Active sheet, tab and description from the last session" +
                "\n#ssid:" + m_spreadsheetId + 
                "\n#stid:" + m_spreadsheetTab + 
                "\n#desc:" + m_description;

            File.WriteAllText(filePath, data);
        }

        private void sheetsInterfaceSetup()
        {
            if (m_foundSheetAndTab == false)
                return;
            
            if (!TCOSheetsInterface.tabExists(m_spreadsheetId, m_spreadsheetTab))
            {
                System.Drawing.Size size = new System.Drawing.Size(5, 2);
                if (!TCOSheetsInterface.createTab(m_spreadsheetId, m_spreadsheetTab, size, 1))
                    Console.WriteLine("Create new tab [" + m_spreadsheetTab + "] failed");

                SheetsLogEntry entryMaker = new SheetsLogEntry();
                List<Google.Apis.Sheets.v4.Data.RowData> rows = entryMaker.createTitleBar();
                TCOSheetsInterface.updateCells(m_spreadsheetId, m_spreadsheetTab, "A1", "E2", rows);
            }
        }

        public string getTimeString()
        {
            return m_timeStr;
        }

        public int getState()
        {
            return m_state;
        }


        /// <summary>
        /// Pauses/Starts the program timer and returns the resulting state.
        /// </summary>
        /// <returns></returns>
        public void onClickButton()
        {
            // remove the sustained bit from the flags
            int state = m_state;
            switch (state)
            {
                case (int)States.Active: m_state = (int)States.Pause; break;
                case (int)States.Idle: m_state = (int)States.Pause; break;
                case (int)States.TimeOut: m_logTimeAccumulator = 0; m_state = (int)States.Active; break;
                case (int)States.Pause: m_logTimeAccumulator = 0; m_state = (int)States.Active; break;
                case (int)States.None: m_logTimeAccumulator = 0; m_state = (int)States.Active; break;
                default: break;
            }
        }

        public void cancelPendingTime()
        {
            m_state = (int)States.Pause;

            m_timePending = 0;
            m_timeBuffer = 0;

            timeToString();
        }

        public void logTime()
        {
            if (m_foundSheetAndTab == false)
                return;

            DialogMessage message = new DialogMessage("", "");
            if (TCOSheetsInterface.getIsServiceActive() == false)
                Console.WriteLine("Google Sheets service inactive.");
            
            if (!TCOSheetsInterface.WriteTimeNowToCell(m_spreadsheetId, m_spreadsheetTab, m_timeStr, m_description))
            {
                Console.WriteLine("Attempt to log to google sheets failed.");
                return;
            }
            
            m_logSession++;
            m_timeConfirmed += m_timePending;
            m_timePending = 0;
            m_logTimeAccumulator = 0;
            Console.WriteLine("Total time logged is " + m_timeStr);
        }

        private void updateState()
        {
            if (m_timeBuffer >= m_timeoutTime &&
                m_state != (int)States.TimeOut &&
                m_neverTimeout == false)
            {
                m_timeBuffer = 0;
                //timeToString();
                //logTime();
                Console.WriteLine("m_state = States.Stop");
                m_state = (int)States.TimeOut;
            }
            else if (m_timeBuffer >= m_idleTime &&
                     m_state != (int)States.Idle)
            {
                Console.WriteLine("m_state = States.Idle");
                m_state = (int)States.Idle;
            }
        }

        private void timeToString()
        {
            int tempTime = m_timePending + m_timeBuffer;

            int hours = tempTime / 3600000;
            tempTime %= 3600000;

            int minutes = tempTime / 60000;
            tempTime %= 60000;

            int seconds = tempTime / 1000;

            if (hours > 9) m_timeStr = hours + ":";
            else m_timeStr = "0" + hours + ":";

            if (minutes > 9) m_timeStr = m_timeStr + minutes + ":";
            else m_timeStr = m_timeStr + "0" + minutes + ":";

            if (seconds > 9) m_timeStr = m_timeStr + seconds;
            else m_timeStr = m_timeStr + "0" + seconds;
        }

        public void onClose()
        {
            logTime();
            exportFileData();
        }

        public void toggleSustainMode()
        {
            m_neverTimeout = !m_neverTimeout;

            if (m_state == (int)States.TimeOut ||
                m_state == (int)States.Idle)
                m_state = (int)States.Active;

        }
        public bool getTimeoutFlag()
        {
            return m_neverTimeout;
        }

        public string getSheetId()
        {
            return m_spreadsheetId;
        }
        public string getTabName()
        {
            return m_spreadsheetTab;
        }
        public string getEntryDescription()
        {
            return m_description;
        }
        public void setSheetsInfo(string spreadsheetId, string tabName, string description)
        {
            m_spreadsheetId = spreadsheetId;
            m_spreadsheetTab = tabName;
            m_description = description;
            
            m_foundSheetAndTab = true;
            sheetsInterfaceSetup();
        }
    }
}
