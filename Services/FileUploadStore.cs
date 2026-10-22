using Microsoft.AspNetCore.Http;

namespace prvalid.Services
{
    public static class FileUploadStore
    {
        public static string GetDirectory(IConfiguration config, string screen, string label)
        {
            var path = config[$"FileUpload:{screen}:{label}:Path"];
            if (string.IsNullOrWhiteSpace(path))
            {
                path = config["DocPath"];
            }
            return path ?? string.Empty;
        }

        public static int MaxSizeMb(IConfiguration config)
        {
            var raw = config["FileUpload:MaxSizeMB"];
            if (int.TryParse(raw, out var mb) && mb > 0)
            {
                return mb;
            }
            return 4;
        }

        public static long MaxSizeBytes(IConfiguration config)
        {
            return MaxSizeMb(config) * 1024L * 1024L;
        }

        public static string[] AllowedExtensions(IConfiguration config)
        {
            var values = config.GetSection("FileUpload:AllowedExtensions").GetChildren()
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.StartsWith(".") ? v.ToLowerInvariant() : "." + v.ToLowerInvariant())
                .ToArray();
            if (values.Length == 0)
            {
                return new[] { ".pdf", ".txt", ".csv", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".rtf", ".png", ".jpg", ".jpeg", ".gif" };
            }
            return values;
        }

        public static string? Validate(IConfiguration config, IEnumerable<IFormFile>? files)
        {
            if (files == null)
            {
                return null;
            }
            var allowed = AllowedExtensions(config);
            var maxBytes = MaxSizeBytes(config);
            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                {
                    continue;
                }
                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
                if (!allowed.Contains(ext))
                {
                    return "Only human readable files are allowed. " + file.FileName + " is not permitted.";
                }
                if (file.Length > maxBytes)
                {
                    return file.FileName + " exceeds the " + MaxSizeMb(config) + " MB upload limit.";
                }
            }
            return null;
        }

        public static void Save(IConfiguration config, string screen, string label, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return;
            }
            var error = Validate(config, new[] { file });
            if (error != null)
            {
                throw new InvalidOperationException(error);
            }
            var uploadDir = GetDirectory(config, screen, label);
            if (string.IsNullOrWhiteSpace(uploadDir))
            {
                throw new InvalidOperationException("Upload path is not configured for " + screen + " " + label + ".");
            }
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }
            var filePath = Path.Combine(uploadDir, Path.GetFileName(file.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
        }

        public static string? Find(IConfiguration config, string? fileName, string? screen, string? label)
        {
            var name = Path.GetFileName(fileName ?? "");
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            var folders = new List<string>();
            if (!string.IsNullOrWhiteSpace(screen) && !string.IsNullOrWhiteSpace(label))
            {
                folders.Add(GetDirectory(config, screen, label));
            }
            foreach (var screenSection in config.GetSection("FileUpload").GetChildren())
            {
                if (screenSection.Key == "MaxSizeMB" || screenSection.Key == "AllowedExtensions")
                {
                    continue;
                }
                foreach (var labelSection in screenSection.GetChildren())
                {
                    var path = labelSection["Path"];
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        folders.Add(path);
                    }
                }
            }
            var docPath = config["DocPath"];
            if (!string.IsNullOrWhiteSpace(docPath))
            {
                folders.Add(docPath);
            }
            foreach (var folder in folders.Where(f => !string.IsNullOrWhiteSpace(f)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var full = Path.Combine(folder, name);
                if (File.Exists(full))
                {
                    return full;
                }
            }
            return null;
        }
    }
}
