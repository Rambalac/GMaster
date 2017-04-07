namespace GMaster
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Windows.UI;

    public class CubeLutParser : ILutParser
    {
        public async Task<Lut> Parse(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                Color[] colors = null;
                var n3d = 0;
                var r = 0;
                var g = 0;
                var b = 0;
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
                        n3d = int.Parse(values[1]);
                        colors = new Color[n3d * n3d * n3d];
                        continue;
                    }

                    if (values.Length == 3 && colors != null
                        && float.TryParse(values[0], out var rC)
                        && float.TryParse(values[1], out var gC)
                        && float.TryParse(values[2], out var bC))
                    {
                        var col = Color.FromArgb(255, (byte)(255 * rC), (byte)(255 * gC), (byte)(255 * bC));

                        if (n3d > 0)
                        {
                            colors[(r * n3d * n3d) + (g * n3d) + b] = col;
                            r++;
                            if (r == n3d)
                            {
                                g++;
                                r = 0;
                                if (g == n3d)
                                {
                                    b++;
                                    g = 0;
                                }
                            }
                        }
                        else
                        {
                            colors[r++] = col;
                        }
                    }
                }

                return new Lut { Colors = colors, BlueNum = n3d, GreenNum = n3d, RedNum = n3d };
            }
        }
    }
}