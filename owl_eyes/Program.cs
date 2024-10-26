using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

class Client
{
    static async Task Main()
    {
        try
        {
            re:
            using (TcpClient client = new TcpClient("127.0.0.1", 5000))
            using (NetworkStream stream = client.GetStream())
            {
                
                while (true)
                {
                    Console.Write("Введіть повідомлення для сервера (або 'exit' для виходу): ");
                    string message = Console.ReadLine();

                    if (message?.ToLower() == "exit") break;

                    // Відправлення повідомлення серверу
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    stream.Write(messageBytes, 0, messageBytes.Length);
                    Console.WriteLine($"Відправлено на сервер: {message}");

                    // Обробка спеціальної команди /screen
                    if (message.ToLower() == "/screen")
                    {
                        // Отримати розмір файлу
                        byte[] sizeBuffer = new byte[4];
                        stream.Read(sizeBuffer, 0, sizeBuffer.Length);
                        int fileSize = BitConverter.ToInt32(sizeBuffer, 0);

                        // Отримати байти файлу
                        byte[] fileBytes = new byte[fileSize];
                        int bytesRead = 0;
                        while (bytesRead < fileSize)
                        {
                            bytesRead += await stream.ReadAsync(fileBytes, bytesRead, fileSize - bytesRead);
                        }

                        // Зберегти файл
                        string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
                        Directory.CreateDirectory(assetsPath); // Створити папку, якщо її не існує
                        string screenshotPath = Path.Combine(assetsPath, $"screenshot"+ GenerateRandomString(10) +".png");
                        await File.WriteAllBytesAsync(screenshotPath, fileBytes);
                        Console.WriteLine($"Скріншот збережено за адресою: {screenshotPath}");
                        Process.Start(new ProcessStartInfo(screenshotPath) { UseShellExecute = true });
                        goto re;
                    }
                    else
                    {
                        // Отримання відповіді від сервера
                        byte[] buffer = new byte[2000];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        string responseMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Отримано від сервера: {responseMessage}\n");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Помилка: {e.Message}");
        }
    }

    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();
        StringBuilder result = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }
}
