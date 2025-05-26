// See https://aka.ms/new-console-template for more information

using flight_log;
using TesseractOCR;
using TesseractOCR.Enums;


// Coordinates for the region to scan with OCR
var region = Rect.FromCoords(45, 2900, 61, 3840);

Console.WriteLine("UEE Pathfinders - Flight Log 1.0");

// scan local director for files with .png extension and add them to a list.
var pngFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.png").ToList();

// if no files found, print a message and exit
if (pngFiles.Count == 0)
{
    Console.WriteLine("No PNG files found in the current directory.");
    return;
}

Console.WriteLine($"Found {pngFiles.Count} PNG files in the current directory.");

// sort the list of files by creation time
pngFiles.Sort((x, y) => File.GetCreationTime(x).CompareTo(File.GetCreationTime(y)));

var logEntries = new List<FlightLogEntry>();
foreach (var pngFile in pngFiles)
{
    if(!File.Exists(pngFile))
    {
        Console.WriteLine($"File {pngFile} does not exist.");
        continue;
    }
    
    Console.WriteLine($"Processing file: {pngFile}");
    
    // Load the image from the file
    TesseractOCR.Pix.Image.LoadFromFile(pngFile);
}

var img = TesseractOCR.Pix.Image.LoadFromFile(pngFiles[0]);
var engine = new TesseractOCR.Engine("./tessdata", "eng", EngineMode.Default);
engine.Process(img, Rect.FromCoords(1, 1, 2, 2));

// create a new file called "flight-log.txt" in the current directory
var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "flight-log.txt");
