using MySql.Data.MySqlClient;
using System.Data;

// ############################################################
// Wrapper-Klasse für Datenbank-Operationen
//
// ACHTUNG: Exceptions!
// Konstruktor und fast alle Methoden können Exceptions werfen!
// Der Aufrufende muss sich darum kümmern!
//
// ############################################################

// SINGLETON Wrapper
public class DbWrapperMySqlV2
{
  // MERKMALE
  private static DbWrapperMySqlV2? Instance = null;  // Das Singleton-Objekt
  private MySqlConnection connection = new("");
  private string connString = "";

  // PROPERTIES
  public static DbWrapperMySqlV2 Wrapper
  {
    get
    {
      if (Instance == null)               // Wenn es das einzige Objekt noch nicht gibt
        Instance = new DbWrapperMySqlV2();  // -> erzeugen
      return Instance;
    }
  }

  public bool IsOpen
  {
    get { return (connection.State == ConnectionState.Open); }
  }

  // KONSTRUKTOR
  private DbWrapperMySqlV2()
  { // Die geheimen Credentials für den DB-Zugriff - am Desktop "versteckt"
    string[] db_args = File.ReadAllLines(
      Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) +
      "/db_args.txt");
    string comment = db_args[0];
    string server = db_args[1];
    string db = db_args[2];
    string userId = db_args[3];
    string password = db_args[4];
    connString = $"Server={server};Database={db};" +
                             $"User ID={userId};Password={password};";

    connection = new MySqlConnection(connString);  // eventuell Exception!.
  }

  // ##############################################################
  // METHODEN Allgemein
  // Hier werden keine Exception abgefangen.
  // Alle Exceptions müssen durch den Aufrufenden behandelt werden!
  // ##############################################################

  public void Open()
  {
    connection.Open();
  }

  public void Close()
  {
    connection.Close();
  }

  /// <summary>
  /// Führt den SQL-Befehl mit der aktuellen connection aus.
  /// </summary>
  /// <returns>Einen DataTable, gefüllt oder auch leer.</returns>
  public DataTable RunQuery(string sqlString)
  {
    MySqlDataAdapter adapter = new MySqlDataAdapter(sqlString, connection);
    DataTable table = new DataTable();
    // Open() nicht nötig
    adapter.Fill(table);   // Connection open/close automatically managed by DataAdapter 
                           // Close() nicht nötig
    return table;
  }

  /// <summary>
  /// Für Insert, Update und Delete Befehle
  /// auch: CREATE, DROP, GRANT
  /// Bezüglich offener / geschlossener Connections gleiches Verhalten wie DataAdapter.
  /// Verwendet die ExecuteNonQuery Methode des Command-Objekts.
  /// </summary>
  /// <returns>Liefert Anzahl der betroffenen Datensätze, ev. auch keine.</returns>
  public int RunNonQuery(string sqlString)
  {
    int numRecords = 0;
    MySqlCommand command = new(sqlString, connection);
    try
    {
      if (IsOpen == false) Open();             // Extra Open() nur wenn nötig
      numRecords = command.ExecuteNonQuery();  // könnte Exception auslösen
      Close();                                 // würde dann nicht ausgeführt
    }
    catch (Exception)
    {
      Close();                 // Nur Verbindung schließen
      throw;                   // Rufer soll aber die Exception erhalten
    }
    return numRecords;
  }

  /// <summary>
  /// Bezüglich offener / geschlossener Connections gleiches Verhalten wie DataAdapter.
  /// Verwendet die ExecuteScalar-Methode des Command-Objekts.
  /// </summary>
  /// <returns>Liefert genau 1 Wert, evtl. sogar DBNull.Value. Deshalb Datentyp object.</returns>
  public object? RunQueryScalar(string sqlString)
  {
    object? obj = null;
    MySqlCommand command = new(sqlString, connection);

    try
    {
      if (IsOpen == false) Open();    // Extra Open() nur wenn nötig
      obj = command.ExecuteScalar();  // könnte Exception auslösen
      Close();                        // würde dann nicht ausgeführt
    }
    catch (Exception)
    {
      Close();                 // Nur Verbindung schließen
      throw;                   // Rufer soll aber die Exception erhalten
    }

    return obj;
  }
}



