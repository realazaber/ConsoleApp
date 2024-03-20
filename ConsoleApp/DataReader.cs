namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class DataReader
    {
        IEnumerable<ImportedObject> ImportedObjects;

        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            List<ImportedObject> ImportedObjects = new List<ImportedObject>();

            var streamReader = new StreamReader(fileToImport);

            var importedLines = new List<string>();
            int lineNumber = 0;
            while (!streamReader.EndOfStream)
            {
                lineNumber++;
                if (lineNumber == 1) continue; // Skip the first line (header)
                var line = streamReader.ReadLine();
                importedLines.Add(line);
            }


            for (int i = 1; i < importedLines.Count; i++)
            {
                var importedLine = importedLines[i];
                string[] values = importedLine.Split(';');

                var importedObject = new ImportedObject();
                if (values.Length == 7)
                {
                    importedObject.Type = values[0];
                    importedObject.ObjName = values[1];
                    importedObject.Schema = values[2];
                    importedObject.ParentName = values[3];
                    importedObject.ParentType = values[4];
                    importedObject.DataType = values[5];

                    if (string.IsNullOrEmpty(values[6]))
                    {
                        importedObject.IsNullable = "NULL"; // Set a default value for empty IsNullable
                    }
                    else
                    {
                        importedObject.IsNullable = values[6];
                    }
                    Console.WriteLine("Adding " + values[1]);
                    ImportedObjects.Add(importedObject);
                }

            }

            // clear and correct imported data
            foreach (ImportedObject importedObject in ImportedObjects)
            {
                //  Console.WriteLine(importedObject.Type);

                if (importedObject != null)
                {
                    Console.WriteLine(importedObject.Type);
                    importedObject.Type = importedObject.Type.Trim().Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
                    importedObject.ObjName = importedObject.ObjName.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                    importedObject.Schema = importedObject.Schema.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                    importedObject.ParentName = importedObject.ParentName.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                    importedObject.ParentType = importedObject.ParentType.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
                }
            }

            // assign number of children
            for (int i = 0; i < ImportedObjects.Count(); i++)
            {
                var importedObject = ImportedObjects.ToArray()[i];
                foreach (var impObj in ImportedObjects)
                {
                    if (impObj.ParentType == importedObject.Type)
                    {
                        if (impObj.ParentName == importedObject.ObjName)
                        {
                            importedObject.NumberOfChildren = 1 + importedObject.NumberOfChildren;
                        }
                    }
                }
            }

            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.ObjName}' ({database.NumberOfChildren} tables)");

                    // print all database's tables
                    foreach (var table in ImportedObjects)
                    {
                        if (table.ParentType.ToUpper() == database.Type)
                        {
                            if (table.ParentName == database.ObjName)
                            {
                                Console.WriteLine($"\tTable '{table.Schema}.{table.ObjName}' ({table.NumberOfChildren} columns)");

                                // print all table's columns
                                foreach (var column in ImportedObjects)
                                {
                                    if (column.ParentType.ToUpper() == table.Type)
                                    {
                                        if (column.ParentName == table.ObjName)
                                        {
                                            Console.WriteLine($"\t\tColumn '{column.ObjName}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.ReadLine();
        }
    }

    class ImportedObject : ImportedObjectBaseClass
    {
        public string ObjName
        {
            get;
            set;
        }
        public string Schema;

        public string ParentName;
        public string ParentType
        {
            get; set;
        }

        public string DataType { get; set; }
        public string IsNullable { get; set; }

        public double NumberOfChildren;
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
