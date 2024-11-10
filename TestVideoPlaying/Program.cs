using System.Net;
using NativeFileDialogSharp;
using Neutron.Scripts;

namespace TestVideoPlaying;

internal class Program
{
    private static string? selectedFilePath;
    private static HttpListener? listener;

    [STAThread]
    static void Main(string[] args)
    {
        Application application;

#if DEBUG
        application = new Application(title: "TestVideoPlaying", width: 960, height: 540, webContentPath: Path.Combine(AppContext.BaseDirectory, "dist"), debug: true);
#else
        application = new Application(title: "TestVideoPlaying", width: 960, height: 540, webContentPath: Path.Combine(AppContext.BaseDirectory, "dist"));
#endif

        application.Center();

        application.Bind<string?>("openFilePicker", () =>
        {
            DialogResult dialogResult = Dialog.FileOpen("mp4");

            if (dialogResult.IsError || dialogResult.IsCancelled)
            {
                return null;
            }

            selectedFilePath = dialogResult.Path;

            if (listener == null || !listener.IsListening)
            {
                StartHttpServer();
            }

            return "http://localhost:5000/video.mp4";
        });

        application.Run();
    }

    private static void StartHttpServer()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/");
        listener.Start();

        Task.Run(() =>
        {
            while (listener != null && listener.IsListening)
            {
                try
                {
                    var context = listener.GetContext();
                    var response = context.Response;

                    if (selectedFilePath != null && File.Exists(selectedFilePath))
                    {
                        ServeFile(response, selectedFilePath);
                    }
                    else
                    {
                        response.StatusCode = 404;
                        response.OutputStream.Close();
                    }
                }
                catch (HttpListenerException exception)
                {
                    Console.WriteLine($"HttpListenerException: {exception.Message}");
                    break;
                }
                catch (ObjectDisposedException exception)
                {
                    Console.WriteLine($"ObjectDisposedException: {exception.Message}");
                    break;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"General Exception: {exception.Message}");
                    break;
                }
            }
        });
    }

    private static void ServeFile(HttpListenerResponse response, string filePath)
    {
        try
        {
            byte[] buffer = File.ReadAllBytes(filePath);
            response.ContentType = "video/mp4";
            response.ContentLength64 = buffer.Length;

            using (var output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
                output.Flush();
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error serving file: {exception.Message}");
        }
        finally
        {
            response.OutputStream.Close();
        }
    }
}
