using System;
using System.Data.SqlTypes;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

public class XmlSanitizer
{
    [SqlFunction(IsDeterministic = true, IsPrecise = false)]
    public static SqlString Sanitize(SqlString input)
    {
        if (input.IsNull)
            return SqlString.Null;

        var str = input.Value;

        // 1. Normalizar comillas tipográficas a estándar
        str = str.Replace('“', '"').Replace('”', '"')
                 .Replace('‘', '\'').Replace('’', '\'');

        // 2. Escapar ampersands que NO son entidades válidas
        str = Regex.Replace(str, "&(?!(amp;|lt;|gt;|quot;|apos;))", "&amp;");

        var sb = new StringBuilder(str.Length);

        foreach (char ch in str)
        {
            if (IsValidXmlChar(ch))
            {
                switch (ch)
                {
                    case '"': sb.Append("&quot;"); break;
                    case '\'': sb.Append("&apos;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    default: sb.Append(ch); break;
                }
            }
            // Caracteres inválidos se omiten
        }

        return new SqlString(sb.ToString());
    }

    [SqlFunction(IsDeterministic = true, IsPrecise = true)]
    public static SqlBytes ToUtf8Bytes(SqlString input)
    {
        if (input.IsNull)
            return SqlBytes.Null;

        byte[] utf8Bytes = Encoding.UTF8.GetBytes(input.Value);
        return new SqlBytes(utf8Bytes);
    }

    private static bool IsValidXmlChar(char ch)
    {
        return
            ch == 0x9 || ch == 0xA || ch == 0xD ||
            (ch >= 0x20 && ch <= 0xD7FF) ||
            (ch >= 0xE000 && ch <= 0xFFFD);
    }
}

public class FileWriter
{
    [SqlProcedure]
    public static void WriteUtf8ToFile(SqlString filePath, SqlString content)
    {
        if (filePath.IsNull || content.IsNull)
            return;

        File.WriteAllText(filePath.Value, content.Value, new UTF8Encoding(false));
    }
}
