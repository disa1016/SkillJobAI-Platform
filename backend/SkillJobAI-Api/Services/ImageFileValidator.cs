using Microsoft.AspNetCore.Http;

namespace SkillJobAI.Api.Services;

public static class ImageFileValidator
{
    public static async Task<bool>
        HasValidSignatureAsync(
            IFormFile file,
            string extension
        )
    {
        if (file.Length < 12)
        {
            return false;
        }

        var header = new byte[12];

        await using var stream =
            file.OpenReadStream();

        var bytesRead =
            await stream.ReadAsync(
                header.AsMemory(
                    0,
                    header.Length));

        if (bytesRead < header.Length)
        {
            return false;
        }

        return extension.ToLowerInvariant()
            switch
            {
                ".jpg" or ".jpeg" =>
                    IsJpeg(header),

                ".png" =>
                    IsPng(header),

                ".webp" =>
                    IsWebP(header),

                _ => false
            };
    }

    private static bool IsJpeg(
        byte[] header
    )
    {
        return header[0] == 0xFF &&
               header[1] == 0xD8 &&
               header[2] == 0xFF;
    }

    private static bool IsPng(
        byte[] header
    )
    {
        byte[] pngSignature =
        [
            0x89,
            0x50,
            0x4E,
            0x47,
            0x0D,
            0x0A,
            0x1A,
            0x0A
        ];

        return header
            .Take(pngSignature.Length)
            .SequenceEqual(
                pngSignature);
    }

    private static bool IsWebP(
        byte[] header
    )
    {
        return header[0] == (byte)'R' &&
               header[1] == (byte)'I' &&
               header[2] == (byte)'F' &&
               header[3] == (byte)'F' &&
               header[8] == (byte)'W' &&
               header[9] == (byte)'E' &&
               header[10] == (byte)'B' &&
               header[11] == (byte)'P';
    }
}