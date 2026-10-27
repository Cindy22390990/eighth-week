using System.Diagnostics;
using System.Text.Json;
using Training.ImageSystem.Models;


namespace Training.ImageSystem.Services;


public class PythonImageProcessor
{
    private readonly IConfiguration _configuration;

    private readonly ILogger<PythonImageProcessor> _logger;



    public PythonImageProcessor(
        IConfiguration configuration,
        ILogger<PythonImageProcessor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }




    public async Task<PythonResult?> ExecuteAsync(
        string inputPath,
        string outputPath,
        CancellationToken cancellationToken)
    {

        var pythonExe =
            _configuration["Python:PythonExe"]!;


        var workingDirectory =
            _configuration["Python:WorkingDirectory"]!;


        var timeoutSeconds =
            int.Parse(
                _configuration["Python:TimeoutSeconds"]!);




        var startInfo =
            new ProcessStartInfo
            {
                FileName = pythonExe,

                WorkingDirectory = workingDirectory,

                RedirectStandardOutput = true,

                RedirectStandardError = true,

                UseShellExecute = false,

                CreateNoWindow = true
            };




        startInfo.ArgumentList.Add("-m");

        startInfo.ArgumentList.Add("src.main_cli");



        startInfo.ArgumentList.Add("--input");

        startInfo.ArgumentList.Add(inputPath);



        startInfo.ArgumentList.Add("--output");

        startInfo.ArgumentList.Add(outputPath);





        using var process = new Process
        {
            StartInfo = startInfo
        };




        process.Start();




        var stdoutTask =
            process.StandardOutput.ReadToEndAsync();


        var stderrTask =
            process.StandardError.ReadToEndAsync();



        var exitTask =
            process.WaitForExitAsync(
                cancellationToken);



        var timeoutTask =
            Task.Delay(
                TimeSpan.FromSeconds(timeoutSeconds),
                cancellationToken);




        var completed =
            await Task.WhenAny(
                exitTask,
                timeoutTask);





        if (completed == timeoutTask)
        {

            try
            {
                process.Kill(true);
            }
            catch
            {

            }



            _logger.LogError(
                "Python Timeout Input:{Input}",
                inputPath);



            return null;
        }






        var stdout =
            await stdoutTask;


        var stderr =
            await stderrTask;






        if (process.ExitCode != 0)
        {

            _logger.LogError(
                "Python ExitCode:{ExitCode}",
                process.ExitCode);



            _logger.LogError(
                "Python stderr:{stderr}",
                stderr);




            return new PythonResult
            {
                Status = "failed",

                Message = stderr
            };
        }







        if (string.IsNullOrWhiteSpace(stdout))
        {

            _logger.LogError(
                "Python stdout empty");


            return null;
        }








        try
        {

            return JsonSerializer.Deserialize<PythonResult>(
                stdout,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        }
        catch (Exception ex)
        {

            _logger.LogError(
                ex,
                "Python JSON parse failed");



            _logger.LogError(
                "stdout:{stdout}",
                stdout);



            return null;
        }

    }

}