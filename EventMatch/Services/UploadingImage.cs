using EventMatch.Models;
//using Kotlin.JS;
using System;
using System.Collections.Generic;
using System.Text;
//using Xamarin.Google.Crypto.Tink.Subtle;

namespace EventMatch.Services
{
    public class UploadingImage
    {
        public async Task<FileResult> OpenMediaPickerAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions()
                {
                    Title = "Pick your profile picture"
                });

                if (result.ContentType.Contains("png") ||
                    result.ContentType.Contains("jpeg") ||
                    result.ContentType.Contains("jpg"))
                    return result;
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error Type Image", "Please choose a new image", "OK");
                    return null;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<Stream> FileResultToStream(FileResult fileResult)
        {
            if (fileResult == null)
            {
                return null;
            }

            return await fileResult.OpenReadAsync();
        }

        public Stream ByteArrayToStream(byte[] bytes)
        {
            return new MemoryStream(bytes);
        }

        public string ByteBase64ToString(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public byte[] StringToByteBase64(string text)
        {
            return Convert.FromBase64String(text);
        }

        public async Task<ImageFile> Upload(FileResult fileResult)
        {
            Byte[] bytes;
            try
            {
                using (var ms = new MemoryStream())
                {
                    var stream = await FileResultToStream(fileResult);
                    stream.CopyTo(ms);
                    bytes = ms.ToArray();
                }

                return new ImageFile
                {
                    ByteBase64 = ByteBase64ToString(bytes),
                    ContentType = fileResult.ContentType,
                    FileName = fileResult.FileName
                };
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
