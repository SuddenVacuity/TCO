

/// <summary>
/// Contains data about the program environment
/// </summary>
public static  class Environment
{
    private static readonly string APPLICATION_VERSION = "0.0.1.0";
    ///////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////
    ///////                                             ///////
    ///////                 DIRECTORIES                 ///////
    ///////                                             ///////
    ///////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// The drectory the .exe file is in.
    /// </summary>
    private static readonly string DIRECTORY_APPLICATION = System.Environment.CurrentDirectory;

    ///////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////
    ///////                                             ///////
    ///////              INTERNAL FOLDERS               ///////
    ///////                                             ///////
    ///////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// The folder google stores credentials in.
    /// <para></para>
    /// Includes the leading /
    /// </summary>
    private static readonly string FOLDER_GOOGLE_CREDENTIALS = "//.credentials";
    //private static readonly 
    //private static readonly 
    //private static readonly 

    ///////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////
    ///////                                             ///////
    ///////                 FILE NAMES                  ///////
    ///////                                             ///////
    ///////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// The name of the user preferences file.
    /// <para></para>
    /// Includes the leading /
    /// </summary>
    private static readonly string FILENAME_USER_PREFERENCES = "//userPrefs.bin";
    /// <summary>
    /// The name of the local log file.
    /// <para></para>
    /// Includes the leading /
    /// </summary>
    private static readonly string FILENAME_LOCAL_LOG = "//LocalLog.txt";
    /// <summary>
    /// The name of the backup of the local log file.
    /// <para></para>
    /// Includes the leading /
    /// </summary>
    private static readonly string FILENAME_LOCAL_LOG_BACKUP = "//~LocalLog.txt";
    /// <summary>
    /// The name of the file the program stores todays log data to reload incase of an unexpected closing
    /// <para></para>
    /// Includes the leading /
    /// </summary>
    private static readonly string FILENAME_TODAYS_LOGS = "//tl.bin";
    /// <summary>
    /// The name of the file the program stores todays log data to reload incase of an unexpected closing
    /// <para></para>
    /// Includes the leading /
    /// </summary>
    private static readonly string FILENAME_TODAYS_LOGS_BACKUP = "//~tl.bin";
    /// <summary>
    /// The name of the client secret file needed to interact with google.
    /// <para></para>
    /// Includes the leading /
    /// </summary>
    /// </summary>
    private static readonly string FILENAME_GOOGLE_CLIENT_SECRET = "//client_secret.json";
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 
    //private static readonly 

    ///////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////
    ///////                                             ///////
    ///////                 FUNCTIONS                   ///////
    ///////                                             ///////
    ///////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////

    /// <summary>
    /// Returns the filepath to the locatino where the local log file is stored
    /// </summary>
    /// <returns></returns>
    public static string getLogFilepath()
    {
        return DIRECTORY_APPLICATION + FILENAME_LOCAL_LOG;
    }
    /// <summary>
    /// Returns the filepath to the location where the backup of the local log file is stored
    /// </summary>
    /// <returns></returns>
    public static string getLogBackupFilepath()
    {
        return DIRECTORY_APPLICATION + FILENAME_LOCAL_LOG_BACKUP;
    }
    /// <summary>
    /// Returns the filaepath to the location where the user preferences file is stored
    /// </summary>
    /// <returns></returns>
    public static string getUserPrefsFilepath()
    {
        return DIRECTORY_APPLICATION + FILENAME_USER_PREFERENCES;
    }
    /// <summary>
    /// Returns the version of this application
    /// </summary>
    /// <returns></returns>
    public static string getVersion()
    {
        return APPLICATION_VERSION;
    }
    /// <summary>
    /// Returns the filepath to the location where the temporary file storing this session's data is stored
    /// </summary>
    /// <returns></returns>
    public static string getSessionDataFilepath()
    {
        return DIRECTORY_APPLICATION + FILENAME_TODAYS_LOGS;
    }
    /// <summary>
    /// Returns the filepath to the location where the backup of the temporary file storing this session's data is stored.
    /// </summary>
    /// <returns></returns>
    public static string getSessionDataBackupFilepath()
    {
        return DIRECTORY_APPLICATION + FILENAME_TODAYS_LOGS_BACKUP;
    }
}
