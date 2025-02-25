using System.Text.RegularExpressions;

namespace userauthjwt.Helpers
{
    public static class FileHelper
    {
        // Add a constant for maximum allowed file size in bytes
        private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        public static bool ValidateFileSize(byte[] fileBytes, out string errorMessage)
        {
            if (fileBytes.Length > MaxFileSizeBytes)
            {
                errorMessage = "File too big. The image or PDF file must not be greater than 5MB in size.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        public static bool ValidBase64(string base64FileString)
        {
            string base64Pattern = @"^[A-Za-z0-9+/=]*$";
            return Regex.IsMatch(base64FileString, base64Pattern);
        }

        public static byte[] DecodeFileString(string base64FileString)
        {
            // Remove the data URL prefix if it exists
            string base64Data = Regex.Replace(base64FileString, @"^data:([\w/\-\.]+);base64,", string.Empty);

            // Remove invalid characters
            base64Data = Regex.Replace(base64Data, @"[^A-Za-z0-9+/=]", string.Empty);
            base64Data = base64Data.Replace(" ", "").Replace("\n", "").Replace("\r", "");

            // Ensure the Base64 string is properly padded
            if (base64Data.Length % 4 != 0)
                base64Data += new String('=', 4 - base64Data.Length % 4);

            byte[] fileBytes = Convert.FromBase64String(base64Data);
            return fileBytes;
        }


        public static bool IsPdf(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            // PDF file signature: %PDF (hex: 25 50 44 46)
            byte[] pdfSignature = { 0x25, 0x50, 0x44, 0x46 };

            foreach (byte sigByte in pdfSignature)
            {
                if (stream.ReadByte() != sigByte)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsJpeg(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            // JPEG file signature: FF D8
            byte[] jpegSignature = { 0xFF, 0xD8 };

            foreach (byte sigByte in jpegSignature)
            {
                if (stream.ReadByte() != sigByte)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsPng(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            // PNG file signature: 89 50 4E 47
            byte[] pngSignature = { 0x89, 0x50, 0x4E, 0x47 };

            foreach (byte sigByte in pngSignature)
            {
                if (stream.ReadByte() != sigByte)
                {
                    return false;
                }
            }
            return true;
        }

        // Check if the file is an image
        public static bool IsImageComplaint(byte[] fileBytes)
        {
            using (var stream = new MemoryStream(fileBytes))
            {
                return stream.IsJpeg() || stream.IsPng();
            }
        }

        public static bool IsFileComplaint(byte[] fileBytes)
        {
            using (var stream = new MemoryStream(fileBytes))
            {
                return stream.IsPdf();
            }
        }
    }
}
