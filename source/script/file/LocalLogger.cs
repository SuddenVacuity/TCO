using System;
using System.IO;

// 
namespace TCO
{
    public static class LocalLogger
    {
        public enum EntryDataIndex
        {
            /// <summary>
            /// The date of the entry in format mm/dd/yyyy
            /// </summary>
            Date,
            /// <summary>
            /// The day of the week of the entry
            /// </summary>
            Day,
            /// <summary>
            /// The time logged to the entry
            /// </summary>
            Time,
            /// <summary>
            /// The name of the log book the entry is stored in
            /// </summary>
            LogName,
            /// <summary>
            /// A name related to the project time is logged for
            /// </summary>
            ProjectName,
            /// <summary>
            /// A description of the project time is logged for
            /// </summary>
            ProjectDescription,
            /// <summary>
            /// The google sheets id of a workbook to write log information to
            /// </summary>
            GoogleSheetsId,

            Count
        }
        
        /// <summary>
        /// The character that seperates each week's entries in the log.
        /// <para></para>
        /// This character is copied several times to make a line to improve human readability
        /// </summary>
        private static string m_weekToken = "-";
        /// <summary>
        /// The character that seperates each pieces of data in an entry
        /// </summary>
        private static string m_dataSeperator = "\t";
        /// <summary>
        /// The character that indicates this line contains the version of the application that last wrote the log file
        /// </summary>
        private static string m_versionToken = "v";
        /// <summary>
        /// The character the indicates the line should be skipped
        /// </summary>
        private static string m_commentToken = "//";
        /// <summary>
        /// The character that seperates each entry in the log
        /// </summary>
        private static string m_lineToken = "\r\n";

        /// <summary>
        /// The format to write the date in
        /// </summary>
        private static string m_dateFormat = "MM/dd/yyyy";
        /// <summary>
        /// The format to write the weekday in
        /// </summary>
        private static string m_dayFormat = "ddd";

        /// <summary>
        /// Stores all of the logs for today
        /// </summary>
        private static string[] m_todaysLogs = { };
        /// <summary>
        /// The date the program last updated it's log
        /// </summary>
        private static DateTime m_date;
        /// <summary>
        /// The day of the week the program last updated it's log
        /// </summary>
        private static DayOfWeek m_today;

        /// <summary>
        /// Filepath to the log file
        /// </summary>
        private static string m_logsFile = Environment.getLogFilepath();
        /// <summary>
        /// Filepath to the log file backup
        /// </summary>
        private static string m_logsFileBackup = Environment.getLogBackupFilepath();
        /// <summary>
        /// Filepath to the temp file containing m_todaysLogs
        /// </summary>
        private static string m_todaysLogsFile = Environment.getSessionDataFilepath();
        /// <summary>
        /// Filepath to the backup temp file containing m_todaysLogs
        /// </summary>
        private static string m_todaysLogsFileBackup = Environment.getSessionDataBackupFilepath();

        /// <summary>
        /// Human readable line seperator used to seperate the entries for each week
        /// </summary>
        private static string m_weekSeperator =
            m_weekToken + m_weekToken +
            m_weekToken + m_weekToken +
            m_weekToken + m_weekToken +
            m_weekToken + m_weekToken +
            m_weekToken + m_weekToken +
            m_weekToken + m_weekToken +
            m_weekToken + m_weekToken;

        /// <summary>
        /// Loads working logs from the local log file.
        /// </summary>
        public static void loadTodayLogsFromFile()
        {
            // create empty strings to add data to
            string todaysLogs = "";
            string backupLogs = "";

            // set the current date
            m_date = DateTime.Now;
            string date = m_date.ToString(m_dateFormat);

            // set the current day of the week
            m_today = m_date.DayOfWeek;

            // check what files exists
            bool logFileExists = File.Exists(m_logsFile);
            bool tempFileExists = File.Exists(m_todaysLogsFile);

            /////////////////////////
            //  Read the Temp File //
            /////////////////////////

            // if a temp backup file exists copy the data from it
            if (tempFileExists == true)
            {
                foreach (string line in File.ReadAllLines(m_todaysLogsFile))
                    backupLogs += m_lineToken + line;
            }

            /////////////////////////
            //  Read the Log File  //
            /////////////////////////
            
            // if a log file exists copy today logs from it
            if (logFileExists == true)
            {
                // is set to true if any logs with todays date are found
                bool latestEntriesAreTodays = false;
                // is set to true when the log file header has been read
                bool hasHeaderBeenRead = false;
                
                // go through all lines in the log file and copy the relevant logs to memory
                foreach (string line in File.ReadLines(m_logsFile))
                {
                    // skip comment lines
                    if (line.StartsWith(m_commentToken) == true ||
                        line.Length == 0)
                        continue;

                    if (line.StartsWith(m_versionToken) == true)
                        checkVersion(line.TrimStart(m_versionToken.ToCharArray()));

                    // check if the header has been read
                    if (hasHeaderBeenRead == false)
                    {
                        // TODO: check version

                        // if the end of header is found
                        if (line.StartsWith(m_weekToken))
                        {
                            // mark that the header has been passed
                            hasHeaderBeenRead = true;

                            // extract the date range from the week seperator
                            string dateRange = line.Trim(m_weekSeperator.ToCharArray());

                            // if the date range ends with todays date there are entries for today
                            if (dateRange.EndsWith(date))
                                latestEntriesAreTodays = true;
                            else
                                return;

                        } // END the end of the header
                    } // End header
                    else /* Header has been read */
                    {
                        // once the header has been read check if any logs for today were found
                        if (latestEntriesAreTodays == true)
                        {
                            // skip commented lines
                            if (line.StartsWith(m_commentToken))
                                continue;

                            // copy the log entries that have todays date to memory
                            // NOTE: This assumes all entries are in reverse chronological order
                            if (line.StartsWith(date))
                                todaysLogs += m_lineToken + line;
                            else
                                break;
                        }
                    }
                } // END foreach (string line in File.ReadLines(m_logsFile))
                
                /////////////////////////////////
                //  Override Out-of-Date Data  //
                /////////////////////////////////

                // prepare log entries and temp log entries to be compared
                string[] entries = todaysLogs.Split(m_lineToken.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] backupLogEntries = backupLogs.Split(m_lineToken.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // do nothing if there are no log entries or backup entries
                if (entries.Length == 0 && backupLogEntries.Length == 0)
                    return;

                // the number of entries in the temp file that didn't exist in the log file
                int backupLogEntriesMissingCount = 0;
                // array to store the index of temp file entries that didn't exist in the log file
                int[] backupLogEnrtiesMissingIndex = new int[backupLogEntries.Length];

                // check for matching entries in backuplogs and entries
                if (backupLogEntries.Length != 0)
                {
                    for (int i = 0; i < backupLogEntries.Length; i++)
                    {
                        if (!backupLogEntries[i].Contains(m_dataSeperator))
                            continue;

                        string[] backupLogData = backupLogEntries[i].Split(m_dataSeperator.ToCharArray());

                        // check if the backup entry exists in the log file
                        int matchIndex = entryExists(
                            entries,
                            backupLogData[(int)EntryDataIndex.LogName],
                            backupLogData[(int)EntryDataIndex.ProjectName],
                            backupLogData[(int)EntryDataIndex.ProjectDescription]);
                        
                        // if the entry exists overwrite the previous entry
                        if (matchIndex != -1)
                        {
                            // if there is a match: set the missing entry index to an invalid index
                            backupLogEnrtiesMissingIndex[i] = -1;
                            // override the entry with the new entry
                            entries[matchIndex] = backupLogEntries[i];
                        }
                        else
                        {
                            // set the missing entry index to the index that was read
                            backupLogEnrtiesMissingIndex[i] = i;
                            // increase the missing entry count
                            backupLogEntriesMissingCount++;
                        }
                    }
                } // END check for matching entries in backuplogs and entries
                
                /////////////////////////
                // Copy Data to Memory //
                /////////////////////////

                // store the entries in memory
                m_todaysLogs = new string[entries.Length + backupLogEntriesMissingCount];
                
                // track position within m_todaysLogs
                int entriesCopied = 0;

                // copy any backup entries that did not exists in the log file
                for (int i = 0; i < backupLogEnrtiesMissingIndex.Length; i++)
                {
                    // check if the missing index is valid
                    if (backupLogEnrtiesMissingIndex[i] >= 0 &&
                        backupLogEnrtiesMissingIndex[i] < backupLogEntries.Length)
                    {
                        // store the temp log entry at the index in memory
                        m_todaysLogs[entriesCopied] = backupLogEntries[backupLogEnrtiesMissingIndex[i]];
                        // increase total entries copied counter
                        entriesCopied++;
                    }
                }

                // copy the entries that did exist in the log file
                for (int i = 0; i < entries.Length; i++)
                    m_todaysLogs[i + entriesCopied] = entries[i];
            }
        }

        /// <summary>
        /// Returns the time string (hh:mm:ss) of the entry that matches the input sheet, tab and ddescription.
        /// <para> </para>
        /// Returns 00:00:00 if a matching entry does not exist.
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="projectName"></param>
        /// <param name="dataIndex"></param>
        /// <returns></returns>
        public static string getTodaysEntryData(string logName, string projectName, string projectDescription, EntryDataIndex dataIndex)
        {
            // set the result string to a default return value
            string result = "00:00:00";

            for (int i = 0; i < m_todaysLogs.Length; i++)
            {
                if (!m_todaysLogs[i].Contains(m_dataSeperator))
                    continue;

                // split the entry into its components
                string[] logEntryData = m_todaysLogs[i].Split(m_dataSeperator.ToCharArray());
                
                // check if the entry exists
                if (logEntryData[(int)EntryDataIndex.ProjectDescription] == projectDescription &&
                    logEntryData[(int)EntryDataIndex.ProjectName] == projectName &&
                    logEntryData[(int)EntryDataIndex.LogName] == logName)
                {
                    // set the result to the strig of the data index called for
                    result = logEntryData[(int)dataIndex];
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the index of of the entry with matching log name, project name and description.
        /// <para></para>
        /// Returns -1 if th entry does not exist.
        /// </summary>
        /// <param name="logName">The name of the log to search for</param>
        /// <param name="projectName">The name of the project to search for</param>
        /// <param name="projectDescription">The description to search for</param>
        /// <returns></returns>
        public static int entryExists(string[] entries, string logName, string projectName, string projectDescription)
        {
            // find and update existing matching entry
            for (int i = 0; i < entries.Length; i++)
            {
                // split the entry into its components
                string[] logEntryData = entries[i].Split(m_dataSeperator.ToCharArray());

                // check if the entry exists
                if (logEntryData[(int)EntryDataIndex.ProjectDescription] == projectDescription &&
                    logEntryData[(int)EntryDataIndex.ProjectName] == projectName &&
                    logEntryData[(int)EntryDataIndex.LogName] == logName)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Returns the index of of the entry with matching log name, project name and description.
        /// <para></para>
        /// Returns -1 if th entry does not exist.
        /// </summary>
        /// <param name="logName">The name of the log to search for</param>
        /// <param name="projectName">The name of the project to search for</param>
        /// <param name="description">The description to search for</param>
        /// <returns></returns>
        public static int entryExists(string logName, string projectName, string projectDescription)
        {
            // find and update existing matching entry
            for (int i = 0; i < m_todaysLogs.Length; i++)
            {
                // split the entry into its components
                string[] logEntryData = m_todaysLogs[i].Split(m_dataSeperator.ToCharArray());

                // check if the entry exists
                if (logEntryData[(int)EntryDataIndex.ProjectDescription] == projectDescription &&
                    logEntryData[(int)EntryDataIndex.ProjectName] == projectName &&
                    logEntryData[(int)EntryDataIndex.LogName] == logName)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Adds the time, log name, project name and description to the log book. If the log+project+descrioption already exists the previous entry is replaced.
        /// </summary>
        /// <param name="timeStr">The string representing the time that will be logged. Must be format hh:mm:ss</param>
        /// <param name="logName">The name of the log the entry is related to</param>
        /// <param name="projectName">The name of the project the entry is related to</param>
        /// <param name="description">The description related to the entry</param>
        /// <returns></returns>
        public static bool updateTodaysLogs(string timeStr, string logName, string projectName, string projectDescription)
        {
            string date = m_date.ToString(m_dateFormat);
            string day = m_date.ToString(m_dayFormat);
            
            // create entry string for the input data
            string entry =
                date +
                m_dataSeperator + day +
                m_dataSeperator + timeStr +
                m_dataSeperator + logName +
                m_dataSeperator + projectName +
                m_dataSeperator + projectDescription;

            int entryIndex = entryExists(m_todaysLogs, logName, projectName, projectDescription);
            
            // add entry to the top of the list if it didn't already exist
            if (entryIndex == -1)
            {
                // get a new handle on all of the strings in m_todaysLogs
                string[] sa = (string[])m_todaysLogs.Clone();

                // repoint m_todaysLogs to an array of strings containing one more position
                m_todaysLogs = new string[m_todaysLogs.Length + 1];

                // copy the data to the new array (in reverse order just incase)
                for (int i = m_todaysLogs.Length - 1; i > 0; i--)
                    m_todaysLogs[i] = sa[i - 1];

                // add the entry to the first position in the new array
                m_todaysLogs[0] = entry;
            }
            else
                m_todaysLogs[entryIndex] = entry;
            
            // save to temp file
            if (File.Exists(m_todaysLogsFileBackup))
                File.Delete(m_todaysLogsFileBackup);

            if(File.Exists(m_todaysLogsFile))
                File.Copy(m_todaysLogsFile, m_todaysLogsFileBackup);

            File.WriteAllLines(m_todaysLogsFile, m_todaysLogs);
            File.Delete(m_todaysLogsFileBackup);

            if (hasDayChanged() == true)
            {
                writeToLogFile();
                m_todaysLogs = new string[0];
                m_date = DateTime.Now;
            }

            return true;
        }

        /// <summary>
        /// Creates a new log file replacing the previous working logs with the current working logs
        /// </summary>
        /// <returns></returns>
        public static bool writeToLogFile()
        {
            string date = m_date.ToString(m_dateFormat);
            string day = m_date.ToString(m_dayFormat);

            string header = "";
            string body = "";

            // add current version to header
            header += m_versionToken + Environment.getVersion();

            ////////////////////////////
            //////   Read File   ///////
            ////////////////////////////
            
            // if file exists get data from it
            // else create new data
            bool fileFound = File.Exists(m_logsFile);
            if (fileFound == true)
            {
                // set to true when the first week seperator is found
                bool isHeaderFound = false;
                // set to true when the second week seperator is found
                bool isHeaderRead = false;
                
                // copy all but todays logs
                foreach (string line in File.ReadLines(m_logsFile))
                {
                    // skip comments and empty lines
                    if (line.StartsWith(m_commentToken) == true ||
                        line.Length == 0)
                        continue;

                    if (line.StartsWith(m_versionToken) == true)
                        checkVersion(line.TrimStart(m_versionToken.ToCharArray()));

                    // if the header has not been read search for the header in the file
                    // else copy the body data from the file
                    if (isHeaderRead == false)
                    {
                        // if line starts with a week seperator line and header has not been found
                        //    copy the data into the header
                        // else if line starts with a week seperator line and header has been found
                        //    copy the seperator to the body
                        if (line.StartsWith(m_weekToken) && isHeaderFound == false)
                        {
                            isHeaderFound = true;
                            string dateRange = line.Trim(m_weekToken.ToCharArray());

                            // if the week seperator is todays copy it to the header
                            // else create a new seperator line
                            if (dateRange.EndsWith(date))
                            {
                                // copy the existing seperator line to the header
                                // line is the week seperator here
                                header += m_lineToken + line;

                                // copy todays logs to the header
                                foreach (string s in m_todaysLogs)
                                    header += m_lineToken + s;
                            }
                            // if the week seperator is NOT todays so copy it to the body
                            // and create new week seperator line
                            else
                            {
                                // place current line in the body
                                body += m_lineToken + line;

                                // create the new seperator line
                                DateTime beginWeek = m_date;
                                beginWeek = beginWeek.Subtract(new TimeSpan((int)m_date.DayOfWeek, 0, 0, 0));
                                string sunday = beginWeek.ToString(m_dateFormat);
                                header = m_lineToken + m_weekSeperator + sunday + "-" + date + m_weekSeperator;

                                // copy todays logs to the header
                                foreach (string s in m_todaysLogs)
                                    header += m_lineToken + s;
                                
                                // set to copy the rest of the file to the body
                                isHeaderRead = true;
                            }
                        } // END read header

                        // check if you hit the next week seperator
                        else if (line.StartsWith(m_weekToken) && isHeaderFound == true)
                        {
                            // mark as done reading header and add the seperator line to the body
                            body += m_lineToken + line;
                            isHeaderRead = true;
                        }

                    } // End header
                    // directly copy the rest of the data from the file
                    else /* Header has been read */
                    {
                        body += m_lineToken + line;
                    }
                } // END foreach

                // create an entry if none are found
                if (isHeaderFound == false)
                {
                    // create the new seperator line
                    DateTime beginWeek = m_date;
                    beginWeek = beginWeek.Subtract(new TimeSpan((int)m_date.DayOfWeek, 0, 0, 0));
                    string sunday = beginWeek.ToString(m_dateFormat);
                    header = m_lineToken + m_weekSeperator + sunday + "-" + date + m_weekSeperator;

                    // copy todays logs to the header
                    foreach (string s in m_todaysLogs)
                        header += m_lineToken + s;
                    //DialogMessage message = new DialogMessage("Error reading file", "First entry missing");
                    //message.ShowDialog();
                    //return false;
                }

            } // END File.Exists() == true
            else
            {
                // add the application version to the header
                header = Environment.getVersion();
                // add a week spereator to the header

                DateTime beginWeek = m_date;
                beginWeek = beginWeek.Subtract(new TimeSpan((int)m_date.DayOfWeek, 0, 0, 0));
                string sunday = beginWeek.ToString(m_dateFormat);
                header += m_lineToken + m_weekSeperator + sunday + "-" + date + m_weekSeperator;
                // add todays logs to the body
                foreach(string s in m_todaysLogs)
                    body += m_lineToken + s;
            }
            
            ////////////////////////////
            ////   Write to File   /////
            ////////////////////////////

            if (File.Exists(m_logsFileBackup))
                File.Delete(m_logsFileBackup);

            if(File.Exists(m_logsFile))
                File.Copy(m_logsFile, m_logsFileBackup);

            File.WriteAllText(m_logsFile, header + body);

            // delete temp file for storing todays logs
            if (File.Exists(m_todaysLogsFile))
                File.Delete(m_todaysLogsFile);

            return true;
        }

        /// <summary>
        /// Adds together two times in format "hh:mm:ss"
        /// </summary>
        /// <param name="time1"></param>
        /// <param name="time2"></param>
        /// <returns></returns>
        public static string addTimeStrings(string time1, string time2)
        {
            string[] time1Split = time1.Split(':');
            string[] time2Split = time2.Split(':');

            // check the string was valid
            if (time1Split.Length != 3 || time2Split.Length != 3)
                throw new ArgumentException("String Format", "Time string did not split correctly");

            // check the string was valid
            for (int i = 0; i < 3; i++)
            {
                //if (time1Split[i].Length != 2 || time2Split[i].Length != 2)
                //    throw new ArgumentException("Split Length", "Time string split is not the corrent length");

                for (int j = 0; j < 2; j++)
                {
                    if (!char.IsDigit(time1Split[i][j]))
                        throw new ArgumentException("time1Split", "split time values must only contain numbers");
                    if (!char.IsDigit(time2Split[i][j]))
                        throw new ArgumentException("time2Split", "split time values must only contain numbers");
                }
            }
        
            // parse the strings
            int hours1 = int.MinValue;
            int hours2 = int.MinValue;
            int minutes1 = int.MinValue;
            int minutes2 = int.MinValue;
            int seconds1 = int.MinValue;
            int seconds2 = int.MinValue;

            int.TryParse(time1Split[0], out hours1);
            int.TryParse(time2Split[0], out hours2);
            int.TryParse(time1Split[1], out minutes1);
            int.TryParse(time2Split[1], out minutes2);
            int.TryParse(time1Split[2], out seconds1);
            int.TryParse(time2Split[2], out seconds2);

            if(hours1 == int.MinValue)
                throw new ArgumentException("hours1", "Invalid input");
            if (hours2 == int.MinValue)
                throw new ArgumentException("hours2", "Invalid input");
            if (minutes1 == int.MinValue)
                throw new ArgumentException("minutes1", "Invalid input");
            if (minutes2 == int.MinValue)
                throw new ArgumentException("minutes2", "Invalid input");
            if (seconds1 == int.MinValue)
                throw new ArgumentException("seconds1", "Invalid input");
            if (seconds2 == int.MinValue)
                throw new ArgumentException("seconds2", "Invalid input");

            // add the values together
            int hourTotal = hours1 + hours2;
            int minuteTotal = minutes1 + minutes2;
            int secondTotal = seconds1 + seconds2;

            // TODO: account for inaccurate float values
            // carry over seconds/minutes greater than 60
            if (secondTotal >= 60)
            {
                minuteTotal += secondTotal / 60;
                secondTotal %= 60;
            }
            if (minuteTotal >= 60)
            {
                hourTotal += minuteTotal / 60;
                minuteTotal %= 60;
            }

            // convert to string
            string hourStr = hourTotal.ToString();
            string minuteStr = minuteTotal.ToString();
            string secondStr = secondTotal.ToString();

            // check if leading zeros need to be added
            if (hourStr.Length == 1)
                hourStr = "0" + hourStr;
            if (minuteStr.Length == 1)
                minuteStr = "0" + minuteStr;
            if (secondStr.Length == 1)
                secondStr = "0" + secondStr;

            return hourStr + ":" + minuteStr + ":" + secondStr;
        }

        private static bool hasDayChanged()
        {
            if (m_today == DateTime.Now.DayOfWeek)
                return false;

            m_today = DateTime.Now.DayOfWeek;
            return true;
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

            Console.WriteLine("LocalLog file version does not match the application version.");
            // handle version differences
        }

        /// <summary>
        /// Debugging function used in unit testing
        /// <para></para>
        /// Run function with no parameters to set to default values
        /// </summary>
        /// <param name="logPath"></param>
        /// <param name="logBackupPath"></param>
        /// <param name="sessionPath"></param>
        /// <param name="sessionBackupPath"></param>
        public static void DEBUGsetFilePaths(string logPath, string logBackupPath, string sessionPath, string sessionBackupPath)
        {
            m_todaysLogsFile = sessionPath;
            m_todaysLogsFileBackup = sessionBackupPath;
            m_logsFile = logPath;
            m_logsFileBackup = logBackupPath;
        }
        public static void DEBUGsetFilePaths()
        {
            m_logsFile = Environment.getLogFilepath();
            m_logsFileBackup = Environment.getLogBackupFilepath();
            m_todaysLogsFile = Environment.getSessionDataFilepath();
            m_todaysLogsFileBackup = Environment.getSessionDataBackupFilepath();
        }
    } // END class LocalLogger
} // */
