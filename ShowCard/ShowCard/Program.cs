using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShowCard.Forms;
using ShowCard.Services;

namespace ShowCard;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<ILogService, LogService>();
                services.AddSingleton<IAppStateService, AppStateService>();
                services.AddSingleton<ICardRunService, CardRunService>();
                services.AddSingleton<IAttractModeService, AttractModeService>();

                services.AddSingleton<CardViewForm>();
                services.AddSingleton<CardManagerForm>();
            })
            .Build();

        var stateService = host.Services.GetRequiredService<IAppStateService>();
        stateService.Load();

        var cardView = host.Services.GetRequiredService<CardViewForm>();
        cardView.Show(); // on 2nd display

        var manager = host.Services.GetRequiredService<CardManagerForm>();
        Application.Run(manager);

        stateService.Save();
    }
}
