using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace GoogleDrive
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string pathToCred = "credentials.json";
            UserCredential credential;
            using (var stream = new FileStream(pathToCred, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { DriveService.Scope.Drive, SheetsService.Scope.Spreadsheets },
                    "user",
                    CancellationToken.None);
            }

            var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveSync",
            });

            var sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDriveSync",
            });

            while (true)
            {
                var fileList = await GetFileList(driveService);
                await UpdateGoogleSheet(sheetsService, fileList);
                Console.WriteLine("Sync completed. Waiting for 15 minutes...");
                await Task.Delay(TimeSpan.FromMinutes(15));
            }
        }

        private static async Task<IList<Google.Apis.Drive.v3.Data.File>> GetFileList(DriveService driveService)
        {
            var request = driveService.Files.List();
            request.PageSize = 1000;
            request.Fields = "nextPageToken, files(id, name, mimeType, createdTime)";

            var result = await request.ExecuteAsync();
            return result.Files;
        }

        private static async Task UpdateGoogleSheet(SheetsService sheetsService, IList<Google.Apis.Drive.v3.Data.File> fileList)
        {
            string spreadsheetId = "sheet_id"; // Replace with your Google Spreadsheet ID
            string sheetName = "Sheet1"; // Replace with your sheet name if different

            var requests = new List<Request>();

            // Clear the existing data
            requests.Add(new Request
            {
                UpdateCells = new UpdateCellsRequest
                {
                    Range = new GridRange
                    {
                        SheetId = 0,
                        StartRowIndex = 0,
                        EndRowIndex = 1000,
                        StartColumnIndex = 0,
                        EndColumnIndex = 4
                    },
                    Fields = "*"
                }
            });

            // Add header row
            requests.Add(new Request
            {
                UpdateCells = new UpdateCellsRequest
                {
                    Range = new GridRange
                    {
                        SheetId = 0,
                        StartRowIndex = 0,
                        EndRowIndex = 1,
                        StartColumnIndex = 0,
                        EndColumnIndex = 4
                    },
                    Rows = new List<RowData>
                    {
                        new RowData
                        {
                            Values = new List<CellData>
                            {
                                new CellData { UserEnteredValue = new ExtendedValue { StringValue = "ID" } },
                                new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Name" } },
                                new CellData { UserEnteredValue = new ExtendedValue { StringValue = "MIME Type" } },
                                new CellData { UserEnteredValue = new ExtendedValue { StringValue = "Created Time" } }
                            }
                        }
                    },
                    Fields = "*"
                }
            });

            // Add file data
            for (int i = 0; i < fileList.Count; i++)
            {
                var file = fileList[i];
                requests.Add(new Request
                {
                    UpdateCells = new UpdateCellsRequest
                    {
                        Range = new GridRange
                        {
                            SheetId = 0,
                            StartRowIndex = i + 1,
                            EndRowIndex = i + 2,
                            StartColumnIndex = 0,
                            EndColumnIndex = 4
                        },
                        Rows = new List<RowData>
                        {
                            new RowData
                            {
                                Values = new List<CellData>
                                {
                                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = file.Id } },
                                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = file.Name } },
                                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = file.MimeType } },
                                    new CellData { UserEnteredValue = new ExtendedValue { StringValue = file.CreatedTime.ToString() } }
                                }
                            }
                        },
                        Fields = "*"
                    }
                });
            }

            var batchUpdateRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
            await sheetsService.Spreadsheets.BatchUpdate(batchUpdateRequest, spreadsheetId).ExecuteAsync();
        }
    }
}
