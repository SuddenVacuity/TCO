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
        public static readonly string VERSION = "v0.0.1.0";

        private bool m_foundGoogleSheetAndTab = false;
        private PrefsData m_userPrefs = null;

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
        /// The interval at which the program checks if there was activity, 
        /// adds to time, updates the time string and checks if it should log.
        /// </summary>
        private int m_pollInterval = 1000;
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
            m_timer.Interval = m_pollInterval;
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
                        m_timeBuffer += m_pollInterval;

                // do this is user was active or program is in sustain mode
                if (isUserActive == true ||
                        m_neverTimeout == true)
                    {
                        m_timePending += m_timeBuffer;
                        m_logTimeAccumulator += m_timeBuffer;
                        m_timeBuffer = 0;
                    }
               
                    timeToString();
                    updateTodaysLogEntries();
                    updateState();

                    if (m_timePending >= m_userPrefs.getInt(PrefsData.PrefInt.LogInterval))
                        logTime();
                }
                EventHandler handler = TimerTick;
                if (handler != null)
                    handler(this, e);
            };
            importFileData();
            googleSheetsInterfaceSetup();
        }

        private void importFileData()
        {
            m_userPrefs = UserPrefs.getPrefs();

            PrefsData.PrefLogIndex index = m_userPrefs.getActiveSheet();

            LocalLogger.loadTodayLogsFromFile();
            m_timeStr = LocalLogger.getTodaysEntryData(
                m_userPrefs.getLogName(index),
                m_userPrefs.getProjectName(index),
                m_userPrefs.getProjectDescription(index),
                LocalLogger.EntryDataIndex.Time);
            m_timeConfirmed = timeStringToMilliseconds(m_timeStr);

            checkTimeLimits();
        }

        private void checkTimeLimits()
        {
            // create minimum values intervals and times can be
            if (m_pollInterval < 1000) // 1 second min
                m_pollInterval = 1000;
            if (m_pollInterval > 300000) // 5 minutes max
                m_pollInterval = 300000;
            if (m_userPrefs.getInt(PrefsData.PrefInt.LogInterval) < 300000) // 5 minutes min
                m_userPrefs.setInt(300000, PrefsData.PrefInt.LogInterval);
            if (m_userPrefs.getInt(PrefsData.PrefInt.IdleTime) < m_pollInterval) // can not be shorter than poll interval
                m_userPrefs.setInt(m_pollInterval, PrefsData.PrefInt.LogInterval);
            if (m_userPrefs.getInt(PrefsData.PrefInt.TimeoutTime) < m_userPrefs.getInt(PrefsData.PrefInt.IdleTime)) // can not be shorter than idle time
                m_userPrefs.setInt(m_userPrefs.getInt(PrefsData.PrefInt.IdleTime), PrefsData.PrefInt.TimeoutTime);
        }

        private void exportFileData()
        {
            UserPrefs.setPrefs(m_userPrefs);
        }

        private void googleSheetsInterfaceSetup()
        {
            // not run if the target isn't set up within the program
            if (m_foundGoogleSheetAndTab == false)
                return;

            PrefsData.PrefLogIndex index = m_userPrefs.getActiveSheet();

            string googleSheetsId = m_userPrefs.getLogGoogleId(index);
            string tabName = m_userPrefs.getProjectName(index);

            if (googleSheetsId == "" || tabName == "")
            {
                DialogMessage message = new DialogMessage("Information", "A Google Sheets id and tab name must be set in order to use google sheets interface.\nInteraction with google sheet will not function.");
                message.ShowDialog();
                return;
            }
            
            // check wth google sheets to see if the tab needs to be created
            if (!TCOSheetsInterface.tabExists(m_userPrefs.getLogGoogleId(index), m_userPrefs.getProjectName(index)))
            {
                System.Drawing.Size size = new System.Drawing.Size(5, 2);
                if (!TCOSheetsInterface.createTab(m_userPrefs.getLogGoogleId(index), m_userPrefs.getProjectName(index), size, 1))
                    Console.WriteLine("Create new tab [" + m_userPrefs.getProjectName(index) + "] failed");

                // create the header bar in the created tab
                SheetsLogEntry entryMaker = new SheetsLogEntry();
                List<Google.Apis.Sheets.v4.Data.RowData> rows = entryMaker.createTitleBar();
                TCOSheetsInterface.updateCells(
                    m_userPrefs.getLogGoogleId(index), 
                    m_userPrefs.getProjectName(index), 
                    "A1", "E2", rows);
            }
        }

        public string getTimeString()
        {
            return m_timeStr;
        }

        public string getDescString()
        {
            return m_userPrefs.getProjectDescription(m_userPrefs.getActiveSheet());
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
            switch (m_state)
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
            // run only if write to local file succeeds
            if (LocalLogger.writeToLogFile())
            {
                if (m_foundGoogleSheetAndTab == false)
                    return;

                PrefsData.PrefLogIndex index = m_userPrefs.getActiveSheet();

                if (TCOSheetsInterface.getIsServiceActive() == false)
                    Console.WriteLine("Google Sheets service inactive.");

                if (!TCOSheetsInterface.WriteTimeNowToCell(
                    m_userPrefs.getLogGoogleId(index), 
                    m_userPrefs.getProjectName(index), 
                    m_timeStr, m_userPrefs.getProjectDescription(index)))
                {
                    Console.WriteLine("Attempt to log to google sheets failed.");
                    return;
                }
                
                m_timeConfirmed += m_timePending;
                m_timePending = 0;
                m_logTimeAccumulator = 0;
                Console.WriteLine("Total time logged is " + m_timeStr);
            }
            else
            {
                Console.WriteLine("Log failed");
            }
        }

        private void updateState()
        {
            if (m_timeBuffer >= m_userPrefs.getInt(PrefsData.PrefInt.TimeoutTime) &&
                m_state != (int)States.TimeOut &&
                m_neverTimeout == false)
            {
                m_timeBuffer = 0;
                //timeToString();
                //logTime();
                Console.WriteLine("m_state = States.Stop");
                m_state = (int)States.TimeOut;
            }
            else if (m_timeBuffer >= m_userPrefs.getInt(PrefsData.PrefInt.IdleTime) &&
                     m_state != (int)States.Idle)
            {
                Console.WriteLine("m_state = States.Idle");
                m_state = (int)States.Idle;
            }
        }

        private void timeToString()
        {
            int tempTime = m_timeConfirmed + m_timePending + m_timeBuffer;

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

        private int timeStringToMilliseconds(string timeStr)
        {
            int result = 0;
            string[] time = timeStr.Split(':');

            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            int.TryParse(time[0], out hours);
            int.TryParse(time[1], out minutes);
            int.TryParse(time[2], out seconds);

            result = (((((hours * 60) + minutes) * 60) + seconds) * 1000);
            
            return result;
        }

        private void updateTodaysLogEntries()
        {
            PrefsData.PrefLogIndex index = m_userPrefs.getActiveSheet();
            LocalLogger.updateTodaysLogs(m_timeStr,
                m_userPrefs.getLogName(index), 
                m_userPrefs.getProjectName(index), 
                m_userPrefs.getProjectDescription(index));
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

        public string getLogGoogleId()
        {
            return m_userPrefs.getLogGoogleId(m_userPrefs.getActiveSheet());
        }
        public string getProjectName()
        {
            return m_userPrefs.getProjectName(m_userPrefs.getActiveSheet());
        }
        public string getProjectDescription()
        {
            return m_userPrefs.getProjectDescription(m_userPrefs.getActiveSheet());
        }
        public string getLogName()
        {
            return m_userPrefs.getLogName(m_userPrefs.getActiveSheet());
        }

        /// <summary>
        /// Changes active the time and google sheets id to match the input log
        /// </summary>
        /// <param name="index"></param>
        private void changeActiveLogData(PrefsData.PrefLogIndex index)
        {
            // try to get time from memory
            m_timeStr = LocalLogger.getTodaysEntryData(
                m_userPrefs.getLogName(index), 
                m_userPrefs.getProjectName(index), 
                m_userPrefs.getProjectDescription(index), 
                LocalLogger.EntryDataIndex.Time);
            m_timeConfirmed = timeStringToMilliseconds(m_timeStr);
            m_timePending = 0;
            m_timeBuffer = 0;

            // let the program know a valid log+project+desc has been set
            m_foundGoogleSheetAndTab = true;

            // compare data with google sheets and act accordingly
            googleSheetsInterfaceSetup();
        }

        /// <summary>
        /// Changes the active log to the input log.
        /// <para>Returns false if the log doesn't exist</para>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool setActiveLog(PrefsData.PrefLogIndex index)
        {
            if (m_userPrefs.setActiveLog(index) == false)
                return false;

            changeActiveLogData(index);
            Console.WriteLine("Active Sheet set to " + (int)index);

            return true;
        }
        
        public PrefsData getPrefData()
        {
            return m_userPrefs;
        }
    }
}
