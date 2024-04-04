# Acknowledgement Shipping Notification file monitoring service

Service built on .NET Core 8 to monitor a folder for new files and parse those files.

Configure the folder to monitor in appsettings.json by editing folderPath variable

```json 
{
    "BoxAppSettings": {
        "FolderPath": "C:\\Path"
    }
}
```

Run the application by opening the solution in an IDE or via command line using the following command from the Boxer project folder

`dotnet run`

### Testing

Copy the data.txt from sample folder to the folder selected in the appSettings.

### App Description

The app is built as a simple web service that has a background service running that is monitoring the folder for new files. File monitoring is performed by System.IO.FileSystemWatcher class.
In current implementation it only monitors for new files being added as the app is working, but ideally you want to handle situations when the service has been down and new files have been uploaded,
to ensure all files are correctly processed.

Once the file creation has been detected the app verifies that is not in use anymore (if it is, it retries every second), in case of larger files taking longer time to transfer and begins processing them line-by-line, to avoid larger parts of the file being allocated in the memory.
Based on the box and line item identifiers each line is processed with Regex to retrieve the properties. Once it reaches a new box item, the previous one gets stored in in-memory database
using Entity Framework Core ORM, repeating the cycle for the whole file.

### ToDo for a production ready application

* More comprehensive validations for parsed data, i.e. specific id formatting. Would try to that using FluentValidations
* Based on requirements there could be a need for duplicate handling of the box level items
* Create Dockerfile to deploy application to a remote environment
* Testing the application with various types of input data that might be possible
* Specify DB constraints. Depending on how the data is used, could be a case to store in NoSQL DB