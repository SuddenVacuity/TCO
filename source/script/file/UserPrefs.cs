using System;
using System.IO;


namespace TCO
{
    public class PrefsData
    {
        public enum PrefInt
        {
            IdleTime,
            TimeoutTime,
            LogInterval,
            ActiveLog,
            Count,
        }
        public enum PrefLogIndex
        {
            Log01,
            Log02,
            Log03,
            Log04,
            Log05,
            Log06,
            Log07,
            Log08,
            Log09,
            Log10,
            Count,
        }
        private static readonly int m_maxLogs = 10;
        private static readonly int m_maxInts = 10;
        
        private static string m_version = null;

        private string[] m_logNames = new string[m_maxLogs];
        private string[] m_projectNames = new string[m_maxLogs];
        private string[] m_projectDescriptions = new string[m_maxLogs];
        private string[] m_LogGoogleIds = new string[m_maxLogs];
        private int[] m_ints = new int[m_maxInts];

        public PrefsData()
        {
            if ((int)PrefInt.Count > m_maxInts ||
                (int)PrefLogIndex.Count > m_maxLogs)
                throw new IndexOutOfRangeException("PrefData Log index out of range");

            for (int i = 0; i < m_maxLogs; i++)
            {
                m_logNames[i] = null;
                m_projectNames[i] = null;
                m_projectDescriptions[i] = null;
                m_LogGoogleIds[i] = null;
            }
            for (int i = 0; i < m_maxInts; i++)
                m_ints[i] = int.MinValue;
        }

        public bool setInt(int value, PrefInt index)
        {
            if ((int)index >= (int)PrefInt.Count)
                return false;

            m_ints[(int)index] = value;
            return true;
        }
        public int getInt(PrefInt index)
        {
            if ((int)index >= (int)PrefInt.Count)
                return int.MinValue;

            return m_ints[(int)index];
        }

        public bool setLogGoogleId(string value, PrefLogIndex index)
        {
            if ((int)index >= (int)PrefLogIndex.Count)
                return false;

            m_LogGoogleIds[(int)index] = value;
            return true;
        }
        public string getLogGoogleId(PrefLogIndex index)
        {
            if ((int)index >= (int)PrefLogIndex.Count)
                return null;

            return m_LogGoogleIds[(int)index];
        }
        public bool setLogName(string value, PrefLogIndex index)
        {
            if ((int)index >= (int)PrefLogIndex.Count)
                return false;

            m_logNames[(int)index] = value;
            return true;
        }
        public string getLogName(PrefLogIndex index)
        {
            if ((int)index >= (int)PrefLogIndex.Count)
                return null;

            return m_logNames[(int)index];
        }
        public bool setProjectName(string value, PrefLogIndex index)
        {
            if ((int)index >= (int)PrefLogIndex.Count)
                return false;

            m_projectNames[(int)index] = value;
            return true;
        }
        public string getProjectName(PrefLogIndex index)
        {
            if ((int)index >= (int)PrefLogIndex.Count)
                return null;

            return m_projectNames[(int)index];
        }
        public bool setProjectDescription(string value, PrefLogIndex index)
        {
            if ((int)index >= (int)PrefLogIndex.Count)
                return false;

            m_projectDescriptions[(int)index] = value;
            return true;
        }
        public string getProjectDescription(PrefLogIndex index)
        {
            if ((int)index >= (int)PrefLogIndex.Count)
                return null;

            return m_projectDescriptions[(int)index];
        }
        public bool setActiveLog(PrefLogIndex index)
        {
            if (index >= PrefLogIndex.Count)
                return false;

            m_ints[(int)PrefsData.PrefInt.ActiveLog] = (int)index;
            return true;
        }
        public PrefLogIndex getActiveSheet()
        {
            return (PrefLogIndex)m_ints[(int)PrefsData.PrefInt.ActiveLog];
        }

        public bool setVersion(string version)
        {
            m_version = version;
            return true;
        }
        public string getVersion()
        {
            return m_version;
        }

    }
    public static class UserPrefs
    {
        private static string m_intToken = "#";
        private static string m_stringToken = "&";
        private static string m_versionToken = "v";
        private static string m_commentToken = "/";
        private static string m_dataSeperator = ":";
        private static string m_sheetsSeperator = "\t";
        
        private static int m_defaultLogInterval = 1800000;
        private static int m_defaultIdleTime = 120000;
        private static int m_defaultTimeoutTime = 300000;
        private static int m_defaultActiveLog = (int)PrefsData.PrefLogIndex.Log01;

        private static string[] m_dataTokens = {
            "idlt", // idle time
            "timt", // timeout time
            "logi", // log interval
            "acti", // active spreadsheet
        };
        
        private static string m_fileName = "\\userPrefs.bin";
        
        // public middle-man class
        public static PrefsData getPrefs()
        {
            return loadPrefs();
        }

        // public middle-man class
        public static void setPrefs(PrefsData userPrefs)
        {
            savePrefs(userPrefs);
        }

        private static PrefsData loadPrefs()
        {
            PrefsData result = new PrefsData();
            
            string filePath = System.Environment.CurrentDirectory + m_fileName;
            if (File.Exists(filePath))
            {
                PrefsData.PrefLogIndex currentSpreadheetIndex = PrefsData.PrefLogIndex.Log01;
                foreach (string line in File.ReadLines(filePath))
                {
                    if (line.Length == 0)
                        continue;
                    if (line.StartsWith(m_commentToken))
                        continue;

                    if (line.StartsWith(m_versionToken))
                        checkVersion(line.TrimStart(m_versionToken.ToCharArray()));

                    // get ints
                    if (line.StartsWith(m_intToken))
                    {
                        // remove the line token that shows this line contains int data
                        string intData = line.Remove(0, m_intToken.Length);

                        string logInterval = m_dataTokens[(int)PrefsData.PrefInt.LogInterval];
                        string idleTime = m_dataTokens[(int)PrefsData.PrefInt.IdleTime];
                        string timeoutTime = m_dataTokens[(int)PrefsData.PrefInt.TimeoutTime];
                        string ActiveLog = m_dataTokens[(int)PrefsData.PrefInt.ActiveLog];

                        if (intData.StartsWith(logInterval))
                        {
                            int defaultValue = m_defaultLogInterval;
                            int.TryParse(intData.Remove(0, intData.LastIndexOf(m_dataSeperator) + 1), out defaultValue);
                            result.setInt(defaultValue, PrefsData.PrefInt.LogInterval);
                        }

                        if (intData.StartsWith(idleTime))
                        {
                            int defaultValue = m_defaultIdleTime;
                            int.TryParse(intData.Remove(0, intData.LastIndexOf(m_dataSeperator) + 1), out defaultValue);
                            result.setInt(defaultValue, PrefsData.PrefInt.IdleTime);
                        }

                        if (intData.StartsWith(timeoutTime))
                        {
                            int defaultValue = m_defaultTimeoutTime;
                            int.TryParse(intData.Remove(0, intData.LastIndexOf(m_dataSeperator) + 1), out defaultValue);
                            result.setInt(defaultValue, PrefsData.PrefInt.TimeoutTime);
                        }

                        if (intData.StartsWith(ActiveLog))
                        {
                            int defaultValue = m_defaultActiveLog;
                            int.TryParse(intData.Remove(0, intData.LastIndexOf(m_dataSeperator) + 1), out defaultValue);
                            
                            result.setInt(defaultValue, PrefsData.PrefInt.ActiveLog);
                        }
                    } // END get ints

                    // get strings
                    if (line.StartsWith(m_stringToken))
                    {
                        // remove the line token shwoing this line contains a string of strings of data
                        // then split each string into its own string
                        string[] strData = (line.Remove(0, m_stringToken.Length))
                                                .Split(m_sheetsSeperator.ToCharArray());

                        // if token was the only character continue
                        if (strData.Length == 0)
                            continue;
                        // if data storage is full continue
                        if (currentSpreadheetIndex >= PrefsData.PrefLogIndex.Count)
                            continue;
                        
                        result.setLogName(strData[0], currentSpreadheetIndex);
                        result.setProjectName(strData[1], currentSpreadheetIndex);
                        result.setProjectDescription(strData[2], currentSpreadheetIndex);
                        result.setLogGoogleId(strData[3], currentSpreadheetIndex);

                        currentSpreadheetIndex++;
                    } // END get strings
                }
                Console.WriteLine("Pref file loaded");
            }
            else // file did not exist
            {
                Console.WriteLine("Pref file not found");

                // create a default user preference file
                result.setInt(m_defaultLogInterval, PrefsData.PrefInt.LogInterval);
                result.setInt(m_defaultIdleTime, PrefsData.PrefInt.IdleTime);
                result.setInt(m_defaultTimeoutTime, PrefsData.PrefInt.TimeoutTime);
                result.setInt(m_defaultActiveLog, PrefsData.PrefInt.ActiveLog);

                // preset first log profile
                result.setLogName("Default Log", 0);
                result.setProjectName("Default Project", 0);
                result.setProjectDescription("Default Description", 0);
                result.setLogGoogleId("", 0);

                // set the other profiles as empty
                for (PrefsData.PrefLogIndex i = PrefsData.PrefLogIndex.Log02; i < PrefsData.PrefLogIndex.Count; i++)
                {
                    result.setLogName("", i);
                    result.setProjectName("", i);
                    result.setProjectDescription("", i);
                    result.setLogGoogleId("", i);
                }
            }
            
            return result;
        }

        private static void savePrefs(PrefsData userPrefs)
        {
            string filePath = System.Environment.CurrentDirectory + m_fileName;
            
            int logInterval = userPrefs.getInt(PrefsData.PrefInt.LogInterval);
            int idleTime = userPrefs.getInt(PrefsData.PrefInt.IdleTime);
            int timeoutTime = userPrefs.getInt(PrefsData.PrefInt.TimeoutTime);
            int activeSheet = userPrefs.getInt(PrefsData.PrefInt.ActiveLog);

            string data =
                m_versionToken + Environment.getVersion() + 
                "\n// Log interval, poll interval, time until idle and time until timeout" +
                "\n" + m_intToken + m_dataTokens[(int)PrefsData.PrefInt.LogInterval] + m_dataSeperator + logInterval +
                "\n" + m_intToken + m_dataTokens[(int)PrefsData.PrefInt.IdleTime]    + m_dataSeperator + idleTime +
                "\n" + m_intToken + m_dataTokens[(int)PrefsData.PrefInt.TimeoutTime] + m_dataSeperator + timeoutTime +
                "\n" + m_intToken + m_dataTokens[(int)PrefsData.PrefInt.ActiveLog]   + m_dataSeperator + activeSheet +
                "\n//";

            for (PrefsData.PrefLogIndex i = 0; i < PrefsData.PrefLogIndex.Count; i++)
                data += "\n" +
                    m_stringToken + userPrefs.getLogName(i) + 
                    m_sheetsSeperator + userPrefs.getProjectName(i) + 
                    m_sheetsSeperator + userPrefs.getProjectDescription(i) +
                    m_sheetsSeperator + userPrefs.getLogGoogleId(i);

        File.WriteAllText(filePath, data);
            Console.WriteLine("Pref file saved");
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// <para>Compares the current version to the version loaded from the file and creates a new file in the current version format. </para>
        /// <para>Intended to account for file format changes in between versions. Not intended to be backwards compatible.</para>
        /// </summary>
        public static void checkVersion(string versionString)
        {
            if (versionString == Environment.getVersion())
                return;

            Console.WriteLine("User Preferences file version does not match the application version.");
            // handle version differences
        }

        public static PrefsData.PrefLogIndex getMaxEntries()
        {
            return PrefsData.PrefLogIndex.Count;
        }
    }
}
