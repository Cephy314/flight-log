// See https://aka.ms/new-console-template for more information

using flight_log;
using TesseractOCR;
using TesseractOCR.Enums;


// Coordinates for the region to scan with OCR
var region = Rect.FromCoords(3200, 45, 3835, 63);

Console.WriteLine("UEE Pathfinders - Flight Log 1.0");

var currentDirectory = Directory.GetCurrentDirectory();

// check to make sure there is a mask.png file in the current directory
var maskFilePath = Path.Combine(currentDirectory, "mask.png");
if (!File.Exists(maskFilePath))
{
    Console.WriteLine("mask.png file not found in the current directory. You must have a mask.png to cover the r_displayinfo 1 data in the screenshots.");
    return;
}

Console.WriteLine("mask.png found...");

// scan local director for files with .png extension and add them to a list.
var pngFiles = Directory.GetFiles(currentDirectory, "*.png")
    .Where(f => !f.Contains("mask.png" ))
    .ToList();

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
var engine = new TesseractOCR.Engine("./", "eng", EngineMode.Default );
foreach (var pngFile in pngFiles)
{
    if(!File.Exists(pngFile))
    {
        Console.WriteLine($"File {pngFile} does not exist.");
        continue;
    }
    
    Console.WriteLine($"Processing file: {pngFile}");

    var stream = Util.LoadAndProcessImage(pngFile);
    var img = TesseractOCR.Pix.Image.LoadFromMemory(stream);
    //var img2 = TesseractOCR.Pix.Image.LoadFromMemory()
    var page = engine.Process(img, region, PageSegMode.SingleLine);
    Console.WriteLine(page.Text);
    
    var split = page.Text.Split(':');
    if(split.Length < 3)
    {
        Console.WriteLine($"Could not parse text from {pngFile}. Expected format: 'Zone: <name> Pos: x y z'.");
        page.Dispose();
        img.Dispose();
        stream.Dispose();
        continue;
    }
    var coords= split[2].Trim().Split(' ');
    if (coords.Length < 3)
    {
        Console.WriteLine($"Could not parse coordinates from {pngFile}. Expected format: 'Zone: <name> Pos: x y z'.");
        page.Dispose();
        img.Dispose();
        stream.Dispose();
        continue;
    }

    var entry = new FlightLogEntry
    {
        FileName = Path.GetFileName(pngFile),
        X = ParseCoordinate(coords[0]),
        Y = ParseCoordinate(coords[1]),
        Z = ParseCoordinate(coords[2])
    };
    logEntries.Add(entry);
    
    Console.WriteLine($"FlightLog: {entry.FileName} {entry.X:F4} {entry.Y:F4} {entry.Z:F4}");
    
    // Cleanup for Tesseract  resources  
    page.Dispose();
    img.Dispose();
    stream.Dispose();
    
    var dot = entry.FileName.IndexOf('.', StringComparison.InvariantCulture);
    var maskFileName = entry.FileName.Substring(0, dot) + ".mask.png";
    var maskFilePathFull = Path.Combine(currentDirectory, maskFileName);
   // Console.WriteLine($"Mask file path: {maskFilePathFull}");
    if (File.Exists(maskFilePath))
    {
        // copy the mask file to the same directory as the png file
        File.Copy(maskFilePath, maskFilePathFull, true);
    }
}

// Save the log entries to a text file
var logFilePath = Path.Combine(currentDirectory, "flight_log.txt");
using (var writer = new StreamWriter(logFilePath))
{
    foreach (var entry in logEntries)
    {
        writer.WriteLine($"{entry.FileName} {entry.X:F4} {entry.Y:F4} {entry.Z:F4}");
    }
}
engine.Dispose();


static Double ParseCoordinate(string coord)
{
    var num = coord.Replace("km", "").Trim();
    num = num.Replace(',', '.'); // Replace comma with dot for decimal parsing
    if (CharacterCount(coord, '.') > 1)
    {
        Console.WriteLine($"Invalid coordinate value '{coord}'. Expected a single decimal point.");
        return 0; // Return 0 or handle as needed
    }
    if (!double.TryParse(num, out var parsedResult))
    {
        Console.WriteLine($"Invalid coordinate value '{coord}'. Expected numeric values.");
        return 0;
    }
    return parsedResult * 1000; // Convert km to m
}

static int CharacterCount(string str, char c)
{
    return str.Count(x => x == c);
}
