<Query Kind="Program" />

void Main()
{
	var directory = @"C:\Users\scott_000\Source\Repos\PunchingBand\punchdata";
	var outputFilePath = @"C:\Users\scott_000\Source\Repos\PunchingBand\punchdata\combined\punchdata.csv";
	
	var lines = new List<string>();
	var builder = new StringBuilder();
	var windowSize = 25;
	var prepunchRecordCount = 1;
	
	builder.Append("PunchType");
	for(int i = 0; i< windowSize; i++)
	{
		builder.Append(",");
		builder.Append(string.Format("X{0},Y{0},Z{0}", i));
	}
	lines.Add(builder.ToString());
	
	foreach (var file in Directory.GetFiles(directory))
	{
		var classification = Path.GetFileNameWithoutExtension(file).Split('-')[1];
		var records = new List<Record>();
		var startRecords = new List<Record>();
		
		foreach (var line in File.ReadAllLines(file))
		{
			var parts = line.Split(',');
			var record = new Record
			{
				X = parts[1],
				Y = parts[2],
				Z = parts[3],
				State = parts[4],
			};
			
			if (record.State != "Unknown")
			{
				if (startRecords.Count > 0)
				{
					for (int i = Math.Max(0, startRecords.Count - prepunchRecordCount); i < startRecords.Count; i++)
					{
						records.Add(startRecords[i]);
					}
				}
				records.Add(record);
			}
			else
			{
				startRecords.Add(record);
			}
			
			if (record.State == "Reset")
			{
				builder = new StringBuilder();
				builder.Append(classification);
				
				int count = 0;
				foreach (var r in records)
				{
					builder.Append(",");
					builder.Append(r.X);
					builder.Append(",");
					builder.Append(r.Y);
					builder.Append(",");
					builder.Append(r.Z);
					
					count++;
				}
				
				for (int i = 0; i < windowSize - count; i++)
				{
					builder.Append(",0.0,0.0,0.0");
				}
				
				lines.Add(builder.ToString());
				
				records.Clear();
			}
		}
	}
	
	File.WriteAllLines(outputFilePath, lines);
}

class Record
{
	public string State { get;set; }
	public string X { get; set; }
	public string Y { get; set; }
	public string Z { get; set; }
}