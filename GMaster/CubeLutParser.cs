namespace GMaster
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Windows.UI;

    public class CubeLutParser
    {
        public async Task<Lut> Parse(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var colors = new List<Color>();
                var n3D = 0;
                while (true)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null)
                    {
                        break;
                    }

                    if (line == string.Empty || line[0] == '#')
                    {
                        continue;
                    }

                    var values = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (values.Length < 2)
                    {
                        continue;
                    }

                    if (values[0] == "LUT_3D_SIZE")
                    {
                        n3D = int.Parse(values[1]);
                        continue;
                    }

                    if (values.Length == 3
                        && float.TryParse(values[0], out var r)
                        && float.TryParse(values[0], out var g)
                        && float.TryParse(values[0], out var b))
                    {
                        colors.Add(Color.FromArgb(255, (byte)(255 * r), (byte)(255 * g), (byte)(255 * b)));
                    }
                }

                return new Lut { Colors = colors, BlueNum = n3D, GreenNum = n3D, RedNum = n3D };
            }
        }
    }
}