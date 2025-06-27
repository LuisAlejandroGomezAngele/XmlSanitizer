
# 🧼 XmlSanitizer CLR para SQL Server

Este proyecto habilita funciones CLR para limpiar texto con caracteres no válidos en XML y escribir archivos codificados en UTF-8 desde SQL Server.

---

## ✅ Requisitos

- SQL Server con permisos de administrador
- .NET Framework 4.7.2 o superior instalado
- Archivo `XML.dll` generado desde el proyecto CLR
- Acceso a disco en el servidor para escribir archivos (por ejemplo: `C:\tu-ruta\`)

---

## 🔧 Configuración en SQL Server

```sql
-- Habilitar CLR y procedimientos OLE
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'clr enabled', 1;
RECONFIGURE;
EXEC sp_configure 'Ole Automation Procedures', 1;
RECONFIGURE;

-- Confiar en la base de datos (necesario para ensamblados UNSAFE)
ALTER DATABASE tubase SET TRUSTWORTHY ON;
```

---

## 📦 Registrar el ensamblado CLR

> Asegúrate de compilar el proyecto en modo `Release` y copiar `XML.dll` a una ruta accesible.

```sql
-- Limpia cualquier versión anterior
DROP ASSEMBLY IF EXISTS XmlSanitizer;
GO

-- Crea el ensamblado desde el archivo DLL
CREATE ASSEMBLY XmlSanitizer 
FROM 'tu-ruta\XML.dll' 
WITH PERMISSION_SET = UNSAFE; -- Requiere TRUSTWORTHY
GO
```

---

## 🧠 Crear funciones CLR

```sql
-- Limpieza de caracteres inválidos XML
CREATE FUNCTION dbo.fn_SanitizeXmlClr(@input NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME XmlSanitizer.XmlSanitizer.Sanitize;
GO

-- Convertir string a UTF-8 (varbinary)
CREATE FUNCTION dbo.fn_ToUtf8Bytes(@input NVARCHAR(MAX))
RETURNS VARBINARY(MAX)
AS EXTERNAL NAME XmlSanitizer.XmlSanitizer.ToUtf8Bytes;
GO
```

---

## 📝 Procedimiento para guardar archivos UTF-8

```sql
CREATE PROCEDURE dbo.sp_WriteUtf8ToFile
    @filePath NVARCHAR(MAX),
    @content NVARCHAR(MAX)
AS EXTERNAL NAME XmlSanitizer.FileWriter.WriteUtf8ToFile;
GO
```

---

## 🚀 Ejemplo de uso

```sql
DECLARE @xml NVARCHAR(MAX) = '<root><tag>Texto válido</tag></root>';

-- Limpiar y convertir
DECLARE @sanitized NVARCHAR(MAX) = dbo.fn_SanitizeXmlClr(@xml);

-- Guardar en archivo .xml
EXEC dbo.sp_WriteUtf8ToFile 
    @filePath = 'tu ruta/xml.xml',
    @content = @sanitized;
```

---

## ⚠️ Notas

- Asegúrate de que el usuario de SQL Server tenga permisos para escribir en la ruta del archivo.
- Puedes usar `DATALENGTH()` para validar que la conversión a UTF-8 genera contenido válido.
