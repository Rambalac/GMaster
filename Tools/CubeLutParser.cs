namespace GMaster.Tools
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
                var num = 0;
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
                        num = int.Parse(values[1]);
                        colors = new Color[num * num * num];
                        continue;
                    }

                    if (values.Length == 3 && colors != null
                        && float.TryParse(values[0], out var rC)
                        && float.TryParse(values[1], out var gC)
                        && float.TryParse(values[2], out var bC))
                    {
                        var col = Color.FromArgb(255, (byte)(255 * rC), (byte)(255 * gC), (byte)(255 * bC));

                        if (num > 0)
                        {
                            colors[(r * num * num) + (g * num) + b] = col;
                            r++;
                            if (r == num)
                            {
                                g++;
                                r = 0;
                                if (g == num)
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

                return new Lut { Colors = colors, BlueNum = num, GreenNum = num, RedNum = num };
            }
        }
    }
}