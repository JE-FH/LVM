using Shared;

namespace TestHelper
{
	public class TestFileFixture
	{
		private string testFilesLocation;
		private Dictionary<string, byte[]> files;
		public TestFileFixture()
		{
			testFilesLocation = Path.Join(Directory.GetCurrentDirectory(), "test-files");
			files = [];
		}

		public Stream GetTestFile(string fileName)
		{
			lock (files)
			{
				if (files.TryGetValue(fileName, out var fileContent))
				{
					return new MemoryStream(fileContent);
				}
				else
				{
					using var stream = new FileStream(Path.Join(testFilesLocation, fileName), FileMode.Open);
					var array = stream.ReadAll();

					files.Add(fileName, array);

					return new MemoryStream(array);
				}
			}
		}

		public byte[] GetTestFileBytes(string fileName)
		{
			lock (files)
			{
				if (files.TryGetValue(fileName, out var fileContent))
				{
					return fileContent.ToArray();
				}
				else
				{
					using var stream = new FileStream(Path.Join(testFilesLocation, fileName), FileMode.Open);
					var array = stream.ReadAll();

					files.Add(fileName, array);

					return array.ToArray();
				}
			}
		}
	}
}
